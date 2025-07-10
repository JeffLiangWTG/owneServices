namespace Enterprise.MasterFiles.Business
{
	class OrgCusCodePremiseAddressValidatorFactory
	{
		public CountrySpecificOrgCusCodePremiseAddressValidator GetValidator(string countryCode)
		{
			CountrySpecificOrgCusCodePremiseAddressValidator result = null;

			var jurisdictionCountryCode = Core.Constants.CountryCodes.GetCustomsCountryOfJurisdiction(countryCode);

			switch (jurisdictionCountryCode)
			{
				case Core.Constants.CountryCodes.UnitedKingdom:
					result = new UKOrgCusCodePremiseAddressValidator();
					break;
				case Core.Constants.CountryCodes.UnitedStates:
					result = new USOrgCusCodePremiseAddressValidator();
					break;
				case Core.Constants.CountryCodes.Germany:
					result = new DEOrgCusCodePremiseAddressValidator();
					break;
				case Core.Constants.CountryCodes.NewZealand:
					result = new NZOrgCusCodePremiseAddressValidator();
					break;
				case Core.Constants.CountryCodes.Canada:
					result = new CAOrgCusCodePremiseAddressValidator();
					break;
				case Core.Constants.CountryCodes.Denmark:
					result = new DKOrgCusCodePremiseAddressValidator();
					break;
				case Core.Constants.CountryCodes.Australia:
					result = new AUOrgCusCodePremiseAddressValidator();
					break;
				case Core.Constants.CountryCodes.Italy:
					result = new ITOrgCusCodePremiseAddressValidator();
					break;
				case Core.Constants.CountryCodes.Taiwan:
					result = new TWOrgCusCodePremiseAddressValidator();
					break;
				case Core.Constants.CountryCodes.France:
					result = new FROrgCusCodePremiseAddressValidator();
					break;
				case Core.Constants.CountryCodes.China:
					result = new CNOrgCusCodePremiseAddressValidator();
					break;
				case Core.Constants.CountryCodes.KoreaSouth:
					result = new KROrgCusCodePremiseAddressValidator();
					break;
				case Core.Constants.CountryCodes.SouthAfrica:
					result = new ZAOrgCusCodePremiseAddressValidator();
					break;
				case Core.Constants.CountryCodes.Turkey:
					result = new TROrgCusCodePremiseAddressValidator();
					break;
				case Core.Constants.CountryCodes.Spain:
					result = new ESOrgCusCodePremiseAddressValidator();
					break;
				case Core.Constants.CountryCodes.Poland:
					result = new PLOrgCusCodePremiseAddressValidator();
					break;
				case Core.Constants.CountryCodes.Japan:
					result = new JPOrgCusCodePremiseAddressValidator();
					break;
				case Core.Constants.CountryCodes.Netherlands:
					result = new NLOrgCusCodePremiseAddressValidator();
					break;
				case Core.Constants.CountryCodes.Switzerland:
					result = new CHOrgCusCodePremiseAddressValidator();
					break;
				case Core.Constants.CountryCodes.India:
					result = new INOrgCusCodePremiseAddressValidator();
					break;
				default:
					result = null;
					break;
			}

			if (result == null)
			{
				if (Core.Constants.CountryCodes.IsUsaOrTerritory(countryCode))
				{
					result = new USOrgCusCodePremiseAddressValidator();
				}
				else if (Core.Constants.CountryCodes.IsFranceOrTerritoryNotReunion(countryCode))
				{
					result = new FROrgCusCodePremiseAddressValidator();
				}
			}
			return result;
		}
	}
}

