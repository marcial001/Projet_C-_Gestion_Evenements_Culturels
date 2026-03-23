namespace GestionEvenements.Models
{
    public class Utilisateur
    {
        public int Id { get; set; }
        public string Login { get; set; } = "";
        public string Nom { get; set; } = "";
        public string Role { get; set; } = "";
    }
}