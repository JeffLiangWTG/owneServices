using CargoWise.EntityFramework.Testing;
using Enterprise.UniversalDataBuss.DataObjects.Universal._2012_11;

namespace Enterprise.Freight.Forwarding.DataTransfer.Testing
{
	sealed class BookingRequestHelperTest : TestCaseWithFactory
	{
		#region TestIsBookingRequestMessage

		public void TestIsBookingRequestMessage_Match()
		{
			var universalShipment = new UniversalDataBuss.DataObjects.Universal.Shipment
			{
				DataContext = new DataContext
				{
					DocumentaryOverride = new UniversalDataBuss.DataObjects.Universal.DocumentaryOverride
					{
						DocumentName = BookingRequestHelper.BookingRequestDataDocumentName
					}
				}
			};

			Assert("Is applicable", universalShipment.IsBookingRequestMessage());
		}

		public void TestIsBookingRequestMessage_NoMatch()
		{
			var universalShipment = new UniversalDataBuss.DataObjects.Universal.Shipment();

			Assert("Is not applicable", !universalShipment.IsBookingRequestMessage());
		}

		#endregion
	}
}
