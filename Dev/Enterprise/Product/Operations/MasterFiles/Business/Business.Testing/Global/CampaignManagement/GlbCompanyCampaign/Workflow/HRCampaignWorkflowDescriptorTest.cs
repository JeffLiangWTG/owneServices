using System;
using CargoWise.Integration;
using Enterprise.Integration.Recruiter;
using Enterprise.Registry.Business;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(HRCampaignWorkflowDescriptor))]
	sealed class HRCampaignWorkflowDescriptorTest : WorkflowDescriptorTestCase<HRCampaignWorkflowDescriptor>
	{
		public override void TestID()
		{
			AssertEquals("Correct Code", "HRC", WorkflowDescriptor.Code);
		}

		public override void TestDescription()
		{
			AssertEquals("Correct Desc", "Human Resources Campaign", WorkflowDescriptor.Description);
		}

		public override void TestSubTypes()
		{
			ICodeDescriptionPairList type1List = OrganisationsDataRegistry.Instance.HRCampaignCategory1List.Value;
			ICodeDescriptionPairList type2List = OrganisationsDataRegistry.Instance.HRCampaignCategory2List.Value;
			OrganisationsDataRegistry.Instance.HRCampaignCategory1Label.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "My Category 1");
			OrganisationsDataRegistry.Instance.HRCampaignCategory2Label.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "My Category 2");
			AssertEquals("2 sub types", 2, WorkflowDescriptor.SubTypeInformation.Length);

			AssertEquals("Sub Type 1 is Category 1", "My Category 1", WorkflowDescriptor.SubTypeInformation[0].Description);
			AssertListEquals("Correct List", type1List, WorkflowDescriptor.SubTypeInformation[0].List);

			AssertEquals("Sub Type 2 is Category 2", "My Category 2", WorkflowDescriptor.SubTypeInformation[1].Description);
			AssertListEquals("Correct List", type2List, WorkflowDescriptor.SubTypeInformation[1].List);
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
			get { return MessageRecipientPartyType.OrgProxy | MessageRecipientPartyType.Email; }
		}

		protected override IWorkflowProvider[] GetParentsWithConfiguredOrganisationPartiesForTest()
		{
			return new IWorkflowProvider[]
			{
				(IWorkflowProvider)Factory.New<IHRGlbCompanyCampaign>()
			};
		}

		#region Implementation

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
