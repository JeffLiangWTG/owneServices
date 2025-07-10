using System.ComponentModel;
using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Windows.UI;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Environment.CodeLists;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Environment.GUI.Testing
{
	class LocationFormTest : WhsGuiTestCaseWithFactory
	{
		#region Constructors

		public void TestConstructor()
		{
			var row = Factory.NewWithValidTestData<WhsRow>();
			using (var form = new LocationFormForTest(row))
			{
				AssertEquals("Posting not setup", true, form.GetSetupPostingCalled());
			}
		}

		#endregion

		#region ZForm Overloads

		public void TestOnLoad()
		{
			var row = Factory.NewWithValidTestData<WhsRow>();
			using (var form = new LocationFormForTest(row))
			{
				form.Show();
				var fileNewMenuItem = form.GetFileMenuItem().MenuItems.FindByName(ZFormMenuStrategy.FileNewMenuItemName);
				AssertEquals("NewAction not disabled", false, fileNewMenuItem.Enabled);
			}
		}

		#endregion

		#region TestFormCaption

		public void TestFormCaption()
		{
			using (var form = new LocationForm(Factory.NewWithValidTestData<WhsRow>()))
			{
				form.Show();
				AssertEquals("Locations", form.FormCaption);
			}
		}

		#endregion

		#region TestFirstPackingStationLocationPreSaveDialog
		public void TestFirstPackingStationLocationPreSaveDialog()
		{
			var whs1 = Helper.CreateWarehouse("WHS1");
			var location1 = Helper.CreateRowAndGenerateLocations(whs1, "L1W1", 5, 5);
			var location2 = Helper.CreateRowAndGenerateLocations(whs1, "L2W1", 1, 1);

			var locationTypePST = Helper.CreateLocationType("PST", LocationClasses.Codes.PST);
			var locationTypeNOR = Helper.CreateLocationType("NOR", LocationClasses.Codes.NOR);

			var row1Location1 = location1.Locations[0];
			var row2Location1 = location1.Locations[3];
			var row3location1 = location1.Locations[4];
			var row1Location2 = location2.Locations[0];
			Factory.Save();

			using (var form1 = new LocationForm(location1))
			{
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				row1Location1.WLV_WLT_LocationType = locationTypeNOR.PK;
				form1.FireSaveButton();
				AssertEquals("Normal/NOR location class does not trigger the popup", null, UnitTestUserNotification.Instance.LastMessage.Text);

				row2Location1.WLV_WLT_LocationType = locationTypePST.PK;
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
				AssertEquals(ContinueWithSave.No, form1.FireSaveButton());
				AssertEquals("Should show confirm save first packing station popup", "This is the first Packing Station location for this warehouse. Once saved, all Tote/Trolley jobs must be put away at the Packing Station before proceeding to the Dock Door. Do you wish to enable Packing Stations in this Warehouse?",
				UnitTestUserNotification.Instance.LastMessage.Text);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				AssertEquals(ContinueWithSave.Yes, form1.FireSaveButton());
				AssertEquals("Should show confirm save first packing station popup", "This is the first Packing Station location for this warehouse. Once saved, all Tote/Trolley jobs must be put away at the Packing Station before proceeding to the Dock Door. Do you wish to enable Packing Stations in this Warehouse?",
				UnitTestUserNotification.Instance.LastMessage.Text);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				row3location1.WLV_WLT_LocationType = locationTypePST.PK;
				form1.FireSaveButton();
				AssertEquals("Second PackingStation/PST should not trigger the popup", null, UnitTestUserNotification.Instance.LastMessage.Text);
			}
			using (var form2 = new LocationForm(location2))
			{
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				row1Location2.WLV_WLT_LocationType = locationTypePST.PK;
				form2.FireSaveButton();
				AssertEquals("First PackingStation/PST of a different location and same warehouse should not trigger the popup", null, UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		#endregion

		#region SortByLocation Button

		public void TestSortButton()
		{
			var whs = Helper.CreateWarehouse("1", "A", 2, 2);
			var row = whs.Rows.Single(r => r.WR_Name == "A");

			row.Locations[0].WLV_MaxCubic = 4;
			row.Locations[1].WLV_MaxCubic = 3;
			row.Locations[2].WLV_MaxCubic = 2;
			row.Locations[3].WLV_MaxCubic = 1;

			row.Locations.ApplySort(WhsLocationViewSchema.WLV_MaxCubic.Name, ListSortDirection.Ascending);

			AssertEquals("A-2-2", row.Locations[0].ToLocationString());
			AssertEquals("A-2-1", row.Locations[1].ToLocationString());
			AssertEquals("A-1-2", row.Locations[2].ToLocationString());
			AssertEquals("A-1-1", row.Locations[3].ToLocationString());

			using (var form = new LocationFormForTest(row))
			{
				form.Show();
				Assert("SortButton is too narrow for translation", form.GetSortButton().Width > 169);
				form.GetSortButton().PerformClick();
				AssertEquals("A-1-1", row.Locations[0].ToLocationString());
				AssertEquals("A-1-2", row.Locations[1].ToLocationString());
				AssertEquals("A-2-1", row.Locations[2].ToLocationString());
				AssertEquals("A-2-2", row.Locations[3].ToLocationString());
			}
		}

		#endregion
	}

	class LocationFormForTest : LocationForm
	{
		public LocationFormForTest(WhsRow row)
			: base(row)
		{
		}

		public bool GetSetupPostingCalled() => SetupPostingCalled;

		public MenuItem GetFileMenuItem() => FileMenuItem;

		public ZButton GetSortButton() => SortButtonForTest;
	}
}
