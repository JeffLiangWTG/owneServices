using System.Globalization;
using CargoWise.Types;

namespace Enterprise.Customs.TW.Business
{
	public class UCRHelper
	{
		public ZString CalculateUCR(CusEntryInstruction cusEntryInstruction)
		{
			var result = ZString.Empty;
			var fUCRPrefix = GetUCRPrefix(cusEntryInstruction);
			if (!fUCRPrefix.IsEmpty)
			{
				result = string.Concat(fUCRPrefix, GetDateString(ZDateTime.Now));
			}
			return result;
		}

		public ZString GetUCRPrefix(CusEntryInstruction cusEntryInstruction)
		{
			var result = ZString.Empty;
			if (cusEntryInstruction != null)
			{
				var vatNumber = GetVATNumber(cusEntryInstruction.EntryHeader);
				if (!vatNumber.IsEmpty)
				{
					var entrySubmittedDate = cusEntryInstruction.CEI_DateForDuty;
					if (!entrySubmittedDate.IsValid)
					{
						entrySubmittedDate = ZDateTime.Now;
					}
					result = string.Concat(GetLastYearChar(entrySubmittedDate), CountryCode, vatNumber);
				}
			}
			return result;
		}

		static ZString GetLastYearChar(ZDateTime date)
		{
			var result = ZString.Empty;
			if (!date.IsEmpty)
			{
				result = date.ToString("yyyy", CultureInfo.CurrentCulture).Substring(3, 1);
			}
			return result;
		}

		static ZString GetDateString(ZDateTime date)
		{
			return date.ToString("yyyyMMddHHmmssfff", CultureInfo.CurrentCulture);
		}

		static ZString CountryCode => Core.Constants.CountryCodes.Taiwan;

		public static ZString GetVATNumber(Customs.Business.CusEntryHeader entryHeader) => entryHeader?.Declaration?.Supplier?.LocalVATCode ?? ZString.Empty;
	}
}
