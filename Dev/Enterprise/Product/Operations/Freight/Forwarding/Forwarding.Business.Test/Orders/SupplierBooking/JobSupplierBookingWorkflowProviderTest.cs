using System.Collections.Generic;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.Orders.Business.Testing
{
	[TestedType(typeof(JobSupplierBooking))]
	sealed class JobSupplierBookingWorkflowProviderTest : WorkflowProviderTest<JobSupplierBooking, SupplierBookingProcessTaskCollection>
	{
		protected override ZString ExpectedWorkflowType => WorkflowDescriptors.JobSupplierBookingWorkflowDescriptorCode;

		protected override JobSupplierBooking GetNewBusinessObject(BusinessObjectFactory factory)
		{
			return factory.NewWithValidTestData<JobSupplierBooking>();
		}

		public void TestJobNumber()
		{
			var supplierBooking = Factory.NewWithValidTestData<JobSupplierBooking>();

			supplierBooking.JSB_TransportMode = "AIR";
			supplierBooking.JSB_LoadMode = "CY";
			supplierBooking.JSB_Status = "PLN";
			supplierBooking.JSB_BookingId = "SBK0001";

			var milestone = supplierBooking.WorkflowItems.Milestones.AddNew();

			AssertEquals("SBK0001", supplierBooking.JobNumber);
			AssertEquals("SBK0001", milestone.JobNumber);
		}

		public void TestGetTemplateSelectionCriteria_WithControllingCustomer()
		{
			var buyer = Factory.NewWithValidTestData<OrgHeader>();
			var cc = Factory.NewWithValidTestData<OrgHeader>();

			var bookingLine = Factory.NewWithValidTestData<JobSupplierBookingLine>();
			bookingLine.OrderLine.Order.JD_OA_BuyerAddress = buyer.MainAddress.PK;

			var booking = GetNewBusinessObject(Factory);
			booking.ControllingCustomerNameOrPK = cc.PK.ToString();
			booking.SupplierBookingLines.Add(bookingLine);

			var templateSelectionCriteria = booking.GetTemplateSelectionCriteria() as ColumnValueRanker;
			var clients = templateSelectionCriteria.GetValues(ProcessTaskTemplateSchema.P0_OH_Client);
			AssertNotNull("should find search criteria matching P0_OH_Client", clients);
			AssertEquals("should have matched 3 values", 3, clients.Length);
			AssertEquals("First organisation should be buyer", buyer.PK, clients[0]);
			AssertEquals("Second organisation should be cc", cc.PK, clients[1]);
			AssertEquals("Last value should be empty", ZGuid.Empty, clients[2]);
		}

		public void TestGetTemplateSelectionCriteria_WithoutControllingCustomer()
		{
			var buyer = Factory.NewWithValidTestData<OrgHeader>();

			var bookingLine = Factory.NewWithValidTestData<JobSupplierBookingLine>();
			bookingLine.OrderLine.Order.JD_OA_BuyerAddress = buyer.MainAddress.PK;

			var booking = GetNewBusinessObject(Factory);
			booking.SupplierBookingLines.Add(bookingLine);

			var templateSelectionCriteria = booking.GetTemplateSelectionCriteria() as ColumnValueRanker;
			var clients = templateSelectionCriteria.GetValues(ProcessTaskTemplateSchema.P0_OH_Client);
			AssertNotNull("should find search criteria matching P0_OH_Client", clients);
			AssertEquals("should have matched 2 values", 2, clients.Length);
			AssertEquals("First organisation should be buyer", buyer.PK, clients[0]);
			AssertEquals("Last value should be empty", ZGuid.Empty, clients[1]);
		}

		public void TestGetTemplateSelectionCriteria_WithMultipleBuyers()
		{
			// This is not a valid scenario with the current business requirements.
			// However, the schema allows it so we should make nothing unexpected happens.
			// In this scenario, it should pick an arbituary buyer and treat that as the main and only buyer, and alert WTG.
			var buyer1 = Factory.NewWithValidTestData<OrgHeader>();
			var buyer2 = Factory.NewWithValidTestData<OrgHeader>();

			var bookingLine1 = Factory.NewWithValidTestData<JobSupplierBookingLine>();
			bookingLine1.OrderLine.Order.JD_OA_BuyerAddress = buyer1.MainAddress.PK;

			var bookingLine2 = Factory.NewWithValidTestData<JobSupplierBookingLine>();
			bookingLine2.OrderLine.Order.JD_OA_BuyerAddress = buyer2.MainAddress.PK;

			var booking = GetNewBusinessObject(Factory);
			booking.SupplierBookingLines.Add(bookingLine1);
			booking.SupplierBookingLines.Add(bookingLine2);

			var templateSelectionCriteria = booking.GetTemplateSelectionCriteria() as ColumnValueRanker;
			var clients = templateSelectionCriteria.GetValues(ProcessTaskTemplateSchema.P0_OH_Client);
			AssertNotNull("should find search criteria matching P0_OH_Client", clients);
			AssertEquals("should have matched 2 values", 2, clients.Length);
			AssertCollectionContains("First organisation should be a buyer", clients[0], new List<ZGuid>() { buyer1.PK, buyer2.PK });
			AssertEquals("Last value should be empty", ZGuid.Empty, clients[1]);

			AssertEquals("Error should have notified of invalid case.", 1, ErrorReporter.TotalErrorCount);
			ErrorReporter.Clear();
		}

		public void TestGetTemplateSelectionCriteria_TestTransportMode()
		{
			var booking = GetNewBusinessObject(Factory);
			booking.JSB_TransportMode = "FOO";

			var templateSelectionCriteria = booking.GetTemplateSelectionCriteria() as ColumnValueRanker;
			object[] transportMode = templateSelectionCriteria.GetValues(ProcessTaskTemplateSchema.P0_SubType1);

			AssertEquals(2, transportMode.Length);
			AssertEquals("FOO", transportMode[0]);
			AssertEquals(ZString.Empty, transportMode[1]);
		}
	}
}
