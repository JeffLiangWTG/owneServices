using Enterprise.Messaging.Integration;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(OrgSupplierPartWorkflowDescriptor))]
	sealed class OrgSupplierPartWorkflowDescriptorTest : WorkflowDescriptorTestCase<OrgSupplierPartWorkflowDescriptor>
	{
		public override void TestID()
		{
			AssertEquals(WorkflowDescriptors.OrgSupplierPartWorkflowDescriptorCode, WorkflowDescriptor.Code);
		}

		public override void TestDescription()
		{
			AssertEquals("Product", WorkflowDescriptor.Description);
		}

		public override void TestSupportsEventTracking()
		{
			AssertEquals(true, WorkflowDescriptor.SupportsEventTracking);
		}

		public void TestSupportsWorkflowTriggerActionXML()
		{
			AssertEquals(true, WorkflowDescriptor.SupportsWorkflowTriggerActionXML);
		}

		public override void TestIncludeWorkflowTriggerActionXMLDebtorBalance()
		{
			AssertEquals(false, WorkflowDescriptor.IncludeWorkflowTriggerActionXMLDebtorBalance);
		}

		public void TestSupportSetFiledTriggerAction()
		{
			AssertEquals(true, WorkflowDescriptor.SupportsSetFieldTriggerAction(null, null));
		}

		protected override MessageRecipientPartyType ExpectedSupportedMessageRecipientParties
		{
			get { return MessageRecipientPartyType.OrgProxy | MessageRecipientPartyType.Client | MessageRecipientPartyType.Email; }
		}

		public override void TestSubTypes()
		{
			AssertEquals(0, WorkflowDescriptor.SubTypeInformation.Length);
		}

		public override void TestRequiresPorts()
		{
			AssertEquals(false, WorkflowDescriptor.RequiresPort1);
			AssertEquals(false, WorkflowDescriptor.RequiresPort2);
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

		#region Implementation

		protected override void SetupOrg(OrgHeader org)
		{
			base.SetupOrg(org);

			org.MainAddress.OA_Address1 = "Owner";
			org.MainAddress.OA_Code = "ABCD";
			org.MainAddress.OA_City = "City";
			org.MainAddress.OA_RL_NKRelatedPortCode = "AUSYD";
			org.MainAddress.OA_Email = "tester@cargowise.com";
		}

		protected override IWorkflowProvider[] GetParentsWithConfiguredOrganisationPartiesForTest()
		{
			return new IWorkflowProvider[] { OrgSupplierPartWithConfiguredOrganisationParties, };
		}

		OrgSupplierPart OrgSupplierPartWithConfiguredOrganisationParties
		{
			get { return orgSupplierPartWithConfiguredOrganisationParties ?? (orgSupplierPartWithConfiguredOrganisationParties = GetOrgSupplierPartWithOrgInfo()); }
		}

		OrgSupplierPart GetOrgSupplierPartWithOrgInfo()
		{
			var product = Factory.New<OrgSupplierPart>();
			product.OP_PartNum = "Test Product";

			var relation = product.RelatedOrganisations.AddNew();
			relation.OU_OH = ClientOrg.PK;
			relation.OU_Relationship = OrgPartRelation.RelationshipTypes.Owner;

			return product;
		}

		OrgSupplierPart orgSupplierPartWithConfiguredOrganisationParties;

		protected override string EDIMessageSubType => EDIMessageSubTypeList.Codes.XmlNativeProduct;

		#endregion
	}
}
