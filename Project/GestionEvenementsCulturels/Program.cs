using System;
using System.Windows.Forms;
using GestionEvenements.Data;

namespace GestionEvenements
{
    internal static class Program
    {
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            Application.ThreadException += (s, e) =>
            {
                MessageBox.Show("Erreur : " + e.Exception?.Message, "Erreur inattendue", MessageBoxButtons.OK, MessageBoxIcon.Error);
            };
            AppDomain.CurrentDomain.UnhandledException += (s, e) =>
            {
                var ex = e.ExceptionObject as Exception;
                MessageBox.Show("Erreur critique : " + (ex?.Message ?? "Inconnue"), "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error);
            };

            try
            {
                DbHelper.InitialiserBase();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Impossible d'initialiser la base : " + ex.Message, "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            Application.Run(new FrmLogin());
        }
    }
}