/*
    Sound Tagger
    @author Philip Van Raalte
    @date May 18, 2017

    This is an application that allows you to view and edit tags in a file.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using CefSharp;
using CefSharp.Wpf;
using System.IO;
using System.Linq;

namespace SoundTagger
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public static ChromiumWebBrowser chromeBrowser;
        private static List<SoundFile> soundFiles;
        private static string lastFolder;
        private static string page;

        internal static List<SoundFile> SoundFiles { get => soundFiles; set => soundFiles = value; }

        public void InitializeChromium()
        {
            CefSettings settings = new CefSettings();

            // Enable WebRTC                           
            settings.CefCommandLineArgs.Add("enable-media-stream", "1");
            // Don't use a proxy server, always make direct connections. Overrides any other proxy server flags that are passed.
            // Slightly improves Cef initialize time as it won't attempt to resolve a proxy
            settings.CefCommandLineArgs.Add("no-proxy-server", "1");

            page = string.Format(@"{0}\html-resources\index.html", AppDomain.CurrentDomain.BaseDirectory);

            if(!File.Exists(page))
            {
                Console.WriteLine("Error The html file doesn't exists : " + page);
                //page = "http://ourcodeworld.com";
            }

            Cef.Initialize(settings);
            // Create a browser component
            chromeBrowser = new ChromiumWebBrowser();
            chromeBrowser.Address = page;
            chromeBrowser.LoadingStateChanged += BrowserLoadingStateChanged;
            // Add it to the form and fill it to the form window.
            myGrid.Children.Add(chromeBrowser);

            // Allow the use of local resources in the browser
            BrowserSettings browserSettings = new BrowserSettings();
            browserSettings.FileAccessFromFileUrls = CefState.Enabled;
            browserSettings.UniversalAccessFromFileUrls = CefState.Enabled;
            chromeBrowser.BrowserSettings = browserSettings;
        }

        public MainWindow()
        {
            lastFolder = Directory.GetCurrentDirectory();
            InitializeComponent();
            InitializeChromium();
            // Register an object in javascript named "cefCustomObject" with function of the CefCustomObject class :3
            chromeBrowser.RegisterJsObject("cefCustomObject", new JSObject(chromeBrowser, this));
        }

        public static void GetFiles(string folder = "")
        {
            if (!Directory.Exists(folder))
                folder = lastFolder;
            lastFolder = folder;
            
            Console.WriteLine("\n\nSEARCHING FOR AUDIO FILES");
            soundFiles = new List<SoundFile>();
            string[] fileEntries = Directory.GetFiles(folder);
            int songID = 0;
            foreach (string path in fileEntries)
            {
                try {
                //http://taglib.org/api/namespaceTagLib.html
                //http://taglib.org/api/
                String[] extensions = { ".MP3", ".FLAC", ".MP4", ".WAV", ".M4A", ".AAC", ".OGG", ".WMA"};

                String ext = Path.GetExtension(path);
                    if (extensions.Contains(ext.ToUpper()))
                    {
                        var file = TagLib.File.Create(path);
                        SoundFile soundFile = new SoundFile();
                        soundFile.id = songID++;
                        soundFile.path = path;
                        soundFile.fileName = Path.GetFileName(path);
                        soundFile.title = file.Tag.Title;
                        soundFile.artist = file.Tag.FirstAlbumArtist;
                        soundFile.performer = file.Tag.FirstPerformer;
                        soundFile.album = file.Tag.Album;
                        soundFile.track = file.Tag.Track;
                        soundFile.genre = file.Tag.FirstGenre;
                        soundFile.year = file.Tag.Year;
                        soundFile.disc = file.Tag.Disc;
                        soundFile.comment = file.Tag.Comment;
                        soundFiles.Add(soundFile);

                        Console.WriteLine("Song Found: " + path);
                        //String artistName = "artist";
                        //String songName = "song";
                        //String[] info = Path.GetFileNameWithoutExtension(path).Split('-');
                        //if (info.Length > 1)
                        //{
                        //    artistName = info[0].Trim();
                        //    songName = info[1].Trim();

                        //    file.Tag.Title = "Name";
                        //    file.Tag.AlbumArtists = new string[] { artistName };
                        //    file.Tag.Performers = new String[1] { artistName };
                        //    file.Save();
                        //}
                    }

                } catch(Exception e)
                {
                    Console.WriteLine(e);
                }
            }
            string jsSendSongs = "songs = [];";
            foreach(SoundFile s in soundFiles)
            {
                Console.WriteLine("FILE: " + s.fileName + " | "  + s.disc + " | " + s.year);
                //` is a string literal in JS
                jsSendSongs += String.Format("songs.push(new songFile({0}, `{1}`, `{2}`, `{3}`, `{4}`, `{5}`, `{6}`, {7}, {8}, {9}, `{10}`));",
                    s.id, s.album, s.fileName, s.artist, s.performer, s.genre, s.title, s.track, s.year, s.disc, s.comment);
            }
            
            jsSendSongs += "updateTable();";

            Console.WriteLine(jsSendSongs);

            chromeBrowser.GetMainFrame().ExecuteJavaScriptAsync(jsSendSongs);

            Console.WriteLine("\n\nDONE SEARCHING FOR AUDIO FILES\n");
        }

        public static void saveSongs()
        {
            foreach(SoundFile s in soundFiles)
            {
                var file = TagLib.File.Create(s.path);

                file.Tag.Title = s.title;
                file.Tag.AlbumArtists = new string[] { s.artist };
                file.Tag.Performers = new string[] { s.performer };
                file.Tag.Album = s.album;
                if(s.track > 0) //don't allow 0 or negative numbers
                    file.Tag.Track = s.track;
                file.Tag.Genres = new string[] { s.genre };
                if(s.year > 0) //don't allow 0 or negative numbers
                    file.Tag.Year = s.year;
                if(s.disc > 0) //don't allow 0 or negative numbers
                    file.Tag.Disc = s.disc;
                file.Tag.Comment = s.comment;
                file.Save();
            }
        }

        private static void BrowserLoadingStateChanged(object sender, LoadingStateChangedEventArgs e)
        {
            if (!e.IsLoading)
            {
                chromeBrowser.ShowDevTools();

                GetFiles();

                var script = "hello();";
                chromeBrowser.GetMainFrame().ExecuteJavaScriptAsync(script);

                //Console.WriteLine("Browser Has Loaded Something");
            }
        }

        private void OnApplicationExit(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Console.WriteLine("Closing");
            Cef.Shutdown();
        }

        public static void ReloadPage()
        {
            chromeBrowser.Load(page);
        }
    }
}
