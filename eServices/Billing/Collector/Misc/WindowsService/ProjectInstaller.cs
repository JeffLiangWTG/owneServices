using System.ComponentModel;
using System.Configuration.Install;

namespace CargoWise.eServices.Billing.Collector.Misc.WindowsService
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