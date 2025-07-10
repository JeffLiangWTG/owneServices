using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.ZA.Module
{
	public class CustomsResponseReceivedDateFilter : ModuleDateFilter
	{
		public CustomsResponseReceivedDateFilter(ZString description)
			: base(description, EDIMessageSchema.EM_SystemCreateTimeUtc, true)
		{
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();

			PropertySearch = SpecifiedDateRange;
			Property1 = ZDateTime.UtcToday.AddDays(-4);
			Property2 = ZDateTime.UtcToday;
		}

		protected override DateRangePairList CreatePropertySearch_ListCore()
		{
			var codeList = new List<object>()
				{
					DateRangeSearchTexts.Today,
					DateRangeSearchTexts.Yesterday,
					DateRangeSearchTexts.ThisWeek,
					DateRangeSearchTexts.LastWeek,
					DateRangeSearchTexts.Last7Days,
					SpecifiedDateRange
				};

			var basePropertySearchList = base.CreatePropertySearch_ListCore();
			var propertySearchList = new DateRangePairList();

			foreach (var code in codeList)
			{
				var index = basePropertySearchList.IndexOfCode(code);

				if (index != -1)
				{
					propertySearchList.Add(basePropertySearchList[index]);
				}
			}

			return propertySearchList;
		}

		protected override ModuleFilterValidation GetNewValidation()
		{
			return new CustomsResponseReceivedDateFilterValidation(this);
		}
	}
}
