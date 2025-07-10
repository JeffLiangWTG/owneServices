namespace Enterprise.Freight.GUI.OnlineSailingSchedules
{
	using System;
	using System.Collections.Generic;
	using CargoWise.Schema;
	using CargoWise.Types;
	using Enterprise.ZArchitecture.Business;

	public class OnlineSchedulesModuleDateFilter : ModuleDateFilter
	{
		public OnlineSchedulesModuleDateFilter(ZString description, SchemaDateTimeColumn filterColumn)
			: base(description, filterColumn)
		{
		}

		public OnlineSchedulesModuleDateFilter(ZString description, GetDateQuery queryDelegate, bool isNullable = true)
			: base(description, queryDelegate, isNullable)
		{
		}

		#region Overridden Methods

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();

			if (!alreadySubscribedFlag)
			{
				alreadySubscribedFlag = true;
				PropertySearchInfo.ValueChanged += PropertySearchInfo_ValueChanged;
			}
		}

		bool alreadySubscribedFlag;

		void PropertySearchInfo_ValueChanged(object sender, EventArgs e)
		{
			if (PropertySearch_List.ContainsCode(PropertySearch))
			{
				SetPropertyValues();

				if (IsPropertySearchUsingSpecifiedDateRange && Property1.IsEmpty)
				{
					Property1 = ZDateTime.Today;
				}
			}
		}

		protected override DateRangePairList CreatePropertySearch_ListCore()
		{
			var codeList = new List<object>()
			{
				DateRangeSearchTexts.Today,
				DateRangeSearchTexts.Tomorrow,
				DateRangeSearchTexts.ThisWeek,
				DateRangeSearchTexts.NextWeek,
				DateRangeSearchTexts.Next7Days,
				DateRangeSearchTexts.Next14Days,
				DateRangeSearchTexts.NextMonth,
				DateRangeSearchTexts.NextCalendarMonth,
				DateRangeSearchTexts.Next2Mths,
				DateRangeSearchTexts.Next3Mths,
				DateRangeSearchTexts.Next6Mths,
				DateRangeSearchTexts.Next12Mths,
				Future,
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

		protected override bool IsEmptyCore => !PropertySearch_List.ContainsCode(PropertySearch) || Property1.IsEmpty && Property2.IsEmpty;

		protected override ModuleFilterValidation GetNewValidation()
		{
			return new ModuleFilterOnlineSchedulesDateValidation(this);
		}

		#endregion
	}
}

