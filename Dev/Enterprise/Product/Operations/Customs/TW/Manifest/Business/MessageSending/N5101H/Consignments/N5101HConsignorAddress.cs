using CargoWise.Types;
using Enterprise.Customs.TW.Messaging;

namespace Enterprise.Customs.TW.Manifest.Business
{
	public class N5101HConsignorAddress : IAddress
	{
		public N5101HConsignorAddress(AsycudaBill bill)
		{
			this.bill = bill;
		}

		readonly AsycudaBill bill;

		public ZString Line
		{
			get
			{
				var result = new ZStringBuilder();
				result.AppendIfNotEmpty(bill.ABL_ShipperStreet1);
				result.AppendIfNotEmpty(bill.ABL_ShipperStreet2);
				result.AppendIfNotEmpty(bill.ABL_ShipperCity);
				result.AppendIfNotEmpty(bill.ABL_ShipperState);
				result.AppendIfNotEmpty(bill.ABL_ShipperPostcode);
				if (bill.ShipperCountry?.RN_DescMultilingual.GetLocalizedValue(Core.SharedConstants.Languages.English) is ZString country)
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
				if (bill.ShipperCountry?.RN_DescMultilingual.GetLocalizedValue(Core.SharedConstants.Languages.ChineseTraditional) is ZString country)
				{
					result.AppendIfNotEmpty(country);
				}
				result.AppendIfNotEmpty(bill.ABL_ShipperLocalState);
				result.AppendIfNotEmpty(bill.ABL_ShipperLocalCity);
				result.AppendIfNotEmpty(bill.ABL_ShipperLocalStreet1);
				result.AppendIfNotEmpty(bill.ABL_ShipperLocalStreet2);
				result.AppendIfNotEmpty(bill.ABL_ShipperPostcode);
				return result.ToString();
			}
		}

		public ZString CountryCode => ZString.Empty;

		public ZString CountrySubDivisionID => ZString.Empty;

		public ZString CountrySubDivisionName => ZString.Empty;
	}
}
