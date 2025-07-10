using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.DocumentEngineCore.DocumentSupport.Testing;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.GUI;
using Enterprise.Registry.Business;
using Enterprise.Security;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transactions.Business.Printing;
using Enterprise.Warehouse.Transactions.Business.Testing;
using Enterprise.Warehouse.Transactions.CodeLists;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Internal;
using Enterprise.ZArchitecture.GUI.Testing;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Warehouse.Transactions.GUI.Testing
{
	[TestedType(typeof(ReceiveEntryForm))]
	public class ReceiveEntryFormBasherTest : ZFormBasherTest
	{
		#region Constructors

		public void TestConstructor()
		{
			var receive = Factory.New<WhsReceive>();
			using (var form = new ReceiveEntryForm(receive, new NotificationSubscriberGuiHelper()))
			{
				AssertNotNull("Billing not plugged in", form.PlugIns.GetPlugIn(ControllerIDs.JobInvoicing));
				AssertNotNull("EDocs not plugged in", form.PlugIns.GetPlugIn(ControllerIDs.eDocsPlugIn));
				AssertNotNull("Documents not plugged in", form.PlugIns.GetPlugIn(ControllerIDs.DocDataPlugIn));
				AssertNotNull("Addresses not plugged in", form.PlugIns.GetPlugIn(ControllerIDs.DocAddresses));
				AssertNotNull("DtbBooking not plugged in", form.PlugIns.GetPlugIn(ControllerIDs.DtbBooking));
				WhsGuiTestCaseWithFactory.AssertBinding(GUITestHelper.FindControl<Button>(form.Controls, "FinaliseButton"), "ReadOnly", "IsFinalisedOrCancelled");
				AssertEquals(true, form.IsResizableByTabPageAllowed);
			}
			AssertExceptionThrown<ArgumentNullException>("Exception should be thrown if null INotificationSubscriberQueryUser is passed in.",
				() => new ReceiveEntryForm(receive, null));
		}

		#endregion

		#region Properties

		public void TestFormCaption()
		{
			var receive = Factory.New<WhsReceive>();
			using (var form = new ReceiveEntryForm(receive, new NotificationSubscriberGuiHelper()))
			{
				AssertEquals("Receipt", form.FormCaption.Trim());
				receive.WD_DocketID = "W00000001";
				AssertEquals("Receipt W00000001", form.FormCaption);
			}
		}

		#endregion

		#region TestMustRunPreSaveValidationForReceiveInError

		public void TestMustRunPreSaveValidationForReceiveInError()
		{
			WhsReceive receive = Factory.New<WhsReceive>();
			receive.WD_DocketStatus = DocketStatus.Codes.Error;
			AssertEquals(false, receive.HasErrors);

			using (ReceiveEntryForm form = new ReceiveEntryForm(receive, new NotificationSubscriberGuiHelper()))
			{
			}

			AssertEquals("Receive must have errors after loading Receive Form", true, receive.HasErrors);
		}

		#endregion

		#region TestCustomNoteTypes

		public void TestCustomNoteTypes()
		{
			var receive = Factory.New<WhsReceive>();

			// Check the initial note types count
			int initialNoteTypesCount = receive.NoteTypes.Count;

			// Create a custom note for the receive module
			var customNoteRegistry = new CustomNoteModuleAndCountry();
			customNoteRegistry.ModuleIDName = ModuleIDs.WhsReceive.Name;
			customNoteRegistry.CountryCode = "ALL";

			var noteType = customNoteRegistry.CustomNoteTypesList.AddNew();
			noteType.IsTextOnly = ZBool.True;
			noteType.DefaultVisibility = nameof(CargoWise.Definitions.StmNoteVisibility.PUB);
			noteType.NoteName = "My Custom Note";

			var customNoteTypes = new CustomNoteTypes();
			customNoteTypes.NoteModuleAndCountryList.Add(customNoteRegistry);

			SystemDataRegistry.Instance.CustomNotes.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, customNoteTypes);

			// Open the form and select the notes tab in order to fire the custom note delegate done in the datasource binding
			using (var form = new ReceiveEntryFormForBasherTest(receive, new NotificationSubscriberGuiHelper()))
			{
				form.Show();
				form.ClickNotesTab();
				AssertEquals("The custom note is missing!", initialNoteTypesCount + 1, receive.NoteTypes.Count);
			}
		}

		#endregion

		#region TestDocumentWhsReceivePalletIDLabels

		public void TestDocumentWhsReceivePalletIDLabels()
		{
			WhsReceive receive = Helper.CreateWhsReceive(Client, Whs);

			using (ReceiveEntryForm form = new ReceiveEntryForm(receive, new NotificationSubscriberGuiHelper()))
			{
				form.Show();
				receive.DocumentSupporter.GetBODocDataProviders(new DataContextValueForTesting(Core.Constants.DataContext.WhsPalletIDLabels), null);
				Assert("Should load and show LabelOptionsForm for PalletID Labels", ZFormModaliser.LastFormShownDialogForTest is LabelOptionsForm);
				ZFormModaliser.LastFormShownDialogForTest.Dispose();
			}
		}

		#endregion

		#region TestHandleSaveException_ShowsMessageWhenTriggersFail

		public void TestHandleSaveException_ShowsMessageWhenTriggersFail()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var helper = new WhsTestHelperFunctions(Factory);
			var receive = helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 2m, finalise: false);
			GUITestHelper.AssertHandleSaveException_ShowsMessageWhenTriggersFail(() => new ReceiveEntryForm(receive, new NotificationSubscriberGuiHelper()));
		}

		#endregion

		#region TestInventoryFetchForView

		public void TestInventoryFetchForViewWithBondedSpecificColumns()
		{
			TestInventoryFetchForViewCore(true);
		}

		public void TestInventoryFetchForViewWithoutBondedSpecificColumns()
		{
			TestInventoryFetchForViewCore(false);
		}

		void TestInventoryFetchForViewCore(bool withBondedSpecificColumns)
		{
			const int numberOfInventoriesToCreate = 10;
			var data = new TestDataSimpleEnvironment(Factory, numberOfInventoriesToCreate, 1);
			data.Whs1.WarehouseAddress.OA_RL_NKRelatedPortCode = Helper.GetCountryUNLOCO(Core.Constants.CountryCodes.UnitedStates).RL_Code;

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			AssertEquals("Precondition", Constants.CountryCodes.UnitedStates, receive.CountryCode);

			if (withBondedSpecificColumns)
			{
				Helper.EnableWarehouseForBond(receive.Warehouse, true);
				receive.WD_DocketSubType = ReceiveType.Codes.Customs;
			}

			for (int i = 0; i < numberOfInventoriesToCreate; i++)
			{
				var part = Helper.CreateProduct(data.Org1, "PR" + i);
				var client = Helper.CreateClient("P" + i, "Client" + i);
				var location = data.Whs1.FindLocation("A-" + (i + 1));
				var line = (WhsReceiveLine)Helper.CreateWhsReceiveInventoryLine(receive, part, 1m, location).InDocketLine;
				line.ConsigneeDocAddress.OrganisationPK = Helper.CreateClient("C" + i, "CNE" + i).PK;
			}

			receive.AllocateLocationsWithMock();
			Factory.Save();

			List<TableColumn> tableColumns;
			using (var form = new ReceiveEntryForm(receive, new NotificationSubscriberGuiHelper()))
			{
				form.Show();
				var grid = ((ReceiveDocketLinesGridUserControl)(form.Controls.Find("inventoryGridUserControl2", true)[0])).LinesGrid;
				grid.SetAllColumnsVisible(true);
				var receiveLines = ((WhsReceive)(grid.DataSource)).Lines;
				tableColumns = new TableColumnCalculator().GetTableColumnsOnThisObject(receiveLines[0], grid.Columns).ToList();
			}

			AssertEquals("Precondition", withBondedSpecificColumns, tableColumns.Any(c => c.ColumnName.Contains(WhsDocketLine.Schema.CustomsData)));

			var expectedDbHits = new Dictionary<string, int>()
			{
				{ JobDocAddressSchema.Constants.TableName, 1 },
				{ OrgAddressSchema.Constants.TableName, 1 },
				{ OrgHeaderSchema.Constants.TableName, 1 },
				{ OrgSupplierPartSchema.Constants.TableName, 1 },
				{ OrgPartBOMSchema.Constants.TableName, 1 },
				{ RefPackTypeSchema.Constants.TableName, 1 },
				{ StmNoteSchema.Constants.TableName, 1 },
				{ WhsAreaSchema.Constants.TableName, 1 },
				{ WhsDocketSchema.Constants.TableName, 1 },
				{ WhsDocketLineSchema.Constants.TableName, 1 },
				{ WhsInventoryViewSchema.Constants.TableName, 1 },
				{ WhsPickLineSchema.Constants.TableName, 1 },
				{ WhsLocationViewSchema.Constants.TableName, 1 },
				{ WhsLocationTypeSchema.Constants.TableName, 1 },
			};

			if (withBondedSpecificColumns)
			{
				expectedDbHits.Add(WhsWarehouseSchema.Constants.TableName, 1);
				expectedDbHits.Add(StmALogSchema.Constants.TableName, 1);
				expectedDbHits.Add(CusClassPartPivotSchema.Constants.TableName, 2);
				expectedDbHits.Add(WhsBondedWarehouseAttributeSchema.Constants.TableName, 1);
			}

			var newFactory = new BusinessObjectFactory { RefreshEnabled = false };
			var receiveInOtherFactory = newFactory.Load<WhsReceive>(receive.PK);

			using (RowFactory.SetCachedTables())
			using (AssertDbHitsWithUsefulQueryInformation(expectedDbHits, newFactory))
			{
				foreach (var inventoryLine in receiveInOtherFactory.Lines)
				{
					inventoryLine.FetchStrategy.FetchForView(tableColumns.ToArray());
				}

				foreach (var inventoryLine in receiveInOtherFactory.Lines)
				{
					PokeColumnsProperties(inventoryLine, tableColumns);
				}
			}
		}

		void PokeColumnsProperties(WhsDocketLine inventoryLine, List<TableColumn> tableColumns)
		{
			var properties = inventoryLine.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);
			foreach (var property in properties.Where(p => p.Name != "Item" && tableColumns.Any(c => c.ColumnName.Contains(p.Name))))
			{
				Poke(property, inventoryLine);
			}
		}

		object Poke(PropertyInfo property, WhsDocketLine inventoryLine) => property.GetValue(inventoryLine, null);

		#endregion

		#region Event Handlers

		#region TestDocket_OnInventoryPrint

		public void TestDocket_OnInventoryPrint()
		{
			var receive = Helper.CreateWhsReceive(Client, Whs);

			using (var form = new ReceiveEntryForm(receive, new NotificationSubscriberGuiHelper()))
			{
				form.Show();
				receive.DocumentSupporter.GetBODocDataProviders(new DataContextValueForTesting(Core.Constants.DataContext.GenericProductLabel), null);
				Assert("Should load and show WhsDocumentInventoryOptionsForm for Product Labels", ZFormModaliser.LastFormShownDialogForTest is WhsDocumentInventoryOptionsForm);
				ZFormModaliser.LastFormShownDialogForTest.Dispose();
			}
		}

		#endregion

		#region Finalise Button

		public void TestFinaliseButton()
		{
			// setup an adjustment docket to use with the form
			var helper = new WhsTestHelperFunctions(Factory);
			var warehouse = helper.CreateWarehouse("1", "A", 1, 1);

			var client = helper.CreateClient();
			var prod = helper.CreateProduct(client, "P1");

			var receive = Helper.CreateWhsReceive(client, warehouse, "1");
			helper.CreateWhsReceiveInventoryLine(receive, prod, 10);
			receive.AllocateLocationsWithMock();
			Assert(receive.IsPuttingAway);
			WhsDocumentPrinter.LastPrintedDocumentName = ZString.Empty;

			using (var form = new ReceiveEntryFormForBasherTest(receive, new NotificationSubscriberGuiHelper()))
			{
				form.Show();

				// test user has no access
				Env.Security.WhsReceiveFinalise.IsAllowed = false;
				var finaliseButton = GUITestHelper.FindControl<Button>(form.Controls, "FinaliseButton");
				finaliseButton.PerformClick();

				// check for security error
				AssertEquals(true, UnitTestUserNotification.Instance.LastMessage.Contains(SecurityCore.SecurityErrorMessage));
				AssertEquals(true, UnitTestUserNotification.Instance.LastMessage.WasError);

				// ensure receive wasnt processed
				AssertEquals(false, receive.IsFinalised);
				AssertEquals(ZString.Empty, WhsDocumentPrinter.LastPrintedDocumentName);

				// test user has access
				Env.Security.WhsReceiveFinalise.IsAllowed = true;
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				AssertEquals(false, form.BusinessEntity.HasErrors());

				// test finalisation worked and notification subscriber was popped
				finaliseButton.PerformClick();
				AssertEquals(true, receive.IsFinalised);
				AssertEquals(form.GetNotificationBuffer_Exposed(), receive.NotificationManager.LastPopped);

				AssertEquals(ZString.Empty, WhsDocumentPrinter.LastPrintedDocumentName);

				// ensure no security error occurred
				AssertEquals(false, UnitTestUserNotification.Instance.LastMessage.Contains(SecurityCore.SecurityErrorMessage));
			}
		}

		#region TestFinaliseButtonReadonly

		public void TestFinaliseButtonReadonly_Finalised()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			AssertEquals(true, receive.IsFinalisedOrCancelled);

			using (var form = new ReceiveEntryForm(receive, new NotificationSubscriberGuiHelper()))
			{
				form.Show();

				AssertEquals(true, form.FinaliseButtonForTest.ReadOnly);
			}
		}

		public void TestFinaliseButtonReadonly_Cancelled()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			receive.WD_DocketStatus = DocketStatus.Codes.Cancelled;
			Factory.Save();
			AssertEquals(true, receive.IsFinalisedOrCancelled);

			using (var form = new ReceiveEntryForm(receive, new NotificationSubscriberGuiHelper()))
			{
				form.Show();

				AssertEquals(true, form.FinaliseButtonForTest.ReadOnly);
			}
		}

		public void TestFinaliseButtonReadonly_CreatedFromPickByBOM()
		{
			var helper = new WhsTestHelperFunctions(Factory);
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var bike = helper.CreateProduct(data.Org1, "BIKE");
			bike.OP_IsComponentPickedOnSalesOrder = true;
			var wheel = helper.CreateProduct(data.Org1, "WHEEL");
			helper.CreateProductBOM(bike, wheel, 2m, "UNT");

			var receive = helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			helper.CreateWhsReceiveInventoryLine(receive, wheel, 70m, data.Whs1.FindLocation("A-1"));
			receive.FinaliseDocketWithoutUserConfirmation();
			AssertEquals("Receive should be finalised.", true, receive.IsFinalised);

			var pick = Factory.New<WhsPick>();
			pick.WP_WW_Whs = data.Whs1.PK;
			pick.WP_WL_DockDoor = data.Whs1.WW_DefaultInboundDockDoor;
			pick.WP_PickOption = WhsPickOption.Codes.Manual;
			Factory.Save();

			var order = helper.CreateWhsOrder(data.Org1, data.Whs1, "O1", pickOption: WhsPickOption.Codes.Manual);
			var orderLine = helper.CreateWhsOrderLine(order, bike, 2m);

			pick.AddOrders(new[] { order });
			pick.AutoAllocateItemsWithMock();
			AssertEquals("Added component orderline should exist.", 1, orderLine.ChildComponentLines.Count);
			AssertEquals("Should be allocated.", 1, orderLine.ChildComponentLines.First().PickLines.Count);
			Factory.Save();

			var createdReceive = Factory.LoadTop1<WhsReceive>(new ZQuery(WhsDocketSchema.WD_WP_ParentPickForReceive, pick.PK));
			AssertEquals(true, receive.IsFinalisedOrCancelled);

			using (var form = new ReceiveEntryForm(createdReceive, new NotificationSubscriberGuiHelper()))
			{
				form.Show();

				AssertEquals(true, form.FinaliseButtonForTest.ReadOnly);
			}
		}

		public void TestFinaliseButtonReadonly_Entered()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			Factory.Save();
			AssertEquals(false, receive.IsFinalisedOrCancelled);

			using (var form = new ReceiveEntryForm(receive, new NotificationSubscriberGuiHelper()))
			{
				form.Show();

				AssertEquals(false, form.FinaliseButtonForTest.ReadOnly);
			}
		}

		#endregion

		#endregion

		#endregion

		#region TestDockDoorRelatedColumnsVisibility

		public void TestDockDoorRelatedColumnsVisibility()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var warehouseWithDockDoorTracking = Helper.CreateWarehouse("WDL");
			var receive = Factory.New<WhsReceive>();
			var receiveForDockDoorLocation = Helper.CreateWhsReceive(data.Org1, warehouseWithDockDoorTracking, "R2");

			using (var form = new ReceiveEntryForm(receive, new NotificationSubscriberGuiHelper()))
			{
				var grid = ((ReceiveDocketLinesGridUserControl)(form.Controls.Find("inventoryGridUserControl2", true)[0])).LinesGrid;
				grid.SetAllColumnsVisible(true);
				form.Show();
				AssertNull("DockDoor Column does not exist anymore.", grid.Columns[WhsDocketLineSchema.Constants.WE_WL_TransferFrom]);
				AssertNull("No Warehouse is selected therefore Putaway Transfer ID column must NOT be visible.", grid.Columns[WhsReceiveLine.Schema.PutawayTransferID]);

				receive.WD_WW_Whs = warehouseWithDockDoorTracking.PK;
				AssertNull("DockDoor Column does not exist anymore.", grid.Columns[WhsDocketLineSchema.Constants.WE_WL_TransferFrom]);
				Assert("Warehouse has dock door locations therefore Putaway Transfer ID column must be visible.", grid.Columns[WhsReceiveLine.Schema.PutawayTransferID].IsVisible);

				receive.WD_WW_Whs = data.Whs1.PK;
				AssertNull("DockDoor Column does not exist anymore.", grid.Columns[WhsDocketLineSchema.Constants.WE_WL_TransferFrom]);
				Assert("Warehouse does not have dock door locations (but tracking is always on now) therfore Putaway Transfer ID column must be visible.", grid.Columns[WhsReceiveLine.Schema.PutawayTransferID].IsVisible);

				receive.WD_WW_Whs = warehouseWithDockDoorTracking.PK;
				AssertNull("DockDoor Column does not exist anymore.", grid.Columns[WhsDocketLineSchema.Constants.WE_WL_TransferFrom]);
				Assert("Warehouse has dock door locations therefore Putaway Transfer ID column must be visible.", grid.Columns[WhsReceiveLine.Schema.PutawayTransferID].IsVisible);

				receive.WD_WW_Whs = ZGuid.Empty;
				AssertNull("DockDoor Column does not exist anymore.", grid.Columns[WhsDocketLineSchema.Constants.WE_WL_TransferFrom]);
				AssertNull("No Warehouse is selected therefore Putaway Transfer ID column must NOT be visible.", grid.Columns[WhsReceiveLine.Schema.PutawayTransferID]);
			}
		}

		#endregion

		#region TestNullReferenceFormCaption

		public void TestNullReferenceFormCaption()
		{
			var receive = Factory.New<WhsReceive>();
			using (var form = new ReceiveEntryForm(receive, new NotificationSubscriberGuiHelper()))
			{
				form.Dispose();
				var test = "";
				AssertNoExceptionThrown(() => test = form.FormCaption);
				AssertEquals("Receipt Unknown", test);
			}
		}

		#endregion

		#region TestDeniedPartyScreeningControls

		public void TestVisibleDeniedPartyScreeningControls()
		{
			var receive = Factory.New<WhsReceive>();
			using (var form = new ReceiveEntryForm(receive, new NotificationSubscriberGuiHelper()))
			{
				form.Show();

				Assert(form.ScreenButtonForTest.Visible);
				Assert(form.ScreeningStatusDropEditForTest.Visible);
				AssertEquals("NOT", form.ScreeningStatusDropEditForTest.Text);
			}
		}

		public void TestDPSMenuItemsPresent()
		{
			var receive = Factory.New<WhsReceive>();
			using (var form = new ReceiveEntryForm(receive, new NotificationSubscriberGuiHelper()))
			{
				form.Show();

				Assert(form.Menu.MenuItems.FindByText("View Compliance Status", true).Visible);
				Assert(form.Menu.MenuItems.FindByText("Resynchronize Screening Status", true).Visible);
			}
		}

		#endregion

		#region TestGenerateSerialNumbers_SuspendListChangeEvent

		public void TestGenerateSerialNumbers_SuspendListChangeEvent()
		{
			var data = new TestDataSimpleEnvironment_ForFatDatTestsRequiringDataSetup(Factory);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Serial, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Serial, true);
			Helper.SetProductWeightAndVolume(data.Part1, 500m, Constants.Weight.Grams, 2m, Constants.Volume.CubicDecimetres);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			var inventory = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m, data.Whs1.DefaultLocation);
			inventory.WI_SerialNumber = "1";
			receive.AllocateLocationsWithMock();

			Factory.Save();

			using (var form = new ReceiveEntryFormForBasherTest(receive, new NotificationSubscriberGuiHelper()))
			{
				form.Show();

				var countOfListChangedEventCalled = 0;
				((IBusinessObjectCollection)receive.Lines).ListChanged += (s, e) => { countOfListChangedEventCalled++; };
				var generateSerialNumbersMenu = form.ActionsMenuItem_ForTest.MenuItems.FindByText("Generate Serial Numbers");
				generateSerialNumbersMenu.PerformClick();
				AssertEquals("Pre-condition: should generated 10 lines.", 10, receive.Lines.Count);
				AssertEquals(1, countOfListChangedEventCalled);
			}
		}
		#endregion

		#region Implementation

		protected override void SetUp()
		{
			base.SetUp();
			Client = Helper.CreateClient();
			Whs = Helper.CreateWarehouse("TST");
		}

		protected override Form GetFormToBashCore()
		{
			ReceiveEntryForm result = new ReceiveEntryForm(Factory.New<WhsReceive>(), new NotificationSubscriberGuiHelper());
			result.ControllerID = ControllerIDs.WhsReceive;
			return result;
		}

		protected WhsTestHelperFunctions Helper
		{
			get
			{
				if (helper == null)
				{
					helper = new WhsTestHelperFunctions(Factory);
				}
				return helper;
			}
		}

		WhsTestHelperFunctions helper;
		protected OrgHeader Client;
		protected WhsWarehouse Whs;

		class ReceiveEntryFormForBasherTest : ReceiveEntryForm
		{
			public ReceiveEntryFormForBasherTest(WhsReceive docket, NotificationSubscriberGuiHelper whsNotificationSubscriberGuiHelper)
					: base(docket, whsNotificationSubscriberGuiHelper)
			{
				this.ControllerID = ControllerIDs.WhsReceive;
			}

			public void ClickNotesTab()
			{
				this.MainTabControl.SelectTab("NotesTabPage");
			}

			public INotifications GetNotificationBuffer_Exposed()
			{
				return GetNotificationBuffer();
			}

			public MenuItem ActionsMenuItem_ForTest
			{
				get { return ActionsMenuItem; }
			}
		}

		#endregion
	}
}
