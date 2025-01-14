using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Client;
using MusicRecognitionSystem.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MusicRecognitionSystem.Data
{
    internal class CRUDManager
    {
        public static MusicRecognitionContext _context = new MusicRecognitionContext();

        public static void addSong(string songName)
        {
            if (_context.Songs.Where(s => s.name == songName).FirstOrDefault() != null)
            {
                Console.WriteLine($"Song {songName} is already in database");
                return;
            }

            Song song = new Song();
            song.name = songName;
            _context.Songs.Add(song);
            _context.SaveChanges();

            Logger.LogSong(songName);
        }

        //metoda wykorzysytywana w addTimestamp
        private static void addHash(string hashValue)
        {
            if(_context.Hashes.Where(h => h.hashValue == hashValue).FirstOrDefault() != null)
            {
                Console.WriteLine($"Hash {hashValue} is already in database");
                return;
            }

            Hash hash = new Hash();
            hash.hashValue = hashValue;
            _context.Hashes.Add(hash);

            _context.SaveChanges();

            Logger.LogHash(hashValue);
        }

        public static void addSongTimestamp(string songName, string hashValue, int chunkNumber)
        {
            //Check if song exists in database
            SongHash timestampExist = _context.SongHashes
                .Include(s => s.song)
                .Include(h => h.hash)
                .Where(sh => sh.hash.hashValue == hashValue)
                .Where(sh => sh.song.name == songName)
                .Where(sh => sh.timestamp == chunkNumber)
                .FirstOrDefault();

            /*SongHash debug = _context.SongHashes
                .Include(s => s.song)
                .Include(h => h.hash)
                .Where(sh => sh.hash.hashValue == hashValue)
                .Where(sh => sh.song.name == songName)
                .FirstOrDefault();

            if(debug != null)
            {
                Console.WriteLine("Here I am");
            }*/

            //if timestamp already exists, do not add it
            if (timestampExist != null)
            {
                Console.WriteLine($"Timestamp {chunkNumber} for song {songName} has already been added to database with hash {hashValue}");
                return;
            }

            //Verify if song exists in database
            Song song = _context.Songs.Where(s => s.name == songName).FirstOrDefault();
            if(song == null)
            {
                throw new Exception("Song with given name does not exist!");
            }

            //Generate Hash or assign existing to Timestamp
            Hash hash = _context.Hashes.Where(h => h.hashValue == hashValue).FirstOrDefault();
            if (hash == null)
            {
                addHash(hashValue);
                hash = _context.Hashes.Where(h => h.hashValue == hashValue).FirstOrDefault();
            }

            SongHash songHash = new SongHash { song = song, hash = hash, timestamp = chunkNumber };
            _context.SongHashes.Add(songHash);
            _context.SaveChanges();

            Logger.LogTimestamp(hashValue, songName, chunkNumber);
        }


    }
}
