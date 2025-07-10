using CargoWise.Definitions;
using CargoWise.Schema;
using Enterprise.Messaging.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(OrgSalesCallWorkflowDescriptor))]
	sealed class OrgSalesCallWorkflowDescriptorTest : WorkflowDescriptorTestCase<OrgSalesCallWorkflowDescriptor>
	{
		public override void TestID()
		{
			AssertEquals("Correct Code", "COM", WorkflowDescriptor.Code);
		}

		public override void TestDescription()
		{
			AssertEquals("Correct Desc", "Communication Manager", WorkflowDescriptor.Description);
		}

		public override void TestSubTypes()
		{
			AssertEquals(3, WorkflowDescriptor.SubTypeInformation.Length);
			AssertEquals("Method", WorkflowDescriptor.SubTypeInformation[0].Description);
			AssertEquals("Status", WorkflowDescriptor.SubTypeInformation[1].Description);
			AssertEquals(OrganisationsDataRegistry.Instance.CategoryListLabel.Value, WorkflowDescriptor.SubTypeInformation[2].Description);
		}

		public override void TestRequiresPorts()
		{
			AssertEquals(false, WorkflowDescriptor.RequiresPort1);
			AssertEquals(false, WorkflowDescriptor.RequiresPort2);
		}

		public override void TestRequiresClient()
		{
			AssertEquals(false, WorkflowDescriptor.RequiresClient);
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

		protected override MessageRecipientPartyType ExpectedSupportedMessageRecipientParties
		{
			get { return MessageRecipientPartyType.OrgProxy | MessageRecipientPartyType.Client | MessageRecipientPartyType.Email; }
		}

		protected override SchemaColumn[] ExpectedWorkflowTriggerFieldColumns
		{
			get { return new SchemaColumn[] { OrgSalesCallSchema.OQ_Status }; }
		}

		protected override bool ExpectingTasksToBeCompanySpecific
		{
			get { return false; }
		}

		protected override IWorkflowProvider[] GetParentsWithConfiguredOrganisationPartiesForTest()
		{
			var call = ClientOrg.SalesCalls.AddNew();
			return new[] { call };
		}

		protected override string EDIMessageSubType => EDIMessageSubTypeList.Codes.XmlNativeCommunication;

		public void TestDocumentBusinessContext()
		{
			AssertEquals(BusinessContext.Communication, WorkflowDescriptor.DocumentBusinessContext[0]);
		}
	}
}
