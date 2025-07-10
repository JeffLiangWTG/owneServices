using System.Windows.Forms;
using CargoWise.ComponentModel;
using CargoWise.Windows.UI;
using Enterprise.Accounting.Integration.Testing;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.Module.Testing
{
	sealed class BulkJobProfitPrintingModuleTest : BaseBulkJobProfitPrintingModuleTest
	{
		protected override void FindAndClickMenuItem(IMenuItem testMenuItem)
		{
			using (ZFilterGridModule module = (ZFilterGridModule)ZModuleFactory.Instance.Create(ModuleIDs.Customs.JobDeclaration))
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
