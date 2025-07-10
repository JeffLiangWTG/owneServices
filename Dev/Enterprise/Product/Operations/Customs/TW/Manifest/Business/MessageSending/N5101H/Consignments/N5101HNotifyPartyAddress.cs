using CargoWise.Types;
using Enterprise.Customs.TW.Messaging;

namespace Enterprise.Customs.TW.Manifest.Business
{
	public class N5101HNotifyPartyAddress : IAddress
	{
		public N5101HNotifyPartyAddress(AsycudaBill bill)
		{
			this.bill = bill;
		}

		readonly AsycudaBill bill;

		public ZString Line
		{
			get
			{
				var result = new ZStringBuilder();
				result.AppendIfNotEmpty(bill.ABL_NotifyPartyStreet1);
				result.AppendIfNotEmpty(bill.ABL_NotifyPartyStreet2);
				result.AppendIfNotEmpty(bill.ABL_NotifyPartyCity);
				result.AppendIfNotEmpty(bill.ABL_NotifyPartyState);
				result.AppendIfNotEmpty(bill.ABL_NotifyPartyPostcode);
				if (bill.NotifyPartyCountry?.RN_DescMultilingual.GetLocalizedValue(Core.SharedConstants.Languages.English) is ZString country)
				{
					result.AppendIfNotEmpty(country);
				}
				return result.ToStringWithDelimiterBetweenAppends(" ");
			}
		}

		public ZString ChineseLine
		{
			get
			{
				var result = new ZStringBuilder();
				result.AppendIfNotEmpty(bill.ABL_NotifyPartyPostcode);
				var refCountry = bill.NotifyPartyCountry;
				if (refCountry != null)
				{
					if (refCountry.RN_DescMultilingual.GetLocalizedValue(Core.SharedConstants.Languages.ChineseTraditional) is ZString country)
					{
						result.AppendIfNotEmpty(country);
					}
					if (refCountry.Code != Core.Constants.CountryCodes.Taiwan)
					{
						result.AppendIfNotEmpty(bill.ABL_NotifyPartyLocalState);
					}
				}
				result.AppendIfNotEmpty(bill.ABL_NotifyPartyLocalCity);
				result.AppendIfNotEmpty(bill.ABL_NotifyPartyLocalStreet1);
				result.AppendIfNotEmpty(bill.ABL_NotifyPartyLocalStreet2);
				return result.ToString();
			}
		}

		public ZString CountryCode => ZString.Empty;

		public ZString CountrySubDivisionID => ZString.Empty;

		public ZString CountrySubDivisionName => ZString.Empty;
	}
}
