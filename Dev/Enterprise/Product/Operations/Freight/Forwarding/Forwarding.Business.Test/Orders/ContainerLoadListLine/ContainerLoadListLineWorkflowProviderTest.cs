using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.Orders.Business.Testing
{
	[TestedType(typeof(ContainerLoadListLine))]
	sealed class ContainerLoadListLineWorkflowProviderTest
		 : WorkflowProviderTest<ContainerLoadListLine, ContainerLoadListLineProcessTaskCollection>
	{
		protected override ZString ExpectedWorkflowType => WorkflowDescriptors.ContainerLoadListLineWorkflowDescriptorCode;

		protected override ContainerLoadListLine GetNewBusinessObject(BusinessObjectFactory factory)
		{
			var bookingLine = factory.NewWithValidTestData<JobSupplierBookingLine>();
			var cargoLoadPlanLine = factory.NewWithValidTestData<ContainerLoadListLine>();
			cargoLoadPlanLine.CLL_JSL_BookingLine = bookingLine.PK;
			cargoLoadPlanLine.CLL_LoadMode = CommonContainerLoadListLoadModeList.Codes.CY;
			return cargoLoadPlanLine;
		}

		public void TestGetTemplateSelectionCriteria_Client_WithControllingCustomer_ShouldUseBookingLogic()
		{
			var buyer = Factory.NewWithValidTestData<OrgHeader>();
			var cc = Factory.NewWithValidTestData<OrgHeader>();

			var containerLoadListLine = GetNewBusinessObject(Factory);
			var bookingLine = containerLoadListLine.SupplierBookingLine;

			containerLoadListLine.SupplierBookingLine.OrderLine.Order.JD_OA_BuyerAddress = buyer.MainAddress.PK;
			containerLoadListLine.SupplierBookingLine.SupplierBooking.ControllingCustomerNameOrPK = cc.PK.ToString();

			var templateSelectionCriteria = containerLoadListLine.GetTemplateSelectionCriteria() as ColumnValueRanker;
			var clients = templateSelectionCriteria.GetValues(ProcessTaskTemplateSchema.P0_OH_Client);

			AssertNotNull("should find search criteria matching P0_OH_Client", clients);
			AssertEquals("should have matched 3 values", 3, clients.Length);
			AssertEquals("First organisation should be buyer", buyer.PK, clients[0]);
			AssertEquals("Second organisation should be cc", cc.PK, clients[1]);
			AssertEquals("Last value should be empty", ZGuid.Empty, clients[2]);
		}

		public void TestGetTemplateSelectionCriteria_Client_WithoutControllingCustomer_ShouldUseBookingLogic()
		{
			var buyer = Factory.NewWithValidTestData<OrgHeader>();

			var containerLoadListLine = GetNewBusinessObject(Factory);
			containerLoadListLine.SupplierBookingLine.OrderLine.Order.JD_OA_BuyerAddress = buyer.MainAddress.PK;

			var templateSelectionCriteria = containerLoadListLine.GetTemplateSelectionCriteria() as ColumnValueRanker;
			var clients = templateSelectionCriteria.GetValues(ProcessTaskTemplateSchema.P0_OH_Client);

			AssertNotNull("should find search criteria matching P0_OH_Client", clients);
			AssertEquals("should have matched 2 values", 2, clients.Length);
			AssertEquals("First organisation should be buyer", buyer.PK, clients[0]);
			AssertEquals("Last value should be empty", ZGuid.Empty, clients[1]);
		}

		public void TestGetTemplateSelectionCriteria_TransportMode_ShouldUseBookingLogic()
		{
			var containerLoadListLine = GetNewBusinessObject(Factory);
			containerLoadListLine.SupplierBookingLine.SupplierBooking.JSB_TransportMode = "FOO";

			var templateSelectionCriteria = containerLoadListLine.GetTemplateSelectionCriteria() as ColumnValueRanker;
			var transportMode = templateSelectionCriteria.GetValues(ProcessTaskTemplateSchema.P0_SubType1);

			AssertEquals(2, transportMode.Length);
			AssertEquals("FOO", transportMode[0]);
			AssertEquals(ZString.Empty, transportMode[1]);
		}
	}
}
