using System;
using System.Drawing;
using System.Windows.Forms;

namespace GestionEvenements
{
    public partial class FrmDashboard : Form
    {
        private readonly Form _parentLogin;
        private Panel _panelHeader;
        private Panel _panelContenu;
        private Label _lblBienvenue;

        public FrmDashboard(Form parentLogin = null)
        {
            _parentLogin = parentLogin;
            InitializeComponent();
            BuildUI();
        }

        private void InitializeComponent()
        {
        }

        private void BuildUI()
        {
            this.Text = $"Gestion Événements - {Session.Nom}";
            this.WindowState = FormWindowState.Maximized;
            this.BackColor = Color.FromArgb(240, 242, 245);
            this.MinimumSize = new Size(900, 600);

            const int headerHeight = 56;

            var tableLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                RowCount = 2,
                ColumnCount = 1,
                BackColor = Color.FromArgb(240, 242, 245)
            };
            tableLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, headerHeight));
            tableLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            tableLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));

            // Header : ligne 0, bien au-dessus du contenu
            _panelHeader = new Panel
            {
                Dock = DockStyle.Fill,
                Height = headerHeight,
                MinimumSize = new Size(0, headerHeight),
                BackColor = Color.FromArgb(0, 80, 140),
                Padding = new Padding(16, 0, 16, 0)
            };

            _lblBienvenue = new Label
            {
                Text = $"Connecté : {Session.Nom} ({Session.Role})",
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                ForeColor = Color.White,
                AutoSize = true,
                Location = new Point(20, (headerHeight - 24) / 2),
                Anchor = AnchorStyles.Top | AnchorStyles.Left
            };

            var btnDeconn = new Button
            {
                Text = "Déconnexion",
                BackColor = Color.FromArgb(180, 50, 50),
                ForeColor = Color.White,
                Size = new Size(120, 36),
                FlatStyle = FlatStyle.Flat,
                Anchor = AnchorStyles.Top | AnchorStyles.Right,
                Cursor = Cursors.Hand
            };
            btnDeconn.FlatAppearance.BorderSize = 0;
            btnDeconn.Click += BtnDeconn_Click;

            _panelHeader.Controls.Add(_lblBienvenue);
            _panelHeader.Controls.Add(btnDeconn);
            tableLayout.Controls.Add(_panelHeader, 0, 0);

            // Contenu : ligne 1, en dessous du header
            _panelContenu = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(8),
                BackColor = Color.FromArgb(240, 242, 245)
            };
            tableLayout.Controls.Add(_panelContenu, 0, 1);

            this.Controls.Add(tableLayout);

            void RepositionBtn()
            {
                btnDeconn.Location = new Point(_panelHeader.Width - btnDeconn.Width - 20, (headerHeight - btnDeconn.Height) / 2);
            }
            _panelHeader.Resize += (s, e) => RepositionBtn();
            this.Load += (s, e) => RepositionBtn();

            ChargerContenuSelonRole();
        }

        private void BtnDeconn_Click(object sender, EventArgs e)
        {
            Session.UtilisateurId = -1;
            Session.Nom = "";
            Session.Role = "";
            _parentLogin?.Show();
            this.Close();
        }

        private void ChargerContenuSelonRole()
        {
            _panelContenu.Controls.Clear();
            UserControl uc = null;

            switch (Session.Role?.ToLowerInvariant())
            {
                case "admin":
                    uc = new UserControls.UcAdmin();
                    break;
                case "organisateur":
                    uc = new UserControls.UcOrganisateur();
                    break;
                case "participant":
                case "etudiant":
                    uc = new UserControls.UcEtudiant();
                    break;
                default:
                    uc = new UserControls.UcEtudiant();
                    break;
            }

            if (uc != null)
            {
                uc.Dock = DockStyle.Fill;
                _panelContenu.Controls.Add(uc);
            }
        }
    }
}
