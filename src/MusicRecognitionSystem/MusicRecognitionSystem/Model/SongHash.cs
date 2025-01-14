using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MusicRecognitionSystem.Model
{
    internal class SongHash
    {
        public Guid songID { get; set; }
        public Song song { get; set; }

        public Guid hashID { get; set; }
        public Hash hash { get; set; }

        public int timestamp { get; set; }

        //chunk where hash is located in song

    }
}
