using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CargoWise.RefDbRepo.CAReferenceData.Business.CAHarmonizedTariffs;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.CAReferenceData.Tests
{
	[TestFixture]
	public class ConditionDataParserTest
	{
		[Test]
		public void TestParse()
		{
			var result = new ConditionDataParser(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"TestFiles\Input\CAHarmonizedTariff\all-pga-programs-cbsa-sw-matching-criteria.xlsx")).Parse();

			var equalToDictionary = result.EqualToDictionary;
			Assert.AreEqual(12, equalToDictionary.Count);
			CollectionAssert.AreEquivalent(equalToDictionary.Keys, ExpectedEqualToDictionaryKeyList);
			Assert.AreEqual(1, equalToDictionary["0507900000"].PGAs.Count);

			var pgas = equalToDictionary["0304920000"].PGAs;
			Assert.AreEqual(2, pgas.Count);
			Assert.AreEqual("DFO", pgas[0].PGACode);
			Assert.AreEqual("AQUATIC INVASIVE SPECIES", pgas[0].Program);
			Assert.AreEqual("DFO", pgas[1].PGACode);
			Assert.AreEqual("TRADE TRACKING", pgas[1].Program);

			var startsWithDictionary = result.StartsWithDictionary;
			Assert.AreEqual(11, startsWithDictionary.Count);
			CollectionAssert.AreEquivalent(startsWithDictionary.Keys, ExpectedStartsWithDictionaryKeyList);

			var withinRangeDictionary = result.WithinRangeDictionary;
			Assert.AreEqual(3, withinRangeDictionary.Count);
			CollectionAssert.AreEquivalent(withinRangeDictionary.Keys, ExpectedWithinRangeDictionaryKeyList);

			var withinRangeHSCodeFromList = result.WithinRangeHSCodeFromList;
			Assert.AreEqual(3, withinRangeHSCodeFromList.Count);
			CollectionAssert.AreEqual(ExpectedWithinRangeDictionaryKeyList, withinRangeHSCodeFromList);
		}

		[Test]
		public void TestGetRefCusConditions_PGAConditionPatch()
		{
			var parser = new ConditionDataParser(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"TestFiles\Input\CAHarmonizedTariff\all-pga-programs-cbsa-sw-matching-criteria.xlsx"));
			var testTariffs = new string[] { "7601100020", "7601100090", "7601200010", "7601200021", "7601200029", "7601200090", "7604100030", "7604100040", "7604210010", "7604210090", "7604290011", "7604290019", "7604290021", "7604290029", "7604290030", "7605110000", "7605190000", "7605210000", "7605290000", "7606110010", "7606110090", "7606120011", "7606120012", "7606120020", "7606910010", "7606910090", "7606920000", "7607110010", "7607110020", "7607110030", "7607190000", "7607200010", "7607200090", "7608100010", "7608100090", "7608200000", "7609000000", "7616999021", "7616999029", "7616999030" };
			foreach (var item in testTariffs)
			{
				var refCusConditions = parser.GetRefCusConditions(item);
				Assert.AreEqual(2, refCusConditions.Count);
				var pgaProgram = refCusConditions[0].RefCusConditionValues.FirstOrDefault(f => f.ZX3_ZX4_NKValueType == "GAC");
				Assert.AreEqual("ALL", pgaProgram.ZX3_Value, "PGA Condition Value");

				var pgacProgram = refCusConditions[1].RefCusConditionValues.FirstOrDefault(f => f.ZX3_ZX4_NKValueType == "GAC");
				Assert.AreEqual("GIP83", pgacProgram.ZX3_Value, "PGAC Condition Value");
			}

		}

		IEnumerable<string> ExpectedEqualToDictionaryKeyList => new string[]
		{
			"0507900000","0308909000","0309909900","0304920000","2301101000","8405100000","0309909100","2309903959","2309903990","9306901020","7102100000","8716400000"
		};

		IEnumerable<string> ExpectedStartsWithDictionaryKeyList => new string[]
		{
			"2845200000","2845300000","2845400000","030799","9706","01051121","3822120000","3822190000","3002490010","3002490020","40111"
		};

		IEnumerable<string> ExpectedWithinRangeDictionaryKeyList => new string[]
		{
			"7206000000","9703100000","9705220000"
		};
	}
}
