using System.Globalization;
using CargoWise.Types;

namespace Enterprise.Freight.Forwarding.DataTransfer
{
	abstract class DateBaseDataFormat : DataFormat<ZDateTime>
	{
		public DateBaseDataFormat(string formatString)
		{
			this.formatString = formatString;
		}

		readonly string formatString;

		public override ZString Format(ZDateTime data, FormattingResult formattingResult)
		{
			if (!data.IsEmpty)
			{
				return data.ToString(this.formatString, CultureInfo.InvariantCulture).ToUpper(CultureInfo.InvariantCulture);
			}

			return ZString.Empty;
		}
	}
}
