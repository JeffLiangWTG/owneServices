using System;
using System.IO;
using System.Linq;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;
using CargoWise.RefDbRepo.TRReferenceData.Business;
using CargoWise.RefDbRepo.TRReferenceData.Services;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.TRReferenceData.Tests
{
	[TestFixture]
	public class TradeGroupParserTest
	{
		[Test]
		public void DataSource()
		{
			Assert.AreEqual("TR Trade Groups and Countries", TradeGroupParser.DataSource);
		}

		[Test]
		public void PublicationDateTime()
		{
			Assert.AreEqual(Now, TradeGroupParser.PublicationDateTime);
		}

		[Test]
		public void GetEntities()
		{
			var tradeGroups = TradeGroupParser.GetEntities().Cast<RefCusTradeGroup>();
			Assert.AreEqual(40, tradeGroups.Count());
			Assert.AreEqual(683, tradeGroups.SelectMany(tradeGroup => tradeGroup.RefCusTradeGroupCountries).Count());

			var firstTradeGroup = tradeGroups.First();
			Assert.AreEqual("AKCT", firstTradeGroup.ZZA_TradeGroup);
			Assert.AreEqual("Türkiye-AB Avrupa Kömür Çelik Topluluğu Ürünleri STA’sı", firstTradeGroup.ZZA_Description);
			Assert.AreEqual(new DateTime(2020, 01, 01, 00, 00, 00), firstTradeGroup.ZZA_StartDate);
			Assert.AreEqual(new DateTime(2079, 06, 06, 23, 59, 00), firstTradeGroup.ZZA_EndDate);

			var firstCountry = firstTradeGroup.RefCusTradeGroupCountries[0];
			Assert.AreEqual("FR", firstCountry.ZZB_RN_NKTradeGroupCountryCode);
			Assert.AreEqual("France", firstCountry.ZZB_Description);
			Assert.AreEqual(new DateTime(2020, 01, 01, 00, 00, 00), firstCountry.ZZB_StartDate);
			Assert.AreEqual(new DateTime(2079, 06, 06, 00, 00, 00), firstCountry.ZZB_EndDate);

			var baeTradeGroup = tradeGroups.First(x => x.ZZA_TradeGroup == "BAE");
			Assert.AreEqual("BAE", baeTradeGroup.ZZA_TradeGroup);
			Assert.AreEqual("T.C. ile BAE Arasında Kapsamlı Ekonomik Ortaklık Anlaşması", baeTradeGroup.ZZA_Description);
			Assert.AreEqual(new DateTime(2023, 09, 06, 00, 00, 00), baeTradeGroup.ZZA_StartDate);
			Assert.AreEqual(new DateTime(2079, 06, 06, 23, 59, 00), baeTradeGroup.ZZA_EndDate);
		}

		[Test]
		public void GetXmlWriterConfiguration()
		{
			var config = TradeGroupParser.GetXmlWriterConfiguration();
			Assert.That(config, Is.Not.Null);

			var refType = typeof(RefCusTradeGroup);
			var entityConfig = config.GetConfiguration(refType);

			Assert.That(entityConfig, Is.Not.Null);
			Assert.That(entityConfig.IsIncluded(refType.GetProperty(nameof(RefCusTradeGroup.ZZA_TradeGroup))));
			Assert.That(entityConfig.IsIncluded(refType.GetProperty(nameof(RefCusTradeGroup.ZZA_Description))));
			Assert.That(entityConfig.IsIncluded(refType.GetProperty(nameof(RefCusTradeGroup.ZZA_StartDate))));
			Assert.That(entityConfig.IsIncluded(refType.GetProperty(nameof(RefCusTradeGroup.ZZA_EndDate))));
			Assert.That(entityConfig.IsIncluded(refType.GetProperty(nameof(RefCusTradeGroup.ZZA_ZZZ_NKDataGrouping))));
			Assert.That(entityConfig.IsIncluded(refType.GetProperty(nameof(RefCusTradeGroup.RefCusTradeGroupCountries))));

			refType = typeof(RefCusTradeGroupCountry);
			entityConfig = config.GetConfiguration(refType);

			Assert.That(entityConfig, Is.Not.Null);
			Assert.That(entityConfig.IsIncluded(refType.GetProperty(nameof(RefCusTradeGroupCountry.ZZB_RN_NKTradeGroupCountryCode))));
			Assert.That(entityConfig.IsIncluded(refType.GetProperty(nameof(RefCusTradeGroupCountry.ZZB_StartDate))));
			Assert.That(entityConfig.IsIncluded(refType.GetProperty(nameof(RefCusTradeGroupCountry.ZZB_EndDate))));
			Assert.That(entityConfig.IsIncluded(refType.GetProperty(nameof(RefCusTradeGroupCountry.ZZB_Description))));
		}

		[OneTimeSetUp]
		public void OneTimeSetup()
		{
			tempFolder = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
			Directory.CreateDirectory(tempFolder);
			TestHelper.SimulateDownload(DataFileName, "CargoWise.RefDbRepo.TRReferenceData.Tests.TradeGroups.TestFiles.Input.TradeGroupData.xlsx");
		}

		[OneTimeTearDown]
		public void OneTimeTearDown()
		{
			if (Directory.Exists(tempFolder))
			{
				Directory.Delete(tempFolder, true);
			}
		}

		string tempFolder;

		string DataFileName => Path.Combine(tempFolder, "TradeGroupData.xlsx");

		DateTime Now => new DateTime(2023, 07, 18, 09, 38, 23);

		IDateTimeProvider DateTimeProvider => TestHelper.MockDateTimeProvider(Now);

		IReferenceDataParser TradeGroupParser => fTradeGroupParser ?? (fTradeGroupParser = new TradeGroupParser(DateTimeProvider, DataFileName));
		TradeGroupParser fTradeGroupParser;
	}
}
