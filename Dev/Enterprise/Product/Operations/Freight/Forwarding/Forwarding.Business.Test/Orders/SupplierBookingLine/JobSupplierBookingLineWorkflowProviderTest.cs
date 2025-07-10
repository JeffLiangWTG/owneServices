using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.Orders.Business.Testing
{
	[TestedType(typeof(JobSupplierBookingLine))]
	sealed class JobSupplierBookingLineWorkflowProviderTest
		 : WorkflowProviderTest<JobSupplierBookingLine, JobSupplierBookingLineProcessTaskCollection>
	{
		protected override ZString ExpectedWorkflowType => WorkflowDescriptors.JobSupplierBookingLineWorkflowDescriptorCode;

		public void TestGetTemplateSelectionCriteria_WithControllingCustomer()
		{
			var buyer = Factory.NewWithValidTestData<OrgHeader>();
			var cc = Factory.NewWithValidTestData<OrgHeader>();

			var bookingLine = Factory.NewWithValidTestData<JobSupplierBookingLine>();
			bookingLine.OrderLine.Order.JD_OA_BuyerAddress = buyer.MainAddress.PK;

			bookingLine.SupplierBooking.ControllingCustomerNameOrPK = cc.PK.ToString();

			var templateSelectionCriteria = bookingLine.GetTemplateSelectionCriteria() as ColumnValueRanker;
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

			var templateSelectionCriteria = bookingLine.GetTemplateSelectionCriteria() as ColumnValueRanker;
			var clients = templateSelectionCriteria.GetValues(ProcessTaskTemplateSchema.P0_OH_Client);

			AssertNotNull("should find search criteria matching P0_OH_Client", clients);
			AssertEquals("should have matched 2 values", 2, clients.Length);
			AssertEquals("First organisation should be buyer", buyer.PK, clients[0]);
			AssertEquals("Last value should be empty", ZGuid.Empty, clients[1]);
		}

		public void TestGetTemplateSelectionCriteria_TestTransportMode()
		{
			var bookingLine = Factory.NewWithValidTestData<JobSupplierBookingLine>();
			bookingLine.SupplierBooking.JSB_TransportMode = "FOO";

			var templateSelectionCriteria = bookingLine.GetTemplateSelectionCriteria() as ColumnValueRanker;
			var transportModes = templateSelectionCriteria.GetValues(ProcessTaskTemplateSchema.P0_SubType1);

			AssertEquals(2, transportModes.Length);
			AssertEquals("FOO", transportModes[0]);
			AssertEquals(ZString.Empty, transportModes[1]);
		}
	}
}
