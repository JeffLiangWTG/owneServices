using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.Freight.CarbonEmissions.Business;
using Enterprise.Freight.Common.Business;
using Enterprise.Freight.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Params = CargoWise.EventReference.Constants.EventReferenceParameters.Codes;
using UniversalEvent = Enterprise.UniversalDataBuss.DataObjects.Universal.Event;

namespace Enterprise.Freight.DataTransfer.Universal.Testing
{
	sealed class CO2eDataTransferHelperTest : TestCaseWithFactory
	{
		public void TestIsCO2eResponse()
		{
			UniversalEvent universalEvent = null;
			AssertEquals(expected: false, universalEvent.IsCO2eResponse());

			universalEvent = new UniversalEvent();
			universalEvent.EventParameters = null;
			AssertEquals(expected: false, universalEvent.IsCO2eResponse());

			universalEvent.EventParameters = new EventParameters();
			universalEvent.EventParameters.MessageType = null;
			AssertEquals(expected: false, universalEvent.IsCO2eResponse());

			universalEvent.EventParameters.MessageType = "TEST";
			AssertEquals(expected: false, universalEvent.IsCO2eResponse());

			universalEvent.EventParameters.MessageType = CO2eDataTransferHelper.CO2eCalculationProvider;
			AssertEquals(expected: true, universalEvent.IsCO2eResponse());
		}

		public void TestOnCO2eRejectionEvent()
		{
			var universalEvent = new UniversalEvent();
			universalEvent.EventType = Events.InterchangeRejectedCode;
			universalEvent.EventParameters = new EventParameters();
			universalEvent.EventParameters.MessageType = CO2eDataTransferHelper.CO2eCalculationProvider;
			universalEvent.ContextCollection = new List<Context>();
			universalEvent.ContextCollection.Add(new Context() { Type = "FailureReason", Value = "ERROR! ERROR! and more ERROR..." });

			var shipment = Factory.New<Enterprise.Integration.Forwarding.IForwardingShipment>() as CommonShipment;
			shipment.JS_UniqueConsignRef = "S00001016";

			var leg1 = shipment.TransportsIncludingRelated.AddNew();
			leg1.JW_RL_NKLoadPort = "AUMEL";
			leg1.JW_RL_NKDiscPort = "SGSIN";

			var leg2 = shipment.TransportsIncludingRelated.AddNew();
			leg2.JW_RL_NKLoadPort = "AUMEL";
			leg2.JW_RL_NKDiscPort = ZString.Empty;

			var leg3 = shipment.TransportsIncludingRelated.AddNew();
			leg3.JW_RL_NKLoadPort = ZString.Empty;
			leg3.JW_RL_NKDiscPort = "SGSIN";

			var leg4 = shipment.TransportsIncludingRelated.AddNew();
			leg4.JW_RL_NKLoadPort = ZString.Empty;
			leg4.JW_RL_NKDiscPort = ZString.Empty;

			Factory.Save();

			AssertEquals("Preconditon", CO2eStatusList.Codes.NotCalculated, ((ICO2eProvider)shipment).GetCO2eStatus());

			universalEvent.OnCO2eRejectionEvent((ICO2eCalculationSupporter)shipment);

			AssertEquals(CO2eStatusList.Codes.Rejected, ((ICO2eProvider)shipment).GetCO2eStatus());
			AssertEquals(CO2eStatusList.Codes.Rejected, leg1.GetCO2eStatus());
			AssertEquals(CO2eStatusList.Codes.Rejected, leg2.GetCO2eStatus());
			AssertEquals(CO2eStatusList.Codes.Rejected, leg3.GetCO2eStatus());
			AssertEquals(CO2eStatusList.Codes.Rejected, leg4.GetCO2eStatus());

			AssertLog(shipment, 1);
			AssertLog(leg1, 1);
			AssertLog(leg2, 0);
			AssertLog(leg3, 0);
			AssertLog(leg4, 0);
		}

		static void AssertLog(EnterpriseBusinessObject bizo, int expected)
		{
			const string failureReason = "ERROR! ERROR! and more ERROR...";
			AssertEquals(expected, bizo.Logs.Find(log =>
				log.SL_SE_NKEvent == AutoEvents.GreenhouseGasEmissionsCalculationCode &&
				log.Parameters.TryGetValue(Params.Type, out var type) && type == nameof(CO2eEventType.Rejected) &&
				log.Parameters.TryGetValue(Params.Reason, out var reason) && reason == failureReason).Count());
		}

		public void TestPrePostCarriageLegWrapperToTransportLegDO()
		{
			var address1 = Factory.NewWithValidTestData<OrgHeader>().MainAddress;
			address1.OA_GeoLocation = ZGeography.CreatePoint(10, 10);
			address1.OA_PostCode = "2000";
			address1.OA_City = "Sydney";
			address1.OA_RN_NKCountryCode = "AU";
			address1.OA_RL_NKRelatedPortCode = "AUSYD";
			address1.OA_ValidationStatus = AddressValidationStatus.Verified;

			var address2 = Factory.NewWithValidTestData<OrgHeader>().MainAddress;
			address2.OA_GeoLocation = ZGeography.CreatePoint(20, 20);
			address2.OA_PostCode = "3000";
			address2.OA_City = "Melbourne";
			address2.OA_RN_NKCountryCode = "AU";
			address2.OA_RL_NKRelatedPortCode = "AUMEL";
			address2.OA_ValidationStatus = AddressValidationStatus.Verified;

			var transportModeConverter = new TransportModeConverter();
			var leg = new PrePostCarriageLegWrapper(new PrePostCarriageLocationWrapper(address1), new PrePostCarriageLocationWrapper(address2));
			var transportLegDO = leg.ToTransportLegDO(DefaultDataObjectWriterStrategy.TestInstance, Factory);
			AssertEquals("TransportMode", transportModeConverter.ToEnumValue(Core.Constants.TransportModes.Road), transportLegDO.TransportMode);
			AssertOrganizationAddress("DepartureFrom", transportLegDO.DepartureFrom, ZGeography.CreatePoint(10, 10), "2000", "Sydney", "AU", "AUSYD");
			AssertOrganizationAddress("ArrivalAt", transportLegDO.ArrivalAt, ZGeography.CreatePoint(20, 20), "3000", "Melbourne", "AU", "AUMEL");

			leg = new PrePostCarriageLegWrapper(new PrePostCarriageLocationWrapper(address1), new PrePostCarriageLocationWrapper("AUBNE"));
			transportLegDO = leg.ToTransportLegDO(DefaultDataObjectWriterStrategy.TestInstance, Factory);
			AssertOrganizationAddress("ArrivalAt", transportLegDO.ArrivalAt, ZGeography.Empty, port: "AUBNE");
		}

		void AssertOrganizationAddress(string message, OrganizationAddress addressDO, ZGeography geoLocation, string postcode = null, string city = null, string countryCode = null, string port = null)
		{
			CombineAssertions(message, () =>
			{
				if (geoLocation.IsEmpty)
				{
					AssertNull("No location", addressDO.GeoLocation);
				}
				else
				{
					AssertEquals("Location latitude", geoLocation.Latitude, (double)addressDO.GeoLocation.Latitude);
					AssertEquals("Location longitude", geoLocation.Longitude, (double)addressDO.GeoLocation.Longitude);
				}
				AssertEquals("Postcode", postcode, addressDO.Postcode);
				AssertEquals("City", city, addressDO.City);
				AssertEquals("Country", countryCode, addressDO.Country?.Code);
				AssertEquals("Port", port, addressDO.Port?.Code);
			});
		}

		public void TestPrePostCarriageLegWrapperToTransportLegDO_TransportMode()
		{
			// Arrange
			var address1 = Factory.NewWithValidTestData<OrgHeader>().MainAddress;
			var address2 = Factory.NewWithValidTestData<OrgHeader>().MainAddress;
			var transportModeConverter = new TransportModeConverter();

			// Act & Assert
			var leg = new PrePostCarriageLegWrapper(new PrePostCarriageLocationWrapper(address1), new PrePostCarriageLocationWrapper(address2), Core.Constants.TransportModes.Rail);
			var transportLegDO = leg.ToTransportLegDO(DefaultDataObjectWriterStrategy.TestInstance, Factory);
			AssertEquals(transportModeConverter.ToEnumValue(Core.Constants.TransportModes.Rail), transportLegDO.TransportMode);

			leg = new PrePostCarriageLegWrapper(new PrePostCarriageLocationWrapper(address1), new PrePostCarriageLocationWrapper(address2));
			transportLegDO = leg.ToTransportLegDO(DefaultDataObjectWriterStrategy.TestInstance, Factory);
			AssertEquals(transportModeConverter.ToEnumValue(Core.Constants.TransportModes.Road), transportLegDO.TransportMode);
		}

		public void TestToUXmlOrganizationAddress_ExcludeNullIslandGeoLocation()
		{
			AssertGeoLocationNullInUXml(0, 0);
			AssertGeoLocationNullInUXml(0, 0.000000000000001);
			AssertGeoLocationNullInUXml(0.0000000000001, 0);
			AssertGeoLocationNullInUXml(180, 0);
			AssertGeoLocationNullInUXml(-180, -0);
			AssertGeoLocationNullInUXml(0, 360);
			AssertGeoLocationNullInUXml(0, -360);

			void AssertGeoLocationNullInUXml(double latitude, double longitude)
			{
				// Arrange
				var address = Factory.New<OrgAddress>();
				address.OA_GeoLocation = ZGeography.CreatePoint(longitude, latitude);
				var location = new PrePostCarriageLocationWrapper(address);

				// Act
				var xmlOrgAddress = location.ToUXmlOrganizationAddress(DefaultDataObjectWriterStrategy.TestInstance, Factory);

				// Assert
				AssertEquals(null, xmlOrgAddress.GeoLocation);
			}
		}

		public void TestToUXmlOrganizationAddress_IncludeValidGeoLocation()
		{
			AssertGeoLocationIncludedInUXml(0, 0.000000000001);
			AssertGeoLocationIncludedInUXml(0.000000000001, 0);
			AssertGeoLocationIncludedInUXml(90, 180);
			AssertGeoLocationIncludedInUXml(-90, -180);
			AssertGeoLocationIncludedInUXml(90, 0);
			AssertGeoLocationIncludedInUXml(-90, -0);
			AssertGeoLocationIncludedInUXml(0, 180);
			AssertGeoLocationIncludedInUXml(0, -180);

			void AssertGeoLocationIncludedInUXml(double latitude, double longitude)
			{
				// Arrange
				var address = Factory.New<OrgAddress>();
				address.OA_GeoLocation = ZGeography.CreatePoint(longitude, latitude);
				address.OA_ValidationStatus = AddressValidationStatus.Verified;
				var location = new PrePostCarriageLocationWrapper(address);

				// Act
				var xmlOrgAddress = location.ToUXmlOrganizationAddress(DefaultDataObjectWriterStrategy.TestInstance, Factory);

				// Assert
				AssertEquals((ZDecimal)latitude, xmlOrgAddress.GeoLocation.Latitude);
				AssertEquals((ZDecimal)longitude, xmlOrgAddress.GeoLocation.Longitude);
			}
		}

		public void TestToUXmlOrganizationAddress_ExcludeGeoLocation_WhenStatusInvalid()
		{
			AssertGeoLocationExcluded(AddressValidationStatus.CountryNotAvailable);
			AssertGeoLocationExcluded(AddressValidationStatus.Invalid);
			AssertGeoLocationExcluded(AddressValidationStatus.ToBeVerified);

			void AssertGeoLocationExcluded(string status)
			{
				// Arrange
				var address = Factory.New<OrgAddress>();
				address.OA_GeoLocation = ZGeography.CreatePoint(10, 10);
				address.OA_ValidationStatus = status;
				var location = new PrePostCarriageLocationWrapper(address);

				// Act
				var xmlOrgAddress = location.ToUXmlOrganizationAddress(DefaultDataObjectWriterStrategy.TestInstance, Factory);

				// Assert
				AssertNull(xmlOrgAddress.GeoLocation);
			}
		}
	}
}
