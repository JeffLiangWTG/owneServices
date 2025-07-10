using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.RefDbRepo.Common.SafeDataClient;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;
using CargoWise.RefDbRepo.KRReferenceData.Services;
using NPOI.SS.UserModel;

namespace CargoWise.RefDbRepo.KRReferenceData.Business
{
	public class NomenclatureAdditionalDataUpdater : IAdditionalDataUpdater<RefCusNomenclatureGroup>
	{
		public static bool IsDataRowValid(IRow row, EntityConfiguration configuration)
		{
			var result = false;
			var columnIndex = configuration.EntityTypeExcelColumnMapping.EntityTypes.SelectMany(x => x.Properties.Where(y => y.Name == nameof(RefCusNomenclatureGroup.ZZ5_Value))).FirstOrDefault()?.ExcelColumn ?? -1;
			if (columnIndex != -1)
			{
				var cell = row.GetCell(columnIndex);
				cell?.SetCellType(CellType.String);
				var cellValue = cell?.StringCellValue ?? string.Empty;
				result = cellValue.Length != Constants.TariffLength;
			}
			return result;
		}

		public void UpdateAdditionally(RefCusNomenclatureGroup nomenclature)
		{
			nomenclature.ZZ5_CompositeKey = CompositeKeyHelper.GenerateCompositeKey(nomenclature);
		}

		RefDataEntityLoader CompositeKeyHelper => compositeKeyHelper ?? (compositeKeyHelper = new RefDataEntityLoader(SafeRepository));
		RefDataEntityLoader compositeKeyHelper;

		protected virtual ISafeRepository SafeRepository => new SafeRepository(new Uri(ApplicationConfig.SafeDataUpdateUri));

		bool IAdditionalDataUpdater<RefCusNomenclatureGroup>.IsDataRowValid(IRow row, EntityConfiguration configuration) => IsDataRowValid(row, configuration);
		void IAdditionalDataUpdater<RefCusNomenclatureGroup>.UpdateAdditionally(RefCusNomenclatureGroup nomenclature, IRow row, EntityConfiguration configuration) => UpdateAdditionally(nomenclature);
		void IAdditionalDataUpdater<RefCusNomenclatureGroup>.UpdateRule(RefCusNomenclatureGroup dataEntity, Rule rule) { }
		void IAdditionalDataUpdater<RefCusNomenclatureGroup>.RegisterUpdated(IRow row, EntityConfiguration configuration) { }
	}
}
