using System;
using CargoWise.EntityFramework;
using CargoWiseOne.ResourceStrings;
using NUnit.Framework;
using static Enterprise.Core.Constants;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(GlbCompanyExternalPasswordForPhilippines))]
	public class GlbCompanyExternalPasswordForPhilippinesTest : UserAndClientCredentialsTest<GlbCompanyExternalPasswordPHU, GlbCompanyExternalPasswordPHA>
	{
		public void TestCompany()
		{
			var company = Factory.NewCompany(CountryCodes.Philippines);
			var credentials = GlbCompanyExternalPasswordForPhilippines.New(company);

			AssertEquals(company, credentials.Company);
		}

		public void TestIsAllowed()
		{
			var australianCompany = Factory.NewCompany();
			Assert("Should *not* be allowed for non-Filippino company", !GlbCompanyExternalPasswordForPhilippines.IsAllowed(australianCompany));

			var philippinesCompany = Factory.NewCompany(CountryCodes.Philippines);
			Assert("Should be allowed for Hungary company", GlbCompanyExternalPasswordForPhilippines.IsAllowed(philippinesCompany));
		}

		public void TestNonPhilippinesCompanyThrowsException()
		{
			var australianCompany = Factory.NewCompany(CountryCodes.Australia);
			var expectedMessage = "Parent company country should be PH, not AU.";
			AssertExceptionThrown(typeof(InvalidOperationException), expectedMessage, () => GlbCompanyExternalPasswordForPhilippines.New(australianCompany));
		}

		public void TestNullCompanyThrowsException()
		{
			AssertExceptionThrown(typeof(ArgumentNullException), () => GlbCompanyExternalPasswordForPhilippines.New(null));
		}

		public void TestResourceStringData()
		{
			var company = Factory.NewCompany(CountryCodes.Philippines);
			var credentials = GlbCompanyExternalPasswordForPhilippines.New(company);

			var clientIdResString = credentials.ClientIdInfo.GetAttribute<ResourceStringDataAttribute>();
			AssertEquals("Application Id", clientIdResString.Caption);

			var clientSecretResString = credentials.ClientSecretInfo.GetAttribute<ResourceStringDataAttribute>();
			AssertEquals("Accreditation Id", clientSecretResString.Caption);
		}

		#region Implementation

		protected override UserAndClientCredentials CreateUserAndClientCredentials()
		{
			var company = Factory.NewCompany(CountryCodes.Philippines);
			return GlbCompanyExternalPasswordForPhilippines.New(company);
		}

		protected override BusinessObject GetNewBusinessObject() => CreateUserAndClientCredentials();

		#endregion
	}
}
