using System.Windows.Forms;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;

namespace Enterprise.MasterFiles.Module.Testing
{
	public abstract class OrgSupplierPartModuleTest : ZModuleBasherTest
	{
		protected override ModuleIdentifier GetModuleID()
		{
			return ModuleIDs.SupplierPart;
		}

		protected override string CountryCode => "ER";

		public void TestMenuItems()
		{
			var menuHasProductImportItem = false;
			var menuHasLastCostImportItem = false;

			using (OrgSupplierPartModule orgSupplierPartModule = new OrgSupplierPartModuleHelper())
			{
				foreach (var tool in orgSupplierPartModule.ToolBarButtons)
				{
					if (tool.Text.Equals("Actions"))
					{
						foreach (MenuItem actionItem in tool.DropDownMenu.MenuItems)
						{
							if (actionItem.Text.Equals("D&ata Transfer"))
							{
								foreach (MenuItem item in actionItem.MenuItems)
								{
									if (item.Text.Equals("Import From CSV"))
									{
										menuHasProductImportItem = true;
									}
									else if (item.Text.Equals("Import Last Cost From CSV"))
									{
										menuHasLastCostImportItem = true;
									}
								}
							}
						}
					}

					if (menuHasProductImportItem && menuHasLastCostImportItem)
					{
						break;
					}
				}
			}

			Assert("Import from CSV menu item missing", menuHasProductImportItem);
			Assert("Import Last Cost from CSV menu item misssing", menuHasLastCostImportItem);
		}

		public class OrgSupplierPartModuleHelper : OrgSupplierPartModule
		{
		}
	}
}
