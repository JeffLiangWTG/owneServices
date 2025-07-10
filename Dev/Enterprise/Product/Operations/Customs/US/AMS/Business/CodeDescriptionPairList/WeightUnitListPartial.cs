using CargoWise.Types;

namespace Enterprise.Customs.US.AMS.Business
{
	partial class WeightUnitList
	{
		public static ZString ConvertFromFreightWeight(string unit)
		{
			switch (unit)
			{
				case Core.Constants.Weight.Pounds:
					return Codes.Pounds;
				case Core.Constants.Weight.Kilograms:
					return Codes.Kilograms;
				case Core.Constants.Weight.LongTons:
					return Codes.LongTon;
				case Core.Constants.Weight.ShortTons:
					return Codes.ShortTon;
				case Core.Constants.Weight.Tonnes:
					return Codes.MetricTon;
				default:
					return Core.Constants.Weight.ContainsCode(unit) ? Codes.Kilograms : unit;
			}
		}
	}
}
