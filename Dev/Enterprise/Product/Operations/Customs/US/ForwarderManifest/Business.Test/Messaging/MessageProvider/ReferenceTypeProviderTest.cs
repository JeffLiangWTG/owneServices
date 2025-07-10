using CargoWise.Customs.US.MessageContracts.Interfaces;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.US.ForwarderManifest.Business.Test
{
	public class ReferenceTypeProviderTest : TestCaseWithFactory
	{
		public void TestReferenceType()
		{
			provider = new ReferenceTypeProvider("A", "B");
			AssertEquals("A", provider.ReferenceTypeCode.Value);
		}

		public void TestReferenceData()
		{
			provider = new ReferenceTypeProvider("A", "B");
			AssertEquals("B", provider.ReferenceData.Value);
		}
		IReferenceType provider;
	}
}
