using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.US.eManifest.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.US.eManifest.GUI.Testing
{
	public class AllEquipmentUserControlTest : TestCaseWithFactory
	{
		public void TestEquipmentGrid_ContextMenuItemPopulateCommodityEquipment_DefaultValueWillBeFilledInCommodities()
		{
			var trip = Factory.New<Trip>();
			trip.BH_GB = Factory.NewWithValidTestData<GlbBranch>().PK;
			trip.BH_OA_Importer = Factory.NewWithValidTestData<OrgAddress>().PK;

			var shipment = trip.Shipments.AddNew();
			var commodity = Factory.NewWithValidTestData<Commodity>();
			shipment.Commodities.Add(commodity);

			Assert("Commodity has not been given a default equiment value", commodity.BY_BJ_Equipment.IsEmpty);

			using (var form = new ZForm(trip))
			{
				using (var control = new AllEquipmentUserControl())
				{
					form.Controls.Add(control);
					form.Show();
					var equipmentGrid = (ZGrid)control.Controls.Find("EquipmentGrid", true).FirstOrDefault();
					AssertNotNull("Equipment Grid", equipmentGrid);
					if (equipmentGrid != null)
					{
						var equipment = Factory.NewWithValidTestData<Equipment>();
						trip.Equipment.Add(equipment);
						equipmentGrid.ContextMenu.MenuItems.FindByText("Add Equipment to all Commodities").PerformClick();
						Assert("Commodity has been set a default equiment value", !commodity.BY_BJ_Equipment.IsEmpty);
						AssertEquals("Equipment of the commodity has been set to equiment PK", commodity.BY_BJ_Equipment, equipment.PK);
					}
				}
			}
		}
	}
}
