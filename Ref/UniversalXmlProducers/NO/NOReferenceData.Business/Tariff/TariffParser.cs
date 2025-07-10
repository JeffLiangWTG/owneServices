using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;
using CargoWise.RefDbRepo.NOReferenceData.Business.Nomenclature;
using CargoWise.RefDbRepo.NOReferenceData.Services.RefCusConditions;
using CargoWise.RefDbRepo.NOReferenceData.Services.Tariff.Avgiftliste;
using CargoWise.RefDbRepo.NOReferenceData.Services.Tariff.Customstariffstructure;
using CargoWise.RefDbRepo.NOReferenceData.Services.Tariff.Tollsats;
using CargoWise.RefDbRepo.NOReferenceData.Services.Tariff.Varenummer;
using CargoWise.RefDbRepo.NOReferenceData.Services.TradeGroups.LandGruppe;

namespace CargoWise.RefDbRepo.NOReferenceData.Business.Tariff
{
	public static class TariffParser
	{
		public static string ConvertToXMLFile(
			string dataSource,
			vareListe customsRateList,
			vareListe rawMaterialRateList,
			CustomsTariffStructure tariffStructure,
			AvgiftListe importFees,
			AvgiftListe exportFees,
			VarenummerListe goodsNumber,
			LandgruppeListe countryGroups,
			RefCusCodeConditionItems refCusCodeConditionItems,
			string generalTariffStructureXml,
			DateTime lastModified,
			string outputFileWithPath)
		{
			ErrorBuilder.Clear();

			_ = customsRateList ?? throw new ArgumentNullException(nameof(customsRateList));
			_ = rawMaterialRateList ?? throw new ArgumentNullException(nameof(rawMaterialRateList));
			_ = tariffStructure ?? throw new ArgumentNullException(nameof(tariffStructure));
			_ = importFees ?? throw new ArgumentNullException(nameof(importFees));
			_ = exportFees ?? throw new ArgumentNullException(nameof(exportFees));
			_ = goodsNumber ?? throw new ArgumentNullException(nameof(goodsNumber));
			_ = refCusCodeConditionItems ?? throw new ArgumentNullException(nameof(refCusCodeConditionItems));
			_ = outputFileWithPath ?? throw new ArgumentNullException(nameof(outputFileWithPath));

			var nomenclature = NomenclatureParser.CreateNomenclature(generalTariffStructureXml);
			var cusTariffList = new List<RefCusTariff>();

			foreach (var commodity in GetCommodities(tariffStructure))
			{
				var tariffId = commodity.id;
				var compositeKey = Nomenclature.GetCompositeKey(nomenclature, commodity.hsNumber, tariffId);

				var refCusTariffUOMs = CusTariffUOM.GetRefCusTariffUOM(goodsNumber, tariffId);
				var refCusConditions = CusConditions.GetCusConditions(refCusCodeConditionItems, importFees, tariffId);

				var allUomForTariff = refCusTariffUOMs.Select(x => x.ZZ8_UOM).ToList();

				var refCusTariffResultZz = new RefCusTariff
				{
					RefCusTariffUOMs = refCusTariffUOMs,
					RefCusVATApplicabilities = CusVATApplicability.GetRefCusVATApplicabilities(importFees, tariffId),
					RefCusRates = CusRate.GetRefCusRate(customsRateList, rawMaterialRateList, importFees, exportFees, countryGroups, tariffId, allUomForTariff, refCusConditions),
					RefCusConditions = refCusConditions,
					ZZ1_Description = DataHelpers.CleanHtmlStringIfApplicable(commodity.item.Trim()),
					ZZ1_TariffCode = tariffId,
					ZZ1_CompositeKeyOnZZ5 = compositeKey,
					ZZ1_EndDate = Constants.MaximumDateTime,
					ZZ1_StartDate = Constants.MinimumDateTime
				};

				if (refCusTariffResultZz.RefCusRates.Any())
				{
					cusTariffList.Add(refCusTariffResultZz);
				}
			}

			if (cusTariffList.Any())
			{
				var refCusTariffConfiguration = XmlWriterConfig.GetRefCusTariffWriterConfiguration();
				FileHelper.ExportToXMLFile(dataSource, outputFileWithPath, refCusTariffConfiguration, lastModified, cusTariffList);
			}

			return ErrorBuilder.ToString();
		}

		static IEnumerable<Commodity> GetCommodities(CustomsTariffStructure tariffStructure)
		{
			var items = from section in tariffStructure?.sections ?? Enumerable.Empty<Section>()
						from chapter in section.chapters
						where chapter?.divisions?.Items?.Length > 0
						from divisions in chapter.divisions?.Items ?? Array.Empty<Type>()
						select divisions;

			return GetCommodities(items);
		}

		static IEnumerable<Commodity> GetCommodities(IEnumerable<object> items)
		{
			if (items is null)
			{
				yield break;
			}

			foreach (var item in items)
			{
				switch (item)
				{
					case Commodity commodity:
						yield return commodity;
						break;
					case Heading { divisions.Items: { } headingItems }:
						foreach (var commodity in GetCommodities(headingItems))
						{
							yield return commodity;
						}
						break;
					case SubHeading { divisions.Items: { } sbHeadingItems }:
						foreach (var commodity in GetCommodities(sbHeadingItems))
						{
							yield return commodity;
						}
						break;
				}
			}
		}

		public static StringBuilder ErrorBuilder => errorBuilder ?? (errorBuilder = new StringBuilder());
		static StringBuilder errorBuilder;
	}
}
