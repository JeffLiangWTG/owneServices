using CargoWise.EntityFramework;
using CargoWiseOne.ResourceStrings;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(GlbBranchExternalPasswordINS))]
	sealed class GlbBranchExternalPasswordINSTest : GlbBranchCredentialTest<GlbBranchExternalPasswordINS>
	{
		public void TestResourceStringData()
		{
			AssertEquals("Client Id", GlbExternalPassword.GP_UserIDInfo.GetAttribute<ResourceStringDataAttribute>().Caption);
			AssertEquals("Client Secret", GlbExternalPassword.CurrentDecryptedPasswordInfo.GetAttribute<ResourceStringDataAttribute>().Caption);
		}

		#region Overrides

		public override void TestPasswordTypeCodeAndDescription()
		{
			AssertEquals(PasswordTypesList.Codes.INS, GlbExternalPassword.PasswordTypeCode);
			AssertEquals(PasswordTypesList.Descriptions.INS, GlbExternalPassword.PasswordTypeDescription);
		}

		protected override GlbBranchExternalPasswordINS CreateNewGlbExternalPassword(BusinessObjectFactory factory)
		{
			return factory.NewWithValidTestData<GlbBranchExternalPasswordINS>();
		}

		#endregion
	}
}
