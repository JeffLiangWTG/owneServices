using System;
using System.Linq;
using CargoWise.RefDbRepo.Common.Argument;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;
using CargoWise.RefDbRepo.TRReferenceData.Services;
using CargoWise.RefDbRepo.TRReferenceData.Services.Loaders;

namespace CargoWise.RefDbRepo.TRReferenceData.Business.DutyCodesParser
{
	public class DutyCodesParser : ReferenceDataParser
	{
		readonly IDateTimeProvider dateTimeProvider;
		readonly string dataFileName;

		public DutyCodesParser(IDateTimeProvider dateTimeProvider, string dataFileName)
		{
			this.dateTimeProvider = Argument.NotNull(dateTimeProvider, nameof(dateTimeProvider));
			this.dataFileName = Argument.NotNull(dataFileName, nameof(dataFileName));
		}

		protected override string DataSource => "TR Duty Codes";

		protected override DateTime PublicationDateTime => dateTimeProvider.GetNow();

		protected override RefDataRepoModelEntityType[] GetEntities()
		{
			return DutyCodesLoader.LoadData(dataFileName).ToArray();
		}

		protected override XmlWriterConfiguration GetXmlWriterConfiguration()
		{
			var writerConfiguration = new XmlWriterConfiguration();

			var refCusRateType = new EntityTypeConfiguration<RefCusRateType>(false);
			refCusRateType.IncludeColumn(x => x.ZZR_RateType, true);
			refCusRateType.IncludeColumnWithConstantValue(x => x.ZZR_ZZZ_NKDataGrouping, true, Constants.CountryCodeTR);
			refCusRateType.IncludeColumn(x => x.RefCusRateCodes);
			writerConfiguration.IncludeEntityTypeConfiguration(refCusRateType);

			var refCusRateCode = new EntityTypeConfiguration<RefCusRateCode>(true);
			refCusRateCode.IncludeColumn(x => x.ZY1_RateCode, true);
			refCusRateCode.IncludeColumn(x => x.ZY1_Description);
			refCusRateCode.IncludeColumnWithConstantValue(x => x.ZY1_ZZZ_NKDataGrouping, true, Constants.CountryCodeTR);
			refCusRateCode.IncludeColumnWithConstantValue(x => x.ZY1_InternalUse, false, "0");
			refCusRateCode.IncludeColumn(x => x.RefCusRateCodeLanguages);
			writerConfiguration.IncludeEntityTypeConfiguration(refCusRateCode);

			var refCusRateCodeLanguage = new EntityTypeConfiguration<RefCusRateCodeLanguage>(true);
			refCusRateCodeLanguage.IncludeColumnWithConstantValue(x => x.ZXC_ZX6_NKLanguage, true, Constants.CountryCodeTR);
			refCusRateCodeLanguage.IncludeColumn(x => x.ZXC_Description);
			writerConfiguration.IncludeEntityTypeConfiguration(refCusRateCodeLanguage);

			return writerConfiguration;
		}
	}
}
