using System.Linq;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.DataTransfer.Universal.Testing;
using Enterprise.UniversalDataBuss.Core.Testing;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using NUnit.Framework;

namespace Enterprise.Freight.DataTransfer.Universal.Testing
{
	[TestedType(typeof(TransportLegCollectionReader<>))]
	sealed class TransportLegCollectionReaderTest : DataObjectCollectionReaderTest
	{
		public override void TestReadIntoCollection()
		{
			var shipment = Factory.New<CommonShipment>();

			var transportLeg1 = shipment.Transports.AddNew();
			transportLeg1.JW_RL_NKLoadPort = "AUSYD";
			transportLeg1.JW_RL_NKDiscPort = "NZAKL";
			transportLeg1.JW_VoyageFlight = "001";

			var transportLeg2 = shipment.Transports.AddNew();
			transportLeg2.JW_RL_NKLoadPort = "NZCHC";

			var transportLegDataObject1 = CreateDataObject("AUSYD", "NZAKL", "999");
			var transportLegDataObject2 = CreateDataObject("NZAKL", "USSFO", "002");

			var logger = new TestErrorLogger();
			var list = new DataObjectList<TransportLeg> { transportLegDataObject1, transportLegDataObject2 };
			var reader = new TransportLegCollectionReader<Transport>(list, logger, new UniversalObjectFactory(), shipment);

			reader.ReadIntoCollection();

			AssertMultilineASCIIEquals("logs", @"
Information - Successfully loaded matching Transport.
Information - Populating Transport...
Information - Transport Leg: Origin: AUSYD Destination: NZAKL
Information - Attempting to get Schedule for the Transport Leg
Information - Unable to link to schedule because the Transport Leg has insufficient information.
Information - Transport Leg updated.
Information - No matching Transport found, creating new Transport.
Information - Populating Transport...
Information - Transport Leg: Origin: NZAKL Destination: USSFO
Information - Attempting to get Schedule for the Transport Leg
Information - Unable to link to schedule because the Transport Leg has insufficient information.
Information - Transport Leg updated.
".Trim(), logger.Logs);

			AssertContainsExactElementsInAnyOrder(new[]
			{
				"AUSYD|NZAKL|999", "NZAKL|USSFO|002"
			},
			FormatTransports(shipment));
		}

		public void TestReadIntoCollection_Partial()
		{
			var shipment = Factory.New<CommonShipment>();

			var transportLeg1 = shipment.Transports.AddNew();
			transportLeg1.JW_RL_NKLoadPort = "AUSYD";
			transportLeg1.JW_RL_NKDiscPort = "NZAKL";
			transportLeg1.JW_VoyageFlight = "001";

			var transportLeg2 = shipment.Transports.AddNew();
			transportLeg2.JW_RL_NKLoadPort = "NZAKL";
			transportLeg2.JW_RL_NKDiscPort = "NZCHC";
			transportLeg2.JW_VoyageFlight = "002";

			var transportLegDataObject1 = CreateDataObject("AUSYD", "NZAKL", "101");
			var transportLegDataObject2 = CreateDataObject("NZCHC", "USSFO", "103");

			var logger = new TestErrorLogger();
			var list = new DataObjectList<TransportLeg> { transportLegDataObject1, transportLegDataObject2 };
			list.Content = CollectionContent.Partial;

			var reader = new TransportLegCollectionReader<Transport>(list, logger, new UniversalObjectFactory(), shipment);
			reader.ReadIntoCollection();

			AssertMultilineASCIIEquals("logs", @"
Information - Successfully loaded matching Transport.
Information - Populating Transport...
Information - Transport Leg: Origin: AUSYD Destination: NZAKL
Information - Attempting to get Schedule for the Transport Leg
Information - Unable to link to schedule because the Transport Leg has insufficient information.
Information - Transport Leg updated.
Information - No matching Transport found, creating new Transport.
Information - Populating Transport...
Information - Transport Leg: Origin: NZCHC Destination: USSFO
Information - Attempting to get Schedule for the Transport Leg
Information - Unable to link to schedule because the Transport Leg has insufficient information.
Information - Transport Leg updated.
".Trim(), logger.Logs);

			AssertContainsExactElementsInAnyOrder(new[]
			{
				"AUSYD|NZAKL|101", "NZAKL|NZCHC|002", "NZCHC|USSFO|103"
			},
			FormatTransports(shipment));
		}

		#region Implementation

		string[] FormatTransports(CommonShipment shipment)
		{
			return shipment.Transports
				.Cast<Transport>()
				.Select(t => string.Format("{0}|{1}|{2}", t.JW_RL_NKLoadPort, t.JW_RL_NKDiscPort, t.JW_VoyageFlight))
				.ToArray();
		}

		TransportLeg CreateDataObject(string loadPort, string dischargePort, string voyageNo)
		{
			var dataObject = new TransportLeg(DefaultDataObjectWriterStrategy.TestInstance);
			dataObject.PortOfLoading = new UNLOCO { Code = loadPort };
			dataObject.PortOfDischarge = new UNLOCO { Code = dischargePort };
			dataObject.VoyageFlightNo = voyageNo;
			return dataObject;
		}

		#endregion
	}
}
