using CargoWise.RefDbRepo.SharedReferenceData.Business.Tariff;
using CargoWise.RefDbRepo.SharedReferenceData.Business.Tariff.Processors;
using CargoWise.RefDbRepo.SharedReferenceData.Services.Common;

namespace CargoWise.RefDbRepo.BEReferenceData.Business.Tariff
{
	public class BEMeasureProcessor : MeasureProcessor
	{
		public BEMeasureProcessor(IDateTimeProvider dateTimeProvider, IMeasureMappingProvider measureMappingProvider, IRefXmlBuilder[] builders) : base(dateTimeProvider, measureMappingProvider, builders)
		{
		}

		protected override bool ProcessConfiguredMeasureTypesOnly => true;
	}
}
