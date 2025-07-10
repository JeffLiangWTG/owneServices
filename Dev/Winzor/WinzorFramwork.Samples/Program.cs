using System;
using System.Windows.Forms;

namespace WinzorFramework.Samples
{
	static class Program
	{
		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		[STAThread]
		static void Main()
		{
#if !WINZOR
			Application.EnableVisualStyles();
			Application.SetCompatibleTextRenderingDefault(false);
			Application.Run(new MainForm());
#else
			AssemblyResolver.Setup();
			var app = new ApplicationServer();
			app.Run<Home>(() =>
			{
				return new MainForm();
			});
#endif
		}
	}
}
