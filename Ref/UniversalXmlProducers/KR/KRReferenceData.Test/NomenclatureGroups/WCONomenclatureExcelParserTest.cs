using System;
using System.Collections.Generic;
using System.IO;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;
using CargoWise.RefDbRepo.KRReferenceData.Business;
using CargoWise.RefDbRepo.KRReferenceData.Services;
using NUnit.Framework;
using File = System.IO.File;

namespace CargoWise.RefDbRepo.KRReferenceData.Test
{
	[TestFixture]
	public class WCONomenclatureExcelParserTest
	{
		[TestCase(01, 01, 2023)]
		public void TestNomenclatures(int day, int month, int year)
		{
			var publicationDate = new DateTime(year, month, day);
			var inputConfigPath = ApplicationConfig.WCONomenclatureConfigFileInputPath;
			var outputFile = Path.Combine(TestHelper.BaseTestFilePath, @"WCOCopiedNomenclatures\Output\KRRefCusNomenclature_Update4To6Only.xml");
			var expectedXML = TestHelper.ReadManifestResourceContent("CargoWise.RefDbRepo.KRReferenceData.Test.TestFiles.WCOCopiedNomenclatures.Output.KRRefCusNomenclature_Update4To6Only.xml");
			if (File.Exists(outputFile))
			{
				File.Delete(outputFile);
			}
			var nomenclatures = PrepareNomenclaturesData();
			var inputDataPathXls = Path.Combine(TestHelper.BaseTestFilePath, @"WCOCopiedNomenclatures\Input\2017Edition\HSKSample_2021.xlsx");
			new WCONomenclatureExcelParserForTest(inputConfigPath, inputDataPathXls, nomenclatures).ConvertToXMLFile(outputFile, publicationDate);
			Assert.That(File.ReadAllText(outputFile), Is.EqualTo(expectedXML));
		}

		List<RefCusNomenclatureGroup> PrepareNomenclaturesData()
		{
			var result = new List<RefCusNomenclatureGroup>();
			foreach (var item in nomenclatureInputData)
			{
				var nomenclature = new RefCusNomenclatureGroup
				{
					ZZ5_Value = item.Item1,
					ZZ5_Description = item.Item2,
					ZZ5_CompositeKey = item.Item3,
					ZZ5_StartDate = new DateTime(2022, 1, 1),
					ZZ5_EndDate = new DateTime(2079, 6, 6),
					ZZ5_ZZ9_NKNomenclatureGroupType = "KR"
				};
				result.Add(nomenclature);
			}
			return result;
		}

		List<Tuple<string, string, string>> nomenclatureInputData = new List<Tuple<string, string, string>>() {
			new Tuple<string, string, string>("0101", "Live horses, asses, mules and hinnies", "01.01..01"),
			new Tuple<string, string, string>("01012", "Horses", "01.01..01.2"),
			new Tuple<string, string, string>("010121", "Pure-bred breeding animals", "01.01..01.2.1"),
			new Tuple<string, string, string>("010231", "Pure-bred breeding animals", "01.01..02.3.1"),
			new Tuple<string, string, string>("020110", "Carcasses and half-carcasses", "01.02..01.1"),
			new Tuple<string, string, string>("0205", "Meat of horses, asses, mules or hinnies, fresh, chilled or frozen", "01.02..05"),
			new Tuple<string, string, string>("030323", "Tilapias (Oreochromis spp.)", "01.03..03.2.3"),
			new Tuple<string, string, string>("030760", "Snails, other than sea snails", "01.03..07.6"),
			new Tuple<string, string, string>("0409", "Natural honey","01.04..09"),
			new Tuple<string, string, string>("0510", "Ambergris, castoreum, civet and musk; cantharides; bile, whether or not dried; glands and other animal products used in the preparation of pharmaceutical products, fresh, chilled, frozen or otherwise provisionally preserved", "01.05..10"),
			new Tuple<string, string, string>("071410", "Manioc (cassava)", "02.07..14.1"),
			new Tuple<string, string, string>("1520", "Glycerol, crude; glycerol waters and glycerol lyes", "03.15..20"),
			new Tuple<string, string, string>("400610", "Camel-back\" strips for retreading rubber tyres", "07.40..06.1"),
			new Tuple<string, string, string>("690210", "Containing by weight, singly or together, more than 50 % of the elements Mg, Ca or Cr, expressed as MgO, CaO or Cr2O3", "13.69.01.02.1"),
			new Tuple<string, string, string>("690290", "Other", "13.69.01.02.9"),
			new Tuple<string, string, string>("6906", "Ceramic pipes, conduits, guttering and pipe fittings", "13.69.02.06"),
			new Tuple<string, string, string>("7006", "Glass of heading 70.03, 70.04 or 70.05, bent, edge-worked, engraved, drilled, enamelled or otherwise worked, but not framed or fitted with other materials", "13.70..06")
		};

		class WCONomenclatureExcelParserForTest : WCONomenclatureExcelParser
		{
			public WCONomenclatureExcelParserForTest(string configFilePath, string dataFilePath, List<RefCusNomenclatureGroup> nomenclatures) : base(configFilePath, dataFilePath, nomenclatures) { }
		}
	}

	

	
}
