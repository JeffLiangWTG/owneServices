using System;
using System.Linq;
using System.Text;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;
using CargoWise.RefDbRepo.SharedReferenceData.Services.Common;
using CargoWise.RefDbRepo.SharedReferenceData.Services.Tariff.Models;

namespace CargoWise.RefDbRepo.GBReferenceData.Business.Tariff
{
	public class EUNVATBuilder : TariffBuilder
	{
		public EUNVATBuilder(IDateTimeProvider dateTimeProvider, StringBuilder errorCollector) : base(dateTimeProvider, errorCollector)
		{
		}

		protected override string FilePrefix => "GB_RefCusTariff_EUN_VAT";
		protected override string XMLWriterDataSource => "EUN GB VAT Tariff for NI";

		protected override XmlWriterConfiguration XmlWriterConfiguration()
		{
			var writerConfig = new XmlWriterConfiguration();
			var tariffConfig = new EntityTypeConfiguration<RefCusTariff>(false);

			tariffConfig.IncludeColumn(x => x.ZZ1_TariffCode, true);
			tariffConfig.IncludeColumn(x => x.ZZ1_ZZI_NKTariffType, true);
			tariffConfig.IncludeColumn(x => x.ZZ1_IAMUnique, true);
			tariffConfig.IncludeColumnWithConstantValue(x => x.ZZ1_ZZI_ZZZ_NKDataGrouping, true, Constants.DefaultValues.EUNDataGrouping);
			tariffConfig.IncludeColumnWithConstantValue(x => x.ZZ1_ZZZ_NKDataGrouping, true, Constants.DefaultValues.EUNDataGrouping);
			tariffConfig.IncludeColumn(x => x.RefCusVATApplicabilities);

			writerConfig.IncludeEntityTypeConfiguration(tariffConfig);

			var vatApplConfig = new EntityTypeConfiguration<RefCusVATApplicability>(true);
			vatApplConfig.IncludeColumnWithConstantValue(x => x.ZX5_ZZZ_NKDataGrouping, true, Constants.DefaultValues.GBDataGrouping);
			vatApplConfig.IncludeColumn(x => x.ZX5_ZZF_NKTaxOrFeeCode, true);
			vatApplConfig.IncludeColumn(x => x.ZX5_AdditionalCode, true);
			vatApplConfig.IncludeColumn(x => x.ZX5_StartDate);
			vatApplConfig.IncludeColumn(x => x.ZX5_EndDate);

			writerConfig.IncludeEntityTypeConfiguration(vatApplConfig);

			return writerConfig;
		}

		protected override void AddRefCusConditions(RefCusTariff tariff, string tariffType, Measure measure, RefCusApplicability newRefCusApplicability) { }
		protected override void AddRefCusRate(RefCusTariff tariff, Measure measure, RefCusApplicability newRefCusApplicability) { }
		protected override void AddRefCusTariffUOMs(RefCusTariff tariff, Measure measure, bool isActualMeasure) { }

		protected override Predicate<RefCusTariff> TariffFilter => (tariff) => tariff?.RefCusVATApplicabilities?.Any() ?? false;
	}
}
