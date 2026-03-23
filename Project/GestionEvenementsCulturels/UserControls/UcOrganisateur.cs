using GestionEvenements.Data;
using GestionEvenements.Utilities;
using Microsoft.Data.Sqlite;
using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;

namespace GestionEvenements.UserControls
{
    public class UcOrganisateur : UserControl
    {
        private TabControl _tabControl;
        private DataGridView _dgvEvenements;
        private DataGridView _dgvInscriptions;
        private Label _lblStats;
        private TextBox _txtTitre, _txtDesc, _txtLieu;
        private DateTimePicker _dtpDebut, _dtpFin;
        private NumericUpDown _nudCapacite, _nudPrixStd, _nudPrixVip;
        private ComboBox _cboCategorie;
        private Label _lblAlerte;
        private PieChartPanel _chartTypesBillets;
        private PieChartPanel _chartStatutsEvenements;

        public UcOrganisateur()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.Dock = DockStyle.Fill;
            this.SuspendLayout();
            _tabControl = new TabControl { Dock = DockStyle.Fill };

            var tabEvts = new TabPage("Mes événements");
            var tabInscrits = new TabPage("Inscriptions");
            var tabStats = new TabPage("Statistiques");

            BuildOngletEvenements(tabEvts);
            BuildOngletInscriptions(tabInscrits);
            BuildOngletStatistiques(tabStats);

            _tabControl.TabPages.AddRange(new[] { tabEvts, tabInscrits, tabStats });
            this.Controls.Add(_tabControl);
            this.ResumeLayout(false);
            this.Load += UcOrganisateur_Load;
        }

        private void UcOrganisateur_Load(object sender, EventArgs e)
        {
            if (this.IsDisposed) return;
            BeginInvoke(new Action(() =>
            {
                if (!this.IsDisposed && this.IsHandleCreated)
                    ChargerDonnees();
            }));
        }

        private void ChargerDonnees()
        {
            try
            {
                ChargerCategories(_cboCategorie);
                ChargerEvenements();
                if (_dgvInscriptions != null) ChargerInscriptions(_dgvInscriptions);
                if (_lblStats != null) ChargerStatistiques(_lblStats);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erreur au chargement : " + ex.Message, "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void BuildOngletEvenements(TabPage tab)
        {
            var split = new SplitContainer
            {
                Dock = DockStyle.Fill,
                Orientation = Orientation.Horizontal,
                Panel1MinSize = 100,
                Panel2MinSize = 120
            };
            split.Resize += (s, ev) =>
            {
                if (split.Height > (split.Panel1MinSize + split.Panel2MinSize + split.SplitterWidth))
                {
                    int maxDist = split.Height - split.Panel2MinSize - split.SplitterWidth;
                    if (split.SplitterDistance > maxDist)
                        split.SplitterDistance = maxDist;
                    if (split.SplitterDistance < split.Panel1MinSize)
                        split.SplitterDistance = split.Panel1MinSize;
                }
            };

            _dgvEvenements = new DataGridView
            {
                Dock = DockStyle.Fill,
                ReadOnly = true,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                RowHeadersVisible = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                AutoGenerateColumns = false
            };
            StyliserGrille(_dgvEvenements);
            ConfigurerColonnesEvenements(_dgvEvenements);
            _dgvEvenements.DataError += (s, ev) => { ev.ThrowException = false; ev.Cancel = true; };
            split.Panel1.Controls.Add(_dgvEvenements);

            var panelForm = new Panel { Dock = DockStyle.Fill, Padding = new Padding(12), AutoScroll = true };

            int y = 8;
            const int wLabel = 100;
            const int wField = 280;
            const int lineH = 32;

            AjouterChamp(panelForm, "Titre *", ref y, wLabel, wField, lineH, out _txtTitre);
            _txtTitre.MaxLength = 200;

            AjouterChampMultiligne(panelForm, "Description", ref y, wLabel, wField, 3, out _txtDesc);
            _txtDesc.MaxLength = 2000;

            AjouterChamp(panelForm, "Lieu", ref y, wLabel, wField, lineH, out _txtLieu);
            _txtLieu.MaxLength = 200;

            AjouterCombobox(panelForm, "Catégorie", ref y, wLabel, wField, lineH, out _cboCategorie);

            var lblDebut = new Label { Text = "Date début *", Location = new Point(12, y), Size = new Size(wLabel, lineH) };
            _dtpDebut = new DateTimePicker
            {
                Location = new Point(wLabel + 20, y - 2),
                Width = wField,
                Format = DateTimePickerFormat.Custom,
                CustomFormat = "dd/MM/yyyy HH:mm",
                ShowUpDown = false
            };
            panelForm.Controls.Add(lblDebut); panelForm.Controls.Add(_dtpDebut);
            y += lineH + 8;

            var lblFin = new Label { Text = "Date fin", Location = new Point(12, y), Size = new Size(wLabel, lineH) };
            _dtpFin = new DateTimePicker
            {
                Location = new Point(wLabel + 20, y - 2),
                Width = wField,
                Format = DateTimePickerFormat.Custom,
                CustomFormat = "dd/MM/yyyy HH:mm",
                ShowCheckBox = true
            };
            panelForm.Controls.Add(lblFin); panelForm.Controls.Add(_dtpFin);
            y += lineH + 8;

            var lblCap = new Label { Text = "Capacité", Location = new Point(12, y), Size = new Size(wLabel, lineH) };
            _nudCapacite = new NumericUpDown
            {
                Location = new Point(wLabel + 20, y - 2),
                Width = 120,
                Minimum = 0,
                Maximum = 100000,
                Value = 100
            };
            panelForm.Controls.Add(lblCap); panelForm.Controls.Add(_nudCapacite);
            y += lineH + 8;

            var lblPrixStd = new Label { Text = "Prix Standard", Location = new Point(12, y), Size = new Size(wLabel, lineH) };
            _nudPrixStd = new NumericUpDown
            {
                Location = new Point(wLabel + 20, y - 2),
                Width = 120,
                Minimum = 0,
                Maximum = 99999,
                DecimalPlaces = 2,
                Value = 0
            };
            panelForm.Controls.Add(lblPrixStd); panelForm.Controls.Add(_nudPrixStd);
            y += lineH + 8;

            var lblPrixVip = new Label { Text = "Prix VIP", Location = new Point(12, y), Size = new Size(wLabel, lineH) };
            _nudPrixVip = new NumericUpDown
            {
                Location = new Point(wLabel + 20, y - 2),
                Width = 120,
                Minimum = 0,
                Maximum = 99999,
                DecimalPlaces = 2,
                Value = 0
            };
            panelForm.Controls.Add(lblPrixVip); panelForm.Controls.Add(_nudPrixVip);
            y += lineH + 8;

            y += 16;
            var btnAjouter = new Button
            {
                Text = "Créer l'événement",
                BackColor = Color.ForestGreen,
                ForeColor = Color.White,
                Location = new Point(wLabel + 20, y),
                Size = new Size(160, 36),
                FlatStyle = FlatStyle.Flat
            };
            btnAjouter.Click += BtnAjouter_Click;
            panelForm.Controls.Add(btnAjouter);

            var btnSupprimer = new Button
            {
                Text = "Supprimer",
                BackColor = Color.OrangeRed,
                ForeColor = Color.White,
                Location = new Point(wLabel + 200, y),
                Size = new Size(100, 36),
                FlatStyle = FlatStyle.Flat
            };
            btnSupprimer.Click += BtnSupprimer_Click;
            panelForm.Controls.Add(btnSupprimer);

            y += 50;
            _lblAlerte = new Label { ForeColor = Color.OrangeRed, Location = new Point(12, y), AutoSize = true, MaximumSize = new Size(500, 0) };
            panelForm.Controls.Add(_lblAlerte);

            split.Panel2.Controls.Add(panelForm);
            tab.Controls.Add(split);
        }

        private void BuildOngletInscriptions(TabPage tab)
        {
            _dgvInscriptions = new DataGridView
            {
                Dock = DockStyle.Fill,
                ReadOnly = true,
                AllowUserToAddRows = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect
            };
            StyliserGrille(_dgvInscriptions);
            tab.Controls.Add(_dgvInscriptions);
        }

        private void BuildOngletStatistiques(TabPage tab)
        {
            var panel = new Panel { Dock = DockStyle.Fill, Padding = new Padding(20), AutoScroll = true };
            _lblStats = new Label
            {
                AutoSize = true,
                Location = new Point(20, 20),
                MaximumSize = new Size(700, 0),
                Font = new Font("Segoe UI", 11)
            };
            _chartTypesBillets = CreerPieChart("Inscriptions par type billet", new Point(20, 130), new Size(420, 260));
            _chartStatutsEvenements = CreerPieChart("Mes événements par statut", new Point(470, 130), new Size(420, 260));
            panel.Controls.Add(_lblStats);
            panel.Controls.Add(_chartTypesBillets);
            panel.Controls.Add(_chartStatutsEvenements);
            tab.Controls.Add(panel);
        }

        private void AjouterChamp(Panel p, string labelText, ref int y, int wL, int wF, int h, out TextBox txt)
        {
            var lbl = new Label { Text = labelText, Location = new Point(12, y), Size = new Size(wL, h) };
            txt = new TextBox
            {
                Location = new Point(wL + 20, y - 2),
                Width = wF,
                MinimumSize = new Size(120, 22),
                Font = new Font("Segoe UI", 10)
            };
            p.Controls.Add(lbl); p.Controls.Add(txt);
            y += h + 8;
        }

        private void AjouterChampMultiligne(Panel p, string labelText, ref int y, int wL, int wF, int lines, out TextBox txt)
        {
            var lbl = new Label { Text = labelText, Location = new Point(12, y), Size = new Size(wL, 22) };
            int h = lines * 22;
            txt = new TextBox
            {
                Location = new Point(wL + 20, y),
                Width = wF,
                Height = h,
                MinimumSize = new Size(120, 44),
                Multiline = true,
                ScrollBars = ScrollBars.Vertical,
                Font = new Font("Segoe UI", 10)
            };
            p.Controls.Add(lbl); p.Controls.Add(txt);
            y += h + 12;
        }

        private void AjouterCombobox(Panel p, string labelText, ref int y, int wL, int wF, int h, out ComboBox cbo)
        {
            var lbl = new Label { Text = labelText, Location = new Point(12, y), Size = new Size(wL, h) };
            cbo = new ComboBox
            {
                Location = new Point(wL + 20, y - 2),
                Width = wF,
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            p.Controls.Add(lbl); p.Controls.Add(cbo);
            y += h + 8;
        }

        private void ChargerCategories(ComboBox cbo)
        {
            try
            {
                cbo.Items.Clear();
                using var conn = DbHelper.OuvrirConnexion();
                using var cmd = new SqliteCommand("SELECT Id, Libelle FROM Categories ORDER BY Libelle", conn);
                using var r = cmd.ExecuteReader();
                cbo.Items.Add(new ComboItem(0, "(Aucune)"));
                while (r.Read())
                    cbo.Items.Add(new ComboItem(r.GetInt32(0), r.GetString(1)));
                cbo.DisplayMember = "Text";
                cbo.ValueMember = "Id";
                if (cbo.Items.Count > 0) cbo.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                AfficherErreur("Erreur chargement catégories: " + ex.Message);
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

        private static void ConfigurerColonnesEvenements(DataGridView dgv)
        {
            dgv.Columns.Clear();
            void Add(string prop, string header)
            {
                dgv.Columns.Add(new DataGridViewTextBoxColumn
                {
                    Name = prop,
                    DataPropertyName = prop,
                    HeaderText = header,
                    MinimumWidth = 50,
                    FillWeight = 80
                });
            }
            Add("Id", "Id");
            Add("Titre", "Titre");
            Add("DateDebut", "Début");
            Add("Lieu", "Lieu");
            Add("Capacite", "Capacité");
            Add("Statut", "Statut");
            Add("Categorie", "Catégorie");
            Add("Inscrits", "Inscrits");
        }

        private void ChargerEvenements()
        {
            try
            {
                using var conn = DbHelper.OuvrirConnexion();
                string sql = @"
SELECT e.Id, e.Titre, e.DateDebut, e.Lieu, e.Capacite, e.Statut,
       COALESCE(c.Libelle,'-') as Categorie,
       COUNT(i.Id) as Inscrits
FROM Evenements e
LEFT JOIN Categories c ON e.CategorieId = c.Id
LEFT JOIN Inscriptions i ON e.Id = i.EvenementId
WHERE e.CreeParId = $uid
GROUP BY e.Id, e.Titre, e.DateDebut, e.Lieu, e.Capacite, e.Statut
ORDER BY e.DateDebut";
                using var cmd = new SqliteCommand(sql, conn);
                cmd.Parameters.AddWithValue("$uid", Session.UtilisateurId);
                var dt = new DataTable();
                using var r = cmd.ExecuteReader();
                dt.Load(r);
                _dgvEvenements.DataSource = dt;
                VerifierAlertes(dt);
            }
            catch (Exception ex)
            {
                AfficherErreur("Erreur chargement: " + ex.Message);
            }
        }

        private void VerifierAlertes(DataTable dt)
        {
            _lblAlerte.Text = "";
            if (dt == null || dt.Rows.Count == 0) return;
            var now = DateTime.Now;
            foreach (DataRow row in dt.Rows)
            {
                var v = row["DateDebut"];
                if (v == null || v == DBNull.Value) continue;
                if (!DateTime.TryParse(v.ToString(), out var debut)) continue;
                if (debut > now && (debut - now).TotalHours <= 24)
                {
                    _lblAlerte.Text = $"⚠ Événement proche : {row["Titre"]} le {debut:dd/MM/yyyy HH:mm}";
                    return;
                }
            }
        }

        private void BtnAjouter_Click(object sender, EventArgs e)
        {
            _lblAlerte.Text = "";
            var (ok, msg) = InputValidator.ValiderTitreEvenement(_txtTitre.Text);
            if (!ok) { AfficherErreur(msg); return; }
            InputValidator.ValiderDescription(_txtDesc.Text);
            InputValidator.ValiderLieu(_txtLieu.Text);
            var (okCap, msgCap) = InputValidator.ValiderCapacite((int)_nudCapacite.Value);
            if (!okCap) { AfficherErreur(msgCap); return; }
            var (okPrixStd, _) = InputValidator.ValiderPrix((decimal)_nudPrixStd.Value);
            var (okPrixVip, _) = InputValidator.ValiderPrix((decimal)_nudPrixVip.Value);
            if (!okPrixStd || !okPrixVip) { AfficherErreur("Prix invalide."); return; }

            try
            {
                int catId = (_cboCategorie.SelectedItem as ComboItem)?.Id ?? 0;
                using (var conn = DbHelper.OuvrirConnexion())
                {
                    using var cmd = conn.CreateCommand();
                    cmd.CommandText = @"
INSERT INTO Evenements (Titre, Description, DateDebut, DateFin, Lieu, Capacite, CreeParId, CategorieId, PrixStandard, PrixVIP, Statut)
VALUES ($titre, $desc, $debut, $fin, $lieu, $cap, $creePar, $cat, $pStd, $pVip, 'EnAttente')";
                    cmd.Parameters.AddWithValue("$titre", InputValidator.Sanitize(_txtTitre.Text));
                    cmd.Parameters.AddWithValue("$desc", InputValidator.Sanitize(_txtDesc.Text));
                    cmd.Parameters.AddWithValue("$debut", _dtpDebut.Value.ToString("yyyy-MM-dd HH:mm"));
                    cmd.Parameters.AddWithValue("$fin", _dtpFin.Checked ? _dtpFin.Value.ToString("yyyy-MM-dd HH:mm") : (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("$lieu", InputValidator.Sanitize(_txtLieu.Text));
                    cmd.Parameters.AddWithValue("$cap", (int)_nudCapacite.Value);
                    cmd.Parameters.AddWithValue("$creePar", Session.UtilisateurId);
                    cmd.Parameters.AddWithValue("$cat", catId > 0 ? catId : DBNull.Value);
                    cmd.Parameters.AddWithValue("$pStd", (decimal)_nudPrixStd.Value);
                    cmd.Parameters.AddWithValue("$pVip", (decimal)_nudPrixVip.Value);
                    cmd.ExecuteNonQuery();
                }

                MessageBox.Show("Événement créé avec succès.", "Succès");
                ReinitialiserFormulaire();
                ChargerEvenements();
            }
            catch (Exception ex)
            {
                AfficherErreur("Erreur : " + ex.Message);
            }
        }

        private void ReinitialiserFormulaire()
        {
            _txtTitre.Clear();
            _txtDesc.Clear();
            _txtLieu.Clear();
            _nudCapacite.Value = 100;
            _nudPrixStd.Value = 0;
            _nudPrixVip.Value = 0;
            if (_cboCategorie.Items.Count > 0) _cboCategorie.SelectedIndex = 0;
        }

        private void BtnSupprimer_Click(object sender, EventArgs e)
        {
            if (_dgvEvenements.SelectedRows.Count == 0)
            {
                MessageBox.Show("Sélectionnez un événement.", "Attention");
                return;
            }
            var cell = _dgvEvenements.SelectedRows[0].Cells["Id"];
            if (cell?.Value == null || !int.TryParse(cell.Value.ToString(), out int id))
            {
                MessageBox.Show("Sélection invalide.", "Erreur");
                return;
            }
            if (MessageBox.Show("Supprimer cet événement ?", "Confirmation", MessageBoxButtons.YesNo) != DialogResult.Yes)
                return;
            try
            {
                using var conn = DbHelper.OuvrirConnexion();
                using var cmd = conn.CreateCommand();
                cmd.CommandText = "DELETE FROM Inscriptions WHERE EvenementId = $id";
                cmd.Parameters.AddWithValue("$id", id);
                cmd.ExecuteNonQuery();
                cmd.CommandText = "DELETE FROM Evenements WHERE Id = $id AND CreeParId = $uid";
                cmd.Parameters.AddWithValue("$uid", Session.UtilisateurId);
                cmd.ExecuteNonQuery();
                ChargerEvenements();
            }
            catch (Exception ex)
            {
                AfficherErreur("Erreur : " + ex.Message);
            }
        }

        private void ChargerInscriptions(DataGridView dgv)
        {
            try
            {
                using var conn = DbHelper.OuvrirConnexion();
                string sql = @"
SELECT e.Titre, u.Nom, i.DateInscription, i.Statut, i.TypeBillet
FROM Inscriptions i
JOIN Evenements e ON e.Id = i.EvenementId
JOIN Utilisateurs u ON u.Id = i.UtilisateurId
WHERE e.CreeParId = $uid
ORDER BY i.DateInscription DESC";
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

        private void ChargerStatistiques(Label lbl)
        {
            try
            {
                using var conn = DbHelper.OuvrirConnexion();
                int nbEvts = 0, nbInsc = 0;
                decimal revenus = 0;
                using (var cmd = new SqliteCommand("SELECT COUNT(*) FROM Evenements WHERE CreeParId = $uid", conn))
                {
                    cmd.Parameters.AddWithValue("$uid", Session.UtilisateurId);
                    nbEvts = Convert.ToInt32(cmd.ExecuteScalar());
                }
                using (var cmd = new SqliteCommand(
                    "SELECT COUNT(*), COALESCE(SUM(i.PrixPaye),0) FROM Inscriptions i JOIN Evenements e ON e.Id = i.EvenementId WHERE e.CreeParId = $uid", conn))
                {
                    cmd.Parameters.AddWithValue("$uid", Session.UtilisateurId);
                    using var r = cmd.ExecuteReader();
                    if (r.Read()) { nbInsc = r.GetInt32(0); revenus = r.GetDecimal(1); }
                }
                lbl.Text = $"📊 Statistiques\n\n• Événements créés : {nbEvts}\n• Total inscriptions : {nbInsc}\n• Revenus (billets) : {revenus:N2} CFA";
                ChargerPieTypesBillets();
                ChargerPieStatutsEvenements();
            }
            catch (Exception ex)
            {
                lbl.Text = "Erreur : " + ex.Message;
            }
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

        private void ChargerPieTypesBillets()
        {
            if (_chartTypesBillets == null) return;
            var data = new System.Collections.Generic.Dictionary<string, int>();
            using var conn = DbHelper.OuvrirConnexion();
            using var cmd = new SqliteCommand(
                "SELECT COALESCE(i.TypeBillet,'-'), COUNT(*) FROM Inscriptions i JOIN Evenements e ON e.Id=i.EvenementId WHERE e.CreeParId=$uid GROUP BY i.TypeBillet",
                conn);
            cmd.Parameters.AddWithValue("$uid", Session.UtilisateurId);
            using var r = cmd.ExecuteReader();
            while (r.Read())
            {
                var type = r.IsDBNull(0) ? "-" : r.GetString(0);
                var count = r.IsDBNull(1) ? 0 : r.GetInt32(1);
                data[type] = count;
            }
            _chartTypesBillets.SetData(data);
        }

        private void ChargerPieStatutsEvenements()
        {
            if (_chartStatutsEvenements == null) return;
            var data = new System.Collections.Generic.Dictionary<string, int>();
            using var conn = DbHelper.OuvrirConnexion();
            using var cmd = new SqliteCommand(
                "SELECT COALESCE(Statut,'-'), COUNT(*) FROM Evenements WHERE CreeParId=$uid GROUP BY Statut",
                conn);
            cmd.Parameters.AddWithValue("$uid", Session.UtilisateurId);
            using var r = cmd.ExecuteReader();
            while (r.Read())
            {
                var statut = r.IsDBNull(0) ? "-" : r.GetString(0);
                var count = r.IsDBNull(1) ? 0 : r.GetInt32(1);
                data[statut] = count;
            }
            _chartStatutsEvenements.SetData(data);
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

        private void AfficherErreur(string msg)
        {
            _lblAlerte.Text = msg;
            _lblAlerte.Visible = true;
        }

        private class ComboItem
        {
            public int Id { get; }
            public string Text { get; }
            public ComboItem(int id, string text) { Id = id; Text = text; }
        }
    }
}
