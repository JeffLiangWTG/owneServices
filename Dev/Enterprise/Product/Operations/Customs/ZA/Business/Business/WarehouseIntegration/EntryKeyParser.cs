using CargoWise.Types;

namespace Enterprise.Customs.ZA.Business
{
	public class EntryKeyGenerator
	{
		public EntryKeyGenerator(ZString entryNumber, ZString districtOfficeCode, ZDateTime clearedDate)
		{
			EntryKey = entryNumber + "/" + districtOfficeCode + "/" + clearedDate.ToString("MM.yy");
		}

		public readonly ZString EntryKey;
	}

	public class EntryKeyParser
	{
		public EntryKeyParser(ZString entryKey)
		{
			this.EntryKey = entryKey;

			int slash1Index = entryKey.IndexOf('/');
			if (slash1Index > 0)
			{
				EntryNumber = entryKey.SubstringSafe(0, slash1Index);

				int slash2Index = entryKey.LastIndexOf('/');
				if (slash2Index > 0)
				{
					DistrictOfficeCode = entryKey.SubstringSafe(slash1Index + 1, slash2Index - slash1Index - 1);
					DatePart = entryKey.SubstringSafe(slash2Index + 1);
				}
			}

			IsValid = !EntryNumber.IsEmpty && !DistrictOfficeCode.IsEmpty && !DatePart.IsEmpty;
		}

		public readonly bool IsValid;
		public readonly ZString EntryKey;
		public readonly ZString EntryNumber;
		public readonly ZString DistrictOfficeCode;
		public readonly ZString DatePart;
	}
}
