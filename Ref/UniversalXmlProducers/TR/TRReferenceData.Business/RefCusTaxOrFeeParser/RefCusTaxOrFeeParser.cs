using System;
using System.Linq;
using CargoWise.RefDbRepo.Common.Argument;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;
using CargoWise.RefDbRepo.TRReferenceData.Services;
using CargoWise.RefDbRepo.TRReferenceData.Services.Loaders;

namespace CargoWise.RefDbRepo.TRReferenceData.Business.RefCusTaxOrFeeParser
{
	public class RefCusTaxOrFeeParser : ReferenceDataParser
	{
		readonly IDateTimeProvider dateTimeProvider;
		readonly string dataFileName;

		public RefCusTaxOrFeeParser(IDateTimeProvider dateTimeProvider, string dataFileName)
		{
			this.dateTimeProvider = Argument.NotNull(dateTimeProvider, nameof(dateTimeProvider));
			this.dataFileName = Argument.NotNull(dataFileName, nameof(dataFileName));
		}

		protected override string DataSource => "TR Stamp Duty";

		protected override DateTime PublicationDateTime => new DateTime(2025, 1, 1);

		protected override RefDataRepoModelEntityType[] GetEntities()
		{
			return RefCusTaxOrFeeLoader.LoadData(dataFileName).ToArray();
		}

		protected override XmlWriterConfiguration GetXmlWriterConfiguration()
		{
			var writerConfiguration = new XmlWriterConfiguration();
			var refCusTaxOrFeeConfiguration = new EntityTypeConfiguration<RefCusTaxOrFee>(true);

			refCusTaxOrFeeConfiguration.IncludeColumn(x => x.ZZF_Code, true);
			refCusTaxOrFeeConfiguration.IncludeColumn(x => x.ZZF_Description, false);
			refCusTaxOrFeeConfiguration.IncludeColumn(x => x.ZZF_Value, false);
			refCusTaxOrFeeConfiguration.IncludeColumn(x => x.ZZF_StartDate, true);
			refCusTaxOrFeeConfiguration.IncludeColumn(x => x.ZZF_EndDate, false);
			refCusTaxOrFeeConfiguration.IncludeColumn(x => x.ZZF_ZZZ_NKDataGrouping, true);
			refCusTaxOrFeeConfiguration.IncludeColumn(x => x.ZZF_ZX0_NKTaxOrFeeType, true);
			refCusTaxOrFeeConfiguration.IncludeColumnWithConstantValue(x => x.ZZF_Minimum, false, 0.0m);
			refCusTaxOrFeeConfiguration.IncludeColumnWithConstantValue(x => x.ZZF_Maximum, false, 0.0m);
			refCusTaxOrFeeConfiguration.IncludeColumnWithConstantValue(x => x.ZZF_Threshold, false, 0.0m);
			refCusTaxOrFeeConfiguration.IncludeColumn(x => x.RefCusTaxOrFeeLanguages);
			writerConfiguration.IncludeEntityTypeConfiguration(refCusTaxOrFeeConfiguration);

			var refCusTaxOrFeeLanguage = new EntityTypeConfiguration<RefCusTaxOrFeeLanguage>(true);
			refCusTaxOrFeeLanguage.IncludeColumn(x => x.ZXU_ZX6_NKLanguage, true);
			refCusTaxOrFeeLanguage.IncludeColumn(x => x.ZXU_Description);
			writerConfiguration.IncludeEntityTypeConfiguration(refCusTaxOrFeeLanguage);

			return writerConfiguration;
		}
	}
}
