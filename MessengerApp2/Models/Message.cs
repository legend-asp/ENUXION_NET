namespace MessengerApp1.Models
{
    public class Message
    {
        public int Id { get; set; }
        public string SenderId { get; set; }
        public string ReceiverId { get; set; }

        // Возвращаем имена, которые понимает твоя БД
        public string Text { get; set; }
        public DateTime SentAt { get; set; }

        public virtual ApplicationUser Sender { get; set; }
        public virtual ApplicationUser Receiver { get; set; }
    }
}