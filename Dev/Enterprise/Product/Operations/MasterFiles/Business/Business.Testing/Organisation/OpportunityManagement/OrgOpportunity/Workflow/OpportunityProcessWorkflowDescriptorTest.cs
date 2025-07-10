using System;
using CargoWise.Integration;
using CargoWise.Schema;
using Enterprise.Messaging.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(OpportunityWorkflowDescriptor))]
	sealed class OpportunityProcessWorkflowDescriptorTest : WorkflowDescriptorTestCase<OpportunityWorkflowDescriptor>
	{
		public override void TestID()
		{
			AssertEquals("Correct Code", "OPP", WorkflowDescriptor.Code);
		}

		public override void TestDescription()
		{
			AssertEquals("Correct Desc", "Sales Opportunity", WorkflowDescriptor.Description);
		}

		public override void TestSubTypes()
		{
			var collection1 = new CodeDescriptionBoolCollection();
			collection1.Add("AAA", (NoResString)"Type A", true);
			OrganisationsDataRegistry.Instance.OpportunitySalesTypes.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, collection1);

			var collection2 = new CodeDescriptionBoolRelatedItemCollection();
			collection2.Add("BBB", (NoResString)"Source B");
			OrganisationsDataRegistry.Instance.OpportunitySource.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, collection2);

			OrganisationsDataRegistry.Instance.ProductTypeLabel.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, (NoResString)"Label C");

			var collection3 = new CodeDescriptionBoolCollection();
			collection3.Add("DDD", (NoResString)"Type List D", true);
			OrganisationsDataRegistry.Instance.ProductTypeList.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, collection3);

			ICodeDescriptionPairList typeList = OrganisationsDataRegistry.Instance.OpportunitySalesTypes.Value.GetCodeDescriptionPairList();
			ICodeDescriptionPairList sourceList = OrganisationsDataRegistry.Instance.OpportunitySource.Value;
			string productTypeLabel = OrganisationsDataRegistry.Instance.ProductTypeLabel.Value;
			ICodeDescriptionPairList productTypeList = OrganisationsDataRegistry.Instance.ProductTypeList.Value;
			AssertEquals("3 sub types", 3, WorkflowDescriptor.SubTypeInformation.Length);

			AssertEquals("Sub Type 1 is Opportunity Type", "Sales Type", WorkflowDescriptor.SubTypeInformation[0].Description);
			AssertListEquals("Correct List", typeList, WorkflowDescriptor.SubTypeInformation[0].List);

			AssertEquals("Sub Type 2 is Source", "Source", WorkflowDescriptor.SubTypeInformation[1].Description);
			AssertListEquals("Correct List", sourceList, WorkflowDescriptor.SubTypeInformation[1].List);

			AssertEquals("Sub Type 3 is based on product type label", productTypeLabel, WorkflowDescriptor.SubTypeInformation[2].Description);
			AssertListEquals("Correct List", productTypeList, WorkflowDescriptor.SubTypeInformation[2].List);
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
			get { return new SchemaColumn[] { OrgOpportunitySchema.P8_Status }; }
		}

		#region Implementation

		#region OrgOpportunityWithConfiguredOrganisationParties

		protected override IWorkflowProvider[] GetParentsWithConfiguredOrganisationPartiesForTest()
		{
			return new IWorkflowProvider[]
			{
				OrgOpportunityWithConfiguredOrganisationParties,
			};
		}

		OrgOpportunity OrgOpportunityWithConfiguredOrganisationParties
		{
			get
			{
				if (orgOpportunityWithConfiguredOrganisationParties == null)
				{
					orgOpportunityWithConfiguredOrganisationParties = Factory.New<OrgOpportunity>();
					orgOpportunityWithConfiguredOrganisationParties.P8_OH = ClientOrg.PK;
				}
				return orgOpportunityWithConfiguredOrganisationParties;
			}
		}

		OrgOpportunity orgOpportunityWithConfiguredOrganisationParties;

		#endregion

		protected override string EDIMessageSubType => EDIMessageSubTypeList.Codes.XmlNativeOpportunity;

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
