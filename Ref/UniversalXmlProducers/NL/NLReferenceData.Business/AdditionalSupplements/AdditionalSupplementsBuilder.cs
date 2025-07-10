using System.Collections.Generic;
using System.Linq;
using System.Text;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;
using CargoWise.RefDbRepo.NLReferenceData.Services;

namespace CargoWise.RefDbRepo.NLReferenceData.Business
{
	public class AdditionalSupplementsBuilder : RefCusCodeListBuilder<AdditionalSupplementsData>
	{
		public AdditionalSupplementsBuilder(StringBuilder errorCollector) : base(errorCollector)
		{
		}

		protected override string XMLWriterDataSource => "Additional Supplements";

		protected override IEnumerable<RefCusCodeList> ConvertToRefModels(IEnumerable<AdditionalSupplementsData> data)
		{
			var results = new List<RefCusCodeList>();

			foreach (var ad in data)
			{
				var newRefCode = new RefCusCodeList
				{
					ZZD_Code = ad.Code,
					ZZD_Description = ad.Description,
					ZZD_ZZZ_NKDataGrouping = Constants.DefaultValues.NLDataGrouping,
					ZZD_ZZK_NKCodeType = Constants.AdditionalSupplementsDefaults.CodeType,
					ZZD_StartDate = Constants.DefaultValues.MinimumDateTime,
					ZZD_EndDate = Constants.DefaultValues.MaximumDateTime
				};
				results.Add(newRefCode);
			}
			return results;
		}

		protected override StringBuilder IsValidCore(RefCusCodeList refModel) => new StringBuilder();

		protected override XmlWriterConfiguration XmlWriterConfiguration()
		{
			var writerConfig = new XmlWriterConfiguration();
			var entityConfig = new EntityTypeConfiguration<RefCusCodeList>(true);

			entityConfig.IncludeColumn(x => x.ZZD_Code, true);
			entityConfig.IncludeColumn(x => x.ZZD_Description);
			entityConfig.IncludeColumnWithConstantValue(x => x.ZZD_ZZK_NKCodeType, true, Constants.AdditionalSupplementsDefaults.CodeType);
			entityConfig.IncludeColumnWithConstantValue(x => x.ZZD_ZZZ_NKDataGrouping, true, Constants.DefaultValues.NLDataGrouping);
			entityConfig.IncludeColumnWithConstantValue(x => x.ZZD_StartDate, false, Constants.DefaultValues.MinimumDateTime);
			entityConfig.IncludeColumnWithConstantValue(x => x.ZZD_EndDate, false, Constants.DefaultValues.MaximumDateTime);
			writerConfig.IncludeEntityTypeConfiguration(entityConfig);
			return writerConfig;
		}

		protected override IEnumerable<AdditionalSupplementsData> OrderList(IList<AdditionalSupplementsData> data)
		{
			return data.OrderBy(x => x.Code);
		}
	}
}
