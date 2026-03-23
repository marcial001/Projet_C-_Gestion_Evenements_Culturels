using GestionEvenements.Data;
using GestionEvenements.Utilities;
using Microsoft.Data.Sqlite;
using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;

namespace GestionEvenements.UserControls
{
    public class UcEtudiant : UserControl
    {
        private TabControl _tabControl;
        private DataGridView _dgvCatalog;

        public UcEtudiant()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.Dock = DockStyle.Fill;
            _tabControl = new TabControl { Dock = DockStyle.Fill };

            var tabExplorer = new TabPage("Explorer les événements");
            var tabMesTickets = new TabPage("Mes billets");
            var tabAvis = new TabPage("Noter / Commenter");

            BuildOngletExplorer(tabExplorer);
            BuildOngletMesTickets(tabMesTickets);
            BuildOngletAvis(tabAvis);

            _tabControl.TabPages.AddRange(new[] { tabExplorer, tabMesTickets, tabAvis });
            this.Controls.Add(_tabControl);
        }

        private void BuildOngletExplorer(TabPage tab)
        {
            var split = new SplitContainer { Dock = DockStyle.Fill, Orientation = Orientation.Horizontal, Panel1MinSize = 100, Panel2MinSize = 80 };
            split.SplitterDistance = split.Panel1MinSize;
            split.Resize += (s, ev) => { if (split.Height > 200) { int m = split.Height - split.Panel2MinSize - split.SplitterWidth; if (split.SplitterDistance > m || split.SplitterDistance < split.Panel1MinSize) split.SplitterDistance = Math.Max(split.Panel1MinSize, Math.Min(400, m)); } };
            _dgvCatalog = new DataGridView
            {
                Dock = DockStyle.Fill,
                ReadOnly = true,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
            };
            StyliserGrille(_dgvCatalog);
            split.Panel1.Controls.Add(_dgvCatalog);

            var panel = new Panel { Dock = DockStyle.Fill, Padding = new Padding(12), AutoScroll = true };
            var lblDetail = new Label
            {
                AutoSize = true,
                Location = new Point(12, 12),
                MaximumSize = new Size(400, 0),
                Font = new Font("Segoe UI", 10)
            };
            panel.Controls.Add(lblDetail);
            panel.Controls.Add(new Label { Text = "Type de billet", Location = new Point(12, 140) });
            var cboTypeBillet = new ComboBox
            {
                Location = new Point(12, 160),
                Width = 180,
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            cboTypeBillet.Items.AddRange(new object[] { "Standard", "VIP" });
            cboTypeBillet.SelectedIndex = 0;
            panel.Controls.Add(cboTypeBillet);

            var btnReserver = new Button
            {
                Text = "Réserver un billet",
                BackColor = Color.ForestGreen,
                ForeColor = Color.White,
                Location = new Point(12, 200),
                Size = new Size(180, 36),
                FlatStyle = FlatStyle.Flat
            };
            btnReserver.Click += (s, e) => ReserverBillet(_dgvCatalog, lblDetail, cboTypeBillet.SelectedItem?.ToString() ?? "Standard");
            panel.Controls.Add(btnReserver);

            _dgvCatalog.SelectionChanged += (s, e) =>
            {
                if (_dgvCatalog.SelectedRows.Count > 0)
                    AfficherDetailSelection(lblDetail);
            };

            split.Panel2.Controls.Add(panel);
            tab.Controls.Add(split);
            ChargerCatalog(lblDetail);
        }

        private void ChargerCatalog(Label lblDetail)
        {
            try
            {
                using var conn = DbHelper.OuvrirConnexion();
                string sql = @"
SELECT e.Id, e.Titre, e.DateDebut, e.Lieu, e.Capacite, e.PrixStandard, e.PrixVIP,
       COALESCE(c.Libelle,'-') as Categorie,
       COUNT(i.Id) as Inscrits, e.Statut
FROM Evenements e
LEFT JOIN Categories c ON e.CategorieId = c.Id
LEFT JOIN Inscriptions i ON e.Id = i.EvenementId
WHERE e.Statut = 'Approuve'
GROUP BY e.Id
HAVING e.Capacite = 0 OR COUNT(i.Id) < e.Capacite
ORDER BY e.DateDebut";
                var dt = new DataTable();
                using var cmd = new SqliteCommand(sql, conn);
                using var r = cmd.ExecuteReader();
                dt.Load(r);
                _dgvCatalog.DataSource = dt;
                lblDetail.Text = "Sélectionnez un événement pour voir les détails et réserver.";
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erreur : " + ex.Message, "Erreur");
            }
        }

        private void AfficherDetailSelection(Label lbl)
        {
            if (_dgvCatalog.SelectedRows.Count == 0) return;
            var row = _dgvCatalog.SelectedRows[0];
            var titre = row.Cells["Titre"]?.Value?.ToString() ?? "-";
            var lieu = row.Cells["Lieu"]?.Value?.ToString() ?? "-";
            var debut = row.Cells["DateDebut"]?.Value?.ToString() ?? "-";
            var cat = row.Cells["Categorie"]?.Value?.ToString() ?? "-";
            var pStd = row.Cells["PrixStandard"]?.Value;
            var pVip = row.Cells["PrixVIP"]?.Value;
            decimal ps = 0, pv = 0;
            if (pStd != null && pStd != DBNull.Value) decimal.TryParse(pStd.ToString(), out ps);
            if (pVip != null && pVip != DBNull.Value) decimal.TryParse(pVip.ToString(), out pv);
            lbl.Text = $"📌 {titre}\n\nLieu : {lieu}\nDate : {debut}\nCatégorie : {cat}\n\nPrix Standard : {ps:N2} CFA\nPrix VIP : {pv:N2} CFA";
        }

        private void ReserverBillet(DataGridView dgv, Label lblDetail, string typeBilletChoisi)
        {
            if (dgv.SelectedRows.Count == 0)
            {
                MessageBox.Show("Sélectionnez un événement.", "Attention");
                return;
            }
            var cellId = dgv.SelectedRows[0].Cells["Id"];
            if (cellId?.Value == null || !int.TryParse(cellId.Value.ToString(), out int evtId))
            {
                MessageBox.Show("Sélection invalide.", "Erreur");
                return;
            }
            try
            {
                using var conn = DbHelper.OuvrirConnexion();
                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = "SELECT COUNT(*) FROM Inscriptions WHERE EvenementId = $e AND UtilisateurId = $u";
                    cmd.Parameters.AddWithValue("$e", evtId);
                    cmd.Parameters.AddWithValue("$u", Session.UtilisateurId);
                    if (Convert.ToInt32(cmd.ExecuteScalar()) > 0)
                    {
                        MessageBox.Show("Vous êtes déjà inscrit à cet événement.", "Information");
                        return;
                    }
                }
                decimal prixPaye = 0;
                var row = dgv.SelectedRows[0];
                bool isVip = string.Equals(typeBilletChoisi, "VIP", StringComparison.OrdinalIgnoreCase);
                var pStd = row.Cells["PrixStandard"]?.Value;
                var pVip = row.Cells["PrixVIP"]?.Value;
                if (isVip)
                {
                    if (pVip != null && pVip != DBNull.Value)
                        decimal.TryParse(pVip.ToString(), out prixPaye);
                }
                else
                {
                    if (pStd != null && pStd != DBNull.Value)
                        decimal.TryParse(pStd.ToString(), out prixPaye);
                }

                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = "INSERT INTO Inscriptions (EvenementId, UtilisateurId, Statut, TypeBillet, PrixPaye) VALUES ($e, $u, 'Inscrit', $t, $p)";
                    cmd.Parameters.AddWithValue("$e", evtId);
                    cmd.Parameters.AddWithValue("$u", Session.UtilisateurId);
                    cmd.Parameters.AddWithValue("$t", isVip ? "VIP" : "Standard");
                    cmd.Parameters.AddWithValue("$p", prixPaye);
                    cmd.ExecuteNonQuery();
                }
                MessageBox.Show("Billet réservé avec succès.", "Succès");
                ChargerCatalog(lblDetail);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erreur : " + ex.Message, "Erreur");
            }
        }

        private void BuildOngletMesTickets(TabPage tab)
        {
            var dgv = new DataGridView
            {
                Dock = DockStyle.Fill,
                ReadOnly = true,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
            };
            StyliserGrille(dgv);
            tab.Controls.Add(dgv);
            ChargerMesTickets(dgv);
        }

        private void ChargerMesTickets(DataGridView dgv)
        {
            try
            {
                using var conn = DbHelper.OuvrirConnexion();
                string sql = @"
SELECT e.Titre, e.DateDebut, e.Lieu, i.DateInscription, i.TypeBillet, i.Statut
FROM Inscriptions i
JOIN Evenements e ON e.Id = i.EvenementId
WHERE i.UtilisateurId = $uid
ORDER BY e.DateDebut DESC";
                using var cmd = new SqliteCommand(sql, conn);
                cmd.Parameters.AddWithValue("$uid", Session.UtilisateurId);
                var dt = new DataTable();
                using var r = cmd.ExecuteReader();
                dt.Load(r);
                dgv.DataSource = dt;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erreur : " + ex.Message, "Erreur");
            }
        }

        private void BuildOngletAvis(TabPage tab)
        {
            var split = new SplitContainer { Dock = DockStyle.Fill, Orientation = Orientation.Horizontal, Panel1MinSize = 100, Panel2MinSize = 80 };
            split.SplitterDistance = split.Panel1MinSize;
            split.Resize += (s, ev) => { if (split.Height > 200) { int m = split.Height - split.Panel2MinSize - split.SplitterWidth; if (split.SplitterDistance > m || split.SplitterDistance < split.Panel1MinSize) split.SplitterDistance = Math.Max(split.Panel1MinSize, Math.Min(320, m)); } };
            var dgv = new DataGridView
            {
                Dock = DockStyle.Fill,
                ReadOnly = true,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect
            };
            StyliserGrille(dgv);
            split.Panel1.Controls.Add(dgv);

            var panel = new Panel { Dock = DockStyle.Fill, Padding = new Padding(12), AutoScroll = true };
            panel.Controls.Add(new Label { Text = "Note (1-5)", Location = new Point(12, 12) });
            var nudNote = new NumericUpDown
            {
                Location = new Point(12, 36),
                Width = 60,
                Minimum = 1,
                Maximum = 5,
                Value = 5
            };
            panel.Controls.Add(nudNote);
            panel.Controls.Add(new Label { Text = "Commentaire", Location = new Point(12, 70) });
            var txtCommentaire = new TextBox
            {
                Location = new Point(12, 94),
                Width = 280,
                Height = 80,
                Multiline = true,
                ScrollBars = ScrollBars.Vertical,
                Font = new Font("Segoe UI", 10)
            };
            txtCommentaire.MaxLength = 500;
            panel.Controls.Add(txtCommentaire);
            var btnEnregistrer = new Button
            {
                Text = "Enregistrer l'avis",
                Location = new Point(12, 190),
                Size = new Size(150, 32)
            };
            btnEnregistrer.Click += (s, e) =>
            {
                if (dgv.SelectedRows.Count == 0)
                {
                    MessageBox.Show("Sélectionnez un événement auquel vous avez participé.");
                    return;
                }
                var cellId = dgv.SelectedRows[0].Cells["Id"];
                if (cellId?.Value == null || !int.TryParse(cellId.Value.ToString(), out int evtId))
                {
                    MessageBox.Show("Sélection invalide.");
                    return;
                }
                var (ok, msg) = InputValidator.ValiderCommentaire(txtCommentaire.Text);
                if (!ok) { MessageBox.Show(msg); return; }
                var (okN, msgN) = InputValidator.ValiderNote((int)nudNote.Value);
                if (!okN) { MessageBox.Show(msgN); return; }
                try
                {
                    using var conn = DbHelper.OuvrirConnexion();
                    using (var cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = "SELECT COUNT(*) FROM Inscriptions WHERE EvenementId = $e AND UtilisateurId = $u";
                        cmd.Parameters.AddWithValue("$e", evtId);
                        cmd.Parameters.AddWithValue("$u", Session.UtilisateurId);
                        if (Convert.ToInt32(cmd.ExecuteScalar()) == 0)
                        {
                            MessageBox.Show("Vous devez être inscrit à cet événement pour le noter.");
                            return;
                        }
                    }
                    using (var cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = "INSERT INTO Commentaires (EvenementId, UtilisateurId, Note, Texte) VALUES ($e, $u, $n, $t)";
                        cmd.Parameters.AddWithValue("$e", evtId);
                        cmd.Parameters.AddWithValue("$u", Session.UtilisateurId);
                        cmd.Parameters.AddWithValue("$n", (int)nudNote.Value);
                        cmd.Parameters.AddWithValue("$t", InputValidator.Sanitize(txtCommentaire.Text));
                        cmd.ExecuteNonQuery();
                    }
                    MessageBox.Show("Avis enregistré.", "Succès");
                    txtCommentaire.Clear();
                    ChargerEvenementsPourAvis(dgv);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Erreur : " + ex.Message);
                }
            };
            panel.Controls.Add(btnEnregistrer);
            split.Panel2.Controls.Add(panel);
            tab.Controls.Add(split);
            ChargerEvenementsPourAvis(dgv);
        }

        private void ChargerEvenementsPourAvis(DataGridView dgv)
        {
            try
            {
                using var conn = DbHelper.OuvrirConnexion();
                string sql = @"
SELECT e.Id, e.Titre, e.DateDebut
FROM Inscriptions i
JOIN Evenements e ON e.Id = i.EvenementId
WHERE i.UtilisateurId = $uid
ORDER BY e.DateDebut DESC";
                using var cmd = new SqliteCommand(sql, conn);
                cmd.Parameters.AddWithValue("$uid", Session.UtilisateurId);
                var dt = new DataTable();
                using var r = cmd.ExecuteReader();
                dt.Load(r);
                dgv.DataSource = dt;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erreur : " + ex.Message);
            }
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
    }
}
