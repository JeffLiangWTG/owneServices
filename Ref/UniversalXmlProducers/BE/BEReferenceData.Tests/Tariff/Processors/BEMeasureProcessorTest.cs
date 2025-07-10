using System.Text;
using CargoWise.RefDbRepo.BEReferenceData.Business;
using CargoWise.RefDbRepo.BEReferenceData.Business.Tariff;
using CargoWise.RefDbRepo.SharedReferenceData.Business.Tariff;
using CargoWise.RefDbRepo.SharedReferenceData.Business.Tariff.Processors;
using CargoWise.RefDbRepo.SharedReferenceData.Services.Common;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.BEReferenceData.Tests.Tariff.Processors
{
	[TestFixture]
	class BEMeasureProcessorTests
	{
		[Test]
		public void ProcessConfiguredMeasureTypesOnly()
		{
			var dateTimeProvider = new DateTimeProvider(0);
			var errorCollector = new StringBuilder();
			var testClass = new BEMeasureProcessorForTest(dateTimeProvider, new MeasureMappingProvider(), new IRefXmlBuilder[] { new TariffBuilder(dateTimeProvider, errorCollector) });
			Assert.That(testClass.ProcessConfiguredMeasureTypesOnlyExposed, Is.EqualTo(true));
		}
	}

	class BEMeasureProcessorForTest : BEMeasureProcessor
	{
		public BEMeasureProcessorForTest(IDateTimeProvider dateTimeProvider, IMeasureMappingProvider measureMappingProvider, IRefXmlBuilder[] builders) : base(dateTimeProvider, measureMappingProvider, builders)
		{
		}

		public bool ProcessConfiguredMeasureTypesOnlyExposed() => base.ProcessConfiguredMeasureTypesOnly;
	}
}
