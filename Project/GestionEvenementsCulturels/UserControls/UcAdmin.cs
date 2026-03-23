using GestionEvenements.Data;
using GestionEvenements.Utilities;
using Microsoft.Data.Sqlite;
using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;

namespace GestionEvenements.UserControls
{
    public class UcAdmin : UserControl
    {
        private TabControl _tabControl;
        private PieChartPanel _chartUtilisateursParRole;
        private PieChartPanel _chartEvenementsParStatut;

        public UcAdmin()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.Dock = DockStyle.Fill;
            _tabControl = new TabControl { Dock = DockStyle.Fill };

            var tabModeration = new TabPage("Modération événements");
            var tabUsers = new TabPage("Utilisateurs");
            var tabCategories = new TabPage("Catégories");
            var tabStats = new TabPage("Tableau de bord");

            BuildOngletModeration(tabModeration);
            BuildOngletUtilisateurs(tabUsers);
            BuildOngletCategories(tabCategories);
            BuildOngletStats(tabStats);

            _tabControl.TabPages.AddRange(new[] { tabModeration, tabUsers, tabCategories, tabStats });
            this.Controls.Add(_tabControl);
        }

        private void BuildOngletModeration(TabPage tab)
        {
            var split = new SplitContainer { Dock = DockStyle.Fill, Orientation = Orientation.Horizontal, Panel1MinSize = 100, Panel2MinSize = 80 };
            split.SplitterDistance = split.Panel1MinSize;

            var dgv = new DataGridView
            {
                Dock = DockStyle.Fill,
                ReadOnly = true,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
            };
            StyliserGrille(dgv);
            split.Panel1.Controls.Add(dgv);

            var panel = new Panel { Dock = DockStyle.Fill, Padding = new Padding(12) };
            var cboStatut = new ComboBox
            {
                Location = new Point(12, 12),
                Width = 150,
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            cboStatut.Items.AddRange(new object[] { "Afficher tout", "Approuve", "EnAttente", "Suspendu", "Supprime" });
            cboStatut.SelectedIndex = 0;
            cboStatut.SelectedIndexChanged += (s, e) => ChargerEvenementsModeration(dgv, cboStatut.SelectedItem?.ToString() ?? "Afficher tout");
            panel.Controls.Add(new Label { Text = "Filtrer par statut", Location = new Point(12, 48) });
            panel.Controls.Add(cboStatut);
            var btnApprouver = new Button { Text = "Approuver", Location = new Point(12, 80), Size = new Size(100, 32) };
            var btnSuspendre = new Button { Text = "Suspendre", Location = new Point(120, 80), Size = new Size(100, 32) };
            var btnSupprimer = new Button { Text = "Supprimer", Location = new Point(228, 80), Size = new Size(100, 32), BackColor = Color.OrangeRed, ForeColor = Color.White };
            btnApprouver.Click += (s, e) => ChangerStatut(dgv, "Approuve", cboStatut.SelectedItem?.ToString() ?? "Afficher tout");
            btnSuspendre.Click += (s, e) => ChangerStatut(dgv, "Suspendu", cboStatut.SelectedItem?.ToString() ?? "Afficher tout");
            btnSupprimer.Click += (s, e) => ChangerStatut(dgv, "Supprime", cboStatut.SelectedItem?.ToString() ?? "Afficher tout");
            panel.Controls.Add(btnApprouver);
            panel.Controls.Add(btnSuspendre);
            panel.Controls.Add(btnSupprimer);
            split.Panel2.Controls.Add(panel);
            tab.Controls.Add(split);
            ChargerEvenementsModeration(dgv, "Afficher tout");
        }

        private void ChangerStatut(DataGridView dgv, string statut, string statutFiltreCourant)
        {
            if (dgv.SelectedRows.Count == 0) { MessageBox.Show("Sélectionnez un événement."); return; }
            var cell = dgv.SelectedRows[0].Cells["Id"];
            if (cell?.Value == null || !int.TryParse(cell.Value.ToString(), out int id))
            { MessageBox.Show("Sélection invalide."); return; }
            try
            {
                using var conn = DbHelper.OuvrirConnexion();
                using var cmd = conn.CreateCommand();
                cmd.CommandText = "UPDATE Evenements SET Statut = $s WHERE Id = $id";
                cmd.Parameters.AddWithValue("$s", statut);
                cmd.Parameters.AddWithValue("$id", id);
                cmd.ExecuteNonQuery();
                if (statut == "Supprime")
                {
                    using var cmd2 = conn.CreateCommand();
                    cmd2.CommandText = "DELETE FROM Inscriptions WHERE EvenementId = $id";
                    cmd2.Parameters.AddWithValue("$id", id);
                    cmd2.ExecuteNonQuery();
                }
                ChargerEvenementsModeration(dgv, statutFiltreCourant);
            }
            catch (Exception ex) { MessageBox.Show("Erreur : " + ex.Message); }
        }

        private void ChargerEvenementsModeration(DataGridView dgv, string statutFiltre)
        {
            try
            {
                using var conn = DbHelper.OuvrirConnexion();
                string sql = @"
SELECT e.Id, e.Titre, e.DateDebut, e.Statut
FROM Evenements e
WHERE ($statut = 'Afficher tout' OR e.Statut = $statut)
ORDER BY e.DateDebut";
                var dt = new DataTable();
                using var cmd = new SqliteCommand(sql, conn);
                cmd.Parameters.AddWithValue("$statut", statutFiltre);
                using var r = cmd.ExecuteReader();
                dt.Load(r);
                dgv.DataSource = dt;
            }
            catch (Exception ex) { MessageBox.Show("Erreur : " + ex.Message); }
        }

        private void BuildOngletUtilisateurs(TabPage tab)
        {
            var split = new SplitContainer { Dock = DockStyle.Fill, Orientation = Orientation.Horizontal, Panel1MinSize = 100, Panel2MinSize = 80 };
            split.SplitterDistance = split.Panel1MinSize;
            split.Resize += (s, ev) => { if (split.Height > 200) { int m = split.Height - split.Panel2MinSize - split.SplitterWidth; if (split.SplitterDistance > m || split.SplitterDistance < split.Panel1MinSize) split.SplitterDistance = Math.Max(split.Panel1MinSize, Math.Min(350, m)); } };
            var dgv = new DataGridView { Dock = DockStyle.Fill, ReadOnly = true, SelectionMode = DataGridViewSelectionMode.FullRowSelect };
            StyliserGrille(dgv);
            split.Panel1.Controls.Add(dgv);

            var panel = new Panel { Dock = DockStyle.Fill, Padding = new Padding(12), AutoScroll = true };
            int y = 12;
            AjouterChamp(panel, "Nom Utilisateur", ref y, out TextBox txtLogin);
            AjouterChamp(panel, "Mot de passe", ref y, out TextBox txtMdp);
            AjouterCombobox(panel, "Rôle", ref y, out ComboBox cboRole);
            cboRole.Items.AddRange(new object[] { "Admin", "Organisateur", "Participant" });
            cboRole.SelectedIndex = 1;
            var btnAjouter = new Button { Text = "Créer", Location = new Point(12, y), Size = new Size(100, 32) };
            var btnDesactiver = new Button { Text = "Désactiver", Location = new Point(120, y), Size = new Size(100, 32), BackColor = Color.OrangeRed, ForeColor = Color.White };
            btnAjouter.Click += (s, e) =>
            {
                var (okL, msgL) = InputValidator.ValiderLogin(txtLogin.Text);
                if (!okL) { MessageBox.Show(msgL); return; }
                var (okM, msgM) = InputValidator.ValiderMotDePasse(txtMdp.Text);
                if (!okM) { MessageBox.Show(msgM); return; }
                try
                {
                    using var conn = DbHelper.OuvrirConnexion();
                    using var cmd = conn.CreateCommand();
                    cmd.CommandText = "INSERT INTO Utilisateurs (Login, MotDePasse, Nom, Role) VALUES ($l, $m, $n, $r)";
                    var login = InputValidator.Sanitize(txtLogin.Text);
                    cmd.Parameters.AddWithValue("$l", login);
                    cmd.Parameters.AddWithValue("$m", txtMdp.Text);
                    cmd.Parameters.AddWithValue("$n", login);
                    cmd.Parameters.AddWithValue("$r", cboRole.SelectedItem?.ToString() ?? "Organisateur");
                    cmd.ExecuteNonQuery();
                    txtLogin.Clear(); txtMdp.Clear();
                    ChargerUtilisateurs(dgv);
                }
                catch (Exception ex) { MessageBox.Show("Erreur : " + ex.Message); }
            };
            btnDesactiver.Click += (s, e) =>
            {
                if (dgv.SelectedRows.Count == 0) { MessageBox.Show("Sélectionnez un utilisateur."); return; }
                var cell = dgv.SelectedRows[0].Cells["Id"];
                if (cell?.Value == null || !int.TryParse(cell.Value.ToString(), out int id))
                { MessageBox.Show("Sélection invalide."); return; }
                try
                {
                    using var conn = DbHelper.OuvrirConnexion();
                    using var cmd = conn.CreateCommand();
                    cmd.CommandText = "UPDATE Utilisateurs SET Actif = 0 WHERE Id = $id";
                    cmd.Parameters.AddWithValue("$id", id);
                    cmd.ExecuteNonQuery();
                    ChargerUtilisateurs(dgv);
                }
                catch (Exception ex) { MessageBox.Show("Erreur : " + ex.Message); }
            };
            panel.Controls.Add(btnAjouter);
            panel.Controls.Add(btnDesactiver);
            split.Panel2.Controls.Add(panel);
            tab.Controls.Add(split);
            ChargerUtilisateurs(dgv);
        }

        private void AjouterChamp(Panel p, string labelText, ref int y, out TextBox txt)
        {
            var lbl = new Label { Text = labelText, Location = new Point(12, y) };
            txt = new TextBox { Location = new Point(12, y + 22), Width = 220, MinimumSize = new Size(120, 22), Font = new Font("Segoe UI", 10) };
            p.Controls.Add(lbl);
            p.Controls.Add(txt);
            y += 56;
        }

        private void AjouterCombobox(Panel p, string labelText, ref int y, out ComboBox cbo)
        {
            var lbl = new Label { Text = labelText, Location = new Point(12, y) };
            cbo = new ComboBox { Location = new Point(12, y + 22), Width = 220, DropDownStyle = ComboBoxStyle.DropDownList };
            p.Controls.Add(lbl);
            p.Controls.Add(cbo);
            y += 56;
        }

        private void ChargerUtilisateurs(DataGridView dgv)
        {
            try
            {
                using var conn = DbHelper.OuvrirConnexion();
                string sql = "SELECT Id, Login AS [Nom Utilisateur], Role, COALESCE(Actif,1) as Actif FROM Utilisateurs ORDER BY Login";
                var dt = new DataTable();
                using var cmd = new SqliteCommand(sql, conn);
                using var r = cmd.ExecuteReader();
                dt.Load(r);
                dgv.DataSource = dt;
            }
            catch (Exception ex) { MessageBox.Show("Erreur : " + ex.Message); }
        }

        private void BuildOngletCategories(TabPage tab)
        {
            var split = new SplitContainer { Dock = DockStyle.Fill, Orientation = Orientation.Horizontal, Panel1MinSize = 100, Panel2MinSize = 80 };
            split.SplitterDistance = split.Panel1MinSize;
            split.Resize += (s, ev) => { if (split.Height > 200) { int m = split.Height - split.Panel2MinSize - split.SplitterWidth; if (split.SplitterDistance > m || split.SplitterDistance < split.Panel1MinSize) split.SplitterDistance = Math.Max(split.Panel1MinSize, Math.Min(300, m)); } };
            var dgv = new DataGridView { Dock = DockStyle.Fill, ReadOnly = true, SelectionMode = DataGridViewSelectionMode.FullRowSelect };
            StyliserGrille(dgv);
            split.Panel1.Controls.Add(dgv);

            var panel = new Panel { Dock = DockStyle.Fill, Padding = new Padding(12) };
            var txtLibelle = new TextBox { Location = new Point(12, 40), Width = 220, MinimumSize = new Size(120, 22), Font = new Font("Segoe UI", 10) };
            panel.Controls.Add(new Label { Text = "Nouvelle catégorie", Location = new Point(12, 12) });
            panel.Controls.Add(txtLibelle);
            var btnAjouter = new Button { Text = "Ajouter", Location = new Point(12, 80), Size = new Size(100, 32) };
            btnAjouter.Click += (s, e) =>
            {
                var (ok, msg) = InputValidator.ValiderCategorie(txtLibelle.Text);
                if (!ok) { MessageBox.Show(msg); return; }
                try
                {
                    using var conn = DbHelper.OuvrirConnexion();
                    using var cmd = conn.CreateCommand();
                    cmd.CommandText = "INSERT INTO Categories (Libelle) VALUES ($l)";
                    cmd.Parameters.AddWithValue("$l", InputValidator.Sanitize(txtLibelle.Text));
                    cmd.ExecuteNonQuery();
                    txtLibelle.Clear();
                    ChargerCategories(dgv);
                }
                catch (Exception ex) { MessageBox.Show("Erreur : " + ex.Message); }
            };
            panel.Controls.Add(btnAjouter);
            split.Panel2.Controls.Add(panel);
            tab.Controls.Add(split);
            ChargerCategories(dgv);
        }

        private void ChargerCategories(DataGridView dgv)
        {
            try
            {
                using var conn = DbHelper.OuvrirConnexion();
                var dt = new DataTable();
                using var cmd = new SqliteCommand("SELECT Id, Libelle FROM Categories ORDER BY Libelle", conn);
                using var r = cmd.ExecuteReader();
                dt.Load(r);
                dgv.DataSource = dt;
            }
            catch (Exception ex) { MessageBox.Show("Erreur : " + ex.Message); }
        }

        private void BuildOngletStats(TabPage tab)
        {
            var panel = new Panel { Dock = DockStyle.Fill, Padding = new Padding(24), AutoScroll = true };
            var lbl = new Label { AutoSize = true, Location = new Point(24, 24), MaximumSize = new Size(600, 0), Font = new Font("Segoe UI", 11) };
            _chartUtilisateursParRole = CreerPieChart("Utilisateurs par rôle", new Point(24, 130), new Size(420, 260));
            _chartEvenementsParStatut = CreerPieChart("Événements par statut", new Point(470, 130), new Size(420, 260));
            panel.Controls.Add(lbl);
            panel.Controls.Add(_chartUtilisateursParRole);
            panel.Controls.Add(_chartEvenementsParStatut);
            tab.Controls.Add(panel);
            try
            {
                using var conn = DbHelper.OuvrirConnexion();
                int nbUsers = 0, nbEvts = 0, nbInsc = 0;
                using (var cmd = new SqliteCommand("SELECT COUNT(*) FROM Utilisateurs WHERE Actif = 1", conn))
                    nbUsers = Convert.ToInt32(cmd.ExecuteScalar());
                using (var cmd = new SqliteCommand("SELECT COUNT(*) FROM Evenements WHERE Statut = 'Approuve'", conn))
                    nbEvts = Convert.ToInt32(cmd.ExecuteScalar());
                using (var cmd = new SqliteCommand("SELECT COUNT(*) FROM Inscriptions", conn))
                    nbInsc = Convert.ToInt32(cmd.ExecuteScalar());
                lbl.Text = $"📊 Tableau de bord global\n\n• Utilisateurs actifs : {nbUsers}\n• Événements approuvés : {nbEvts}\n• Total inscriptions : {nbInsc}";

                ChargerPieUtilisateursParRole();
                ChargerPieEvenementsParStatut();
            }
            catch (Exception ex) { lbl.Text = "Erreur : " + ex.Message; }
        }

        private PieChartPanel CreerPieChart(string titre, Point location, Size size)
        {
            return new PieChartPanel
            {
                Title = titre,
                Location = location,
                Size = size,
                BackColor = Color.White
            };
        }

        private void ChargerPieUtilisateursParRole()
        {
            if (_chartUtilisateursParRole == null) return;
            var data = new System.Collections.Generic.Dictionary<string, int>();
            using var conn = DbHelper.OuvrirConnexion();
            using var cmd = new SqliteCommand(
                "SELECT Role, COUNT(*) FROM Utilisateurs WHERE COALESCE(Actif,1)=1 GROUP BY Role",
                conn);
            using var r = cmd.ExecuteReader();
            while (r.Read())
            {
                var role = r.IsDBNull(0) ? "-" : r.GetString(0);
                var count = r.IsDBNull(1) ? 0 : r.GetInt32(1);
                data[role] = count;
            }
            _chartUtilisateursParRole.SetData(data);
        }

        private void ChargerPieEvenementsParStatut()
        {
            if (_chartEvenementsParStatut == null) return;
            var data = new System.Collections.Generic.Dictionary<string, int>();
            using var conn = DbHelper.OuvrirConnexion();
            using var cmd = new SqliteCommand(
                "SELECT Statut, COUNT(*) FROM Evenements GROUP BY Statut",
                conn);
            using var r = cmd.ExecuteReader();
            while (r.Read())
            {
                var statut = r.IsDBNull(0) ? "-" : r.GetString(0);
                var count = r.IsDBNull(1) ? 0 : r.GetInt32(1);
                data[statut] = count;
            }
            _chartEvenementsParStatut.SetData(data);
        }

        private void StyliserGrille(DataGridView dgv)
        {
            dgv.BackgroundColor = Color.White;
            dgv.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(245, 247, 250);
            dgv.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(0, 120, 215);
            dgv.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            dgv.ColumnHeadersVisible = true;
            dgv.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dgv.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            dgv.ColumnHeadersDefaultCellStyle.Padding = new Padding(4, 8, 4, 8);
            dgv.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;
            dgv.DefaultCellStyle.Padding = new Padding(2, 4, 2, 4);
            dgv.RowTemplate.Height = 32;
            dgv.EnableHeadersVisualStyles = false;
        }

        private class PieChartPanel : Panel
        {
            private System.Collections.Generic.Dictionary<string, int> _data = new();
            private readonly Color[] _palette = new[] { Color.SteelBlue, Color.Orange, Color.SeaGreen, Color.MediumPurple, Color.IndianRed, Color.Goldenrod };
            public string Title { get; set; } = "";

            public void SetData(System.Collections.Generic.Dictionary<string, int> data)
            {
                _data = data ?? new System.Collections.Generic.Dictionary<string, int>();
                Invalidate();
            }

            protected override void OnPaint(PaintEventArgs e)
            {
                base.OnPaint(e);
                e.Graphics.Clear(BackColor);
                e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

                using var titleFont = new Font("Segoe UI", 10, FontStyle.Bold);
                using var textBrush = new SolidBrush(Color.Black);
                e.Graphics.DrawString(Title, titleFont, textBrush, new PointF(8, 8));

                int total = 0;
                foreach (var kv in _data) total += kv.Value;
                if (total <= 0)
                {
                    e.Graphics.DrawString("Aucune donnée", Font, textBrush, new PointF(8, 36));
                    return;
                }

                var pieRect = new Rectangle(8, 36, Math.Min(180, Width - 180), Math.Min(180, Height - 48));
                float start = 0f;
                int i = 0;
                foreach (var kv in _data)
                {
                    float sweep = 360f * kv.Value / total;
                    using var b = new SolidBrush(_palette[i % _palette.Length]);
                    e.Graphics.FillPie(b, pieRect, start, sweep);
                    start += sweep;
                    i++;
                }

                int legendX = pieRect.Right + 12;
                int legendY = pieRect.Top;
                i = 0;
                foreach (var kv in _data)
                {
                    using var b = new SolidBrush(_palette[i % _palette.Length]);
                    e.Graphics.FillRectangle(b, legendX, legendY + 2, 12, 12);
                    e.Graphics.DrawString($"{kv.Key}: {kv.Value}", Font, textBrush, new PointF(legendX + 18, legendY));
                    legendY += 20;
                    i++;
                }
            }
        }
    }
}
