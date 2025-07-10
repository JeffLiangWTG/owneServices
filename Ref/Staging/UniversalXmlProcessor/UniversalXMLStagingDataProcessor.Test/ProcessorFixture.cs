using System;
using CargoWise.RefDbRepo.Staging.Schema_New;
using CargoWise.RefDbRepo.XmlProducer.Common;
using Moq;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.Staging.UniversalXMLStagingDataProcessor.Test
{
	[TestFixture]
	class ProcessorFixture
	{
		[Test]
		public void WriteLogWheneverAnExceptionIsThrownOnTheRootLevelOfProcess()
		{
			var safeDataProvider = new Mock<ISafeDataProvider>();
			var merger = new Mock<ISafeObjectUpdater>();
			var serviceFactory = new Mock<IServiceFactory>();
			var wrapper = new Mock<IStagingDataWrapperFactory>();
			var cache = new Mock<ICacheProvider>().Object;
			var metadataProvider = new Mock<IMetadataProvider>();
			var odataUriProvider = new Mock<IOdataUriProvider>();

			var stagingDataProviderFromServiceFactory = new Mock<IStagingDataProvider>();
			serviceFactory.Setup(x => x.GetStagingDataProvider(false, It.IsAny<int?>())).Returns(stagingDataProviderFromServiceFactory.Object);
			serviceFactory.Setup(x => x.GetSafeDataProvider(cache)).Returns(safeDataProvider.Object);
			serviceFactory.Setup(x => x.GetStagingDataWrapperFactory(It.IsAny<ISafeDataProvider>(),
				It.IsAny<IMetadataProvider>(), It.IsAny<IStagingDataProvider>())).Returns(wrapper.Object);
			serviceFactory.Setup(x => x.GetSafeObjectUpdater(It.IsAny<ISafeDataProvider>(), metadataProvider.Object, false)).Returns(merger.Object);

			var readOnlyStagingDataProvider = new Mock<IStagingDataProvider>();
			var exception = new Exception("Error");
			readOnlyStagingDataProvider.Setup(x => x.GetNextDataBatch(It.IsAny<SourceData>(), It.IsAny<IMetadataProvider>(), It.IsAny<int?>(), null))
				.Throws(exception);

			var dataSourceProvider = new Mock<IDataSourceProvider>();
			var sourceData = new SourceData { SDA_SourceTime = DateTime.UtcNow };
			var processor = new Processor(sourceData, serviceFactory.Object, stagingDataProviderFromServiceFactory.Object, readOnlyStagingDataProvider.Object,
				dataSourceProvider.Object, cache, metadataProvider.Object, odataUriProvider.Object);

			Assert.That(async () => await processor.DoProcess(), Is.EqualTo(ProducerStatus.MergeFailure));
			stagingDataProviderFromServiceFactory.Verify(x => x.SaveChangesAsync(), Times.Exactly(1));
			Assert.That(sourceData.IsSourceDataErrorStatus());
		}
	}
}
