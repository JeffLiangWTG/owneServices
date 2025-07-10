using System;
using System.Globalization;
using System.IO;
using System.Reflection;
using CargoWise.RefDbRepo.USReferenceData.Business;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.USReferenceData.Tests
{
	[TestFixture]
	public class NewWatchTariffTest
	{
		[Test]
		public void NewAddTariffAttributeToWatchTariffTest()
		{
			var directory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
			var path = Path.Combine(directory, @"RefCusCodeList\US New Watch Rules.txt");
			var publicationTime = new DateTime(2021, 12, 25, 12, 01, 05);
			var outPutFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "USNewWatchTariff_" + publicationTime.ToString("yyyyMMddTHHmmss", CultureInfo.InvariantCulture) + ".xml");

			var parser = new NewWatchTariffParser(path, outPutFilePath, publicationTime);
			parser.ParseToXMLFile();

			var expectedXML = ReadTestFile("USNewWatchTariff.xml");
			var generatedXML = File.ReadAllText(outPutFilePath);
			Assert.That(expectedXML, Is.EqualTo(generatedXML));
		}

		string ReadTestFile(string fileName)
		{
			var result = string.Empty;
			var assembly = Assembly.GetExecutingAssembly();
			using (var stream = assembly.GetManifestResourceStream("CargoWise.RefDbRepo.USReferenceData.Tests.Tariffs.TestFiles.Output." + fileName))
			using (var reader = new StreamReader(stream))
			{
				result = reader.ReadToEnd();
			}

			return result;
		}

		[SetUp]
		public void Setup()
		{
			assembly = Assembly.GetExecutingAssembly();
		}
		Assembly assembly;
	}
}
