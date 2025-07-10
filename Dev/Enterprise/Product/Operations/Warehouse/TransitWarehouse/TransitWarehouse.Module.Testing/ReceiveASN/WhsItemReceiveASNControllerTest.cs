using System.Linq;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Integration.CodeLists;
using Enterprise.Warehouse.Transit.Business;
using Enterprise.Warehouse.Transit.GUI;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Warehouse.Transit.Module.Testing.ReceiveASN
{
	[TestedType(typeof(WhsItemReceiveASNController))]
	public class WhsItemReceiveASNControllerTest : WhsTransitControllerTest<WhsItemReceiveASNController, WhsItemReceiveASN>
	{
		protected override ControllerID GetControllerID()
		{
			return ControllerIDs.WhsItemReceiveASN;
		}

		protected override ModuleIdentifier ExpectedModuleID => ModuleIDs.WhsItemReceiveASN;

		public void TestGetForm()
		{
			var address = Factory.NewWithValidTestData<OrgAddress>();
			var warehouse1 = Helper.CreateWarehouse("WH1", address, GlbBranch.CurrentBranch);
			warehouse1.WW_WarehouseType = WarehouseTypes.Codes.Transit;
			var row1 = Helper.CreateRowAndGenerateLocations(warehouse1, "Dock", 2, 2);
			var location1 = row1.Locations.First(l => l.ToLocationString() == "Dock-1-1");

			var warehouse2 = Helper.CreateTRWWarehouse(warehouseCode: "WH2");
			var row2 = Helper.CreateRowAndGenerateLocations(warehouse2, "Dock", 2, 2);
			var location2 = row2.Locations.First(l => l.ToLocationString() == "Dock-1-1");

			var asn1 = Helper.CreateReceiveASN("ASN1", warehouse1.PK);
			var asn2 = Helper.CreateReceiveASN("ASN2", warehouse2.PK);

			Factory.Save();

			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();

			using (var form = GetNewController.ShowEditForm(asn1))
			{
				AssertNull(UnitTestUserNotification.Instance.LastMessage.Text);
				AssertNotNull(form);
				AssertEquals(typeof(ReceiveASNForm), form.GetType());
			}

			using (var form = GetNewController.ShowEditForm(asn2))
			{
				AssertEquals(
					"Should not show the form",
					"The ASN could not be found. Please check that you are logged into the ASN's Warehouse Branch and try again.",
					UnitTestUserNotification.Instance.LastMessage.Text);
				AssertNull(form);
			}
		}

		public void TestShowFormForNewEntity()
		{
			using (var form = GetNewController.ShowFormForNewEntity(Factory.NewWithValidTestData<WhsItemReceiveASN>()))
			{
				AssertEquals(
					"Should not show the form",
					"You cannot create a Transit Advanced Shipping Notice on the CargoWise Desktop, please go to the Transit Warehouse Management Portal.",
					UnitTestUserNotification.Instance.LastMessage.Text);
				AssertNull(form);
			}
		}
	}
}
