using System;
using System.Data;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Forwarding.Registry;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using Constants = Enterprise.Core.Constants;

namespace Enterprise.Freight.Forwarding.Orders.Business.Testing
{
	[TestedType(typeof(JobSupplierBooking))]
	sealed class JobSupplierBookingTest : EnterpriseBusinessObjectTestCase
	{
		protected override Type ExpectedMetadataType => typeof(Metadata.Business.JobSupplierBooking);

		#region ICustomFieldProvider

		public void TestICustomFieldProvider()
		{
			var customFieldProvider = supplierBooking as ICustomFieldProvider;
			AssertNotNull(customFieldProvider);

			var customBusinessObject = customFieldProvider.GetCustomBusinessObject();
			AssertNotNull(customBusinessObject);
		}

		public void TestICustomFieldProviderForCustomLabels()
		{
			var registryValue = WorkflowDataRegistry.Instance.ClientInTemplateSelection.Value;
			registryValue = (ClientInTemplateSelectionCriteriaCollection)registryValue.Clone(null, null);
			var criteria = registryValue.AddNew();
			criteria.ProcessTaskCode = (supplierBooking as IWorkflowProvider).WorkflowType;
			criteria.SelectedItems.AddNew().OrgTypeCode = Enterprise.Registry.Business.ClientInTemplateSelectionOrgTypeList.Codes.ControllingCustomer;
			using (WorkflowDataRegistry.Instance.ClientInTemplateSelection.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, registryValue))
			{
				var controllingCustomer = Factory.NewWithValidTestData<OrgHeader>();
				supplierBooking.ControllingCustomerAddress.OrganisationPK = controllingCustomer.PK;
				supplierBooking.ControllingCustomerAddress.E2_OA_Address = controllingCustomer.MainAddress.PK;
				supplierBooking.ControllingCustomerAddress.E2_AddressOverride = true;

				var defaultMISCOrg = Factory.Load<OrgHeader>(new ZGuid("79de2ecd-1bb0-40fe-a02f-f3c16f9c3686"));
				var label1 = defaultMISCOrg.ConfigOrg.CustomLabels.AddNew();
				label1.OT_Type = "FR";
				label1.OT_Caption = OrgMiscServ.Schema.OM_CustomAttrib1;
				label1.OT_FieldName = "Organisation.CustomAttribute1";
				var customFieldProvider = supplierBooking as ICustomFieldProvider;
				AssertNotNull(customFieldProvider);

				var customBusinessObject = customFieldProvider.GetCustomBusinessObject();
				AssertNotNull(customBusinessObject);
			}
		}

		#endregion

		#region BookingId

		public void TestBookingIdWithDefault()
		{
			var booking = Factory.NewWithValidTestData<JobSupplierBooking>();
			booking.JSB_BookingId = ZString.Empty;

			AssertEquals(ZString.Empty, booking.JSB_BookingId);

			Factory.Save();

			AssertNotEquals(ZString.Empty, booking.JSB_BookingId);
			Assert(booking.JSB_BookingId.StartsWith("SB"));
		}

		public void TestBookingIdWithNumberCustomisation()
		{
			var customisation = new BillOfLadingNumberCustomisation();
			customisation.RemoveFountainPrefix = true;
			customisation.UnFilteredElements[BillOfLadingNumberCustomisationElement.Keys.SequenceNumber].CheckDigit = true;
			customisation.UnFilteredElements[BillOfLadingNumberCustomisationElement.Keys.SequenceNumber].Include = true;
			customisation.UnFilteredElements[BillOfLadingNumberCustomisationElement.Keys.SequenceNumber].Detail = "10";
			customisation.UnFilteredElements[BillOfLadingNumberCustomisationElement.Keys.SequenceNumber].Order = 0;
			customisation.UnFilteredElements[BillOfLadingNumberCustomisationElement.Keys.ClientCoded1].Include = true;
			customisation.UnFilteredElements[BillOfLadingNumberCustomisationElement.Keys.ClientCoded1].Detail = "XYZ";
			customisation.UnFilteredElements[BillOfLadingNumberCustomisationElement.Keys.ClientCoded1].Order = 1;

			using (OrderManagerRegistry.Instance.SupplierBookingNumberFormat.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, customisation))
			{
				var booking = Factory.NewWithValidTestData<JobSupplierBooking>();
				booking.JSB_BookingId = ZString.Empty;

				AssertEquals(ZString.Empty, booking.JSB_BookingId);

				Factory.Save();

				AssertNotEquals(ZString.Empty, booking.JSB_BookingId);
				Assert(booking.JSB_BookingId.StartsWith("XYZ"));
				AssertEquals(13, booking.JSB_BookingId.Length);
			}
		}

		#endregion

		#region JSB_DetailedGoodsDescription

		public void TestJSB_DetailedGoodsDescription()
		{
			var booking = Factory.NewWithValidTestData<JobSupplierBooking>();
			AssertEquals("Precondition: No note on the booking by default", 0, booking.Notes.GetAllNotes().Count);

			booking.JSB_DetailedGoodsDescription = "How you doing!";
			AssertEquals("Setting Note Description created a new note: Notes.Count", 1, booking.Notes.GetAllNotes().Count);
			AssertEquals("Setting Note Description created a new note: Note.Description", "Detailed Goods Description", booking.Notes.GetAllNotes().Cast<StmNote>().First().ST_Description);
			AssertEquals("Setting Note Description created a new note: Note.NoteText", "How you doing!", booking.Notes.GetAllNotes().Cast<StmNote>().First().ST_NoteText);
			AssertEquals("Can retrieve note text from JSB_DetailedGoodsDescription after creating note", "How you doing!", booking.JSB_DetailedGoodsDescription);

			booking.JSB_DetailedGoodsDescription = "I'm great thanks!";
			AssertEquals("Resetting Note Description updated existing note: Notes.Count", 1, booking.Notes.GetAllNotes().Count);
			AssertEquals("Resetting Note Description updated existing note: Note.Description", "Detailed Goods Description", booking.Notes.GetAllNotes().Cast<StmNote>().First().ST_Description);
			AssertEquals("Resetting Note Description updated existing note: Note.NoteText", "I'm great thanks!", booking.Notes.GetAllNotes().Cast<StmNote>().First().ST_NoteText);
			AssertEquals("Can retrieve note text from JSB_DetailedGoodsDescription after reseting it", "I'm great thanks!", booking.JSB_DetailedGoodsDescription);

			booking.Notes.RemoveAndDeleteAll();
			AssertEquals("accessing note text works after deleting note", ZString.Empty, booking.JSB_DetailedGoodsDescription);
			AssertEquals("no new note was created after accessing note text", 0, booking.Notes.GetAllNotes().Count);
		}

		#endregion

		#region custom total property

		public void TestCustomTotalProperty()
		{
			JobSupplierBooking booking = Factory.New<JobSupplierBooking>();
			AssertEquals((ZDecimal)0, booking.TotalVolume);
			AssertEquals((ZDecimal)0, booking.TotalWeight);
			AssertEquals((ZString)"M3", booking.TotalVolumeUnit);
			AssertEquals((ZString)"KG", booking.TotalWeightUnit);

			JobSupplierBookingLine bookingLine1 = booking.SupplierBookingLines.AddNew();
			JobSupplierBookingLine bookingLine2 = booking.SupplierBookingLines.AddNew();
			AssertEquals((ZDecimal)0, booking.TotalVolume);
			AssertEquals((ZDecimal)0, booking.TotalWeight);
			AssertEquals((ZString)"M3", booking.TotalVolumeUnit);
			AssertEquals((ZString)"KG", booking.TotalWeightUnit);

			bookingLine1.JSL_Volume = 12;
			bookingLine1.JSL_VolumeUnit = "CY";
			bookingLine1.JSL_GrossWeight = 11;
			bookingLine1.JSL_GrossWeightUnit = "G";

			bookingLine2.JSL_Volume = 12;
			bookingLine2.JSL_VolumeUnit = "CY";
			bookingLine2.JSL_GrossWeight = 111;
			bookingLine2.JSL_GrossWeightUnit = "G";

			AssertEquals((ZString)"CY", booking.TotalVolumeUnit);
			AssertEquals((ZString)"G", booking.TotalWeightUnit);
			AssertEquals((ZDecimal)24, booking.TotalVolume);
			AssertEquals((ZDecimal)122, booking.TotalWeight);

			JobSupplierBookingLine bookingLine3 = booking.SupplierBookingLines.AddNew();
			JobSupplierBookingLine bookingLine4 = booking.SupplierBookingLines.AddNew();
			bookingLine3.JSL_Volume = 12;
			bookingLine3.JSL_VolumeUnit = "M3";
			bookingLine3.JSL_GrossWeight = 11;
			bookingLine3.JSL_GrossWeightUnit = "KG";
			bookingLine4.JSL_Volume = 12;
			bookingLine4.JSL_VolumeUnit = "CY";
			bookingLine4.JSL_GrossWeight = 11;
			bookingLine4.JSL_GrossWeightUnit = "KG";

			AssertEquals((ZString)"M3", booking.TotalVolumeUnit);
			AssertEquals((ZString)"KG", booking.TotalWeightUnit);
			AssertEquals((ZDecimal)39.523974888, booking.TotalVolume);
			AssertEquals((ZDecimal)22.122, booking.TotalWeight);
		}

		#endregion

		public void TestIsNotIncompleteAndCancelled()
		{
			var booking = Factory.NewWithValidTestData<JobSupplierBooking>();
			booking.JSB_Status = Constants.SupplierBookingStatus.Cancelled;
			Assert(booking.IsIncompleteOrCancelled);

			booking.JSB_Status = Constants.SupplierBookingStatus.Incomplete;
			Assert(booking.IsIncompleteOrCancelled);

			booking.JSB_Status = Constants.SupplierBookingStatus.Rejected;
			Assert(!booking.IsIncompleteOrCancelled);
		}

		#region IDocAddresses

		public void TestSupportedAddressTypes()
		{
			AdvOrmFeatureHelper.RunTestWith(true, action: () =>
			{
				IDocAddresses booking = Factory.New<JobSupplierBooking>();
				AssertContainsExactElementsInAnyOrder(new[]
				{
					DocAddressType.ControllingCustomer,
					DocAddressType.SupplierDocumentaryAddress,
					DocAddressType.LocalCartageCFS,
					DocAddressType.NotifyParty,
					DocAddressType.NotifyParty2,
					DocAddressType.NotifyParty3,
					DocAddressType.ConsigneeDocumentaryAddress,
				}, booking.SupportedAddressTypes);
			});

			AdvOrmFeatureHelper.RunTestWith(false, action: () =>
			{
				IDocAddresses booking = Factory.New<JobSupplierBooking>();
				AssertContainsExactElementsInAnyOrder(new[]
				{
					DocAddressType.ControllingCustomer,
					DocAddressType.SupplierDocumentaryAddress,
					DocAddressType.LocalCartageCFS,
					DocAddressType.NotifyParty,
					DocAddressType.NotifyParty2,
					DocAddressType.NotifyParty3,
				}, booking.SupportedAddressTypes);
			});
		}

		#endregion

		public void TestLogEventForMismatchedShipmentWindow()
		{
			foreach (var shipmentWindowStartError in new bool[] { true, false })
			{
				foreach (var shipmentWindowEndError in new bool[] { true, false })
				{
					LogEvent(shipmentWindowStartError, shipmentWindowEndError);
				}
			}

			void LogEvent(bool shipmentWindowStartError, bool shipmentWindowEndError)
			{
				var booking = Factory.NewWithValidTestData<JobSupplierBooking>();
				booking.AddLogOnShipmentWindowDatesIfNeeded(shipmentWindowStartError, shipmentWindowEndError);

				var logs = booking.Logs.GetAllLogs();
				AssertEquals(shipmentWindowStartError ? 1 : 0, logs.Where(log => log.SL_SE_NKEvent == Events.ExceptionRaised.Code && log.SL_Reference.Equals("|RES=This Supplier Booking has booking lines whose Ship Window Start dates do not match with their order line dates.|TYP=Ship Window Start")).Count());
				AssertEquals(shipmentWindowEndError ? 1 : 0, logs.Where(log => log.SL_SE_NKEvent == Events.ExceptionRaised.Code && log.SL_Reference.Equals("|RES=This Supplier Booking has booking lines whose Ship Window End dates do not match with their order line dates.|TYP=Ship Window End")).Count());
			}
		}

		public void TestOrderShipmentPlannings()
		{
			var booking = Factory.NewWithValidTestData<JobSupplierBooking>();
			Factory.Save();
			AssertEquals(0, booking.OrderShipmentPlannings.Count);

			var shipmentPlanning = booking.OrderShipmentPlannings.AddNew();
			Factory.Save();
			AssertEquals(1, booking.OrderShipmentPlannings.Count);

			booking.Delete();
			Factory.Save();
			AssertEquals(true, shipmentPlanning.IsDeleted);
		}

		public void TestAddMismatchedPackingCompletedEvent()
		{
			var overhead = "|RFN=|TYP=Mismatched".Length;
			var maxReferenceLength = StmALogSchema.SL_Reference.MaxLength;
			supplierBooking.AddMismatchedPackingCompletedEvent(new[] { "JS002", "JS001" });

			AssertEquals("Multiple shipment IDs are comma separated",
				1,
				supplierBooking.Logs.GetAllLogs()
				.Where(log => log.SL_SE_NKEvent == "PKC" && log.SL_Reference == "|RFN=JS001,JS002|TYP=Mismatched")
				.Count());

			supplierBooking.AddMismatchedPackingCompletedEvent(new[] { new string('A', maxReferenceLength - overhead) });

			AssertEquals("Reference exact at the length limit is not truncated",
				1,
				supplierBooking.Logs.GetAllLogs()
				.Where(log => log.SL_SE_NKEvent == "PKC" && log.SL_Reference.Length == maxReferenceLength && log.SL_Reference.EndsWith("AAAA|TYP=Mismatched"))
				.Count());

			supplierBooking.AddMismatchedPackingCompletedEvent(new[] { new string('B', maxReferenceLength + 1) });

			AssertEquals(
				"Reference exceeding the length limit is truncated with ellipsis",
				1,
				supplierBooking.Logs.GetAllLogs()
				.Where(log => log.SL_SE_NKEvent == "PKC" && log.SL_Reference.Length == maxReferenceLength && log.SL_Reference.EndsWith("B...|TYP=Mismatched"))
				.Count());
		}

		#region IExternalRequestGenerationProvider

		public void TestGetRequestJobID()
		{
			var booking = Factory.New<JobSupplierBooking>();
			booking.JSB_BookingId = "S125";

			AssertEquals("S125", booking.GetRequestJobID());
		}

		public void TestGetRequestTypeCode()
		{
			var booking = Factory.New<JobSupplierBooking>();
			AssertEquals(ExternalRequestTypes.Codes.SupplierBooking, booking.GetRequestTypeCode());
		}

		public void TestGetRequestSupportedAddressInfo()
		{
			var assigneeOrg = Factory.NewWithValidTestData<OrgHeader>();
			assigneeOrg.Contacts.Add(Factory.NewWithValidTestData<OrgContact>());
			var reviewerOrg = Factory.NewWithValidTestData<OrgHeader>();
			reviewerOrg.Contacts.Add(Factory.NewWithValidTestData<OrgContact>());
			var controllingCustomerOrg = Factory.NewWithValidTestData<OrgHeader>();
			controllingCustomerOrg.Contacts.Add(Factory.NewWithValidTestData<OrgContact>());
			var manufactureOrg = Factory.NewWithValidTestData<OrgHeader>();
			manufactureOrg.Contacts.Add(Factory.NewWithValidTestData<OrgContact>());
			var bookingPartyOrg = Factory.NewWithValidTestData<OrgHeader>();
			Factory.Save();

			var order = Factory.NewWithValidTestData<Order>();
			order.JD_OrderNumber = "X125";
			order.JD_OA_BuyerAddress = assigneeOrg.MainAddress.PK;
			order.JD_OC_BuyerContact = assigneeOrg.Contacts[0].PK;

			var orderLine = Factory.NewWithValidTestData<OrderLine>();
			order.OrderLines.Add(orderLine);

			var booking = Factory.NewWithValidTestData<JobSupplierBooking>();
			OrderManagerTestHelper.CreateDocAddressAndFillWithAddressAndContact(booking, (DocAddressType.ControllingCustomer), controllingCustomerOrg.MainAddress.PK, controllingCustomerOrg.Contacts[0].OC_ContactName);
			OrderManagerTestHelper.CreateDocAddressAndFillWithAddressAndContact(booking, (DocAddressType.SupplierDocumentaryAddress), reviewerOrg.MainAddress.PK, reviewerOrg.Contacts[0].OC_ContactName);
			booking.JSB_OH_BookingParty = bookingPartyOrg.PK;

			var bookingLine = Factory.NewWithValidTestData<JobSupplierBookingLine>();
			bookingLine.JSL_JO_OrderLine = orderLine.PK;
			booking.SupplierBookingLines.Add(bookingLine);
			OrderManagerTestHelper.CreateDocAddressAndFillWithAddressAndContact(bookingLine, (DocAddressType.Manufacturer), manufactureOrg.MainAddress.PK, manufactureOrg.Contacts[0].OC_ContactName);
			Factory.Save();

			AssertEquals(assigneeOrg.PK, booking.GetRequestSupportedAddressInfo(DocAddressTypes.Codes.BuyerDocumentaryAddress).OrginzationPK);
			AssertEquals(assigneeOrg.Contacts[0].PK, booking.GetRequestSupportedAddressInfo(DocAddressTypes.Codes.BuyerDocumentaryAddress).ContactPK);
			AssertEquals(reviewerOrg.PK, booking.GetRequestSupportedAddressInfo(DocAddressTypes.Codes.SupplierDocumentaryAddress).OrginzationPK);
			AssertEquals(reviewerOrg.Contacts[0].PK, booking.GetRequestSupportedAddressInfo(DocAddressTypes.Codes.SupplierDocumentaryAddress).ContactPK);
			AssertEquals(controllingCustomerOrg.PK, booking.GetRequestSupportedAddressInfo(DocAddressTypes.Codes.ControllingCustomer).OrginzationPK);
			AssertEquals(controllingCustomerOrg.Contacts[0].PK, booking.GetRequestSupportedAddressInfo(DocAddressTypes.Codes.ControllingCustomer).ContactPK);
			AssertEquals(manufactureOrg.PK, booking.GetRequestSupportedAddressInfo(DocAddressTypes.Codes.Manufacturer).OrginzationPK);
			AssertEquals(manufactureOrg.Contacts[0].PK, booking.GetRequestSupportedAddressInfo(DocAddressTypes.Codes.Manufacturer).ContactPK);
			AssertEquals(bookingPartyOrg.PK, booking.GetRequestSupportedAddressInfo(DocAddressTypes.Codes.BookingPartyDocumentaryAddress).OrginzationPK);
			AssertEquals(ZGuid.Empty, booking.GetRequestSupportedAddressInfo(DocAddressTypes.Codes.BookingPartyDocumentaryAddress).ContactPK);
		}

		public void TestGetRequestSupportedAddressInfo_DifferentManufactures()
		{
			var assigneeOrg = Factory.NewWithValidTestData<OrgHeader>();
			assigneeOrg.Contacts.Add(Factory.NewWithValidTestData<OrgContact>());
			var reviewerOrg = Factory.NewWithValidTestData<OrgHeader>();
			reviewerOrg.Contacts.Add(Factory.NewWithValidTestData<OrgContact>());
			var controllingCustomerOrg = Factory.NewWithValidTestData<OrgHeader>();
			controllingCustomerOrg.Contacts.Add(Factory.NewWithValidTestData<OrgContact>());
			var manufactureOrg1 = Factory.NewWithValidTestData<OrgHeader>();
			manufactureOrg1.Contacts.Add(Factory.NewWithValidTestData<OrgContact>());
			var manufactureOrg2 = Factory.NewWithValidTestData<OrgHeader>();
			manufactureOrg2.Contacts.Add(Factory.NewWithValidTestData<OrgContact>());
			var bookingPartyOrg = Factory.NewWithValidTestData<OrgHeader>();
			Factory.Save();

			var order = Factory.NewWithValidTestData<Order>();
			order.JD_OrderNumber = "X125";
			order.JD_OA_BuyerAddress = assigneeOrg.MainAddress.PK;
			order.JD_OC_BuyerContact = assigneeOrg.Contacts[0].PK;

			var orderLine = Factory.NewWithValidTestData<OrderLine>();
			order.OrderLines.Add(orderLine);

			var booking = Factory.NewWithValidTestData<JobSupplierBooking>();
			OrderManagerTestHelper.CreateDocAddressAndFillWithAddressAndContact(booking, (DocAddressType.ControllingCustomer), controllingCustomerOrg.MainAddress.PK, controllingCustomerOrg.Contacts[0].OC_ContactName);
			OrderManagerTestHelper.CreateDocAddressAndFillWithAddressAndContact(booking, (DocAddressType.SupplierDocumentaryAddress), reviewerOrg.MainAddress.PK, reviewerOrg.Contacts[0].OC_ContactName);
			booking.JSB_OH_BookingParty = bookingPartyOrg.PK;

			var bookingLine1 = Factory.NewWithValidTestData<JobSupplierBookingLine>();
			bookingLine1.JSL_JO_OrderLine = orderLine.PK;
			var bookingLine2 = Factory.NewWithValidTestData<JobSupplierBookingLine>();
			bookingLine2.JSL_JO_OrderLine = orderLine.PK;
			booking.SupplierBookingLines.Add(bookingLine1);
			booking.SupplierBookingLines.Add(bookingLine2);
			OrderManagerTestHelper.CreateDocAddressAndFillWithAddressAndContact(bookingLine1, (DocAddressType.Manufacturer), manufactureOrg1.MainAddress.PK, manufactureOrg1.Contacts[0].OC_ContactName);
			OrderManagerTestHelper.CreateDocAddressAndFillWithAddressAndContact(bookingLine2, (DocAddressType.Manufacturer), manufactureOrg2.MainAddress.PK, manufactureOrg2.Contacts[0].OC_ContactName);
			Factory.Save();

			AssertEquals(ZGuid.Empty, booking.GetRequestSupportedAddressInfo(DocAddressTypes.Codes.Manufacturer).OrginzationPK);
			AssertEquals(ZGuid.Empty, booking.GetRequestSupportedAddressInfo(DocAddressTypes.Codes.Manufacturer).ContactPK);
		}

		#endregion

		#region Implementation

		public override BusinessObject GetNewBusinessObjectSafeSaving()
		{
			return supplierBooking;
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			return supplierBooking;
		}

		protected override BusinessObject GetBusinessObjectForFetchForLoad()
		{
			return supplierBooking;
		}

		protected override void SetUp()
		{
			base.SetUp();

			supplierBooking = Factory.NewWithValidTestData<JobSupplierBooking>();
			supplierBooking.JSB_BookingId = "SBK0001";
		}

		JobSupplierBooking supplierBooking;

		#endregion
	}
}
