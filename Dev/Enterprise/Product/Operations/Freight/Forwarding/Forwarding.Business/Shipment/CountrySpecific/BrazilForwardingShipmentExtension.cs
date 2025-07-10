using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.Common;

namespace Enterprise.Freight.Forwarding.Business
{
	public static class BrazilForwardingShipmentExtension
	{
		public static ZString GetBrazilAWBHandlingInformation(this ForwardingShipment shipment)
		{
			if (shipment.IsAir
				&& shipment.Origin != null
				&& shipment.Origin.RL_RN_NKCountryCode == Core.Constants.CountryCodes.Brazil)
			{
				var entryNumbers = shipment.Numbers.Cast<CusEntryNumber>().Where(c => c.CE_RN_NKCountryCode == Core.Constants.CountryCodes.Brazil
					&& c.CE_EntryType == BrazilAdditionalReferenceNumberTypes.Codes.RUC
					&& !c.CE_EntryNum.IsEmpty)
					.Union(
						shipment.CusEntryNumbers.Cast<CusEntryNumber>().Where(c => c.CE_RN_NKCountryCode == Core.Constants.CountryCodes.Brazil
					&& c.CE_EntryType == CusEntryNumberTypes.Brazil.DUE
					&& !c.CE_EntryNum.IsEmpty)
					).Select(c => $"{c.CE_EntryType}:{c.CE_EntryNum}");
				return new ZStringBuilder(entryNumbers).ToStringWithNewLineBetweenAppends();
			}
			return ZString.Empty;
		}
	}
}
