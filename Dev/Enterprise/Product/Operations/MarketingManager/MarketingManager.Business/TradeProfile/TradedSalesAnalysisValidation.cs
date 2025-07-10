using CargoWise.EntityFramework;
namespace Enterprise.MarketingManager.Business
{
	public class TradedSalesAnalysisValidation : AutoTradedSalesAnalysisValidation
	{
		public TradedSalesAnalysisValidation(AutoTradedSalesAnalysis parent)
			: base(parent)
		{
		}

		protected override void CheckPeriod()
		{
			base.CheckPeriod();
			ListValidation.ErrorIfInvalidCode(Parent.PeriodInfo);
		}

		protected override void CheckMainGroupingType()
		{
			base.CheckMainGroupingType();
			ListValidation.ErrorIfInvalidCode(Parent.MainGroupingTypeInfo);
		}

		protected override void CheckLocationGroupingType()
		{
			base.CheckLocationGroupingType();
			ListValidation.ErrorIfInvalidCode(Parent.LocationGroupingTypeInfo);
		}
	}
}
