using System.Windows.Forms;
using CargoWise.ComponentModel;
using Enterprise.Accounting.Integration.Testing;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Freight.CFS.Module.Testing
{
	sealed class ShipmentReceivalBulkJobProfitPrintingModuleTest : BaseBulkJobProfitPrintingModuleTest
	{
		protected override void FindAndClickMenuItem(IMenuItem testMenuItem)
		{
			using (ZFilterGridModule module =
					(ZFilterGridModule)ZModuleFactory.Instance.Create(ModuleIDs.ShipmentReceival))
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
	}
}
