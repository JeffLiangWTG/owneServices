using System.Windows.Forms;
using CargoWise.ComponentModel;
using CargoWise.Windows.UI;
using Enterprise.Accounting.Integration.Testing;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.TransportConsignment.Module.Testing
{
	public class DtbBookingConsignmentPrintingModuleTest : BaseBulkJobProfitPrintingModuleTest
	{
		protected override void FindAndClickMenuItem(IMenuItem testMenuItem)
		{
			using (var module = new DtbBookingConsignmentModule())
			{
				var post = module.FormActionMenu.FindByText("&Actions").MenuItems.FindByText("&Post");
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
