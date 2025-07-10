using System.Collections.Generic;
using System.Linq;
using System.Text;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;
using CargoWise.RefDbRepo.SharedReferenceData.Business.Tariff;
using CargoWise.RefDbRepo.SharedReferenceData.Services.Common;
using CargoWise.RefDbRepo.SharedReferenceData.Services.Tariff.Models;
using CargoWise.RefDbRepo.UniversalXMLProducers.EUNTariffDataProducer;

namespace CargoWise.RefDbRepo.BEReferenceData.Business
{
	public class TariffBuilder : TariffBuilderBase
	{
		public TariffBuilder(IDateTimeProvider dateTimeProvider, StringBuilder errorCollector) : base(dateTimeProvider, errorCollector)
		{
		}

		protected override string FilePrefix => "BE_RefCusTariff";

		protected override string XMLWriterDataSource => "BE Tariff";

		protected override string DataGrouping => Constants.Common.LocalCountryCode;

		protected override string ParentDataGrouping => Constants.Common.EUNCountryCode;

		protected override bool IsEntityTypeConfigurationRefCusTariffEnableNullOrEmptyKeyMatching => false;

		protected override IEnumerable<RefCusTariff> ConvertToRefModels(List<ITariffModel> data)
		{
			var baseData = base.ConvertToRefModels(data);
			return MergeImportTariffsWithEUNTarrifs(baseData.ToList());
		}

		protected virtual List<RefCusTariff> MergeImportTariffsWithEUNTarrifs(List<RefCusTariff> refCusTariffData)
		{
			return tariffExpander.MergeImportTariffsWithEUNTarrifs(refCusTariffData, false);
		}

		protected override string DefaultTariffType => Constants.TariffTypes.Import;

		EUNTariffExpander tariffExpander => tariffExpanderCached ?? (tariffExpanderCached = new EUNTariffExpander());
		EUNTariffExpander tariffExpanderCached;
	}
}
