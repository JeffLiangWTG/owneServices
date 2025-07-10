using CargoWise.Types;
using Enterprise.MasterFiles.Business.Organisation.OpportunityManagement.OrgOpportunity;
using NUnit.Framework;

namespace Enterprise.MarketingManager.Business.Testing
{
	sealed class OpportunityCreationTemplateTestHelper
	{
		public static OpportunityCreationTemplate PopulateOpportunityCreationTemplate(OpportunityCreationTemplate template, string opportunityDescription)
		{
			template.PackageType = "STD";
			template.OpportunityType = "UDF";
			template.OpportunityDescription = opportunityDescription;
			template.OpportunityStatus = "CRT";
			template.OpportunityStage = "UDF";
			template.Source = "WEB";
			template.ActiveSourceDetails = "NOT";
			template.OpportunityAssignment = OpportunityAssignmentList.Codes.MatchParentTouchSender;

			return template;
		}

		public static void AssertTemplateValuesEmpty(string message, OpportunityCreationTemplate template)
		{
			AssertTemplateValues(message, template, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty);
		}
		public static void AssertTemplateValues(string message, OpportunityCreationTemplate template, ZString packageType, ZString opportunityType, ZString opportunityDescription, ZString opportunityStatus, ZString opportunityStage, ZString source, ZString activeSourceDetails, ZString opportunityAssignment)
		{
			Assertion.CombineAssertions(message, () =>
			{
				Assertion.AssertEquals(packageType, template.PackageType);
				Assertion.AssertEquals(opportunityType, template.OpportunityType);
				Assertion.AssertEquals(opportunityDescription, template.OpportunityDescription);
				Assertion.AssertEquals(opportunityStatus, template.OpportunityStatus);
				Assertion.AssertEquals(opportunityStage, template.OpportunityStage);
				Assertion.AssertEquals(source, template.Source);
				Assertion.AssertEquals(activeSourceDetails, template.ActiveSourceDetails);
				Assertion.AssertEquals(opportunityAssignment, template.OpportunityAssignment);
			});
		}
	}
}
