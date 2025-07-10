using System;
using System.Collections.Generic;
using CargoWise.RefDbRepo.Common.Argument;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;

namespace CargoWise.RefDbRepo.UniversalXMLProducers.Common.CompositeKey
{
	public class CompositeKeyGenerator : ICompositeKeyGenerator
	{
		readonly ICompositeKeyGeneratorResult result;

		public CompositeKeyGenerator()
		{
			result = new CompositeKeyGeneratorResult();
		}

		public ICompositeKeyGeneratorResult GenerateCompositeKeys(ICompositeKeyNode rootNode)
		{
			result.NomenclatureGroups.Clear();
			result.Tariffs.Clear();

			Traverse(rootNode);
			return result;
		}

		void Traverse(ICompositeKeyNode node)
		{
			Argument.NotNull(node, nameof(node));

			var nodeValue = Utils.RemoveTrailingZeros(node.Value);

			switch (node.Level)
			{
				case -1:
					node.CompositeKey = string.Empty;
					break;

				case 0:
					node.CompositeKey = CompositeKeyGeneratorHelper.ExtractCompositeKeyOfSection(node);
					break;

				case 1:
					node.CompositeKey = CompositeKeyGeneratorHelper.ExtractCompositeKeyOfChapter(node);
					break;

				case 2:
					node.CompositeKey = CompositeKeyGeneratorHelper.ExtractCompositeKeyOfSubChapter(node);
					break;

				case 3:
					node.CompositeKey = CompositeKeyGeneratorHelper.ExtractCompositeKeyOfHeading(node);
					break;

				case 4:
					node.CompositeKey = CompositeKeyGeneratorHelper.ExtractCompositeKeyOfSubHeading(node);
					break;

				case 5:
					if (nodeValue.Length <= 6)
					{
						node.CompositeKey = CompositeKeyGeneratorHelper.ExtractCompositeKeyOfSubHeading(node);
					}
					break;
			}

			if (node.Level > -1 && string.IsNullOrEmpty(node.CompositeKey))
			{
				throw new InvalidOperationException("Composite Key expected");
			}

			switch (node.NodeType)
			{
				case CompositeKeyNodeType.NomenclatureGroup:
					var nomenclatureLanguage = new List<RefCusNomenclatureLanguage>();
					if (node.Language != null)
					{
						foreach (var language in node.Language)
						{
							nomenclatureLanguage.Add(new RefCusNomenclatureLanguage() { ZX8_ZX6_NKLanguage = language.language, ZX8_Description = language.description });
						}
					}

					var nomenclatureGroup = new RefCusNomenclatureGroup
					{
						ZZ5_Value = node.ZZ5Value,
						ZZ5_CompositeKey = node.CompositeKey,
						ZZ5_StartDate = node.StartDate,
						ZZ5_EndDate = node.EndDate,
						ZZ5_Description = node.Description,
						RefCusNomenclatureLanguages = nomenclatureLanguage.ToArray()
					};
					result.NomenclatureGroups.Add(nomenclatureGroup);
					break;

				case CompositeKeyNodeType.Tariff:
					var tariffLanguage = new List<RefCusTariffLanguage>();
					if (node.Language != null)
					{
						foreach (var language in node.Language)
						{
							tariffLanguage.Add(new RefCusTariffLanguage() { ZX7_ZX6_NKLanguage = language.language, ZX7_Description = language.description });
						}
					}
					var tariff = new RefCusTariff
					{
						ZZ1_TariffCode = nodeValue,
						ZZ1_Description = node.Description,
						ZZ1_StartDate = node.StartDate,
						ZZ1_EndDate = node.EndDate,
						ZZ1_CompositeKeyOnZZ5 = node.CompositeKey,
						RefCusTariffLanguages = tariffLanguage.ToArray()
					};
					result.Tariffs.Add(tariff);
					break;
			}

			var index = 1;
			var prefix0 = node.Children.Count >= 10;

			foreach (var child in node.Children)
			{
				var compositeKey = $"{node.CompositeKey}.{index * 10}";
				if (prefix0 && index < 10)
				{
					compositeKey = $"{node.CompositeKey}.0{index * 10}";
				}

				child.CompositeKey = compositeKey;
				Traverse(child);
				index++;
			}
		}
	}
}
