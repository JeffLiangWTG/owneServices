using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.US.Business
{
	public class USCTariffDateRestrictionCollection : ActiveBusinessObjectCollection<USCTariffDateRestriction>
	{
		public USCTariffDateRestrictionCollection(USCTariff tariff)
			: base(tariff)
		{
		}

		public ZString GetAllRestrictionDateDescriptions(ZInt year)
		{
			ZStringBuilder result = new ZStringBuilder();

			foreach (USCTariffDateRestriction restriction in this)
			{
				result.Append(restriction.RestrictionDateDescription);
			}

			return result.ToStringWithDelimiterBetweenAppends(", ");
		}

		public USCTariffDateRestriction GetRestrictionFor(ZDateTime date)
		{
			USCTariffDateRestriction result = null;

			foreach (USCTariffDateRestriction restriction in this)
			{
				if (restriction.IsPassedDateWithinRestrictedDates(date.Date))
				{
					result = restriction;
					break;
				}
			}

			return result;
		}

		public USCTariffDateRestriction GetRestrictionDateFor(ZString restrictionCode, ZShort dateFrom, ZShort dateTo)
		{
			USCTariffDateRestriction result = null;

			foreach (USCTariffDateRestriction restriction in this)
			{
				if (restriction.UF_EntryDateRestrictionFrom == dateFrom &&
					restriction.UF_EntryDateRestrictionTo == dateTo &&
					restriction.UF_EntryDateRestrictionCode == restrictionCode)
				{
					result = restriction;
					break;
				}
			}

			return result;
		}

		public USCTariffDateRestriction CreateWithRestrictionDatesIfNotExist(ZString restrictionCode, ZShort dateFrom, ZShort dateTo)
		{
			USCTariffDateRestriction result = GetRestrictionDateFor(restrictionCode, dateFrom, dateTo);

			if (result == null)
			{
				result = AddNew();
				result.UF_EntryDateRestrictionCode = restrictionCode;
				result.UF_EntryDateRestrictionFrom = dateFrom;
				result.UF_EntryDateRestrictionTo = dateTo;
			}

			return result;
		}
	}
}
