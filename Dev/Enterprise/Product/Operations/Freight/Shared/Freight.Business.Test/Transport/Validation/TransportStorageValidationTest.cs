using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Freight.Business.Testing
{
	sealed class TransportStorageValidationTest : BusinessObjectValidationTestCase
	{
		public void TestValidateEstimatedDepartureAndArrival()
		{
			GenericValidateDateRangeForEstimatedDate(Transport.JW_ETDInfo, Transport.JW_ETAInfo);
		}

		public void TestValidateActualDepartureAndArrival()
		{
			GenericValidateDateRangeForActualDate(Transport.JW_ATDInfo, Transport.JW_ATAInfo);
		}

		public void TestJW_TransportType()
		{
			Transport.JW_TransportType = "";
			Transport.Validation.ValidateJW_TransportType();
			AssertNoNotifications("The storage transport mode does not use transport type", Transport.JW_TransportTypeInfo);
		}

		public void TestJW_Status()
		{
			Transport.JW_Status = "";
			Transport.Validation.ValidateJW_Status();
			AssertNoNotifications("The storage transport mode does not use status", Transport.JW_StatusInfo);
		}

		public void TestUsingCorrectValidation()
		{
			AssertEquals(typeof(TransportStorageValidation), TransportValidation.New(Transport).GetType());
			AssertEquals(typeof(TransportStorageValidation), Transport.Validation.GetType());
		}

		#region Implementation

		void GenericValidateDateRangeForEstimatedDate(ZPropertyInfo departureInfo, ZPropertyInfo arrivalInfo)
		{
			ZDateTime date = ZDateTime.Now;

			departureInfo.Value = date;
			arrivalInfo.Value = ZDateTime.Empty;
			AssertNoNotifications("Arrival Date empty. No error expected on Departure", departureInfo);
			AssertHasWarning(arrivalInfo, "You have not entered an " + arrivalInfo.Description + ".");

			arrivalInfo.Value = new ZDateTime(date.AddHours(1));
			ValidateInfo(departureInfo);
			AssertHasErrors("Error expected on Departure. Departure < Arrival.", departureInfo);
			AssertHasErrors("Error expected on Arrival. Departure < Arrival.", arrivalInfo);

			arrivalInfo.Value = new ZDateTime(date.AddHours(-1));
			ValidateInfo(departureInfo);
			AssertNoNotifications("No error expected on Departure. Departure > Arrival.", departureInfo);
			AssertNoNotifications("No error expected on Arrival. Departure > Arrival.", arrivalInfo);
		}

		void GenericValidateDateRangeForActualDate(ZPropertyInfo departureInfo, ZPropertyInfo arrivalInfo)
		{
			ZDateTime date = ZDateTime.Now;

			departureInfo.Value = date;
			arrivalInfo.Value = ZDateTime.Empty;
			AssertNoNotifications("Arrival Date empty. No error expected on Departure", departureInfo);
			AssertNoNotifications("Arrival Date empty. No error expected on Arrival", arrivalInfo);

			arrivalInfo.Value = new ZDateTime(date.AddHours(1));
			ValidateInfo(departureInfo);
			AssertHasErrors("Error expected on Departure. Departure < Arrival.", departureInfo);
			AssertHasErrors("Error expected on Arrival. Departure < Arrival.", arrivalInfo);

			arrivalInfo.Value = new ZDateTime(date.AddHours(-1));
			ValidateInfo(departureInfo);
			AssertNoNotifications("No error expected on Departure. Departure > Arrival.", departureInfo);
			AssertNoNotifications("No error expected on Arrival. Departure > Arrival.", arrivalInfo);
		}

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
					transport.JW_TransportMode = Core.Constants.TransportModes.Storage;
				}
				return transport;
			}
		}
		Transport transport;

		#endregion
	}
}
