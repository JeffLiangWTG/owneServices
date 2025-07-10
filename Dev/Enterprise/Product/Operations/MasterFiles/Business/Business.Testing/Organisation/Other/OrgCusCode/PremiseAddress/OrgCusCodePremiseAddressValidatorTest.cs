using CargoWise.EntityFramework.Testing;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class OrgCusCodePremiseAddressValidatorTest : TestCaseWithFactory
	{
		public void TestIsPremiseAddressRequired()
		{
			var validator = new OrgCusCodePremiseAddressValidator();
			AssertEquals("Default false if no Validator defined", false, validator.IsPremiseAddressRequired(OrgCusCode.CodeTypes.ControlledPremisesID, string.Empty));
		}

		public void TestIsPremiseAddressAllowedForGenericCodeType()
		{
			var validator = new OrgCusCodePremiseAddressValidator();
			foreach (var codeType in GenericOrgCusCodeType)
			{
				AssertEquals(true, validator.IsPremiseAddressAllowed(codeType, string.Empty));
			}
		}

		string[] GenericOrgCusCodeType => new[]
		{
			OrgCusCode.CodeTypes.ControlledPremisesID,
			OrgCusCode.CodeTypes.DepotControlledPremisesID,
			OrgCusCode.CodeTypes.WarehouseControlledPremisesID,
			OrgCusCode.CodeTypes.GS1,
			OrgCusCode.CodeTypes.RegulatedAgentID,
			OrgCusCode.CodeTypes.DataUniversalNumberingSystem,
			OrgCusCode.CodeTypes.VGMRegistrationNumber,
			OrgCusCode.CodeTypes.NorthAmericanIndustryClassificationSystem,
			OrgCusCode.CodeTypes.CommercialAndGovernmentEntity,
			OrgCusCode.CodeTypes.TerminalControlledPremisesID,
			OrgCusCode.CodeTypes.PortSystemNumber,
			OrgCusCode.CodeTypes.PortServiceReference,
			OrgCusCode.EuropeanUnionSharedCodeTypes.TraderID,
			OrgCusCode.CodeTypes.NVOCCReference,
			OrgCusCode.CodeTypes.BoleroTitleRegisterID
		};

		public void TestIsPremiseAddressAllowedForConsumptionTaxRegistrationOrgCusCode()
		{
			var validator = new OrgCusCodePremiseAddressValidator();
			AssertEquals(true, validator.IsPremiseAddressAllowed(GermanyOrgCusCodeInfo.OrgCusCodes.UST, Core.Constants.CountryCodes.Germany));
			AssertEquals(true, validator.IsPremiseAddressAllowed(OrgCusCode.CACodeTypes.BusinessNumberForGoodsServicesHarmonizedSalesTax, Core.Constants.CountryCodes.Canada));
			AssertEquals(true, validator.IsPremiseAddressAllowed(OrgCusCode.AustraliaCodeTypes.AustralianBusinessNumber, Core.Constants.CountryCodes.Australia));
		}
	}
}
