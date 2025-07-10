using System;
using System.Globalization;
using TinyCsvParser.TypeConverter;

namespace CargoWise.RefDbRepo.IHSReferenceData.Business.Vessel
{
	class IhsNonNullableByteConverter : BaseConverter<byte>
	{
		readonly IFormatProvider formatProvider;
		readonly NumberStyles numberStyles;

		public IhsNonNullableByteConverter()
			: this(CultureInfo.InvariantCulture)
		{
		}

		public IhsNonNullableByteConverter(IFormatProvider formatProvider)
			: this(formatProvider, NumberStyles.Integer)
		{
		}

		public IhsNonNullableByteConverter(IFormatProvider formatProvider, NumberStyles numberStyles)
		{
			this.formatProvider = formatProvider;
			this.numberStyles = numberStyles;
		}

		public override bool TryConvert(string value, out byte result)
		{
			if (string.IsNullOrWhiteSpace(value))
			{
				result = 0;
				return true;
			}
			return byte.TryParse(value, numberStyles, formatProvider, out result);
		}
	}
}
