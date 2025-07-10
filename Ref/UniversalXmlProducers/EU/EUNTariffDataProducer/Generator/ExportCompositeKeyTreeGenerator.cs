using System.Collections.Generic;
using System.Linq;
using CargoWise.RefDbRepo.UniversalXMLProducers.Common.CompositeKey;

namespace CargoWise.RefDbRepo.UniversalXMLProducers.EUNTariffDataProducer
{
	public class ExportCompositeKeyTreeGenerator : ICompositeKeyTreeGenerator
	{
		public ExportCompositeKeyTreeGenerator(IChapterToSectionMapper chapterToSectionMapper)
		{
			compositeKeyGenerator = new CompositeKeyTreeGenerator(chapterToSectionMapper);
		}

		readonly CompositeKeyTreeGenerator compositeKeyGenerator;

		public ICompositeKeyNode GenerateTree(IEnumerable<INomenclatureRecord> nomenclatureRecords)
		{
			var compositeKeyNode = compositeKeyGenerator.GenerateTree(nomenclatureRecords);
			PrepareDataForExport(compositeKeyNode);
			return compositeKeyNode;
		}

		#region Implementation

		static void PrepareDataForExport(ICompositeKeyNode node)
		{
			var hasLeafChilds = RemoveLeafChild(node);

			node.Children.ToList().ForEach(x => PrepareDataForExport(x));

			if (hasLeafChilds)
			{
				_ = AddNewExportLeafChild(node);
			}
		}

		static CompositeKeyNode AddNewExportLeafChild(ICompositeKeyNode node)
		{
			return new CompositeKeyNode(EUNUtils.NormalizeExportTariffCode(node.Value), node.Description, node.StartDate, node.EndDate, CompositeKeyNodeType.Tariff, node.Level + 1, node, node.Language.ToList());
		}

		static bool RemoveLeafChild(ICompositeKeyNode node)
		{
			var leafChildNodesToRemove = node.Children.Where(x => x.Value.Length == 10).ToList();

			foreach (var nodeToRemove in leafChildNodesToRemove)
			{
				node.Children.Remove(nodeToRemove);
			}

			return leafChildNodesToRemove.Any();
		}

		#endregion
	}
}
