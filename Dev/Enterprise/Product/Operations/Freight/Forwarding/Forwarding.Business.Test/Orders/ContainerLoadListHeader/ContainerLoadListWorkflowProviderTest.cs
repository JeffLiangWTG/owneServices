using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.Orders.Business.Testing
{
	[TestedType(typeof(CYContainerLoadList))]
	sealed class ContainerLoadListWorkflowProviderTest : WorkflowProviderTest<CYContainerLoadList, ContainerLoadListProcessTaskCollection>
	{
		protected override ZString ExpectedWorkflowType => WorkflowDescriptors.ContainerLoadListWorkflowDescriptorCode;

		protected override CYContainerLoadList GetNewBusinessObject(BusinessObjectFactory factory)
		{
			var loadListHeader = factory.NewWithValidTestData<CYContainerLoadList>();
			loadListHeader.CLH_JSB_Booking = factory.NewWithValidTestData<JobSupplierBooking>().PK;
			return loadListHeader;
		}

		public void TestGetTemplateSelectionCriteria_Client_WithControllingCustomer_ShouldUseBookingLogic()
		{
			var containerLoadList = GetNewBusinessObject(Factory);

			var bookingLine = Factory.NewWithValidTestData<JobSupplierBookingLine>();
			containerLoadList.Booking.SupplierBookingLines.Add(bookingLine);

			var buyer = Factory.NewWithValidTestData<OrgHeader>();
			var cc = Factory.NewWithValidTestData<OrgHeader>();

			bookingLine.OrderLine.Order.JD_OA_BuyerAddress = buyer.MainAddress.PK;
			bookingLine.SupplierBooking.ControllingCustomerNameOrPK = cc.PK.ToString();

			var templateSelectionCriteria = containerLoadList.GetTemplateSelectionCriteria() as ColumnValueRanker;
			var clients = templateSelectionCriteria.GetValues(ProcessTaskTemplateSchema.P0_OH_Client);

			AssertNotNull("should find search criteria matching P0_OH_Client", clients);
			AssertEquals("should have matched 3 values", 3, clients.Length);
			AssertEquals("First organisation should be buyer", buyer.PK, clients[0]);
			AssertEquals("Second organisation should be cc", cc.PK, clients[1]);
			AssertEquals("Last value should be empty", ZGuid.Empty, clients[2]);
		}

		public void TestGetTemplateSelectionCriteria_Client_WithoutControllingCustomer_ShouldUseBookingLogic()
		{
			var containerLoadList = GetNewBusinessObject(Factory);

			var bookingLine = Factory.NewWithValidTestData<JobSupplierBookingLine>();
			containerLoadList.Booking.SupplierBookingLines.Add(bookingLine);

			var buyer = Factory.NewWithValidTestData<OrgHeader>();
			bookingLine.OrderLine.Order.JD_OA_BuyerAddress = buyer.MainAddress.PK;

			var templateSelectionCriteria = containerLoadList.GetTemplateSelectionCriteria() as ColumnValueRanker;
			var clients = templateSelectionCriteria.GetValues(ProcessTaskTemplateSchema.P0_OH_Client);

			AssertNotNull("should find search criteria matching P0_OH_Client", clients);
			AssertEquals("should have matched 2 values", 2, clients.Length);
			AssertEquals("First organisation should be buyer", buyer.PK, clients[0]);
			AssertEquals("Last value should be empty", ZGuid.Empty, clients[1]);
		}

		public void TestGetTemplateSelectionCriteria_TestTransportMode_ShouldUseBookingTransportMode()
		{
			var containerLoadList = GetNewBusinessObject(Factory);
			containerLoadList.Booking.JSB_TransportMode = "FOO";

			var templateSelectionCriteria = containerLoadList.GetTemplateSelectionCriteria() as ColumnValueRanker;
			var transportModes = templateSelectionCriteria.GetValues(ProcessTaskTemplateSchema.P0_SubType1);

			AssertEquals(2, transportModes.Length);
			AssertEquals("FOO", transportModes[0]);
			AssertEquals(ZString.Empty, transportModes[1]);
		}
	}
}
