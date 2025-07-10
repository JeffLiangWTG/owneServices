using System.Collections.Generic;
using System.Linq;
using System.Text;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;
using CargoWise.RefDbRepo.NLReferenceData.Services;
using CargoWise.RefDbRepo.SharedReferenceData.Services.Common;

namespace CargoWise.RefDbRepo.NLReferenceData.Business
{
	public class AdditionalInformationBuilder : RefCusCodeListBuilder<TableElement>, ICodebookBuilder
	{
		public AdditionalInformationBuilder(StringBuilder errorCollector, IDateTimeProvider dateTimeProvider = null) : base(errorCollector)
		{
			DateTimeProvider = dateTimeProvider ?? new DateTimeProvider(0);
		}
		public IDateTimeProvider DateTimeProvider { get; }

		protected override StringBuilder IsValidCore(RefCusCodeList refModel) => new StringBuilder();

		protected override IEnumerable<RefCusCodeList> ConvertToRefModels(IEnumerable<TableElement> data)
		{
			var results = new List<RefCusCodeList>();
			var headerLevel = CreateAttribute(Constants.AdditionalInformationDefaults.AttributeNames.Level, Constants.AttributeValues.HeaderLevel);
			var itemLevel = CreateAttribute(Constants.AdditionalInformationDefaults.AttributeNames.Level, Constants.AttributeValues.ItemLevel);
			var description = CreateAttribute(Constants.AdditionalInformationDefaults.AttributeNames.Description, Constants.AttributeValues.Yes);

			foreach (var elem in data)
			{
				results.Add(CreateRefCusCodeList(elem, Constants.AdditionalInformationDefaults.ImportCodeType, headerLevel, itemLevel, description));
				results.Add(CreateRefCusCodeList(elem, Constants.AdditionalInformationDefaults.ExportCodeType, headerLevel, itemLevel, description));
			}

			return results;
		}

		protected override XmlWriterConfiguration XmlWriterConfiguration()
		{
			var writerConfig = new XmlWriterConfiguration();
			var entityConfig = new EntityTypeConfiguration<RefCusCodeList>(true);

			entityConfig.IncludeColumn(x => x.ZZD_Code, true);
			entityConfig.IncludeColumn(x => x.ZZD_Description);
			entityConfig.IncludeColumn(x => x.ZZD_ZZK_NKCodeType, true);
			entityConfig.IncludeColumnWithConstantValue(x => x.ZZD_ZZZ_NKDataGrouping, true, Constants.DefaultValues.NLDataGrouping);
			entityConfig.IncludeColumnWithConstantValue(x => x.ZZD_StartDate, false, Constants.DefaultValues.MinimumDateTime);
			entityConfig.IncludeColumnWithConstantValue(x => x.ZZD_EndDate, false, Constants.DefaultValues.MaximumDateTime);
			entityConfig.IncludeColumn(x => x.RefCusCodeListAttributes);

			writerConfig.IncludeEntityTypeConfiguration(entityConfig);

			var attribConfig = new EntityTypeConfiguration<RefCusCodeListAttribute>(true);
			attribConfig.IncludeColumn(x => x.ZZE_ZXE_NKName, true);
			attribConfig.IncludeColumn(x => x.ZZE_Value, true);

			writerConfig.IncludeEntityTypeConfiguration(attribConfig);

			return writerConfig;
		}

		protected override string XMLWriterDataSource => "Additional Information";

		protected override IEnumerable<TableElement> OrderList(IList<TableElement> data)
		{
			return data.OrderBy(x => x.elementCode);
		}
	}
}
