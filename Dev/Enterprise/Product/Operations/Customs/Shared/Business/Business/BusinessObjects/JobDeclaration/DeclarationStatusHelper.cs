using System.Collections.Immutable;
using System.Linq;
using CargoWise.Types;
using WTG.StaticAnalysis.Annotation;

namespace Enterprise.Customs.Business
{
	[Immutable]
	public static class DeclarationStatusHelper
	{
		public static bool ShouldCombineEntryStatusFromHeaders(ZString countryCode, ZGuid companyPK)
		{
			return !countryCode.IsEmpty && !IntegratedCountryHelper.IsUsedToBeBuiltInOnlyCountry(countryCode);
		}

		public static bool ShouldCombineMessageStatusFromHeaders(ZString countryCode)
		{
			return !countryCode.IsEmpty && CountriesRequiringCombinedMessageStatusFromHeaders.Contains(countryCode);
		}

		public static readonly ImmutableArray<string> CountriesRequiringCombinedMessageStatusFromHeaders = ImmutableArray.Create(
			Core.Constants.CountryCodes.SouthAfrica,
			Core.Constants.CountryCodes.Taiwan,
			Core.Constants.CountryCodes.Brazil,
			Core.Constants.CountryCodes.Japan,
			Core.Constants.CountryCodes.Switzerland,
			Core.Constants.CountryCodes.Netherlands
		);

		public static bool ShouldExcludeMessageStatusFromModuleGrid(ZString countryCode, ZGuid companyPK)
		{
			return !countryCode.IsEmpty && !IntegratedCountryHelper.IsUsedToBeBuiltInOnlyCountry(countryCode)
				&& !CountriesRequiringMessageStatusInModuleGrid.Contains<string>(countryCode);
		}

		static readonly ImmutableArray<string> CountriesRequiringMessageStatusInModuleGrid = ImmutableArray.Create(
			Core.Constants.CountryCodes.SouthAfrica,
			Core.Constants.CountryCodes.Taiwan,
			Core.Constants.CountryCodes.Brazil,
			Core.Constants.CountryCodes.Italy,
			Core.Constants.CountryCodes.Spain,
			Core.Constants.CountryCodes.Ireland,
			Core.Constants.CountryCodes.Japan,
			Core.Constants.CountryCodes.Switzerland,
			Core.Constants.CountryCodes.Netherlands
			);
	}
}
