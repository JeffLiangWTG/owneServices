using System.Linq;
using CargoWise.Integration;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.DataTransfer.Universal.Testing;
using Enterprise.UniversalDataBuss.Core.Testing;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using NUnit.Framework;

namespace Enterprise.Freight.DataTransfer.Universal.Testing
{
	[TestedType(typeof(ScheduleTransportLegCollectionReader<>))]
	sealed class ScheduleTransportLegCollectionReaderTest : DataObjectCollectionReaderTest
	{
		public override void TestReadIntoCollection()
		{
			var voyage = Factory.New<JobVoyage>();
			voyage.JV_VoyageFlight = "XYZ";

			var sailing1 = voyage.Sailings.AddNew();
			var origin1 = Factory.New<VoyageOrigin>();
			origin1.JA_JV = voyage.PK;
			origin1.JA_RL_NKPortOfLoading = "AUSYD";
			sailing1.JX_JA = origin1.PK;

			var destination1 = Factory.New<VoyageDestination>();
			destination1.JB_JV = voyage.PK;
			destination1.JB_RL_NKPortOfDischarge = "NZAKL";
			sailing1.JX_JB = destination1.PK;

			var sailing2 = voyage.Sailings.AddNew();
			var origin2 = Factory.New<VoyageOrigin>();
			origin2.JA_JV = voyage.PK;
			origin2.JA_RL_NKPortOfLoading = "NZCHC";
			sailing2.JX_JA = origin2.PK;

			var destination2 = Factory.New<VoyageDestination>();
			destination2.JB_JV = voyage.PK;
			destination2.JB_RL_NKPortOfDischarge = "FRCAL";
			sailing2.JX_JB = destination2.PK;

			Factory.SaveForTesting();

			var transportLegDataObject1 = CreateDataObject("AUSYD", "NZAKL", "001");
			var transportLegDataObject2 = CreateDataObject("NZAKL", "USSFO", "001");

			var logger = new TestErrorLogger();
			var collection = new DataObjectList<TransportLeg>(new[] { transportLegDataObject1, transportLegDataObject2 });
			var reader = new ScheduleTransportLegCollectionReader<JobSailing>(collection, logger, new UniversalObjectFactory(), voyage);

			reader.ReadIntoCollection();

			AssertMultilineASCIIEquals("logs", @"
Information - Successfully loaded matching JobSailing.
Information - Populating JobSailing...
Information - No matching JobSailing found, creating new JobSailing.
Information - Populating JobSailing...
".Trim(), logger.Logs);

			AssertContainsExactElementsInAnyOrder("AUSYD->NZAKL updated. NZAKL->USSFO added. NZCHC->FRCAL removed.", new[]
				{
					"AUSYD|NZAKL|001", "NZAKL|USSFO|001"
				},
				FormatTransports(voyage));
		}

		public void TestTransportLegCollectionWithCompleteAttribute()
		{
			var voyage = Factory.New<JobVoyage>();
			voyage.JV_AirSeaRoad = "SEA";
			voyage.JV_VoyageFlight = "123";

			var origin1 = Factory.New<VoyageOrigin>();
			origin1.JA_RL_NKPortOfLoading = "SGSIN";

			var origin2 = Factory.New<VoyageOrigin>();
			origin2.JA_RL_NKPortOfLoading = "HKHKG";

			var destination1 = Factory.New<VoyageDestination>();
			destination1.JB_RL_NKPortOfDischarge = "AUSYD";

			var destination2 = Factory.New<VoyageDestination>();
			destination2.JB_RL_NKPortOfDischarge = "AUMEL";

			voyage.Origins.Add(origin1);
			voyage.Origins.Add(origin2);
			voyage.Destinations.Add(destination1);
			voyage.Destinations.Add(destination2);
			voyage.GenerateSailings();

			AssertEquals("Precondition", 4, voyage.Sailings.Count);

			Factory.SaveForTesting();

			var dataObject = new Shipment(DefaultDataObjectWriterStrategy.TestInstance);
			dataObject.TransportMode = ListHelper.GetWithDescription<CodeDescriptionPair>("SEA", (ICodeDescriptionPairList)null);
			dataObject.VoyageFlightNo = "123";
			dataObject.SetTransportLegCollection(() => new DataObjectList<TransportLeg>());
			dataObject.TransportLegCollection.Content = CollectionContent.Complete;

			var transportLegDataObject1 = CreateDataObject("SGSIN", "AUMEL", "123");
			dataObject.TransportLegCollection.Add(transportLegDataObject1);

			var logger = new TestErrorLogger();
			var reader = new JobVoyageShipmentDataObjectReader(dataObject, logger, Factory);
			voyage = reader.ReadIntoBusinessObject();

			AssertMultilineASCIIEquals("logs", @"
Information - Successfully loaded matching JobVoyage.
Information - Populating JobVoyage...
Information - Successfully loaded matching JobSailing.
Information - Populating JobSailing...
Information - Updated Sailing Schedule (Vessel='', Voyage='123', Carrier='') from UniversalShipment.
".Trim(), logger.Logs);

			AssertContainsExactElementsInAnyOrder("SGSIN->AUMEL updated. Others removed.", new[]
				{
					"SGSIN|AUMEL|123"
				},
				FormatTransports(voyage));
		}

		public void TestTransportLegCollectionWithPartialAttribute()
		{
			var voyage = Factory.New<JobVoyage>();
			voyage.JV_VoyageFlight = "000";

			var sailing1 = voyage.Sailings.AddNew();
			var origin1 = Factory.New<VoyageOrigin>();
			origin1.JA_JV = voyage.PK;
			origin1.JA_RL_NKPortOfLoading = "AUSYD";
			origin1.JA_E_DEP = new CargoWise.Types.ZDateTime(2017, 11, 23, 14, 48, 0);
			sailing1.JX_JA = origin1.PK;

			var destination1 = Factory.New<VoyageDestination>();
			destination1.JB_JV = voyage.PK;
			destination1.JB_RL_NKPortOfDischarge = "SGSIN";
			sailing1.JX_JB = destination1.PK;

			var sailing2 = voyage.Sailings.AddNew();
			var origin2 = Factory.New<VoyageOrigin>();
			origin2.JA_JV = voyage.PK;
			origin2.JA_RL_NKPortOfLoading = "SGSIN";
			sailing2.JX_JA = origin2.PK;

			var destination2 = Factory.New<VoyageDestination>();
			destination2.JB_JV = voyage.PK;
			destination2.JB_RL_NKPortOfDischarge = "DEFRA";
			sailing2.JX_JB = destination2.PK;

			var transportLegDataObject1 = CreateDataObject("AUSYD", "SGSIN", "QF9");

			var logger = new TestErrorLogger();
			var collection = new DataObjectList<TransportLeg>(new[] { transportLegDataObject1 });
			collection.Content = CollectionContent.Partial;

			var shipment = new Shipment(DefaultDataObjectWriterStrategy.TestInstance);
			shipment.SetTransportLegCollection(() => collection);

			var reader = new ScheduleTransportLegCollectionReader<JobSailing>(collection, logger, new UniversalObjectFactory(), voyage);

			reader.ReadIntoCollection();

			AssertMultilineASCIIEquals("logs", @"
Information - Successfully loaded matching JobSailing.
Information - Populating JobSailing...
".Trim(), logger.Logs);

			AssertContainsExactElementsInAnyOrder(new[]
				{
					"AUSYD|SGSIN|QF9", "SGSIN|DEFRA|QF9"
				},
				FormatTransports(voyage));
		}

		#region Implementation

		string[] FormatTransports(JobVoyage voyage)
		{
			return voyage.Sailings
				.Cast<JobSailing>()
				.Select(s => string.Format("{0}|{1}|{2}", s.JX_JA_RL_NKPortOfLoading, s.JX_JB_RL_NKPortOfDischarge, voyage.JV_VoyageFlight))
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
