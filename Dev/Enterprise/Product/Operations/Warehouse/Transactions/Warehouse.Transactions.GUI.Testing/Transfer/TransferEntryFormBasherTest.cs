using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.GUI;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transactions.Business.Testing;
using Enterprise.Warehouse.Transactions.CodeLists;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Warehouse.Transactions.GUI.Testing
{
	[TestedType(typeof(TransferEntryForm))]
	public class TransferEntryFormBasherTest : ZFormBasherTest
	{
		#region TestFinaliseLinesMenuItem

		public void TestFinaliseLinesMenuItem()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m, data.Whs1.FindLocation("A-1"), "");
			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR1");
			Helper.CreateWhsTransferLine(transfer, data.Part1, 10m, "A-1", "A-2");
			using (var form = new TransferEntryFormForTest(transfer, new NotificationSubscriberGuiHelper()))
			{
				form.Show();
				var actionsMenuItem = form.ActionsMenuItem_Exposed;
				var finaliseLinesMenuItem = actionsMenuItem.MenuItems.FindByText("Finalize Line(s)");
				actionsMenuItem.ShowPopupMenu();
				AssertEquals("When transfer is not finalised or canceled Finalise Lines functionality should be enabled.", true, finaliseLinesMenuItem.Enabled);

				transfer.FinaliseDocketWithoutUserConfirmation();
				WhsTestCaseWithFactory.AssertIsFinalisedPrecondition(transfer);
				actionsMenuItem.ShowPopupMenu();
				AssertEquals("When transfer is finalised, then Finalise Lines functionality should be disabled.", false, finaliseLinesMenuItem.Enabled);

				transfer.WD_DocketStatus = DocketStatus.Codes.Cancelled;
				actionsMenuItem.ShowPopupMenu();
				AssertEquals("When transfer is Cancelled, then Finalise Lines functionality should be disabled.", false, finaliseLinesMenuItem.Enabled);
			}
		}

		#endregion

		#region TestFinaliseLinesMenuItem_OutboundDockDoorTransfer

		public void TestFinaliseLinesMenuItem_OutboundDockDoorTransfer()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m, data.Whs1.FindLocation("A-1"), "");
			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR1");
			Helper.CreateWhsTransferLine(transfer, data.Part1, 10m, "A-1", "A-2");
			transfer.WD_WP_ParentPickForTransfer = Factory.New<WhsPick>().PK;

			using (var form = new TransferEntryFormForTest(transfer, new NotificationSubscriberGuiHelper()))
			{
				form.Show();
				var actionsMenuItem = form.ActionsMenuItem_Exposed;
				var finaliseLinesMenuItem = actionsMenuItem.MenuItems.FindByText("Finalize Line(s)");
				actionsMenuItem.ShowPopupMenu();
				AssertEquals("When transfer is an Outbound Dock Door Transfer, Finalise Lines Menu Item should not be enabled.", false, finaliseLinesMenuItem.Enabled);
			}
		}

		#endregion

		#region TestFinaliseLinesMenuItem_InterWhs

		public void TestFinaliseLinesMenuItem_InterWhs_Dest()
		{
			TestFinaliseLinesMenuItem_InterWhs_Core(TransferType.Codes.InterWhsDest);
		}

		public void TestFinaliseLinesMenuItem_InterWhs_Source()
		{
			TestFinaliseLinesMenuItem_InterWhs_Core(TransferType.Codes.InterWhsSource);
		}

		void TestFinaliseLinesMenuItem_InterWhs_Core(string parentType)
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var whs2 = Helper.CreateWarehouse("W2", "A");

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Factory.Save();

			var transfer = Helper.CreateWhsTransfer(data.Org1, parentType == TransferType.Codes.InterWhsDest ? whs2 : data.Whs1);
			transfer.DocketSubType = parentType;

			var transferLine = Helper.CreateWhsTransferLine(transfer, data.Part1, 10m, "A", parentType == TransferType.Codes.InterWhsDest ? data.Whs1.PK : whs2.PK, "A");
			AssertEquals("Master Transfer Line should *not* be a child line.", false, transferLine.IsChildTransferLine);
			transferLine.RunPreSaveValidation();
			AssertEquals("Precondition.", 10m, transferLine.QtyCommittedIncludingMatchingLines);

			transferLine.PickedTime = ZDateTimeOffset.Now;
			AssertNotNull("Precondition: Child Transfer created.", transferLine.ChildTransferLine);

			// Parent
			using (var form = new TransferEntryFormForTest(transfer, new NotificationSubscriberGuiHelper()))
			{
				form.Show();
				var actionsMenuItem = form.ActionsMenuItem_Exposed;
				var finaliseLinesMenuItem = actionsMenuItem.MenuItems.FindByText("Finalize Line(s)");
				actionsMenuItem.ShowPopupMenu();
				AssertEquals("When transfer is not finalised or canceled Finalise Lines functionality should be enabled on the parent.", true, finaliseLinesMenuItem.Enabled);
			}

			// Child
			using (var form = new TransferEntryFormForTest(transfer.ChildTransfers.Single(), new NotificationSubscriberGuiHelper()))
			{
				form.Show();
				var actionsMenuItem = form.ActionsMenuItem_Exposed;
				var finaliseLinesMenuItem = actionsMenuItem.MenuItems.FindByText("Finalize Line(s)");
				actionsMenuItem.ShowPopupMenu();
				AssertEquals("Finalise Lines should *not* be enabled on the child.", false, finaliseLinesMenuItem.Enabled);
			}
		}

		#endregion

		#region TestZPanel1MovedToCorrectPosition

		public void TestZPanel1MovedToCorrectPosition()
		{
			using (var form = new TransferEntryForm(Factory.New<WhsTransfer>(), new NotificationSubscriberGuiHelper()))
			{
				form.Show();
				var panel = GUITestHelper.FindControl<ZPanel>(form.Controls, "zPanel1");
				AssertEquals(System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right, panel.Anchor);
			}
		}

		#endregion

		#region TestFinaliseTransferDbHits

		[StressTest]
		public void TestFinaliseTransferDbHits()
		{
			const int numberOfLinesToCreate = 100; // Lines to create
			var data = new TestDataSimpleEnvironment_ForFatDatTestsRequiringDataSetup(Factory, 2, 1);
			var receive = Helper.CreateWhsReceiveWithInventory(
				data.Org1, data.Whs1, data.GetUniqueNameForFatDat(WhsDocketSchema.WD_ExternalReference),
				ZDateTimeOffset.Today, data.Part1, numberOfLinesToCreate, data.Whs1.FindLocation("A-1"), "");
			Factory.Save();

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, data.GetUniqueNameForFatDat(WhsDocketSchema.WD_ExternalReference));
			for (int i = 0; i < numberOfLinesToCreate; i++)
			{
				Helper.CreateWhsTransferLine(transfer, data.Part1, 1m, "A-1", "A-2");
			}
			transfer.RunPreSaveValidation(); // to commit inventory
			Factory.Save();

			var otherFactory = new BusinessObjectFactory() { RefreshEnabled = false };
			var transferInOtherFactory = otherFactory.Load<WhsTransfer>(transfer.PK);
			using (var form = new TransferEntryForm(transferInOtherFactory, new NotificationSubscriberGuiHelper()))
			{
				form.Show();
				var finaliseButton = GUITestHelper.FindControl<ZButton>(form.Controls, "FinaliseButton");
				finaliseButton.PerformClick();

				WhsTestCaseWithFactory.AssertIsFinalisedPrecondition(transferInOtherFactory);
				var expectedDbHits = new Dictionary<string, int>();
				expectedDbHits.Add(WhsDocketLineSchema.Constants.TableName, 6); // original: 204
				expectedDbHits.Add(WhsInventoryViewSchema.Constants.TableName, 3);
				// increased by BAS from 3 to 4
				// increased by SVZ from 4 to 5
				// decreased by A.V from 5 to 3
				expectedDbHits.Add(WhsPickLineSchema.Constants.TableName, 6); // original: 102
				AssertDbHits(expectedDbHits, otherFactory, true);
			}
		}

		#endregion

		#region TestDbHits_ChildInterWhsSourceLinesWithNoDestLocation

		[StressTest]
		public void TestDbHits_ChildInterWhsSourceLinesWithNoDestLocation()
		{
			var data = new TestDataSimpleEnvironment(Factory);

			var warehouses = new WhsWarehouse[10];
			for (int i = 0; i < 10; i++)
			{
				warehouses[i] = Helper.CreateWarehouse(string.Format("W{0}", i), "A");
				Helper.CreateWhsReceiveWithInventory(data.Org1, warehouses[i], string.Format("R{0}", i), data.Part1, 100m, warehouses[i].FindLocation("A"), "");
			}

			Factory.Save();

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR1", transferType: TransferType.Codes.InterWhsDest);

			for (int i = 0; i < 100; i++)
			{
				var transferLine = Helper.CreateWhsTransferLine(transfer, data.Part1, 1m, "A", warehouses[i % 10].PK, "");
				transfer.RunPreSaveValidation(); // to generate pick lines
				transferLine.PickedTime = ZDateTimeOffset.Now;

				var childLine = transferLine.ChildTransferLine;
				AssertNotNull("Precondition: Created child line.", childLine);
			}

			Factory.Save();

			// Child Transfer
			var otherFactory1 = new BusinessObjectFactory { RefreshEnabled = false };
			var childTransferInOtherFactory = otherFactory1.Load<WhsTransfer>(transfer.ChildTransfers.First().PK);
			using (var form = new TransferEntryForm(childTransferInOtherFactory, new NotificationSubscriberGuiHelper()))
			{
				form.Show();

				var expectedDbHits = new Dictionary<string, int>
				{
					{ OrgAddressSchema.Constants.TableName, 2 },
					{ OrgAddressCapabilitySchema.Constants.TableName, 1 },
					{ OrgCustomLabelsSchema.Constants.TableName, 1 },
					{ OrgHeaderSchema.Constants.TableName, 1 },
					{ OrgMiscServSchema.Constants.TableName, 1 },
					{ OrgWebURLSchema.Constants.TableName, 1 },
					{ StmNoteSchema.Constants.TableName, 1 },
					{ WhsAreaSchema.Constants.TableName, 1 },
					{ WhsDocketSchema.Constants.TableName, 2 },
					{ WhsVASOrderSchema.Constants.TableName, 1 },
					{ WhsDocketLineSchema.Constants.TableName, 3 },
					{ WhsWarehouseSchema.Constants.TableName, 1 },
					{ WhsLocationViewSchema.Constants.TableName, 1 },
				};
				AssertDbHits(expectedDbHits, otherFactory1);
			}

			// Parent
			var otherFactory2 = new BusinessObjectFactory { RefreshEnabled = false };
			var transferInOtherFactory = otherFactory2.Load<WhsTransfer>(transfer.PK);
			using (var form = new TransferEntryForm(transferInOtherFactory, new NotificationSubscriberGuiHelper()))
			{
				form.Show();

				var expectedDbHits = new Dictionary<string, int>
				{
					{ OrgAddressSchema.Constants.TableName, 2 },
					{ OrgAddressCapabilitySchema.Constants.TableName, 1 },
					{ OrgCustomLabelsSchema.Constants.TableName, 1 },
					{ OrgHeaderSchema.Constants.TableName, 1 },
					{ OrgMiscServSchema.Constants.TableName, 1 },
					{ OrgWebURLSchema.Constants.TableName, 1 },
					{ StmNoteSchema.Constants.TableName, 1 },
					{ WhsAreaSchema.Constants.TableName, 1 },
					{ WhsDocketSchema.Constants.TableName, 1 },
					{ WhsVASOrderSchema.Constants.TableName, 1 },
					{ WhsPickLineSchema.Constants.TableName, 2 },
					{ WhsDocketLineSchema.Constants.TableName, 1 },
					{ WhsWarehouseSchema.Constants.TableName, 1 },
					{ WhsLocationViewSchema.Constants.TableName, 1 },
				};
				AssertDbHits(expectedDbHits, otherFactory2);
			}
		}

		#endregion

		#region TestDbHits_ChildInterWhsDestLines

		[StressTest]
		public void TestDbHits_ChildInterWhsDestLines()
		{
			var data = new TestDataSimpleEnvironment(Factory);

			var warehouses = new WhsWarehouse[10];
			for (int i = 0; i < 10; i++)
			{
				warehouses[i] = Helper.CreateWarehouse(string.Format("W{0}", i), "A");
			}

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 100m, data.Whs1.FindLocation("A"), "");
			Factory.Save();

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR1", transferType: TransferType.Codes.InterWhsSource);

			for (int i = 0; i < 100; i++)
			{
				var transferLine = Helper.CreateWhsTransferLine(transfer, data.Part1, 1m, "A", warehouses[i % 10].PK, "");
				AssertNoErrors("Precondition: Dest Location cannot be empty. If this is valid we may need extra fetch hints.", transferLine.WE_WLInfo);
				transferLine.LocationString = "A";

				transfer.RunPreSaveValidation(); // to generate pick lines
				transferLine.PickedTime = ZDateTimeOffset.Now;

				var childLine = transferLine.ChildTransferLine;
				AssertNotNull("Precondition: Created child line.", childLine);
			}

			Factory.Save();

			// Child Transfer
			var otherFactory1 = new BusinessObjectFactory { RefreshEnabled = false };
			var childTransferInOtherFactory = otherFactory1.Load<WhsTransfer>(transfer.ChildTransfers.First().PK);
			using (var form = new TransferEntryForm(childTransferInOtherFactory, new NotificationSubscriberGuiHelper()))
			{
				form.Show();

				var expectedDbHits = new Dictionary<string, int>
				{
					{ OrgAddressSchema.Constants.TableName, 2 },
					{ OrgAddressCapabilitySchema.Constants.TableName, 1 },
					{ OrgCustomLabelsSchema.Constants.TableName, 1 },
					{ OrgHeaderSchema.Constants.TableName, 1 },
					{ OrgMiscServSchema.Constants.TableName, 1 },
					{ OrgWebURLSchema.Constants.TableName, 1 },
					{ StmNoteSchema.Constants.TableName, 1 },
					{ WhsAreaSchema.Constants.TableName, 1 },
					{ WhsDocketSchema.Constants.TableName, 1 },
					{ WhsVASOrderSchema.Constants.TableName, 1 },
					{ WhsDocketLineSchema.Constants.TableName, 1 },
					{ WhsWarehouseSchema.Constants.TableName, 1 },
					{ WhsLocationViewSchema.Constants.TableName, 1 },
				};
				AssertDbHits(expectedDbHits, otherFactory1);
			}

			// Parent
			var otherFactory2 = new BusinessObjectFactory { RefreshEnabled = false };
			var transferInOtherFactory = otherFactory2.Load<WhsTransfer>(transfer.PK);
			using (var form = new TransferEntryForm(transferInOtherFactory, new NotificationSubscriberGuiHelper()))
			{
				form.Show();

				var expectedDbHits = new Dictionary<string, int>
				{
					{ OrgAddressSchema.Constants.TableName, 2 },
					{ OrgAddressCapabilitySchema.Constants.TableName, 1 },
					{ OrgCustomLabelsSchema.Constants.TableName, 1 },
					{ OrgHeaderSchema.Constants.TableName, 1 },
					{ OrgMiscServSchema.Constants.TableName, 1 },
					{ OrgWebURLSchema.Constants.TableName, 1 },
					{ StmNoteSchema.Constants.TableName, 1 },
					{ WhsAreaSchema.Constants.TableName, 1 },
					{ WhsDocketSchema.Constants.TableName, 1 },
					{ WhsVASOrderSchema.Constants.TableName, 1 },
					{ WhsPickLineSchema.Constants.TableName, 2 },
					{ WhsDocketLineSchema.Constants.TableName, 3 },
					{ WhsWarehouseSchema.Constants.TableName, 1 },
					{ WhsLocationViewSchema.Constants.TableName, 1 },
				};
				AssertDbHits(expectedDbHits, otherFactory2);
			}
		}

		#endregion

		#region TestFinaliseButton_ReadOnly

		public void TestFinaliseButton_ReadOnly()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1, saveFactory_doNotUseForNewTests: false);
			var whs2 = Helper.CreateWarehouse("WH2", "B", 2, 1);
			Factory.Save();

			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m, data.Whs1.FindLocation("A-1"), "");
			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR1", new Environment.Business.Testing.TestNotificationBuffer(), TransferType.Codes.InterWhsSource);
			var transferLine = Helper.CreateWhsTransferLine(transfer, data.Part1, 10m, "A-1", whs2.PK, "B-1");
			using (var form = new TransferEntryFormForTest(transfer, new NotificationSubscriberGuiHelper()))
			{
				form.Show();
				AssertEquals(true, form.FinaliseButton_Exposed.Enabled);
			}

			transferLine.FinaliseDocketLine();
			WhsTestCaseWithFactory.AssertIsFinalisedPrecondition(transferLine);
			using (var form = new TransferEntryFormForTest(transfer, new NotificationSubscriberGuiHelper()))
			{
				form.Show();
				AssertEquals(true, form.FinaliseButton_Exposed.Enabled);
			}
			using (var form = new TransferEntryFormForTest(transfer.ChildTransfers.ElementAt(0), new NotificationSubscriberGuiHelper()))
			{
				form.Show();
				AssertEquals(false, form.FinaliseButton_Exposed.Enabled);
			}

			transfer.FinaliseDocket();
			WhsTestCaseWithFactory.AssertIsFinalisedPrecondition(transfer);
			WhsTestCaseWithFactory.AssertIsFinalisedPrecondition(transfer.ChildTransfers.ElementAt(0));
			using (var form = new TransferEntryFormForTest(transfer, new NotificationSubscriberGuiHelper()))
			{
				form.Show();
				AssertEquals(false, form.FinaliseButton_Exposed.Enabled);
			}
			using (var form = new TransferEntryFormForTest(transfer.ChildTransfers.ElementAt(0), new NotificationSubscriberGuiHelper()))
			{
				form.Show();
				AssertEquals(false, form.FinaliseButton_Exposed.Enabled);
			}
		}

		#endregion

		#region TestShowBottomPanel_OutboundDockDoorTransfer

		public void TestShowBottomPanel_OutboundDockDoorTransfer()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m, data.Whs1.FindLocation("A-1"), "");
			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR1");
			transfer.WD_WP_ParentPickForTransfer = Factory.New<WhsPick>().PK;
			Helper.CreateWhsTransferLine(transfer, data.Part1, 10m, "A-1", "A-2");

			using (var form = new TransferEntryFormForTest(transfer, new NotificationSubscriberGuiHelper()))
			{
				form.Show();
				AssertEquals("When transfer is an Outbound Dock Door Transfer, Bottom Panel should not be shown.", false, form.ShowBottomPanel);
			}
		}

		#endregion

		#region TestHandleSaveException_ShowsMessageWhenTriggersFail

		public void TestHandleSaveException_ShowsMessageWhenTriggersFail()
		{
			var transfer = Factory.NewWithValidTestData<WhsTransfer>();
			transfer.WD_BookingDate = ZDateTimeOffset.Now;
			GUITestHelper.AssertHandleSaveException_ShowsMessageWhenTriggersFail(() => new TransferEntryForm(transfer, new NotificationSubscriberGuiHelper()));
		}

		#endregion

		#region Implementation

		protected override Form GetFormToBashCore()
		{
			TransferEntryForm result = new TransferEntryForm(Factory.New<WhsTransfer>(), new NotificationSubscriberGuiHelper());
			result.ControllerID = ControllerIDs.WhsTransfer;
			return result;
		}

		#region Helper

		protected WhsTestHelperFunctions Helper
		{
			get { return helper ?? (helper = new WhsTestHelperFunctions(Factory)); }
		}

		WhsTestHelperFunctions helper;

		#endregion

		#endregion
	}

	#region TransferEntryFormForTest class

	class TransferEntryFormForTest : TransferEntryForm
	{
		public TransferEntryFormForTest(WhsTransfer transfer, NotificationSubscriberGuiHelper whsNotificationSubscriberGuiHelper)
			: base(transfer, whsNotificationSubscriberGuiHelper)
		{
		}

		#region FinaliseButton_Exposed

		public Button FinaliseButton_Exposed
		{
			get
			{
				return finaliseButton_Exposed ?? (finaliseButton_Exposed = (Button)Controls.Cast<Control>().Single(c => c.Name == "zPanel1")
						.Controls.Cast<Control>().Single(c => c.Name == "zPanel2")
						.Controls.Cast<Control>().Single(c => c.Name == "zPanel3")
						.Controls.Cast<Control>().Single(c => c.Name == "FinaliseButton"));
			}
		}

		Button finaliseButton_Exposed;

		#endregion

		#region ActionsMenuItem_Exposed

		public MenuItem ActionsMenuItem_Exposed
		{
			get { return ActionsMenuItem; }
		}

		#endregion
	}

	#endregion
}
