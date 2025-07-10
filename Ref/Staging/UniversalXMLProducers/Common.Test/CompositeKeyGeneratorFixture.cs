using System;
using System.Globalization;
using System.Linq;
using CargoWise.RefDbRepo.UniversalXMLProducers.Common.CompositeKey;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.UniversalXMLProducers.Common.Test
{
	[TestFixture]
	public class CompositeKeyGeneratorFixture
	{
		[Test]
		public void GenerateReturnsResults()
		{
			var root = new CompositeKeyNode("root", "root", startDate, endDate, CompositeKeyNodeType.PlaceHolder, -1, null);
			var section = new CompositeKeyNode("01", "Live Animals; Animal Products", startDate, endDate, CompositeKeyNodeType.NomenclatureGroup, 0, root);
			var chapter = new CompositeKeyNode("01", "Live Animals;", startDate, endDate, CompositeKeyNodeType.NomenclatureGroup, 1, section);
			var subchapter = new CompositeKeyNode("00", "Live Animals;", startDate, endDate, CompositeKeyNodeType.PlaceHolder, 2, chapter);

			var heading1 = new CompositeKeyNode("0101", "Live horses, asses, mules and hinnies:", startDate, endDate, CompositeKeyNodeType.NomenclatureGroup, 3, subchapter);
			var subheading1 = new CompositeKeyNode("01012", "Horses:", startDate, endDate, CompositeKeyNodeType.NomenclatureGroup, 4, heading1);
			_ = new CompositeKeyNode("010121", "Pure-bred breeding animals", startDate, endDate, CompositeKeyNodeType.Tariff, 5, subheading1);
			_ = new CompositeKeyNode("010129", "Other", startDate, endDate, CompositeKeyNodeType.Tariff, 5, subheading1);
			_ = new CompositeKeyNode("010130", "Asses", startDate, endDate, CompositeKeyNodeType.Tariff, 4, heading1);
			_ = new CompositeKeyNode("010190", "Other", startDate, endDate, CompositeKeyNodeType.Tariff, 4, heading1);

			var heading2 = new CompositeKeyNode("0102", "Live bovine animals:", startDate, endDate, CompositeKeyNodeType.NomenclatureGroup, 3, subchapter);
			var subheading2 = new CompositeKeyNode("01022", "Cattle:", startDate, endDate, CompositeKeyNodeType.NomenclatureGroup, 4, heading2);
			_ = new CompositeKeyNode("010221", "Pure-bred breeding animals", startDate, endDate, CompositeKeyNodeType.Tariff, 5, subheading2);
			_ = new CompositeKeyNode("010229", "Other", startDate, endDate, CompositeKeyNodeType.Tariff, 5, subheading2);

			var subheading3 = new CompositeKeyNode("01023", "Buffalo:", startDate, endDate, CompositeKeyNodeType.NomenclatureGroup, 4, heading2);
			_ = new CompositeKeyNode("010231", "Pure-bred breeding animals", startDate, endDate, CompositeKeyNodeType.Tariff, 5, subheading3);
			_ = new CompositeKeyNode("010239", "Other", startDate, endDate, CompositeKeyNodeType.Tariff, 5, subheading3);
			_ = new CompositeKeyNode("010290", "Other", startDate, endDate, CompositeKeyNodeType.Tariff, 4, heading2);

			var generator = new CompositeKeyGenerator();
			var result = generator.GenerateCompositeKeys(root);
			Assert.That(result, Is.Not.Null);

			Assert.That(result.NomenclatureGroups, Is.Not.Null);
			Assert.That(result.NomenclatureGroups, Has.Count.EqualTo(7));

			Assert.That(result.Tariffs, Is.Not.Null);
			Assert.That(result.Tariffs, Has.Count.EqualTo(9));
		}

		[Test]
		public void GenerateReturnsSectionCompositeKey()
		{
			var root = new CompositeKeyNode("root", "root", startDate, endDate, CompositeKeyNodeType.PlaceHolder, -1, null);
			_ = new CompositeKeyNode("01", "Live Animals; Animal Products", startDate, endDate, CompositeKeyNodeType.NomenclatureGroup, 0, root);

			var generator = new CompositeKeyGenerator();

			var result = generator.GenerateCompositeKeys(root);
			Assert.That(result, Is.Not.Null);

			Assert.That(result.NomenclatureGroups, Is.Not.Null);
			Assert.That(result.NomenclatureGroups, Has.Count.EqualTo(1));
			Assert.That(result.NomenclatureGroups.First().ZZ5_CompositeKey, Is.EqualTo("01"));
		}

		[Test]
		public void GenerateReturnsChapterCompositeKey()
		{
			var root = new CompositeKeyNode("root", "root", startDate, endDate, CompositeKeyNodeType.PlaceHolder, -1, null);
			var section = new CompositeKeyNode("01", "Live Animals; Animal Products", startDate, endDate, CompositeKeyNodeType.NomenclatureGroup, 0, root);
			_ = new CompositeKeyNode("01", "Live Animals;", startDate, endDate, CompositeKeyNodeType.NomenclatureGroup, 1, section);
			_ = new CompositeKeyNode("02", "MEAT AND EDIBLE MEAT OFFAL", startDate, endDate, CompositeKeyNodeType.NomenclatureGroup, 1, section);

			var generator = new CompositeKeyGenerator();

			var result = generator.GenerateCompositeKeys(root);
			Assert.That(result, Is.Not.Null);

			Assert.That(result.NomenclatureGroups, Is.Not.Null);
			Assert.That(result.NomenclatureGroups, Has.Count.EqualTo(3));
			Assert.Multiple(() =>
			{
				Assert.That(result.NomenclatureGroups.Skip(1).First().ZZ5_CompositeKey, Is.EqualTo("01.01"));
				Assert.That(result.NomenclatureGroups.Skip(2).First().ZZ5_CompositeKey, Is.EqualTo("01.02"));
			});
		}

		[Test]
		public void GenerateReturnsSubChapterCompositeKey()
		{
			var root = new CompositeKeyNode("root", "root", startDate, endDate, CompositeKeyNodeType.PlaceHolder, -1, null);
			var section = new CompositeKeyNode("06", "MINERAL PRODUCTS", startDate, endDate, CompositeKeyNodeType.NomenclatureGroup, 0, root);
			var chapter = new CompositeKeyNode("28", "INORGANIC CHEMICALS; ORGANIC OR INORGANIC COMPOUNDS OF PRECIOUS METALS, OF RARE-EARTH METALS, OF RADIOACTIVE ELEMENTS OR OF ISOTOPES", startDate, endDate, CompositeKeyNodeType.NomenclatureGroup, 1, section);
			_ = new CompositeKeyNode("01", "CHEMICAL ELEMENTS", startDate, endDate, CompositeKeyNodeType.NomenclatureGroup, 2, chapter);

			var generator = new CompositeKeyGenerator();

			var result = generator.GenerateCompositeKeys(root);
			Assert.That(result, Is.Not.Null);

			Assert.That(result.NomenclatureGroups, Is.Not.Null);
			Assert.That(result.NomenclatureGroups, Has.Count.EqualTo(3));
			Assert.Multiple(() =>
			{
				Assert.That(result.NomenclatureGroups.First().ZZ5_CompositeKey, Is.EqualTo("06"));
				Assert.That(result.NomenclatureGroups.Skip(1).First().ZZ5_CompositeKey, Is.EqualTo("06.28"));
				Assert.That(result.NomenclatureGroups.Skip(2).First().ZZ5_CompositeKey, Is.EqualTo("06.28.01"));
			});
		}

		[Test]
		public void GenerateReturnsHeadingCompositeKey_WhenSubChapterExists()
		{
			var root = new CompositeKeyNode("root", "root", startDate, endDate, CompositeKeyNodeType.PlaceHolder, -1, null);
			var section = new CompositeKeyNode("06", "MINERAL PRODUCTS", startDate, endDate, CompositeKeyNodeType.NomenclatureGroup, 0, root);
			var chapter = new CompositeKeyNode("28", "INORGANIC CHEMICALS; ORGANIC OR INORGANIC COMPOUNDS OF PRECIOUS METALS, OF RARE-EARTH METALS, OF RADIOACTIVE ELEMENTS OR OF ISOTOPES", startDate, endDate, CompositeKeyNodeType.NomenclatureGroup, 1, section);
			var subchapter = new CompositeKeyNode("01", "CHEMICAL ELEMENTS", startDate, endDate, CompositeKeyNodeType.NomenclatureGroup, 2, chapter);
			_ = new CompositeKeyNode("2801", "Fluorine, chlorine, bromine and iodine:", startDate, endDate, CompositeKeyNodeType.NomenclatureGroup, 3, subchapter);

			var generator = new CompositeKeyGenerator();

			var result = generator.GenerateCompositeKeys(root);
			Assert.That(result, Is.Not.Null);

			Assert.That(result.NomenclatureGroups, Is.Not.Null);
			Assert.That(result.NomenclatureGroups, Has.Count.EqualTo(4));
			Assert.Multiple(() =>
			{
				Assert.That(result.NomenclatureGroups.First().ZZ5_CompositeKey, Is.EqualTo("06"));
				Assert.That(result.NomenclatureGroups.Skip(1).First().ZZ5_CompositeKey, Is.EqualTo("06.28"));
				Assert.That(result.NomenclatureGroups.Skip(2).First().ZZ5_CompositeKey, Is.EqualTo("06.28.01"));
				Assert.That(result.NomenclatureGroups.Skip(3).First().ZZ5_CompositeKey, Is.EqualTo("06.28.01.01"));
			});
		}

		[Test]
		public void GenerateReturnsHeadingCompositeKey_WhenSubChapterDoesNotExists()
		{
			var root = new CompositeKeyNode("root", "root", startDate, endDate, CompositeKeyNodeType.PlaceHolder, -1, null);
			var section = new CompositeKeyNode("01", "Live Animals; Animal Products", startDate, endDate, CompositeKeyNodeType.NomenclatureGroup, 0, root);
			var chapter = new CompositeKeyNode("01", "Live Animals;", startDate, endDate, CompositeKeyNodeType.NomenclatureGroup, 1, section);
			var subchapter = new CompositeKeyNode("00", "Live Animals;", startDate, endDate, CompositeKeyNodeType.PlaceHolder, 2, chapter);
			_ = new CompositeKeyNode("0101", "Live horses, asses, mules and hinnies:", startDate, endDate, CompositeKeyNodeType.NomenclatureGroup, 3, subchapter);

			var generator = new CompositeKeyGenerator();

			var result = generator.GenerateCompositeKeys(root);
			Assert.That(result, Is.Not.Null);

			Assert.That(result.NomenclatureGroups, Is.Not.Null);
			Assert.That(result.NomenclatureGroups, Has.Count.EqualTo(3));
			Assert.That(result.NomenclatureGroups.Skip(2).First().ZZ5_CompositeKey, Is.EqualTo("01.01..01"));
		}

		[Test]
		public void GenerateReturnsSubHeadingCompositeKey()
		{
			var root = new CompositeKeyNode("root", "root", startDate, endDate, CompositeKeyNodeType.PlaceHolder, -1, null);
			var section = new CompositeKeyNode("01", "Live Animals; Animal Products", startDate, endDate, CompositeKeyNodeType.NomenclatureGroup, 0, root);
			var chapter = new CompositeKeyNode("01", "Live Animals;", startDate, endDate, CompositeKeyNodeType.NomenclatureGroup, 1, section);
			var subchapter = new CompositeKeyNode("00", "Live Animals;", startDate, endDate, CompositeKeyNodeType.PlaceHolder, 2, chapter);
			var heading = new CompositeKeyNode("0101", "Live horses, asses, mules and hinnies:", startDate, endDate, CompositeKeyNodeType.NomenclatureGroup, 3, subchapter);
			_ = new CompositeKeyNode("01012", "Horses:", startDate, endDate, CompositeKeyNodeType.NomenclatureGroup, 4, heading);
			_ = new CompositeKeyNode("010134", "New Horses:", startDate, endDate, CompositeKeyNodeType.NomenclatureGroup, 4, heading);

			var generator = new CompositeKeyGenerator();

			var result = generator.GenerateCompositeKeys(root);
			Assert.That(result, Is.Not.Null);

			Assert.That(result.NomenclatureGroups, Is.Not.Null);
			Assert.That(result.NomenclatureGroups, Has.Count.EqualTo(5));
			Assert.Multiple(() =>
			{
				Assert.That(result.NomenclatureGroups.Skip(3).First().ZZ5_CompositeKey, Is.EqualTo("01.01..01.2"));
				Assert.That(result.NomenclatureGroups.Skip(4).First().ZZ5_CompositeKey, Is.EqualTo("01.01..01.3.4"));
			});
		}

		[Test]
		public void GenerateHandlesMoreThan10ChildrenCases()
		{
			var root = new CompositeKeyNode("root", "root", startDate, endDate, CompositeKeyNodeType.PlaceHolder, -1, null);
			var section = new CompositeKeyNode("01", "Live Animals; Animal Products", startDate, endDate, CompositeKeyNodeType.NomenclatureGroup, 0, root);
			var chapter = new CompositeKeyNode("01", "Live Animals;", startDate, endDate, CompositeKeyNodeType.NomenclatureGroup, 1, section);
			var subchapter = new CompositeKeyNode("00", "Live Animals;", startDate, endDate, CompositeKeyNodeType.PlaceHolder, 2, chapter);
			var heading = new CompositeKeyNode("0101", "Live horses, asses, mules and hinnies:", startDate, endDate, CompositeKeyNodeType.NomenclatureGroup, 3, subchapter);
			var subheading = new CompositeKeyNode("010134", "New Horses:", startDate, endDate, CompositeKeyNodeType.NomenclatureGroup, 4, heading);

			for (var counter = 1; counter < 15; counter++)
			{
				var counterValue = counter.ToString(CultureInfo.InvariantCulture);
				if (counter < 10)
				{
					counterValue = $"0{counterValue}";
				}
				_ = new CompositeKeyNode($"010134{counterValue}", $"description {counterValue}", startDate, endDate, CompositeKeyNodeType.Tariff, 5, subheading);
			}

			var generator = new CompositeKeyGenerator();

			var result = generator.GenerateCompositeKeys(root);
			Assert.That(result, Is.Not.Null);

			Assert.That(result.Tariffs, Is.Not.Null);
			Assert.That(result.Tariffs, Has.Count.EqualTo(14));

			for (var counter = 1; counter < 15; counter++)
			{
				var lastBit = counter * 10;
				var nodeValue = lastBit.ToString(CultureInfo.InvariantCulture);
				if (counter < 10)
				{
					nodeValue = $"0{lastBit}";
				}

				Assert.That(result.Tariffs.Skip(counter - 1).First().ZZ1_CompositeKeyOnZZ5, Is.EqualTo($"01.01..01.3.4.{nodeValue}"));
			}
		}

		[Test]
		public void GenerateReturnsDeepHierarchyCompositeKey()
		{
			var root = new CompositeKeyNode("root", "root", startDate, endDate, CompositeKeyNodeType.PlaceHolder, -1, null);
			var section = new CompositeKeyNode("04", "	PREPARED FOODSTUFFS; BEVERAGES, SPIRITS AND VINEGAR; TOBACCO AND MANUFACTURED TOBACCO SUBSTITUTES", startDate, endDate, CompositeKeyNodeType.NomenclatureGroup, 0, root);
			var chapter = new CompositeKeyNode("20", "	PREPARATIONS OF VEGETABLES, FRUIT, NUTS OR OTHER PARTS OF PLANTS", startDate, endDate, CompositeKeyNodeType.NomenclatureGroup, 1, section);
			var subchapter = new CompositeKeyNode("00", "	PREPARATIONS OF VEGETABLES, FRUIT, NUTS OR OTHER PARTS OF PLANTS", startDate, endDate, CompositeKeyNodeType.PlaceHolder, 2, chapter);
			var heading = new CompositeKeyNode("2008", "Fruit, nuts and other edible parts of plants, otherwise prepared or preserved, whether or not containing added sugar or other sweetening matter or spirit, not elsewhere specified or included", startDate, endDate, CompositeKeyNodeType.NomenclatureGroup, 3, subchapter);
			var subheading = new CompositeKeyNode("200897", "Mixtures :", startDate, endDate, CompositeKeyNodeType.NomenclatureGroup, 4, heading);

			var placeholder1 = new CompositeKeyNode("2008971200", "Other", startDate, endDate, CompositeKeyNodeType.PlaceHolder, 5, subheading);
			var placeholder2 = new CompositeKeyNode("2008971200", "Containing added spirit", startDate, endDate, CompositeKeyNodeType.PlaceHolder, 6, placeholder1);
			var placeholder3 = new CompositeKeyNode("2008971200", "With a sugar content exceeding 9|% by weight", startDate, endDate, CompositeKeyNodeType.PlaceHolder, 7, placeholder2);
			var placeholder4 = new CompositeKeyNode("2008971200", "Of an actual alcoholic strength by mass not exceeding 11,85|%|mas", startDate, endDate, CompositeKeyNodeType.PlaceHolder, 8, placeholder3);

			var nomenclatureGroup = new CompositeKeyNode("2008971200", "Of tropical fruit (including mixtures containing by weight 50|% or more of tropical nuts and tropical fruit)", startDate, endDate, CompositeKeyNodeType.NomenclatureGroup, 9, placeholder4);
			_ = new CompositeKeyNode("2008971211", "Of tropical fruit (including mixtures containing by weight 50|% or more of tropical nuts and tropical fruit)", startDate, endDate, CompositeKeyNodeType.Tariff, 10, nomenclatureGroup);

			var generator = new CompositeKeyGenerator();

			var result = generator.GenerateCompositeKeys(root);
			Assert.That(result, Is.Not.Null);

			Assert.That(result.NomenclatureGroups, Is.Not.Null);
			Assert.That(result.NomenclatureGroups, Has.Count.EqualTo(5));
			Assert.That(result.NomenclatureGroups.Skip(4).First().ZZ5_CompositeKey, Is.EqualTo("04.20..08.9.7.10.10.10.10.10"));

			Assert.That(result.Tariffs, Is.Not.Null);
			Assert.That(result.Tariffs, Has.Count.EqualTo(1));
			Assert.That(result.Tariffs.First().ZZ1_CompositeKeyOnZZ5, Is.EqualTo("04.20..08.9.7.10.10.10.10.10.10"));
		}

		[Test]
		public void GenerateEightDigitNomenclatureCompositeKey()
		{
			var root = new CompositeKeyNode("root", "root", startDate, endDate, CompositeKeyNodeType.PlaceHolder, -1, null);

			_ = new CompositeKeyNode("01", "ANIMAL PRODUCTS", startDate, endDate, CompositeKeyNodeType.NomenclatureGroup, 0, root);
			var section = new CompositeKeyNode("02", "VEGETABLE PRODUCTS", startDate, endDate, CompositeKeyNodeType.NomenclatureGroup, 0, root);

			var chapter = new CompositeKeyNode("08", "EDIBLE FRUITS AND NUTS", startDate, endDate, CompositeKeyNodeType.NomenclatureGroup, 1, section);
			var subchapter = new CompositeKeyNode("00", "Grapes, fresh or dried", startDate, endDate, CompositeKeyNodeType.PlaceHolder, 2, chapter);
			var subsubchapter = new CompositeKeyNode("0806", "Grapes, fresh or dried", startDate, endDate, CompositeKeyNodeType.NomenclatureGroup, 3, subchapter);

			var freshGrapes = new CompositeKeyNode("080610", "Fresh", startDate, endDate, CompositeKeyNodeType.Tariff, 4, subsubchapter);
			var tableGrapes = new CompositeKeyNode("08061010", "Table Grapes", startDate, endDate, CompositeKeyNodeType.NomenclatureGroup, 5, freshGrapes);
			_ = new CompositeKeyNode("08061010", "Table Grapes", startDate, endDate, CompositeKeyNodeType.Tariff, 6, tableGrapes);
			var freshEmperorGrapesTariff = new CompositeKeyNode("0806101005", "Of the variety Emperor", startDate, endDate, CompositeKeyNodeType.Tariff, 6, tableGrapes);
			var freshOtherGrapesTariff = new CompositeKeyNode("0806101090", "Other", startDate, endDate, CompositeKeyNodeType.Tariff, 6, tableGrapes);
			_ = new CompositeKeyNode("08061090", "Other", startDate, endDate, CompositeKeyNodeType.NomenclatureGroup, 5, freshGrapes);

			var driedGrapes = new CompositeKeyNode("080620", "Dried", startDate, endDate, CompositeKeyNodeType.Tariff, 4, subsubchapter);
			var currantsNomenclature = new CompositeKeyNode("08062010", "Currants", startDate, endDate, CompositeKeyNodeType.NomenclatureGroup, 5, driedGrapes);
			var currantsTariff = new CompositeKeyNode("08062010", "Currants", startDate, endDate, CompositeKeyNodeType.Tariff, 6, currantsNomenclature);

			var sultanasNomenclature = new CompositeKeyNode("08062030", "Sultanas", startDate, endDate, CompositeKeyNodeType.NomenclatureGroup, 5, driedGrapes);
			var sultanasTariff = new CompositeKeyNode("08062030", "Sultanas", startDate, endDate, CompositeKeyNodeType.Tariff, 6, sultanasNomenclature);
			var sultanasContainerTariff = new CompositeKeyNode("0806203010", "In immediate containers", startDate, endDate, CompositeKeyNodeType.Tariff, 6, sultanasNomenclature);
			var sultanasOtherTariff = new CompositeKeyNode("0806203090", "Other", startDate, endDate, CompositeKeyNodeType.Tariff, 6, sultanasNomenclature);

			_ = new CompositeKeyNode("08062090", "Other", startDate, endDate, CompositeKeyNodeType.NomenclatureGroup, 5, driedGrapes);

			var generator = new CompositeKeyGenerator();
			_ = generator.GenerateCompositeKeys(root);

			Assert.Multiple(() =>
			{
				Assert.That(freshGrapes.CompositeKey, Is.EqualTo("02.08..06.1"));
				Assert.That(freshEmperorGrapesTariff.CompositeKey, Is.EqualTo("02.08..06.1.10.20"));
				Assert.That(freshOtherGrapesTariff.CompositeKey, Is.EqualTo("02.08..06.1.10.30"));
				Assert.That(driedGrapes.CompositeKey, Is.EqualTo("02.08..06.2"));
				Assert.That(currantsNomenclature.CompositeKey, Is.EqualTo("02.08..06.2.10"));
				Assert.That(currantsTariff.CompositeKey, Is.EqualTo("02.08..06.2.10.10"));
				Assert.That(sultanasNomenclature.CompositeKey, Is.EqualTo("02.08..06.2.20"));
				Assert.That(sultanasTariff.CompositeKey, Is.EqualTo("02.08..06.2.20.10"));
				Assert.That(sultanasContainerTariff.CompositeKey, Is.EqualTo("02.08..06.2.20.20"));
				Assert.That(sultanasOtherTariff.CompositeKey, Is.EqualTo("02.08..06.2.20.30"));
			});
		}

		[Test]
		public void GenerateCompositeKeyUpdatesCompositeKeyNodes()
		{
			var root = new CompositeKeyNode("root", "root", startDate, endDate, CompositeKeyNodeType.PlaceHolder, -1, null);

			var section = new CompositeKeyNode("01", "ANIMAL PRODUCTS", startDate, endDate, CompositeKeyNodeType.NomenclatureGroup, 0, root);

			var chapter = new CompositeKeyNode("01", "Live animals", startDate, endDate, CompositeKeyNodeType.NomenclatureGroup, 1, section);
			var subchapter = new CompositeKeyNode("00", "Other", startDate, endDate, CompositeKeyNodeType.PlaceHolder, 2, chapter);
			var subsubchapter = new CompositeKeyNode("0106", "Other animals", startDate, endDate, CompositeKeyNodeType.NomenclatureGroup, 3, subchapter);

			var otherAnimalsTariff = new CompositeKeyNode("010690", "Other", startDate, endDate, CompositeKeyNodeType.Tariff, 4, subsubchapter);
			var frogsNomenclature = new CompositeKeyNode("0106900010", "Frogs", startDate, endDate, CompositeKeyNodeType.NomenclatureGroup, 5, otherAnimalsTariff);
			var frogsEdibleTariff = new CompositeKeyNode("0106900010", "Fit for human consumption", startDate, endDate, CompositeKeyNodeType.Tariff, 6, frogsNomenclature);
			var frogsOtherTariff = new CompositeKeyNode("0106900019", "Other", startDate, endDate, CompositeKeyNodeType.Tariff, 6, frogsNomenclature);

			Assert.Multiple(() =>
			{
				Assert.That(otherAnimalsTariff.CompositeKey, Is.Null);
				Assert.That(frogsNomenclature.CompositeKey, Is.Null);
				Assert.That(frogsEdibleTariff.CompositeKey, Is.Null);
				Assert.That(frogsOtherTariff.CompositeKey, Is.Null);
			});

			var generator = new CompositeKeyGenerator();
			var result = generator.GenerateCompositeKeys(root);

			Assert.Multiple(() =>
			{
				Assert.That(otherAnimalsTariff.CompositeKey, Is.EqualTo("01.01..06.9"));
				Assert.That(frogsNomenclature.CompositeKey, Is.EqualTo("01.01..06.9.10"));
				Assert.That(frogsEdibleTariff.CompositeKey, Is.EqualTo("01.01..06.9.10.10"));
				Assert.That(frogsOtherTariff.CompositeKey, Is.EqualTo("01.01..06.9.10.20"));
			});

			Assert.That(result, Is.Not.Null);

			Assert.That(result.NomenclatureGroups, Is.Not.Null);
			Assert.That(result.NomenclatureGroups, Has.Count.EqualTo(4));
			Assert.That(result.NomenclatureGroups.Skip(3).First().ZZ5_CompositeKey, Is.EqualTo(frogsNomenclature.CompositeKey));

			Assert.That(result.Tariffs, Is.Not.Null);
			Assert.That(result.Tariffs, Has.Count.EqualTo(3));
			Assert.That(result.Tariffs.Skip(1).First().ZZ1_CompositeKeyOnZZ5, Is.EqualTo(frogsEdibleTariff.CompositeKey));
		}

		readonly DateTime startDate = new DateTime(2018, 05, 25, 00, 00, 00);
		readonly DateTime endDate = new DateTime(2079, 06, 06, 23, 59, 00);
	}
}
