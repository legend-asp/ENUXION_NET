using System.ComponentModel.DataAnnotations.Schema;

namespace MessengerApp1.Models
{
    public class GroupMessage
    {
        public int Id { get; set; }
        public string Text { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.Now;

        public int GroupId { get; set; }
        [ForeignKey("GroupId")]
        public virtual Group Group { get; set; }

        public string SenderId { get; set; }
        [ForeignKey("SenderId")]
        public virtual ApplicationUser Sender { get; set; }
    }
}