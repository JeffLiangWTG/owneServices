using System.Text;
using CargoWise.RefDbRepo.SharedReferenceData.Business.Tariff;
using CargoWise.RefDbRepo.SharedReferenceData.Services.Common;

namespace CargoWise.RefDbRepo.GBReferenceData.Business.Tariff
{
	public class TariffBuilder : TariffBuilderBase
	{
		public TariffBuilder(IDateTimeProvider dateTimeProvider, StringBuilder errorCollector) : base(dateTimeProvider, errorCollector)
		{
		}

		protected override string FilePrefix => "GB_RefCusTariff";
		protected override string XMLWriterDataSource => "GB Tariff";

		protected override string DataGrouping => Constants.DefaultValues.GBDataGrouping;
	}
}
