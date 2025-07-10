using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;
using CargoWise.RefDbRepo.CAReferenceData.Business.CAExchangeRate;
using Newtonsoft.Json;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.CAReferenceData.Tests
{
	[TestFixture]
	sealed class CAExchangeRateParserTest
	{
		[TestCase]
		public void TestParseExchangeRateIntoXML()
		{
			AssertXmlIsExpected(foreignExchangeRates_01, "CBSAExchangeRates_01.xml", new DateTime(2023, 03, 07));
			AssertXmlIsExpected(foreignExchangeRates_02, "CBSAExchangeRates_02.xml", new DateTime(2023, 02, 16));
		}

		void AssertXmlIsExpected(List<ForeignExchangeRates> foreignExchangeRates, string expectedXmlName, DateTime publicationDateTime)
		{
			var outputFile = "CBSAExchangeRates.xml";
			var exportFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"TestFiles\Output\" + outputFile + "");
			using (var expectedResultXml = TestHelper.GetTestInputFile(expectedXmlName))
			{
				var supportingDocument = new CAExchangeRateParser();
				supportingDocument.ParseExchangeRateIntoXML(foreignExchangeRates, exportFilePath, publicationDateTime);
				var xmlDoc = new XmlDocument();
				xmlDoc.Load(exportFilePath);
				var expectedXmlDoc = new XmlDocument();
				expectedXmlDoc.Load(expectedResultXml);
				Assert.AreEqual(xmlDoc.InnerXml, expectedXmlDoc.InnerXml);
			}
		}

		void PopulateListOfRates(List<ForeignExchangeRates> foreignExchangeRates, string jsonName)
		{
			var jsonFilepath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"TestFiles\Input\CAExchangeRate\" + jsonName);
			string json = string.Empty;
			using (FileStream fs = new FileStream(jsonFilepath, FileMode.OpenOrCreate, System.IO.FileAccess.ReadWrite, FileShare.ReadWrite))
			{
				using (StreamReader sr = new StreamReader(fs, Encoding.UTF8))
				{
					json = sr.ReadToEnd().ToString();
				}
			}
			CAExchangeRates exchangeRateJson = JsonConvert.DeserializeObject<CAExchangeRates>(json);

			if (exchangeRateJson != null)
			{
				foreignExchangeRates.AddRange(exchangeRateJson.ForeignExchangeRates);
			}
		}

		[SetUp]
		public void SetUp()
		{
			foreignExchangeRates_01 = new List<ForeignExchangeRates>() { };
			PopulateListOfRates(foreignExchangeRates_01, "ForeignExchangeRates_01.json");

			foreignExchangeRates_02 = new List<ForeignExchangeRates>() { };
			PopulateListOfRates(foreignExchangeRates_02, "ForeignExchangeRates_02.json");
		}
		List<ForeignExchangeRates> foreignExchangeRates_01;
		List<ForeignExchangeRates> foreignExchangeRates_02;
	}
}
