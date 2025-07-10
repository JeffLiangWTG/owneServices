using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common;

namespace Enterprise.Customs.NO.Manifest.Business;

static class CusEntryNumberManager
{
	public static void CreateOrUpdateEntry(BusinessObject parent, string entryType, ZString value, ZString countryCode, ref CusEntryNumber cusEntryNumber)
	{
		Argument.NotNull(parent, nameof(parent));
		Argument.NotNullOrEmpty(entryType, nameof(entryType));
		Argument.NotNullOrEmpty(countryCode, nameof(countryCode));

		var hasExistingCusEntryNumber = cusEntryNumber is not null;
		if (hasExistingCusEntryNumber && !cusEntryNumber.IsDeleted)
		{
			cusEntryNumber.CE_EntryNum = value;
			return;
		}

		if (value.IsEmpty)
		{
			return;
		}

		cusEntryNumber = CusEntryNumber.LoadOrCreate(parent, entryType, countryCode);
		cusEntryNumber.CE_EntryNum = value;
	}

	public static void Load(BusinessObject parent, string entryType, ZString countryCode, ref CusEntryNumber cusEntryNumber)
	{
		Argument.NotNull(parent, nameof(parent));
		Argument.NotNullOrEmpty(entryType, nameof(entryType));
		Argument.NotNullOrEmpty(countryCode, nameof(countryCode));

		cusEntryNumber = CusEntryNumber.Load(parent, entryType, countryCode);
	}
}
