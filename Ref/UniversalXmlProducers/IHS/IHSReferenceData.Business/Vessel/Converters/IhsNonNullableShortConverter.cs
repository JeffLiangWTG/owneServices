using System;
using System.Globalization;
using TinyCsvParser.TypeConverter;

namespace CargoWise.RefDbRepo.IHSReferenceData.Business.Vessel
{
	class IhsNonNullableShortConverter : BaseConverter<short>
	{
		readonly IFormatProvider formatProvider;
		readonly NumberStyles numberStyles;

		public IhsNonNullableShortConverter()
			: this(CultureInfo.InvariantCulture)
		{
		}

		public IhsNonNullableShortConverter(IFormatProvider formatProvider)
			: this(formatProvider, NumberStyles.Integer)
		{
		}

		public IhsNonNullableShortConverter(IFormatProvider formatProvider, NumberStyles numberStyles)
		{
			this.formatProvider = formatProvider;
			this.numberStyles = numberStyles;
		}

		public override bool TryConvert(string value, out short result)
		{
			if (string.IsNullOrWhiteSpace(value))
			{
				result = 0;
				return true;
			}
			return short.TryParse(value, numberStyles, formatProvider, out result);
		}
	}
}
