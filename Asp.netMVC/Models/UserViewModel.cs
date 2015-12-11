using System.ComponentModel.DataAnnotations;

namespace Asp.netMVC.Models
{
    public class UserViewModel
    {
        public string Username { get; set; }
        public string Password { get; set; }
        public bool? isCorrect { get; set; }
        public string Name { get; set; }
        public string Permissione { get; set; }
    }
}