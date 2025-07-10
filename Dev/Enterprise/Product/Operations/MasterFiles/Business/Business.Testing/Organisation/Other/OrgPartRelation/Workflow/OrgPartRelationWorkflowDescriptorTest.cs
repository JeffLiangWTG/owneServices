using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(OrgPartRelationWorkflowDescriptor))]
	sealed class OrgPartRelationWorkflowDescriptorTest : WorkflowDescriptorTestCase<OrgPartRelationWorkflowDescriptor>
	{
		#region Identification

		public override void TestID()
		{
			AssertEquals(WorkflowDescriptors.OrgPartRelationWorkflowDescriptorCode, WorkflowDescriptor.Code);
		}

		public override void TestDescription()
		{
			AssertEquals("Organization Product Relationship", WorkflowDescriptor.Description);
		}

		public override void TestSupportsTasks()
		{
			AssertEquals(false, WorkflowDescriptor.SupportsTasks);
		}

		#endregion

		#region TestIncludeWorkflowTriggerActionXMLDebtorBalance

		public override void TestIncludeWorkflowTriggerActionXMLDebtorBalance()
		{
			AssertEquals(false, WorkflowDescriptor.IncludeWorkflowTriggerActionXMLDebtorBalance);
		}

		#endregion

		#region TestRequiresBranch

		public override void TestRequiresBranch()
		{
			AssertEquals(false, WorkflowDescriptor.RequiresBranch);
		}

		#endregion

		#region TestRequiresClient

		public override void TestRequiresClient()
		{
			AssertEquals(true, WorkflowDescriptor.RequiresClient);
		}

		#endregion

		#region TestRequiresDepartment

		public override void TestRequiresDepartment()
		{
			AssertEquals(false, WorkflowDescriptor.RequiresDepartment);
		}

		#endregion

		#region TestRequiresPorts

		public override void TestRequiresPorts()
		{
			AssertEquals(false, WorkflowDescriptor.RequiresPort1);
			AssertEquals(false, WorkflowDescriptor.RequiresPort2);
		}

		#endregion

		#region TestSubTypes

		public override void TestSubTypes()
		{
			AssertEquals(0, WorkflowDescriptor.SubTypeInformation.Length);
		}

		#endregion

		#region TestSupportSetFiledTriggerAction

		public void TestSupportSetFiledTriggerAction()
		{
			AssertEquals(false, WorkflowDescriptor.SupportsSetFieldTriggerAction(null, null));
		}

		#endregion

		#region Implementation

		#region ExpectedSupportedMessageRecipientParties

		protected override MessageRecipientPartyType ExpectedSupportedMessageRecipientParties
		{
			get { return MessageRecipientPartyType.None; }
		}

		#endregion

		#region GetParentsWithConfiguredOrganisationPartiesForTest

		protected override IWorkflowProvider[] GetParentsWithConfiguredOrganisationPartiesForTest()
		{
			return new IWorkflowProvider[] { OrgPartRelationWithConfiguredOrganisationParties, };
		}

		OrgPartRelation OrgPartRelationWithConfiguredOrganisationParties
		{
			get { return orgPartRelationWithConfigureOrganisationParties ?? (orgPartRelationWithConfigureOrganisationParties = GetOrgPartRelationWithOrgInfo()); }
		}

		OrgPartRelation GetOrgPartRelationWithOrgInfo()
		{
			var product = Factory.New<OrgSupplierPart>();
			product.OP_PartNum = "Test Product";

			var relation = product.RelatedOrganisations.AddNew();
			relation.OU_OH = ClientOrg.PK;
			relation.OU_Relationship = OrgPartRelation.RelationshipTypes.Owner;

			return relation;
		}

		OrgPartRelation orgPartRelationWithConfigureOrganisationParties;

		#endregion

		#region SetupOrg

		protected override void SetupOrg(OrgHeader org)
		{
			base.SetupOrg(org);

			org.MainAddress.OA_Address1 = "Owner";
			org.MainAddress.OA_Code = "ABCD";
			org.MainAddress.OA_City = "City";
			org.MainAddress.OA_RL_NKRelatedPortCode = "AUSYD";
			org.MainAddress.OA_Email = "tester@cargowise.com";
		}

		#endregion

		#endregion
	}
}
