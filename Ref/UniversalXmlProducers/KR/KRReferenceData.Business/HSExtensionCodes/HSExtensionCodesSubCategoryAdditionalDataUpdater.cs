using System;
using System.Linq;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;
using NPOI.SS.UserModel;

namespace CargoWise.RefDbRepo.KRReferenceData.Business
{
	public class HSExtensionCodesAdditionalDataUpdater : IAdditionalDataUpdater<RefCusTariff>
	{

		public static bool IsDataRowValid(IRow row, EntityConfiguration configuration)
		{
			var result = false;
			var columnIndex = configuration.EntityTypeExcelColumnMapping.EntityTypes.SelectMany(x => x.Properties.Where(y => y.Name == nameof(NonPersistentNames.SubCategoryCode))).FirstOrDefault()?.ExcelColumn ?? -1;
			if (columnIndex != -1)
			{
				var cell = row.GetCell(columnIndex);
				cell?.SetCellType(CellType.String);
				var cellValue = cell?.StringCellValue ?? string.Empty;
				result = !string.IsNullOrEmpty(cellValue);
			}
			return result;
		}

		public static void UpdateAdditionally(RefCusTariff tariff, IRow row, EntityConfiguration configuration)
		{
			var tariffAdditionalCodeEntityType = configuration.EntityTypeExcelColumnMapping.EntityTypes.FirstOrDefault(x => x.Name == nameof(RefCusTariffAdditionalCode));
			var subCategory = GetSubCategory(tariffAdditionalCodeEntityType, row);

			var tariffAdditionalCode = tariff.RefCusTariffAdditionalCodes.FirstOrDefault(x => string.IsNullOrEmpty(x.ZY2_AdditionalCode));
			if (tariffAdditionalCode != null)
			{
				tariffAdditionalCode.ZY2_AdditionalCode = subCategory.SubCategoryCode + subCategory.ClassificationTypeCode;
				tariffAdditionalCode.ZY2_Description = GetSubCategoryDescription(subCategory.SubCategoryDescription, subCategory.ClassificationType);
			}
		}

		static string GetSubCategoryDescription(string subCategoryDescription, string classificationType)
		{
			var result = string.Empty;
			var hasClassificationType = !string.IsNullOrEmpty(classificationType) && classificationType != Constants.HSExtensionCodesConstants.EmptySymbol;
			var formattedClassificationType = hasClassificationType ? "(" + classificationType + ")" : string.Empty;
			var descriptionLength = hasClassificationType ? subCategoryDescription.Length + formattedClassificationType.Length : subCategoryDescription.Length;
			if (descriptionLength > Constants.HSExtensionCodesConstants.DescriptionMaxLength)
			{
				result = string.Concat(subCategoryDescription.AsSpan(0, Constants.HSExtensionCodesConstants.DescriptionMaxLength - formattedClassificationType.Length), formattedClassificationType);
			}
			else
			{
				result = subCategoryDescription + formattedClassificationType;
			}
			return result;
		}
		static SubCategoryItems GetSubCategory(MappingEntityType entityType, IRow row)
		{
			var subCategoryCode = string.Empty;
			var classificationTypeCode = string.Empty;
			var subCategoryDescription = string.Empty;
			var classificationType = string.Empty;
			foreach (var property in entityType.Properties)
			{
				var cell = row.GetCell(property.ExcelColumn);
				cell?.SetCellType(CellType.String);
				var cellValue = cell?.StringCellValue ?? string.Empty;

				if (property.IsNonPersistent)
				{
					switch (property.Name)
					{
						case NonPersistentNames.SubCategoryCode:
							subCategoryCode = cellValue;
							break;
						case NonPersistentNames.ClassificationTypeCode:
							classificationTypeCode = cellValue;
							break;
						case NonPersistentNames.SubCategoryDescription:
							subCategoryDescription = cellValue;
							break;
						case NonPersistentNames.ClassificationType:
							classificationType = cellValue;
							break;
					}
				}
			}
			return new SubCategoryItems { SubCategoryCode = subCategoryCode, ClassificationTypeCode = classificationTypeCode, SubCategoryDescription = subCategoryDescription, ClassificationType = classificationType };
		}

		static class NonPersistentNames
		{
			public const string SubCategoryCode = "SubCategoryCode";
			public const string ClassificationTypeCode = "ClassificationTypeCode";
			public const string SubCategoryDescription = "SubCategoryDescription";
			public const string ClassificationType = "ClassificationType";
		}

		struct SubCategoryItems
		{
			public string SubCategoryCode;
			public string ClassificationTypeCode;
			public string SubCategoryDescription;
			public string ClassificationType;
		}

		bool IAdditionalDataUpdater<RefCusTariff>.IsDataRowValid(IRow row, EntityConfiguration configuration) => IsDataRowValid(row, configuration);
		void IAdditionalDataUpdater<RefCusTariff>.UpdateAdditionally(RefCusTariff dataEntity, IRow row, EntityConfiguration configuration) => UpdateAdditionally(dataEntity, row, configuration);
		void IAdditionalDataUpdater<RefCusTariff>.RegisterUpdated(IRow row, EntityConfiguration configuration) { }
		void IAdditionalDataUpdater<RefCusTariff>.UpdateRule(RefCusTariff dataEntity, Rule rule) { }
	}
}
