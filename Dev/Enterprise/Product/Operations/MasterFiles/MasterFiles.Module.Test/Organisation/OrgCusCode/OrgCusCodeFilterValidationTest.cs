using CargoWise.EntityFramework.Testing;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.MasterFiles.Business.OrgCusCode;

namespace Enterprise.MasterFiles.Module.Testing
{
	sealed class OrgCusCodeFilterValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckProperty()
		{
			Env.Security.OrgDetailsViewPersonalInformation.IsAllowed = true;
			var filter = GetNewFilter();
			filter.Property = USACodeTypes.SocialSecurityNumber;
			AssertNoError(filter.PropertyInfo, errorMsg);

			Env.Security.OrgDetailsViewPersonalInformation.IsAllowed = false;
			filter = GetNewFilter();
			filter.Property = USACodeTypes.SocialSecurityNumber;
			AssertHasError(filter.PropertyInfo, errorMsg);

			filter = GetNewFilter();
			filter.Property = USACodeTypes.EmployerIdentificationNumber;
			AssertNoError(filter.PropertyInfo, errorMsg);
		}

		OrgCusCodeFilter GetNewFilter()
		{
			return new OrgCusCodeFilter("Code Type", OrgCusCodeSchema.OK_CodeType);
		}

		const string errorMsg = "You do not have the appropriate security rights to select this Code. If you require access to this function, ask your system administrator to change either your Staff or Group Security Rights to allow access to: Maintain -> Master Data -> Organization -> View -> View Personal Information.";
	}
}
