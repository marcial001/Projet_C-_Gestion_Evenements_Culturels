using Microsoft.Data.Sqlite;
using System;
using System.IO;

namespace GestionEvenements.Data
{
    public static class DbContext
    {
        public static string DbPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "evenements_culturels.db");
        public static string ConnectionString => $"Data Source={DbPath};";

        public static void InitDatabase()
        {
            Directory.CreateDirectory(Path.GetDirectoryName(DbPath));

            using var conn = new SqliteConnection(ConnectionString);
            conn.Open();

            string sql = @"
CREATE TABLE IF NOT EXISTS Utilisateurs (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    Login TEXT UNIQUE NOT NULL,
    Password TEXT NOT NULL,
    Nom TEXT NOT NULL,
    Role TEXT DEFAULT 'Organisateur'
);

CREATE TABLE IF NOT EXISTS Evenements (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    Titre TEXT NOT NULL,
    Description TEXT,
    DateDebut TEXT NOT NULL,          -- 'yyyy-MM-dd HH:mm'
    DateFin TEXT,
    Lieu TEXT,
    Capacite INTEGER DEFAULT 0,
    CreeParId INTEGER,
    FOREIGN KEY(CreeParId) REFERENCES Utilisateurs(Id)
);

CREATE TABLE IF NOT EXISTS Inscriptions (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    EvenementId INTEGER NOT NULL,
    UtilisateurId INTEGER NOT NULL,
    DateInscription TEXT DEFAULT CURRENT_TIMESTAMP,
    Statut TEXT DEFAULT 'Inscrit',
    UNIQUE(EvenementId, UtilisateurId),
    FOREIGN KEY(EvenementId) REFERENCES Evenements(Id),
    FOREIGN KEY(UtilisateurId) REFERENCES Utilisateurs(Id)
);";

            using var cmd = new SqliteCommand(sql, conn);
            cmd.ExecuteNonQuery();

            // Utilisateurs de test
            cmd.CommandText = @"
INSERT OR IGNORE INTO Utilisateurs (Login, Password, Nom, Role)
VALUES 
    ('admin', 'admin123', 'Administrateur', 'Admin'),
    ('user1', '1234', 'Étudiant Test', 'Participant'),
    ('orga1', 'orga2025', 'Organisateur 1', 'Organisateur');";
            cmd.ExecuteNonQuery();
        }

        // Méthode helper pour exécuter SELECT → retourne liste ou premier résultat
        // (tu pourras l’étendre)
    }
}