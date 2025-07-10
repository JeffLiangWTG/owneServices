using System;
using System.Linq;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;
using CargoWise.RefDbRepo.EUReferenceData.CUSNumbers.Business;
using Moq;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.EUReferenceData.CUSNumbers.Tests
{
	[TestFixture]
	class CUSNumberRefCusCodeListProducerTest
	{
		[Test]
		public void GuardClause()
		{
			Assert.Throws<ArgumentNullException>(() => new CUSNumberRefCusCodeListProducer(cusCodeAttributeProvider: null));
		}

		[Test]
		public void GenerateXML_MapCusCodeAttribute()
		{
			var attributeForTest = new RefCusCodeListAttribute() { ZZE_ZXE_NKName = "CL016", ZZE_Value = "Y" };

			var cusCodeAttributeProviderMock = new Mock<ICusCodeAttributeProvider>();
			cusCodeAttributeProviderMock.Setup(x => x.GetCusCodeAttribute("0010003-8")).Returns(attributeForTest);

			var cusNumberForTest = new CUSNumber("0010003-8", "acenaphthene", "29029000", "83-32-9", "201-469-6", "3082", Array.Empty<CUSTranslatedName>());
			var producer = new CUSNumberRefCusCodeListProducer(cusCodeAttributeProviderMock.Object);
			var generatedXml = producer.GenerateRefCusCodeListItems(new CUSNumber[] { cusNumberForTest }).ToArray();

			Assert.That(generatedXml.Length, Is.EqualTo(1), "Number of generated RefCusCodeList(s) entries");
			cusCodeAttributeProviderMock.Verify(x => x.GetCusCodeAttribute("0010003-8"), Times.Exactly(1));
			Assert.That(generatedXml[0].RefCusCodeListAttributes, Contains.Item(attributeForTest), "RefCusCodeListAttribute is added to the RefCusCodeList entry");
		}
	}
}
