using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.UniversalDataBuss.DataObjects.Universal._2012_11;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Forwarding.DataTransfer.Testing
{
	sealed class AirBookingHelperTest : TestCaseWithFactory
	{
		#region TestIsAirEBookingMessage

		public void TestIsAirEBookingMessage_AirBooking()
		{
			var uxmlEvent = new UniversalDataBuss.DataObjects.Universal.Event
			{
				DataContext = new DataContext
				{
					DocumentaryOverride = new UniversalDataBuss.DataObjects.Universal.DocumentaryOverride
					{
						DocumentName = AirBookingHelper.AirBookingDataStoreName
					}
				}
			};

			Assert("Is applicable", uxmlEvent.IsAirEBookingMessage());
		}

		public void TestIsAirEBookingMessage_NoDocumentaryOverride()
		{
			var uxmlEvent = new UniversalDataBuss.DataObjects.Universal.Event();

			Assert("Is not applicable", !uxmlEvent.IsAirEBookingMessage());
		}

		#endregion

		#region TestAirBookingDataStoreName

		public void TestAirBookingDataStoreName()
		{
			var pivotQuery = new ZQuery(StmMenuTemplatePivotSchema.SI_DataStoreName, AirBookingHelper.AirBookingDataStoreName);
			var pivot = Factory.LoadTop1<StmMenuTemplatePivot>(pivotQuery);

			AssertNotNull("Pivot for Air Booking Exists", pivot);
		}

		#endregion
	}
}
