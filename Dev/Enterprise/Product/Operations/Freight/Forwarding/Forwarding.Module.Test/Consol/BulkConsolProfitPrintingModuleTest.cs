using System.Windows.Forms;
using CargoWise.ComponentModel;
using Enterprise.Accounting.Integration.Testing;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Freight.Forwarding.Module.Testing
{
	internal sealed class BulkConsolProfitPrintingModuleTest : BaseBulkJobProfitPrintingModuleTest
	{
		#region Implementation

		protected override void FindAndClickMenuItem(IMenuItem testMenuItem)
		{
			using (ZFilterGridModule module =
					(ZFilterGridModule)ZModuleFactory.Instance.Create(ModuleIDs.JobConsol))
			{
				MenuItem post = module.FormActionMenu.FindByText("&Actions").MenuItems.FindByText("&Post");
				foreach (MenuItem childMenuItem in post.MenuItems)
				{
					if (childMenuItem == testMenuItem)
					{
						childMenuItem.PerformClick();
						break;
					}
				}
			}
		}

		protected override bool UseImplementationForConsol => true;

		#endregion
	}
}
