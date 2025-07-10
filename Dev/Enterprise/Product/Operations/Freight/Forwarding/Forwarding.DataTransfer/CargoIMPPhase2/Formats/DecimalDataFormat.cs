using System.Globalization;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Freight.Forwarding.DataTransfer
{
	class DecimalDataFormat : DataFormat<ZDecimal>
	{
		public DecimalDataFormat(int max, int decimals)
		{
			this.max = max;
			this.decimals = decimals;
		}

		readonly int max;
		readonly int decimals;

		public override ZString Format(ZDecimal data, FormattingResult formattingResult)
		{
			ZString result = ZString.Empty;
			if (!data.IsEmpty)
			{
				data = Utilities.Round(data, decimals);
				string format = "#." + new string('#', decimals);
				result = data.ToString(format, CultureInfo.InvariantCulture);
				if (result.Length > max)
				{
					result = ZString.Empty;
					if (formattingResult != null)
					{
						formattingResult.IsFormattedCorrectly = false;
						formattingResult.ErrorMessage = Res.GetString("0280360e-be3d-4990-b0e2-7813e0ccf73b", "A decimal number with up to {0} digits and up to {1} decimal places", this.max, this.decimals);
					}
				}
			}

			return result;
		}
	}
}
