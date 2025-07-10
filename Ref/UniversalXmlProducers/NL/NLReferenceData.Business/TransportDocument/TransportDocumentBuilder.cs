using System.Collections.Generic;
using System.Linq;
using System.Text;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;
using CargoWise.RefDbRepo.NLReferenceData.Services;
using CargoWise.RefDbRepo.SharedReferenceData.Services.Common;

namespace CargoWise.RefDbRepo.NLReferenceData.Business
{
	public class TransportDocumentBuilder : RefCusCodeListBuilder<TableElement>, ICodebookBuilder
	{
		public TransportDocumentBuilder(StringBuilder errorCollector, IDateTimeProvider dateTimeProvider = null) : base(errorCollector)
		{
			DateTimeProvider = dateTimeProvider ?? new DateTimeProvider(0);
		}
		public IDateTimeProvider DateTimeProvider { get; }

		protected override string XMLWriterDataSource => "Transport Document";

		protected override IEnumerable<RefCusCodeList> ConvertToRefModels(IEnumerable<TableElement> data)
		{
			var results = new List<RefCusCodeList>();
			var headerLevel = CreateAttribute(Constants.TransportDocument.AttributeNames.Level, Constants.AttributeValues.HeaderLevel);
			var itemLevel = CreateAttribute(Constants.TransportDocument.AttributeNames.Level, Constants.AttributeValues.ItemLevel);
			var description = CreateAttribute(Constants.TransportDocument.AttributeNames.Reference, Constants.AttributeValues.Yes);

			foreach (var elem in data)
			{
				results.Add(CreateRefCusCodeList(elem, Constants.TransportDocument.ImportTransportDocument, headerLevel, itemLevel, description));
				results.Add(CreateRefCusCodeList(elem, Constants.TransportDocument.ExportTransportDocument, headerLevel, itemLevel, description));
			}

			return results;
		}

		protected override StringBuilder IsValidCore(RefCusCodeList refModel) => new StringBuilder();

		protected override IEnumerable<TableElement> OrderList(IList<TableElement> data) => data.OrderBy(x => x.elementCode);

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
	}
}
