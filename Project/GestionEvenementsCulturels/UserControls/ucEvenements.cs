using GestionEvenements.Data;
using Microsoft.Data.Sqlite;
using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;

namespace GestionEvenements
{
    public partial class UcEvenements : UserControl
    {
        public UcEvenements()
        {
            InitializeComponent();
            ChargerListeEvenements();
            StyliserGrille();
        }

        private DataGridView dgvEvenements;
        private TextBox txtTitre, txtDesc, txtLieu;
        private DateTimePicker dtpDebut, dtpFin;
        private NumericUpDown nudCapacite;
        private Button btnAjouter, btnSupprimer;
        private Label lblAlerte;

        private void InitializeComponent()
        {
            // --- Configuration basique des contrôles (à compléter dans le designer si possible) ---

            dgvEvenements = new DataGridView { Dock = DockStyle.Top, Height = 300, ReadOnly = true };
            this.Controls.Add(dgvEvenements);

            // Formulaire ajout (panel en bas)
            var panelForm = new Panel { Dock = DockStyle.Bottom, Height = 220 };
            this.Controls.Add(panelForm);

            // Champs
            var lblTitre = new Label { Text = "Titre :", Location = new Point(20, 20) };
            txtTitre = new TextBox { Location = new Point(100, 20), Width = 300 };

            var lblDesc = new Label { Text = "Description :", Location = new Point(20, 60) };
            txtDesc = new TextBox { Location = new Point(100, 60), Width = 300, Multiline = true, Height = 60 };

            var lblDebut = new Label { Text = "Début :", Location = new Point(20, 130) };
            dtpDebut = new DateTimePicker { Location = new Point(100, 130), Width = 200 };

            var lblFin = new Label { Text = "Fin :", Location = new Point(20, 170) };
            dtpFin = new DateTimePicker { Location = new Point(100, 170), Width = 200, ShowCheckBox = true };

            var lblLieu = new Label { Text = "Lieu :", Location = new Point(320, 20) };
            txtLieu = new TextBox { Location = new Point(380, 20), Width = 200 };

            var lblCap = new Label { Text = "Capacité :", Location = new Point(320, 60) };
            nudCapacite = new NumericUpDown { Location = new Point(380, 60), Width = 100 };

            btnAjouter = new Button { Text = "Ajouter événement", BackColor = Color.ForestGreen, ForeColor = Color.White, Location = new Point(380, 130), Size = new Size(200, 40) };
            btnAjouter.Click += BtnAjouter_Click;

            btnSupprimer = new Button { Text = "Supprimer sélection", BackColor = Color.OrangeRed, ForeColor = Color.White, Location = new Point(380, 180), Size = new Size(200, 40) };
            btnSupprimer.Click += BtnSupprimer_Click;

            lblAlerte = new Label { ForeColor = Color.OrangeRed, Location = new Point(20, 220), Size = new Size(600, 30), Font = new Font("Segoe UI", 10, FontStyle.Bold) };

            panelForm.Controls.AddRange(new Control[] { lblTitre, txtTitre, lblDesc, txtDesc, lblDebut, dtpDebut, lblFin, dtpFin, lblLieu, txtLieu, lblCap, nudCapacite, btnAjouter, btnSupprimer, lblAlerte });
        }

        private void StyliserGrille()
        {
            dgvEvenements.BackgroundColor = Color.White;
            dgvEvenements.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(245, 247, 250);
            dgvEvenements.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(0, 120, 215);
            dgvEvenements.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            dgvEvenements.EnableHeadersVisualStyles = false;
            dgvEvenements.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        }

        private void ChargerListeEvenements()
        {
            using var conn = DbHelper.OuvrirConnexion();

            string sql = @"
SELECT e.Id, e.Titre, e.DateDebut, e.Lieu, e.Capacite,
       COUNT(i.Id) as Inscrits
FROM Evenements e
LEFT JOIN Inscriptions i ON e.Id = i.EvenementId
GROUP BY e.Id
ORDER BY e.DateDebut";

            DataTable dt = new DataTable();
            using var cmd = new SqliteCommand(sql, conn);
            using var reader = cmd.ExecuteReader();
            dt.Load(reader);
            dgvEvenements.DataSource = dt;

            VerifierAlertes(dt);
        }

        private void VerifierAlertes(DataTable dt)
        {
            DateTime maintenant = DateTime.Now;
            lblAlerte.Text = "";

            foreach (DataRow row in dt.Rows)
            {
                DateTime debut = DateTime.Parse(row["DateDebut"].ToString());
                if (debut > maintenant && (debut - maintenant).TotalHours <= 24)
                {
                    lblAlerte.Text = $"⚠ Événement proche : {row["Titre"]} le {debut:dd/MM/yyyy HH:mm}";
                    return;
                }
            }
        }

        private void BtnAjouter_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtTitre.Text) || dtpDebut.Value == DateTime.MinValue)
            {
                MessageBox.Show("Titre et date de début obligatoires", "Erreur");
                return;
            }

            using var conn = DbHelper.OuvrirConnexion();

            using var cmd = conn.CreateCommand();
            cmd.CommandText = @"
INSERT INTO Evenements (Titre, Description, DateDebut, DateFin, Lieu, Capacite, CreeParId)
VALUES ($titre, $desc, $debut, $fin, $lieu, $cap, $creePar)";

            cmd.Parameters.AddWithValue("$titre", txtTitre.Text);
            cmd.Parameters.AddWithValue("$desc", txtDesc.Text);
            cmd.Parameters.AddWithValue("$debut", dtpDebut.Value.ToString("yyyy-MM-dd HH:mm"));
            cmd.Parameters.AddWithValue("$fin", dtpFin.Checked ? dtpFin.Value.ToString("yyyy-MM-dd HH:mm") : DBNull.Value);
            cmd.Parameters.AddWithValue("$lieu", txtLieu.Text);
            cmd.Parameters.AddWithValue("$cap", nudCapacite.Value);
            cmd.Parameters.AddWithValue("$creePar", Session.UtilisateurId);

            cmd.ExecuteNonQuery();

            MessageBox.Show("Événement ajouté avec succès", "Succès");
            ChargerListeEvenements();
            // Clear fields si voulu
        }

        private void BtnSupprimer_Click(object sender, EventArgs e)
        {
            if (dgvEvenements.SelectedRows.Count == 0) return;

            int id = Convert.ToInt32(dgvEvenements.SelectedRows[0].Cells["Id"].Value);

            if (MessageBox.Show("Voulez-vous vraiment supprimer cet événement ?", "Confirmation", MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                using var conn = DbHelper.OuvrirConnexion();
                using var cmd = conn.CreateCommand();
                cmd.CommandText = "DELETE FROM Evenements WHERE Id = $id";
                cmd.Parameters.AddWithValue("$id", id);
                cmd.ExecuteNonQuery();

                ChargerListeEvenements();
            }
        }
    }
}