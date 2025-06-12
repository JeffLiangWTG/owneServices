using System.ComponentModel;
using System.Configuration.Install;

namespace CargoWise.eServices.Billing.Collector.eHub.WindowsService
{
	[RunInstaller(true)]
	public partial class ProjectInstaller : Installer
	{
		public ProjectInstaller()
		{
			InitializeComponent();
		}
	}
}