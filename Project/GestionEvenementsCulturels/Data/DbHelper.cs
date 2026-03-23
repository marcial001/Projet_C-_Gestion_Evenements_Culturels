using Microsoft.Data.Sqlite;
using System;
using System.Data;
using System.IO;
using System.Windows.Forms;

namespace GestionEvenements.Data
{
    public static class DbHelper
    {
        public static string CheminBase = Path.Combine(Application.StartupPath ?? ".", "evenements.db");
        public static string ChaineConnexion => $"Data Source={CheminBase};";

        public static SqliteConnection OuvrirConnexion()
        {
            var conn = new SqliteConnection(ChaineConnexion);
            conn.Open();
            AppliquerPragmas(conn);
            return conn;
        }

        public static void AppliquerPragmas(SqliteConnection conn)
        {
            if (conn.State != ConnectionState.Open) return;
            using var cmd = conn.CreateCommand();
            cmd.CommandText = "PRAGMA busy_timeout = 8000;"; cmd.ExecuteNonQuery();
            cmd.CommandText = "PRAGMA journal_mode = WAL;"; cmd.ExecuteNonQuery();
            cmd.CommandText = "PRAGMA foreign_keys = ON;"; cmd.ExecuteNonQuery();
        }

        public static void InitialiserBase()
        {
            var dir = Path.GetDirectoryName(CheminBase);
            if (!string.IsNullOrEmpty(dir)) Directory.CreateDirectory(dir);
            using var connexion = new SqliteConnection(ChaineConnexion);
            connexion.Open();
            AppliquerPragmas(connexion);
            CreerTables(connexion);
            MigrerSchema(connexion);
            InsererDonneesInitiales(connexion);
        }

        private static void CreerTables(SqliteConnection conn)
        {
            string creation = @"
CREATE TABLE IF NOT EXISTS Utilisateurs (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    Login TEXT UNIQUE NOT NULL,
    MotDePasse TEXT NOT NULL,
    Nom TEXT NOT NULL,
    Role TEXT DEFAULT 'Organisateur',
    Actif INTEGER DEFAULT 1
);
CREATE TABLE IF NOT EXISTS Categories (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    Libelle TEXT UNIQUE NOT NULL
);
CREATE TABLE IF NOT EXISTS Evenements (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    Titre TEXT NOT NULL,
    Description TEXT,
    DateDebut TEXT NOT NULL,
    DateFin TEXT,
    Lieu TEXT,
    Capacite INTEGER DEFAULT 0,
    CreeParId INTEGER,
    CategorieId INTEGER,
    PrixStandard REAL DEFAULT 0,
    PrixVIP REAL DEFAULT 0,
    Statut TEXT DEFAULT 'EnAttente',
    FOREIGN KEY(CreeParId) REFERENCES Utilisateurs(Id),
    FOREIGN KEY(CategorieId) REFERENCES Categories(Id)
);
CREATE TABLE IF NOT EXISTS Inscriptions (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    EvenementId INTEGER NOT NULL,
    UtilisateurId INTEGER NOT NULL,
    DateInscription TEXT DEFAULT CURRENT_TIMESTAMP,
    Statut TEXT DEFAULT 'Inscrit',
    TypeBillet TEXT DEFAULT 'Standard',
    PrixPaye REAL DEFAULT 0,
    UNIQUE(EvenementId, UtilisateurId),
    FOREIGN KEY(EvenementId) REFERENCES Evenements(Id),
    FOREIGN KEY(UtilisateurId) REFERENCES Utilisateurs(Id)
);
CREATE TABLE IF NOT EXISTS Commentaires (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    EvenementId INTEGER NOT NULL,
    UtilisateurId INTEGER NOT NULL,
    Note INTEGER NOT NULL,
    Texte TEXT,
    DateCommentaire TEXT DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY(EvenementId) REFERENCES Evenements(Id),
    FOREIGN KEY(UtilisateurId) REFERENCES Utilisateurs(Id)
);";
            using var cmd = new SqliteCommand(creation, conn);
            cmd.ExecuteNonQuery();
        }

        private static void MigrerSchema(SqliteConnection conn)
        {
            string[] migrations = new[]
            {
                "ALTER TABLE Utilisateurs ADD COLUMN Actif INTEGER DEFAULT 1",
                "ALTER TABLE Evenements ADD COLUMN CategorieId INTEGER",
                "ALTER TABLE Evenements ADD COLUMN PrixStandard REAL DEFAULT 0",
                "ALTER TABLE Evenements ADD COLUMN PrixVIP REAL DEFAULT 0",
                "ALTER TABLE Evenements ADD COLUMN Statut TEXT DEFAULT 'EnAttente'",
                "ALTER TABLE Inscriptions ADD COLUMN TypeBillet TEXT DEFAULT 'Standard'",
                "ALTER TABLE Inscriptions ADD COLUMN PrixPaye REAL DEFAULT 0"
            };
            foreach (var sql in migrations)
            {
                try { using var cmd = new SqliteCommand(sql, conn); cmd.ExecuteNonQuery(); }
                catch (SqliteException) { }
            }
        }

        private static void InsererDonneesInitiales(SqliteConnection conn)
        {
            using var cmd = conn.CreateCommand();
            cmd.CommandText = @"
INSERT OR IGNORE INTO Categories (Libelle) VALUES ('Concert'), ('Théâtre'), ('Exposition'), ('Conférence'), ('Autre');
INSERT OR IGNORE INTO Utilisateurs (Login, MotDePasse, Nom, Role) VALUES
    ('admin', 'admin123', 'Administrateur', 'Admin'),
    ('orga', 'orga2025', 'Organisateur Test', 'Organisateur'),
    ('etudiant', '1234', 'Étudiant Test', 'Participant');";
            cmd.ExecuteNonQuery();
        }
    }
}