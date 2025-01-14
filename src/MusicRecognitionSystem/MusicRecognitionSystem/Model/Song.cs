using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MusicRecognitionSystem.Model
{
    internal class Song
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid songID { get; set; }
        public string name { get; set; }

        public ICollection<SongHash> songHashes { get; set; }
    }
}
