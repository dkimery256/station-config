using System;
using System.Threading;
using System.Windows.Forms;

namespace Station_Configuration_Utility
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            bool result;
            var mutex = new Mutex(true, "UniqueAppId", out result);
            //Check to see if another instance of the application is running
            if (!result)
            {
                MessageBox.Show("Another instance is already running.", "Error!");
            }            
            else
            {
                GC.KeepAlive(mutex);
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                Application.Run(new StationConfigUtil());
            }
            Application.Exit();
        }
    }
}
