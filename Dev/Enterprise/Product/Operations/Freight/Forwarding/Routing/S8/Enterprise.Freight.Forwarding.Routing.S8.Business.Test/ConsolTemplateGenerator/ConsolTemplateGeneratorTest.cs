namespace Enterprise.Freight.Forwarding.Routing.S8.Business.Test
{
	using System.Collections.Generic;
	using System.Linq;
	using CargoWise.Data;
	using CargoWise.EntityFramework.Testing;
	using CargoWise.Types;
	using Enterprise.Freight.Business;
	using Enterprise.Freight.Forwarding.Business;
	using Enterprise.MasterFiles.Business;
	using Enterprise.MasterFiles.Integration;
	using Enterprise.ZArchitecture.Modules;

	class ConsolTemplateGeneratorTest : TestCaseWithFactory
	{
		#region TestGenerateConsols

		public void TestGenerateConsols_WithoutName()
		{
			TestGenerateConsols(string.Empty, string.Empty);
		}

		public void TestGenerateConsols_WithName()
		{
			TestGenerateConsols("A", "B");
		}

		void TestGenerateConsols(string templateName1, string templateName2)
		{
			var templateRecord = GetTemplateRecord(templateName1);
			GetTemplateRecord(templateName2);

			var multiDaysSelection = MultiDaysSelectionForTest;
			var generator = new ConsolTemplateGenerator(multiDaysSelection)
			{
				AllocateNeutralMaster = true
			};

			var template = generator.ConsolTemplates.AddNew();
			template.ConsolTemplateReferenceId = templateRecord.STR_ReferenceId;
			template.ConsolsPerFlight = 2;

			var requestedDate = new ZDateTime(2018, 11, 20);
			var departureDates = new List<ZDateTime>()
			{
				requestedDate,
				requestedDate.AddDays(1)
			};

			AssertEquals(0, multiDaysSelection.CreatedConsols.Count);

			var header = CreateHeader();
			var sailingList = RoutingResponseHeader.TryCreateEnterpriseVoyagesSailings(header, departureDates, header.Factory);
			AssertEquals(2, sailingList.Count);

			generator.GenerateConsols(sailingList.Cast<JobSailingCollection>().ToList());

			var consolsExpected = template.ConsolsPerFlight * departureDates.Count;
			AssertEquals(consolsExpected, multiDaysSelection.CreatedConsols.Count);

			var createdConsol1 = multiDaysSelection.CreatedConsols[0];
			var createdConsol2 = multiDaysSelection.CreatedConsols[1];
			AssertConsolData(createdConsol1, (JobSailingCollection)sailingList[0], 0);
			AssertConsolData(createdConsol2, (JobSailingCollection)sailingList[0], 0);

			var createdConsol3 = multiDaysSelection.CreatedConsols[2];
			var createdConsol4 = multiDaysSelection.CreatedConsols[3];
			AssertConsolData(createdConsol3, (JobSailingCollection)sailingList[1], 1);
			AssertConsolData(createdConsol4, (JobSailingCollection)sailingList[1], 1);
		}

		public void TestGenerateConsols_SetsJK_STR()
		{
			var templateRecord = GetTemplateRecord();

			var multiDaysSelection = MultiDaysSelectionForTest;
			var generator = new ConsolTemplateGenerator(multiDaysSelection)
			{
				AllocateNeutralMaster = true
			};

			var template = generator.ConsolTemplates.AddNew();
			template.ConsolTemplateReferenceId = templateRecord.STR_ReferenceId;
			template.ConsolsPerFlight = 2;

			var requestedDate = new ZDateTime(2018, 11, 20);
			var departureDates = new List<ZDateTime>()
			{
				requestedDate,
				requestedDate.AddDays(1)
			};

			AssertEquals(0, multiDaysSelection.CreatedConsols.Count);

			var header = CreateHeader();
			var sailingList = RoutingResponseHeader.TryCreateEnterpriseVoyagesSailings(header, departureDates, header.Factory);
			AssertEquals(2, sailingList.Count);

			generator.GenerateConsols(sailingList.Cast<JobSailingCollection>().ToList());

			var consolsExpected = template.ConsolsPerFlight * departureDates.Count;
			AssertEquals(consolsExpected, multiDaysSelection.CreatedConsols.Count);

			foreach (var consol in multiDaysSelection.CreatedConsols.OfType<ForwardingConsol>())
			{
				AssertEquals(templateRecord.PK, consol.JK_STR);
			}
		}

		#endregion

		#region Implementation

		void AssertConsolData(ForwardingConsol createdConsol, JobSailingCollection sailings, int daysDiff)
		{
			AssertEquals(2, createdConsol.Transports.Count);
			AssertEquals(2, sailings.Count);

			var transport1 = createdConsol.Transports[0];
			AssertTransportData(createdConsol, transport1, sailings[0],
				"AUSYD", "AUMEL",
				"BA7437", "332",
				new ZDateTime(2018, 11, 20, 7, 0, 0).AddDays(daysDiff),
				new ZDateTime(2018, 11, 20, 8, 35, 0).AddDays(daysDiff));

			var transport2 = createdConsol.Transports[1];
			AssertTransportData(createdConsol, transport2, sailings[1],
				"AUMEL", "AUBNE",
				"BA4138", "333",
				new ZDateTime(2018, 11, 20, 8, 50, 0).AddDays(daysDiff),
				new ZDateTime(2018, 11, 20, 15, 15, 0).AddDays(daysDiff));
		}

		void AssertTransportData(ForwardingConsol createdConsol, Transport transport, JobSailing expectedSailing,
			ZString transportLoadPort, ZString transportDiscPort,
			ZString voyageFlight, ZString aircraftType,
			ZDateTime etd, ZDateTime eta)
		{
			AssertEquals("Transport should be in the same factory with consol", createdConsol.Factory, transport.Factory);

			var sailing = transport.Sailing;
			AssertNotNull("Transport should not be null if located in consol's factory", sailing);

			CombineAssertions(() =>
			{
				AssertEquals(Core.Constants.TransportModes.Air, createdConsol.JK_TransportMode);
				AssertEquals("AUSYD", createdConsol.JK_RL_NKLoadPort);
				AssertEquals("AUBNE", createdConsol.JK_RL_NKDischargePort);
				AssertEquals(true, createdConsol.JK_IsNeutralMaster);
				AssertNotNull(createdConsol.TemplateRecord);
				Assert(!createdConsol.IsTemplateRecord);

				AssertEquals(true, transport.JW_IsLinked);
				AssertEquals(expectedSailing.PK, transport.JW_JX);

				AssertEquals(Core.Constants.TransportModes.Air, transport.JW_TransportMode);
				AssertEquals(transportLoadPort, transport.JW_RL_NKLoadPort);
				AssertEquals(transportDiscPort, transport.JW_RL_NKDiscPort);
				AssertEquals(voyageFlight, transport.JW_VoyageFlight);
				AssertEquals(aircraftType, transport.JW_AircraftType);
				AssertEquals(etd, transport.JW_ETD);
				AssertEquals(eta, transport.JW_ETA);
			});
		}

		StmTemplateRecord GetTemplateRecord(string templateName = "A")
		{
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "AUBNE";

			var templateRecord = Factory.New<StmTemplateRecord>();
			templateRecord.STR_TemplateName = templateName;
			templateRecord.STR_ModuleID = ModuleIDs.JobConsol.Name;

			var templateRecordProvider = consol as ITemplateRecordProvider;
			templateRecordProvider.IsTemplateRecord = true;
			templateRecordProvider.TemplateRecord = templateRecord;

			using (((IDbConnected)Factory).Connection.BeginTransactionWithManager())
			{
				templateRecordProvider.SaveToTemplateRecord();
			}

			return templateRecord;
		}

		RoutingResponseHeader CreateHeader()
		{
			var messageLine = "100 SYD BNE 11:15   07:00   15:15 QF CX        <SYD 3 MEL 1   07:00   08:35 BA  7437    332     0   439                                        12345.. 16/10/03 17/03/31 J> <MEL 2 BNE 1   08:50   15:15 BA  4138    333     0  4590                                        1234567 16/10/31 17/03/26 J> ";
			var header = new RoutingResponseHeader(messageLine, Factory);
			Factory.Save();
			return header;
		}

		RoutingMultiDaysSelection MultiDaysSelectionForTest
		{
			get
			{
				var requestDate = new ZDateTime(2018, 7, 6);
				var testMessageLine1 =
					"000 SYD WUH 10:55   11:20   20:15 MU           2018/06/26 2018/10/06 1.3..6. <SYD 1 WUH     11:20   20:15 MU   750    332     0  5063                                        1..4.6. 18/06/28 18/10/06 J> ";
				var header1 = new RoutingResponseHeader(testMessageLine1, Factory);
				var testMessageLine2 =
					"000 SYD WUH 10:55   11:20   20:15 MU           2018/06/28 2018/10/08 1..4.6. <SYD 1 WUH     11:20   20:15 QF  5003    332     0  5063                                        1..4.6. 18/06/28 18/10/06 J> ";
				var header2 = new RoutingResponseHeader(testMessageLine2, Factory);
				var routingResponseHeaders = new RoutingResponseHeaderCollection(Factory)
				{
					header1,
					header2
				};

				return RoutingMultiDaysSelection.Create(requestDate, routingResponseHeaders, false, false, Factory);
			}
		}

		#endregion
	}
}
