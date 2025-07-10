using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.SG.V4.Business.Testing
{
	[TestedType(typeof(GlbExternalPassword_SGv4))]
	class GlbExternalPassword_SGv4Test : GlbExternalPassword_SGTest<GlbExternalPassword_SGv4>
	{
		public void TestPasswordStatusDescription()
		{
			GlbExternalPassword.GP_PasswordStatus = Core.Constants.PasswordOK;
			AssertEquals("Password Valid" + " (" + Core.Constants.PasswordOK + ")", GlbExternalPassword.GP_PasswordStatusDescription);
			GlbExternalPassword.GP_PasswordStatus = SGDeactivationCodes.Codes.FRZ;
			AssertEquals(SGDeactivationCodes.Descriptions.FRZ + " (" + SGDeactivationCodes.Codes.FRZ + ")", GlbExternalPassword.GP_PasswordStatusDescription);
			GlbExternalPassword.GP_PasswordStatus = SGDeactivationCodes.Codes.IID;
			AssertEquals(SGDeactivationCodes.Descriptions.IID + " (" + SGDeactivationCodes.Codes.IID + ")", GlbExternalPassword.GP_PasswordStatusDescription);
		}

		public override void TestSetDefaultValues()
		{
			AssertEquals(GlbCompany.CurrentCompany.PK, GlbExternalPassword.GP_GC);
			AssertEquals(PasswordTypesList.Codes.SG4, GlbExternalPassword.GP_PasswordType);
		}

		public override void TestValidation()
		{
			Assert(GlbExternalPassword.Validation is GlbExternalPasswordValidation_SGv4);
		}
	}
}
