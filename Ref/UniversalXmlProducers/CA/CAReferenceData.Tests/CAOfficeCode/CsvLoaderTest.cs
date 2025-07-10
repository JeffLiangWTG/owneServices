using System.Collections.Generic;
using System.IO;
using System.Reflection;
using CargoWise.RefDbRepo.CAReferenceData.Business.CAOfficeCode;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.CAReferenceData.Tests.CAOfficeCode
{
	[TestFixture]
	public class CsvLoaderTest
	{
		[Test]
		public void TestGetUSPortOfExitMapping()
		{
			var filePath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "USPortOfExitMapping.csv");
			var dict = CsvLoader.GetUSPortOfExitMapping(filePath);
			Assert.AreEqual(27, dict.Count);
			CollectionAssert.AreEquivalent(dict.Keys, ExpectedOfficeCodeList);
			CollectionAssert.AreEquivalent(dict.Values, ExpectedUSPortOfExitList);

			Assert.AreEqual("0106", dict["0212"]);
		}

		IEnumerable<string> ExpectedOfficeCodeList => new string[]
		{
			"0212", "0440", "0705", "0453", "0314", "0441", "0454", "0813", "0456", "0507", "0410", "0602", "0213", "0817", "0818", "0231", "0841", "0354", "0328", "0409", "0211", "0351", "0452", "0607", "0842", "0502", "0427"
		};

		IEnumerable<string> ExpectedUSPortOfExitList => new string[]
		{
			"0106", "3802", "3310", "3801", "0209", "3803", "3801", "3004", "0708", "3422", "0901", "3403", "0110", "3009", "3302", "0115", "3023", "0211", "0212", "0704", "0115", "0712", "3801", "3301", "3004", "3401", "0901"
		};
	}
}
