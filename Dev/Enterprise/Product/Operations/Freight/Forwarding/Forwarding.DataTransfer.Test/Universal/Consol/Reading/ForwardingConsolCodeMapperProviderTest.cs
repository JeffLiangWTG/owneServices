using System.Collections;
using CargoWise.Application;
using CargoWise.EntityFramework.Testing;
using Enterprise.Freight.DataTransfer.Universal;
using Enterprise.Freight.DataTransfer.Universal.Testing;
using Enterprise.UniversalDataBuss.Integration;
using UniversalShipment = Enterprise.UniversalDataBuss.DataObjects.Universal.Shipment;

namespace Enterprise.Freight.Forwarding.DataTransfer.Testing
{
	sealed class ForwardingConsolCodeMapperProviderTest : TestCaseWithFactory
	{
		public void TestGetCodeMapperForBookingConfirmation() => AssertGetCodeMapper(BookingMessagesForTest.CreateBookingConfirmation());

		public void TestGetCodeMapperForBookingRequestReply() => AssertGetCodeMapper(BookingMessagesForTest.CreateBookingRequestReply());

		void AssertGetCodeMapper(UniversalShipment shipment)
		{
			var consol = BookingMessagesForTest.CreateConsolMatchingDataTarget(Factory, shipment);
			AssertNotNull("prerequisite; consol matching uxml DataTarget has been created", consol);
			Factory.Save();

			var provider = new ForwardingConsolCodeMapperProvider();

			AssertNotNull("created code mapper for message",
				provider.Create(shipment, Factory));

			var otherShipment = new UniversalShipment();
			AssertNull("do not create code mapper for other universal xml message",
				provider.Create(otherShipment, Factory));
		}

		public void TestRegisteredWithObjectFactory()
		{
			var providers = (Hashtable)ObjectFactory.Get("UniversalCodeMapperProviderList");
			var objectHandle = (ObjectHandle)providers[nameof(DataContextType.ForwardingConsol)];

			AssertNotNull("ForwardingConsolCodeMapperProvider is accessible via ObjectFactory", objectHandle);
		}

		public void TestGetCodeMapperForCO2eResponse()
		{
			var cO2eResponse = CO2eTestHelper.GetSampleCO2eResponseDataObject();
			var provider = new ForwardingConsolCodeMapperProvider();

			var mapper = provider.Create(cO2eResponse, Factory);
			AssertNotNull("created code mapper for message", mapper);
			AssertType<CO2eUniversalCodeMapper>(mapper);

			var otherShipment = new UniversalShipment();
			AssertNull("do not create code mapper for other universal xml message", provider.Create(otherShipment, Factory));
		}
	}
}
