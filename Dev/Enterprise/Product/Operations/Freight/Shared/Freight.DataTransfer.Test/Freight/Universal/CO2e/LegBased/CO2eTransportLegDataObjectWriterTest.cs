using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.Freight.DataTransfer.Universal.Testing
{
	sealed class CO2eTransportLegDataObjectWriterTest : TransportLegDataObjectWriterTest
	{
		public void TestPopulateBusinessObject()
		{
			var transport = GetTransport();
			SetupTransportLeg(transport);
			var cusCode = transport.Carrier.CustomsCodes.AddNew();
			cusCode.OK_CodeType = OrgCusCode.CodeTypes.CarrierCode;
			cusCode.OK_RN_NKCodeCountry = "US";
			cusCode.OK_CustomsRegNo = "1111";

			var writer = new CO2eTransportLegDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, transport)));
			var transportDataObject = writer.GetDataObject(transport);
			CombineAssertions(() =>
			{
				AssertEquals("transportDataObject.LegOrder.Value", (ZByte)1, transportDataObject.LegOrder);
				AssertEquals("transportDataObject.IsCargoOnly", true, transportDataObject.IsCargoOnly);
				AssertEquals("transportDataObject.TransportMode", new TransportModeConverter().ToEnumValue(Core.Constants.TransportModes.Sea), transportDataObject.TransportMode);
				AssertEquals("transportDataObject.LegType", new LegTypeConverter().ToEnumValue(Core.Constants.TransportPlanningType.MainVessel), transportDataObject.LegType);
				AssertEquals("transportDataObject.VesselName", "ANRO ASIA ZZ", transportDataObject.VesselName);
				AssertEquals("transportDataObject.VoyageFlightNo", "121A", transportDataObject.VoyageFlightNo);
				AssertEquals("transportDataObject.AircraftType.Code", "E90", transportDataObject.AircraftType.Code);
				AssertNull("transportDataObject.AircraftType.Description", transportDataObject.AircraftType.Description);
				AssertEquals("transportDataObject.VesselLloydsNumber", "9174622", transportDataObject.VesselLloydsIMO);
				AssertEquals("transportDataObject.PortOfLoading.Code", "NZAKL", transportDataObject.PortOfLoading.Code);
				AssertEquals("transportDataObject.EstimatedDeparture", new ZDateTime(2012, 8, 14), transportDataObject.EstimatedDeparture);
				AssertEquals("transportDataObject.PortOfDischarge.Code", "AUSYD", transportDataObject.PortOfDischarge.Code);
				AssertEquals("transportDataObject.EstimatedArrival", new ZDateTime(2012, 8, 18), transportDataObject.EstimatedArrival);
				AssertEquals("transportDataObject.Carrier.RegistrationNumber", "1111", transportDataObject.Carrier.RegistrationNumberCollection[0].Value);
			});
		}
	}
}
