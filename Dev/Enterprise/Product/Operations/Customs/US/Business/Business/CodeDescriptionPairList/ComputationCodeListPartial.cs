
namespace Enterprise.Customs.US.Business
{
	partial class ComputationCodeList
	{
		public const string CustomComputationCodeForCalculatingIRTax = "_4";

		public static bool IsFirstQuantityRequired(string code)
		{
			return code == Codes.SpecificRateFirstQuantity ||
				code == Codes.MultipleSpecific ||
				code == Codes.CompoundSpecificAndAdValoremFirstQuantity ||
				code == Codes.SpecificPlusCompound ||
				code == Codes.SpecificSpecific ||
				code == Codes.SpecificPyrotechnics ||
				code == Codes.SpecificSugarK;
		}

		public static bool IsSecondQuantityRequired(string code)
		{
			return code == Codes.SpecificRateSecondQuantity ||
				code == Codes.MultipleSpecific ||
				code == Codes.CompoundSpecificAndAdValoremSecondQuantity ||
				code == Codes.SpecificPlusCompound ||
				code == Codes.SpecificFunctionalAdValorem ||
				code == Codes.SpecificCompound ||
				code == Codes.SpecificPyrotechnics ||
				code == Codes.SpecificSugarJ ||
				code == Codes.SpecificSugarK;
		}

		public static bool IsThirdQuantityRequired(string code)
		{
			return code == Codes.FunctionalAdValorem ||
				code == Codes.SpecificFunctionalAdValorem ||
				code == Codes.CompoundSpecificAdValorem ||
				code == Codes.SpecificPlusCompound ||
				code == Codes.SpecificSugarJ;
		}
	}
}
