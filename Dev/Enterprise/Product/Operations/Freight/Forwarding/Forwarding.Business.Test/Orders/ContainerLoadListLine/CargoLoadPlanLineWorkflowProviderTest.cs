using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.Orders.Business.Testing
{
	[TestedType(typeof(ContainerLoadListLine))]
	sealed class CargoLoadPlanLineWorkflowProviderTest
		 : WorkflowProviderTest<ContainerLoadListLine, ContainerLoadListLineProcessTaskCollection>
	{
		protected override ZString ExpectedWorkflowType => WorkflowDescriptors.CargoLoadPlanLineWorkflowDescriptorCode;

		protected override ContainerLoadListLine GetNewBusinessObject(BusinessObjectFactory factory)
		{
			var bookingLine = factory.NewWithValidTestData<JobSupplierBookingLine>();
			var cargoLoadPlanLine = factory.NewWithValidTestData<ContainerLoadListLine>();
			cargoLoadPlanLine.CLL_JSL_BookingLine = bookingLine.PK;
			cargoLoadPlanLine.CLL_LoadMode = CommonContainerLoadListLoadModeList.Codes.CFS;
			return cargoLoadPlanLine;
		}

		public void TestGetTemplateSelectionCriteria_Client_WithControllingCustomer_ShouldUseBookingLineLogic()
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

		public void TestGetTemplateSelectionCriteria_Client_WithoutControllingCustomer_ShouldUseBookingLineLogic()
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

		public void TestGetTemplateSelectionCriteria_TransportMode_ShouldUseHeaderPlanned()
		{
			var containerLoadListLine = GetNewBusinessObject(Factory);
			containerLoadListLine.LoadListHeader.CLH_PlannedTransportMode = "FOO";

			var templateSelectionCriteria = containerLoadListLine.GetTemplateSelectionCriteria() as ColumnValueRanker;
			var transportModes = templateSelectionCriteria.GetValues(ProcessTaskTemplateSchema.P0_SubType1);

			AssertEquals(2, transportModes.Length);
			AssertEquals("FOO", transportModes[0]);
			AssertEquals(ZString.Empty, transportModes[1]);
		}
	}
}
