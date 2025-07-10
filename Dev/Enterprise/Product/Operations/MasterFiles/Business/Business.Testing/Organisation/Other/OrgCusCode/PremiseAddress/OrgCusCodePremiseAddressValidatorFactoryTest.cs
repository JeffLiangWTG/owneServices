using System.Linq;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class OrgCusCodePremiseAddressValidatorFactoryTest : NUnit.Framework.TestCase
	{
		public void TestGetValidator_USTerritory()
		{
			var factory = new OrgCusCodePremiseAddressValidatorFactory();

			foreach (var countryCode in Core.Constants.CountryCodes.UsaAndTerritoriesList)
			{
				AssertEquals(typeof(USOrgCusCodePremiseAddressValidator), factory.GetValidator(countryCode).GetType());
			}
		}

		public void TestGetValidator_FRTerritory()
		{
			var factory = new OrgCusCodePremiseAddressValidatorFactory();

			var frTerritories = Core.Constants.CountryCodes.FranceAndOverseasDepartmentsAndTerritories.Where(x => x != Core.Constants.CountryCodes.Reunion);

			foreach (var countryCode in frTerritories)
			{
				AssertEquals(typeof(FROrgCusCodePremiseAddressValidator), factory.GetValidator(countryCode).GetType());
			}
		}

		public void TestGetValidator_PremiseAddressValidatorType()
		{
			var factory = new OrgCusCodePremiseAddressValidatorFactory();
			CombineAssertions(() =>
			{
				AssertType<PLOrgCusCodePremiseAddressValidator>(factory.GetValidator(Core.Constants.CountryCodes.Poland));
				AssertType<AUOrgCusCodePremiseAddressValidator>(factory.GetValidator(Core.Constants.CountryCodes.Australia));
				AssertType<CAOrgCusCodePremiseAddressValidator>(factory.GetValidator(Core.Constants.CountryCodes.Canada));
				AssertType<CNOrgCusCodePremiseAddressValidator>(factory.GetValidator(Core.Constants.CountryCodes.China));
				AssertType<DEOrgCusCodePremiseAddressValidator>(factory.GetValidator(Core.Constants.CountryCodes.Germany));
				AssertType<DKOrgCusCodePremiseAddressValidator>(factory.GetValidator(Core.Constants.CountryCodes.Denmark));
				AssertType<ESOrgCusCodePremiseAddressValidator>(factory.GetValidator(Core.Constants.CountryCodes.Spain));
				AssertType<ITOrgCusCodePremiseAddressValidator>(factory.GetValidator(Core.Constants.CountryCodes.Italy));
				AssertType<KROrgCusCodePremiseAddressValidator>(factory.GetValidator(Core.Constants.CountryCodes.KoreaSouth));
				AssertType<NZOrgCusCodePremiseAddressValidator>(factory.GetValidator(Core.Constants.CountryCodes.NewZealand));
				AssertType<TROrgCusCodePremiseAddressValidator>(factory.GetValidator(Core.Constants.CountryCodes.Turkey));
				AssertType<TWOrgCusCodePremiseAddressValidator>(factory.GetValidator(Core.Constants.CountryCodes.Taiwan));
				AssertType<UKOrgCusCodePremiseAddressValidator>(factory.GetValidator(Core.Constants.CountryCodes.UnitedKingdom));
				AssertType<ZAOrgCusCodePremiseAddressValidator>(factory.GetValidator(Core.Constants.CountryCodes.SouthAfrica));
				AssertType<NLOrgCusCodePremiseAddressValidator>(factory.GetValidator(Core.Constants.CountryCodes.Netherlands));
				AssertType<INOrgCusCodePremiseAddressValidator>(factory.GetValidator(Core.Constants.CountryCodes.India));
			});
		}
	}
}
