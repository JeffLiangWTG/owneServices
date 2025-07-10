using CargoWise.EntityFramework;
using Enterprise.Customs.US.Messaging.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Internal.Testing
{
	[TestedType(typeof(USCarrier))]
	public class USCCarrierTest : EnterpriseBusinessObjectTestCase
	{
		public void TestSetDefault()
		{
			var carrier = Factory.New<USCarrier>();
			AssertEquals("carrier.USC_ModeOfTransportation", TransportModeCodes.Codes.VesselNonContainer, carrier.USC_ModeOfTransportation);
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var carrier = factory.New<USCarrier>();
			carrier.USC_Code = "#@!@";
			return carrier;
		}
	}
}
