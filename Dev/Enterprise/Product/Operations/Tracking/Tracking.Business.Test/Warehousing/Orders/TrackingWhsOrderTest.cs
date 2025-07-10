using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.TransportBookings.Business;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transactions.Business.Testing;
using Enterprise.Warehouse.Transactions.CodeLists;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using Enterprise.ZArchitecture.Web.Business;
using Enterprise.ZArchitecture.Web.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Tracking.Business.Testing
{
	[TestedType(typeof(WhsOrder))]
	sealed class TrackingWhsOrderTest : WhsOrderTest
	{
		#region Milestones

		public void TestMilestones()
		{
			var testWhsOrder = TrackingHelper.Get(Factory.NewWithValidTestData<WhsOrder>());
			AssertNotNull(testWhsOrder.Milestones);
			AssertEquals(0, testWhsOrder.Milestones.Count);

			var milestone1 = testWhsOrder.WhsOrder.WorkflowItems.Milestones.AddNew();
			milestone1.P9_Description = "lastCompleted1";
			milestone1.P9_IsPublished = true;
			milestone1.SetMilestoneActualDateForTest(DateTime.Now);
			milestone1.P9_Sequence = 1;

			var milestone2 = testWhsOrder.WhsOrder.WorkflowItems.Milestones.AddNew();
			milestone2.P9_Description = "lastCompleted2";
			milestone2.P9_IsPublished = true;
			milestone2.SetMilestoneActualDateForTest(DateTime.Now);
			milestone2.P9_Sequence = 1;
			testWhsOrder.Factory.Save();
			AssertEquals(0, testWhsOrder.Milestones.Count);

			testWhsOrder.ReloadMilestones();
			AssertEquals(2, testWhsOrder.Milestones.Count);
		}

		public void TestEditableMilestones()
		{
			var testWhsOrder = TrackingHelper.Get(Factory.NewWithValidTestData<WhsOrder>());

			var milestone1 = testWhsOrder.WhsOrder.WorkflowItems.Milestones.AddNew();
			milestone1.P9_Description = "lastCompleted1";
			milestone1.P9_IsPublished = true;
			milestone1.SetMilestoneActualDateForTest(DateTime.Now);
			milestone1.P9_Sequence = 1;

			AssertNoExceptionThrown(() => { var poke = testWhsOrder.EditableMilestones; });
		}

		#endregion

		#region TestHandleCritcalErrorMessage

		public void TestHandleCritcalErrorMessage_ChangeWE_TransactionQuantity()
		{
			AssertCriticalMessageWhenFactorySave((data, trackingOrderLine) =>
			{
				trackingOrderLine.WhsOrderLine.WE_TransactionQuantity = 2m;
			});
		}

		public void TestHandleCritcalErrorMessage_ChangeWE_OP()
		{
			AssertCriticalMessageWhenFactorySave((data, trackingOrderLine) =>
			{
				trackingOrderLine.WhsOrderLine.WE_OP = data.Part2.PK;
			});
		}

		public void TestHandleCritcalErrorMessage_ChangeWE_F3_NKPackType()
		{
			AssertCriticalMessageWhenFactorySave((data, trackingOrderLine) =>
			{
				trackingOrderLine.WhsOrderLine.WE_F3_NKPackType = "BOX";
			});
		}

		delegate void ChangeCriticalField(TestDataSimpleEnvironment data, TrackingWhsOrderLine trackingOrderLine);

		void AssertCriticalMessageWhenFactorySave(ChangeCriticalField changeCriticalField)
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var inventoryLocation = data.Whs1.FindLocation("A-1");
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			var inventory = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 5m, inventoryLocation);
			receive.FinaliseDocketWithoutUserConfirmation();
			AssertEquals("Receive should be finalised.", true, receive.IsFinalised);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 5m);
			Factory.Save();
			AssertEquals("OrderLine should be in Database", true, orderLine.IsInDatabase);

			using (Db.DisposableActionForDbConnection())
			using (var connectionForSecondUser = Db.NewExtraConnectionToMainDb())
			{
				var pick = Factory.New<WhsPick>();
				pick.PickOrdersWithAllocationMock(order);
				Factory.Save();

				AssertEquals(true, Globals.IsWeb);
				var trackingFactory = new BusinessObjectFactory();
				var trackingOrder = TrackingHelper.Get(trackingFactory.Load<WhsOrder>(order.PK));
				var trackingOrderLine = TrackingHelper.Get(trackingOrder.WhsOrder.Lines[0]);

				changeCriticalField(data, trackingOrderLine);

				AssertExceptionThrown(typeof(ZSaveException), () => trackingFactory.Save());
				AssertHasRowError(trackingOrder.WhsOrder, "Cannot edit critical information. Another user has picked Warehouse Order W00000002.");
			}
		}

		#endregion

		#region TestPreventSaveIfOrderAlreadyPicked_PickedOrderValidateChangeLinesInLocalCache

		public void TestPreventSaveIfOrderAlreadyPicked_PickedOrderValidateChangeLinesInLocalCache()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var inventoryLocation = data.Whs1.FindLocation("A-1");
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			var inventory = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 5m, inventoryLocation);
			receive.FinaliseDocketWithoutUserConfirmation();
			AssertEquals("Receive should be finalised.", true, receive.IsFinalised);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 5m);
			Factory.Save();

			var pick = Factory.New<WhsPick>();
			pick.PickOrdersWithAllocationMock(order);
			Factory.Save();

			Factory.ResetDatabaseLoadCount();

			var expectedNoDBHits = new Dictionary<string, int>();

			AssertEquals(true, Globals.IsWeb);
			var trackingFactory = new BusinessObjectFactory();
			var trackingOrder = TrackingHelper.Get(trackingFactory.Load<WhsOrder>(order.PK));
			var trackingOrderLine = TrackingHelper.Get(trackingOrder.WhsOrder.Lines[0]);

			trackingOrderLine.WhsOrderLine.WE_TransactionQuantity += 1m; // any change in order line

			AssertExceptionThrown(typeof(ZSaveException), () => trackingFactory.Save());
			AssertHasRowError(trackingOrder.WhsOrder, "Cannot edit critical information. Another user has picked Warehouse Order W00000002.");

			AssertDbHits(expectedNoDBHits, Factory);
		}

		#endregion

		#region TestTrackingWhsOrderLineCollection_PickedOrderLinesReadOnly

		public void TestTrackingWhsOrderLineCollection_PickedOrderLinesReadOnly()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var inventoryLocation = data.Whs1.FindLocation("A-1");
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			var inventory = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 5m, inventoryLocation);
			receive.FinaliseDocketWithoutUserConfirmation();
			AssertEquals("Receive should be finalised.", true, receive.IsFinalised);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 5m);
			Factory.Save();

			AssertEquals("Order is not pick, user should be able to edit lines.", true, new TrackingWhsOrder(order).Lines.All(l => !l.ReadOnly));

			var pick = Factory.New<WhsPick>();
			pick.PickOrdersWithAllocationMock(order);

			AssertEquals("Order is pick, user should *not* be able to edit lines.", true, new TrackingWhsOrder(order).Lines.All(l => l.ReadOnly));
		}

		#endregion

		#region TestParentLines_Tracking

		public void TestParentLines_Tracking()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			var childOrderLine = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			childOrderLine.WE_WE_ParentDocketLine = orderLine.PK;
			AssertContainsExactElementsInAnyOrder(new[] { orderLine }, order.ParentLines);
		}

		#endregion

		#region Lines

		public void TestLinesSorting()
		{
			var part1 = Factory.NewWithValidTestData<OrgSupplierPart>();
			part1.OP_PartNum = "Part1";
			part1.OP_Desc = "Part1 Description";

			var part2 = Factory.NewWithValidTestData<OrgSupplierPart>();
			part2.OP_PartNum = "Part2";
			part2.OP_Desc = "Part2 Description";

			var orderLines = TestOrder.WhsOrder.Lines;
			var line1 = orderLines.AddNew();
			line1.WE_OP = part1.PK;
			line1.WE_PackQuantity = 10;

			var line2 = orderLines.AddNew();
			line2.WE_OP = part1.PK;
			line2.WE_PackQuantity = 20;

			var line3 = orderLines.AddNew();
			line3.WE_OP = part1.PK;
			line3.WE_PackQuantity = 5;

			orderLines.ApplySort(WhsDocketLine.Schema.WE_PackQuantity, ListSortDirection.Ascending);
			AssertEquals(3, orderLines.Count);
			AssertEquals(line3, orderLines[0]);
			AssertEquals(line1, orderLines[1]);
			AssertEquals(line2, orderLines[2]);
			AssertEquals((short)1, line1.WE_LineNo);
			AssertEquals((short)2, line2.WE_LineNo);
			AssertEquals((short)3, line3.WE_LineNo);

			AssertEquals(3, TestOrder.Lines.Count);
			AssertEquals(line1, TestOrder.Lines[0].WhsOrderLine);
			AssertEquals(line2, TestOrder.Lines[1].WhsOrderLine);
			AssertEquals(line3, TestOrder.Lines[2].WhsOrderLine);
		}

		#endregion

		#region Summary Lines

		public void TestSummaryLines()
		{
			OrgSupplierPart part1 = Factory.NewWithValidTestData<OrgSupplierPart>();
			part1.OP_PartNum = "Part1";
			part1.OP_Desc = "Part1 Description";
			CreateProductUnit(part1, "BOX", "PLT", 2m);
			CreateProductUnit(part1, "BOX", "CAS", 3m);
			part1.OP_StockKeepingUnit = "BOX";

			OrgSupplierPart part2 = Factory.NewWithValidTestData<OrgSupplierPart>();
			part2.OP_PartNum = "Part2";
			part2.OP_Desc = "Part2 Description";
			CreateProductUnit(part2, "BOX", "PLT", 4m);
			part2.OP_StockKeepingUnit = "BOX";

			WhsOrderLine line1 = TestOrder.WhsOrder.Lines.AddNew();
			line1.WE_OP = part1.PK;
			line1.WE_PackQuantity = 10;
			line1.WE_F3_NKPackType = "PLT";

			WhsOrderLine line2 = TestOrder.WhsOrder.Lines.AddNew();
			line2.WE_OP = part1.PK;
			line2.WE_PackQuantity = 20;
			line2.WE_F3_NKPackType = "PLT";

			WhsOrderLine line3 = TestOrder.WhsOrder.Lines.AddNew();
			line3.WE_OP = part1.PK;
			line3.WE_PackQuantity = 20;
			line3.WE_F3_NKPackType = "CAS";

			WhsOrderLine line4 = TestOrder.WhsOrder.Lines.AddNew();
			line4.WE_OP = part1.PK;
			line4.WE_PackQuantity = 20;
			line4.WE_F3_NKPackType = "CAS";
			line4.WE_CustomAttrib1 = "TEST";

			WhsOrderLine line5 = TestOrder.WhsOrder.Lines.AddNew();
			line5.WE_OP = part2.PK;
			line5.WE_PackQuantity = 10;
			line5.WE_F3_NKPackType = "PLT";

			AssertEquals(5, TestOrder.WhsOrder.Lines.Count);

			AssertEquals(3, TestOrder.SummaryLines.Count);

			AssertSummaryLinesContains(TestOrder.SummaryLines, "PART1", "Part1 Description", 30m, "PLT", 60m, "BOX", 0m);
			AssertSummaryLinesContains(TestOrder.SummaryLines, "PART1", "Part1 Description", 40m, "CAS", 120m, "BOX", 0m);
			AssertSummaryLinesContains(TestOrder.SummaryLines, "PART2", "Part2 Description", 10m, "PLT", 40m, "BOX", 0m);
		}

		void AssertSummaryLinesContains(TrackingWhsOrderSummaryLineCollection lines, ZString productCode, ZString productDescription, ZDecimal packs, ZString packsUQ, ZDecimal ordered, ZString uQ, ZDecimal reserved)
		{
			bool contains = false;
			foreach (TrackingWhsOrderSummaryLine line in lines)
			{
				if (line.ProductCode == productCode &&
					line.ProductDescription == productDescription &&
					line.PacksQuantity == packs &&
					line.PacksUQ == packsUQ &&
					line.OrderedQuantity == ordered &&
					line.UQ == uQ &&
					line.ReservedQuantity == reserved)
				{
					contains = true;
					break;
				}
			}
			Assert("Summary Line for Product: " + productCode + " could not be found", contains);
		}

		void CreateProductUnit(OrgSupplierPart part, string partUnitType, string parentPartUnitType, ZDecimal partUnitSize)
		{
			OrgPartUnit partUnit = part.PartUnits.AddNew();
			partUnit.OF_PackType = partUnitType;
			partUnit.OF_ParentPackType = parentPartUnitType;
			partUnit.OF_QuantityInParent = partUnitSize;
		}

		#endregion

		#region TestNotificationOptions

		public void TestNotificationOptions()
		{
			AssertEquals(WebDataRegistry.Instance.WarehouseOrdersNotificationOptions, ((IBizOChangesEmailNotification)TestOrder).NotificationSendingRule);
		}

		#endregion

		#region AdditionalInformationFieldsTest

		public void TestGetAdditionalInformationFields()
		{
			OrgHeader testOrg = Factory.New<OrgHeader>();
			testOrg.OH_FullName = "Test organization";
			testOrg.OH_Code = "TST";

			OrgCustomLabels orgCustomLabel1 = testOrg.CustomLabels.AddNew();
			orgCustomLabel1.OT_FieldName = "WhsDocket.CustomAttrib1";
			orgCustomLabel1.OT_Caption = "Custom Attribute 1 Test";

			OrgCustomLabels orgCustomLabel2 = testOrg.CustomLabels.AddNew();
			orgCustomLabel2.OT_FieldName = "WhsDocket.CustomFlag1";
			orgCustomLabel2.OT_Caption = "Custom Flag 1 Test";

			AssertEquals("There must be 2 custom labels (created for testing).", 2, testOrg.CustomLabels.Count);

			TestOrder.WhsOrder.WD_OH_Client = testOrg.PK;
			CustomLabelInfoList customLabels = TestOrder.GetAdditionalInformationFields();

			AssertEquals("All 2 custom labels must be retrieved.", 2, customLabels.Count);

			Assert("All custom labels created must be retrieved.", customLabels.Contains(AutoWhsDocket.Schema.WD_CustomAttrib1));
			CustomLabelInfo customLabel1 = (CustomLabelInfo)customLabels.GetFieldByPropertyName(AutoWhsDocket.Schema.WD_CustomAttrib1);
			AssertEquals("Custom label properties retrieved must be the same as those set.", orgCustomLabel1.OT_Caption, customLabel1.Caption);
			AssertEquals("Custom label properties retrieved must be the same as those set.", orgCustomLabel1.OT_FieldName, customLabel1.LabelName);

			Assert("All custom labels created must be retrieved.", customLabels.Contains(AutoWhsDocket.Schema.WD_CustomFlag1));
			CustomLabelInfo customLabel2 = (CustomLabelInfo)customLabels.GetFieldByPropertyName(AutoWhsDocket.Schema.WD_CustomFlag1);
			AssertEquals("Custom label properties retrieved must be the same as those set.", orgCustomLabel2.OT_Caption, customLabel2.Caption);
			AssertEquals("Custom label properties retrieved must be the same as those set.", orgCustomLabel2.OT_FieldName, customLabel2.LabelName);
		}

		#endregion

		#region TestWarehousesAreOnlyActive

		[HttpContextEnabledTest]
		public void TestWarehousesAreOnlyActive()
		{
			TestHelper helper = new TestHelper(Factory);
			helper.TestSiteUser.Login(helper.TestOrg.OH_Code, helper.TestContact.OC_Email, helper.TestContact.PasswordForTesting);

			var whs1 = GetWarehouse(helper.TestSiteUser.LoggedInOrganisation.PK);
			var whs2 = GetWarehouse(helper.TestSiteUser.LoggedInOrganisation.PK);

			whs1.WW_IsActive = true;
			whs2.WW_IsActive = false;
			Factory.Save();
			TrackingWhsOrder order = TrackingHelper.Get(Factory.New<WhsOrder>());
			order.WhsOrder.Lookups.Warehouses.Load();
			Assert("Active warehouse is in collection", order.WhsOrder.Lookups.Warehouses.Contains(whs1));
			Assert("Inactive warehouse is not in collection", !order.WhsOrder.Lookups.Warehouses.Contains(whs2));
		}

		WhsWarehouse GetWarehouse(ZGuid orgPK)
		{
			WhsWarehouse warehouse = Factory.NewWithValidTestData<WhsWarehouse>();

			WhsOrder order = Factory.NewWithValidTestData<WhsOrder>();
			order.WD_WW_Whs = warehouse.PK;
			order.WD_OH_Client = orgPK;

			return warehouse;
		}

		#endregion

		#region Overrides

		protected override Type GetExpectedValidationType()
		{
			return typeof(TrackingWhsOrderValidation);
		}

		protected override Type GetExpectedLookupsType()
		{
			return typeof(TrackingWhsOrderLookups);
		}

		new TrackingWhsOrder GetNewBusinessObject()
		{
			return TrackingHelper.Get(base.GetNewBusinessObject());
		}

		protected override WhsOrder GetNewDocketForTemplateCopy()
		{
			var data = new TestDataSimpleEnvironment(Factory);

			var consignee = Helper.CreateClient("consignee");
			consignee.MiscServ.OM_IMDefaultINCOTerm = "FOB";
			consignee.MainAddress.OA_RN_NKCountryCode = "AU";
			consignee.MainAddress.OA_RL_NKRelatedPortCode = "AUSYD";

			var docket = GetNewBusinessObject().WhsOrder;
			docket.WD_WW_Whs = data.Whs1.PK;
			docket.WD_OH_Client = data.Org1.PK;
			docket.ConsigneePK = consignee.PK;

			var orderLine1 = Helper.CreateWhsPickableDocketLine(docket, data.Part1, 10m);
			var orderLine2 = Helper.CreateWhsPickableDocketLine(docket, data.Part2, 20m);

			return docket;
		}

		#endregion

		#region TestRelatedTransportBookingPKsAdded

		public void TestRelatedTransportBookingPKsAdded()
		{
			var whsOrder = Factory.NewWithValidTestData<WhsOrder>();
			var trackingWhsOrder = new TrackingWhsOrder(whsOrder);

			AssertEquals("Precondition: DocRelatedPKs should return no PKs at this stage.", 0, trackingWhsOrder.DocRelatedPKs.Count);

			var consolidation = Factory.NewWithValidTestData<DtbBookingConsolidation>();
			consolidation.KB_ParentID = whsOrder.PK;
			consolidation.KB_ParentTableCode = whsOrder.TablePrefix;

			var booking1 = Factory.NewWithValidTestData<DtbBooking>();
			var booking2 = Factory.NewWithValidTestData<DtbBooking>();
			var booking3 = Factory.NewWithValidTestData<DtbBooking>();

			consolidation.Bookings.AddRange(new List<DtbBooking> { booking1, booking2, booking3 });

			Factory.Save();

			AssertEquals("DocRelatedPKs should return 3 PKs.", 3, trackingWhsOrder.DocRelatedPKs.Count);
			AssertContainsExactElementsInAnyOrder("DocRelatedPKs should return all 3 booking PKs", new[] { booking1.PK, booking2.PK, booking3.PK }, trackingWhsOrder.DocRelatedPKs);
		}

		#endregion

		#region TestJobDocAddress_OverridenSuppressValidationError

		[HttpContextEnabledTest]
		public void TestJobDocAddress_OverridenSuppressValidationError_CarrierBookingAgentDocAddress()
		{
			TestJobDocAddress_OverridenSuppressValidationError_Core((order) => order.CarrierBookingAgentDocAddress, "CarrierBookingAgentDocAddress");
		}

		[HttpContextEnabledTest]
		public void TestJobDocAddress_OverridenSuppressValidationError_ConsigneeDocAddress()
		{
			TestJobDocAddress_OverridenSuppressValidationError_Core((order) => order.ConsigneeDocAddress, "ConsigneeDocAddress");
		}

		[HttpContextEnabledTest]
		public void TestJobDocAddress_OverridenSuppressValidationError_DistributionCentreDocAddress()
		{
			TestJobDocAddress_OverridenSuppressValidationError_Core((order) => order.DistributionCentreDocAddress, "DistributionCentreDocAddress");
		}

		[HttpContextEnabledTest]
		public void TestJobDocAddress_OverridenSuppressValidationError_DropOffDocAddress()
		{
			TestJobDocAddress_OverridenSuppressValidationError_Core((order) => order.DropOffDocAddress, "DropOffDocAddress");
		}

		[HttpContextEnabledTest]
		public void TestJobDocAddress_OverridenSuppressValidationError_GoodsBillToDocAddress()
		{
			TestJobDocAddress_OverridenSuppressValidationError_Core((order) => order.GoodsBillToDocAddress, "GoodsBillToDocAddress");
		}

		[HttpContextEnabledTest]
		public void TestJobDocAddress_OverridenSuppressValidationError_PickUpDocAddress()
		{
			TestJobDocAddress_OverridenSuppressValidationError_Core((order) => order.PickUpDocAddress, "PickUpDocAddress");
		}

		[HttpContextEnabledTest]
		public void TestJobDocAddress_OverridenSuppressValidationError_SupplierDocAddress()
		{
			TestJobDocAddress_OverridenSuppressValidationError_Core((order) => order.SupplierDocAddress, "SupplierDocAddress");
		}

		[HttpContextEnabledTest]
		public void TestJobDocAddress_OverridenSuppressValidationError_TransportBillToDocAddress()
		{
			TestJobDocAddress_OverridenSuppressValidationError_Core((order) => order.TransportBillToDocAddress, "TransportBillToDocAddress");
		}

		[HttpContextEnabledTest]
		public void TestJobDocAddress_OverridenSuppressValidationError_TransportCoDocAddress()
		{
			TestJobDocAddress_OverridenSuppressValidationError_Core((order) => order.TransportCoDocAddress, "TransportCoDocAddress");
		}

		void TestJobDocAddress_OverridenSuppressValidationError_Core(Func<WhsOrder, JobDocAddress> getJobDocAddress, string jobDocName)
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			Factory.Save();

			var overriddenAddress = Factory.NewWithValidTestData<OrgAddress>();
			var trackingOrder = TrackingHelper.Get(Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1"));
			var jobDocAddress = getJobDocAddress(trackingOrder.WhsOrder);
			jobDocAddress.E2_OA_Address = overriddenAddress.PK;
			jobDocAddress.E2_AddressOverride = true;
			Factory.Save();
			AssertEquals($"{jobDocName} validation error should be suppressed.", true, jobDocAddress.E2_SuppressAddressValidationError);

			trackingOrder.WhsOrder.WD_ExternalReference = "true";
			Factory.Save();
			AssertEquals($"{jobDocName} validation error should be suppressed.", true, jobDocAddress.E2_SuppressAddressValidationError);

			jobDocAddress.E2_AddressOverride = false;
			Factory.Save();
			AssertEquals($"{jobDocName} validation error should be Not suppressed again.", false, jobDocAddress.E2_SuppressAddressValidationError);

			trackingOrder.WhsOrder.WD_ExternalReference = "false";
			Factory.Save();
			AssertEquals($"{jobDocName} validation error should be Not suppressed again.", false, jobDocAddress.E2_SuppressAddressValidationError);
		}

		[HttpContextEnabledTest]
		public void TestJobDocAddress_NoErrorWhenOrderDocAddressesChangedDuringFactorySave()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var rateTransportProvider1 = Helper.SetUpRateTransportProvider(true, "OPS", "AU", "ALL", data.Org1);

			SetupMainOrgAddress(data.Org1, "20 Maxwell Street", "", "PERTH", "6162", "WA", "AUBYW");
			var rateTransportZoneDianella = Helper.SetUpRateTransportZone(rateTransportProvider1, true, "Dianella");
			Helper.SetUpRateTransportZoneItem(rateTransportZoneDianella, "AU", "6162");
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "1");
			order.TransportCoPK = data.Org1.PK;
			Factory.Save();

			// set suppress to false to change during the save and refresh addresses and call SetRateTransportZone
			order.ConsigneeDocAddress.E2_AddressOverride = true;
			order.ConsigneeDocAddress.E2_SuppressAddressValidationError = false;
			// delete to force create new doc address during the save which change the DocAddress collection
			order.TransportCoDocAddress.Delete();
			AssertNoExceptionThrown(Factory.Save);
		}

		void SetupMainOrgAddress(OrgHeader org, ZString address1, ZString address2, ZString city, ZString postCode, ZString state, ZString relatedPortCode)
		{
			var address = org.MainAddress;
			address.OA_Address1 = address1;
			address.OA_Address2 = address2;
			address.OA_City = city;
			address.OA_PostCode = postCode;
			address.OA_State = state;
			address.OA_RL_NKRelatedPortCode = relatedPortCode;
		}

		#endregion

		#region TestFromNumber
		[HttpContextEnabledTest]
		public void TestFromPK()
		{
			AssertNotNull(WebHelper.TestSiteUser);
			Factory.Save();

			WebHelper.TestSiteUser.Login(WebHelper.TestOrg.OH_Code, WebHelper.TestContact.OC_Email, WebHelper.TestContact.PasswordForTesting);
			TestOrder.SiteUser = WebHelper.TestSiteUser;

			TestOrder.WhsOrder.WD_OH_Client = WebHelper.TestOrg.PK;

			TestOrder.WhsOrder.WD_WW_Whs = Helper.CreateWarehouse("WHS1").PK;
			Factory.Save();

			TrackingWhsOrder testOrderFromPK = TrackingWhsOrder.FromPKFilteredByContact(Factory, TestOrder.WhsOrder.PK, WebHelper.TestSiteUser);
			AssertEquals(TestOrder, testOrderFromPK);
			AssertEquals("LoggedInContact", WebHelper.TestContact.PK, TestOrder.LoggedInContact.PK);
			AssertEquals("Order AutoCreatedLogDefaultSL_Reference", WebHelper.TestSiteUser.ContactAndCompanyReference, TestOrder.Logs.AutoCreatedLogDefaultSL_Reference);
		}

		[HttpContextEnabledTest]
		public void TestFromPKQuickShipment()
		{
			AssertNotNull(WebHelper.TestSiteUser);
			Factory.Save();

			WebHelper.TestSiteUser.LoginSupportForTest(GlbCompany.CurrentCompany.OrgProxy.OH_Code);
			TestOrder.SiteUser = WebHelper.TestSiteUser;

			TestOrder.WhsOrder.WD_OH_Client = GlbCompany.CurrentCompany.OrgProxy.PK;

			TestOrder.WhsOrder.WD_WW_Whs = Helper.CreateWarehouse("WHS1").PK;
			Factory.Save();

			TrackingWhsOrder testOrderFromPK = TrackingWhsOrder.FromPKFilteredByContact(Factory, TestOrder.WhsOrder.PK, WebHelper.TestSiteUser);
			AssertEquals(TestOrder, testOrderFromPK);
		}

		[HttpContextEnabledTest]
		public void TestFromNumber()
		{
			AssertNotNull(WebHelper.TestContact);
			Factory.Save();

			WebHelper.TestSiteUser.Login(WebHelper.TestOrg.OH_Code, WebHelper.TestContact.OC_Email, WebHelper.TestContact.PasswordForTesting);
			TestOrder.SiteUser = WebHelper.TestSiteUser;

			TestOrder.WhsOrder.WD_OH_Client = WebHelper.TestOrg.PK;

			TestOrder.WhsOrder.WD_WW_Whs = Helper.CreateWarehouse("WHS1").PK;
			Factory.Save();

			TrackingWhsOrder testOrderFromNumber = TrackingWhsOrder.FromNumberFilteredByContact(Factory, TestOrder.WhsOrder.WD_ExternalReference, WebHelper.TestSiteUser);
			AssertEquals(TestOrder, testOrderFromNumber);
			AssertEquals("LoggedInContact", WebHelper.TestContact.PK, TestOrder.LoggedInContact.PK);
			AssertEquals("Order AutoCreatedLogDefaultSL_Reference", WebHelper.TestSiteUser.ContactAndCompanyReference, TestOrder.Logs.AutoCreatedLogDefaultSL_Reference);
		}

		[HttpContextEnabledTest]
		public void TestFromNumberIncorrectOrg()
		{
			AssertNotNull(WebHelper.TestContact);
			Factory.Save();

			WebHelper.TestSiteUser.Login(WebHelper.TestOrg.OH_Code, WebHelper.TestContact.OC_Email, WebHelper.TestContact.PasswordForTesting);
			TestOrder.SiteUser = WebHelper.TestSiteUser;

			TestOrder.WhsOrder.WD_OH_Client = WebHelper.TestOrg.PK;

			TestOrder.WhsOrder.WD_WW_Whs = Helper.CreateWarehouse("WHS1").PK;
			Factory.Save();
			WebEnv.AppInstance.SiteUser.Logout();
			TrackingWhsOrder testOrderFromNumber = TrackingWhsOrder.FromPKFilteredByContact(Factory, TestOrder.WhsOrder.PK, null);
			AssertNull(testOrderFromNumber);
		}

		#endregion TestFromNumber

		#region TestCanCancel

		public void TestCanCancel()
		{
			TrackingWhsOrder testOrder = GetNewBusinessObject();

			testOrder.WhsOrder.WD_DocketStatus = DocketStatus.Codes.Entered;
			AssertEquals("CanCancel should be true for Entered Dockets", true, testOrder.CanCancelDocket);

			testOrder.WhsOrder.WD_DocketStatus = DocketStatus.Codes.Finalised;
			AssertEquals("CanCancel should be false for Finalised Dockets", false, testOrder.CanCancelDocket);
		}

		#endregion

		#region TestCanEdit

		public void TestCanEdit()
		{
			var testOrder = GetNewBusinessObject();

			testOrder.WhsOrder.WD_DocketStatus = DocketStatus.Codes.Entered;
			AssertEquals("CanEdit should be true for Entered Dockets", true, testOrder.CanEdit);

			testOrder.WhsOrder.WD_DocketStatus = DocketStatus.Codes.Finalised;
			AssertEquals("CanEdit should be false for Finalised Dockets", false, testOrder.CanEdit);

			testOrder.WhsOrder.WD_DocketStatus = DocketStatus.Codes.New;
			AssertEquals("CanEdit should be true for New Dockets", true, testOrder.CanEdit);

			testOrder.WhsOrder.WD_DocketStatus = DocketStatus.Codes.Picking;
			AssertEquals("CanEdit should be false for Picking Dockets", false, testOrder.CanEdit);

			testOrder.WhsOrder.WD_DocketStatus = DocketStatus.Codes.Putaway;
			AssertEquals("CanEdit should be false for Putaway Dockets", false, testOrder.CanEdit);
		}

		#endregion

		#region TestTrackingRequiredDate

		[TestDate(2024, 08, 6, 12, 15, 0)]
		public void TestTrackingRequiredDate()
		{
			var testOrder = GetNewBusinessObject();
			AssertEquals("Initially TrackingRequiredDate should be empty", ZDateTime.Empty, testOrder.TrackingRequiredDate);

			testOrder.TrackingRequiredDate = ZDateTime.Now;
			AssertEquals("Setting TrackingRequiredDate to a date should set time to end of day", new ZDateTime(2024, 08, 6, 23, 59, 0), testOrder.TrackingRequiredDate);

			testOrder.TrackingRequiredDate = ZDateTime.Empty;
			AssertEquals("Setting TrackingRequiredDate to empty should clear value", ZDateTime.Empty, testOrder.TrackingRequiredDate);
			AssertEquals("Setting TrackingRequiredDate should set RequiredDate", ZDateTime.Empty, testOrder.WhsOrder.RequiredDate);

			testOrder.TrackingRequiredDate = ZDateTime.Invalid;
			AssertEquals("Setting TrackingRequiredDate to invalid should clear value", ZDateTime.Empty, testOrder.TrackingRequiredDate);
			AssertEquals("Setting TrackingRequiredDate should set RequiredDate", ZDateTime.Empty, testOrder.WhsOrder.RequiredDate);

			testOrder.TrackingRequiredDate = ZDateTime.Now;
			AssertEquals("Setting TrackingRequiredDate to a date should set time to end of day", new ZDateTime(2024, 08, 6, 23, 59, 0), testOrder.TrackingRequiredDate);
			AssertEquals("Setting TrackingRequiredDate should set RequiredDate", new ZDateTime(2024, 08, 6, 23, 59, 0), testOrder.WhsOrder.RequiredDate);

			testOrder.TrackingRequiredDate = new ZDateTime(2024, 07, 15);
			AssertEquals("Setting TrackingRequiredDate to a date should set time to end of day", new ZDateTime(2024, 07, 15, 23, 59, 0), testOrder.TrackingRequiredDate);
			AssertEquals("Setting TrackingRequiredDate should set RequiredDate", new ZDateTime(2024, 07, 15, 23, 59, 0), testOrder.WhsOrder.RequiredDate);
		}

		#endregion

		#region TestWD_TransportCoUrl

		public void TestWD_TransportCoUrl()
		{
			OrgHeader transportCo = Factory.New<OrgHeader>();
			OrgWebURL orgUrl = transportCo.OrgWebURLs.AddNew(OrgWebUrlList.Codes.CartageTracking);

			TestOrder.WhsOrder.WD_TransportReference = "123";
			TestOrder.WhsOrder.TransportCoDocAddress.OrganisationPK = transportCo.PK;
			orgUrl.PU_URL = "www.blah.com?ref=(*CargoWiseREF*)";
			AssertEquals("www.blah.com?ref=123", TestOrder.WD_TransportCoUrl);

			orgUrl.PU_URL = "www.blah.com?ref=(*CargoWiseBadREF*)";
			AssertEquals("", TestOrder.WD_TransportCoUrl);

			orgUrl.PU_URL = "";
			AssertEquals("", TestOrder.WD_TransportCoUrl);

			orgUrl.PU_URL = "www.blah.com?ref=(*CargoWiseREF*)";
			TestOrder.WhsOrder.WD_TransportReference = "";
			AssertEquals("", TestOrder.WD_TransportCoUrl);

			TestOrder.WhsOrder.WD_TransportReference = "123";
			TestOrder.WhsOrder.TransportCoDocAddress.OrganisationPK = ZGuid.Empty;
			AssertEquals("", TestOrder.WD_TransportCoUrl);
		}

		#endregion

		#region TestGenerateReferenceDetailsForEmailReporting

		public void TestGenerateReferenceDetailsForEmailReporting()
		{
			WhsDocketReference reference = Factory.New<WhsDocketReference>();

			reference.WX_RefType = "CAN";
			reference.WX_Reference = "Reference Text";

			ZString expectedString = string.Format("Ref Type: Customs Approval Number{0}Reference: Reference Text", System.Environment.NewLine);

			AssertEquals("Generate Reference Details", expectedString, TestOrder.GenerateReferenceDetailsForEmailReporting(reference));
		}

		#endregion

		#region TestGenerateOrderLineDetailsForEmailReporting

		public void TestGenerateOrderLineDetailsForEmailReporting()
		{
			var line = TrackingHelper.Get(Factory.NewWithValidTestData<WhsOrderLine>()).WhsOrderLine;

			line.SupplierPart.OP_PartNum = "PARTNUM";
			line.WE_PackQuantity = 1;
			line.WE_F3_NKPackType = "UNT";
			line.WE_TransactionQuantity = 1;
			line.WE_PartAttrib1 = "one";
			line.WE_PartAttrib2 = "two";
			line.WE_PartAttrib3 = "three";
			line.WE_SerialNumber = "SN1";

			var expectedString = $@"Product: PARTNUM
Packs: 1
Packs UQ: UNT
Quantity: 1
{line.Docket.Client.PartAttributeManager.PartAttributeName1}: one
{line.Docket.Client.PartAttributeManager.PartAttributeName2}: two
{line.Docket.Client.PartAttributeManager.PartAttributeName3}: three
Serial Number: SN1";

			AssertEquals("Generate Reference Details", expectedString, TestOrder.GenerateOrderLineDetailsForEmailReporting(line));
		}

		#endregion

		#region TestFinaliseDocket_DBHits

		protected override Dictionary<string, int> TestFinaliseDocket_DBHits_ExpectedDBHitsForValidation
		{
			get
			{
				var expectedDBHits = base.TestFinaliseDocket_DBHits_ExpectedDBHitsForValidation;

				expectedDBHits.Remove(OrgCompanyDataSchema.Constants.TableName);

				return expectedDBHits;
			}
		}

		#endregion

		#region TestFinaliseDocket_DBHits_WithOrders_ExpectedDBHitsForValidation

		protected override Dictionary<string, int> TestFinaliseDocket_DBHits_WithOrders_ExpectedDBHitsForValidation
		{
			get
			{
				var expectedDBHits = base.TestFinaliseDocket_DBHits_WithOrders_ExpectedDBHitsForValidation;

				expectedDBHits.Remove(OrgCompanyDataSchema.Constants.TableName);

				return expectedDBHits;
			}
		}

		#endregion

		#region TestFinaliseDocket_DBHits_ExpectedDBHitsForValidation_WithPackageAudit

		protected override Dictionary<string, int> TestFinaliseDocket_DBHits_ExpectedDBHitsForValidation_WithPackageAudit
		{
			get
			{
				var expectedDBHits = base.TestFinaliseDocket_DBHits_ExpectedDBHitsForValidation_WithPackageAudit;

				expectedDBHits.Remove(OrgCompanyDataSchema.Constants.TableName);

				return expectedDBHits;
			}
		}

		#endregion

		#region TestValidatesWhsOrder

		public void TestValidatesWhsOrder()
		{
			var testWhsOrder = Factory.New<WhsOrder>();
			var testTrackingWhsOrder = TrackingHelper.Get(testWhsOrder);

			testTrackingWhsOrder.RunPreSaveValidation();

			Assert("Precondition", testWhsOrder.HasErrors);
			AssertSequencesEqual("TrackingWhsOrder should have all WhsOrder errors", testWhsOrder.RowErrors, testTrackingWhsOrder.RowErrors);
		}

		#endregion

		public void TestGetWrappedBizO()
		{
			var wrappedBizO = TestOrder.GetWrappedBizO();

			AssertEquals(TestOrder.WhsOrder, wrappedBizO);
		}

		public void TestGetWrappedBindTo()
		{
			var wrappedBindTo = TestOrder.GetWrappedBindTo("Lookups");

			AssertEquals("WhsOrder+Lookups", wrappedBindTo);
		}

		#region Implementation

		TrackingWhsOrder testOrder;
		TrackingWhsOrder TestOrder
		{
			get { return testOrder ?? (testOrder = TrackingHelper.Get(Order)); }
		}

		TestHelper WebHelper
		{
			get
			{
				if (fHelper == null)
				{
					fHelper = new TestHelper(Factory);
					Factory.Save();
				}
				return fHelper;
			}
		}

		TestHelper fHelper;

		#region Test Setup

		bool oldIsWeb;

		protected override void SetUp()
		{
			base.SetUp();
			oldIsWeb = Globals.IsWeb;
			Globals.IsWeb = true;
		}

		protected override void TearDown()
		{
			Globals.IsWeb = oldIsWeb;
			base.TearDown();
		}

		#endregion

		#endregion
	}
}
