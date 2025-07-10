using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using CargoWise.RefDbRepo.CHReferenceData.Business.ExportTariffs.MasterDataV3;
using CargoWise.RefDbRepo.CHReferenceData.Services;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;

namespace CargoWise.RefDbRepo.CHReferenceData.Business.Tariffs
{
	public class ExportTariffsParser : PassarTariffsParser
	{
		readonly DownloadResult masterDataDownload;
		readonly DownloadResult tariffStructureDownload;
		readonly DownloadResult keyStructureDownload;
		readonly DownloadResult permitInformationDownload;
		readonly DownloadResult nonCustomsLawInformationDownload;

		public ExportTariffsParser(DownloadResult masterDataDownload, DownloadResult tariffStructureDownload, DownloadResult keyStructureDownload, DownloadResult permitInformationDownload, DownloadResult nonCustomsLawInformationDownload, DateTime actualDate) : base(actualDate)
		{
			this.masterDataDownload = masterDataDownload;
			this.tariffStructureDownload = tariffStructureDownload;
			this.keyStructureDownload = keyStructureDownload;
			this.permitInformationDownload = permitInformationDownload;
			this.nonCustomsLawInformationDownload = nonCustomsLawInformationDownload;
		}

		protected override string TariffType => "EXP";

		public override void ConvertToRefXML(string outputFile, Func<string, bool> tariffCodeFilter = null)
		{
			var writerConfiguration = GetXmlWriterConfiguration();

			var tariffStructure = new TariffStructure().Load(tariffStructureDownload);
			var keyStructureForSCE = new KeyStructureExport(keyStructureDownload, false);
			var keyStructureForVLS = new KeyStructureExport(keyStructureDownload, true);
			var permitInformation = new PermitInformation(permitInformationDownload);
			var nonCustomsLawInformation = new NonCustomsLawInformation(nonCustomsLawInformationDownload);

			var inputDoc = Helper.DeserializeXML<tariffMasterData>(masterDataDownload.Content);
			var tariffList = new List<RefCusTariff>();

			foreach (var tariff in from commodityCode in inputDoc.commodityCodes
								 where commodityCode.validForExport
								 from controlCode in commodityCode.controlCode
								 where controlCode.validForExport
								 orderby controlCode.value, controlCode.validFrom
								 select new { commodityCode, controlCode })
			{
				var tariffCode = tariff.commodityCode.value.Replace(".", "");
				tariffCode += tariff.controlCode.value.ToString("000", CultureInfo.InvariantCulture);

				var description = GetDescription(tariffStructure, keyStructureForSCE, keyStructureForVLS, tariff.commodityCode.value, tariff.controlCode.value);

				var tariffUOMList = new List<RefCusTariffUOM>();
				GetDefaultUOMs(tariffUOMList);
				GetSensibleGoodsUOM(tariffUOMList, tariff.commodityCode.value);
				var additionalQuantityUOM = GetAdditionalQuantityUOM(tariffUOMList, tariff.controlCode);

				var languages = GetLanguages(description);
				if (!languages.Any())
				{
					continue;
				}

				var refCusConditionList = new List<RefCusCondition>();
				GetScaleWeightCode(refCusConditionList, tariff.controlCode, additionalQuantityUOM);
				GetPermitConditions(refCusConditionList, tariff.controlCode, permitInformation, tariff.commodityCode.value);
				GetNonCustomsLawConditions(refCusConditionList, tariff.controlCode, nonCustomsLawInformation, tariff.commodityCode.value);

				var refCusTariffAttributesList = new List<RefCusTariffAttribute>();
				GetStorageType(refCusTariffAttributesList, tariff.controlCode);
				GetQuantityCode(refCusTariffAttributesList, tariff.controlCode);
				GetSensibleGoodsCode(refCusTariffAttributesList, tariff.commodityCode);

				var validFrom = tariffList.Exists(x => x.ZZ1_TariffCode == tariffCode) ? Helper.Max(tariff.commodityCode.validFrom, tariff.controlCode.validFrom) : tariff.commodityCode.validFrom;
				var validTo = Helper.Min(tariff.commodityCode.validTo, tariff.controlCode.validTo);

				tariffList.Add(new RefCusTariff
				{
					ZZ1_TariffCode = tariffCode,
					ZZ1_StartDate = Helper.Truncate(validFrom),
					ZZ1_EndDate = Helper.EndOfDay(Helper.Truncate(validTo)),
					ZZ1_CompositeKeyOnZZ5 = description.SortKey,
					ZZ1_Description = languages.First().ZX7_Description,
					RefCusTariffLanguages = languages.Skip(1).ToArray(),
					RefCusTariffUOMs = tariffUOMList.ToArray(),
					RefCusConditions = refCusConditionList.ToArray(),
					RefCusTariffAttributes = refCusTariffAttributesList.ToArray(),
				});
			}

			tariffList.Add(GetPlaceholderTariff());

			var published = inputDoc.created.Date;
			tariffCodeFilter = tariffCodeFilter ?? (x => true);

			Helper.ExportToXMLFile(DataSource, outputFile, writerConfiguration, published, tariffList.Where(x => tariffCodeFilter(x.ZZ1_TariffCode)));
		}

		static void GetScaleWeightCode(List<RefCusCondition> refCusConditionList, tariffMasterDataCommodityCodeControlCode controlCode, string uom)
		{
			GetScaleWeightCode(refCusConditionList, controlCode.scaleWeightCode, controlCode.lowerScaleWeight, controlCode.upperScaleWeight, controlCode.validFrom, controlCode.validTo, uom);
		}

		static void GetSensibleGoodsCode(List<RefCusTariffAttribute> refCusTariffAttributesList, tariffMasterDataCommodityCode commodityCode)
		{
			GetSensibleGoodsCode(refCusTariffAttributesList, commodityCode.value);
		}

		protected override RefCusTariff GetPlaceholderTariff()
		{
			var placeholderTariff = base.GetPlaceholderTariff();
			placeholderTariff.ZZ1_TariffCode = "99999999000";
			return placeholderTariff;
		}

		protected override string[] WriterConfigurationProperties => new[]
		{
			nameof(RefCusTariffLanguage),
			nameof(RefCusTariffUOM),
			nameof(RefCusCondition),
			nameof(RefCusConditionLanguage),
			nameof(RefCusConditionValue),
			nameof(RefCusApplicability),
			nameof(RefCusTariffAttribute),
			nameof(RefCusExcludedTradeGroup),
			nameof(RefCusTariff.ZZ1_CompositeKeyOnZZ5),
			nameof(RefCusRate.ZZ2_ZZS_NKPreference),
			nameof(RefCusRate.ZZ2_ZZS_ZZZ_NKDataGrouping),
		};

		protected override string RateTypeConstantValue => "DTY";

		protected override string RateCodeConstantValue => "DTY";

		const string DataSource = "CH Tariff EXP";
	}
}
