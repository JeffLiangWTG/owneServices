using System;
using CargoWise.EntityFramework.Testing;
using Enterprise.TransportConsignment.Business;
using Moq;
using WTG.ProductionRules.Business.Common;
using WTG.ProductionRules.Business.JobBillingDefaulting.Facts;

namespace Enterprise.TransportConsignment.ProductionRulesEngine.Testing
{
	public class ConsignmentFactTest : TestCaseWithFactory
	{
		public void Test_Constructor_ShouldThrowArgumentNullException_WhenConsignmentIsNull()
		{
			var ex = AssertExceptionThrown<ArgumentNullException>(() => new ConsignmentFact(null, branchFactMock.Object, departmentFactMock.Object, "US", localClientMock.Object, parentWarehouseOrderMock.Object, parentForwardingShipmentMock.Object));
			AssertEquals("consignment", ex.ParamName);
		}

		public void Test_Constructor_ShouldInitializeProperties_WhenArgumentsAreValid()
		{
			var expectedCompanyCountry = "US";
			var consignment = Factory.NewWithValidTestData<DtbConsignment>();
			var fact = new ConsignmentFact(
				consignment,
				branchFactMock.Object,
				departmentFactMock.Object,
				expectedCompanyCountry,
				localClientMock.Object,
				parentWarehouseOrderMock.Object,
				parentForwardingShipmentMock.Object);

			AssertEquals(consignment.PK, fact.PK);
			AssertNotNull(fact.LocalClient);
			AssertNotNull(fact.ParentWarehouseOrder);
			AssertNotNull(fact.ParentForwardingShipment);
			AssertNotNull(fact.CurrentBranch);
			AssertNotNull(fact.CurrentDepartment);
			AssertEquals(expectedCompanyCountry, fact.CurrentCompanyCountry);
		}

		protected override void SetUp()
		{
			base.SetUp();
			branchFactMock = new Mock<IBranchFact>();
			departmentFactMock = new Mock<IDepartmentFact>();
			localClientMock = new Mock<IOrganisationWithMainAddressFact>();
			parentWarehouseOrderMock = new Mock<ILandTransportJobWarehouseFact>();
			parentForwardingShipmentMock = new Mock<ILandTransportJobShipmentFact>();
		}

		Mock<IBranchFact> branchFactMock;
		Mock<IDepartmentFact> departmentFactMock;
		Mock<IOrganisationWithMainAddressFact> localClientMock;
		Mock<ILandTransportJobWarehouseFact> parentWarehouseOrderMock;
		Mock<ILandTransportJobShipmentFact> parentForwardingShipmentMock;
	}
}
