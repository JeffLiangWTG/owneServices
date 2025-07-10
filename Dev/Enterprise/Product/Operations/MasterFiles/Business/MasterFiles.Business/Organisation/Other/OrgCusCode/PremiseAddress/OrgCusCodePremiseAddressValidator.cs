using Enterprise.ZArchitecture.Environment;

namespace Enterprise.MasterFiles.Business
{
	sealed class OrgCusCodePremiseAddressValidator
	{
		public bool IsPremiseAddressAllowed(string codeType, string countryCode)
		{
			bool result = IsPremiseAddressRequired(codeType, countryCode);

			if (!result)
			{
				result = IsPremiseAddressAllowedForGenericCodeType(codeType, countryCode);
			}

			if (!result)
			{
				result = IsPremiseAddressAllowedForCountrySpecificCodeType(codeType, countryCode);
			}

			return result;
		}

		public bool IsPremiseAddressRequired(string codeType, string countryCode)
		{
			var result = IsPremiseAddressRequiredForGenericCodeType(codeType);
			if (!result)
			{
				var validator = new OrgCusCodePremiseAddressValidatorFactory().GetValidator(countryCode);
				result = validator?.IsPremiseAddressRequired(codeType) ?? false;
			}
			return result;
		}

		bool IsPremiseAddressRequiredForGenericCodeType(string codeType)
		{
			switch (codeType)
			{
				case OrgCusCode.CodeTypes.ContainerChainCommunityCode:
				case OrgCusCode.EuropeanUnionSharedCodeTypes.CustomsOfficeForExit:
				case OrgCusCode.EuropeanUnionSharedCodeTypes.TrustedTrader:
					return true;
				default:
					return false;
			}
		}

		bool IsPremiseAddressAllowedForGenericCodeType(string codeType, string countryCode)
		{
			switch (codeType)
			{
				case OrgCusCode.CodeTypes.ControlledPremisesID:
				case OrgCusCode.CodeTypes.DepotControlledPremisesID:
				case OrgCusCode.CodeTypes.WarehouseControlledPremisesID:
				case OrgCusCode.CodeTypes.GS1:
				case OrgCusCode.CodeTypes.RegulatedAgentID:
				case OrgCusCode.CodeTypes.DataUniversalNumberingSystem:
				case OrgCusCode.CodeTypes.VGMRegistrationNumber:
				case OrgCusCode.CodeTypes.NorthAmericanIndustryClassificationSystem:
				case OrgCusCode.CodeTypes.CommercialAndGovernmentEntity:
				case OrgCusCode.CodeTypes.TerminalControlledPremisesID:
				case OrgCusCode.CodeTypes.PortSystemNumber:
				case OrgCusCode.CodeTypes.PortServiceReference:
				case OrgCusCode.EuropeanUnionSharedCodeTypes.TraderID:
				case OrgCusCode.CodeTypes.NVOCCReference:
				case OrgCusCode.CodeTypes.CustomsOfficeForTransit:
				case OrgCusCode.CodeTypes.BoleroTitleRegisterID:
					return true;
				default:
					return codeType == Country.GetConsumptionTaxRegistrationOrgCusCode(countryCode);
			}
		}

		bool IsPremiseAddressAllowedForCountrySpecificCodeType(string codeType, string countryCode)
		{
			var validator = new OrgCusCodePremiseAddressValidatorFactory().GetValidator(countryCode);
			return validator?.IsPremiseAddressAllowed(codeType) ?? false;
		}
	}

	abstract class CountrySpecificOrgCusCodePremiseAddressValidator
	{
		public bool IsPremiseAddressAllowed(string codeType) => IsPremiseAddressRequired(codeType) || IsPremiseAddressAllowedWithoutBeingRequired(codeType);

		public abstract bool IsPremiseAddressRequired(string codeType);

		protected virtual bool IsPremiseAddressAllowedWithoutBeingRequired(string codeType) => false;
	}
}
