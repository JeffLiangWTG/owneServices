using System.Configuration.Install;
using System.ServiceProcess;

namespace CargoWise.eHub.Shared.ServiceTaskHost
{
	partial class ProjectInstaller
	{
		System.ComponentModel.IContainer components = null;

		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		void InitializeComponent()
		{
		}
	}
}