using Microsoft.AspNetCore.Http;
using System;
using System.ComponentModel.DataAnnotations;

namespace MessengerApp1.Models
{
    public class ProfileViewModel
    {
        public string? FirstName { get; set; }
        public string? LastName { get; set; }

        [DataType(DataType.Date)]
        public DateTime BirthDate { get; set; }

        public string? Status { get; set; }
        public string? About { get; set; }
        public string? Country { get; set; }
        public string? PhotoPath { get; set; }

        // Для загрузки файла аватара
        public IFormFile? Photo { get; set; }
    }
}