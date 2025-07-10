using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Organisation.OpportunityManagement.OrgOpportunity;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.MarketingManager.Business.Testing
{
	sealed class OpportunityCreationTemplateLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestPackageTypeList()
		{
			AssertEquals(typeof(CodeDescriptionPairList), OpportunityCreationTemplate.Lookups.PackageTypeList.GetType());
			AssertEquals(1, OpportunityCreationTemplate.Lookups.PackageTypeList.Count);
			AssertEquals("You can change this list in the registry at Sales & Marketing/Opportunity Management/Product Type/List", OpportunityCreationTemplate.Lookups.PackageTypeList.GetDescriptionFromCode("STD"));
		}

		public void TestOpportunityTypeList()
		{
			AssertEquals(typeof(CodeDescriptionPairList), OpportunityCreationTemplate.Lookups.OpportunityTypeList.GetType());
			AssertEquals(1, OpportunityCreationTemplate.Lookups.OpportunityTypeList.Count);
			AssertEquals("Undefined - You can modify this in the System Registry, under Sales & Marketing/Opportunity Management/Sales Types", OpportunityCreationTemplate.Lookups.OpportunityTypeList.GetDescriptionFromCode("UDF"));
		}

		public void TestOpportunityStatusList()
		{
			AssertEquals(typeof(OpportunityStatusCollection), OpportunityCreationTemplate.Lookups.OpportunityStatusList.GetType());
			AssertEquals(5, OpportunityCreationTemplate.Lookups.OpportunityStatusList.Count);
			AssertEquals("Current", OpportunityCreationTemplate.Lookups.OpportunityStatusList.GetDescriptionFromCode("CRT"));
			AssertEquals("Lost", OpportunityCreationTemplate.Lookups.OpportunityStatusList.GetDescriptionFromCode("LOS"));
			AssertEquals("Abandoned", OpportunityCreationTemplate.Lookups.OpportunityStatusList.GetDescriptionFromCode("ABA"));
			AssertEquals("Suspended", OpportunityCreationTemplate.Lookups.OpportunityStatusList.GetDescriptionFromCode("SUS"));
			AssertEquals("Won", OpportunityCreationTemplate.Lookups.OpportunityStatusList.GetDescriptionFromCode("WON"));
		}

		public void TestOpportunityStageList()
		{
			AssertEquals(typeof(CodeDescriptionPairList), OpportunityCreationTemplate.Lookups.OpportunityStageList.GetType());
			AssertEquals(1, OpportunityCreationTemplate.Lookups.OpportunityStageList.Count);
			AssertEquals("Undefined - You can modify this in the System Registry, under Sales & Marketing/Opportunity Management/Stages", OpportunityCreationTemplate.Lookups.OpportunityStageList.GetDescriptionFromCode("UDF"));
		}

		public void TestActiveSourcesList()
		{
			AssertEquals(typeof(CodeDescriptionPairList), OpportunityCreationTemplate.Lookups.ActiveSourcesList.GetType());
			AssertEquals(4, OpportunityCreationTemplate.Lookups.ActiveSourcesList.Count);
			AssertEquals("Website", OpportunityCreationTemplate.Lookups.ActiveSourcesList.GetDescriptionFromCode("WEB"));
			AssertEquals("Telemarketing Organization", OpportunityCreationTemplate.Lookups.ActiveSourcesList.GetDescriptionFromCode("TMK"));
			AssertEquals("Word Of Mouth", OpportunityCreationTemplate.Lookups.ActiveSourcesList.GetDescriptionFromCode("WOM"));
			AssertEquals("Other", OpportunityCreationTemplate.Lookups.ActiveSourcesList.GetDescriptionFromCode("OTH"));
		}

		public void TestActiveSourceDetailsList()
		{
			AssertEquals(typeof(CodeDescriptionPairList), OpportunityCreationTemplate.Lookups.ActiveSourceDetailsList.GetType());
			AssertEquals(0, OpportunityCreationTemplate.Lookups.ActiveSourceDetailsList.Count);
		}

		public void TestSourceDetailsList()
		{
			AssertEquals(typeof(CodeDescriptionPairList), OpportunityCreationTemplate.Lookups.SourceDetailsList.GetType());
			AssertEquals(0, OpportunityCreationTemplate.Lookups.SourceDetailsList.Count);
		}

		public void TestOpportunityAssignmentList()
		{
			AssertEquals(typeof(OpportunityAssignmentList), OpportunityCreationTemplate.Lookups.OpportunityAssignmentList.GetType());
			AssertEquals(4, OpportunityCreationTemplate.Lookups.OpportunityAssignmentList.Count);
			AssertEquals(OpportunityAssignmentList.Descriptions.IndividualSalesPerson, OpportunityCreationTemplate.Lookups.OpportunityAssignmentList.GetDescriptionFromCode(OpportunityAssignmentList.Codes.IndividualSalesPerson));
			AssertEquals(OpportunityAssignmentList.Descriptions.StaffAssignment, OpportunityCreationTemplate.Lookups.OpportunityAssignmentList.GetDescriptionFromCode(OpportunityAssignmentList.Codes.StaffAssignment));
			AssertEquals(OpportunityAssignmentList.Descriptions.MatchParentTouchSender, OpportunityCreationTemplate.Lookups.OpportunityAssignmentList.GetDescriptionFromCode(OpportunityAssignmentList.Codes.MatchParentTouchSender));
			AssertEquals(OpportunityAssignmentList.Descriptions.StaffPoolAssignments, OpportunityCreationTemplate.Lookups.OpportunityAssignmentList.GetDescriptionFromCode(OpportunityAssignmentList.Codes.StaffPoolAssignments));
		}

		public void TestSalesPersonList()
		{
			var staffCollection = new GlbStaffCollection(Factory).ToList();

			AssertEquals(typeof(GlbStaffCollection), OpportunityCreationTemplate.Lookups.SalesPersonList.GetType());
			AssertEquals(staffCollection.Count, OpportunityCreationTemplate.Lookups.SalesPersonList.Count);

			foreach (var salePerson in OpportunityCreationTemplate.Lookups.SalesPersonList)
			{
				AssertCollectionContains(salePerson, staffCollection);
			}
		}

		public void TestStaffAssignmentList()
		{
			AssertEquals(typeof(CodeDescriptionPairList), OpportunityCreationTemplate.Lookups.StaffAssignmentList.GetType());
			AssertEquals(8, OpportunityCreationTemplate.Lookups.StaffAssignmentList.Count);
			AssertEquals("Account Manager", OpportunityCreationTemplate.Lookups.StaffAssignmentList.GetDescriptionFromCode("ACT"));
			AssertEquals("Cartage Coordinator", OpportunityCreationTemplate.Lookups.StaffAssignmentList.GetDescriptionFromCode("CAR"));
			AssertEquals("Controller", OpportunityCreationTemplate.Lookups.StaffAssignmentList.GetDescriptionFromCode("CON"));
			AssertEquals("Credit Controller", OpportunityCreationTemplate.Lookups.StaffAssignmentList.GetDescriptionFromCode("CRE"));
			AssertEquals("Customer Service Representative", OpportunityCreationTemplate.Lookups.StaffAssignmentList.GetDescriptionFromCode("CUS"));
			AssertEquals("Customs Agent", OpportunityCreationTemplate.Lookups.StaffAssignmentList.GetDescriptionFromCode("CAG"));
			AssertEquals("Project Manager", OpportunityCreationTemplate.Lookups.StaffAssignmentList.GetDescriptionFromCode("PRJ"));
			AssertEquals("Sales Representative", OpportunityCreationTemplate.Lookups.StaffAssignmentList.GetDescriptionFromCode("SAL"));
		}

		public void TestOverallDispositionList()
		{
			AssertEquals(typeof(OrgOpportunityOverallDispositionList), OpportunityCreationTemplate.Lookups.OverallDispositionList.GetType());
			AssertEquals(2, OpportunityCreationTemplate.Lookups.OverallDispositionList.Count);
			AssertEquals("Open", OpportunityCreationTemplate.Lookups.OverallDispositionList.GetDescriptionFromCode("OPN"));
			AssertEquals("Closed", OpportunityCreationTemplate.Lookups.OverallDispositionList.GetDescriptionFromCode("CLS"));
		}

		OpportunityCreationTemplate OpportunityCreationTemplate
		{
			get
			{
				if (opportunityCreationTemplate == null)
				{
					opportunityCreationTemplate = new OpportunityCreationTemplate(Factory.New<GlbCompanyCampaign>());
				}
				return opportunityCreationTemplate;
			}
		}

		OpportunityCreationTemplate opportunityCreationTemplate;
	}
}
