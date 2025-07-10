using CargoWise.Types;

namespace Enterprise.Customs.SG.V4.Business
{
	public class SGTariffRate
	{
		public ZDecimal PercentageRate { get; set; }
		public ZDecimal UnitRate { get; set; }
		public ZString UnitQty { get; set; }

		public SGTariffRate()
		{
		}

		public SGTariffRate(ZDecimal percentageRate, ZDecimal unitRate, ZString unitQty)
		{
			PercentageRate = percentageRate;
			UnitRate = unitRate;
			UnitQty = unitQty;
		}

		public override bool Equals(object obj)
			=> obj is SGTariffRate other &&
					other.PercentageRate == PercentageRate &&
					other.UnitRate == UnitRate &&
					other.UnitQty == UnitQty;

		public override int GetHashCode() => PercentageRate.GetHashCode() ^ UnitRate.GetHashCode() ^ UnitQty.GetHashCode();

		public override string ToString()
			=> System.FormattableString.Invariant($"PercentageRate={PercentageRate:0.00}, UnitRate={UnitRate:0.00}, UnitQty={UnitQty}");
	}
}
