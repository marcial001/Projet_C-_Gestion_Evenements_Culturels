using GestionEvenements.Data;
using GestionEvenements.Utilities;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace GestionEvenements
{
    public partial class FrmLogin : Form
    {
        private const int FieldWidth = 220;
        private const int FieldHeight = 28;
        private const int LabelWidth = 100;
        private const int LeftMargin = 50;
        private const int TopMargin = 20;
        private const int RowSpacing = 42;

        private TextBox txtLogin;
        private TextBox txtMdp;
        private Panel pnlPassword;
        private Button btnTogglePassword;
        private Label lblErreur;

        public FrmLogin()
        {
            InitializeComponent();
            BuildUI();
        }

        private void InitializeComponent()
        {
        }

        private void BuildUI()
        {
            this.Text = "Connexion - Gestion Événements Culturels";
            this.Size = new Size(420, 320);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.FromArgb(30, 30, 45);
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimumSize = new Size(400, 300);

            // Titre
            var lblTitre = new Label
            {
                Text = "Bienvenue",
                Font = new Font("Segoe UI", 16, FontStyle.Bold),
                ForeColor = Color.White,
                AutoSize = true,
                Location = new Point(LeftMargin, TopMargin)
            };
            this.Controls.Add(lblTitre);

            // Ligne Nom Utilisateur
            int y = TopMargin + 50;
            var lblLogin = new Label
            {
                Text = "Nom Utilisateur :",
                ForeColor = Color.White,
                Location = new Point(LeftMargin, y + 4),
                Size = new Size(LabelWidth, 20)
            };
            txtLogin = new TextBox
            {
                Location = new Point(LeftMargin + LabelWidth, y),
                Size = new Size(FieldWidth, FieldHeight),
                Font = new Font("Segoe UI", 10),
                MaxLength = 50,
                MinimumSize = new Size(150, FieldHeight)
            };
            this.Controls.Add(lblLogin);
            this.Controls.Add(txtLogin);

            // Ligne Mot de passe avec icône
            y += RowSpacing;
            var lblMdp = new Label
            {
                Text = "Mot de passe :",
                ForeColor = Color.White,
                Location = new Point(LeftMargin, y + 4),
                Size = new Size(LabelWidth, 20)
            };
            pnlPassword = new Panel
            {
                Location = new Point(LeftMargin + LabelWidth, y),
                Size = new Size(FieldWidth, FieldHeight),
                BorderStyle = BorderStyle.FixedSingle,
                BackColor = Color.White,
                MinimumSize = new Size(150, FieldHeight)
            };
            txtMdp = new TextBox
            {
                Location = new Point(4, 2),
                Size = new Size(FieldWidth - 40, FieldHeight - 4),
                BorderStyle = BorderStyle.None,
                Font = new Font("Segoe UI", 10),
                PasswordChar = '●',
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };
            txtMdp.Width = Math.Max(120, pnlPassword.Width - 40);
            btnTogglePassword = new Button
            {
                Text = "👁",
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.White,
                ForeColor = Color.Gray,
                Size = new Size(32, FieldHeight - 4),
                Location = new Point(pnlPassword.Width - 34, 2),
                Anchor = AnchorStyles.Top | AnchorStyles.Right,
                Cursor = Cursors.Hand
            };
            btnTogglePassword.FlatAppearance.BorderSize = 0;
            btnTogglePassword.Click += BtnTogglePassword_Click;
            pnlPassword.Controls.Add(txtMdp);
            pnlPassword.Controls.Add(btnTogglePassword);
            this.Controls.Add(lblMdp);
            this.Controls.Add(pnlPassword);

            // Bouton connexion (même largeur que les champs)
            y += RowSpacing + 10;
            var btnConn = new Button
            {
                Text = "Se connecter",
                BackColor = Color.DodgerBlue,
                ForeColor = Color.White,
                Location = new Point(LeftMargin + LabelWidth, y),
                Size = new Size(FieldWidth, 36),
                FlatStyle = FlatStyle.Flat
            };
            btnConn.Click += BtnConn_Click;
            this.Controls.Add(btnConn);

            // Message d'erreur
            y += 50;
            lblErreur = new Label
            {
                ForeColor = Color.OrangeRed,
                Location = new Point(LeftMargin, y),
                Size = new Size(FieldWidth + LabelWidth, 40),
                Visible = false,
                AutoSize = false,
                MaximumSize = new Size(350, 0),
                AutoEllipsis = true
            };
            this.Controls.Add(lblErreur);

            pnlPassword.Resize += (s, e) =>
            {
                if (btnTogglePassword != null && txtMdp != null)
                {
                    int btnW = 34;
                    btnTogglePassword.Left = Math.Max(0, pnlPassword.Width - btnW);
                    txtMdp.Width = Math.Max(100, pnlPassword.Width - btnW - 4);
                }
            };
        }

        private void BtnTogglePassword_Click(object sender, EventArgs e)
        {
            if (txtMdp.PasswordChar == '●')
            {
                txtMdp.PasswordChar = '\0';
                btnTogglePassword.Text = "🙈";
            }
            else
            {
                txtMdp.PasswordChar = '●';
                btnTogglePassword.Text = "👁";
            }
        }

        private void BtnConn_Click(object sender, EventArgs e)
        {
            lblErreur.Visible = false;
            string login = InputValidator.Sanitize(txtLogin.Text);
            string mdp = txtMdp.Text ?? "";

            // Cas 1 : Validation échouée (caractères ou format non respectés)
            var (okLogin, msgLogin) = InputValidator.ValiderLogin(login);
            if (!okLogin)
            {
                MessageBox.Show(msgLogin, "Saisie invalide", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var (okMdp, msgMdp) = InputValidator.ValiderMotDePasse(mdp);
            if (!okMdp)
            {
                MessageBox.Show(msgMdp, "Saisie invalide", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                Connexion(login, mdp);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erreur technique : " + ex.Message, "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void AfficherErreur(string msg)
        {
            lblErreur.Text = msg;
            lblErreur.Visible = true;
        }

        private void Connexion(string login, string mdp)
        {
            using var conn = DbHelper.OuvrirConnexion();

            using var cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT Id, Nom, Role FROM Utilisateurs WHERE Login = $login AND MotDePasse = $mdp AND (Actif IS NULL OR Actif = 1)";
            cmd.Parameters.AddWithValue("$login", login);
            cmd.Parameters.AddWithValue("$mdp", mdp);

            using var reader = cmd.ExecuteReader();
            if (reader.Read())
            {
                Session.UtilisateurId = reader.GetInt32(0);
                Session.Nom = reader.GetString(1);
                Session.Role = reader.GetString(2);

                this.Hide();
                var dash = new FrmDashboard(this);
                dash.FormClosed += (s, args) => { this.Show(); };
                dash.ShowDialog();
                dash.Dispose();
            }
            else
            {
                // Cas 2 : Saisie valide mais utilisateur n'existe pas
                MessageBox.Show("Aucun compte ne correspond à ces identifiants.\nVérifiez votre nom utilisateur et mot de passe.", "Identifiants incorrects", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }
    }
}
