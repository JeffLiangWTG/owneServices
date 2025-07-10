using CargoWise.EntityFramework;
using CargoWise.Types;
using NUnit.Framework;
using static Enterprise.Core.Constants;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(GlbCompanyExternalPasswordPHU))]
	public class GlbCompanyExternalPasswordPHUTest : GlbExternalPasswordWithPasswordTypeTest<GlbCompanyExternalPasswordPHU>
	{
		public void TestDefaultValues()
		{
			var externalPasswordPHU = Factory.NewWithValidTestData<GlbCompanyExternalPasswordPHU>();
			AssertEquals(nameof(externalPasswordPHU.GP_GS), ZGuid.Empty, externalPasswordPHU.GP_GS);
			AssertNotEquals(nameof(externalPasswordPHU.GP_GC), ZGuid.Empty, externalPasswordPHU.GP_GC);
			AssertEquals(nameof(externalPasswordPHU.GP_GB), ZGuid.Empty, externalPasswordPHU.GP_GB);
		}

		#region Implementation

		public override void TestPasswordTypeCodeAndDescription()
		{
			AssertEquals(PasswordTypesList.Codes.PHU, GlbExternalPassword.PasswordTypeCode);
			AssertEquals(PasswordTypesList.Descriptions.PHU, GlbExternalPassword.PasswordTypeDescription);
		}

		protected override GlbCompanyExternalPasswordPHU CreateNewGlbExternalPassword(BusinessObjectFactory factory)
		{
			var company = Factory.NewCompany(CountryCodes.Philippines);
			return company.WithExternalPassword<GlbCompanyExternalPasswordPHU>();
		}

		#endregion
	}
}
