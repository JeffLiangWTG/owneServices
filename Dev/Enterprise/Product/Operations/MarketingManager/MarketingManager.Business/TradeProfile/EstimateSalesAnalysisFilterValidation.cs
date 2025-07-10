using CargoWise.EntityFramework;

namespace Enterprise.MarketingManager.Business
{
	public class EstimateSalesAnalysisFilterValidation : AutoEstimateSalesAnalysisFilterValidation
	{
		public EstimateSalesAnalysisFilterValidation(AutoEstimateSalesAnalysisFilter parent)
			: base(parent)
		{
		}

		protected override void CheckStatus()
		{
			base.CheckStatus();
			ListValidation.ErrorIfInvalidCode(Parent.StatusInfo);
		}
	}
}
