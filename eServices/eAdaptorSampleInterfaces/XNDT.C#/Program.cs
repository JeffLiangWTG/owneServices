namespace WTG.XNDT.SampleClient
{
	using System;
	using System.Windows.Forms;

	internal static class Program
	{
		[STAThread]
		private static void Main()
		{
			Application.EnableVisualStyles();
			Application.SetCompatibleTextRenderingDefault(false);
			SampleClientForm mainForm = new SampleClientForm();
			Application.Run(mainForm);
		}
	}
}

