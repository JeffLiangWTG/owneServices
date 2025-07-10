using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.US.DIS.Business.Testing
{
	[TestedType(typeof(EDIInterchange))]
	sealed class EDIInterchangeTest : EnterpriseBusinessObjectTestCase
	{
		public void TestSetDefaultValues()
		{
			var interchange = Factory.New<EDIInterchange>();
			AssertEquals(EDIInterchange.ApplicationCodes.USCustomsDIS, interchange.EI_ApplicationCode);
		}

		public void TestSendViaEHub()
		{
			var interchange = Factory.New<EDIInterchange>();
			interchange.EI_ReceiveTransmit = EDIInterchange.Direction.Transmit;
			interchange.EI_Status = EDIInterchange.Status.Queued;
			AssertEquals(EDIInterchange.Status.eHubQueued, interchange.EI_Status);
			AssertEquals(EDIInterchange.TransportType.eHub, interchange.EI_TransportType);
		}
	}
}
