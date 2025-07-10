using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.Common;

namespace Enterprise.Freight.Forwarding.Business
{
	public static class BrazilForwardingConsolExtension
	{
		public static ZString GetBrazilAWBHandlingInformation(this ForwardingConsol consol)
		{
			if (consol.IsExportFrom(Core.Constants.CountryCodes.Brazil))
			{
				var refNumber = consol.Numbers.Cast<CusEntryNumber>().FirstOrDefault(c => c.CE_RN_NKCountryCode == Core.Constants.CountryCodes.Brazil
					&& c.CE_EntryType == BrazilAdditionalReferenceNumberTypes.Codes.RUC);
				if (refNumber != null && !refNumber.CE_EntryNum.IsEmpty)
				{
					return new ZString($"{refNumber.CE_EntryType}:{refNumber.CE_EntryNum}");
				}
			}
			return ZString.Empty;
		}
	}
}
