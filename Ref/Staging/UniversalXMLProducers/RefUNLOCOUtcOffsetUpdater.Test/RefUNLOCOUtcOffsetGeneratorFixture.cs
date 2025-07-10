using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using CargoWise.RefDbRepo.Common.DbUpgrade;
using CargoWise.RefDbRepo.Common.SafeDataClient.CargoWise.RefDbRepo.Service.Schema_0_9_New;
using Moq;
using NodaTime;
using NodaTime.TimeZones;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.UniversalXMLProducers.RefUNLOCOUtcOffsetUpdater.Test
{
	[TestFixture]
	class RefUNLOCOUtcOffsetGeneratorFixture
	{
		[Test]
		public void RefUNLOCOUtcOffsetGeneratorOutputTest()
		{
			var outputDate = new DateTime(2023, 7, 1);
			var dataHelper = new Mock<UnlocoDataHelper>();
			dataHelper.Setup(x => x.GetRefUNLOCODictionary()).Returns(unlocoDictionary);
			dataHelper.Setup(x => x.GetRefTimezoneSetDictionary()).Returns(timezoneSetDictionary);
			var xmlHelper = new RefUNLOCOUtcOffsetXmlProducer(outputDate);
			var producer = new RefUNLOCOUtcOffsetGenerator(dataHelper.Object, outputDate, 24, xmlHelper, timezoneDb);
			producer.ExportXml();
			var expectedResultXml = "RefUNLOCOUtcOffsetExpectedOutput.xml";
			var outputFile = "UnlocoUtcOffset.xml";
			var exportFilepath = Path.Combine(ApplicationConfig.OutputFilePath, outputFile);
			var expectedExportFilepath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"TestFiles\" + expectedResultXml + "");
			var xmlDoc = new XmlDocument();
			xmlDoc.Load(exportFilepath);

			var expectedXmlDoc = new XmlDocument();
			expectedXmlDoc.Load(expectedExportFilepath);

			Assert.AreEqual(expectedXmlDoc.InnerXml, xmlDoc.InnerXml);
		}

		[Test]
		public void GenerateTransitionBetweenDatesTest()
		{
			var generator = new RefUNLOCOUtcOffsetGeneratorTest(new DateTime(2023, 7, 1), 24, timezoneDb);
			using (var stream = File.OpenRead(timezoneDb))
			{
				var source = TzdbDateTimeZoneSource.FromStream(stream);
				var provider = new DateTimeZoneCache(source);
				var zone = provider.GetZoneOrNull("Africa/Cairo");
				var intervals = generator.GetTransitions(zone);
				var results = generator.TransformTransitionsTest(intervals);
				Assert.AreEqual(ExpectedEGCAI.Count, results.Count);
				for (int i = 0; i < results.Count; i++)
				{
					Assert.AreEqual(ExpectedEGCAI[i].StartDate, results[i].StartDate);
					Assert.AreEqual(ExpectedEGCAI[i].EndDate, results[i].EndDate);
					Assert.AreEqual(ExpectedEGCAI[i].UtcOffset, results[i].UtcOffset);
				}

				zone = provider.GetZoneOrNull("Australia/Sydney");
				intervals = generator.GetTransitions(zone);
				results = generator.TransformTransitionsTest(intervals);
				Assert.AreEqual(ExpectedAUSYD.Count, results.Count);
				for (int i = 0; i < results.Count; i++)
				{
					Assert.AreEqual(ExpectedAUSYD[i].StartDate, results[i].StartDate);
					Assert.AreEqual(ExpectedAUSYD[i].EndDate, results[i].EndDate);
					Assert.AreEqual(ExpectedAUSYD[i].UtcOffset, results[i].UtcOffset);
				}
			}
		}

		static List<RefUNLOCOUtcOffsetRecord> ExpectedEGCAI = new List<RefUNLOCOUtcOffsetRecord>()
		{
			new RefUNLOCOUtcOffsetRecord() { StartDate = new DateTime(2014,09,25,21,0,0), EndDate = new DateTime(2023,04,27,22,0,0), UtcOffset = 120 },
			new RefUNLOCOUtcOffsetRecord() { StartDate = new DateTime(2023,04,27,22,0,0), EndDate = new DateTime(2023,10,26,21,0,0), UtcOffset = 180 },
			new RefUNLOCOUtcOffsetRecord() { StartDate = new DateTime(2023,10,26,21,0,0), EndDate = new DateTime(2024,04,25,22,0,0), UtcOffset = 120 },
			new RefUNLOCOUtcOffsetRecord() { StartDate = new DateTime(2024,04,25,22,0,0), EndDate = new DateTime(2024,10,31,21,0,0), UtcOffset = 180 },
			new RefUNLOCOUtcOffsetRecord() { StartDate = new DateTime(2024,10,31,21,0,0), EndDate = new DateTime(2025,04,24,22,0,0), UtcOffset = 120 },
			new RefUNLOCOUtcOffsetRecord() { StartDate = new DateTime(2025,04,24,22,0,0), EndDate = new DateTime(2025,10,30,21,0,0), UtcOffset = 180 },
		};

		static List<RefUNLOCOUtcOffsetRecord> ExpectedAUSYD = new List<RefUNLOCOUtcOffsetRecord>()
		{
			new RefUNLOCOUtcOffsetRecord() { StartDate = new DateTime(2021,4,3,16,0,0), EndDate = new DateTime(2021,10,2,16,0,0), UtcOffset = 600 },
			new RefUNLOCOUtcOffsetRecord() { StartDate = new DateTime(2021,10,2,16,0,0), EndDate = new DateTime(2022,4,2,16,0,0), UtcOffset = 660 },
			new RefUNLOCOUtcOffsetRecord() { StartDate = new DateTime(2022,4,2,16,0,0), EndDate = new DateTime(2022,10,1,16,0,0), UtcOffset = 600 },
			new RefUNLOCOUtcOffsetRecord() { StartDate = new DateTime(2022,10,1,16,0,0), EndDate = new DateTime(2023,4,1,16,0,0), UtcOffset = 660 },
			new RefUNLOCOUtcOffsetRecord() { StartDate = new DateTime(2023,4,1,16,0,0), EndDate = new DateTime(2023,9,30,16,0,0), UtcOffset = 600 },
			new RefUNLOCOUtcOffsetRecord() { StartDate = new DateTime(2023,9,30,16,0,0), EndDate = new DateTime(2024,4,6,16,0,0), UtcOffset = 660 },
			new RefUNLOCOUtcOffsetRecord() { StartDate = new DateTime(2024,4,6,16,0,0), EndDate = new DateTime(2024,10,5,16,0,0), UtcOffset = 600 },
			new RefUNLOCOUtcOffsetRecord() { StartDate = new DateTime(2024,10,5,16,0,0), EndDate = new DateTime(2025,4,5,16,0,0), UtcOffset = 660 },
			new RefUNLOCOUtcOffsetRecord() { StartDate = new DateTime(2025,4,5,16,0,0), EndDate = new DateTime(2025,10,4,16,0,0), UtcOffset = 600 },
		};

		[SetUp]
		public void Setup()
		{
			var unloco1TimezoneSetGuid = Guid.NewGuid();
			var unloco2TimezoneSetGuid = Guid.NewGuid();
			var unloco3TimezoneSetGuid = Guid.NewGuid();

			timezoneSetDictionary = new Dictionary<Guid, RefTimeZoneSet>
			{
				{
					unloco1TimezoneSetGuid,
					new RefTimeZoneSet() { R3_IsActive = true, R3_PK = unloco1TimezoneSetGuid, R3_TimeZoneSetName = "Australia/Sydney" }
				},
				{
					unloco2TimezoneSetGuid,
					new RefTimeZoneSet() { R3_IsActive = true, R3_PK = unloco2TimezoneSetGuid, R3_TimeZoneSetName = "Africa/Cairo" }
				},
				{
					unloco3TimezoneSetGuid,
					new RefTimeZoneSet() { R3_IsActive = true, R3_PK = unloco3TimezoneSetGuid, R3_TimeZoneSetName = "America/Sao_Paulo" }
				},
			};

			unlocoDictionary = new Dictionary<Guid, string[]>()
			{
				{ unloco1TimezoneSetGuid, new [] { "AUSYD" } },
				{ unloco2TimezoneSetGuid, new [] { "EGCAI" } },
				{ unloco3TimezoneSetGuid, new [] { "BRSAO" } },
			};
		}

		Dictionary<Guid, string[]> unlocoDictionary;
		Dictionary<Guid, RefTimeZoneSet> timezoneSetDictionary;
		string timezoneDb = Path.Combine(FolderHelper.GetBinFolder(), @"TestFiles\tzdb2023c.nzd");
	}

	public class RefUNLOCOUtcOffsetGeneratorTest : RefUNLOCOUtcOffsetGenerator
	{
		public RefUNLOCOUtcOffsetGeneratorTest(DateTime requiredDataDate, short requiredOffsetPeriodInMonths, string timezoneDb) : base(null, requiredDataDate, requiredOffsetPeriodInMonths, null, timezoneDb)
		{ }

		public List<RefUNLOCOUtcOffsetRecord> TransformTransitionsTest(List<ZoneInterval> intervals)
		{
			return TransformTransitions(intervals);
		}

		public new List<ZoneInterval> GetTransitions(DateTimeZone timeZone)
		{
			return base.GetTransitions(timeZone);
		}
	}
}
