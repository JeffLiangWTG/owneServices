using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.Universal
{
	public interface IRateCodeLoadCriteria
	{
		ZString[] RateTypesToInclude { get; }
		ZString[] RateTypesToExclude { get; }
		bool? InternalUseRate { get; }
		void AddToRateCodeFilter(ZQuery rateCodeFilter);
	}

	public class RateCodeLoadCriteria : IRateCodeLoadCriteria
	{
		public ZString[] RateTypesToInclude { get; set; }
		public ZString[] RateTypesToExclude { get; set; }
		public bool? InternalUseRate { get; set; }

		void IRateCodeLoadCriteria.AddToRateCodeFilter(ZQuery rateCodeFilter)
		{
			if (InternalUseRate.HasValue)
			{
				rateCodeFilter.AddToFilter(CusRefRateCodeViewSchema.ZY1_InternalUse, InternalUseRate.Value);
			}

			AddRateTypeFilterIfApplicable(rateCodeFilter, RateTypesToInclude, SQLComparisonOperator.Equal);
			AddRateTypeFilterIfApplicable(rateCodeFilter, RateTypesToExclude, SQLComparisonOperator.NotEqual);
		}

		void AddRateTypeFilterIfApplicable(ZQuery filter, ZString[] rateTypes, SQLComparisonOperator comparisonOperator)
		{
			if (rateTypes != null && rateTypes.Length > 0)
			{
				filter.AddToFilter(CusRefRateCodeViewSchema.ZY1_RateType, comparisonOperator, rateTypes);
			}
		}
	}
}
