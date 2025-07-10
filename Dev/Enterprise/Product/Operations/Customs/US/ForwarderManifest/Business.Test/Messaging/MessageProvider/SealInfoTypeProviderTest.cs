using CargoWise.Customs.US.MessageContracts.Interfaces;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.US.ForwarderManifest.Business.Test
{
	public class SealInfoTypeProviderTest : TestCaseWithFactory
	{
		public void TestSealNumber()
		{
			provider = new SealInfoTypeProvider("1");

			AssertEquals("1", provider.SealNumber.Value);
		}
		ISealInfoType provider;
	}
}
