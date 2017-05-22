/*
    Sound Tagger
    @author Philip Van Raalte
    @date May 18, 2017

    This class can be used in index.html with JavaScript.
    It is used to call the folder select dialog and retrieving the edited songs.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CefSharp;
using CefSharp.Wpf;
using System.Diagnostics;
using Newtonsoft.Json.Linq;
using Microsoft.WindowsAPICodePack.Dialogs;
using System.Windows;
using System.Text.RegularExpressions;

namespace SoundTagger
{
    class JSObject
    {
        // Declare a local instance of chromium and the main form in order to execute things from here in the main thread
        private static ChromiumWebBrowser _instanceBrowser = null;
        private static MainWindow _instanceMainForm = null;

        public JSObject(ChromiumWebBrowser originalBrowser, MainWindow mainForm)
        {
            _instanceBrowser = originalBrowser;
            _instanceMainForm = mainForm;
        }

        public void showDevTools()
        {
            _instanceBrowser.ShowDevTools();
        }

        public void opencmd()
        {
            ProcessStartInfo start = new ProcessStartInfo("cmd.exe", "/c pause");
            Process.Start(start);
        }

        public void getSongs(string songs)
        {
            Console.WriteLine(songs);
            try
            {
                JArray a = JArray.Parse(songs);
                List<SoundFile> soundFiles = MainWindow.SoundFiles;

                foreach (JObject o in a.Children<JObject>())
                {
                    string id = o["id"].ToString();

                    if (isNumeric(id))
                    {
                        int soundID = int.Parse(id);
                        if (soundID < soundFiles.Count)
                        {
                            SoundFile s = soundFiles[soundID];
                            s.artist = "Changed"; //o["artist"].ToString();
                            s.performer = o["performer"].ToString();
                            s.album = o["album"].ToString();
                            s.genre = o["genre"].ToString();
                            s.title = o["title"].ToString();
                            s.comment = o["comment"].ToString();

                            string track = o["track"].ToString().Trim();
                            string year = o["year"].ToString().Trim();
                            string disc = o["disc"].ToString().Trim();

                            if (isNumeric(track))
                                s.track = uint.Parse(track);
                            if (isNumeric(year))
                                s.year = uint.Parse(year);
                            if (isNumeric(disc))
                                s.disc = uint.Parse(disc);
                        }
                    }
                }
                MainWindow.saveSongs();
                MainWindow.ReloadPage();
            }
            catch(Exception e)
            {
                Console.WriteLine(e);
            }
        }

        public void chooseFolder()
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                var dialog = new CommonOpenFileDialog();
                dialog.IsFolderPicker = true;
                CommonFileDialogResult result = dialog.ShowDialog();
                if (result.ToString().Equals("Ok"))
                {
                    Console.WriteLine(dialog.FileName);
                    MainWindow.GetFiles(dialog.FileName);
                }
            });
        }

        private bool isNumeric(string value)
        {
            return Regex.IsMatch(value, @"^\d+$");
        }
    }
}
