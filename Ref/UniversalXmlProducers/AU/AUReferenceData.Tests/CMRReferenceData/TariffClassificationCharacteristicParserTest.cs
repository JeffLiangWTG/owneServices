using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using CargoWise.RefDbRepo.AUReferenceData.Business;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.AUReferenceData.Tests.CMRReferenceData
{
	[TestFixture]
	sealed class TariffClassificationCharacteristicParserTest
	{
		string TestFileFolderName => "TariffClassificationCharacteristic";

		string TextFileName => "TRFCCHAR-P1-EDMAIN-2111300133.txt";

		Dictionary<string, List<string>> ExpectedOutput
		{
			get
			{
				return new Dictionary<string, List<string>>
				{
					{ "15162010", new List<string>{ "29" }},
					{ "24022080", new List<string>{ "30", "36", "29", "41" }},
					{ "22043090", new List<string>{ "35", "30" }},
					{ "20030063", new List<string>{ "29", "30", "35" }},
					{ "22060076", new List<string>{ "29", "30", "35" }}
				};
			}
		}

		[Test]
		public void TestParser()
		{
			var executingAssembly = Assembly.GetExecutingAssembly();
			var manifestResourcePathBase = string.Join(".", executingAssembly.GetName().Name, "CMRReferenceData", "TestFiles", TestFileFolderName);
			using (var txtStream = executingAssembly.GetManifestResourceStream(string.Join(".", manifestResourcePathBase, TextFileName)))
			using (var txtReader = new StreamReader(txtStream))
			{
				var actualOutput = TariffClassificationCharacteristicParser.Parse(txtReader.ReadToEnd());
				Assert.AreEqual(ExpectedOutput.Count, actualOutput.Count, "Both dictionaries contain the same number of elements");
				Assert.IsTrue(actualOutput.All(x => ExpectedOutput[x.Key].SequenceEqual(x.Value)), "Both dictionaries contain the same elements");
			}
		}
	}
}
