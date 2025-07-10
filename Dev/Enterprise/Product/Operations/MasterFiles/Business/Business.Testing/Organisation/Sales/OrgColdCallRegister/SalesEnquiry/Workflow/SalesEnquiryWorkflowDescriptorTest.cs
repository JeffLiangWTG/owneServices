using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Schema;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(SalesEnquiryWorkflowDescriptor))]
	sealed class SalesEnquiryWorkflowDescriptorTest : WorkflowDescriptorTestCase<SalesEnquiryWorkflowDescriptor>
	{
		public override void TestID()
		{
			AssertEquals("Correct Code", "INQ", WorkflowDescriptor.Code);
		}

		public override void TestDescription()
		{
			AssertEquals("Correct Desc", "Sales Inquiry", WorkflowDescriptor.Description);
		}

		public override void TestSubTypes()
		{
			var sourceList = OrganisationsDataRegistry.Instance.OpportunitySource.Value;
			var orgRelationshipList = new SalesEnquiryOrgRelationshipCodeList();
			AssertEquals("sub types", 3, WorkflowDescriptor.SubTypeInformation.Length);

			var subType = WorkflowDescriptor.SubTypeInformation[0];
			AssertEquals("Sub Type 0 is Inquiry Type", "Inquiry Type", subType.Description);
			AssertListEquals("Correct List", SalesEnquiryLookups.GetAllEnquiryTypes(), subType.List);

			subType = WorkflowDescriptor.SubTypeInformation[1];
			AssertEquals("Sub Type 2 is Source", "Source", subType.Description);
			AssertListEquals("Correct List", sourceList, subType.List);

			subType = WorkflowDescriptor.SubTypeInformation[2];
			AssertEquals("Sub Type 3 is Org", "Organization", subType.Description);
			AssertListEquals("Correct List", orgRelationshipList, subType.List);
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

		public override void TestRequiresDepartment()
		{
			AssertEquals(false, WorkflowDescriptor.RequiresDepartment);
		}

		public override void TestRequiresBranch()
		{
			AssertEquals(false, WorkflowDescriptor.RequiresBranch);
		}

		public override void TestSupportsEventTracking()
		{
			AssertEquals(true, WorkflowDescriptor.SupportsEventTracking);
		}

		protected override bool ExpectingTasksToBeCompanySpecific
		{
			get { return false; }
		}

		protected override MessageRecipientPartyType ExpectedSupportedMessageRecipientParties
		{
			get { return MessageRecipientPartyType.OrgProxy | MessageRecipientPartyType.Client | MessageRecipientPartyType.Email; }
		}

		protected override SchemaColumn[] ExpectedWorkflowTriggerFieldColumns
		{
			get { return new SchemaColumn[] { OrgColdCallRegisterSchema.O1_LeadStatus }; }
		}

		#region Implementation

		#region OrgEnquiryWithConfiguredOrganisationParties

		protected override BusinessObject NewBusinessObjectInTable(ITableSchema table)
		{
			return Factory.New<SalesEnquiry>();
		}

		protected override IWorkflowProvider[] GetParentsWithConfiguredOrganisationPartiesForTest()
		{
			return new IWorkflowProvider[]
			{
				OrgEnquiryWithConfiguredOrganisationParties,
			};
		}

		SalesEnquiry OrgEnquiryWithConfiguredOrganisationParties
		{
			get
			{
				if (orgEnquiryWithConfiguredOrganisationParties == null)
				{
					orgEnquiryWithConfiguredOrganisationParties = Factory.New<SalesEnquiry>();
					orgEnquiryWithConfiguredOrganisationParties.OrgPk = ClientOrg.PK;
				}
				return orgEnquiryWithConfiguredOrganisationParties;
			}
		}

		SalesEnquiry orgEnquiryWithConfiguredOrganisationParties;

		#endregion

		void AssertListEquals(string message, ICodeDescriptionPairList expectedList, ICodeDescriptionPairList actualList)
		{
			AssertEquals("Same number of items", expectedList.Count, actualList.Count);
			for (int i = 0; i < expectedList.Count; i++)
			{
				AssertEquals(message + " - Code for item " + i.ToString(), ((ICodeDescription)expectedList[i]).Code, ((ICodeDescription)actualList[i]).Code);
				AssertEquals(message + " - Description for item " + i.ToString(), ((ICodeDescription)expectedList[i]).Description, ((ICodeDescription)actualList[i]).Description);
			}
		}

		#endregion
	}
}
