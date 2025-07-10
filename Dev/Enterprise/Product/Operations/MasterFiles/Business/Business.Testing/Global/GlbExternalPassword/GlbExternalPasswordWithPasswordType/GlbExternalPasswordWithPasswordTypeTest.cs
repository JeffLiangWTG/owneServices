namespace Enterprise.MasterFiles.Business.Testing
{
	public abstract class GlbExternalPasswordWithPasswordTypeTest<T> : GlbExternalPasswordTest<T> where T : GlbExternalPasswordWithPasswordType
	{
		public abstract void TestPasswordTypeCodeAndDescription();

		public void TestAdditionalSetDefaultValues()
		{
			AssertEquals(GlbExternalPassword.PasswordTypeCode, GlbExternalPassword.GP_PasswordType);
		}

		public void TestChangeStatusWhenUpdateCurrentPassword()
		{
			GlbExternalPassword.GP_PasswordStatus = PasswordStatusList.Codes.Valid;
			GlbExternalPassword.GP_CurrentPassword = "New Password";
			AssertNullOrEmpty("GP_PasswordStatus should be cleared", GlbExternalPassword.GP_PasswordStatus);
		}

		public void TestLookupsAndValidationType()
		{
			Assert(GlbExternalPassword.Lookups is GlbExternalPasswordWithPasswordTypeLookups);

			AssertEquals("GlbExternalPassword.Lookups.PasswordTypeList.Count", 1, GlbExternalPassword.Lookups.PasswordTypeList.Count);
			AssertEquals("GlbExternalPassword.Lookups.PasswordTypeList[0].Code", GlbExternalPassword.PasswordTypeCode, GlbExternalPassword.Lookups.PasswordTypeList[0].Code);
			AssertEquals("GlbExternalPassword.Lookups.PasswordTypeList[0].Description", GlbExternalPassword.PasswordTypeDescription, GlbExternalPassword.Lookups.PasswordTypeList[0].Description);

			Assert(GlbExternalPassword.Validation is GlbExternalPasswordWithPasswordTypeValidation);
		}
	}
}
