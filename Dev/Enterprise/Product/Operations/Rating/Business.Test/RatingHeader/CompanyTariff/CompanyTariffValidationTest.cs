using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Rating.Business.Testing
{
	public class CompanyTariffValidationTest : RatingHeaderValidationTest
	{
		public void TestCompanyTariffMustNotHaveOrganisation()
		{
			var header = Factory.NewWithValidTestData<OrgHeader>();
			header.OH_IsCreditor = true;
			Factory.Save();

			var companyTariff = Factory.NewWithValidTestData<CompanyTariff>();
			companyTariff.RunPreSaveValidation();
			AssertNoErrors(companyTariff.TH_OHInfo);

			companyTariff.TH_OH = header.PK;
			companyTariff.RunPreSaveValidation();
			AssertHasError(companyTariff.TH_OHInfo, ErrorMessages.CompanyTariffMustNotHaveOrganisation);
		}

		public void TestCompanyTariffMustNotHaveOrganizationDatabaseConstraint()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();

			var companyTariff = Factory.NewWithValidTestData<CompanyTariff>();
			companyTariff.TH_OH = org.PK;

			var ex = AssertExceptionThrown<ZSaveException>(Factory.Save);

			AssertContains(
				"Company Tariff must not have an organization",
				"Constraint_CompanyTariffMustNotHaveOrganisation",
				ex.GetBaseException().Message,
				true);
		}

		public void TestGlobalTariffMustNotHaveOrganizationDatabaseConstraint()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();

			var globalTariff = Factory.NewWithValidTestData<GlobalTariff>();
			globalTariff.TH_OH = org.PK;

			var ex = AssertExceptionThrown<ZSaveException>(Factory.Save);

			AssertContains(
				"Company Tariff must not have an organization",
				"Constraint_CompanyTariffMustNotHaveOrganisation",
				ex.GetBaseException().Message,
				true);
		}
	}
}
