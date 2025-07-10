using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.US.Business
{
	static class StateOfOriginDefaulter
	{
		public static void Default(ZPropertyInfo info, JobDocAddress docAddress)
		{
			if (docAddress != null)
			{
				var addressOverridden = docAddress.E2_AddressOverride;
				if (addressOverridden || docAddress.HasRealAddress)
				{
					RefCountry country = null;
					var state = ZString.Empty;

					if (addressOverridden)
					{
						country = docAddress.Country;
						state = docAddress.E2_State;
					}
					else
					{
						var address = docAddress.Address;
						var port = address.EffectiveRelatedPortCode;
						country = port == null ? null : port.Country;
						if (country != null)
						{
							state = address.OA_State;
							if (state.IsEmpty && port.CountryStates != null)
							{
								state = port.CountryStates.RW_Code;
							}
						}
					}

					if (country != null)
					{
						switch (country.RN_Code)
						{
							case Core.Constants.CountryCodes.UnitedStates:
							case Core.Constants.CountryCodes.Canada:
								if (state.Length <= info.MaxLength)
								{
									info.Value = state;
								}
								break;
							case Core.Constants.CountryCodes.Mexico:
								info.Value = new ZString(new AESMexicoStates().GetCodeFromDescription(state));
								break;
						}
					}
				}
			}
		}
	}
}
