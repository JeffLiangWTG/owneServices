using CargoWise.Types;
using Enterprise.Core;
using Enterprise.ZArchitecture.Web.GUI;

namespace Enterprise.Tracking.Web
{
	public static class DeclarationColumnProviderTypeDecider
	{
		public static GridColumnProvider GetColumnProviderForCountry(ZString countryCode)
		{
			switch (countryCode)
			{
				case Constants.CountryCodes.UnitedStates:
					return new USDeclarationModuleColumnProvider();
				case Constants.CountryCodes.Canada:
					return new CADeclarationModuleColumnProvider();
				case Constants.CountryCodes.NewZealand:
					return new NZDeclarationModuleColumnProvider();
				default:
					return new BaseDeclarationModuleColumnProvider();
			}
		}
	}
}
