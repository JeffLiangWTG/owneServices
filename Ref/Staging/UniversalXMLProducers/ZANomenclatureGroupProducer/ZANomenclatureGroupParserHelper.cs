using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.RefDbRepo.Common.Argument;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;
using CargoWise.RefDbRepo.UniversalXMLProducers.Common.CompositeKey;

namespace CargoWise.RefDbRepo.UniversalXMLProducers.ZANomenclatureGroupProducer
{
	public static class ZANomenclatureGroupParserHelper
	{
		public static ICompositeKeyGeneratorResult CompositeKeyGenerate(CompositeKeyNode rootNode)
		{
			Argument.NotNull(rootNode, nameof(rootNode));
			Argument.IsTrue(rootNode.Parent == null, "rootNode.Parent");

			var compositeKeyGenerator = new CompositeKeyGenerator();
			return compositeKeyGenerator.GenerateCompositeKeys(rootNode);
		}

		public static void PopulateTariffUXML(ICollection<RefCusTariff> tariffs, IXmlWriter tariffXmlWriter)
		{
			Argument.NotNull(tariffs, nameof(tariffs));
			Argument.NotNull(tariffXmlWriter, nameof(tariffXmlWriter));

			var addedTariffCompositeKeys = new List<string>();
			foreach (var tariff in tariffs)
			{
				if (addedTariffCompositeKeys.Any(o => o == tariff.ZZ1_CompositeKeyOnZZ5))
				{
					throw new NotSupportedException($"Duplicate composite key present {tariff.ZZ1_CompositeKeyOnZZ5}, A composite key is already present in the XML.");
				}
				tariffXmlWriter.PopulateData(tariff);
				addedTariffCompositeKeys.Add(tariff.ZZ1_CompositeKeyOnZZ5);
			}
		}

		public static void PopulateNomenclatureGroupUXML(ICollection<RefCusNomenclatureGroup> nomenclatureGroups, IXmlWriter nomenclatureGroupXmlWriter)
		{
			Argument.NotNull(nomenclatureGroups, nameof(nomenclatureGroups));
			Argument.NotNull(nomenclatureGroupXmlWriter, nameof(nomenclatureGroupXmlWriter));

			var addedNomenclatureCompositeKeys = new List<string>();
			foreach (var nomenclature in nomenclatureGroups)
			{
				if (addedNomenclatureCompositeKeys.Any(o => o == nomenclature.ZZ5_CompositeKey))
				{
					throw new NotSupportedException($"Duplicate composite key present {nomenclature.ZZ5_CompositeKey}, A composite key is already present in the XML.");
				}
				nomenclatureGroupXmlWriter.PopulateData(nomenclature);
				addedNomenclatureCompositeKeys.Add(nomenclature.ZZ5_CompositeKey);
			}
		}

		public static ICompositeKeyNode GetParent(List<ICompositeKeyNode> compositeKeyNodeList, int level)
		{
			Argument.NotNull(compositeKeyNodeList, nameof(compositeKeyNodeList));
			if (!compositeKeyNodeList.Any())
			{
				return null;
			}
			var last = compositeKeyNodeList.Last(o => o.NodeType != CompositeKeyNodeType.Tariff);
			if (last.Level != (level - 1))
			{
				return GetParent(last.Children.ToList(), level);
			}

			return last;
		}
	}
}
