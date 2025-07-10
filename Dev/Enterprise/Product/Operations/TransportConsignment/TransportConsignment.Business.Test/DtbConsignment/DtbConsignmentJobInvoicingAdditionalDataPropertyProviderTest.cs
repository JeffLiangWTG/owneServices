using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.TransportCommon.Shared;
using Enterprise.Warehouse.Integration;

namespace Enterprise.TransportConsignment.Business.Testing
{
	class DtbConsignmentJobInvoicingAdditionalDataPropertyProviderTest : TestCaseWithFactory
	{
		public void TestGetCustomProperties()
		{
			String[] customProperties =
			{
				DtbConsignmentJobInvoicingAdditionalDataPropertyProvider.DocketID,
				DtbConsignmentJobInvoicingAdditionalDataPropertyProvider.DocketReference,
				DtbConsignmentJobInvoicingAdditionalDataPropertyProvider.CustomerReference,
				DtbConsignmentJobInvoicingAdditionalDataPropertyProvider.ConsigneeAddress1,
				DtbConsignmentJobInvoicingAdditionalDataPropertyProvider.ConsigneeAddress2,
				DtbConsignmentJobInvoicingAdditionalDataPropertyProvider.ConsigneeCity,
				DtbConsignmentJobInvoicingAdditionalDataPropertyProvider.ConsigneePostCode,
				DtbConsignmentJobInvoicingAdditionalDataPropertyProvider.ConsigneeState,
				DtbConsignmentJobInvoicingAdditionalDataPropertyProvider.ConsigneeUNLOCO,
				DtbConsignmentJobInvoicingAdditionalDataPropertyProvider.ConsigneeCode
			};
			foreach (var item in customProperties)
			{
				var customProperty = GetAdditionalDataProvider.GetAdditionalProperties()[item];
				AssertNotNull("Should be included in the CustomProperty collection.", customProperty);
				AssertEquals("Type should be ZString.", typeof(ZString), customProperty.Info.Type);
				Assert("Property should not be visible by default.", !customProperty.Info.Visible);
			}
		}

		public void TestGetJobID_ShouldHaveBookingParentJobID()
		{
			var customProperty = GetAdditionalDataProvider.GetAdditionalProperties()[DtbConsignmentJobInvoicingAdditionalDataPropertyProvider.DocketID];
			var consignment = Helper.CreateConsignment();
			var booking = Helper.CreateBookingWithConsolidation();
			var warehouseOrder = Factory.New<IWhsOrder>();
			warehouseOrder.WD_DocketID = "WH_Job_ID";
			booking.ConsolidationSingleJob.KB_ParentID = warehouseOrder.PK;
			booking.ConsolidationSingleJob.KB_ParentTableCode = "WD";

			consignment.LTC_KM_Booking = booking.PK;
			var job = Helper.CreateJobHeader(consignment);
			var charge = Helper.CreateJobCharge(job);
			AssertEquals("Value.", "WH_Job_ID", customProperty.GetValue(charge));
		}

		public void TestGetJobID_ShouldHaveBookingJobID()
		{
			var customProperty = GetAdditionalDataProvider.GetAdditionalProperties()[DtbConsignmentJobInvoicingAdditionalDataPropertyProvider.DocketID];
			var consignment = Helper.CreateConsignment();
			var booking = Helper.CreateBookingWithConsolidation();
			booking.KM_JobID = "J123";
			consignment.LTC_KM_Booking = booking.PK;
			var job = Helper.CreateJobHeader(consignment);
			var charge = Helper.CreateJobCharge(job);
			AssertEquals("Value.", "J123", customProperty.GetValue(charge));
		}

		public void TestGetJobID_ShouldHaveConsignmentAdditionalReference_WithTypeBookingPartyReference()
		{
			var customProperty = GetAdditionalDataProvider.GetAdditionalProperties()[DtbConsignmentJobInvoicingAdditionalDataPropertyProvider.DocketID];
			var consignment = Helper.CreateConsignment();
			Helper.CreateAdditionalReference(consignment, DateTime.UtcNow, TransportCommonAdditionalReferenceTypes.Codes.BookingPartyReference, "ConsignmentAdditionalReferenceID", "LineRef1");
			var job = Helper.CreateJobHeader(consignment);
			var charge = Helper.CreateJobCharge(job);
			AssertEquals("Value.", "ConsignmentAdditionalReferenceID", customProperty.GetValue(charge));
		}

		public void TestGetJobReference()
		{
			var customProperty = GetAdditionalDataProvider.GetAdditionalProperties()[DtbConsignmentJobInvoicingAdditionalDataPropertyProvider.DocketReference];
			AssertNotNull("JobReference should be included in the CustomProperty collection.", customProperty);
			AssertEquals("Type should be ZString.", typeof(ZString), customProperty.Info.Type);
			Assert("Property should not be visible by default.", !customProperty.Info.Visible);

			var consignment = Helper.CreateConsignment();
			Helper.CreateAdditionalReference(consignment, DateTime.UtcNow, TransportCommonAdditionalReferenceTypes.Codes.OrderNumber, "ConsignmentAdditionalReferenceID", "LineRef1");
			Helper.CreateAdditionalReference(consignment, DateTime.UtcNow, TransportCommonAdditionalReferenceTypes.Codes.OrderNumber, "ConsignmentAdditionalReferenceID2", "LineRef2");
			var job = Helper.CreateJobHeader(consignment);
			var charge = Helper.CreateJobCharge(job);
			AssertEquals("Value.", "ConsignmentAdditionalReferenceID", customProperty.GetValue(charge));
		}

		public void TestGetCustomerReference()
		{
			var customProperty = GetAdditionalDataProvider.GetAdditionalProperties()[DtbConsignmentJobInvoicingAdditionalDataPropertyProvider.CustomerReference];
			var consignment = Helper.CreateConsignment();
			var job = Helper.CreateJobHeader(consignment);
			var charge = Helper.CreateJobCharge(job);

			AssertEquals("Value.", ZString.Empty,customProperty.GetValue(charge));

			Helper.CreateAdditionalReference(consignment, DateTime.UtcNow, TransportCommonAdditionalReferenceTypes.Codes.ClientReferenceNumber, "AddReference_CustomerReference", "CustomerReference");
			AssertEquals("Value.", "AddReference_CustomerReference", customProperty.GetValue(charge));
		}

		public void TestGetConsigneeCode()
		{
			var customProperty = GetAdditionalDataProvider.GetAdditionalProperties()[DtbConsignmentJobInvoicingAdditionalDataPropertyProvider.ConsigneeCode];

			CreateConsignmentWithDeliveryAddress(out var consignment, out _, out var orgHeader, out var charge);
			AssertEquals("Value.", orgHeader.OH_Code, customProperty.GetValue(charge));
		}

		public void TestGetConsigneeAddress1()
		{
			var customProperty = GetAdditionalDataProvider.GetAdditionalProperties()[DtbConsignmentJobInvoicingAdditionalDataPropertyProvider.ConsigneeAddress1];

			CreateConsignmentWithDeliveryAddress(out var consignment, out var deliveryAddress, out _, out var charge);
			AssertEquals("Value.", deliveryAddress.Address1, customProperty.GetValue(charge));
		}

		public void TestGetConsigneeAddress2()
		{
			var customProperty = GetAdditionalDataProvider.GetAdditionalProperties()[DtbConsignmentJobInvoicingAdditionalDataPropertyProvider.ConsigneeAddress2];

			CreateConsignmentWithDeliveryAddress(out var consignment, out var deliveryAddress, out _, out var charge);
			AssertEquals("Value.", deliveryAddress.Address2, customProperty.GetValue(charge));
		}

		public void TestGetConsigneeCity()
		{
			var customProperty = GetAdditionalDataProvider.GetAdditionalProperties()[DtbConsignmentJobInvoicingAdditionalDataPropertyProvider.ConsigneeCity];

			CreateConsignmentWithDeliveryAddress(out var consignment, out var deliveryAddress, out _, out var charge);
			AssertEquals("Value.", deliveryAddress.City, customProperty.GetValue(charge));
		}

		public void TestGetConsigneePostCode()
		{
			var customProperty = GetAdditionalDataProvider.GetAdditionalProperties()[DtbConsignmentJobInvoicingAdditionalDataPropertyProvider.ConsigneePostCode];

			CreateConsignmentWithDeliveryAddress(out var consignment, out var deliveryAddress, out _, out var charge);
			AssertEquals("Value.", deliveryAddress.Postcode, customProperty.GetValue(charge));
		}

		public void TestGetConsigneeState()
		{
			var customProperty = GetAdditionalDataProvider.GetAdditionalProperties()[DtbConsignmentJobInvoicingAdditionalDataPropertyProvider.ConsigneeState];

			CreateConsignmentWithDeliveryAddress(out var consignment, out var deliveryAddress, out _, out var charge);
			AssertEquals("Value.", deliveryAddress.State, customProperty.GetValue(charge));
		}

		public void TestGetConsigneeUNLOCO()
		{
			var customProperty = GetAdditionalDataProvider.GetAdditionalProperties()[DtbConsignmentJobInvoicingAdditionalDataPropertyProvider.ConsigneeUNLOCO];

			CreateConsignmentWithDeliveryAddress(out var consignment, out var deliveryAddress, out _, out var charge);
			AssertEquals("Value.", deliveryAddress.OA_RL_NKRelatedPortCode, customProperty.GetValue(charge));
		}

		protected DtbConsignmentJobInvoicingAdditionalDataPropertyProvider GetAdditionalDataProvider
		{
			get { return new DtbConsignmentJobInvoicingAdditionalDataPropertyProvider(); }
		}

		void CreateConsignmentWithDeliveryAddress(out DtbConsignment consignment, out OrgAddress deliveryAddress, out OrgHeader deliveryOrg, out JobCharge charge)
		{
			deliveryOrg = Helper.CreateOrganisation("O2", "NZAKL");
			deliveryAddress = Helper.CreateOrgAddress(deliveryOrg, "AUBNE");
			deliveryAddress.Address2 = "Test Address 2";
			deliveryAddress.City = "Test City";
			deliveryAddress.Postcode = "1234";
			deliveryAddress.State = "Test State";

			consignment = Helper.CreateConsignment();
			_ = Helper.CreateConsignmentAddress(consignment, ConsignmentAddressTypes.Codes.Delivery, deliveryAddress);
			var job = Helper.CreateJobHeader(consignment);
			charge = Helper.CreateJobCharge(job);
		}

		TransportConsignmentTestHelper Helper
		{
			get { return helper ?? (helper = new TransportConsignmentTestHelper(Factory)); }
		}

		TransportConsignmentTestHelper helper;
	}
}
