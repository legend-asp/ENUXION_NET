using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace MessengerApp1.Models
{
    public class ApplicationUser : IdentityUser
    {
        // Используем только это поле для денег
        [Column(TypeName = "decimal(18, 2)")]
        public decimal Balance { get; set; } = 1250.00m;

        public string? PhotoPath { get; set; }
        public string? Status { get; set; }
        public string? About { get; set; }
        public DateTime BirthDate { get; set; }

        [InverseProperty("User")]
        public virtual ICollection<Friendship> Friends { get; set; } = new List<Friendship>();

        public decimal EnuBalance { get; set; } = 0;
    }
}