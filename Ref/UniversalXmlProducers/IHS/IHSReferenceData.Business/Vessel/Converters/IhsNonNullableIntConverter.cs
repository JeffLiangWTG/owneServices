using System;
using System.Globalization;
using TinyCsvParser.TypeConverter;

namespace CargoWise.RefDbRepo.IHSReferenceData.Business.Vessel
{
	class IhsNonNullableIntConverter : BaseConverter<int>
	{
		readonly IFormatProvider formatProvider;
		readonly NumberStyles numberStyles;

		public IhsNonNullableIntConverter()
			: this(CultureInfo.InvariantCulture)
		{
		}

		public IhsNonNullableIntConverter(IFormatProvider formatProvider)
			: this(formatProvider, NumberStyles.Integer)
		{
		}

		public IhsNonNullableIntConverter(IFormatProvider formatProvider, NumberStyles numberStyles)
		{
			this.formatProvider = formatProvider;
			this.numberStyles = numberStyles;
		}

		public override bool TryConvert(string value, out int result)
		{
			if (string.IsNullOrWhiteSpace(value))
			{
				result = 0;
				return true;
			}
			return int.TryParse(value, numberStyles, formatProvider, out result);
		}
	}
}
