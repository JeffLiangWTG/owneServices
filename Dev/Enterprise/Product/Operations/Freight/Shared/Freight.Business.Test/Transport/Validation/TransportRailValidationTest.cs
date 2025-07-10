using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Freight.Business.Testing
{
	sealed class TransportRailValidationTest : BusinessObjectValidationTestCase
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
			AssertNoNotifications("No error expected on Arrival. Departure > Arrival.", arrivalInfo);

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
			Transport.JW_IsLinked = true;
			Transport.JW_VoyageFlight = "";
			Transport.Validation.ValidateJW_VoyageFlight();
			AssertHasErrors("Blank Journey - Error expected", Transport.JW_VoyageFlightInfo);
		}

		public void TestUsingCorrectValidation()
		{
			AssertEquals(typeof(TransportRailValidation), TransportValidation.New(Transport).GetType());
			AssertEquals(typeof(TransportRailValidation), Transport.Validation.GetType());
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
					transport.JW_TransportMode = Core.Constants.TransportModes.Rail;
				}
				return transport;
			}
		}
		Transport transport;

		#endregion
	}
}
