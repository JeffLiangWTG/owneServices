using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Customs.US.Business.Testing
{
	class USOrganisationRequirementValidationTest : TestCaseWithFactory
	{
		public void TestValidateUSPPIGovRegNumType()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;

			var invoice = declaration.Invoices.AddNew();
			var docAddress = invoice.USPPIDocAddress;
			docAddress.E2_AddressOverride = true;
			docAddress.E2_GovRegNumType = ZString.Empty;
			docAddress.E2_RN_NKCountryCode = ZString.Empty;
			var validation = docAddress.Validation;
			AssertNoExceptionThrown(() =>
			{
				validation.ValidateE2_GovRegNumType();
			});
			AssertHasMessageError(docAddress.E2_GovRegNumTypeInfo, USOrganisationRequirementValidation.USPPIMustHaveEitherEINOrFRNOrDuns);
			AssertNoMessageError(docAddress.E2_GovRegNumTypeInfo, USOrganisationRequirementValidation.USPPIMustHaveEIN);

			docAddress.E2_RN_NKCountryCode = Core.Constants.CountryCodes.UnitedStates;
			validation.ValidateE2_GovRegNumType();
			AssertNoMessageError(docAddress.E2_GovRegNumTypeInfo, USOrganisationRequirementValidation.USPPIMustHaveEitherEINOrFRNOrDuns);
			AssertHasMessageError(docAddress.E2_GovRegNumTypeInfo, USOrganisationRequirementValidation.USPPIMustHaveEIN);

			docAddress.E2_RN_NKCountryCode = Core.Constants.CountryCodes.PuertoRico;
			validation.ValidateE2_GovRegNumType();
			AssertNoMessageError(docAddress.E2_GovRegNumTypeInfo, USOrganisationRequirementValidation.USPPIMustHaveEitherEINOrFRNOrDuns);
			AssertHasMessageError(docAddress.E2_GovRegNumTypeInfo, USOrganisationRequirementValidation.USPPIMustHaveEIN);

			docAddress.E2_RN_NKCountryCode = Core.Constants.CountryCodes.VirginIslands;
			validation.ValidateE2_GovRegNumType();
			AssertNoMessageError(docAddress.E2_GovRegNumTypeInfo, USOrganisationRequirementValidation.USPPIMustHaveEitherEINOrFRNOrDuns);
			AssertHasMessageError(docAddress.E2_GovRegNumTypeInfo, USOrganisationRequirementValidation.USPPIMustHaveEIN);
		}
	}
}
