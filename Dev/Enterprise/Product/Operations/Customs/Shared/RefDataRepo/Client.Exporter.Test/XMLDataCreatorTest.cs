using CargoWise.RefDataRepo.Ent.Client.DataStorage;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;
using Moq;
using NUnit.Framework;

namespace CargoWise.RefDataRepo.Ent.Client.Exporter.Test
{
	public class XMLDataCreatorTest : TestCase
	{
		public void TestCreated_CustomsMapping()
		{
			var valueRetrieval = new Mock<IValueRetrieval>();
			var codeMock = new Mock<IDataRow>();
			valueRetrieval.Setup(x => x.GetValue(codeMock.Object, typeof(IZZRefCusCodeListCombined), typeof(string),
				nameof(IZZRefCusCodeListCombined.ZZD_Code))).Returns("AA");
			valueRetrieval.Setup(x => x.GetValue(codeMock.Object, typeof(IZZRefCusCodeListCombined), typeof(string),
				nameof(IZZRefCusCodeListCombined.ZZD_IsAir))).Returns("Air");
			var attrMock = new Mock<IDataRow>();
			valueRetrieval.Setup(x => x.GetValue(attrMock.Object, typeof(IZZRefCusCodeListAttributeCombined),
				typeof(string), nameof(IZZRefCusCodeListAttributeCombined.ZZE_ZXE_NKName))).Returns("BB");
			valueRetrieval.Setup(x => x.GetValue(attrMock.Object, typeof(IZZRefCusCodeListAttributeCombined),
				typeof(string), nameof(IZZRefCusCodeListAttributeCombined.ZZE_IsSea))).Returns("Sea");

			valueRetrieval.Setup(x => x.GetRelatedEntities(codeMock.Object, typeof(IZZRefCusCodeListCombined),
				typeof(IZZRefCusCodeListAttributeCombined))).Returns(new[] { attrMock.Object });

			var creator = new XMLDataCreator(valueRetrieval.Object, new ReferenceXMLMappingProvider());
			var result = creator.Create(codeMock.Object, typeof(IZZRefCusCodeListCombined)) as RefCusCodeList;

			AssertEquals("AA", result.ZZD_Code);
			var attr = result.RefCusCodeListAttributes[0];
			AssertEquals("BB", attr.ZZE_ZXE_NKName);
			var transport1 = result.RefCusCodeOrAttributeTransportModes[0];
			AssertEquals("Air", transport1.ZZU_TransportMode);
			var transport2 = attr.RefCusCodeOrAttributeTransportModes[0];
			AssertEquals("Sea", transport2.ZZU_TransportMode);
		}
	}
}
