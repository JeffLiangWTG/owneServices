using CargoWise.EntityFramework.Testing;
using UniversalShipment = Enterprise.UniversalDataBuss.DataObjects.Universal.Shipment;

namespace Enterprise.Freight.Forwarding.DataTransfer.Testing
{
	sealed class BookingConfirmationUniversalCodeMapperTest : TestCaseWithFactory
	{
		public void TestCreate_BookingConfirmation() => AssertCreate(BookingMessagesForTest.CreateBookingConfirmation());

		public void TestCreate_BookingRequestReply() => AssertCreate(BookingMessagesForTest.CreateBookingRequestReply());

		void AssertCreate(UniversalShipment shipment)
		{
			var consol = BookingMessagesForTest.CreateConsolMatchingDataTarget(Factory, shipment);

			var codeMapper = BookingConfirmationUniversalCodeMapper.Create(shipment, Factory);
			AssertNotNull("code mapper has been created", codeMapper);
			AssertEquals("code mapper organization matches", consol.ShippingLine, codeMapper.SourceOrganisation);
		}
	}
}
