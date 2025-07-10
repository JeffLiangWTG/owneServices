using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Universal;

namespace Enterprise.Customs.US.Business.Testing
{
	sealed class USCForeignPortWrapperTest : TestCaseWithFactory
	{
		public void TestUSCForeignPortWrapper()
		{
			var foreignPort = Factory.New<ZZRefCusCodeListCombined>();
			foreignPort.ZZD_Code = "12345";
			foreignPort.ZZD_Description = "Test";
			IForeignRegionalDistrictPort wrapper = new USCForeignPortWrapper(foreignPort);
			AssertEquals(foreignPort.ZZD_Code, wrapper.PortCode);
			AssertEquals(foreignPort.ZZD_Description, wrapper.PortName);
		}
	}
}
