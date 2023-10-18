using PicsWebApp.Interfaces;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PicsWebApp.Models.Database
{
    public class Friendship
    {
        [Key]
        public ulong Id { get; set; }

        public virtual User Proposer { get; set; } = null!;
        public required ulong ProposerId { get; set; }

        public virtual User Receiver { get; set; } = null!;
        public required ulong ReceiverId { get; set; }

        public bool IsPending { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
