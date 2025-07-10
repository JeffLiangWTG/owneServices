using System.Globalization;
using CargoWise.Types;

namespace Enterprise.Freight.Forwarding.DataTransfer
{
	class NumericDataFormat : DataFormat<ZInt>
	{
		public NumericDataFormat(int min, int max)
		{
			this.min = min;
			this.max = max;
		}

		readonly int min;
		readonly int max;

		public override ZString Format(ZInt data, FormattingResult formattingResult)
		{
			ZString result = ZString.Empty;
			if (!data.IsEmpty || min > 0)
			{
				string format = new string('0', min);
				result = data.ToString(format, CultureInfo.InvariantCulture);
				if (result.Length > max)
				{
					result = ZString.Empty;
					if (formattingResult != null)
					{
						formattingResult.IsFormattedCorrectly = false;
						formattingResult.ErrorMessage = Res.GetString("792519ad-b215-41cb-9797-9fc599366c75", "A number with from {0} to {1} digits", this.min, this.max);
					}
				}
			}

			return result;
		}
	}
}
