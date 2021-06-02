using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace InfrasysDataImporter
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        /// 
        static Mutex mutex = new Mutex(true, "{00012233-4455-6678-8899-aabbccyyhhkt}");
        [STAThread]
        static void Main()
        {
            if (mutex.WaitOne(TimeSpan.Zero, true))
            {
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                Application.Run(new DataImporter());
                mutex.ReleaseMutex();
            }
            else
            {
                MessageBox.Show("This application is already running!",
                    "Instance detected.",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Asterisk);
            }
        }
    }
}
