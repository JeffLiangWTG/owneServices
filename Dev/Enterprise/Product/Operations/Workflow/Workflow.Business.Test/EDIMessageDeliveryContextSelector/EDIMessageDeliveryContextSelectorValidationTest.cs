using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Workflow.Business.Test
{
	public class EDIMessageDeliveryContextSelectorValidationTest : BusinessObjectValidationTestCase
	{
		public void TestECS_Code()
		{
			var bizo = Factory.New<EDIMessageDeliveryContextSelector>();
			bizo.ECS_Code = "";
			AssertMandatoryValidationError(bizo.ECS_CodeInfo, true);
			bizo.ECS_Code = "XX";
			AssertHasError(bizo.ECS_CodeInfo, "Code must be 3 characters in length.");
			bizo.ECS_Code = "X X";
			AssertHasError(bizo.ECS_CodeInfo, "Code cannot contain whitespace.");
			bizo.ECS_Code = "XXX";

			var bizo2 = Factory.New<EDIMessageDeliveryContextSelector>();
			bizo2.ECS_Code = "XXX";
			AssertHasError(bizo2.ECS_CodeInfo, "Code must be unique. Duplicate: XXX.");
		}

		public void TestECS_ProcessType()
		{
			var bizo = Factory.New<EDIMessageDeliveryContextSelector>();
			bizo.ECS_ProcessType = "";
			AssertMandatoryValidationError(bizo.ECS_ProcessTypeInfo, true);
			bizo.ECS_ProcessType = "XXX";
			AssertMandatoryValidationError(bizo.ECS_ProcessTypeInfo, false);
			AssertListValidationInvalidCodeError(bizo.ECS_ProcessTypeInfo, true);
			bizo.ECS_ProcessType = WorkflowDescriptors.AccComplianceReportCode;
			AssertNoErrors(bizo.ECS_ProcessTypeInfo);
		}

		public void TestECS_Description()
		{
			var bizo = Factory.New<EDIMessageDeliveryContextSelector>();
			bizo.ECS_Description = "";
			AssertMandatoryValidationError(bizo.ECS_DescriptionInfo, true);
			bizo.ECS_Description = "XXX";
			AssertNoErrors(bizo.ECS_DescriptionInfo);

			var bizo2 = Factory.New<EDIMessageDeliveryContextSelector>();
			bizo2.ECS_Description = "XXX";
			AssertHasError(bizo2.ECS_DescriptionInfo, "Description must be unique. Duplicate: XXX.");
		}
	}
}
