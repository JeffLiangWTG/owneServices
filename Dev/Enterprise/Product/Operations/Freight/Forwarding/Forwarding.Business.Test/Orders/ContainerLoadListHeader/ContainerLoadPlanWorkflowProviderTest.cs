using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.Orders.Business.Testing
{
	[TestedType(typeof(CFSContainerLoadList))]
	sealed class ContainerLoadPlanWorkflowProviderTest : WorkflowProviderTest<CFSContainerLoadList, ContainerLoadPlanProcessTaskCollection>
	{
		protected override ZString ExpectedWorkflowType => WorkflowDescriptors.ContainerLoadPlanWorkflowDescriptorCode;

		protected override CFSContainerLoadList GetNewBusinessObject(BusinessObjectFactory factory)
		{
			var loadListHeader = factory.NewWithValidTestData<CFSContainerLoadList>();
			return loadListHeader;
		}

		public void TestGetTemplateSelectionCriteria_Client_ShouldUseOwnControllingCustomer()
		{
			var containerLoadPlan = GetNewBusinessObject(Factory);
			var cc = Factory.NewWithValidTestData<OrgHeader>();
			containerLoadPlan.ControllingCustomerNameOrPK = cc.PK.ToString();

			var templateSelectionCriteria = containerLoadPlan.GetTemplateSelectionCriteria() as ColumnValueRanker;
			var clients = templateSelectionCriteria.GetValues(ProcessTaskTemplateSchema.P0_OH_Client);

			AssertNotNull("should find search criteria matching P0_OH_Client", clients);
			AssertEquals("should have matched 2 values", 2, clients.Length);
			AssertEquals("should match with own CC", cc.PK, clients[0]);
			AssertEquals("Last value should be empty", ZGuid.Empty, clients[1]);
		}

		public void TestGetTemplateSelectionCriteria_TransportMode_ShouldUsePlanned()
		{
			var containerLoadPlan = GetNewBusinessObject(Factory);
			var cc = Factory.NewWithValidTestData<OrgHeader>();
			containerLoadPlan.ControllingCustomerNameOrPK = cc.PK.ToString();
			containerLoadPlan.CLH_PlannedTransportMode = "FOO";

			var templateSelectionCriteria = containerLoadPlan.GetTemplateSelectionCriteria() as ColumnValueRanker;
			var transportMode = templateSelectionCriteria.GetValues(ProcessTaskTemplateSchema.P0_SubType1);

			AssertEquals(2, transportMode.Length);
			AssertEquals("FOO", transportMode[0]);
			AssertEquals(ZString.Empty, transportMode[1]);
		}
	}
}
