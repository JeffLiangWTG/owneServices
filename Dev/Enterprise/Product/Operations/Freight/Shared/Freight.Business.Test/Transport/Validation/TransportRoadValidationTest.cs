using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Core;

namespace Enterprise.Freight.Business.Testing
{
	sealed class TransportRoadValidationTest : BusinessObjectValidationTestCase
	{
		public void TestValidateEstimatedDepartureAndArrival()
		{
			GenericValidateDateRangeForEstimatedDate(Transport.JW_ETDInfo, Transport.JW_ETAInfo);
		}

		void GenericValidateDateRangeForEstimatedDate(ZPropertyInfo departureInfo, ZPropertyInfo arrivalInfo)
		{
			var date = ZDateTime.Now;

			departureInfo.Value = date;
			arrivalInfo.Value = ZDateTime.Empty;
			AssertNoNotifications("Arrival Date empty. No error expected on Departure", departureInfo);
			AssertHasWarning(arrivalInfo, "You have not entered an " + arrivalInfo.Description + ".");

			arrivalInfo.Value = new ZDateTime(date.AddDays(-1));
			ValidateInfo(departureInfo);
			AssertHasErrors("Error expected on Departure. Departure > Arrival.", departureInfo);
			AssertHasErrors("Error expected on Arrival. Departure > Arrival.", arrivalInfo);

			arrivalInfo.Value = new ZDateTime(date.AddDays(2));
			ValidateInfo(departureInfo);
			AssertNoNotifications("No error expected on Departure. Departure < Arrival.", departureInfo);
			AssertNoNotifications("No error expected on Arrival. Departure < Arrival.", arrivalInfo);
		}

		public void TestValidateActualDepartureAndArrival()
		{
			GenericValidateDateRangeForActualDate(Transport.JW_ATDInfo, Transport.JW_ATAInfo);
		}

		void GenericValidateDateRangeForActualDate(ZPropertyInfo departureInfo, ZPropertyInfo arrivalInfo)
		{
			ZDateTime date = ZDateTime.Now;

			departureInfo.Value = date;
			arrivalInfo.Value = ZDateTime.Empty;
			AssertNoNotifications("Arrival Date empty. No error expected on Departure", departureInfo);
			AssertNoNotifications("Arrival Date empty. No error expected on Arrival", arrivalInfo);

			arrivalInfo.Value = new ZDateTime(date.AddHours(-1));
			ValidateInfo(departureInfo);
			AssertHasErrors("Error expected on Departure. Departure > Arrival.", departureInfo);
			AssertHasErrors("Error expected on Arrival. Departure > Arrival.", arrivalInfo);

			arrivalInfo.Value = new ZDateTime(date.AddDays(2));
			ValidateInfo(departureInfo);
			AssertNoNotifications("No error expected on Departure. Departure < Arrival.", departureInfo);
			AssertNoNotifications("No error expected on Arrival. Departure < Arrival.", arrivalInfo);
		}

		public void TestValidateJW_VoyageFlight()
		{
			string warning = "It is recommended that you enter a specific truck reference.";

			CombineAssertions("When Support ETD", () =>
			{
				Transport.JW_IsLinked = true;
				Transport.JW_VoyageFlight = "";
				Transport.Validation.ValidateJW_VoyageFlight();
				AssertHasWarning("Blank Truck Registration - Error expected", Transport.JW_VoyageFlightInfo, warning);

				Transport.JW_VoyageFlight = "blat";
				AssertNoWarning("Non-Blank truck registration - no warning expected", Transport.JW_VoyageFlightInfo, warning);
			});

			CombineAssertions("When not Support ETD", () =>
			{
				var parent = Factory.New<DummyTransportParent>();
				var transport = parent.Transports.AddNew();
				transport.JW_IsLinked = true;
				transport.JW_VoyageFlight = "";
				transport.Validation.ValidateJW_VoyageFlight();
				AssertNoWarning("Blank Truck Registration - Error expected", transport.JW_VoyageFlightInfo, warning);
			});
		}

		public void TestUsingCorrectValidation()
		{
			AssertEquals(typeof(TransportRoadValidation), TransportValidation.New(Transport).GetType());
			AssertEquals(typeof(TransportRoadValidation), Transport.Validation.GetType());
		}

		#region Implementation

		void ValidateInfo(ZPropertyInfo info)
		{
			((IBusinessObjectInternals)info.BizObj).Validate(info);
		}

		Transport Transport
		{
			get
			{
				if (transport == null)
				{
					transport = Factory.New<CommonShipment>().Transports.AddNew();
					transport.JW_TransportMode = Constants.TransportModes.Road;
				}
				return transport;
			}
		}
		Transport transport;

		#endregion
	}
}
