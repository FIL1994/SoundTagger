/*
    Sound Tagger
    @author Philip Van Raalte
    @date May 18, 2017

    This clas models a sound file (.mp3, .wav, .flac, etc.) including its path and tags
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SoundTagger
{
    class SoundFile
    {
        public int id;
        public string path;
        public string fileName;
        public string artist;
        public string album;
        public string performer;
        public string genre;
        public string title;
        public uint track;
        public uint year;
        public uint disc;
        public string comment;

        public SoundFile()
        {

        }
    }
}
