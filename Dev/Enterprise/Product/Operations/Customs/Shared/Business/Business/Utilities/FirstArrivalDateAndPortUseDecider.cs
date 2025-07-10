using CargoWise.Application;
using CargoWise.Types;

namespace Enterprise.Customs.Business
{
	public static class FirstArrivalDateAndPortUseDecider
	{
		public static bool IsUsed(ZString country, ZString messageType)
		{
			if (ObjectFactory.Get<Integration.Customs.Shared.IEuropeanUnionCustomsMembersProvider>().IsInEuropeanCustomsUnion(country))
			{
				return messageType == JobMessageTypeList.Codes.Import;
			}

			switch (Core.Constants.CountryCodes.GetCustomsCountryOfJurisdiction(country))
			{
				case Core.Constants.CountryCodes.Australia:
				case Core.Constants.CountryCodes.Canada:
					return true;
				case Core.Constants.CountryCodes.UnitedStates:
					return messageType == "FTZ";
				default:
					return false;
			}
		}
	}
}
