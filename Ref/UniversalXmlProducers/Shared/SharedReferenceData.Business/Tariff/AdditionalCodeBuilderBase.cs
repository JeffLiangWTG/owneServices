using System.Collections.Generic;
using System.Linq;
using System.Text;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;
using CargoWise.RefDbRepo.SharedReferenceData.Services.Common;
using CargoWise.RefDbRepo.SharedReferenceData.Services.Tariff.Models;
using static System.FormattableString;
using static CargoWise.RefDbRepo.SharedReferenceData.Services.Tariff.Models.AdditionalCode;

namespace CargoWise.RefDbRepo.SharedReferenceData.Business.Tariff
{
	public abstract class AdditionalCodeBuilderBase : BuilderBase<RefCusCodeList>
	{
		protected AdditionalCodeBuilderBase(IDateTimeProvider dateTimeProvider, StringBuilder errorCollector) : base(dateTimeProvider, errorCollector)
		{
		}

		protected override void DuplicateError(RefCusCodeList refModel, string uniqueId, string chapterFilter)
		{
			var msg = Invariant($"RefCusCodeList duplicate exists. Key: '{uniqueId}' Description: {refModel.ZZD_Description}");
			ErrorCollector.AppendLine(msg);
		}

		protected override bool IsExpired(RefCusCodeList refModel) => refModel.ZZD_EndDate.Date < dateTimeProvider.UTCHistoricalDate;

		protected override bool IsValid(RefCusCodeList refModel, string chapterFilter)
		{
			bool valid = true;

			var validationErrors = new StringBuilder();

			if (string.IsNullOrWhiteSpace(refModel.ZZD_Code))
			{
				validationErrors.Append("ZZD_Code is required. ");
				valid = false;
			}

			if (string.IsNullOrWhiteSpace(refModel.ZZD_Description))
			{
				validationErrors.Append("ZZD_Description is required. ");
				valid = false;
			}

			if (!valid)
			{
				var msg = Invariant($"RefCusCodeList validation error. Key: '{refModel.ZZD_Code}' Errors: '{validationErrors}'");
				ErrorCollector.AppendLine(msg);
			}

			return valid;
		}

		protected override string UniqueId(RefCusCodeList refModel) => $"{refModel.ZZD_Code}_{refModel.ZZD_StartDate:yyyyMMddHHmmss}_{refModel.ZZD_EndDate:yyyyMMddHHmmss}";

		protected override XmlWriterConfiguration XmlWriterConfiguration()
		{
			var writerConfig = new XmlWriterConfiguration();
			var entityConfig = new EntityTypeConfiguration<RefCusCodeList>(true);

			entityConfig.IncludeColumn(x => x.ZZD_Code, true);
			entityConfig.IncludeColumn(x => x.ZZD_Description);
			entityConfig.IncludeColumnWithConstantValue(x => x.ZZD_ZZK_NKCodeType, true, "ADDCD");
			entityConfig.IncludeColumnWithConstantValue(x => x.ZZD_ZZZ_NKDataGrouping, true, DataGrouping);
			entityConfig.IncludeColumn(x => x.ZZD_StartDate);
			entityConfig.IncludeColumn(x => x.ZZD_EndDate);

			writerConfig.IncludeEntityTypeConfiguration(entityConfig);

			if (SupportsMultipleLanguages)
			{
				entityConfig.IncludeColumn(x => x.RefCusCodeListLanguages);
				var languageConfig = new EntityTypeConfiguration<RefCusCodeListLanguage>(true);
				languageConfig.IncludeColumn(x => x.ZXA_ZX6_NKLanguage, true);
				languageConfig.IncludeColumn(x => x.ZXA_Description);

				writerConfig.IncludeEntityTypeConfiguration(languageConfig);
			}

			return writerConfig;
		}

		protected override IEnumerable<RefCusCodeList> ConvertToRefModels(List<ITariffModel> data)
		{
			var models = data.Cast<AdditionalCode>()
				.ToList();

			return models.Select(x => ConvertToRefModel(x));
		}

		RefCusCodeList ConvertToRefModel(AdditionalCode ac)
		{
			var result = new RefCusCodeList
			{
				ZZD_Code = $"{ac.CodeType}{ac.Code}",
				ZZD_Description = ac.Description.Length > MaxDescriptionLength ? ac.Description.Substring(0, MaxDescriptionLength) : ac.Description,
				ZZD_StartDate = CommonHelper.CalcMinDate(ac.StartDate),
				ZZD_EndDate = CommonHelper.CalcMaxDate(ac.EndDate)
			};

			if (SupportsMultipleLanguages && (ac.Descriptions?.Any() ?? false))
			{
				result.RefCusCodeListLanguages = CreateDescriptions(ac.Descriptions);
			}

			return result;
		}

		static RefCusCodeListLanguage[] CreateDescriptions(IEnumerable<DescriptionPeriods.DescriptionModel> descriptions)
		{
			var languageList = new List<RefCusCodeListLanguage>();

			foreach (var d in descriptions)
			{
				var description = languageList.FirstOrDefault(x => x.ZXA_ZX6_NKLanguage == d.LanguageCode);
				if (description == null)
				{
					languageList.Add(new RefCusCodeListLanguage
					{
						ZXA_ZX6_NKLanguage = d.LanguageCode,
						ZXA_Description = d.Description,
					});
				}
			}

			return languageList.ToArray();
		}

		protected abstract string DataGrouping { get; }

		const int MaxDescriptionLength = 2000;


		protected virtual bool SupportsMultipleLanguages => false;
	}
}
