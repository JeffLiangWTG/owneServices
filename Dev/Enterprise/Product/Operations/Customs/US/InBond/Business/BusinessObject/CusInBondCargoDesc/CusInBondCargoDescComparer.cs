using CargoWise.Types;

namespace Enterprise.Customs.US.InBond.Business
{
	public static class CusInBondCargoDescComparer
	{
		public static ZString GetOrderKey(CusInBondCargoDesc commodity)
		{
			var result = new ZStringBuilder();
			if (commodity != null)
			{
				result.Append(commodity.BY_HarmonisedTariff.PadLeft(CusInBondCargoDesc.Schema.BY_HarmonisedTariffMaxLength));
				result.Append(commodity.BY_PartNumber.PadRight(CusInBondCargoDesc.Schema.BY_PartNumberMaxLength));
				result.Append(commodity.ChildCommodities.Count.ToString().PadRight(3));
				result.Append(commodity.BY_Description.Left(30));
			}
			return result.ToString();
		}
	}
}
