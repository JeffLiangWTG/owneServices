using CargoWise.Types;
using Enterprise.Customs.TW.Messaging;

namespace Enterprise.Customs.TW.Manifest.Business
{
	public class N5101HConsigneeAddress : IAddress
	{
		public N5101HConsigneeAddress(AsycudaBill bill)
		{
			this.bill = bill;
		}

		readonly AsycudaBill bill;

		public ZString Line
		{
			get
			{
				var result = new ZStringBuilder();
				result.AppendIfNotEmpty(bill.ABL_ConsigneeStreet1);
				result.AppendIfNotEmpty(bill.ABL_ConsigneeStreet2);
				result.AppendIfNotEmpty(bill.ABL_ConsigneeCity);
				result.AppendIfNotEmpty(bill.ABL_ConsigneeState);
				result.AppendIfNotEmpty(bill.ABL_ConsigneePostcode);
				if (bill.ConsigneeCountry?.RN_DescMultilingual.GetLocalizedValue(Core.SharedConstants.Languages.English) is ZString country)
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
				if (bill.ConsigneeCountry?.RN_DescMultilingual.GetLocalizedValue(Core.SharedConstants.Languages.ChineseTraditional) is ZString country)
				{
					result.AppendIfNotEmpty(country);
				}
				result.AppendIfNotEmpty(bill.ABL_ConsigneeLocalState);
				result.AppendIfNotEmpty(bill.ABL_ConsigneeLocalCity);
				result.AppendIfNotEmpty(bill.ABL_ConsigneeLocalStreet1);
				result.AppendIfNotEmpty(bill.ABL_ConsigneeLocalStreet2);
				result.AppendIfNotEmpty(bill.ABL_ConsigneePostcode);
				return result.ToString();
			}
		}

		public ZString CountryCode => ZString.Empty;

		public ZString CountrySubDivisionID => ZString.Empty;

		public ZString CountrySubDivisionName => ZString.Empty;
	}
}
