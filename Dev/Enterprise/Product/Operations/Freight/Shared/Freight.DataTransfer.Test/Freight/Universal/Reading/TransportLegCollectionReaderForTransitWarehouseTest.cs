using System.Linq;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.DataTransfer.Universal.Testing;
using Enterprise.UniversalDataBuss.Core.Testing;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using NUnit.Framework;

namespace Enterprise.Freight.DataTransfer.Universal.Testing
{
	[TestedType(typeof(TransportLegCollectionReaderForTransitWarehouse))]
	sealed class TransportLegCollectionReaderForTransitWarehouseTest : DataObjectCollectionReaderTest
	{
		public override void TestReadIntoCollection()
		{
			var shipment = Factory.New<CommonShipment>();

			var transportLeg1 = shipment.Transports.AddNew();
			transportLeg1.JW_RL_NKLoadPort = "LKCMB";
			transportLeg1.JW_RL_NKDiscPort = "AUSYD";
			transportLeg1.JW_VoyageFlight = "001";

			var transportLeg2 = shipment.Transports.AddNew();
			transportLeg2.JW_RL_NKLoadPort = "AUSYD";
			transportLeg2.JW_RL_NKDiscPort = "NZAKL";
			transportLeg2.JW_VoyageFlight = "002";

			var transportLeg3 = shipment.Transports.AddNew();
			transportLeg3.JW_RL_NKLoadPort = "NZAKL";
			transportLeg3.JW_RL_NKDiscPort = "USSFO";
			transportLeg3.JW_VoyageFlight = "003";

			var transportLegDataObject1 = CreateDataObject("LKCMB", "AUSYD", "001");
			var transportLegDataObject2 = CreateDataObject("AUSYD", "NZAKL", "002");
			var transportLegDataObject3 = CreateDataObject("NZAKL", "USSFO", "003");

			var logger = new TestErrorLogger();
			var list = new DataObjectList<TransportLeg> { transportLegDataObject1, transportLegDataObject2, transportLegDataObject3 };
			var reader = new TransportLegCollectionReaderForTransitWarehouse(list, logger, new UniversalObjectFactory(),
				shipment, t => !(t.PortOfLoading.Code.Value == "NZAKL" || t.PortOfDischarge.Code.Value == "NZAKL"));

			reader.ReadIntoCollection();

			AssertMultilineASCIIEquals("logs", @"
Information - Successfully loaded matching Transport.
Information - Populating Transport...
Information - Transport Leg: Origin: AUSYD Destination: NZAKL
Information - Attempting to get Schedule for the Transport Leg
Information - Unable to link to schedule because the Transport Leg has insufficient information.
Information - Transport Leg updated.
Information - Successfully loaded matching Transport.
Information - Populating Transport...
Information - Transport Leg: Origin: NZAKL Destination: USSFO
Information - Attempting to get Schedule for the Transport Leg
Information - Unable to link to schedule because the Transport Leg has insufficient information.
Information - Transport Leg updated.
".Trim(), logger.Logs);

			AssertContainsExactElementsInAnyOrder(new[]
			{
				"AUSYD|NZAKL|002", "NZAKL|USSFO|003"
			},
			FormatTransports(shipment));
		}

		TransportLeg CreateDataObject(string loadPort, string dischargePort, string voyageNo)
		{
			var dataObject = new TransportLeg
			{
				PortOfLoading = new UNLOCO { Code = loadPort },
				PortOfDischarge = new UNLOCO { Code = dischargePort },
				VoyageFlightNo = voyageNo
			};
			return dataObject;
		}

		string[] FormatTransports(CommonShipment shipment)
		{
			return shipment.Transports
				.Cast<Transport>()
				.Select(t => string.Format("{0}|{1}|{2}", t.JW_RL_NKLoadPort, t.JW_RL_NKDiscPort, t.JW_VoyageFlight))
				.ToArray();
		}
	}
}
