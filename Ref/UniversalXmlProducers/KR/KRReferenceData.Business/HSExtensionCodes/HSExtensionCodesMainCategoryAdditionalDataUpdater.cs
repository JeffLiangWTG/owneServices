using System.Linq;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;
using NPOI.SS.UserModel;

namespace CargoWise.RefDbRepo.KRReferenceData.Business
{
	public class HSExtensionCodesMainCategoryAdditionalDataUpdater : IAdditionalDataUpdater<RefCusTariff>
	{
		public HSExtensionCodesMainCategoryAdditionalDataUpdater()
		{
			ProcessedRows = new ProcessedRows();
		}

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

		public static void UpdateAdditionally(RefCusTariff tariff, IRow row, EntityConfiguration configuration)
		{
			var tariffAdditionalCode = tariff.RefCusTariffAdditionalCodes.LastOrDefault();
			if (tariffAdditionalCode != null)
			{
				if (tariffAdditionalCode.ZY2_Description.Length > Constants.HSExtensionCodesConstants.DescriptionMaxLength)
				{
					tariffAdditionalCode.ZY2_Description = tariffAdditionalCode.ZY2_Description.Substring(0, Constants.HSExtensionCodesConstants.DescriptionMaxLength);
				}
			}
		}

		public void RegisterUpdated(IRow row, EntityConfiguration configuration)
		{
			ProcessedRows.RegisterProcessed(row, configuration);
		}

		bool IAdditionalDataUpdater<RefCusTariff>.IsDataRowValid(IRow row, EntityConfiguration configuration) => IsDataRowValid(row, configuration);
		void IAdditionalDataUpdater<RefCusTariff>.UpdateAdditionally(RefCusTariff dataEntity, IRow row, EntityConfiguration configuration) => UpdateAdditionally(dataEntity, row, configuration);
		void IAdditionalDataUpdater<RefCusTariff>.UpdateRule(RefCusTariff dataEntity, Rule rule) { }
		void IAdditionalDataUpdater<RefCusTariff>.RegisterUpdated(IRow row, EntityConfiguration configuration) => RegisterUpdated(row, configuration);
	}
}
