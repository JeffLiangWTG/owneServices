using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using CargoWise.RefDbRepo.UniversalXMLProducers.Common.CompositeKey;
using Moq;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.UniversalXMLProducers.EUNTariffDataProducer.Test
{
	[TestFixture]
	public class CompositeKeyTreeGeneratorFixture
	{
		[Test]
		public void CreateTree()
		{
			var chapterToSectionMapper = new Mock<IChapterToSectionMapper>();
			chapterToSectionMapper.Setup(x => x.GetAllSection()).Returns(new[]
			{
				new Section(1, "LIVE ANIMALS; ANIMAL PRODUCTS"),
				new Section(2, "VEGETABLE PRODUCTS")
			});
			chapterToSectionMapper.Setup(x => x.GetSection(It.IsAny<int>())).Returns(1);

			var compositeKeyTreeGenerator = new CompositeKeyTreeGenerator(chapterToSectionMapper.Object);
			var rootNode = compositeKeyTreeGenerator.GenerateTree(rawRecords);

			Assert.That(rootNode, Is.Not.Null);
			Assert.That(rootNode.Children, Has.Count.EqualTo(2));

			var sectionNode = rootNode.Children.First();
			AssertNode(sectionNode, "01", 1, CompositeKeyNodeType.NomenclatureGroup);

			var sectionNode2 = rootNode.Children.Last();
			AssertNode(sectionNode2, "02", 0, CompositeKeyNodeType.NomenclatureGroup);

			var chapter = sectionNode.Children.First();
			AssertNode(chapter, "01", 1, CompositeKeyNodeType.NomenclatureGroup);

			var subchapter = chapter.Children.First();
			AssertNode(subchapter, "00", 2, CompositeKeyNodeType.PlaceHolder);

			var heading1 = subchapter.Children.First();
			AssertNode(heading1, "0101", 5, CompositeKeyNodeType.NomenclatureGroup);

			var node = heading1.Children.First();
			AssertNode(node, "01012", 0, CompositeKeyNodeType.NomenclatureGroup);

			var heading2 = subchapter.Children.Last();
			AssertNode(heading2, "0102", 3, CompositeKeyNodeType.NomenclatureGroup);

			node = heading2.Children.First();
			AssertNode(node, "01022", 0, CompositeKeyNodeType.NomenclatureGroup);

			var subNode = heading2.Children.Skip(1).First();
			AssertNode(subNode, "010221", 3, CompositeKeyNodeType.NomenclatureGroup);

			var subsubNode = subNode.Children.First();
			AssertNode(subsubNode, "01022110", 1, CompositeKeyNodeType.NomenclatureGroup);

			subsubNode = subNode.Children.First().Children.First();
			AssertNode(subsubNode, "01022110", 0, CompositeKeyNodeType.Tariff);

			subsubNode = subNode.Children.Skip(1).First();
			AssertNode(subsubNode, "01022130", 1, CompositeKeyNodeType.NomenclatureGroup);

			subsubNode = subNode.Children.Skip(1).First().Children.First();
			AssertNode(subsubNode, "01022130", 0, CompositeKeyNodeType.Tariff);

			subsubNode = subNode.Children.Last();
			AssertNode(subsubNode, "01022190", 1, CompositeKeyNodeType.NomenclatureGroup);

			subsubNode = subNode.Children.Last().Children.First();
			AssertNode(subsubNode, "01022190", 0, CompositeKeyNodeType.Tariff);

			subNode = heading2.Children.Last();
			AssertNode(subNode, "010229", 2, CompositeKeyNodeType.NomenclatureGroup);

			subsubNode = subNode.Children.First();
			AssertNode(subsubNode, "01022905", 1, CompositeKeyNodeType.NomenclatureGroup);
			subsubNode = subNode.Children.First().Children.First();
			AssertNode(subsubNode, "01022905", 0, CompositeKeyNodeType.Tariff);
			Assert.That(subNode.Language, Has.Count.EqualTo(1));

			var subsubNode2 = subNode.Children.Last();
			AssertNode(subsubNode2, "01022910", 1, CompositeKeyNodeType.NomenclatureGroup);

			subsubNode2 = subsubNode2.Children.First();
			AssertNode(subsubNode2, "01022910", 6, CompositeKeyNodeType.NomenclatureGroup);
		}

		[TestCase]
		public void CreateTree_TariffAtLevel1()
		{
			var rawRecords = new[] {
				new NomenclatureRecord("9800000000 80", startDate, endDate, 2, 0, "COMPLETE INDUSTRIAL PLANT", new List<(string language, string description)>() { ("EN", "English description") }, false),
				new NomenclatureRecord("9880000000 80", startDate, endDate, 4, 0, "Component parts of complete industrial plant in the framework of external trade (Commission Regulation EC No 113/2010 of 9.02.2010)", new List<(string language, string description)>() { ("EN", "English description") }, false),
				new NomenclatureRecord("9880010000 80", startDate, endDate, 6, 1, "Classified in Chapter 01", new List<(string language, string description)>() { ("EN", "English description") }, true),
				new NomenclatureRecord("9880020000 80", startDate, endDate, 6, 1, "Classified in Chapter 02", new List<(string language, string description)>() { ("EN", "English description") }, true),
			};
			var chapterToSectionMapper = new Mock<IChapterToSectionMapper>();
			chapterToSectionMapper.Setup(x => x.GetAllSection()).Returns(new[]
			{
				new Section(21, "WORKS OF ART, COLLECTORS' PIECES AND ANTIQUES")
			});
			chapterToSectionMapper.Setup(x => x.GetSection(It.IsAny<int>())).Returns(21);

			var compositeKeyTreeGenerator = new CompositeKeyTreeGenerator(chapterToSectionMapper.Object);
			var rootNode = compositeKeyTreeGenerator.GenerateTree(rawRecords);

			Assert.That(rootNode, Is.Not.Null);
			Assert.That(rootNode.Children, Has.Count.EqualTo(1));

			var sectionNode = rootNode.Children.First();
			AssertNode(sectionNode, "21", 1, CompositeKeyNodeType.NomenclatureGroup);

			var chapter = sectionNode.Children.First();
			AssertNode(chapter, "98", 1, CompositeKeyNodeType.NomenclatureGroup);

			var subchapter = chapter.Children.First();
			AssertNode(subchapter, "00", 1, CompositeKeyNodeType.PlaceHolder);

			var heading = subchapter.Children.First();
			AssertNode(heading, "9880", 2, CompositeKeyNodeType.NomenclatureGroup);

			var tariffNode1 = heading.Children.First();
			AssertNode(tariffNode1, "988001", 0, CompositeKeyNodeType.Tariff);

			var tariffNode2 = heading.Children.Last();
			AssertNode(tariffNode2, "988002", 0, CompositeKeyNodeType.Tariff);
		}

		[Test]
		public void EighthHierarchyCreatesNomenclatureRecordAsParentAndTariffAsChild()
		{
			var chapterToSectionMapper = new Mock<IChapterToSectionMapper>();
			chapterToSectionMapper.Setup(x => x.GetAllSection()).Returns(new[]
			{
				new Section(1, "LIVE ANIMALS; ANIMAL PRODUCTS"),
			});
			chapterToSectionMapper.Setup(x => x.GetSection(It.IsAny<int>())).Returns(1);

			var compositeKeyTreeGenerator = new CompositeKeyTreeGenerator(chapterToSectionMapper.Object);
			var rootNode = compositeKeyTreeGenerator.GenerateTree(rawRecords);
			Assert.That(rootNode, Is.Not.Null);

			var sectionNode = rootNode.Children.Last();
			AssertNode(sectionNode, "01", 1, CompositeKeyNodeType.NomenclatureGroup);

			var chapter = sectionNode.Children.First();
			AssertNode(chapter, "01", 1, CompositeKeyNodeType.NomenclatureGroup);

			var subchapter = chapter.Children.First();
			AssertNode(subchapter, "00", 2, CompositeKeyNodeType.PlaceHolder);

			var heading = subchapter.Children.First();
			AssertNode(heading, "0101", 5, CompositeKeyNodeType.NomenclatureGroup);

			var headingChildrenWithChildren = heading.Children.Where(x => x.Children.Any());
			var firstHeadingChildrenWithChildren = headingChildrenWithChildren.First();
			var recordChildren = firstHeadingChildrenWithChildren.Children;

			var compositeKeyExtractor = new CompositeKeyGenerator();
			compositeKeyExtractor.GenerateCompositeKeys(rootNode);

			Assert.That(recordChildren.Count, Is.EqualTo(2));
			var firstChildren = recordChildren.First();
			Assert.That(firstChildren.NodeType, Is.EqualTo(CompositeKeyNodeType.NomenclatureGroup));
			var firstChildrenTariffRecord = firstChildren.Children.FirstOrDefault();
			Assert.That(firstChildrenTariffRecord, Is.Not.Null);
			Assert.That(firstChildrenTariffRecord.NodeType, Is.EqualTo(CompositeKeyNodeType.Tariff));
			Assert.That(firstChildren.CompositeKey, Is.EqualTo("01.01..01.2.9.10"));
			Assert.That(firstChildrenTariffRecord.CompositeKey, Is.EqualTo("01.01..01.2.9.10.10"));

			var secondChildren = recordChildren.Last();
			Assert.That(secondChildren.NodeType, Is.EqualTo(CompositeKeyNodeType.NomenclatureGroup));
			var secondChildrenTariffRecord = secondChildren.Children.FirstOrDefault();
			Assert.That(secondChildrenTariffRecord, Is.Not.Null);
			Assert.That(secondChildrenTariffRecord.NodeType, Is.EqualTo(CompositeKeyNodeType.Tariff));
			Assert.That(secondChildren.CompositeKey, Is.EqualTo("01.01..01.2.9.20"));
			Assert.That(secondChildrenTariffRecord.CompositeKey, Is.EqualTo("01.01..01.2.9.20.10"));
		}

		void AssertNode(ICompositeKeyNode node, string nodeValue, int childCount, CompositeKeyNodeType nodeType)
		{
			Assert.That(node.Value, Is.EqualTo(nodeValue), "Node value");
			Assert.That(node.Children, Has.Count.EqualTo(childCount), "Children count");
			Assert.That(node.NodeType, Is.EqualTo(nodeType), "Node type");
		}

		[Test]
		public void EightDigitNomenclatureCompositeKey()
		{
			IEnumerable<INomenclatureRecord> nomenclatureRecords = new[]
			{
				new NomenclatureRecord("0800000000 80", startDate, endDate, 2, 0, "EDIBLE FRUIT AND NUTS; PEEL OF CITRUS FRUIT OR MELONS", new List<(string language, string description)>() { ("EN", "English description") }, false),
				new NomenclatureRecord("0806000000 80", startDate, endDate, 4, 0, "Grapes, fresh or dried", new List<(string language, string description)>() { ("EN", "English description") }, false),
				new NomenclatureRecord("0806100000 80", startDate, endDate, 6, 1, "Fresh", new List<(string language, string description)>() { ("EN", "English description") }, true),
				new NomenclatureRecord("0806101000 80", startDate, endDate, 8, 2, "Table Grapes", new List<(string language, string description)>() { ("EN", "English description") }, true),
				new NomenclatureRecord("0806101005 80", startDate, endDate, 10, 3, "Of the variety Emperor (Vitis vinifera c.v.), from 1 January to 31 January and from 1 December to 31 December", new List<(string language, string description)>() { ("EN", "English description") }, true),
				new NomenclatureRecord("0806101090 80", startDate, endDate, 10, 3, "Other", new List<(string language, string description)>() { ("EN", "English description") }, true),
				new NomenclatureRecord("0806109000 80", startDate, endDate, 8, 2, "Other", new List<(string language, string description)>() { ("EN", "English description") }, false),
				new NomenclatureRecord("0806200000 80", startDate, endDate, 6, 1, "Dried", new List<(string language, string description)>() { ("EN", "English description") }, false),
				new NomenclatureRecord("0806201000 80", startDate, endDate, 8, 2, "Currants", new List<(string language, string description)>() { ("EN", "English description") }, true),
				new NomenclatureRecord("0806203000 80", startDate, endDate, 8, 2, "Sultanas", new List<(string language, string description)>() { ("EN", "English description") }, true),
				new NomenclatureRecord("0806203010 80", startDate, endDate, 10, 3, "In immediate containers of a net capacity not exceeding|2|kg", new List<(string language, string description)>() { ("EN", "English description") }, true),
				new NomenclatureRecord("0806203090 80", startDate, endDate, 10, 3, "Other", new List<(string language, string description)>() { ("EN", "English description") }, true),
				new NomenclatureRecord("0806209000 80", startDate, endDate, 8, 2, "Other", new List<(string language, string description)>() { ("EN", "English description") }, false),
			};

			var chapterToSectionMapper = new Mock<IChapterToSectionMapper>();
			chapterToSectionMapper.Setup(x => x.GetAllSection()).Returns(new[]
			{
				new Section(1, "LIVE ANIMALS; ANIMAL PRODUCTS"),
				new Section(2, "VEGETABLE PRODUCTS")
			});
			chapterToSectionMapper.Setup(x => x.GetSection(It.IsAny<int>())).Returns(2);

			var compositeKeyTreeGenerator = new CompositeKeyTreeGenerator(chapterToSectionMapper.Object);

			var rootNode = compositeKeyTreeGenerator.GenerateTree(nomenclatureRecords);

			Assert.That(rootNode, Is.Not.Null);
			Assert.That(rootNode.Children, Has.Count.EqualTo(2));

			var sectionNode2 = rootNode.Children.Last();
			AssertNode(sectionNode2, "02", 1, CompositeKeyNodeType.NomenclatureGroup);

			var chapter = sectionNode2.Children.First();
			AssertNode(chapter, "08", 1, CompositeKeyNodeType.NomenclatureGroup);

			var subchapter = chapter.Children.First();
			AssertNode(subchapter, "00", 1, CompositeKeyNodeType.PlaceHolder);

			var heading = subchapter.Children.First();
			AssertNode(heading, "0806", 2, CompositeKeyNodeType.NomenclatureGroup);

			var driedNode = heading.Children.Last();
			AssertNode(driedNode, "080620", 3, CompositeKeyNodeType.NomenclatureGroup);

			var currantsNomenclatureNode = driedNode.Children.ElementAt(0);
			var sultanasNode = driedNode.Children.ElementAt(1);
			var otherNode = driedNode.Children.ElementAt(2);

			Assert.That(currantsNomenclatureNode, Is.Not.Null);
			Assert.That(sultanasNode, Is.Not.Null);
			Assert.That(otherNode, Is.Not.Null);

			var currantsTariffNode = currantsNomenclatureNode.Children.ElementAt(0);

			var compositeKeyExtractor = new CompositeKeyGenerator();
			compositeKeyExtractor.GenerateCompositeKeys(rootNode);

			Assert.That(currantsTariffNode.CompositeKey, Is.EqualTo("02.08..06.2.10.10"));
			Assert.That(sultanasNode.CompositeKey, Is.EqualTo("02.08..06.2.20"));
			Assert.That(otherNode.CompositeKey, Is.EqualTo("02.08..06.2.30"));

			var sultanasChild = sultanasNode.Children.First();
			Assert.That(sultanasChild.CompositeKey, Is.EqualTo("02.08..06.2.20.10"));
		}

		[Test]
		public void RecordsAreTranslatedInAllHierarchyLevels()
		{
			var chapterToSectionMapper = new Mock<IChapterToSectionMapper>();
			chapterToSectionMapper.Setup(x => x.GetAllSection()).Returns(new[]
			{
				new Section(1, "LIVE ANIMALS; ANIMAL PRODUCTS"),
				new Section(2, "VEGETABLE PRODUCTS")
			});
			chapterToSectionMapper.Setup(x => x.GetSection(It.IsAny<int>())).Returns(2);

			var compositeKeyTreeGenerator = new CompositeKeyTreeGenerator(chapterToSectionMapper.Object);

			IEnumerable<INomenclatureRecord> treeRecords = new[]
			{
				new NomenclatureRecord("0100000000 80", startDate, endDate, 2, 0, "LIVE ANIMALS", new List<(string language, string description)>() { ("FR", "French description") }, false),
				new NomenclatureRecord("0101000000 80", startDate, endDate, 4, 0, "Live horses, asses, mules and hinnies", new List<(string language, string description)>() { ("FR", "French description") }, false),
				new NomenclatureRecord("0102000000 80", startDate, endDate, 4, 0, "I. Testing Roman", new List<(string language, string description)>() { ("FR", "French description") }, false),
				new NomenclatureRecord("0101210000 10", startDate, endDate, 6, 1, "Horses", new List<(string language, string description)>() { ("FR", "French description") }, false),
				new NomenclatureRecord("0101210000 80", startDate, endDate, 6, 2, "Pure-bred breeding animals", new List<(string language, string description)>() { ("FR", "French description") }, true),
				new NomenclatureRecord("0101290000 80", startDate, endDate, 6, 2, "Other", new List<(string language, string description)>() { ("FR", "French description") }, false),
				new NomenclatureRecord("0101291000 80", startDate, endDate, 8, 3, "For slaughter", new List<(string language, string description)>() { ("FR", "French description") }, true),
				new NomenclatureRecord("0101299000 80", startDate, endDate, 8, 3, "Other", new List<(string language, string description)>() { ("FR", "French description") }, true),
				new NomenclatureRecord("0101299000 80", startDate, endDate, 10, 4, "Made Up", new List<(string language, string description)>() { ("FR", "French description") }, true),
			};

			var rootNode = compositeKeyTreeGenerator.GenerateTree(treeRecords);
			Assert.That(rootNode, Is.Not.Null);
			var sectionNodes = rootNode.Children;

			foreach (var sectionNode in sectionNodes.Where(x => x.Children.Any()))
			{
				AssertChildrenHasLanguageSet(sectionNode.Children);
			}
		}

		[TestCase]
		public void CreateTree_Tariffs0739()
		{
			var rawRecords = new[] {
				new NomenclatureRecord("3900000000 90", startDate, endDate, 2, 0, "PLASTICS AND ARTICLES THEREOF", new List<(string language, string description)>() { ("EN", "English description") }, false),
				new NomenclatureRecord("3915000000 10", startDate, endDate, 4, 0, "WASTE, PARINGS AND SCRAP", new List<(string language, string description)>() { ("EN", "English description") }, false),
				new NomenclatureRecord("3920000000 80", startDate, endDate, 4, 0, "Other plates, sheets, film, foil and strip", new List<(string language, string description)>() { ("EN", "English description") }, false),
				new NomenclatureRecord("3920910000 10", startDate, endDate, 6, 1, "Of other plastic", new List<(string language, string description)>() { ("EN", "English description") }, false),
				new NomenclatureRecord("3920990000 80", startDate, endDate, 6, 2, "Of other plastic", new List<(string language, string description)>() { ("EN", "English description") }, true),
				new NomenclatureRecord("3920995200 10", startDate, endDate, 8, 3, "Of additional polymerisation", new List<(string language, string description)>() { ("EN", "English description") }, false),
				new NomenclatureRecord("3920995200 80", startDate, endDate, 8, 4, "Poly(vinyl fluoride) sheet", new List<(string language, string description)>() { ("EN", "English description") }, true),
				new NomenclatureRecord("3920995300 80", startDate, endDate, 8, 4, "Ion-exchange membranes", new List<(string language, string description)>() { ("EN", "English description") }, true),
				new NomenclatureRecord("3920995900 80", startDate, endDate, 8, 4, "Plates, sheets, film, foil and strip", new List<(string language, string description)>() { ("EN", "English description") }, true),
				new NomenclatureRecord("3920999000 80", startDate, endDate, 8, 3, "Plates, sheets, film, foil and strip", new List<(string language, string description)>() { ("EN", "English description") }, true),

				new NomenclatureRecord("3920995965 80", startDate, endDate, 10, 6, "Film of a vinyl alcohol copolymer", new List<(string language, string description)>() { ("EN", "English description") }, true),

			};
			var chapterToSectionMapper = new Mock<IChapterToSectionMapper>();
			chapterToSectionMapper.Setup(x => x.GetAllSection()).Returns(new[]
			{
				new Section(7, "PLASTICS AND ARTICLES THEREOF; RUBBER AND ARTICLES THEREOF")
			});
			chapterToSectionMapper.Setup(x => x.GetSection(It.IsAny<int>())).Returns(7);

			var compositeKeyTreeGenerator = new CompositeKeyTreeGenerator(chapterToSectionMapper.Object);
			var rootNode = compositeKeyTreeGenerator.GenerateTree(rawRecords);

			Assert.That(rootNode, Is.Not.Null);

			var compositeKeyExtractor = new CompositeKeyGenerator();
			compositeKeyExtractor.GenerateCompositeKeys(rootNode);

			var treeDescription = GenerateTreeDescription(rootNode);

			var expectedTreeDescription = @"root (PlaceHolder): 
    07 (NomenclatureGroup): 07
        39 (NomenclatureGroup): 07.39
            00 (PlaceHolder): 07.39.
                3915 (NomenclatureGroup): 07.39..15
                3920 (NomenclatureGroup): 07.39..20
                    39209 (NomenclatureGroup): 07.39..20.9
                    392099 (Tariff): 07.39..20.9.9
                        39209952 (NomenclatureGroup): 07.39..20.9.9.10
                            39209952 (NomenclatureGroup): 07.39..20.9.9.10.10
                                39209952 (Tariff): 07.39..20.9.9.10.10.10
                            39209953 (NomenclatureGroup): 07.39..20.9.9.10.20
                                39209953 (Tariff): 07.39..20.9.9.10.20.10
                            39209959 (NomenclatureGroup): 07.39..20.9.9.10.30
                                39209959 (Tariff): 07.39..20.9.9.10.30.10
                                3920995965 (Tariff): 07.39..20.9.9.10.30.20
                    39209990 (NomenclatureGroup): 07.39..20.9.9
                        39209990 (Tariff): 07.39..20.9.9.10";
			Assert.That(treeDescription.Trim(), Is.EqualTo(expectedTreeDescription.Trim()));
		}

		[Test]
		public void GenerateTree_Retry3Times()
		{
			var callCount = 0;
			var mapper = new Mock<IChapterToSectionMapper>();
			mapper.Setup(x => x.GetAllSection()).Returns(Enumerable.Empty<ISection>()).Callback(() => callCount++);
			var generator = new CompositeKeyTreeGenerator(mapper.Object);
			var exception = Assert.Throws<InvalidOperationException>(() => generator.GenerateTree(Enumerable.Empty<NomenclatureRecord>()));
			Assert.That(exception.Message, Is.EqualTo("Can not load the section details."));
			Assert.That(callCount, Is.EqualTo(3));
		}

		string GenerateTreeDescription(ICompositeKeyNode rootNode)
		{
			const string tab = "    ";

			var result = new StringBuilder();
			AddTreeNodeDescription(rootNode, string.Empty, result);

			return result.ToString();

			void AddTreeNodeDescription(ICompositeKeyNode node, string indent, StringBuilder builder)
			{
				builder.AppendLine(CultureInfo.InvariantCulture, $"{indent}{node.Value} ({node.NodeType}): {node.CompositeKey}");
				foreach (var childNode in node.Children)
				{
					AddTreeNodeDescription(childNode, indent + tab, builder);
				}
			}
		}

		void AssertChildrenHasLanguageSet(ICollection<ICompositeKeyNode> children)
		{
			foreach (var child in children)
			{
				Assert.That(child.Language, Is.Not.Null);
				Assert.That(child.Language, Has.Count.EqualTo(1));
				Assert.That(child.Language.Any(x => x.language == "FR"));
				if (child.Children != null && child.Children.Any())
				{
					AssertChildrenHasLanguageSet(child.Children);
				}
			}
		}

		[SetUp]
		protected void Setup()
		{
			rawRecords = new[]
			{
				new NomenclatureRecord("0100000000 80", startDate, endDate, 2, 0, "LIVE ANIMALS", new List<(string language, string description)>() { ("FR", "French description") }, false),
				new NomenclatureRecord("0101000000 80", startDate, endDate, 4, 0, "Live horses, asses, mules and hinnies", new List<(string language, string description)>() { ("FR", "French description") }, false),
				new NomenclatureRecord("0101210000 10", startDate, endDate, 6, 1, "Horses", new List<(string language, string description)>() { ("FR", "French description") }, false),
				new NomenclatureRecord("0101210000 80", startDate, endDate, 6, 2, "Pure-bred breeding animals", new List<(string language, string description)>() { ("FR", "French description") }, true),
				new NomenclatureRecord("0101290000 80", startDate, endDate, 6, 2, "Other", new List<(string language, string description)>() { ("FR", "French description") }, false),
				new NomenclatureRecord("0101291000 80", startDate, endDate, 8, 3, "For slaughter", new List<(string language, string description)>() { ("FR", "French description") }, true),
				new NomenclatureRecord("0101299000 80", startDate, endDate, 8, 3, "Other", new List<(string language, string description)>() { ("FR", "French description") }, true),
				new NomenclatureRecord("0101300000 80", startDate, endDate, 6, 1, "Asses", new List<(string language, string description)>() { ("FR", "French description") }, true),
				new NomenclatureRecord("0101900000 80", startDate, endDate, 6, 1, "Other", new List<(string language, string description)>() { ("FR", "French description") }, true),
				new NomenclatureRecord("0102000000 80", startDate, endDate, 4, 0, "Live bovine animals", new List<(string language, string description)>() { ("FR", "French description") }, false),
				new NomenclatureRecord("0102210000 10", startDate, endDate, 6, 1, "Cattle", new List<(string language, string description)>() { ("FR", "French description") }, false),
				new NomenclatureRecord("0102210000 80", startDate, endDate, 6, 2, "Pure-bred breeding animals", new List<(string language, string description)>() { ("FR", "French description") }, false),
				new NomenclatureRecord("0102211000 80", startDate, endDate, 8, 3, "Heifers(female bovines that have never calved)", new List<(string language, string description)>() { ("FR", "French description") }, true),
				new NomenclatureRecord("0102213000 80", startDate, endDate, 8, 3, "Cows", new List<(string language, string description)>() { ("FR", "French description") }, true),
				new NomenclatureRecord("0102219000 80", startDate, endDate, 8, 3, "Other", new List<(string language, string description)>() { ("FR", "French description") }, true),
				new NomenclatureRecord("0102290000 80", startDate, endDate, 6, 2, "Other", new List<(string language, string description)>() { ("FR", "French description") }, false),
				new NomenclatureRecord("0102290500 80", startDate, endDate, 8, 3, "Of the sub-genus Bibos or of the sub-genus Poephagus", new List<(string language, string description)>() { ("FR", "French description") }, true),
				new NomenclatureRecord("0102291000 10", startDate, endDate, 8, 3, "Other", new List<(string language, string description)>() { ("FR", "French description") }, false),
				new NomenclatureRecord("0102291000 80", startDate, endDate, 8, 4, "Of a weight not exceeding 80|kg", new List<(string language, string description)>() { ("FR", "French description") }, false),
				new NomenclatureRecord("0102291010 80", startDate, endDate, 10, 5, "Young male bovine animals, intended for fattening", new List<(string language, string description)>() { ("FR", "French description") }, true),
				new NomenclatureRecord("0102291020 80", startDate, endDate, 10, 5, "Heifers of the grey, brown or yellow mountain breeds and spotted Pinzgau breed, other than for slaughter", new List<(string language, string description)>() { ("FR", "French description") }, true),
				new NomenclatureRecord("0102291030 80", startDate, endDate, 10, 5, "Heifers of the Schwyz and Fribourg breeds, other than for slaughter", new List<(string language, string description)>() { ("FR", "French description") }, true),
				new NomenclatureRecord("0102291040 80", startDate, endDate, 10, 5, "Heifers of the spotted Simmental breed, other than for slaughter", new List<(string language, string description)>() { ("FR", "French description") }, true),
				new NomenclatureRecord("0102291050 80", startDate, endDate, 10, 5, "Bulls of the Schwyz, Fribourg and spotted Simmental breeds, other than for slaughter", new List<(string language, string description)>() { ("FR", "French description") }, true),
				new NomenclatureRecord("0102291090 80", startDate, endDate, 10, 5, "Other", new List<(string language, string description)>() { ("FR", "French description") }, true),
			};
		}

		IEnumerable<INomenclatureRecord> rawRecords;
		readonly DateTime startDate = DateTime.MinValue;
		readonly DateTime endDate = DateTime.MaxValue;
	}
}






