using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.TransportBookings.Business;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transactions.Business.Testing;
using Enterprise.Warehouse.Transactions.CodeLists;
using Enterprise.ZArchitecture.Web.Business;
using Enterprise.ZArchitecture.Web.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Tracking.Business.Testing
{
	[TestedType(typeof(TrackingWhsReceive))]
	[SetGlobalsIsWeb]
	[HttpContextEnabledTest]
	sealed class TrackingWhsReceiveTest : NonPersistentBusinessObjectTestCase
	{
		#region Milestones

		public void TestMilestones()
		{
			var testWhsReceive = TrackingHelper.Get(Factory.NewWithValidTestData<WhsReceive>());
			AssertNotNull(testWhsReceive.Milestones);
			AssertEquals(0, testWhsReceive.Milestones.Count);

			var milestone1 = testWhsReceive.WhsReceive.WorkflowItems.Milestones.AddNew();
			milestone1.P9_Description = "lastCompleted1";
			milestone1.P9_IsPublished = true;
			milestone1.SetMilestoneActualDateForTest(DateTime.Now);
			milestone1.P9_Sequence = 1;

			var milestone2 = testWhsReceive.WhsReceive.WorkflowItems.Milestones.AddNew();
			milestone2.P9_Description = "lastCompleted2";
			milestone2.P9_IsPublished = true;
			milestone2.SetMilestoneActualDateForTest(DateTime.Now);
			milestone2.P9_Sequence = 1;
			testWhsReceive.Factory.Save();
			AssertEquals(0, testWhsReceive.Milestones.Count);

			testWhsReceive.ReloadMilestones();
			AssertEquals(2, testWhsReceive.Milestones.Count);
		}

		#endregion

		#region AdditionalInformationFieldsTest

		public void TestGetAdditionalInformationFields()
		{
			var testOrg = Factory.New<OrgHeader>();
			testOrg.OH_FullName = "Test organization";
			testOrg.OH_Code = "TST";

			var orgCustomLabel1 = testOrg.CustomLabels.AddNew();
			orgCustomLabel1.OT_FieldName = "WhsDocket.CustomAttrib1";
			orgCustomLabel1.OT_Caption = "Custom Attribute 1 Test";

			var orgCustomLabel2 = testOrg.CustomLabels.AddNew();
			orgCustomLabel2.OT_FieldName = "WhsDocket.CustomFlag1";
			orgCustomLabel2.OT_Caption = "Custom Flag 1 Test";

			AssertEquals("There must be 2 custom labels (created for testing).", 2, testOrg.CustomLabels.Count);

			TestReceive.WhsReceive.WD_OH_Client = testOrg.PK;
			var customLabels = TestReceive.GetAdditionalInformationFields();

			AssertEquals("All 2 custom labels must be retrieved.", 2, customLabels.Count);

			Assert("All custom labels created must be retrieved.", customLabels.Contains(AutoWhsDocket.Schema.WD_CustomAttrib1));
			var customLabel1 = (CustomLabelInfo)customLabels.GetFieldByPropertyName(AutoWhsDocket.Schema.WD_CustomAttrib1);
			AssertEquals("Custom label properties retrieved must be the same as those set.", orgCustomLabel1.OT_Caption, customLabel1.Caption);
			AssertEquals("Custom label properties retrieved must be the same as those set.", orgCustomLabel1.OT_FieldName, customLabel1.LabelName);

			Assert("All custom labels created must be retrieved.", customLabels.Contains(AutoWhsDocket.Schema.WD_CustomFlag1));
			var customLabel2 = (CustomLabelInfo)customLabels.GetFieldByPropertyName(AutoWhsDocket.Schema.WD_CustomFlag1);
			AssertEquals("Custom label properties retrieved must be the same as those set.", orgCustomLabel2.OT_Caption, customLabel2.Caption);
			AssertEquals("Custom label properties retrieved must be the same as those set.", orgCustomLabel2.OT_FieldName, customLabel2.LabelName);
		}

		#endregion

		#region TestNotificationOptions

		public void TestNotificationOptions()
		{
			AssertEquals(WebDataRegistry.Instance.WarehouseReceiptsNotificationOptions, ((IBizOChangesEmailNotification)TestReceive).NotificationSendingRule);
		}

		#endregion

		#region TestWarehousesAreOnlyActive

		public void TestWarehousesAreOnlyActive()
		{
			TestHelper helper = new TestHelper(Factory);
			helper.TestSiteUser.Login(helper.TestOrg.OH_Code, helper.TestContact.OC_Email, helper.TestContact.PasswordForTesting);

			var whs1 = GetWarehouse(helper.TestSiteUser.LoggedInOrganisation.PK);
			var whs2 = GetWarehouse(helper.TestSiteUser.LoggedInOrganisation.PK);

			whs1.WW_IsActive = true;
			whs2.WW_IsActive = false;
			Factory.Save();
			var receive = TrackingHelper.Get(Factory.NewWithValidTestData<WhsReceive>());
			receive.WhsReceive.Lookups.Warehouses.Load();
			Assert("Active warehouse is in collection", receive.WhsReceive.Lookups.Warehouses.Contains(whs1));
			Assert("Inactive warehouse is not in collection", !receive.WhsReceive.Lookups.Warehouses.Contains(whs2));
		}

		WhsWarehouse GetWarehouse(ZGuid orgPK)
		{
			WhsWarehouse warehouse = Factory.NewWithValidTestData<WhsWarehouse>();

			WhsReceive docket = Factory.NewWithValidTestData<WhsReceive>();
			docket.WD_WW_Whs = warehouse.PK;
			docket.WD_OH_Client = orgPK;

			return warehouse;
		}

		#endregion

		#region Overrides

		protected override BusinessObject GetNewBusinessObject()
		{
			return TrackingHelper.Get(Factory.New<WhsReceive>());
		}

		#endregion

		#region TestRelatedTransportBookingPKsAdded

		public void TestRelatedTransportBookingPKsAdded()
		{
			var trackingWhsReceive = TrackingHelper.Get(Factory.NewWithValidTestData<WhsReceive>());
			var whsReceive = trackingWhsReceive.WhsReceive;

			AssertEquals("Precondition: DocRelatedPKs should return no PKs at this stage.", 0, trackingWhsReceive.DocRelatedPKs.Count);

			var consolidation = Factory.NewWithValidTestData<DtbBookingConsolidation>();
			consolidation.KB_ParentID = whsReceive.PK;
			consolidation.KB_ParentTableCode = whsReceive.TablePrefix;

			var booking1 = Factory.NewWithValidTestData<DtbBooking>();
			var booking2 = Factory.NewWithValidTestData<DtbBooking>();
			var booking3 = Factory.NewWithValidTestData<DtbBooking>();

			consolidation.Bookings.AddRange(new List<DtbBooking> { booking1, booking2, booking3 });

			Factory.Save();

			AssertEquals("DocRelatedPKs should return 3 PKs.", 3, trackingWhsReceive.DocRelatedPKs.Count);
			AssertContainsExactElementsInAnyOrder("DocRelatedPKs should return all 3 booking PKs", new[] { booking1.PK, booking2.PK, booking3.PK }, trackingWhsReceive.DocRelatedPKs);
		}

		#endregion

		#region TestRelatedForwardingShipmentPKAdded()

		public void TestRelatedForwardingShipmentPKAdded()
		{
			var trackingWhsReceive = TrackingHelper.Get(Factory.NewWithValidTestData<WhsReceive>());
			AssertEquals(0, trackingWhsReceive.DocRelatedPKs.Count);

			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();

			var receiveJobPivot = Factory.NewWithValidTestData<WhsDocketJobPivot>();
			receiveJobPivot.WV_DocketType = trackingWhsReceive.WhsReceive.WD_DocketType;
			receiveJobPivot.WV_WD_Docket = trackingWhsReceive.WhsReceive.PK;
			receiveJobPivot.WV_ParentTableCode = shipment.TablePrefix;
			receiveJobPivot.WV_ParentId = shipment.PK;

			Factory.Save();

			AssertEquals(1, trackingWhsReceive.DocRelatedPKs.Count);
			AssertContainsExactElementsInAnyOrder(new[] { shipment.PK }, trackingWhsReceive.DocRelatedPKs);
		}

		#endregion

		#region TestFromNumber

		public void TestFromPK()
		{
			AssertNotNull(WebHelper.TestSiteUser);
			Factory.Save();

			WebHelper.TestSiteUser.Login(WebHelper.TestOrg.OH_Code, WebHelper.TestContact.OC_Email, WebHelper.TestContact.PasswordForTesting);
			TestReceive.SiteUser = WebHelper.TestSiteUser;

			TestReceive.WhsReceive.WD_OH_Client = WebHelper.TestOrg.PK;

			TestReceive.WhsReceive.WD_WW_Whs = Helper.CreateWarehouse("WHS1").PK;
			Factory.Save();

			var testReceiveFromPK = TrackingWhsReceive.FromPKFilteredByContact(Factory, TestReceive.WhsReceive.PK, WebHelper.TestSiteUser);
			AssertEquals(TestReceive, testReceiveFromPK);
			AssertEquals("LoggedInContact", WebHelper.TestContact.PK, TestReceive.LoggedInContact.PK);
			AssertEquals("Order AutoCreatedLogDefaultSL_Reference", WebHelper.TestSiteUser.ContactAndCompanyReference, TestReceive.Logs.AutoCreatedLogDefaultSL_Reference);
		}

		public void TestFromNumber()
		{
			AssertNotNull(WebHelper.TestContact);
			Factory.Save();

			WebHelper.TestSiteUser.Login(WebHelper.TestOrg.OH_Code, WebHelper.TestContact.OC_Email, WebHelper.TestContact.PasswordForTesting);
			TestReceive.SiteUser = WebHelper.TestSiteUser;

			TestReceive.WhsReceive.WD_OH_Client = WebHelper.TestOrg.PK;
			TestReceive.WhsReceive.WD_ExternalReference = "REF1234567890";
			TestReceive.WhsReceive.WD_WW_Whs = Helper.CreateWarehouse("WHS1").PK;
			Factory.Save();

			var testReceiveFromNumber = TrackingWhsReceive.FromNumberFilteredByContact(Factory, TestReceive.WhsReceive.WD_ExternalReference, WebHelper.TestSiteUser);
			AssertEquals(TestReceive, testReceiveFromNumber);
			AssertEquals("LoggedInContact", WebHelper.TestContact.PK, TestReceive.LoggedInContact.PK);
			AssertEquals("Order AutoCreatedLogDefaultSL_Reference", WebHelper.TestSiteUser.ContactAndCompanyReference, TestReceive.Logs.AutoCreatedLogDefaultSL_Reference);
		}

		public void TestFromNumberIncorrectOrg()
		{
			AssertNotNull(WebHelper.TestContact);
			Factory.Save();

			WebHelper.TestSiteUser.Login(WebHelper.TestOrg.OH_Code, WebHelper.TestContact.OC_Email, WebHelper.TestContact.PasswordForTesting);
			TestReceive.SiteUser = WebHelper.TestSiteUser;

			TestReceive.WhsReceive.WD_OH_Client = WebHelper.TestOrg.PK;

			TestReceive.WhsReceive.WD_WW_Whs = Helper.CreateWarehouse("WHS1").PK;
			Factory.Save();
			WebEnv.AppInstance.SiteUser.Logout();
			TrackingWhsReceive testReceiveFromNumber = TrackingWhsReceive.FromPKFilteredByContact(Factory, TestReceive.PK, null);
			AssertNull(testReceiveFromNumber);
		}

		#endregion TestFromNumber

		#region TestCanCancel

		public void TestCanCancel()
		{
			var testReceive = (TrackingWhsReceive)GetNewBusinessObject();

			testReceive.WhsReceive.WD_DocketStatus = DocketStatus.Codes.Entered;
			AssertEquals("CanCancel should be true for Entered Dockets", true, testReceive.CanCancelDocket);

			testReceive.WhsReceive.WD_DocketStatus = DocketStatus.Codes.Finalised;
			AssertEquals("CanCancel should be false for Finalised Dockets", false, testReceive.CanCancelDocket);
		}

		#endregion

		#region TestCanEdit

		public void TestCanEdit()
		{
			TrackingWhsReceive testReceive = (TrackingWhsReceive)GetNewBusinessObject();

			testReceive.WhsReceive.WD_DocketStatus = DocketStatus.Codes.Entered;
			AssertEquals("CanEdit should be true for Entered Dockets", true, testReceive.CanEdit);

			testReceive.WhsReceive.WD_DocketStatus = DocketStatus.Codes.Finalised;
			AssertEquals("CanEdit should be false for Finalised Dockets", false, testReceive.CanEdit);

			testReceive.WhsReceive.WD_DocketStatus = DocketStatus.Codes.New;
			AssertEquals("CanEdit should be true for New Dockets", true, testReceive.CanEdit);

			testReceive.WhsReceive.WD_DocketStatus = DocketStatus.Codes.Picking;
			AssertEquals("CanEdit should be false for Picking Dockets", false, testReceive.CanEdit);

			testReceive.WhsReceive.WD_DocketStatus = DocketStatus.Codes.Putaway;
			AssertEquals("CanEdit should be false for Putaway Dockets", false, testReceive.CanEdit);
		}
		#endregion

		#region TestGenerateInventoryDetailsForEmailReporting

		public void TestGenerateInventoryDetailsForEmailReporting()
		{
			var testReceive = (TrackingWhsReceive)GetNewBusinessObject();
			var receiveLine = TrackingHelper.Get(Factory.NewWithValidTestData<WhsReceiveLine>());

			receiveLine.WhsReceiveLine.SupplierPart.OP_PartNum = "PARTNUM";
			receiveLine.WhsReceiveLine.WE_PackQuantity = 1;
			receiveLine.WhsReceiveLine.WE_F3_NKPackType = "UNT";
			receiveLine.WhsReceiveLine.WE_StockOnHand = 1;
			receiveLine.WhsReceiveLine.WE_PartAttrib1 = "one";
			receiveLine.WhsReceiveLine.WE_PartAttrib2 = "two";
			receiveLine.WhsReceiveLine.WE_PartAttrib3 = "three";
			receiveLine.WhsReceiveLine.WE_SerialNumber = "SN1";

			var expectedString = $@"Product: PARTNUM
Packs: 1
Packs UQ: UNT
Quantity: 1
{receiveLine.Docket.WhsReceive.Client.PartAttributeManager.PartAttributeName1}: one
{receiveLine.Docket.WhsReceive.Client.PartAttributeManager.PartAttributeName2}: two
{receiveLine.Docket.WhsReceive.Client.PartAttributeManager.PartAttributeName3}: three
Serial Number: SN1";

			AssertEquals("Generate Reference Details", expectedString, testReceive.GenerateInventoryDetailsForEmailReporting(receiveLine.WhsReceiveLine));
		}

		#endregion

		#region TestGenerateContainerDetailsForEmailReporting

		public void TestGenerateContainerDetailsForEmailReporting()
		{
			TrackingWhsReceive testReceive = (TrackingWhsReceive)GetNewBusinessObject();
			WhsDocketContainer container = Factory.NewWithValidTestData<WhsDocketContainer>();
			RefContainer rc = Factory.New<RefContainer>();
			container.WC_RC = rc.PK;

			container.WC_ContainerNum = "ABC123";
			container.WC_SealNum = "DEF456";
			rc.RC_ContainerType = "001";
			container.WC_IsPalletised = true;
			container.WC_IsChargeable = true;
			container.WC_ItemCount = 3;
			container.WC_PalletCount = 3;

			ZString expectedString = string.Format("Container #: ABC123{0}Seal #: DEF456{0}Type: 001{0}Palletized: yes{0}Chargeable: yes{0}Items: 3{0}Pallets: 3", System.Environment.NewLine);

			AssertEquals("Generate Reference Details", expectedString, testReceive.GenerateContainerDetailsForEmailReporting(container));
		}

		#endregion

		#region TestInventoryCollectionOrderedByLineNo

		public void TestInventoryCollectionOrderedByLineNo()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			var inventory1 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 1m);
			var inventory2 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 1m);
			var inventory3 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 1m);
			inventory2.WI_LineNo = 1;
			inventory3.WI_LineNo = 2;
			inventory1.WI_LineNo = 4;
			Factory.Save();

			var trackingReceive = TrackingHelper.Get(receive);
			AssertEquals("Precondition", 3, trackingReceive.Lines.Count);
			AssertEquals("Precondition", 3, trackingReceive.Inventory.Count);
			AssertEquals("Inventory 2 should be returned first", inventory2.PK, trackingReceive.Inventory[0].PK);
			AssertEquals("Inventory 3 should be returned second", inventory3.PK, trackingReceive.Inventory[1].PK);
			AssertEquals("Inventory 1 should be returned third", inventory1.PK, trackingReceive.Inventory[2].PK);
			AssertEquals("Receive Line 2 should be returned first", inventory2.InDocketLine.PK, trackingReceive.Lines[0].WhsReceiveLine.PK);
			AssertEquals("Receive Line 3 should be returned second", inventory3.InDocketLine.PK, trackingReceive.Lines[1].WhsReceiveLine.PK);
			AssertEquals("Receive Line 1 should be returned third", inventory1.InDocketLine.PK, trackingReceive.Lines[2].WhsReceiveLine.PK);
		}

		#endregion

		public void TestClonable()
		{
			var trackingReceive = TrackingHelper.Get(Factory.New<WhsReceive>());
			Assert(trackingReceive.SupportsClone());

			trackingReceive.WhsReceive.WD_ExternalReference = "1";
			trackingReceive.WhsReceive.Containers.AddNew().WC_ContainerNum = "1";
			trackingReceive.WhsReceive.Containers.AddNew().WC_ContainerNum = "2";

			CombineAssertions(() =>
			{
				AssertEquals("Precondition", "1", trackingReceive.WhsReceive.WD_ExternalReference);
				AssertEquals("Precondition", 2, trackingReceive.WhsReceive.Containers.Count);
			});

			var clone = (TrackingWhsReceive)trackingReceive.Clone();

			CombineAssertions(() =>
			{
				AssertEquals("WhsReceive properties should be cloned", "1", clone.WhsReceive.WD_ExternalReference);
				AssertEquals("WhsReceive properties should be cloned", 2, clone.WhsReceive.Containers.Count);
			});
		}

		public void TestGetWrappedBizO()
		{
			var trackingReceive = TrackingHelper.Get(Factory.New<WhsReceive>());
			var wrappedBizO = trackingReceive.GetWrappedBizO();

			AssertEquals(trackingReceive.WhsReceive, wrappedBizO);
		}

		public void TestGetWrappedBindTo()
		{
			var trackingReceive = TrackingHelper.Get(Factory.New<WhsReceive>());
			var wrappedBindTo = trackingReceive.GetWrappedBindTo("Lookups");

			AssertEquals("WhsReceive+Lookups", wrappedBindTo);
		}

		#region TestHumanReadableName

		public void TestHumanReadableName()
		{
			var testReceive = (TrackingWhsReceive)GetNewBusinessObject();
			testReceive.WhsReceive.WD_DocketID = "123f";
			var humanReadable = testReceive.HumanReadableName;

			AssertEquals(new ZString("Warehouse Receive 123f"), humanReadable);
		}

		#endregion

		#region Implementation

		TrackingWhsReceive testReceive;
		TrackingWhsReceive TestReceive
		{
			get { return testReceive ?? (testReceive = (TrackingWhsReceive)GetNewBusinessObject()); }
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

		WhsTestHelperFunctions Helper
		{
			get { return helper ?? (helper = new WhsTestHelperFunctions(Factory)); }
		}

		WhsTestHelperFunctions helper;

		#endregion
	}
}
