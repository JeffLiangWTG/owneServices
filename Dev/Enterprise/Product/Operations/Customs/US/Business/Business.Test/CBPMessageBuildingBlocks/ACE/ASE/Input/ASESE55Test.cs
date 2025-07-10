using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.US.Business.BIRD.ACE;

namespace Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.ACE.Input.Testing
{
	sealed class ASESE55Test : TestCaseWithFactory
	{
		public void TestAddressDetail()
		{
			var asese55 = new ASESE35()
			{
				AddressInformation = "ADDRESS"
			};

			AssertEquals("ADDRESS", ((IACEBIRDOrgAddressRecord)asese55).Address1);
		}
	}
}
