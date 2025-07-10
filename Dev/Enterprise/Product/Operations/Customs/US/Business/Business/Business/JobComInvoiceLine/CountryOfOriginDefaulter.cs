using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.US.Business
{
	class CountryOfOriginDefaulter
	{
		public void Default(ZPropertyInfo countryOfOriginInfo, OrgAddress shipperOrManufacturerAddress)
		{
			if (shipperOrManufacturerAddress != null)
			{
				OrgHeader organisation = shipperOrManufacturerAddress.Header;

				if (organisation != null && !organisation.IsMiscellaneous)
				{
					BusinessObjectFactory factory = organisation.Factory;

					ZString defaultValue = ZString.Empty;

					ZString mid = shipperOrManufacturerAddress.CustomsCodes.GetCustomsRegNo(OrgCusCode.USACodeTypes.ManufacturerID, Core.Constants.CountryCodes.UnitedStates);

					if (!mid.IsEmpty)
					{
						USCCountry country = factory.LoadFromNaturalKey<USCCountry>(USCCountrySchema.UC_Code, mid.Left(2));

						if (country != null)
						{
							defaultValue = mid.Left(2);
						}
					}
					else
					{
						RefCountry countryOfOrigin = factory.LoadFromNaturalKey<RefCountry>(RefCountrySchema.RN_Code, organisation.MiscServ.OM_RN_NKEXDefaultCntryOfOrigin);
						defaultValue = countryOfOrigin != null ? countryOfOrigin.RN_Code : organisation.OH_RL_NKClosestPort.Left(2);

						if (defaultValue == Core.Constants.CountryCodes.Canada)// 'CA' is never a valid C/O
						{
							defaultValue = ZString.Empty;
						}
					}

					if (!defaultValue.IsEmpty)
					{
						countryOfOriginInfo.Value = defaultValue;
					}
				}
			}
		}
	}
}
