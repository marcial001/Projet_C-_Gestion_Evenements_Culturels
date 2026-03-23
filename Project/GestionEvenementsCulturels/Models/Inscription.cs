using System;

namespace GestionEvenements.Models
{
    public class Inscription
    {
        public int Id { get; set; }
        public int EvenementId { get; set; }
        public int UtilisateurId { get; set; }
        public DateTime DateInscription { get; set; }
        public string Statut { get; set; } = "Inscrit";
    }
}