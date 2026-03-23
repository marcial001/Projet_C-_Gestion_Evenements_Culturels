using System;

namespace GestionEvenements.Models
{
    public class Evenement
    {
        public int Id { get; set; }
        public string Titre { get; set; } = "";
        public string Description { get; set; } = "";
        public DateTime DateDebut { get; set; }
        public DateTime? DateFin { get; set; }
        public string Lieu { get; set; } = "";
        public int Capacite { get; set; }
        public int CreeParId { get; set; }
        public int NbInscrits { get; set; }
    }
}