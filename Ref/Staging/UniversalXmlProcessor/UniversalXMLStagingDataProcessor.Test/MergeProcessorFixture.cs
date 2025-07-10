using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using CargoWise.RefDbRepo.Common.Infrastructure.Test;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter;
using CargoWise.RefDbRepo.Staging.Schema_New;
using Moq;
using NUnit.Framework;
using Safe = CargoWise.RefDbRepo.Common.SafeDataClient.CargoWise.RefDbRepo.Service.Schema_0_9_New;
using Stage = CargoWise.RefDbRepo.Staging.Schema_New;

namespace CargoWise.RefDbRepo.Staging.UniversalXMLStagingDataProcessor.Test
{
	[TestFixture]
	[Property("DAT:CapabilityRequirements", "SQL2019+")]
	[CreateDatabase("17C71F9054DE408FA5559CBEE1E8AD43", DbSchema.RefDbRepoStaging)]
	class MergeProcessorFixture
	{
		[TestCase(true, false)]
		[TestCase(false, false)]
		[TestCase(true, true)]
		[TestCase(false, true)]
		public async Task SaveUpdateResults(bool mergeResult, bool isPartial)
		{
			var safeProvider = new Mock<ISafeDataProvider>();
			if (!mergeResult)
			{
				safeProvider.Setup(x => x.SaveChangesAsync()).Throws(new Exception());
			}
			safeProvider.Setup(x => x.GetData<Safe.RefCusTariff>(It.IsAny<IStagingDataWrapper[]>(), It.IsAny<IMetadataProvider>())).Returns(new[] { new Safe.RefCusTariff() });
			var dict = new Dictionary<int, Safe.RefCusTariff[]>
			{
				{ 1, new[] { new Safe.RefCusTariff() } }
			};
			safeProvider.Setup(x => x.GetNewestObjectFromList<Safe.RefCusTariff>(It.IsAny<object[]>(), It.IsAny<IStagingDataWrapper>(), It.IsAny<IMetadataProvider>(), It.IsAny<bool>())).Returns(dict);
			var entities = new[] { Tuple.Create((object)new RefCusTariff(), new DataProcessingInformation()) };
			var metaProvider = new Mock<IMetadataProvider>();
			var batchStagingDataProvider = new Mock<IStagingDataProvider>();
			var serviceFactory = new Mock<IServiceFactory>();
			var wrapperFactory = new Mock<IStagingDataWrapperFactory>();
			var wrapper = new Mock<IStagingDataWrapper>();
			wrapper.Setup(x => x.GetStagingTypeName()).Returns(entities[0].Item1.GetEntityType().Name);
			wrapperFactory.Setup(x => x.CreateWrapper(entities[0].Item1)).Returns(new[] { wrapper.Object });
			serviceFactory.Setup(x => x.GetSafeDataProvider(cache)).Returns(safeProvider.Object);
			serviceFactory.Setup(x => x.GetStagingDataWrapperFactory(It.IsAny<ISafeDataProvider>(),
				It.IsAny<IMetadataProvider>(), It.IsAny<IStagingDataProvider>())).Returns(wrapperFactory.Object);
			serviceFactory.Setup(x => x.GetStagingDataProvider(true, It.IsAny<int?>())).Returns(batchStagingDataProvider.Object);
			var dataProviderWithoutAutoDetect = new Mock<IStagingDataProvider>();
			using var stagingRepo = new StagingRepository(stagingConnectionString);
			using var tran = stagingRepo.BeginTransaction();
			dataProviderWithoutAutoDetect.Setup(x => x.BeginTransaction()).Returns(tran);
			serviceFactory.Setup(x => x.GetStagingDataProvider(false, It.IsAny<int?>())).Returns(dataProviderWithoutAutoDetect.Object);
			var updater = new Mock<ISafeObjectUpdater>();
			var updaterResult1 = new SafeObjectUpdaterResult { ParentPK = Guid.NewGuid(), Action = ResultAction.Insert };
			var updaterResult2 = new SafeObjectUpdaterResult { ParentPK = Guid.NewGuid(), Action = ResultAction.Expire, NewRecordForCloneActionPK = Guid.NewGuid() };
			updater.Setup(x => x.Update(It.IsAny<IStagingDataWrapper[]>())).Returns(new[] { updaterResult1, updaterResult2 });
			serviceFactory.Setup(x => x.GetSafeObjectUpdater(safeProvider.Object, metaProvider.Object, false)).Returns(updater.Object);
			var stagingDataProvider = new Mock<IStagingDataProvider>();
			stagingDataProvider.Setup(x => x.GetNextDataBatch(It.IsAny<SourceData>(), It.IsAny<IMetadataProvider>(), It.IsAny<int?>(), null))
				.Returns(entities);

			var sourceData = new SourceData();
			var repo = new Mock<IStagingRepository>();
			repo.Setup(x => x.Get<SourceData>()).Returns(new[] { sourceData }.AsQueryable());
			batchStagingDataProvider.Setup(x => x.GetSingleSourceData(sourceData.SDA_PK)).Returns(sourceData);
			var processor = new MergeProcessor(stagingDataProvider.Object, serviceFactory.Object, metaProvider.Object, new Mock<IOdataUriProvider>().Object,
					sourceData, cache, 2, 2, isPartial ? UpdateType.Partial : UpdateType.Full);
			await processor.Process(1);

			if (mergeResult)
			{
				dataProviderWithoutAutoDetect.Verify(x => x.SaveUpdateResults(sourceData, new[] { updaterResult1, updaterResult2 }, It.IsAny<IDbTransaction>()));
			}
			else
			{
				dataProviderWithoutAutoDetect.Verify(x => x.SaveErrorUpdateResult(sourceData, It.IsAny<SafeObjectUpdaterResult>()));
			}

			dataProviderWithoutAutoDetect.Verify(x => x.CleanUpOldUpdateResultsAsync(sourceData), Times.Exactly(2));
			Assert.AreEqual(mergeResult ? DataProcessingStatus.PRS.ToString() : DataProcessingStatus.ERR.ToString(), entities[0].Item2.DPI_Status);
			dataProviderWithoutAutoDetect.Verify(x => x.Update(entities[0].Item2));
			if (mergeResult)
			{
				dataProviderWithoutAutoDetect.Verify(x => x.CreateDataProcessingCloneRecords(sourceData.SDA_PK, new[] { updaterResult2 }, It.IsAny<IDbTransaction>()), Times.Once);
			}
		}

		[Test]
		public async Task ProcessInBatch()
		{
			var stagingDataProvider = new Mock<IStagingDataProvider>();
			var batchStagingDataProvider = new Mock<IStagingDataProvider>();
			var dataProviderWithoutAutoDetect = new Mock<IStagingDataProvider>();
			var serviceFactory = new Mock<IServiceFactory>();
			var wrapper = new Mock<IStagingDataWrapperFactory>();
			var entities = new[] { Tuple.Create((object)new Stage.RefCusTariff(), new Stage.DataProcessingInformation()) };
			wrapper.Setup(x => x.CreateWrapper(entities[0].Item1)).Returns(new[] { new Mock<IStagingDataWrapper>().Object });
			serviceFactory.Setup(x => x.GetStagingDataProvider(true, It.IsAny<int?>())).Returns(batchStagingDataProvider.Object);
			serviceFactory.Setup(x => x.GetStagingDataProvider(false, It.IsAny<int?>())).Returns(dataProviderWithoutAutoDetect.Object);
			serviceFactory.Setup(x => x.GetSafeDataProvider(cache)).Returns(new Mock<ISafeDataProvider>().Object);
			serviceFactory.Setup(x => x.GetStagingDataWrapperFactory(It.IsAny<ISafeDataProvider>(),
				It.IsAny<IMetadataProvider>(), It.IsAny<IStagingDataProvider>())).Returns(wrapper.Object);

			stagingDataProvider.Setup(x => x.GetNextDataBatch(It.IsAny<Stage.SourceData>(), It.IsAny<IMetadataProvider>(), It.IsAny<int?>(), null))
				.Returns(entities);
			var sourceData = new Stage.SourceData();
			var repo = new Mock<Stage.IStagingRepository>();
			repo.Setup(x => x.Get<Stage.SourceData>()).Returns(new[] { sourceData }.AsQueryable());
			batchStagingDataProvider.Setup(x => x.GetSingleSourceData(sourceData.SDA_PK)).Returns(sourceData);
			var processor = new MergeProcessor(stagingDataProvider.Object, serviceFactory.Object, new Mock<IMetadataProvider>().Object, new Mock<IOdataUriProvider>().Object,
					sourceData, cache, 2, 1, UpdateType.Partial);
			await processor.Process(1);
			stagingDataProvider.Verify(x => x.GetNextDataBatch(It.IsAny<Stage.SourceData>(), It.IsAny<IMetadataProvider>(),
				It.IsAny<int?>(), entities[0].Item2));
		}

		[Test]
		public async Task LogExceptionOnlyOnFinalTrial()
		{
			var originalErrorWriter = Console.Error;
			try
			{
				using (var errorWriter = new StringWriter())
				{
					Console.SetError(errorWriter);
					var safeProvider = new Mock<ISafeDataProvider>();
					var stagingDataProvider = new Mock<IStagingDataProvider>();
					var batchStagingDataProvider = new Mock<IStagingDataProvider>();
					var dataProviderWithoutAutoDetect = new Mock<IStagingDataProvider>();
					var serviceFactory = new Mock<IServiceFactory>();
					var wrapper = new Mock<IStagingDataWrapper>();
					wrapper.Setup(x => x.GetStagingTypeName()).Returns(nameof(Stage.RefCusTariff));
					var wrapperFactory = new Mock<IStagingDataWrapperFactory>();
					var entities = new[] { Tuple.Create((object)new Stage.RefCusTariff(), new Stage.DataProcessingInformation()) };
					wrapperFactory.Setup(x => x.CreateWrapper(entities[0].Item1)).Returns(new[] { wrapper.Object });
					serviceFactory.Setup(x => x.GetStagingDataProvider(true, It.IsAny<int?>())).Returns(batchStagingDataProvider.Object);
					serviceFactory.Setup(x => x.GetStagingDataProvider(false, It.IsAny<int?>())).Returns(dataProviderWithoutAutoDetect.Object);
					serviceFactory.Setup(x => x.GetSafeDataProvider(cache)).Returns(safeProvider.Object);
					serviceFactory.Setup(x => x.GetStagingDataWrapperFactory(It.IsAny<ISafeDataProvider>(),
						It.IsAny<IMetadataProvider>(), It.IsAny<IStagingDataProvider>())).Returns(wrapperFactory.Object);

					stagingDataProvider.Setup(x => x.GetNextDataBatch(It.IsAny<Stage.SourceData>(), It.IsAny<IMetadataProvider>(), It.IsAny<int?>(), null))
						.Returns(entities);
					var sourceData = new Stage.SourceData();
					var repo = new Mock<Stage.IStagingRepository>();
					repo.Setup(x => x.Get<Stage.SourceData>()).Returns(new[] { sourceData }.AsQueryable());
					batchStagingDataProvider.Setup(x => x.GetSingleSourceData(sourceData.SDA_PK)).Returns(sourceData);
					var updater = new Mock<ISafeObjectUpdater>();
					updater.SetupSequence(x => x.Update(It.IsAny<IStagingDataWrapper[]>()))
						.Throws(new InvalidOperationException("error in the first trial."))
						.Throws(new InvalidOperationException("error in the final trial."));
					serviceFactory.Setup(x => x.GetSafeObjectUpdater(safeProvider.Object, It.IsAny<IMetadataProvider>(), false)).Returns(updater.Object);
					var processor = new MergeProcessor(stagingDataProvider.Object, serviceFactory.Object, new Mock<IMetadataProvider>().Object, new Mock<IOdataUriProvider>().Object,
							sourceData, cache, 2, 1, UpdateType.Partial);
					await processor.Process(10);
					Assert.False(errorWriter.ToString().Contains("error in the first trial."));
					Assert.True(errorWriter.ToString().Contains("error in the final trial."));

					Assert.ThrowsAsync<ArgumentOutOfRangeException>(async () => await processor.Process(20));
				}
			}
			finally
			{
				Console.SetError(originalErrorWriter);
			}
		}

		[Test]
		public async Task RepresentOdataUriWhenFailToMerge()
		{
			var originalErrorWriter = Console.Error;
			try
			{
				using (var errorWriter = new StringWriter())
				{
					Console.SetError(errorWriter);
					var safeProvider = new Mock<ISafeDataProvider>();
					var stagingDataProvider = new Mock<IStagingDataProvider>();
					var batchStagingDataProvider = new Mock<IStagingDataProvider>();
					var dataProviderWithoutAutoDetect = new Mock<IStagingDataProvider>();
					var serviceFactory = new Mock<IServiceFactory>();
					var wrapper = new Mock<IStagingDataWrapper>();
					wrapper.Setup(x => x.GetStagingTypeName()).Returns(nameof(Stage.RefCusTariff));
					safeProvider.Setup(x => x.GetNewestObjectFromList<Safe.RefCusTariff>(It.IsAny<object[]>(), wrapper.Object, It.IsAny<IMetadataProvider>(), false))
						.Returns(new Dictionary<int, Safe.RefCusTariff[]> { { 0, new Safe.RefCusTariff[] { new Safe.RefCusTariff { ZZ1_PK = Guid.NewGuid() } } } });
					var wrapperFactory = new Mock<IStagingDataWrapperFactory>();
					var entities = new[] { Tuple.Create((object)new Stage.RefCusTariff(), new Stage.DataProcessingInformation()) };
					wrapperFactory.Setup(x => x.CreateWrapper(entities[0].Item1)).Returns(new[] { wrapper.Object });
					serviceFactory.Setup(x => x.GetStagingDataProvider(true, It.IsAny<int?>())).Returns(batchStagingDataProvider.Object);
					serviceFactory.Setup(x => x.GetStagingDataProvider(false, It.IsAny<int?>())).Returns(dataProviderWithoutAutoDetect.Object);
					serviceFactory.Setup(x => x.GetSafeDataProvider(cache)).Returns(safeProvider.Object);
					serviceFactory.Setup(x => x.GetStagingDataWrapperFactory(It.IsAny<ISafeDataProvider>(),
						It.IsAny<IMetadataProvider>(), It.IsAny<IStagingDataProvider>())).Returns(wrapperFactory.Object);

					stagingDataProvider.Setup(x => x.GetNextDataBatch(It.IsAny<Stage.SourceData>(), It.IsAny<IMetadataProvider>(), It.IsAny<int?>(), null))
						.Returns(entities);
					var sourceData = new Stage.SourceData();
					var repo = new Mock<Stage.IStagingRepository>();
					repo.Setup(x => x.Get<Stage.SourceData>()).Returns(new[] { sourceData }.AsQueryable());
					batchStagingDataProvider.Setup(x => x.GetSingleSourceData(sourceData.SDA_PK)).Returns(sourceData);
					var updater = new Mock<ISafeObjectUpdater>();
					updater.SetupSequence(x => x.Update(It.IsAny<IStagingDataWrapper[]>()))
						.Throws(new InvalidOperationException("error in the first trial."))
						.Throws(new InvalidOperationException("error in the final trial."));
					serviceFactory.Setup(x => x.GetSafeObjectUpdater(safeProvider.Object, It.IsAny<IMetadataProvider>(), false)).Returns(updater.Object);
					var odataUriProvider = new Mock<IOdataUriProvider>();
					odataUriProvider.Setup(x => x.GetUri(DbSource.RefDbRepoStaging, typeof(Stage.RefCusTariff), It.IsAny<Guid>(), It.IsAny<DateTime>()))
						.Returns(new Uri("http://localhost:17488/odata/RefCusTariffUpdate"));
					odataUriProvider.Setup(x => x.GetUri(DbSource.RefDbRepoSafe, typeof(Stage.RefCusTariff), It.IsAny<Guid>(), It.IsAny<DateTime>()))
						.Returns(new Uri("http://localhost:37016/odata/RefCusTariffUpdate/Default.GetWithOptimizedExpand()"));
					var processor = new MergeProcessor(stagingDataProvider.Object, serviceFactory.Object, new Mock<IMetadataProvider>().Object, odataUriProvider.Object,
							sourceData, cache, 2, 1, UpdateType.Partial);
					await processor.Process(10);
					var errorOutput = errorWriter.ToString();

					Assert.True(errorWriter.ToString().Contains("Stage Odata Uri: http://localhost:17488/odata/RefCusTariffUpdate"));
					Assert.True(errorWriter.ToString().Contains("Safe Odata Uri: http://localhost:37016/odata/RefCusTariffUpdate/Default.GetWithOptimizedExpand()"));
				}
			}
			finally
			{
				Console.SetError(originalErrorWriter);
			}
		}

		[Test]
		public async Task Retry()
		{
			var safeDataProvider = new Mock<ISafeDataProvider>();
			var dataProviderWithoutAutoDetect = new Mock<IStagingDataProvider>();
			safeDataProvider.Setup(x => x.SaveChangesAsync()).Throws(new Exception());
			var merger = new Mock<ISafeObjectUpdater>();
			var serviceFactory = new Mock<IServiceFactory>();
			var wrapper = new Mock<IStagingDataWrapperFactory>();
			var entities = new[] { Tuple.Create((object)new Stage.RefCusTariff(), new Stage.DataProcessingInformation()) };
			wrapper.Setup(x => x.CreateWrapper(entities[0].Item1)).Returns(new[] { new Mock<IStagingDataWrapper>().Object });
			serviceFactory.Setup(x => x.GetStagingDataProvider(false, It.IsAny<int?>())).Returns(dataProviderWithoutAutoDetect.Object);
			serviceFactory.Setup(x => x.GetSafeDataProvider(cache)).Returns(safeDataProvider.Object);
			serviceFactory.Setup(x => x.GetStagingDataWrapperFactory(It.IsAny<ISafeDataProvider>(),
				It.IsAny<IMetadataProvider>(), It.IsAny<IStagingDataProvider>())).Returns(wrapper.Object);
			var metadata = new Mock<IMetadataProvider>().Object;
			var odataUriProvider = new Mock<IOdataUriProvider>();
			serviceFactory.Setup(x => x.GetSafeObjectUpdater(It.IsAny<ISafeDataProvider>(), metadata, false)).Returns(merger.Object);
			var stagingDataProvider = new Mock<IStagingDataProvider>();
			stagingDataProvider.Setup(x => x.GetNextDataBatch(It.IsAny<Stage.SourceData>(), It.IsAny<IMetadataProvider>(), It.IsAny<int?>(), null))
				.Returns(entities);
			var sourceData = new Stage.SourceData();
			var repo = new Mock<Stage.IStagingRepository>();
			repo.Setup(x => x.Get<Stage.SourceData>()).Returns(new[] { sourceData }.AsQueryable());
			var processor = new MergeProcessor(stagingDataProvider.Object, serviceFactory.Object, metadata, odataUriProvider.Object,
					sourceData, cache, 2, 1, UpdateType.Partial);
			await processor.Process(2);
			// 2 times from retry, 1 times from ProcessFinalBatch, 1 time from PopulateDatasetPKForErrorRecords
			wrapper.Verify(x => x.CreateWrapper(entities[0].Item1), Times.Exactly(4));
		}

		[Test]
		public async Task RetryDoNotUseManyTransactions()
		{
			var safeDataProvider = new Mock<ISafeDataProvider>();
			var dataProviderWithoutAutoDetect = new Mock<IStagingDataProvider>();
			safeDataProvider.Setup(x => x.SaveChangesAsync()).Throws(new Exception());
			var merger = new Mock<ISafeObjectUpdater>();
			var serviceFactory = new Mock<IServiceFactory>();
			var wrapper = new Mock<IStagingDataWrapperFactory>();
			var entities = new[] { Tuple.Create((object)new Stage.RefCusTariff(), new Stage.DataProcessingInformation()) };
			wrapper.Setup(x => x.CreateWrapper(entities[0].Item1)).Returns(new[] { new Mock<IStagingDataWrapper>().Object });
			serviceFactory.Setup(x => x.GetStagingDataProvider(false, It.IsAny<int?>())).Returns(dataProviderWithoutAutoDetect.Object);
			serviceFactory.Setup(x => x.GetSafeDataProvider(cache)).Returns(safeDataProvider.Object);
			serviceFactory.Setup(x => x.GetStagingDataWrapperFactory(It.IsAny<ISafeDataProvider>(),
				It.IsAny<IMetadataProvider>(), It.IsAny<IStagingDataProvider>())).Returns(wrapper.Object);
			var metadata = new Mock<IMetadataProvider>().Object;
			var odataUriProvider = new Mock<IOdataUriProvider>();
			serviceFactory.Setup(x => x.GetSafeObjectUpdater(It.IsAny<ISafeDataProvider>(), metadata, false)).Returns(merger.Object);
			var stagingDataProvider = new Mock<IStagingDataProvider>();
			stagingDataProvider.Setup(x => x.GetNextDataBatch(It.IsAny<Stage.SourceData>(), It.IsAny<IMetadataProvider>(), It.IsAny<int?>(), null))
				.Returns(entities);
			var sourceData = new Stage.SourceData();
			var repo = new Mock<Stage.IStagingRepository>();
			repo.Setup(x => x.Get<Stage.SourceData>()).Returns(new[] { sourceData }.AsQueryable());
			var processor = new MergeProcessor(stagingDataProvider.Object, serviceFactory.Object, metadata, odataUriProvider.Object,
					sourceData, cache, 2, 1, UpdateType.Partial);
			await processor.Process(2);

			//2 from ProcessSingleBatch, 1 from PopulateDatasetPKForErrorRecords
			dataProviderWithoutAutoDetect.Verify(x => x.BeginTransaction(), Times.Exactly(3));
		}

		[Test]
		public async Task FinalTrialFindAMatch()
		{
			var safeDataProvider = new Mock<ISafeDataProvider>();
			var dataProviderWithoutAutoDetect = new Mock<IStagingDataProvider>();
			using var stagingRepo = new StagingRepository(stagingConnectionString);
			using var tran = stagingRepo.BeginTransaction();
			dataProviderWithoutAutoDetect.Setup(x => x.BeginTransaction()).Returns(tran);
			var entities = new[] { Tuple.Create((object)new Stage.RefCusTariff(), new Stage.DataProcessingInformation()) };
			safeDataProvider.Setup(x => x.SaveChangesAsync()).Throws(new Exception());
			safeDataProvider.Setup(x => x.GetData<Safe.RefCusTariff>(It.IsAny<IStagingDataWrapper[]>(), It.IsAny<IMetadataProvider>())).Returns(new[] { new Safe.RefCusTariff() });
			var dic = new Dictionary<int, Safe.RefCusTariff[]>();
			dic.Add(1, [new Safe.RefCusTariff()]);
			safeDataProvider.Setup(x => x.GetNewestObjectFromList<Safe.RefCusTariff>(It.IsAny<object[]>(), It.IsAny<IStagingDataWrapper>(), It.IsAny<IMetadataProvider>(), It.IsAny<bool>())).Returns(dic);
			var merger = new Mock<ISafeObjectUpdater>();
			var serviceFactory = new Mock<IServiceFactory>();
			var wrapperFactory = new Mock<IStagingDataWrapperFactory>();
			var wrapper = new Mock<IStagingDataWrapper>();
			wrapper.Setup(x => x.GetStagingTypeName()).Returns(nameof(Stage.RefCusTariff));
			wrapperFactory.Setup(x => x.CreateWrapper(entities[0].Item1)).Returns(new[] { wrapper.Object });
			serviceFactory.Setup(x => x.GetStagingDataProvider(false, It.IsAny<int?>())).Returns(dataProviderWithoutAutoDetect.Object);
			serviceFactory.Setup(x => x.GetSafeDataProvider(cache)).Returns(safeDataProvider.Object);
			serviceFactory.Setup(x => x.GetStagingDataWrapperFactory(It.IsAny<ISafeDataProvider>(),
				It.IsAny<IMetadataProvider>(), It.IsAny<IStagingDataProvider>())).Returns(wrapperFactory.Object);
			var metadata = new Mock<IMetadataProvider>().Object;
			var odataUriProvider = new Mock<IOdataUriProvider>();
			serviceFactory.Setup(x => x.GetSafeObjectUpdater(It.IsAny<ISafeDataProvider>(), metadata, false)).Returns(merger.Object);
			var stagingDataProvider = new Mock<IStagingDataProvider>();
			stagingDataProvider.Setup(x => x.GetNextDataBatch(It.IsAny<Stage.SourceData>(), It.IsAny<IMetadataProvider>(), It.IsAny<int?>(), null))
				.Returns(entities);
			var sourceData = new Stage.SourceData();
			var repo = new Mock<Stage.IStagingRepository>();
			repo.Setup(x => x.Get<Stage.SourceData>()).Returns(new[] { sourceData }.AsQueryable());
			var processor = new MergeProcessor(stagingDataProvider.Object, serviceFactory.Object, metadata, odataUriProvider.Object,
					sourceData, cache, 2, 1, UpdateType.Partial);
			await processor.Process(2);
			dataProviderWithoutAutoDetect.Verify(x => x.SaveErrorUpdateResult(It.IsAny<Stage.SourceData>(), It.IsAny<SafeObjectUpdaterResult>()));
			dataProviderWithoutAutoDetect.Verify(x => x.Update(It.IsAny<Stage.DataProcessingInformation>()), Times.Exactly(4)); // 2 times from retry and 1 time from ProcessFinalBatch, and one last time from populating DPI_HasDPRRecordWhenError
		}

		[Test]
		public async Task ProcessSetAsErrorDataProcessingInformationRecordsWithEmptyParentPK()
		{
			var stagingDataProvider = new Mock<IStagingDataProvider>();
			var batchStagingDataProvider = new Mock<IStagingDataProvider>();
			var dataProviderWithoutAutoDetect = new Mock<IStagingDataProvider>();
			var serviceFactory = new Mock<IServiceFactory>();
			var cache = new Mock<ICacheProvider>().Object;
			serviceFactory.Setup(x => x.GetStagingDataProvider(true, It.IsAny<int?>())).Returns(batchStagingDataProvider.Object);
			serviceFactory.Setup(x => x.GetStagingDataProvider(false, It.IsAny<int?>())).Returns(dataProviderWithoutAutoDetect.Object);
			serviceFactory.Setup(x => x.GetSafeDataProvider(cache)).Returns(new Mock<ISafeDataProvider>().Object);
			serviceFactory.Setup(x => x.GetStagingDataWrapperFactory(It.IsAny<ISafeDataProvider>(),
				It.IsAny<IMetadataProvider>(), It.IsAny<IStagingDataProvider>())).Returns(new Mock<IStagingDataWrapperFactory>().Object);

			stagingDataProvider.Setup(x => x.GetNextDataBatch(It.IsAny<Stage.SourceData>(), It.IsAny<IMetadataProvider>(), It.IsAny<int?>(), null))
				.Returns(Enumerable.Empty<Tuple<object, Stage.DataProcessingInformation>>());
			stagingDataProvider.Setup(x => x.GetDataProcessingInformation(It.IsAny<Stage.SourceData>()))
				.Returns(new Stage.DataProcessingInformation[]
				{
					new Stage.DataProcessingInformation
					{
						DPI_ParentPk = null,
						DPI_Status = "QUE"
					},
					new Stage.DataProcessingInformation
					{
						DPI_ParentPk = null,
						DPI_Status = "QUE"
					},
					new Stage.DataProcessingInformation
					{
						DPI_ParentPk = Guid.NewGuid(),
						DPI_Status = "QUE"
					}
				}.AsQueryable());

			var sourceData = new Stage.SourceData();
			var repo = new Mock<Stage.IStagingRepository>();
			repo.Setup(x => x.Get<Stage.SourceData>()).Returns(new[] { sourceData }.AsQueryable());
			batchStagingDataProvider.Setup(x => x.GetSingleSourceData(sourceData.SDA_PK)).Returns(sourceData);
			var processor = new MergeProcessor(stagingDataProvider.Object, serviceFactory.Object, new Mock<IMetadataProvider>().Object, new Mock<IOdataUriProvider>().Object,
					sourceData, cache, 2, 1, UpdateType.Partial);
			await processor.Process(1);

			dataProviderWithoutAutoDetect.Verify(x => x.UpdateEmptyDPIParentPKStatusToErrAsync(It.IsAny<Stage.SourceData>()), Times.Exactly(1));
		}

		ICacheProvider cache;
		const string stagingDbName = CreateDatabaseAttribute.DbNamePrefix + "17C71F9054DE408FA5559CBEE1E8AD43";
		readonly string stagingConnectionString = TestConnectionString.GetAdmin(stagingDbName);
		[SetUp]
		public void SetUp()
		{
			var cacheMock = new Mock<ICacheProvider>();
			cacheMock.Setup(x => x.MatchedSafeObjPKCache).Returns(new ConcurrentDictionary<string, Lazy<Guid[]>>());
			cache = cacheMock.Object;
		}
	}
}
