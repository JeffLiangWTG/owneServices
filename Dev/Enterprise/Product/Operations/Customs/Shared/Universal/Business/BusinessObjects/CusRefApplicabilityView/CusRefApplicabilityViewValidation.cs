using System.Linq;

namespace Enterprise.Customs.Universal
{
	public class CusRefApplicabilityViewValidation : AutoCusRefApplicabilityViewValidation
	{
		public CusRefApplicabilityViewValidation(AutoCusRefApplicabilityView parent) : base(parent)
		{
		}

		public override void ValidateAll()
		{
			if (!Parent.ZZT_IsSystem)
			{
				base.ValidateAll();
			}
		}

		protected new CusRefApplicabilityView Parent => (CusRefApplicabilityView)base.Parent;

		protected override void CheckZZT_StartDate()
		{
			base.CheckZZT_StartDate();

			var parent = Parent;
			var startDate = parent.ZZT_StartDate;
			var propertyInfo = parent.ZZT_StartDateInfo;
			if (startDate > parent.ZZT_EndDate)
			{
				propertyInfo.AddError(Res.GetString("0DE67AA0-9176-40A7-82F1-749111D550C0", "Start Date cannot be later than End Date."));
			}

			var startDateOfRate = parent.Rate?.ZZ2_StartDate;
			if (startDate < startDateOfRate)
			{
				propertyInfo.AddError(Res.GetString("370C23F3-99D2-4273-8C9D-7DB64B38FAFE", "Start Date cannot be earlier than Rate's Start Date."));
			}
		}

		protected override void CheckZZT_StartDateIsValidZDateTimeRange()
		{
		}

		protected override void CheckZZT_EndDate()
		{
			base.CheckZZT_EndDate();

			var parent = Parent;
			var endDate = parent.ZZT_EndDate;
			var propertyInfo = parent.ZZT_EndDateInfo;
			if (endDate < parent.ZZT_StartDate)
			{
				propertyInfo.AddError(Res.GetString("68BA1D9E-8319-4872-98CB-CCD897A3C249", "End Date cannot be earlier than Start Date."));
			}

			var endDateOfRate = parent.Rate?.ZZ2_EndDate;
			if (endDate > endDateOfRate)
			{
				propertyInfo.AddError(Res.GetString("C37F38F6-ED9F-4CED-8419-E7211C447C52", "End Date cannot be later than Rate's End Date."));
			}
		}

		protected override void CheckZZT_EndDateIsValidZDateTimeRange()
		{
		}

		protected override void CheckZZT_OrderNumber()
		{
			base.CheckZZT_OrderNumber();
			var parent = Parent;
			var orderNumber = parent.ZZT_OrderNumber;
			var rate = parent.Rate;
			if (rate != null)
			{
				var startDate = parent.ZZT_StartDate;
				var endDate = parent.ZZT_EndDate;
				var tradeGroup = parent.ZZT_ZZA_TradeGroup;
				var parentPK = parent.PK;
				var propertyInfo = parent.ZZT_OrderNumberInfo;
				var applicabilitiesWithSameTradeGroupAndOrderNumber = rate.RateApplicabilities.Where(x => x.ZZT_ZZA_TradeGroup == tradeGroup && x.ZZT_OrderNumber == orderNumber && x.PK != parentPK).ToArray();
				if (applicabilitiesWithSameTradeGroupAndOrderNumber.Length > 0)
				{
					if (applicabilitiesWithSameTradeGroupAndOrderNumber.Any(x => x.ZZT_StartDate == startDate))
					{
						propertyInfo.AddError(Res.GetString("F14B6782-1908-4DF4-801A-75E335FB5942", "The Applicability with same Start Date, Trade Group and Order already exists."));
					}
					else if (applicabilitiesWithSameTradeGroupAndOrderNumber.Any(x => (startDate < x.ZZT_StartDate && endDate >= x.ZZT_StartDate) || (startDate <= x.ZZT_EndDate && endDate >= x.ZZT_EndDate)))
					{
						propertyInfo.AddError(Res.GetString("AD79490A-8310-477B-9566-E088CF8C9989", "The date range of this Applicability overlaps with another Applicability with same Trade Group and Order."));
					}
				}
			}
		}

		protected override void CheckZZT_AdditionalCodeIsNotEmpty()
		{
		}

		protected override void CheckZZT_OrderNumberIsNotEmpty()
		{
		}

		protected override void CheckZZT_ZZA_TradeGroup()
		{
			var parent = Parent;
			if (parent.ZZT_ZZA_TradeGroup.IsEmpty)
			{
				parent.ZZT_ZZA_TradeGroupInfo.AddError(Res.GetString("A91F0A40-29F5-4A08-9C01-993F383F8377", "The Trade Group should not be empty."));
			}
		}
	}
}
