using System.Collections.Generic;
using System.Linq;
using System.Text;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;
using CargoWise.RefDbRepo.SharedReferenceData.Services.Common;
using CargoWise.RefDbRepo.SharedReferenceData.Services.Tariff.Models;
using static System.FormattableString;

namespace CargoWise.RefDbRepo.SharedReferenceData.Business.Tariff
{
	public abstract class GoodsNomenclatureBuilderBase : BuilderBase<RefCusNomenclatureGroup>
	{
		protected GoodsNomenclatureBuilderBase(IDateTimeProvider dateTimeProvider, StringBuilder errorCollector) : base(dateTimeProvider, errorCollector)
		{
		}

		protected override IEnumerable<RefCusNomenclatureGroup> ConvertToRefModels(List<ITariffModel> data)
		{
			var models = data.Cast<GoodsNomenclature>()
				.Where(x => !(x.IsForMeasure ?? false) || (x.IsForNomenclature ?? false))
				.ToList();

			return models.Select(x => ConvertToRefModel(x));
		}

		RefCusNomenclatureGroup ConvertToRefModel(GoodsNomenclature model)
		{
			return new RefCusNomenclatureGroup()
			{
				ZZ5_Value = model.CleanId,
				ZZ5_Description = model.Description,
				ZZ5_StartDate = model.CalcStartDate,
				ZZ5_EndDate = model.CalcEndDate,
				ZZ5_CompositeKey = model.Key,
				ZZ5_ZZ9_NKNomenclatureGroupType = DataGrouping,
				ZZ5_ZZZ_NKDataGrouping = DataGrouping
			};
		}

		protected override XmlWriterConfiguration XmlWriterConfiguration()
		{
			var writerConfig = new XmlWriterConfiguration();
			var entityConfig = new EntityTypeConfiguration<RefCusNomenclatureGroup>(true);

			entityConfig.IncludeColumn(x => x.ZZ5_Value);
			entityConfig.IncludeColumn(x => x.ZZ5_CompositeKey, true);
			entityConfig.IncludeColumn(x => x.ZZ5_Description);
			entityConfig.IncludeColumn(x => x.ZZ5_EndDate);
			entityConfig.IncludeColumn(x => x.ZZ5_StartDate);
			entityConfig.IncludeColumnWithConstantValue(x => x.ZZ5_ZZ9_NKNomenclatureGroupType, true, DataGrouping);
			entityConfig.IncludeColumnWithConstantValue(x => x.ZZ5_ZZZ_NKDataGrouping, true, DataGrouping);

			writerConfig.IncludeEntityTypeConfiguration(entityConfig);

			return writerConfig;
		}

		protected override bool IsValid(RefCusNomenclatureGroup refModel, string chapterFilter)
		{
			bool valid = true;

			var validationErrors = new StringBuilder();

			if (string.IsNullOrWhiteSpace(refModel.ZZ5_Value))
			{
				validationErrors.Append("ZZ5_Value is required. ");
				valid = false;
			}

			if (string.IsNullOrWhiteSpace(refModel.ZZ5_Description))
			{
				validationErrors.Append("ZZ5_Description is required. ");
				valid = false;
			}

			if (!valid)
			{
				var msg = Invariant($"GoodsNomenclature validation error. Key: '{refModel.ZZ5_CompositeKey}' Errors: '{validationErrors}' Filter: '{chapterFilter}'");
				ErrorCollector.AppendLine(msg);
			}

			return valid;
		}

		protected override bool IsExpired(RefCusNomenclatureGroup refModel) => refModel.ZZ5_EndDate.Date < dateTimeProvider.UTCHistoricalDate;

		protected override void DuplicateError(RefCusNomenclatureGroup refModel, string uniqueId, string chapterFilter)
		{
			var msg = Invariant($"GoodsNomenclature duplicate exists. Key: '{uniqueId}' Filter: '{chapterFilter}'");
			ErrorCollector.AppendLine(msg);
		}

		protected override string UniqueId(RefCusNomenclatureGroup refModel) => $"{refModel.ZZ5_CompositeKey}_{refModel.ZZ5_StartDate:yyyyMMddHHmmss}_{refModel.ZZ5_EndDate:yyyyMMddHHmmss}";

		protected abstract string DataGrouping { get; }
	}
}
