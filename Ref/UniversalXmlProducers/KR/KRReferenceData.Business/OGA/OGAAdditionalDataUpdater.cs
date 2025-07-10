using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;
using NPOI.SS.UserModel;

namespace CargoWise.RefDbRepo.KRReferenceData.Business.OGA
{
	public class OGAAdditionalDataUpdater : IAdditionalDataUpdater<RefCusTariff>
	{
		public OGAAdditionalDataUpdater(DateTime publicationDate)
		{
			if (publicationDate == DateTime.MinValue)
			{
				throw new ArgumentException("Publication date is empty", nameof(publicationDate));
			}
			PublicationDate = publicationDate;
			ProcessedRows = new ProcessedRows();
		}
		DateTime PublicationDate { get; }
		ProcessedRows ProcessedRows { get; }

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
				result = !ProcessedRows.IsAlreadyProcessed(row, configuration);
			}
			return result;
		}

		public void UpdateAdditionally(RefCusTariff dataEntity)
		{
			dataEntity.ZZ1_TariffCode = dataEntity.ZZ1_TariffCode.Replace(".", "").Replace("-", "");
			foreach (var condition in dataEntity.RefCusConditions)
			{
				condition.ZX1_StartDate = condition.ZX1_StartDate.Equals(DateTime.MinValue) ? PublicationDate : condition.ZX1_StartDate;
				condition.ZX1_EndDate = condition.ZX1_EndDate > MaxEndDate ? MaxEndDate : condition.ZX1_EndDate;
			}
		}

		public void RegisterUpdated(IRow row, EntityConfiguration configuration)
		{
			ProcessedRows.RegisterProcessed(row, configuration);
		}

		bool IAdditionalDataUpdater<RefCusTariff>.IsDataRowValid(IRow row, EntityConfiguration configuration) => IsDataRowValid(row, configuration);
		void IAdditionalDataUpdater<RefCusTariff>.UpdateAdditionally(RefCusTariff dataEntity, IRow row, EntityConfiguration configuration) => UpdateAdditionally(dataEntity);
		void IAdditionalDataUpdater<RefCusTariff>.UpdateRule(RefCusTariff dataEntity, Rule rule) { }
		void IAdditionalDataUpdater<RefCusTariff>.RegisterUpdated(IRow row, EntityConfiguration configuration) => RegisterUpdated(row, configuration);
		readonly DateTime MaxEndDate = new DateTime(2079, 06, 06, 23, 59, 00);
	}
}
