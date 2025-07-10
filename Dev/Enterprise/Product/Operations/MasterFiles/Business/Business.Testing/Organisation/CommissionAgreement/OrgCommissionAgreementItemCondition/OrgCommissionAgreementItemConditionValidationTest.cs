using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class OrgCommissionAgreementItemConditionValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckUnique()
		{
			var opportunity = Factory.New<OrgOpportunity>();
			var agreement = opportunity.CommissionAgreements.AddNew();
			var item = agreement.ProductItems.AddNew();
			var cond1 = item.ConditionCollection.AddNew();
			var cond2 = item.ConditionCollection.AddNew();

			string error = "It is not possible to save a duplicate entry for the same Product.";

			cond1.CIC_Mode = "AIR";
			cond1.CIC_RL_NKOrigin = "UAIEV";
			cond1.CIC_RL_NKDestination = "AUSYD";

			cond1.Validation.ValidateAll();
			cond2.Validation.ValidateAll();
			AssertNoError(cond1.CIC_ModeInfo, error);
			AssertNoError(cond1.CIC_RL_NKOriginInfo, error);
			AssertNoError(cond1.CIC_RL_NKDestinationInfo, error);
			AssertNoError(cond2.CIC_ModeInfo, error);
			AssertNoError(cond2.CIC_RL_NKOriginInfo, error);
			AssertNoError(cond2.CIC_RL_NKDestinationInfo, error);

			cond2.CIC_Mode = "AIR";
			cond2.CIC_RL_NKOrigin = "UAIEV";
			cond2.CIC_RL_NKDestination = "AUSYD";

			AssertNoError(cond1.CIC_ModeInfo, error);
			AssertNoError(cond1.CIC_RL_NKOriginInfo, error);
			AssertNoError(cond1.CIC_RL_NKDestinationInfo, error);
			AssertNoError(cond2.CIC_ModeInfo, error);
			AssertNoError(cond2.CIC_RL_NKOriginInfo, error);
			AssertHasError(cond2.CIC_RL_NKDestinationInfo, error);

			cond1.Validation.ValidateAll();
			cond2.Validation.ValidateAll();
			AssertHasError(cond1.CIC_ModeInfo, error);
			AssertHasError(cond1.CIC_RL_NKOriginInfo, error);
			AssertHasError(cond1.CIC_RL_NKDestinationInfo, error);
			AssertHasError(cond2.CIC_ModeInfo, error);
			AssertHasError(cond2.CIC_RL_NKOriginInfo, error);
			AssertHasError(cond2.CIC_RL_NKDestinationInfo, error);
		}

		public void TestCheckUnique_Reversed()
		{
			var opportunity = Factory.New<OrgOpportunity>();
			var agreement1 = opportunity.CommissionAgreements.AddNew();
			var item1 = agreement1.ProductItems.AddNew();
			var cond1 = item1.ConditionCollection.AddNew();

			var agreement2 = opportunity.CommissionAgreements.AddNew();
			agreement2.Reverse();
			var item2 = agreement2.ProductItems.AddNew();
			var cond2 = item2.ConditionCollection.AddNew();

			string error = "It is not possible to save a duplicate entry for the same Product.";

			cond1.CIC_Mode = "AIR";
			cond1.CIC_RL_NKOrigin = "UAIEV";
			cond1.CIC_RL_NKDestination = "AUSYD";

			cond1.Validation.ValidateAll();
			cond2.Validation.ValidateAll();
			AssertNoError(cond1.CIC_ModeInfo, error);
			AssertNoError(cond1.CIC_RL_NKOriginInfo, error);
			AssertNoError(cond1.CIC_RL_NKDestinationInfo, error);
			AssertNoError(cond2.CIC_ModeInfo, error);
			AssertNoError(cond2.CIC_RL_NKOriginInfo, error);
			AssertNoError(cond2.CIC_RL_NKDestinationInfo, error);

			cond2.CIC_Mode = "AIR";
			cond2.CIC_RL_NKOrigin = "UAIEV";
			cond2.CIC_RL_NKDestination = "AUSYD";

			AssertNoError(cond1.CIC_ModeInfo, error);
			AssertNoError(cond1.CIC_RL_NKOriginInfo, error);
			AssertNoError(cond1.CIC_RL_NKDestinationInfo, error);
			AssertNoError(cond2.CIC_ModeInfo, error);
			AssertNoError(cond2.CIC_RL_NKOriginInfo, error);
			AssertNoError(cond2.CIC_RL_NKDestinationInfo, error);

			cond1.Validation.ValidateAll();
			cond2.Validation.ValidateAll();
			AssertNoError(cond1.CIC_ModeInfo, error);
			AssertNoError(cond1.CIC_RL_NKOriginInfo, error);
			AssertNoError(cond1.CIC_RL_NKDestinationInfo, error);
			AssertNoError(cond2.CIC_ModeInfo, error);
			AssertNoError(cond2.CIC_RL_NKOriginInfo, error);
			AssertNoError(cond2.CIC_RL_NKDestinationInfo, error);

			agreement2.CA0_ReversedDateUtc = ZDateTime.Empty;
			cond1.Validation.ValidateAll();
			cond2.Validation.ValidateAll();
			AssertHasError(cond1.CIC_ModeInfo, error);
			AssertHasError(cond1.CIC_RL_NKOriginInfo, error);
			AssertHasError(cond1.CIC_RL_NKDestinationInfo, error);
			AssertHasError(cond2.CIC_ModeInfo, error);
			AssertHasError(cond2.CIC_RL_NKOriginInfo, error);
			AssertHasError(cond2.CIC_RL_NKDestinationInfo, error);
		}

		public void TestCheckUnique_CommissionStream()
		{
			const string validationError = "It is not possible to save a duplicate entry for the same Product.";

			var opportunity = Factory.New<OrgOpportunity>();
			var agreement1 = opportunity.CommissionAgreements.AddNew();
			var agreement2 = opportunity.CommissionAgreements.AddNew();
			var item1 = agreement1.ProductItems.AddNew();
			var item2 = agreement2.ProductItems.AddNew();

			var condition1 = item1.ConditionCollection.AddNew();
			condition1.CIC_Mode = "SEA";
			condition1.CIC_RL_NKOrigin = "AUSYD";
			condition1.CIC_RL_NKDestination = "NZAKL";

			var condition2 = item2.ConditionCollection.AddNew();
			condition2.CIC_Mode = "SEA";
			condition2.CIC_RL_NKOrigin = "AUSYD";
			condition2.CIC_RL_NKDestination = "NZAKL";

			agreement1.CA0_CommissionStream = "FRE";
			agreement2.CA0_CommissionStream = "FRE";
			condition1.Validation.ValidateAll();
			condition2.Validation.ValidateAll();

			AssertHasError(condition1.CIC_ModeInfo, validationError);
			AssertHasError(condition1.CIC_RL_NKOriginInfo, validationError);
			AssertHasError(condition1.CIC_RL_NKDestinationInfo, validationError);
			AssertHasError(condition2.CIC_ModeInfo, validationError);
			AssertHasError(condition2.CIC_RL_NKOriginInfo, validationError);
			AssertHasError(condition2.CIC_RL_NKDestinationInfo, validationError);

			agreement1.CA0_CommissionStream = "FRE";
			agreement2.CA0_CommissionStream = "CUS";
			condition1.Validation.ValidateAll();
			condition2.Validation.ValidateAll();

			AssertNoError(condition1.CIC_ModeInfo, validationError);
			AssertNoError(condition1.CIC_RL_NKOriginInfo, validationError);
			AssertNoError(condition1.CIC_RL_NKDestinationInfo, validationError);
			AssertNoError(condition2.CIC_ModeInfo, validationError);
			AssertNoError(condition2.CIC_RL_NKOriginInfo, validationError);
			AssertNoError(condition2.CIC_RL_NKDestinationInfo, validationError);

			agreement1.CA0_CommissionStream = "CUS";
			agreement2.CA0_CommissionStream = "CUS";
			condition1.Validation.ValidateAll();
			condition2.Validation.ValidateAll();

			AssertHasError(condition1.CIC_ModeInfo, validationError);
			AssertHasError(condition1.CIC_RL_NKOriginInfo, validationError);
			AssertHasError(condition1.CIC_RL_NKDestinationInfo, validationError);
			AssertHasError(condition2.CIC_ModeInfo, validationError);
			AssertHasError(condition2.CIC_RL_NKOriginInfo, validationError);
			AssertHasError(condition2.CIC_RL_NKDestinationInfo, validationError);

			agreement1.CA0_CommissionStream = "FRE";
			agreement2.CA0_CommissionStream = string.Empty;
			condition1.Validation.ValidateAll();
			condition2.Validation.ValidateAll();

			AssertNoError(condition1.CIC_ModeInfo, validationError);
			AssertNoError(condition1.CIC_RL_NKOriginInfo, validationError);
			AssertNoError(condition1.CIC_RL_NKDestinationInfo, validationError);
			AssertNoError(condition2.CIC_ModeInfo, validationError);
			AssertNoError(condition2.CIC_RL_NKOriginInfo, validationError);
			AssertNoError(condition2.CIC_RL_NKDestinationInfo, validationError);
		}
	}
}
