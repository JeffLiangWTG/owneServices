using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.Services.OperationalActions.Business;
using Enterprise.Services.OperationalActions.Business.Testing;
using Enterprise.Services.OperationalActions.Support;
using Enterprise.Services.OperationalActions.Support.Testing;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transactions.Business.Testing;
using Enterprise.Warehouse.Transactions.CodeLists;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Warehouse.Transactions.Module.Testing
{
	[TestedType(typeof(PickOperationalActionsSupporter))]
	public class PickOperationalActionsSupporterTest : OperationalActionSupporterTest<PickOperationalActionsSupporter>
	{
		#region TestCustomOA_PrintingPickingSlip

		public void TestCustomOA_PrintingPickingSlip()
		{
			var pickingSlipQuery = new ZQuery();
			pickingSlipQuery.AddToFilter(StmMenuItemSchema.SU_MenuName, "Picking Slip");
			pickingSlipQuery.AddToFilter(StmMenuItemSchema.SU_MenuPath, ""); // get new document, not legacy

			var pickingSlipMenuItem = Factory.LoadTop1<StmMenuItem>(pickingSlipQuery);

			var action = Factory.New<OperationalAction>();
			action.Context = new OperationalActionContext(new PickOperationalActionsSupporter(), "Picking");
			action.SU_MenuName = "Print Document";
			action.DocumentPivots.AddNew().SF_SU_Outward = pickingSlipMenuItem.PK;

			var data = new TestDataSimpleEnvironment(Factory);
			var contact = data.Org1.Contacts.AddNew();
			contact.OC_ContactName = "AAA";
			contact.OC_Email = "AAA@AAA.COM";

			var document = contact.Documents.AddNew();
			document.OD_DocumentGroup = "ALL";

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 10m);
			var pick = Helper.CreatePickNew(order);
			Factory.Save();
			AssertEquals("Precondition", 10m, pick.GetAllPickLines().Sum(pl => pl.WZ_Units));
			AssertEquals("Precondition", PickStatus.Codes.Created, pick.WP_PickStatus);

			using (var pickModule = new PickingModule())
			{
				var moduleSelection = new ModuleSelection(pickModule);
				moduleSelection.Module.GridCollection.Add(pick);
				((ZFilterGridModule)moduleSelection.Module).DisplayGrid.SelectAllElements();

				var runner = new OperationalActionRunner(action, typeof(WhsPick), moduleSelection);
				runner.BulkDeliveryMethod = OnlyElectronicBulkDeliveryMethod.CodeText;
				AssertEquals("Precondition", 1, runner.SelectedGridRowCount);

				var dummyLog = new DummyOperationalActionLog();
				var factoryForChanges = new BusinessObjectFactory();

				runner.Run(dummyLog, factoryForChanges);
				AssertEquals("Should have only information, no errors.", OperationalActionLogErrorLevel.Informational, dummyLog.HighestErrorLevelEncountered);
				AssertEquals("Should have send document.", true, dummyLog.MessagesString().Contains("Documents"));
			}

			var otherFactory = new BusinessObjectFactory() { RefreshEnabled = false };
			var pickInOtherFactory = otherFactory.Load<WhsPick>(pick.PK);
			AssertEquals("Pick status should have been changed and saved to DB.", PickStatus.Codes.PickSlip, pickInOtherFactory.WP_PickStatus);
		}

		#endregion

		#region Implementation

		protected override ModuleIdentifier ModuleID => ModuleIDs.WhsPicking;

		#region Helper

		WhsTestHelperFunctions Helper => helper ?? (helper = new WhsTestHelperFunctions(Factory));

		WhsTestHelperFunctions helper;

		#endregion

		#endregion
	}
}
