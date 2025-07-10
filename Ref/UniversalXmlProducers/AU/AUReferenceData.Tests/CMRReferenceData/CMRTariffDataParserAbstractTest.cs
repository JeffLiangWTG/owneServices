using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using CargoWise.RefDbRepo.AUReferenceData.Business;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.AUReferenceData.Tests.CMRReferenceData
{
	abstract class CMRTariffDataParserAbstractTest : CommonCMRDataParserAbstractTest
	{
		protected abstract string[] StringArgs { get; }

		protected abstract ITariffDataParser Parser { get; }

		protected abstract DateTime TodaysDate { get; }

		protected string TestTariffsSourceFilePath
		{
			get
			{
				var outputFolderPath = Path.Combine(Path.GetDirectoryName(executingAssembly.Location), "TestFiles");
				return Path.Combine(outputFolderPath, "AU Tariffs.json");
			}
		}

		protected Dictionary<string, List<string>> STCPCharacteristicCodes
		{
			get
			{
				return new Dictionary<string, List<string>>()
				{
					{ "0701900002", new List<string>{ "141", "9", "12" }},
					{ "0201100001", new List<string>{ "7" }},
					{ "9221100008", new List<string>{ "170" }},
					{ "0410000026", new List<string>{ "20", "159" }},
					{ "2203006211", new List<string>{ "3761" }},
				};
			}
		}

		protected Dictionary<string, List<string>> TRFCCharacteristicCodes
		{
			get
			{
				return new Dictionary<string, List<string>>()
				{
					{ "04100000", new List<string>{ "20", "159", "72" }},
					{ "02011000", new List<string>{ "7", "21" }},
					{ "20099000", new List<string>{ "35", "41" }},
					{ "35216172", new List<string>{ "10", "20" }},
					{ "44092000", new List<string>{ "99" }},
				};
			}
		}

		protected Dictionary<string, List<string>> AQISCommodityStatisticalClassificationCodes
		{
			get
			{
				return new Dictionary<string, List<string>>()
				{
					{ "3102100003", new List<string>{ "FTR1", "FTR2" }},
					{ "9401300003", new List<string>{ "ANIM" }},
				};
			}
		}

		[Test]
		public void TestParser()
		{
			var manifestResourcePathBase = string.Join(".", executingAssembly.GetName().Name, "CMRReferenceData", "TestFiles", TestFileFolderName);
			using (var txtStream = executingAssembly.GetManifestResourceStream(string.Join(".", manifestResourcePathBase, TextFileName)))
			using (var txtReader = new StreamReader(txtStream))
			using (var expectedXmlStream = executingAssembly.GetManifestResourceStream(string.Join(".", manifestResourcePathBase, XMLFileName)))
			using (var expectedXmlReader = new StreamReader(expectedXmlStream))
			{
				var outputFolderPath = Path.Combine(Path.GetDirectoryName(executingAssembly.Location), "TestFiles");
				Directory.CreateDirectory(outputFolderPath);

				Parser.Parse(txtReader.ReadToEnd(), StringArgs, outputFolderPath);

				var outputFilePath = Path.Combine(outputFolderPath, XMLFileName);
				using (var actualXmlStream = new FileStream(outputFilePath, FileMode.Open))
				using (var actualXmlReader = new StreamReader(actualXmlStream))
				{
					Assert.AreEqual(expectedXmlReader.ReadToEnd(), actualXmlReader.ReadToEnd());
				}
				File.Delete(outputFilePath);
			}
		}

		[OneTimeSetUp]
		public void OneTimeSetUp()
		{
			jsonOptions = new JsonSerializerOptions
			{
				PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
				PropertyNameCaseInsensitive = true
			};
		}

		[SetUp]
		public void SetUp()
		{
			if (File.Exists(TestTariffsSourceFilePath))
			{
				return;
			}

			var manifestResourcePathBase = string.Join(".", executingAssembly.GetName().Name, "CMRReferenceData", "TestFiles", TestFileFolderName);
			using (var txtStream =
			       executingAssembly.GetManifestResourceStream(string.Join(".", manifestResourcePathBase, "AU_Tariff_Source.json")))
			using (var txtReader = new StreamReader(txtStream))
			using (var testFileWriteStream = new FileStream(TestTariffsSourceFilePath, FileMode.Create, FileAccess.Write))
			using (var writer = new StreamWriter(testFileWriteStream))
			{
				var content = txtReader.ReadToEnd();
				writer.Write(content);
			}
		}

		[OneTimeTearDown]
		public void OneTimeTearDown()
		{
			if (File.Exists(TestTariffsSourceFilePath))
			{
				File.Delete(TestTariffsSourceFilePath);
			}
		}

		JsonSerializerOptions jsonOptions;
	}
}
