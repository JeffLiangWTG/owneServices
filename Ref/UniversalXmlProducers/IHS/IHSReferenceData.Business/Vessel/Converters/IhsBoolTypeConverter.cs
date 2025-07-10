using System;
using TinyCsvParser.TypeConverter;

namespace CargoWise.RefDbRepo.IHSReferenceData.Business.Vessel
{
	class IhsBoolTypeConverter : BaseConverter<bool>
	{
		readonly ITypeConverter<bool> converter;

		public IhsBoolTypeConverter()
			: this(new BoolConverter())
		{
		}

		public IhsBoolTypeConverter(ITypeConverter<bool> converter)
		{
			this.converter = converter;
		}

		public override bool TryConvert(string value, out bool result)
		{
			result = false;

			if (string.Equals("Y", value, StringComparison.OrdinalIgnoreCase))
			{
				result = true;
			}

			return true;
		}
	}
}
