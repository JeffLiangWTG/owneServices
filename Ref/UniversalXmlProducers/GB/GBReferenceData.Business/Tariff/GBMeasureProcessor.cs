using CargoWise.RefDbRepo.SharedReferenceData.Business.Tariff;
using CargoWise.RefDbRepo.SharedReferenceData.Business.Tariff.Processors;
using CargoWise.RefDbRepo.SharedReferenceData.Services.Common;

namespace CargoWise.RefDbRepo.GBReferenceData.Business.Tariff
{
	public class GBMeasureProcessor : MeasureProcessor
	{
		public GBMeasureProcessor(IDateTimeProvider dateTimeProvider, IMeasureMappingProvider measureMappingProvider, IRefXmlBuilder[] builders) : base(dateTimeProvider, measureMappingProvider, builders)
		{
		}

		protected override MeasureHelper GetNewMeasureHelper() => new GBMeasureHelper();
	}
}
