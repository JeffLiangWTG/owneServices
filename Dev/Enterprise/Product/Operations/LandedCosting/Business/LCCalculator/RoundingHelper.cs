using CargoWise.Types;

namespace Enterprise.LandedCosting.Business
{
	public class RoundingHelper
	{
		/// <summary>
		/// Returns string representation of input value after rounding.
		/// </summary>
		public RoundingHelper()
		{
		}

		public ZDecimal Round(ZDecimal input, int decimals)
		{
			return RoundCore(input, decimals);
		}

		public ZString RoundAndToString(ZDecimal input, int decimals)
		{
			return Round(input, decimals).ToString(decimals);
		}

		protected virtual ZDecimal RoundCore(ZDecimal input, int decimals)
		{
			return ZArchitecture.Core.Utilities.Round(input, decimals);
		}
	}
}
