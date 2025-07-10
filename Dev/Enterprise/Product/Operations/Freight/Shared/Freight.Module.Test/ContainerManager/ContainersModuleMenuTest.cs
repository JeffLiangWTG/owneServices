using System;
using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.DataTransfer.GUI;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Module.Testing
{
	public class ContainersModuleMenuTest : TestCaseWithFactory
	{
		public void TestImportFromXmlButton()
		{
			var tomorrow = new InterfaceConnectorTemporarilyEnabledUntil() { EnabledUntil = ZDateTime.Now.Date.AddDays(1) };
			eHubMessagingRegistry.Instance.InterfaceConnectorTemporarilyEnabledUntilItem.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, tomorrow);

			using (ContainersModule module = new ContainersModule())
			{
				AssertNotNull(module.FormActionMenu.FindByText("Import From &XML", true));

				EventHandler handler = module.ImportMenuItemsForTest["Import From &XML"];
				AssertNotNull("Should find the button on the module", handler);

				handler(null, null);
				AssertEquals(typeof(XmlDataImporterForm), ZFormModaliser.LastFormShownDialogForTest.GetType());
			}
		}

		public void TestStandardMenuItems()
		{
			GlbCompany australiaCompany = Factory.LoadFromUniqueKey<GlbCompany>(GlbCompanySchema.GC_Code, new ZString("EDI"));
			GlbCompany singaporeCompany = Factory.LoadFromUniqueKey<GlbCompany>(GlbCompanySchema.GC_Code, new ZString("SIN"));

			using (singaporeCompany.Branches[0].SetAsTemporaryContext())
			using (ContainersModule module = new ContainersModule())
			{
				MenuItem[] standardMenuItems = module.GetNewStandardMenuItemsForTest();
				foreach (MenuItem menuItem in standardMenuItems)
				{
					AssertNotEquals("Menu Item Text", menuItem.Text, "Pay Storage Charges");
				}
			}

			bool storageChargesMenuItemIsFound = false;

			using (australiaCompany.Branches[0].SetAsTemporaryContext())
			using (ContainersModule module = new ContainersModule())
			{
				MenuItem[] menuItemsForAustralia = module.GetNewStandardMenuItemsForTest();
				foreach (MenuItem menuItem in menuItemsForAustralia)
				{
					if (menuItem.Text == "Pay Storage Charges")
					{
						storageChargesMenuItemIsFound = true;
					}
				}

				AssertEquals("Pay Storage Charges should be shown for Australian Companies", true, storageChargesMenuItemIsFound);
			}
		}
	}
}
