using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.SG.V4.Business.Testing
{
	public abstract class GlbExternalPassword_SGTest<T> : MasterFiles.Business.Testing.GlbExternalPasswordTest<T> where T : GlbExternalPassword_SG
	{
		public virtual void TestSetDefaultValues()
		{
			AssertEquals(GlbCompany.CurrentCompany.PK, GlbExternalPassword.GP_GC);
		}

		public void TestCurrentDecryptedPasswordMaxLength()
		{
			AssertEquals(32, GlbExternalPassword.CurrentDecryptedPasswordInfo.MaxLength);
		}

		public void TestNextDecryptedPasswordMaxLength()
		{
			AssertEquals(32, GlbExternalPassword.NextDecryptedPasswordInfo.MaxLength);
		}

		public virtual void TestValidation()
		{
			Assert(GlbExternalPassword.Validation is GlbExternalPasswordValidation_SG);
		}
	}
}
