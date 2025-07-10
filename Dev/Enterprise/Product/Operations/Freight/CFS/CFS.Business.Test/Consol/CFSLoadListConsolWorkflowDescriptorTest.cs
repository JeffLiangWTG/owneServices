using CargoWise.Definitions;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.CFS.Business.Testing
{
	[TestedType(typeof(CFSLoadListConsolWorkflowDescriptor))]
	class CFSLoadListConsolWorkflowDescriptorTest : WorkflowDescriptorTestCase<CFSLoadListConsolWorkflowDescriptor>
	{
		public override void TestID()
		{
			AssertEquals(JobInvoicingConsumerTypes.CFSLoadList.Code, WorkflowDescriptor.Code);
		}

		public override void TestDescription()
		{
			AssertEquals(JobInvoicingConsumerTypes.CFSLoadList.Description, WorkflowDescriptor.Description);
		}

		public override void TestSubTypes()
		{
			AssertEquals(1, WorkflowDescriptor.SubTypeInformation.Length);

			AssertEquals("Transport Mode", WorkflowDescriptor.SubTypeInformation[0].Description);
			AssertNotNull(WorkflowDescriptor.SubTypeInformation[0].List);
		}

		public override void TestRequiresPorts()
		{
			AssertEquals(true, WorkflowDescriptor.RequiresPort1);
			AssertEquals(true, WorkflowDescriptor.RequiresPort2);
		}

		public override void TestRequiresClient()
		{
			AssertEquals(true, WorkflowDescriptor.RequiresClient);
		}

		public override void TestRequiresBranch()
		{
			AssertEquals(false, WorkflowDescriptor.RequiresBranch);
		}

		public override void TestRequiresDepartment()
		{
			AssertEquals(false, WorkflowDescriptor.RequiresDepartment);
		}

		public override void TestSupportsEventTracking()
		{
			AssertEquals(true, WorkflowDescriptor.SupportsEventTracking);
		}

		public void TestDocumentBusinessContext()
		{
			AssertEquals(BusinessContext.CFSLoadList, WorkflowDescriptor.DocumentBusinessContext[0]);
		}

		protected override IWorkflowProvider[] GetParentsWithConfiguredOrganisationPartiesForTest()
		{
			var loadList = Factory.New<CFSLoadListConsol>();

			loadList.JK_OA_CartageCoAddress = DeliveryCartageOrg.MainAddress.PK;
			loadList.JK_OA_CTOAddress = DepartureCTOOrg.MainAddress.PK;
			loadList.JK_OA_EmptyContainerYard = DepartureContainerYardOrg.MainAddress.PK;
			loadList.JK_OH_Forwarder = Forwarder.PK;

			return new[] { loadList };
		}

		protected override MessageRecipientPartyType ExpectedSupportedMessageRecipientParties
		{
			get
			{
				return MessageRecipientPartyType.OrgProxy
					| MessageRecipientPartyType.Email
					| MessageRecipientPartyType.TransportCo
					| MessageRecipientPartyType.ContainerYard
					| MessageRecipientPartyType.CTO
					| MessageRecipientPartyType.Forwarder;
			}
		}
	}
}
