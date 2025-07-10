
//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.InBond.Business
{
	public class WeightUnitList : CodeDescriptionPairList
	{
		public static class Codes
		{
			public const string Kilograms = Core.Constants.Weight.Kilograms;
			public const string Pounds = Core.Constants.Weight.Pounds;
		}

		public static class Descriptions
		{
			public const string Kilograms = "Kilograms";
			public const string Pounds = "Pounds";
		}

		public WeightUnitList()
		{
			AddPair(Codes.Kilograms, Descriptions.Kilograms);
			AddPair(Codes.Pounds, Descriptions.Pounds);
		}
	}
}
