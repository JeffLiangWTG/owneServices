using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.MasterFiles.Integration;
using NUnit.Framework;

namespace Enterprise.Warehouse.Yard.Business.Test
{
	[TestedType(typeof(CYDTransportationUnit))]
	public class CYDTransportationUnitWorkflowProviderTest : WorkflowProviderTest<CYDTransportationUnit, CYDTransportationUnitProcessTaskCollection>
	{
		protected override ZString ExpectedWorkflowType => WorkflowDescriptors.CYDTransportationUnitWorkflowDescriptorCode;

		public void TestTemplateIsAppliedToCorrectClients()
		{
			var clientWithTemplate = Factory.NewWithValidTestData<OrgHeader>();
			var clientWithoutTemplate = Factory.NewWithValidTestData<OrgHeader>();

			var template = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			template.P0_ProcessType = ExpectedWorkflowType;
			template.P0_OH_Client = clientWithTemplate.PK;

			var milestone = template.WorkflowItems.Milestones.AddNew();
			milestone.TriggerConditions.TriggerEventCode = "XXX";

			Factory.Save();

			var transportUnit1 = Factory.NewWithValidTestData<CYDTransportationUnit>();
			var jobDocAddress1 = Factory.NewWithValidTestData<JobDocAddress>();
			jobDocAddress1.E2_ParentID = transportUnit1.PK;
			jobDocAddress1.OrganisationPK = clientWithTemplate.PK;
			jobDocAddress1.E2_AddressType = AutoDocAddressTypes.Codes.TransportCompanyDocumentaryAddress;

			var transportUnit2 = Factory.NewWithValidTestData<CYDTransportationUnit>();
			var jobDocAddress2 = Factory.NewWithValidTestData<JobDocAddress>();
			jobDocAddress2.E2_ParentID = transportUnit2.PK;
			jobDocAddress2.OrganisationPK = clientWithoutTemplate.PK;
			jobDocAddress2.E2_AddressType = AutoDocAddressTypes.Codes.TransportCompanyDocumentaryAddress;

			Factory.Save();

			AssertEquals("Template tasks created when client matches", 1, transportUnit1.WorkflowItems.Milestones.Count);
			AssertEquals("Template tasks NOT created when client doesn't match", 0, transportUnit2.WorkflowItems.Milestones.Count);
		}
	}
}
