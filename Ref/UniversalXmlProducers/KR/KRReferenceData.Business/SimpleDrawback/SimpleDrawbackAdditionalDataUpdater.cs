using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;
using NPOI.OpenXml4Net.OPC;
using NPOI.SS.UserModel;

namespace CargoWise.RefDbRepo.KRReferenceData.Business
{
	public class SimpleDrawbackAdditionalDataUpdater : IAdditionalDataUpdater<RefCusTariff>
	{
		public SimpleDrawbackAdditionalDataUpdater(DateTime publicationDate)
		{
			if (publicationDate == DateTime.MinValue)
			{
				throw new ArgumentException("Publication date is empty", nameof(publicationDate));
			}
			PublicationDate = publicationDate;
		}
		DateTime PublicationDate { get; }

		public bool IsDataRowValid(IRow row, EntityConfiguration configuration)
		{
			var result = false;
			var columnIndex = configuration.EntityTypeExcelColumnMapping.EntityTypes.SelectMany(x => x.Properties.Where(y => y.Name == nameof(RefCusTariff.ZZ1_TariffCode))).FirstOrDefault()?.ExcelColumn ?? -1;
			if (columnIndex != -1)
			{
				var cell = row.GetCell(columnIndex);
				cell?.SetCellType(CellType.String);
				var cellValue = cell?.StringCellValue ?? string.Empty;
				result = !string.IsNullOrEmpty(cellValue);
			}
			if (result)
			{
				var startDateColumnIndex = configuration.EntityTypeExcelColumnMapping.EntityTypes.SelectMany(x => x.Properties.Where(y => y.Name == nameof(RefCusCondition.ZX1_StartDate))).FirstOrDefault()?.ExcelColumn ?? -1;
				if (startDateColumnIndex != -1)
				{
					var cell = row.GetCell(startDateColumnIndex);
					cell?.SetCellType(CellType.String);
					var startDateText = cell?.StringCellValue ?? string.Empty;
					result = !string.IsNullOrEmpty(startDateText) && startDateText.Substring(0, 4) == PublicationDate.ToString("yyyy", CultureInfo.CurrentCulture);
				}
				else
				{
					result = false;
				}
			}
			return result;
		}

		public static void UpdateAdditionally(RefCusTariff dataEntity)
		{
			dataEntity.ZZ1_TariffCode = dataEntity.ZZ1_TariffCode.Replace(".", "").Replace("-", "");
		}

		bool IAdditionalDataUpdater<RefCusTariff>.IsDataRowValid(IRow row, EntityConfiguration configuration) => IsDataRowValid(row, configuration);
		void IAdditionalDataUpdater<RefCusTariff>.UpdateAdditionally(RefCusTariff dataEntity, IRow row, EntityConfiguration configuration) => UpdateAdditionally(dataEntity);
		void IAdditionalDataUpdater<RefCusTariff>.UpdateRule(RefCusTariff dataEntity, Rule rule) { }
		void IAdditionalDataUpdater<RefCusTariff>.RegisterUpdated(IRow row, EntityConfiguration configuration) { }
	}
}
