using Microsoft.Owin.Hosting;
using System;
using System.ComponentModel;
using System.Windows.Forms;

namespace CargoWise.eHub.Products.TWCustoms.TWCServiceSampleClient
{
	static class Program
    {
        static string baseAddress = "http://*:9000";
        public static frmMain mainForm;


        [STAThread]
        static void Main()
        {

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            mainForm = new frmMain();
            mainForm.Show();

            var serviceWorker = new BackgroundWorker();
            serviceWorker.DoWork += ServiceWorker_DoWork;
            serviceWorker.RunWorkerAsync();

            Application.Run(mainForm);


        }

        private static void ServiceWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            using (var app = WebApp.Start<ServiceStartup>(url: baseAddress))
            {
                mainForm.Invoke((MethodInvoker)delegate
                {
                    mainForm.Text = "Listening " + baseAddress;
                });
                while (mainForm.Created)
                {
                    System.Threading.Thread.Sleep(0);
                }
            }
            if (mainForm.Created)
            {
                mainForm.Invoke((MethodInvoker)delegate
                {
                    mainForm.Text = "Not Listening.";
                });
            }
        }
    }
}
