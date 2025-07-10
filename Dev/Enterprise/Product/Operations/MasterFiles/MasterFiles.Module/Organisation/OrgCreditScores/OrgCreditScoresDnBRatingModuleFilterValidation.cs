using System;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.MasterFiles.Module
{
	public class OrgCreditScoresDnBRatingModuleFilterValidation : ModuleTextFilterValidation
	{
		public OrgCreditScoresDnBRatingModuleFilterValidation(OrgCreditScoresDnBRatingModuleFilter parent)
			: base(parent)
		{
			this.parent = parent;
		}

		readonly OrgCreditScoresDnBRatingModuleFilter parent;

		public void ValidateFinancialStrength()
		{
			ValidateCalculatedProperty(parent.FinancialStrengthInfo);
		}

		protected void CheckFinancialStrength()
		{
			ListValidation.ErrorIfInvalidCode(parent.FinancialStrengthInfo);
		}

		public void ValidateCreditAppraisal()
		{
			ValidateCalculatedProperty(parent.CreditAppraisalInfo);
		}

		protected void CheckCreditAppraisal()
		{
			ListValidation.ErrorIfInvalidCode(parent.CreditAppraisalInfo);
		}

		public override void ValidateAll()
		{
			base.ValidateAll();
			ValidateFinancialStrength();
			ValidateCreditAppraisal();
		}

		public override Type AutoValidationType => GetType();
	}
}
