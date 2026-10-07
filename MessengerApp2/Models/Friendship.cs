using System.ComponentModel.DataAnnotations.Schema;

namespace MessengerApp1.Models
{
    public class Friendship
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        public string FriendId { get; set; }
        public bool IsAccepted { get; set; }

        [ForeignKey("UserId")]
        public virtual ApplicationUser? User { get; set; }

        [ForeignKey("FriendId")]
        public virtual ApplicationUser? Friend { get; set; }
    }
}