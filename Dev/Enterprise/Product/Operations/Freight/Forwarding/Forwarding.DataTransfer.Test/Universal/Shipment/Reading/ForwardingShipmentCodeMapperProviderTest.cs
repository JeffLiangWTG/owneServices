using System.Collections;
using CargoWise.Application;
using CargoWise.EntityFramework.Testing;
using Enterprise.Freight.DataTransfer.Universal;
using Enterprise.Freight.DataTransfer.Universal.Testing;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.Freight.Forwarding.DataTransfer.Testing
{
	sealed class ForwardingShipmentCodeMapperProviderTest : TestCaseWithFactory
	{
		public void TestRegisteredWithObjectFactory()
		{
			var providers = (Hashtable)ObjectFactory.Get("UniversalCodeMapperProviderList");
			var objectHandle = (ObjectHandle)providers[nameof(DataContextType.ForwardingShipment)];

			AssertNotNull("ForwardingShipmentCodeMapperProvider is accessible via ObjectFactory", objectHandle);
		}

		public void TestGetCodeMapperForCO2eResponse()
		{
			var cO2eResponse = CO2eTestHelper.GetSampleCO2eResponseDataObject();
			var provider = new ForwardingShipmentCodeMapperProvider();

			var mapper = provider.Create(cO2eResponse, Factory);
			AssertNotNull("created code mapper for message", mapper);
			AssertType<CO2eUniversalCodeMapper>(mapper);

			var otherShipment = new UniversalDataBuss.DataObjects.Universal.Shipment();
			AssertNull("do not create code mapper for other universal xml message", provider.Create(otherShipment, Factory));
		}
	}
}
