namespace Enterprise.Freight.Forwarding.Business.AWB
{
	public struct FreightTaxes
	{
		public FreightTaxes(decimal prepaidTax, decimal collectTax)
		{
			PrepaidTax = prepaidTax;
			CollectTax = collectTax;
		}

		public decimal PrepaidTax { get; }
		public decimal CollectTax { get; }

		#region Overrides

		public override int GetHashCode()
		{
			unchecked
			{
				var hashCode = 2030672487;
				hashCode = hashCode * -1521134295 + PrepaidTax.GetHashCode();
				hashCode = hashCode * -1521134295 + CollectTax.GetHashCode();
				return hashCode;
			}
		}

		public override bool Equals(object obj)
		{
			return obj is FreightTaxes taxes
				&& PrepaidTax == taxes.PrepaidTax
				&& CollectTax == taxes.CollectTax;
		}

		public static bool operator ==(FreightTaxes left, FreightTaxes right)
		{
			return left.Equals(right);
		}

		public static bool operator !=(FreightTaxes left, FreightTaxes right)
		{
			return !left.Equals(right);
		}

		#endregion
	}
}
