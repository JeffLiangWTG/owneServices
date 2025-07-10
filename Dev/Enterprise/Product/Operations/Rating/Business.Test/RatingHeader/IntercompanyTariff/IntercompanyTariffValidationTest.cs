using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;

namespace Enterprise.Rating.Business.Testing
{
	public class IntercompanyTariffValidationTest : RatingHeaderValidationTest
	{
		#region TestIntercompanyTariffMustBeGlobal

		public void TestIntercompanyTariffMustBeGlobal()
		{
			var intercompanyTariff = Helper.NewIntercompanyTariff(null);
			intercompanyTariff.TH_GC = Env.CurrentCompanyPK;
			intercompanyTariff.RunPreSaveValidation();

			AssertHasError(intercompanyTariff.TH_GCInfo, "Please do not enter a global company for an intercompany tariff.");
		}

		#endregion

		#region TestIntercompanyTariffMustHaveOrganization

		public void TestIntercompanyTariffOrganisationMustBeAnOrgProxy()
		{
			var nonOrgProxyOrg = Helper.NewOrgHeader();

			var intercompanyTariff = Helper.NewIntercompanyTariff(nonOrgProxyOrg);

			Factory.Save();
			AssertHasError(intercompanyTariff.TH_OHInfo, ErrorMessages.OrgProxyIsMandatoryForIntercompanyTariff);
		}

		public void TestIntercompanyTariffMustHaveOrganization()
		{
			var intercompanyTariff = Helper.NewIntercompanyTariff(null);
			intercompanyTariff.TH_OH = ZGuid.Empty;

			var ex = AssertExceptionThrown<ZSaveException>(Factory.Save);

			AssertContains(
				"An Intercompany Tariff must have an organization",
				"Constraint_SaleRateAndIntercompanyTariffMustHaveOrganisation",
				ex.GetBaseException().Message,
				true);
		}

		#endregion
	}
}
