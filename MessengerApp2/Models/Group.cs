using System.ComponentModel.DataAnnotations;

namespace MessengerApp1.Models
{
    public class Group
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Название группы обязательно")]
        public string Name { get; set; } // Название чата

        public string? Description { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // Кто состоит в группе
        public virtual ICollection<GroupMember> Members { get; set; }
        // Сообщения группы
        public virtual ICollection<GroupMessage> Messages { get; set; }
    }
}