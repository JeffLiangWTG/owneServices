using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.US.Business
{
	public class USOrganisationRequirementValidation
	{
		public USOrganisationRequirementValidation()
		{
		}

		public void ValidateUSPPIGovRegNumType(JobDocAddressValidation jobDocAddressValidation)
		{
			var docAddress = jobDocAddressValidation.Parent;
			if (docAddress.E2_AddressOverride)
			{
				ListValidation.MessageErrorIfInvalidCode(docAddress.E2_GovRegNumTypeInfo);

				if (docAddress.E2_AddressType == DocAddressTypes.Codes.USPrincipalPartyInInterest && docAddress.E2_GovRegNumType.IsEmpty)
				{
					var countryCode = docAddress.E2_RN_NKCountryCode;
					if (countryCode == Core.Constants.CountryCodes.UnitedStates
						|| countryCode == Core.Constants.CountryCodes.PuertoRico
						|| countryCode == Core.Constants.CountryCodes.VirginIslands)
					{
						docAddress.E2_GovRegNumTypeInfo.AddMessageError(USPPIMustHaveEIN);
					}
					else
					{
						docAddress.E2_GovRegNumTypeInfo.AddMessageError(USPPIMustHaveEitherEINOrFRNOrDuns);
					}
				}
			}
		}
		internal const string USPPIMustHaveEitherEINOrFRNOrDuns = "This USPPI does not have an Employer Identification Number or a Foreign Registration Number or a DUNS configured. Please press F3 in the field and go to Config > Registration Numbers/Codes.";
		internal const string USPPIMustHaveEIN = "This USPPI does not have an Employer Identification Number. Please press F3 in the field and go to Config > Registration Numbers/Codes.";

		public void ValidateRegistrationNumber(JobDocAddressValidation jobDocAddressValidation)
		{
			var docAddress = jobDocAddressValidation.Parent;
			if (docAddress.E2_AddressOverride)
			{
				switch (docAddress.E2_GovRegNumType)
				{
					case OrgCusCode.USACodeTypes.EmployerIdentificationNumber:
						OrgCusCodeValidation.ValidateEINNumber(docAddress.E2_GovRegNumInfo);
						break;
					case OrgCusCode.USACodeTypes.ForeignRegistrationNumber:
						OrgCusCodeValidation.ValidateForeignRegistrationNumber(docAddress.E2_GovRegNumInfo);
						break;
					case OrgCusCode.CodeTypes.DataUniversalNumberingSystem:
						ZString message = DataUniversalNumberingSystemValidator.GetDUNSNumberError(docAddress.E2_GovRegNum);
						if (!message.IsEmpty)
						{
							docAddress.E2_GovRegNumInfo.AddWarning(message);
						}
						break;
				}
			}
		}
	}
}
