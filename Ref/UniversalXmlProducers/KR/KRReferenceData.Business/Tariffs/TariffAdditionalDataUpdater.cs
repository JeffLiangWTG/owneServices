using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.RefDbRepo.Common.SafeDataClient;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;
using CargoWise.RefDbRepo.KRReferenceData.Services;
using NPOI.SS.UserModel;

namespace CargoWise.RefDbRepo.KRReferenceData.Business
{
	public class TariffAdditionalDataUpdater : IAdditionalDataUpdater<RefCusTariff>
	{
		public void UpdateAdditionally(RefCusTariff tariff)
		{
			tariff.ZZ1_CompositeKeyOnZZ5 = CompositeKeyHelper.GenerateCompositeKey(tariff);
			if (tariff.RefCusTariffUOMs != null)
			{
				for (int index = 0; index < tariff.RefCusTariffUOMs.Length; index++)
				{
					var uom = tariff.RefCusTariffUOMs[index];
					if (string.IsNullOrEmpty(uom.ZZ8_Type))
					{
						uom.ZZ8_Type = "CU" + (index + 1);
					}
				}
			}
		}

		public static void UpdateRule(RefCusTariff tariff, Rule rule)
		{
			switch (rule.Name)
			{
				case RuleID.InvoiceQtyInCU1:
					ManageInvoiceQtyInCU1(tariff, rule);
					break;
				case RuleID.MINAsCustomsQuantity:
					ManageMinAsCU3(tariff, rule);
					break;
			}
		}

		public static bool IsDataRowValid(IRow row, EntityConfiguration configuration)
		{
			var result = false;
			var columnIndex = configuration.EntityTypeExcelColumnMapping.EntityTypes.SelectMany(x => x.Properties.Where(y => y.Name == nameof(RefCusTariff.ZZ1_TariffCode))).FirstOrDefault()?.ExcelColumn ?? -1;
			if (columnIndex != -1)
			{
				var cell = row.GetCell(columnIndex);
				cell?.SetCellType(CellType.String);
				var cellValue = cell?.StringCellValue ?? string.Empty;
				result = cellValue.Length == Constants.TariffLength;
			}
			return result;
		}
		protected virtual ISafeRepository SafeRepository => new SafeRepository(new Uri(ApplicationConfig.SafeDataUpdateUri));
		RefDataEntityLoader CompositeKeyHelper => compositeKeyHelper ?? (compositeKeyHelper = new RefDataEntityLoader(SafeRepository));
		RefDataEntityLoader compositeKeyHelper;


		static void ManageInvoiceQtyInCU1(RefCusTariff tariff, Rule rule)
		{
			if (rule.Relationship == Relationship.OR.ToString())
			{
				if (rule.RuleValues.Any(x => tariff.ZZ1_TariffCode.StartsWith(x.Value, StringComparison.Ordinal)))
				{
					var attribute = new RefCusTariffAttribute();
					attribute.ZZ3_Name = RuleID.InvoiceQtyInCU1;
					attribute.ZZ3_Value = "Y";
					var attributeList = tariff.RefCusTariffAttributes != null ? tariff.RefCusTariffAttributes.ToList() : new List<RefCusTariffAttribute>();
					attributeList.Add(attribute);
					tariff.RefCusTariffAttributes = attributeList.ToArray();
				}
			}
		}

		static void ManageMinAsCU3(RefCusTariff tariff, Rule rule)
		{
			if (rule.Relationship == Relationship.OR.ToString())
			{
				if (rule.RuleValues.Any(x => x.Value == tariff.ZZ1_TariffCode))
				{
					var index = tariff.RefCusTariffUOMs?.Length ?? 0;
					var uom = new RefCusTariffUOM();
					uom.ZZ8_Type = "CU" + (index + 1);
					uom.ZZ8_UOM = "MIN";
					var uomList = tariff.RefCusTariffUOMs != null ? tariff.RefCusTariffUOMs.ToList() : new List<RefCusTariffUOM>();
					uomList.Add(uom);
					tariff.RefCusTariffUOMs = uomList.ToArray();
				}
			}
		}

		void IAdditionalDataUpdater<RefCusTariff>.UpdateAdditionally(RefCusTariff tariff, IRow row, EntityConfiguration configuration) => UpdateAdditionally(tariff);
		void IAdditionalDataUpdater<RefCusTariff>.UpdateRule(RefCusTariff tariff, Rule rule) => UpdateRule(tariff, rule);
		bool IAdditionalDataUpdater<RefCusTariff>.IsDataRowValid(IRow row, EntityConfiguration configuration) => IsDataRowValid(row, configuration);
		void IAdditionalDataUpdater<RefCusTariff>.RegisterUpdated(IRow row, EntityConfiguration configuration) { }

		class RuleID
		{
			public const string InvoiceQtyInCU1 = "InvoiceQuantity in CU1";
			public const string MINAsCustomsQuantity = "MIN as Customs Quantity";
		}
	}
}
