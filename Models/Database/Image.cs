using PicsWebApp.Interfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PicsWebApp.Models.Database
{
    public class Image
    {
        [Key]
        public ulong Id { get; set; }
        public required string FileName { get; set; }

        public virtual User User { get; set; } = null!;
        public required ulong UserId { get; set; }
    }
}
