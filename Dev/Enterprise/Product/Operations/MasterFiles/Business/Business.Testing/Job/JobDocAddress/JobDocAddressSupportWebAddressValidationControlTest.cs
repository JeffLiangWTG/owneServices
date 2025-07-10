namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class JobDocAddressSupportWebAddressValidationControlTest : SupportWebAddressValidationTest<JobDocAddress>
	{
		protected override JobDocAddress GetBOToTest()
		{
			var testBO = base.GetBOToTest();
			testBO.E2_AddressOverride = true;
			return testBO;
		}
	}
}
