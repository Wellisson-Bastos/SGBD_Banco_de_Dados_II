using System;
using System.Windows.Forms;

namespace Trabalho_Cadona
{
    static class Program
    {
        public static string PathBanco = @"C:\temp\ArquivoBanco.txt";
        public static string PathLog = @"C:\temp\LogBanco.txt";
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Inicio());
        }
    }
}
