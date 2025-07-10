using System;
using System.IO;
using System.Xml;
using CargoWise.RefDbRepo.USReferenceData.Business;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.USReferenceData.Tests
{
	[TestFixture]
	public class A99TariffParserTests
	{
		[Test]
		public void TestA99TariffParserForXML()
		{
			var testFilesPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "A99Tariffs\\TestFiles\\");
			var outputFileName = Path.Combine(testFilesPath, "20230315T000000_USA99Tariff.xml");
			var parser = new A99TariffsParser(testFilesPath);
			parser.ParseToXMLFile(Path.Combine(testFilesPath, "TestInput.json"));

			var xmlDoc = new XmlDocument();
			xmlDoc.Load(outputFileName);

			var expectedXmlDoc = new XmlDocument();
			expectedXmlDoc.Load(Path.Combine(testFilesPath, "TestOutput.xml"));
			Assert.AreEqual(expectedXmlDoc.InnerXml, xmlDoc.InnerXml);
		}

		[Test]
		public void TestProcessInvalidFiles()
		{
			var testFilesPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "A99Tariffs\\TestFiles\\");
			var processFile = Path.Combine(testFilesPath, "empty.json");

			var parser = new A99TariffsParser(testFilesPath);
			var error = parser.ParseToXMLFile(processFile);

			Assert.That(error, Does.Contain("No tariff data defined in json file."));

			processFile = Path.Combine(testFilesPath, "InvalidSchema.json");
			parser.ParseToXMLFile(processFile);
			Assert.That(consoleOutput.ToString(), Does.Contain($"{processFile} has invalid json format."));

			processFile = Path.Combine(testFilesPath, "InvalidSchema2.json");
			error = parser.ParseToXMLFile(processFile);
			Assert.That(error, Does.Contain($"Schema Error in {processFile}: Required properties are missing from object: TariffType"));

			processFile = Path.Combine(testFilesPath, "InvalidDateTime.json");
			error = parser.ParseToXMLFile(processFile);
			Assert.That(error, Does.Contain($"Invalid or missing start date for tariff: 9903.88.01"));

			processFile = Path.Combine(testFilesPath, "InvalidProperty.json");
			error = parser.ParseToXMLFile(processFile);
			Assert.That(error, Does.Contain($"xml file generated with 2 Tariff records"));
		}

		[TearDown]
		public void Cleanup()
		{
			Console.SetError(originalConsoleOutput);
			consoleOutput.Dispose();
		}

		[SetUp]
		public virtual void SetUp()
		{
			originalConsoleOutput = Console.Error;
			consoleOutput = new StringWriter();
			Console.SetError(consoleOutput);
		}
		StringWriter consoleOutput;
		TextWriter originalConsoleOutput;
	}
}
