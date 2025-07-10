using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using CargoWise.RefDbRepo.CAReferenceData.Business;
using CargoWise.RefDbRepo.CAReferenceData.Business.CAHarmonizedTariffs;
using CargoWise.RefDbRepo.CAReferenceData.Model;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.CAReferenceData.Tests
{
	[TestFixture]
	public class HarmonizedCsvLoaderTest
	{
		[Test]
		public void TestGetConveyanceRequiredTariffList()
		{
			var list = CsvLoader.GetConveyanceRequiredTariffList();
			Assert.AreEqual(241, list.Count());
			CollectionAssert.AreEquivalent(list, ExpectedTariffList);
		}

		[Test]
		public void TestGetTTCodeList()
		{
			var list = CsvLoader.Deserialize<CAHarmonizedTTCode>(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "TTCodeList.csv"));
			Assert.AreEqual(25, list.Count);
			var newList = list.Select(x => x.Code);
			CollectionAssert.AreEquivalent(list.Select(x => x.Code), ExpectedTTCodeList);
			CollectionAssert.AreEquivalent(list.Select(x => x.Description), ExpectedDescriptionList);
			CollectionAssert.AreEquivalent(list.Select(x => x.Abbreviation), ExpectedAbbreviationList);
		}

		[Test]
		public void TestGetGSTCodeList()
		{
			var list = CsvLoader.Deserialize<CACTaxRate>(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), ApplicationConfig.GSTCodePatchFileName));
			Assert.AreEqual(4, list.Count);
			var first = list.First();
			Assert.AreEqual("001", first.TaxRefNumber);
			Assert.AreEqual(DateTime.Parse("2008-01-01 12:00:00.000", CultureInfo.InvariantCulture), first.EffectiveDate);
			Assert.AreEqual(DateTime.Parse("2079-06-05 00:00:00.000", CultureInfo.InvariantCulture), first.ExpiryDate);
			Assert.AreEqual("Y", first.CheckInd);
			Assert.AreEqual("1", first.CheckGroup);
			Assert.AreEqual("V", first.RateType);
			Assert.AreEqual("5.00000", first.Rate);
			Assert.AreEqual("NORMAL RATE", first.Title);
		}

		IEnumerable<string> ExpectedTTCodeList => new string[]
		{
			"02","03","04","05","07","08","09","10","11","13","14","21","22","23","24","25","26","27","28","29","30","31","32","33","34"
		};

		IEnumerable<string> ExpectedAbbreviationList => new string[]
		{
			"MFN","GT","AUT","NZT","CCCT","LDCT","GPT","UST","MXT","CIAT","CT","CRT","IT","NT","SLT","PT","COLT","JT","PAT","HNT","KRT","CEUT","UAT","CPTPT","UKT"
		};

		IEnumerable<string> ExpectedDescriptionList => new string[]
		{
			"Most-Favoured-Nation",
"General Tariff",
"Australia Tariff",
"New Zealand Tariff",
"Commonwealth Caribbean Countries Tariff",
"Least Developed Countries Tariff",
"General Preferential Tariff",
"United States Tariff",
"Mexico Tariff",
"Canada-Israel Agreement Tariff",
"Chile Tariff",
"Costa Rica Tariff",
"Iceland Tariff",
"Norway Tariff",
"Switzerland-Lichtenstein Tariff",
"Peru Tariff",
"Colombia Tariff",
"Jordan Tariff",
"Panama Tariff",
"Honduras Tariff",
"Korea Tariff",
"European Union Tariff",
"Ukraine Tariff",
"Comprehensive and Progressive Trans-Pacific Partnership Tariff",
"United Kingdom Tariff"
		};


		IEnumerable<string> ExpectedTariffList => new string[]
		{
			"8701101000",
"8701109000",
"8701200011",
"8701200012",
"8701200020",
"8701300011",
"8701300012",
"8701300013",
"8701300014",
"8701300015",
"8701300020",
"8701300090",
"8701901000",
"8701909010",
"8701909021",
"8701909022",
"8701909023",
"8701909024",
"8701909025",
"8701909026",
"8701909031",
"8701909032",
"8701909033",
"8701909034",
"8701909035",
"8701909040",
"8701909050",
"8701909081",
"8701909082",
"8701909083",
"8701909090",
"8702101000",
"8702102000",
"8702901000",
"8702902000",
"8703101000",
"8703109011",
"8703109019",
"8703109091",
"8703109099",
"8703211000",
"8703219000",
"8703220010",
"8703220020",
"8703220091",
"8703220092",
"8703220093",
"8703220094",
"8703220095",
"8703230011",
"8703230012",
"8703230020",
"8703230030",
"8703230041",
"8703230042",
"8703230043",
"8703230044",
"8703230045",
"8703230051",
"8703230052",
"8703230053",
"8703230054",
"8703230062",
"8703230063",
"8703230064",
"8703240010",
"8703240021",
"8703240022",
"8703240030",
"8703240040",
"8703240054",
"8703240055",
"8703240061",
"8703240062",
"8703240063",
"8703240064",
"8703240071",
"8703240072",
"8703240073",
"8703240074",
"8703310000",
"8703320011",
"8703320012",
"8703320020",
"8703320092",
"8703320093",
"8703320094",
"8703320095",
"8703330010",
"8703330021",
"8703330022",
"8703330030",
"8703330092",
"8703330093",
"8703330094",
"8703330095",
"8703900010",
"8703900090",
"8704100010",
"8704100021",
"8704100022",
"8704100023",
"8704100024",
"8704100090",
"8704210010",
"8704210020",
"8704210030",
"8704220010",
"8704220020",
"8704220030",
"8704220040",
"8704230000",
"8704310010",
"8704310020",
"8704320010",
"8704320020",
"8704320030",
"8704320040",
"8704320050",
"8704900000",
"8705101010",
"8705101090",
"8705109011",
"8705109019",
"8705109091",
"8705109099",
"8705200010",
"8705200020",
"8705200090",
"8705300000",
"8705401000",
"8705409000",
"8705901000",
"8705909010",
"8705909090",
"8706001000",
"8706002010",
"8706002020",
"8706002030",
"8706009010",
"8706009020",
"8706009030",
"8706009040",
"8706009090",
"8709111000",
"8709119010",
"8709119090",
"8709191000",
"8709199000",
"8711100000",
"8711200000",
"8711300000",
"8711400000",
"8711500000",
"8711900000",
"8716100011",
"8716100012",
"8716100021",
"8716100022",
"8716100023",
"8716100024",
"8716100031",
"8716100032",
"8716100090",
"8716201010",
"8716201020",
"8716201030",
"8716209000",
"8716391000",
"8716392011",
"8716392019",
"8716392020",
"8716393010",
"8716393020",
"8716393030",
"8716393090",
"8716393091",
"8716393099",
"8716399011",
"8716399019",
"8716399091",
"8716399099",
"8716400000",
"8716801000",
"8716802010",
"8716802090",
"8802110012",
"8802110013",
"8802110029",
"8802120019",
"8802120029",
"8802200090",
"8802300013",
"8802300014",
"8802300015",
"8802300019",
"8802300029",
"8802400014",
"8802400015",
"8802400019",
"8802400029",
"8802601000",
"8802609000",
"8901100000",
"8901200000",
"8901300000",
"8901901000",
"8901909000",
"8902001000",
"8902002000",
"8903100000",
"8903910011",
"8903910012",
"8903910021",
"8903910022",
"8903910023",
"8903910024",
"8903920011",
"8903920012",
"8903920021",
"8903920022",
"8903920023",
"8903920024",
"8903920031",
"8903920032",
"8903920090",
"8903999011",
"8903999012",
"8903999013",
"8903999014",
"8903999019",
"8903999020",
"8904000000",
"8905100000",
"8905201000",
"8905202000",
"8905901000",
"8905909000",
"8906901100",
"8906901900",
"8906909000"
		};
	}
}
