using CargoWise.EntityFramework;
using CargoWise.Types;
using NUnit.Framework;
using static Enterprise.Core.Constants;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(GlbCompanyExternalPasswordPHA))]
	public class GlbCompanyExternalPasswordPHATest : GlbExternalPasswordWithPasswordTypeTest<GlbCompanyExternalPasswordPHA>
	{
		public void TestDefaultValues()
		{
			var externalPasswordPHA = Factory.NewWithValidTestData<GlbCompanyExternalPasswordPHA>();
			AssertEquals(nameof(externalPasswordPHA.GP_GS), ZGuid.Empty, externalPasswordPHA.GP_GS);
			AssertNotEquals(nameof(externalPasswordPHA.GP_GC), ZGuid.Empty, externalPasswordPHA.GP_GC);
			AssertEquals(nameof(externalPasswordPHA.GP_GB), ZGuid.Empty, externalPasswordPHA.GP_GB);
		}

		#region Implementation

		public override void TestPasswordTypeCodeAndDescription()
		{
			AssertEquals(PasswordTypesList.Codes.PHA, GlbExternalPassword.PasswordTypeCode);
			AssertEquals(PasswordTypesList.Descriptions.PHA, GlbExternalPassword.PasswordTypeDescription);
		}

		protected override GlbCompanyExternalPasswordPHA CreateNewGlbExternalPassword(BusinessObjectFactory factory)
		{
			var company = Factory.NewCompany(CountryCodes.Philippines);
			return company.WithExternalPassword<GlbCompanyExternalPasswordPHA>();
		}

		#endregion
	}
}
