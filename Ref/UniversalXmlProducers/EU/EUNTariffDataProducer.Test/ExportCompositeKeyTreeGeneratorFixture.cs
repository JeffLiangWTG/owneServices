using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.RefDbRepo.UniversalXMLProducers.Common.CompositeKey;
using Moq;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.UniversalXMLProducers.EUNTariffDataProducer.Test
{
	[TestFixture]
	class ExportCompositeKeyTreeGeneratorFixture
	{
		[Test]
		public void TestCreateTree()
		{
			var chapterToSectionMapper = new Mock<IChapterToSectionMapper>();
			chapterToSectionMapper.Setup(x => x.GetAllSection()).Returns(new[]
			{
				new Section(1, "LIVE ANIMALS; ANIMAL PRODUCTS"),
				new Section(2, "VEGETABLE PRODUCTS")
			});
			chapterToSectionMapper.Setup(x => x.GetSection(It.IsAny<int>())).Returns(1);

			var compositeKeyTreeGenerator = new ExportCompositeKeyTreeGenerator(chapterToSectionMapper.Object);
			var rootNode = compositeKeyTreeGenerator.GenerateTree(rawRecords);

			Assert.IsNotNull(rootNode);
			Assert.True(rootNode.Children.Count == 2);

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
			Assert.AreEqual(1, subNode.Language.Count);

			var subsubNode2 = subNode.Children.Last();
			AssertNode(subsubNode2, "01022910", 1, CompositeKeyNodeType.NomenclatureGroup);

			subsubNode2 = subsubNode2.Children.First();
			AssertNode(subsubNode2, "01022910", 1, CompositeKeyNodeType.NomenclatureGroup);

			subsubNode2 = subsubNode2.Children.First();
			AssertNode(subsubNode2, "01022910", 0, CompositeKeyNodeType.Tariff);
		}

		[SetUp]
		protected void Setup()
		{
			var record1 = new NomenclatureRecord("0100000000 80", DateTime.MinValue, DateTime.MaxValue, 2, 0, "LIVE ANIMALS", new List<(string language, string description)>() { ("FR", "French description") }, false);
			var record2 = new NomenclatureRecord("0101000000 80", DateTime.MinValue, DateTime.MaxValue, 4, 0, "Live horses, asses, mules and hinnies", new List<(string language, string description)>() { ("FR", "French description") }, false);
			var record3 = new NomenclatureRecord("0101210000 10", DateTime.MinValue, DateTime.MaxValue, 6, 1, "Horses", new List<(string language, string description)>() { ("FR", "French description") }, false);
			var record4 = new NomenclatureRecord("0101210000 80", DateTime.MinValue, DateTime.MaxValue, 6, 2, "Pure-bred breeding animals", new List<(string language, string description)>() { ("FR", "French description") }, true);
			var record5 = new NomenclatureRecord("0101290000 80", DateTime.MinValue, DateTime.MaxValue, 6, 2, "Other", new List<(string language, string description)>() { ("FR", "French description") }, false);
			var record6 = new NomenclatureRecord("0101291000 80", DateTime.MinValue, DateTime.MaxValue, 8, 3, "For slaughter", new List<(string language, string description)>() { ("FR", "French description") }, true);
			var record7 = new NomenclatureRecord("0101299000 80", DateTime.MinValue, DateTime.MaxValue, 8, 3, "Other", new List<(string language, string description)>() { ("FR", "French description") }, true);
			var record8 = new NomenclatureRecord("0101300000 80", DateTime.MinValue, DateTime.MaxValue, 6, 1, "Asses", new List<(string language, string description)>() { ("FR", "French description") }, true);
			var record9 = new NomenclatureRecord("0101900000 80", DateTime.MinValue, DateTime.MaxValue, 6, 1, "Other", new List<(string language, string description)>() { ("FR", "French description") }, true);
			var record10 = new NomenclatureRecord("0102000000 80", DateTime.MinValue, DateTime.MaxValue, 4, 0, "Live bovine animals", new List<(string language, string description)>() { ("FR", "French description") }, false);
			var record11 = new NomenclatureRecord("0102210000 10", DateTime.MinValue, DateTime.MaxValue, 6, 1, "Cattle", new List<(string language, string description)>() { ("FR", "French description") }, false);
			var record12 = new NomenclatureRecord("0102210000 80", DateTime.MinValue, DateTime.MaxValue, 6, 2, "Pure-bred breeding animals", new List<(string language, string description)>() { ("FR", "French description") }, false);
			var record13 = new NomenclatureRecord("0102211000 80", DateTime.MinValue, DateTime.MaxValue, 8, 3, "Heifers(female bovines that have never calved)", new List<(string language, string description)>() { ("FR", "French description") }, true);
			var record14 = new NomenclatureRecord("0102213000 80", DateTime.MinValue, DateTime.MaxValue, 8, 3, "Cows", new List<(string language, string description)>() { ("FR", "French description") }, true);
			var record15 = new NomenclatureRecord("0102219000 80", DateTime.MinValue, DateTime.MaxValue, 8, 3, "Other", new List<(string language, string description)>() { ("FR", "French description") }, true);
			var record16 = new NomenclatureRecord("0102290000 80", DateTime.MinValue, DateTime.MaxValue, 6, 2, "Other", new List<(string language, string description)>() { ("FR", "French description") }, false);
			var record17 = new NomenclatureRecord("0102290500 80", DateTime.MinValue, DateTime.MaxValue, 8, 3, "Of the sub-genus Bibos or of the sub-genus Poephagus", new List<(string language, string description)>() { ("FR", "French description") }, true);
			var record18 = new NomenclatureRecord("0102291000 10", DateTime.MinValue, DateTime.MaxValue, 8, 3, "Other", new List<(string language, string description)>() { ("FR", "French description") }, false);
			var record19 = new NomenclatureRecord("0102291000 80", DateTime.MinValue, DateTime.MaxValue, 8, 4, "Of a weight not exceeding 80|kg", new List<(string language, string description)>() { ("FR", "French description") }, false);
			var record20 = new NomenclatureRecord("0102291010 80", DateTime.MinValue, DateTime.MaxValue, 10, 5, "Young male bovine animals, intended for fattening", new List<(string language, string description)>() { ("FR", "French description") }, true);
			var record21 = new NomenclatureRecord("0102291020 80", DateTime.MinValue, DateTime.MaxValue, 10, 5, "Heifers of the grey, brown or yellow mountain breeds and spotted Pinzgau breed, other than for slaughter", new List<(string language, string description)>() { ("FR", "French description") }, true);
			var record22 = new NomenclatureRecord("0102291030 80", DateTime.MinValue, DateTime.MaxValue, 10, 5, "Heifers of the Schwyz and Fribourg breeds, other than for slaughter", new List<(string language, string description)>() { ("FR", "French description") }, true);
			var record23 = new NomenclatureRecord("0102291040 80", DateTime.MinValue, DateTime.MaxValue, 10, 5, "Heifers of the spotted Simmental breed, other than for slaughter", new List<(string language, string description)>() { ("FR", "French description") }, true);
			var record24 = new NomenclatureRecord("0102291050 80", DateTime.MinValue, DateTime.MaxValue, 10, 5, "Bulls of the Schwyz, Fribourg and spotted Simmental breeds, other than for slaughter", new List<(string language, string description)>() { ("FR", "French description") }, true);
			var record25 = new NomenclatureRecord("0102291090 80", DateTime.MinValue, DateTime.MaxValue, 10, 5, "Other", new List<(string language, string description)>() { ("FR", "French description") }, true);

			rawRecords = new[]
			{
				record1, record2, record3, record4, record5, record6, record7, record8, record9, record10,
				record11, record12, record13, record14, record15, record16, record17, record18, record19, record20,
				record21, record22, record23, record24, record25
			};
		}

		IEnumerable<INomenclatureRecord> rawRecords;

		void AssertNode(ICompositeKeyNode node, string nodeValue, int childCount, CompositeKeyNodeType nodeType)
		{
			Assert.AreEqual(nodeValue, node.Value);
			Assert.True(node.Children.Count == childCount);
			Assert.True(node.NodeType == nodeType);
		}
	}
}
