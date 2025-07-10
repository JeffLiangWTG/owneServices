using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core.Encryption;
using NUnit.Framework;

namespace Enterprise.Customs.NZ.Business.Declaration.Testing
{
	[TestedType(typeof(GlbExternalPassword_NZ))]
	public class GlbExternalPassword_NZTest : Enterprise.MasterFiles.Business.Testing.GlbExternalPasswordTest<GlbExternalPassword_NZ>
	{
		public void TestSetDefaultValues()
		{
			AssertEquals(GlbCompany.CurrentCompany.PK, GlbExternalPassword.GP_GC);
			AssertEquals(PasswordTypesList.Codes.NZB, GlbExternalPassword.GP_PasswordType);
		}

		public void TestCurrentDecryptedPasswordMaxLength()
		{
			AssertEquals(35, GlbExternalPassword.CurrentDecryptedPasswordInfo.MaxLength);
		}

		public void TestSetNewBrokerPassword()
		{
			GlbExternalPassword.GP_GS = Staff.PK;
			GlbExternalPassword.CurrentDecryptedPassword = "TEST";
			GlbExternalPassword.GP_UserID = "MMM";

			Factory.Save();

			TwoWayEncoder encoder = new TwoWayEncoder(Staff.PK.ToGuid());
			AssertEquals("MMM", GlbExternalPassword.GP_UserID);
			AssertEquals("Broker password should be encrypted", encoder.Encrypt("TEST"), GlbExternalPassword.GP_CurrentPassword);
		}
	}
}
