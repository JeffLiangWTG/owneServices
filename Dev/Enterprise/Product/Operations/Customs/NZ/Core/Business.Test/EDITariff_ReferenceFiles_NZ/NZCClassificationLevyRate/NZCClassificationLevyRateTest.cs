namespace Enterprise.Customs.NZ.Business.EDITariff_ReferenceFiles_NZ.Testing
{
	using Enterprise.ZArchitecture.Business.Testing;
	using NUnit.Framework;

	[TestedType(typeof(NZCClassificationLevyRate))]
	class NZCClassificationLevyRateTest : EnterpriseBusinessObjectTestCase
	{
		public void TestExistsInCurrentDataVersion()
		{
			AssertEquals("NZCClassificationLevyRate.ExistsInCurrentDataVersion", true, NZCClassificationLevyRate.ExistsInCurrentDataVersion);
		}
	}
}
