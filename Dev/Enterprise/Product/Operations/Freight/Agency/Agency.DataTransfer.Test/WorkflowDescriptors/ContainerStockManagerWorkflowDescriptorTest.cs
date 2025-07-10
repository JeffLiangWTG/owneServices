using CargoWise.Definitions;
using Enterprise.Freight.Agency.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.Agency.DataTransfer.Testing
{
	[TestedType(typeof(ContainerStockManagerWorkflowDescriptor))]
	internal class ContainerStockManagerWorkflowDescriptorTest : WorkflowDescriptorTestCase<ContainerStockManagerWorkflowDescriptor>
	{
		public override void TestID()
		{
			AssertEquals("WorkflowDescriptor.Code", WorkflowDescriptors.ContainerStockManagerWorkflowDescriptorCode, WorkflowDescriptor.Code);
		}

		public override void TestDescription()
		{
			AssertEquals("WorkflowDescriptor.Description", "Container Stock Manager", WorkflowDescriptor.Description);
		}

		public void TestDocumentBusinessContext()
		{
			AssertEquals("WorkflowDescriptor.DocumentBusinessContext[0]", BusinessContext.INVALID, WorkflowDescriptor.DocumentBusinessContext[0]);
		}

		public override void TestSubTypes()
		{
			AssertEquals(0, WorkflowDescriptor.SubTypeInformation.Length);
		}

		public override void TestRequiresPorts()
		{
			AssertEquals("RequiresPort1", false, WorkflowDescriptor.RequiresPort1);
			AssertEquals("RequiresPort2", false, WorkflowDescriptor.RequiresPort2);
		}

		public override void TestRequiresClient()
		{
			AssertEquals("RequiresClient", true, WorkflowDescriptor.RequiresClient);
		}

		public override void TestRequiresBranch()
		{
			AssertEquals("RequiresBranch", false, WorkflowDescriptor.RequiresBranch);
		}

		public override void TestRequiresDepartment()
		{
			AssertEquals("RequiresDepartment", false, WorkflowDescriptor.RequiresDepartment);
		}

		public override void TestSupportsEventTracking()
		{
			AssertEquals(true, WorkflowDescriptor.SupportsEventTracking);
		}

		protected override IWorkflowProvider[] GetParentsWithConfiguredOrganisationPartiesForTest()
		{
			return new IWorkflowProvider[] { Factory.NewWithValidTestData<RefContainerStock>() };
		}

		protected override MessageRecipientPartyType ExpectedSupportedMessageRecipientParties
		{
			get
			{
				return MessageRecipientPartyType.OrgProxy | MessageRecipientPartyType.Email;
			}
		}
	}
}
