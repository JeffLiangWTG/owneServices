using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using CargoWise.RefDbRepo.AUReferenceData.Business;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.AUReferenceData.Tests.CMRReferenceData
{
	[TestFixture]
	sealed class StatisticalClassificationPeriodCharacteristicParserTest
	{
		string TestFileFolderName => "StatisticalClassificationPeriodCharacteristic";

		string TextFileName => "STCPCHAR-P1-EDMAIN-2208050143.txt";

		Dictionary<string, List<string>> ExpectedOutput
		{
			get
			{
				return new Dictionary<string, List<string>>
				{
					{ "9999300615", new List<string>{ "1" }},
					{ "8703501941", new List<string>{ "1", "7" }},
					{ "8703211903", new List<string>{ "7" }},
					{ "2206003020", new List<string>{ "16" }},
					{ "4412130032", new List<string>{ "20", "27" }},
					{ "4407111040", new List<string>{ "25" }},
					{ "4410290015", new List<string>{ "27" }},
					{ "4412310054", new List<string>{ "304" }},
					{ "4407941012", new List<string>{ "7219" }},
				};
			}
		}

		[Test]
		public void TestParser()
		{
			var manifestResourcePathBase = string.Join(".", executingAssembly.GetName().Name, "CMRReferenceData", "TestFiles", TestFileFolderName);
			using (var txtStream = executingAssembly.GetManifestResourceStream(string.Join(".", manifestResourcePathBase, TextFileName)))
			using (var txtReader = new StreamReader(txtStream))
			{
				var actualOutput = StatisticalClassificationPeriodCharacteristicParser.Parse(txtReader.ReadToEnd());
				Assert.AreEqual(ExpectedOutput.Count, actualOutput.Count, "Both dictionaries contain the same number of elements");
				Assert.IsTrue(actualOutput.All(x => ExpectedOutput[x.Key].SequenceEqual(x.Value)), "Both dictionaries contain the same elements");
			}
		}

		[SetUp]
		public void Setup()
		{
			executingAssembly = Assembly.GetExecutingAssembly();
		}
		Assembly executingAssembly;
	}
}
