using System;
using System.Globalization;
using System.Text.RegularExpressions;

namespace GestionEvenements.Utilities
{
    public static class InputValidator
    {
        private const int LoginMinLength = 2;
        private const int LoginMaxLength = 50;
        private const int PasswordMinLength = 3;
        private const int PasswordMaxLength = 100;
        private const int TitreMaxLength = 200;
        private const int DescriptionMaxLength = 2000;
        private const int LieuMaxLength = 200;
        private const int NomMaxLength = 100;
        private const int CommentaireMaxLength = 500;
        private const int CategorieMaxLength = 50;
        private const decimal PrixMin = 0;
        private const decimal PrixMax = 99999.99m;
        private const int NoteMin = 1;
        private const int NoteMax = 5;

        private static readonly Regex SafeTextRegex = new Regex(@"^[\p{L}\p{N}\s\-_.,'();:!?àâäéèêëïîôùûüçÀÂÄÉÈÊËÏÎÔÙÛÜÇ]+$", RegexOptions.Compiled);

        public static (bool Ok, string Message) ValiderLogin(string login)
        {
            if (string.IsNullOrWhiteSpace(login))
                return (false, "Le nom utilisateur est obligatoire.");
            string s = login.Trim();
            if (s.Length < LoginMinLength)
                return (false, $"Le nom utilisateur doit contenir au moins {LoginMinLength} caractères.");
            if (s.Length > LoginMaxLength)
                return (false, $"Le nom utilisateur ne doit pas dépasser {LoginMaxLength} caractères.");
            return (true, "");
        }

        public static (bool Ok, string Message) ValiderMotDePasse(string mdp)
        {
            if (string.IsNullOrEmpty(mdp))
                return (false, "Le mot de passe est obligatoire.");
            if (mdp.Length < PasswordMinLength)
                return (false, $"Le mot de passe doit contenir au moins {PasswordMinLength} caractères.");
            if (mdp.Length > PasswordMaxLength)
                return (false, $"Le mot de passe ne doit pas dépasser {PasswordMaxLength} caractères.");
            return (true, "");
        }

        public static (bool Ok, string Message) ValiderTitreEvenement(string titre)
        {
            if (string.IsNullOrWhiteSpace(titre))
                return (false, "Le titre est obligatoire.");
            string s = titre.Trim();
            if (s.Length > TitreMaxLength)
                return (false, $"Le titre ne doit pas dépasser {TitreMaxLength} caractères.");
            return (true, "");
        }

        public static (bool Ok, string Message) ValiderDescription(string desc)
        {
            if (desc == null) return (true, "");
            if (desc.Length > DescriptionMaxLength)
                return (false, $"La description ne doit pas dépasser {DescriptionMaxLength} caractères.");
            return (true, "");
        }

        public static (bool Ok, string Message) ValiderLieu(string lieu)
        {
            if (lieu == null) return (true, "");
            if (lieu.Length > LieuMaxLength)
                return (false, $"Le lieu ne doit pas dépasser {LieuMaxLength} caractères.");
            return (true, "");
        }

        public static (bool Ok, string Message) ValiderNom(string nom)
        {
            if (string.IsNullOrWhiteSpace(nom))
                return (false, "Le nom est obligatoire.");
            if (nom.Trim().Length > NomMaxLength)
                return (false, $"Le nom ne doit pas dépasser {NomMaxLength} caractères.");
            return (true, "");
        }

        public static (bool Ok, string Message) ValiderCommentaire(string texte)
        {
            if (string.IsNullOrWhiteSpace(texte))
                return (false, "Le commentaire ne peut pas être vide.");
            if (texte.Length > CommentaireMaxLength)
                return (false, $"Le commentaire ne doit pas dépasser {CommentaireMaxLength} caractères.");
            return (true, "");
        }

        public static (bool Ok, string Message) ValiderCategorie(string libelle)
        {
            if (string.IsNullOrWhiteSpace(libelle))
                return (false, "Le libellé de catégorie est obligatoire.");
            if (libelle.Trim().Length > CategorieMaxLength)
                return (false, $"Le libellé ne doit pas dépasser {CategorieMaxLength} caractères.");
            return (true, "");
        }

        public static (bool Ok, string Message) ValiderPrix(decimal? prix)
        {
            if (!prix.HasValue) return (true, "");
            if (prix < PrixMin || prix > PrixMax)
                return (false, $"Le prix doit être entre {PrixMin} et {PrixMax}.");
            return (true, "");
        }

        public static (bool Ok, string Message) ValiderNote(int note)
        {
            if (note < NoteMin || note > NoteMax)
                return (false, $"La note doit être entre {NoteMin} et {NoteMax}.");
            return (true, "");
        }

        public static (bool Ok, string Message) ValiderCapacite(int cap)
        {
            if (cap < 0)
                return (false, "La capacité ne peut pas être négative.");
            if (cap > 100000)
                return (false, "La capacité maximale est 100 000.");
            return (true, "");
        }

        public static string Sanitize(string input)
        {
            if (string.IsNullOrEmpty(input)) return "";
            return input.Trim();
        }

        public static string SanitizeForDisplay(string input, int maxLen = 100)
        {
            if (string.IsNullOrEmpty(input)) return "";
            string s = input.Trim();
            if (s.Length <= maxLen) return s;
            return s.Substring(0, maxLen) + "...";
        }

        public static bool TryParseInt(string input, out int value)
        {
            value = 0;
            if (string.IsNullOrWhiteSpace(input)) return false;
            return int.TryParse(input.Trim(), NumberStyles.Integer, CultureInfo.InvariantCulture, out value);
        }

        public static bool TryParseDecimal(string input, out decimal value)
        {
            value = 0;
            if (string.IsNullOrWhiteSpace(input)) return false;
            return decimal.TryParse(input.Replace(",", "."), NumberStyles.Number, CultureInfo.InvariantCulture, out value);
        }
    }
}
