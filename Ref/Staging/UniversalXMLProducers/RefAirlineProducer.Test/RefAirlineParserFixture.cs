using System;
using System.IO;
using System.Linq;
using System.Reflection;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.UniversalXMLProducers.RefAirlineProducer.Test
{
	[TestFixture]
	public class RefAirlineParserFixture
	{
		[Test]
		public void Output_WithMultipleKeys()
		{
			var binPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
			var filePath = Path.Combine(binPath, @"TestFiles\Airline List_Sample.txt");
			var resultXmlFilePath = Path.Combine(binPath, @"TestFiles\RefAirlineWithMultipleKeys Result.xml");
			var expectedXmlFilePath = Path.Combine(binPath, @"TestFiles\RefAirlineWithMultipleKeys Expected.xml");

			var parser = new RefAirlineParser(filePath);
			var airlineList = parser.GetAirlinesWithNumericalOrThreeLetterCode();
			Assert.AreEqual(5, airlineList.Count());

			var xmlWriter = RefAirlineXmlConfiguration.GetXmlWriter(filePath, true);
			xmlWriter.SetPublicationTime(new DateTime(2020, 12, 10, 0, 0, 0));
			XmlWriterHelper.ExportToXml(xmlWriter, airlineList, resultXmlFilePath);
			var result = File.ReadAllText(resultXmlFilePath);
			var expected = File.ReadAllText(expectedXmlFilePath).TrimEnd();
			Assert.AreEqual(expected, result);
			if (File.Exists(resultXmlFilePath))
			{
				File.Delete(resultXmlFilePath);
			}
		}

		[Test]
		public void Output_WithAirlineName1()
		{
			var binPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
			var filePath = Path.Combine(binPath, @"TestFiles\Airline List_Sample.txt");
			var resultXmlFilePath = Path.Combine(binPath, @"TestFiles\RefAirlineWithAirlineName1 Result.xml");
			var expectedXmlFilePath = Path.Combine(binPath, @"TestFiles\RefAirlineWithAirlineName1 Expected.xml");

			var parser = new RefAirlineParser(filePath);
			var airlineList = parser.GetAirlinesWithAirlineName1();
			Assert.AreEqual(2, airlineList.Count());

			var xmlWriter = RefAirlineXmlConfiguration.GetXmlWriter(filePath, false);
			xmlWriter.SetPublicationTime(new DateTime(2020, 12, 10, 0, 0, 0));
			XmlWriterHelper.ExportToXml(xmlWriter, airlineList, resultXmlFilePath);
			var result = File.ReadAllText(resultXmlFilePath);
			var expected = File.ReadAllText(expectedXmlFilePath).TrimEnd();
			Assert.AreEqual(expected, result);
			if (File.Exists(resultXmlFilePath))
			{
				File.Delete(resultXmlFilePath);
			}
		}

		[Test]
		public void SortDuplicateAirline()
		{
			var airline1 = CreateAirline("999", "AAA", "Name1", "State1");
			var airline2 = CreateAirline("999", "BBB", "Name2", "State2");
			var airline = RefAirlineParser.SortDuplicateAirline(airline1, airline2);
			Assert.AreEqual(airline2, airline);

			airline2.RM_ThreeLetterCode = airline1.RM_ThreeLetterCode;
			airline = RefAirlineParser.SortDuplicateAirline(airline1, airline2);
			Assert.AreEqual(airline2, airline);

			airline2.RM_AirlineName1 = airline1.RM_AirlineName1;
			airline = RefAirlineParser.SortDuplicateAirline(airline1, airline2);
			Assert.AreEqual(airline2, airline);
		}

		RefAirline CreateAirline(string airlinePrefix, string threeLetterCode, string airlineName1, string airlineState)
		{
			return new RefAirline
			{
				RM_EagleAddedAirlinePrefixOrAccountingCode = airlinePrefix,
				RM_ThreeLetterCode = threeLetterCode,
				RM_AirlineName1 = airlineName1,
				RM_AirlineState = airlineState
			};
		}
	}
}
