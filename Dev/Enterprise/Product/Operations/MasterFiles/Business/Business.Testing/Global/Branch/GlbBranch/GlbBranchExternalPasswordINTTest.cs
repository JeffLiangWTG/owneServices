using CargoWise.EntityFramework;
using CargoWiseOne.ResourceStrings;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(GlbBranchExternalPasswordINT))]
	sealed class GlbBranchExternalPasswordINTTest : GlbBranchCredentialTest<GlbBranchExternalPasswordINT>
	{
		public void TestResourceStringData()
		{
			AssertEquals("User Id", GlbExternalPassword.GP_UserIDInfo.GetAttribute<ResourceStringDataAttribute>().Caption);
			AssertEquals("Password", GlbExternalPassword.CurrentDecryptedPasswordInfo.GetAttribute<ResourceStringDataAttribute>().Caption);
		}

		#region Overrides

		public override void TestPasswordTypeCodeAndDescription()
		{
			AssertEquals(PasswordTypesList.Codes.INT, GlbExternalPassword.PasswordTypeCode);
			AssertEquals(PasswordTypesList.Descriptions.INT, GlbExternalPassword.PasswordTypeDescription);
		}

		protected override GlbBranchExternalPasswordINT CreateNewGlbExternalPassword(BusinessObjectFactory factory)
		{
			return factory.NewWithValidTestData<GlbBranchExternalPasswordINT>();
		}

		#endregion
	}
}
