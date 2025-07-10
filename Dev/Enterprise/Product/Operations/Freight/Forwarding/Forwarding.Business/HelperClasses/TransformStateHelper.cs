using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Forwarding.Business
{
	public static class TransformStateHelper
	{
		public static ZString TransformState(this ZString stateCode, BusinessObjectFactory factory, ZString countryCode)
		{
			Argument.NotNull(factory, nameof(factory));

			if ((countryCode == Core.Constants.CountryCodes.Japan || countryCode == Core.Constants.CountryCodes.China) && !stateCode.IsEmpty)
			{
				var country = factory.LoadFromNaturalKey<RefCountry>(RefCountrySchema.RN_Code, countryCode);
				if (country != null)
				{
					var stateList = new OrgCodeLists().State_List(country);
					if (stateList[stateCode] is RefCountryStates state && !string.IsNullOrEmpty(state.RW_Description))
					{
						var transformState = state.RW_Description;
						if (countryCode == Core.Constants.CountryCodes.China)
						{
							transformState = state.RW_Description.ToUpper();
						}

						return transformState;
					}
				}
			}

			return stateCode;
		}
	}
}
