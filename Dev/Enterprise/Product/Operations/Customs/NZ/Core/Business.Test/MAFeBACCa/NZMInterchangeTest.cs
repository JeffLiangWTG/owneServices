namespace Enterprise.Customs.NZ.Business.MAFeBACCa.Testing
{
	using Enterprise.ZArchitecture.Business.Testing;
	using NUnit.Framework;

	[TestedType(typeof(NZMInterchange))]
	class NZMInterchangeTest : EnterpriseBusinessObjectTestCase
	{
		public void TestDefaultValues()
		{
			NZMInterchange interchange = Factory.New<NZMInterchange>();
			AssertEquals("interchange.EI_ApplicationCode", NZMMessage.ApplicationCodes.NewZealandMAFeBACCa, interchange.EI_ApplicationCode);
		}
	}
}
