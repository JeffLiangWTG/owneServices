using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using CargoWise.RefDbRepo.Common.Infrastructure.Test;
using CargoWise.RefDbRepo.Common.Utils;
using CargoWise.RefDbRepo.Staging.Schema_New;
using Moq;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using Safe = CargoWise.RefDbRepo.Common.SafeDataClient.CargoWise.RefDbRepo.Service.Schema_0_9_New;

namespace CargoWise.RefDbRepo.Staging.UniversalXMLStagingDataProcessor.Test
{
	[TestFixture]
	public class CloneProcessorFixture
	{
		[Test]
		public async Task CloneRefCusTariffOnly()
		{
			var sourceData = new SourceData() { SDA_PK = Guid.NewGuid(), SDA_SourceTime = new DateTime(2018, 01, 01), SDA_ContentText = "AA", SDA_SubSource = "DS1" };
			safeDataProviderMock.Setup(x => x.GetTypeFromTblPrefix("ZZ1")).Returns(typeof(Safe.RefCusTariff));
			safeDataProviderMock.Setup(x => x.GetTypeFromTblPrefix("ZZD")).Returns(typeof(Safe.RefCusCodeList));
			stagingDataProviderMock.Setup(x => x.GetDataProcessingInformation(sourceData)).Returns(new[] { new DataProcessingInformation { DPI_Status = "PRS", DPI_ParentTableCode = "ZZD" } }.AsQueryable());

			var cloneProcessor = new CloneProcessor(serviceFactoryMock.Object, cacheProviderMock.Object, sourceData, 1, 1);
			var result = await cloneProcessor.Process(1);
			stagingDataProviderMock.Verify(x => x.GetQueuedProcessingClone(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<Guid>()), Times.Never);
			Assert.True(result);

			stagingDataProviderMock.Setup(x => x.GetDataProcessingInformation(sourceData)).Returns(new[] { new DataProcessingInformation { DPI_Status = "PRS", DPI_ParentTableCode = "ZZ1" } }.AsQueryable());
			cloneProcessor = new CloneProcessor(serviceFactoryMock.Object, cacheProviderMock.Object, sourceData, 1, 1);
			result = await cloneProcessor.Process(1);
			stagingDataProviderMock.Verify(x => x.GetQueuedProcessingClone(sourceData.SDA_PK, "ZZ1", Guid.Empty), Times.Once);
			Assert.True(result);
		}

		[Test]
		public async Task CloneProcessWithRetry()
		{
			var maxNoOfRetries = 3;
			var maxNoOfBatches = 2;
			var batchSize = 1;

			var dpr1Pk = Guid.NewGuid();
			var dpr2Pk = Guid.NewGuid();
			var (expiredPk1, newRecordPk1) = (Guid.NewGuid(), Guid.NewGuid());
			var (expiredPk2, newRecordPk2) = (Guid.NewGuid(), Guid.NewGuid());

			var sourceData = new SourceData() { SDA_PK = Guid.NewGuid(), SDA_SourceTime = new DateTime(2018, 01, 01), SDA_ContentText = "AA", SDA_SubSource = "DS1" };
			var dataProcessingClone1 = new DataProcessingClone
			{
				DPC_PK = Guid.Parse("8856B4FA-233D-4C2C-8768-A228EBD5D78E"),
				DPC_NewRecordPK = newRecordPk1,
				DPC_ExpiredRecordPK = expiredPk1,
				DPC_SourceId = sourceData.SDA_PK,
				DPC_Status = DataProcessingStatus.QUE.ToString(),
				DPC_TableCode = "ZZ1"
			};
			var dataProcessingClone2 = new DataProcessingClone
			{
				DPC_PK = Guid.Parse("F8FE61BB-A225-4A9A-85FA-4DFE7D263151"),
				DPC_NewRecordPK = newRecordPk2,
				DPC_ExpiredRecordPK = expiredPk2,
				DPC_SourceId = sourceData.SDA_PK,
				DPC_Status = DataProcessingStatus.QUE.ToString(),
				DPC_TableCode = "ZZ1"
			};

			var result1 = new DataProcessingResult { DPR_ParentPK = newRecordPk1, DPR_ParentTableCode = "ZZ1", DPR_PublicationTime = new DateTime(2018, 01, 01), DPR_Status = "QUE", DPR_ExpirableAncestorPK = expiredPk1 };
			var result2 = new DataProcessingResult { DPR_ParentPK = newRecordPk2, DPR_ParentTableCode = "ZZ1", DPR_PublicationTime = new DateTime(2017, 01, 01), DPR_Status = "QUE", DPR_ExpirableAncestorPK = expiredPk2 };
			var result3 = new DataProcessingResult { DPR_ParentPK = dpr1Pk, DPR_ParentTableCode = "ZX1", DPR_PublicationTime = new DateTime(2018, 01, 01), DPR_Status = "PRS", DPR_ExpirableAncestorPK = expiredPk1 };
			var result4 = new DataProcessingResult { DPR_ParentPK = dpr2Pk, DPR_ParentTableCode = "ZX1", DPR_PublicationTime = new DateTime(2017, 01, 01), DPR_Status = "PRS", DPR_ExpirableAncestorPK = expiredPk2 };
			stagingDataProviderMock.Setup(x => x.GetProcessingResults(sourceData)).Returns(new[] { result1, result2, result3, result4 }.AsQueryable());
			stagingDataProviderMock.Setup(x => x.GetDataProcessingInformation(sourceData)).Returns(new[] { new DataProcessingInformation { DPI_Status = "PRS", DPI_ParentTableCode = "ZZ1" } }.AsQueryable());

			stagingDataProviderMock.Setup(x => x.GetQueuedProcessingClone(sourceData.SDA_PK, "ZZ1", Guid.Empty)).Returns(new[] { dataProcessingClone1, dataProcessingClone2 }.AsQueryable()).Callback(() =>
			{
				stagingDataProviderMock.Setup(x => x.GetQueuedProcessingClone(sourceData.SDA_PK, "ZZ1", dataProcessingClone2.DPC_PK)).Returns(Enumerable.Empty<DataProcessingClone>().AsQueryable());
			});
			safeDataProviderMock.Setup(x => x.GetTypeFromTblPrefix("ZZ1")).Returns(typeof(Safe.RefCusTariff));
			safeDataProviderMock.Setup(x => x.GetTypeFromTblPrefix("ZX1")).Returns(typeof(Safe.RefCusCondition));

			var condition = new Safe.RefCusCondition { ZX1_PK = Guid.NewGuid(), ZX1_ZZ1_Tariff = Guid.NewGuid(), ZX1_ZZZ_NKDataGrouping = "ZZ" };
			var jObject = JObject.FromObject(condition);
			var cloneProcessResults = new[] { new CloneProcessResult { ClonedRecordTypeName = "RefCusCondition", ClonedRecord = jObject } };
			safeDataProviderMock.Setup(x => x.CloneExistingRecordChildrenIntoNewRecord<Safe.RefCusTariff>(It.Is<IEnumerable<CloneProcessObject>>(a => a.Any(c => c.NewRecordPk == newRecordPk1)))).Returns(Task.FromResult(cloneProcessResults.AsEnumerable()));
			safeDataProviderMock.Setup(x => x.CloneExistingRecordChildrenIntoNewRecord<Safe.RefCusTariff>(It.Is<IEnumerable<CloneProcessObject>>(a => a.Any(c => c.NewRecordPk == newRecordPk2)))).Throws(new NotSupportedException("Not supported"));

			safeDataProviderMock.Invocations.Clear();
			var cloneProcessor = new CloneProcessor(serviceFactoryMock.Object, cacheProviderMock.Object, sourceData, maxNoOfRetries, maxNoOfBatches);
			var result = await cloneProcessor.Process(batchSize);
			Assert.False(result);
			safeDataProviderMock.Verify(x => x.CloneExistingRecordChildrenIntoNewRecord<Safe.RefCusTariff>(It.IsAny<IEnumerable<CloneProcessObject>>()), Times.Exactly(maxNoOfRetries));
			safeDataProviderMock.Verify(x => x.CloneExistingRecordChildrenIntoNewRecord<Safe.RefCusTariff>(It.Is<IEnumerable<CloneProcessObject>>(a => a.Any(c => c.ExceptionListForCloning.ContainsKey("ZX1") && c.ExceptionListForCloning["ZX1"].Count == 2))));
			safeDataProviderMock.Verify(x => x.CloneExistingRecordChildrenIntoNewRecord<Safe.RefCusTariff>(It.Is<IEnumerable<CloneProcessObject>>(a => a.Any(c => c.NewRecordPk == newRecordPk1 && c.ExpiredRecordPk == expiredPk1))), Times.Once);
			safeDataProviderMock.Verify(x => x.CloneExistingRecordChildrenIntoNewRecord<Safe.RefCusTariff>(It.Is<IEnumerable<CloneProcessObject>>(a => a.Any(c => c.NewRecordPk == newRecordPk2 && c.ExpiredRecordPk == expiredPk2))), Times.Exactly(2));
			safeDataProviderMock.Verify(x => x.Add(It.Is<Safe.RefCusCondition>(a => a.ZX1_PK == condition.ZX1_PK)));
			safeDataProviderMock.Verify(x => x.SaveChangesAsync());

			stagingDataProviderMock.Verify(x => x.CreateDPRForCloneResults(cloneProcessResults));
			stagingDataProviderMock.Verify(x => x.UpdateDataProcessingCloneStatus(It.IsAny<IEnumerable<DataProcessingClone>>(), DataProcessingStatus.PRS));
			stagingDataProviderMock.Verify(x => x.SaveChangesAsync());
			cacheProviderMock.VerifyGet(x => x.TableNameAndTypeCache);
			Assert.True(cacheProviderMock.Object.TableNameAndTypeCache.ContainsKey("RefCusCondition"));
		}

		[Test]
		public async Task TestCloneWhenSubSourceInBlackList()
		{
			var sourceData = new SourceData() { SDA_PK = Guid.NewGuid(), SDA_SourceTime = new DateTime(2018, 01, 01), SDA_ContentText = "AA", SDA_SubSource = "ZATariffs" };
			safeDataProviderMock.Setup(x => x.GetTypeFromTblPrefix("ZZ1")).Returns(typeof(Safe.RefCusTariff));
			stagingDataProviderMock.Setup(x => x.GetDataProcessingInformation(sourceData)).Returns(new[] { new DataProcessingInformation { DPI_Status = "PRS", DPI_ParentTableCode = "ZZ1" } }.AsQueryable());
			var cloneProcessor = new CloneProcessor(serviceFactoryMock.Object, cacheProviderMock.Object, sourceData, 1, 1);
			var result = await cloneProcessor.Process(1);
			Assert.True(result);
		}

		[Test]
		public async Task ClonedDataConvertToUTCDateTimeOffset()
		{
			var dpr1PK = Guid.NewGuid();
			var dpr2PK = Guid.NewGuid();
			var expiredPK = Guid.NewGuid();
			var newRecordPK = Guid.NewGuid();
			var startDate = new DateTime(2024, 1, 1);
			var endDate = new DateTime(2079, 6, 6);

			var sourceData = new SourceData() { SDA_PK = Guid.NewGuid(), SDA_SourceTime = startDate, SDA_ContentText = "AA", SDA_SubSource = "DS1" };
			var dataProcessingClone = new DataProcessingClone
			{
				DPC_PK = Guid.Parse("8856B4FA-233D-4C2C-8768-A228EBD5D78E"),
				DPC_NewRecordPK = newRecordPK,
				DPC_ExpiredRecordPK = expiredPK,
				DPC_SourceId = sourceData.SDA_PK,
				DPC_Status = DataProcessingStatus.QUE.ToString(),
				DPC_TableCode = "ZZ1"
			};

			var result1 = new DataProcessingResult { DPR_ParentPK = newRecordPK, DPR_ParentTableCode = "ZZ1", DPR_PublicationTime = startDate, DPR_Status = "QUE", DPR_ExpirableAncestorPK = expiredPK };
			var result2 = new DataProcessingResult { DPR_ParentPK = dpr1PK, DPR_ParentTableCode = "ZX1", DPR_PublicationTime = startDate, DPR_Status = "PRS", DPR_ExpirableAncestorPK = expiredPK };
			stagingDataProviderMock.Setup(x => x.GetProcessingResults(sourceData)).Returns(new[] { result1, result2 }.AsQueryable());
			stagingDataProviderMock.Setup(x => x.GetDataProcessingInformation(sourceData)).Returns(new[] { new DataProcessingInformation { DPI_Status = "PRS", DPI_ParentTableCode = "ZZ1" } }.AsQueryable());

			stagingDataProviderMock.Setup(x => x.GetQueuedProcessingClone(sourceData.SDA_PK, "ZZ1", Guid.Empty)).Returns(new[] { dataProcessingClone }.AsQueryable());
			safeDataProviderMock.Setup(x => x.GetTypeFromTblPrefix("ZZ1")).Returns(typeof(Safe.RefCusTariff));
			safeDataProviderMock.Setup(x => x.GetTypeFromTblPrefix("ZX1")).Returns(typeof(Safe.RefCusCondition));

			var condition = new Safe.RefCusCondition { ZX1_PK = Guid.NewGuid(), ZX1_ZZ1_Tariff = Guid.NewGuid(), ZX1_ZZZ_NKDataGrouping = "ZZ", ZX1_StartDate = startDate, ZX1_EndDate = endDate };
			var jObject = JObject.FromObject(condition);
			var cloneProcessResults = new[] { new CloneProcessResult { ClonedRecordTypeName = "RefCusCondition", ClonedRecord = jObject } };
			safeDataProviderMock.Setup(x => x.CloneExistingRecordChildrenIntoNewRecord<Safe.RefCusTariff>(It.Is<IEnumerable<CloneProcessObject>>(a => a.Any(c => c.NewRecordPk == newRecordPK)))).Returns(Task.FromResult(cloneProcessResults.AsEnumerable()));

			safeDataProviderMock.Invocations.Clear();
			var cloneProcessor = new CloneProcessor(serviceFactoryMock.Object, cacheProviderMock.Object, sourceData, 1, 1);
			var result = await cloneProcessor.Process(1);
			safeDataProviderMock.Verify(x => x.Add(It.Is<Safe.RefCusCondition>(a => a.ZX1_PK == condition.ZX1_PK && a.ZX1_StartDate.DateTime == startDate && a.ZX1_StartDate.Offset == TimeSpan.Zero)));
			safeDataProviderMock.Verify(x => x.Add(It.Is<Safe.RefCusCondition>(a => a.ZX1_PK == condition.ZX1_PK && a.ZX1_EndDate.DateTime == endDate && a.ZX1_EndDate.Offset == TimeSpan.Zero)));
			safeDataProviderMock.Verify(x => x.SaveChangesAsync());
		}

		[Test]
		[TransactionedTestCase]
		[Property("DAT:CapabilityRequirements", "SQL2019+")]
		public async Task TransactionAvoidConflictInParallelTasks()
		{
			var maxNoOfRetries = 1;
			var maxNoOfBatches = 100;
			var batchSize = 1;

			var stagingDbName = TransactionedTestCaseAttribute.GetDbName(DbSchema.RefDbRepoStaging);
			var stagingDbConnectionString = TestConnectionString.GetAdmin(stagingDbName);
			using var stagingRepo = new StagingRepository(stagingDbConnectionString);
			using var stagingDataProvider = new StagingDataProvider(stagingRepo);
			var sourceData = new SourceData()
			{
				SDA_PK = Guid.NewGuid(),
				SDA_Source = "INT",
				SDA_SourceTime = new DateTime(2018, 01, 01),
				SDA_ContentText = "",
				SDA_SubSource = "Transactions Test",
				SDA_CreatedTime = DateTime.UtcNow
			};
			stagingRepo.Add(sourceData);
			var testData = new List<(DataProcessingClone DPC, DataProcessingResult DPR1, DataProcessingResult DPR2)>();
			for (int i = 0; i < 100; i++)
			{
				var dprPk = Guid.NewGuid();
				var (expiredPk, newRecordPk) = (Guid.NewGuid(), Guid.NewGuid());
				var dataProcessingClone = new DataProcessingClone
				{
					DPC_PK = Guid.NewGuid(),
					DPC_NewRecordPK = newRecordPk,
					DPC_ExpiredRecordPK = expiredPk,
					DPC_SourceId = sourceData.SDA_PK,
					DPC_Status = DataProcessingStatus.QUE.ToString(),
					DPC_TableCode = "ZZ1"
				};

				var result1 = new DataProcessingResult { DPR_ParentPK = newRecordPk, DPR_ParentTableCode = "ZZ1", DPR_PublicationTime = new DateTime(2018, 01, 01), DPR_Status = "QUE", DPR_ExpirableAncestorPK = expiredPk };
				var result2 = new DataProcessingResult { DPR_ParentPK = dprPk, DPR_ParentTableCode = "ZX1", DPR_PublicationTime = new DateTime(2018, 01, 01), DPR_Status = "PRS", DPR_ExpirableAncestorPK = expiredPk };
				testData.Add((dataProcessingClone, result1, result2));
				stagingRepo.Add(dataProcessingClone);
				stagingRepo.Add(result1);
				stagingRepo.Add(result2);
			}
			stagingRepo.Add(new DataProcessingInformation
			{
				DPI_ParentPk = Guid.NewGuid(),
				DPI_SourceId = sourceData.SDA_PK,
				DPI_Status = "PRS",
				DPI_ParentTableCode = "ZZ1"
			});
			await stagingRepo.SaveChangesAsync();
			var t = testData.Select(x => x.DPR1).Union(testData.Select(x => x.DPR2)).ToList();
			var r = testData.Select(x => x.DPC).ToArray();
			safeDataProviderMock.Setup(x => x.GetTypeFromTblPrefix("ZZ1")).Returns(typeof(Safe.RefCusTariff));
			safeDataProviderMock.Setup(x => x.GetTypeFromTblPrefix("ZX1")).Returns(typeof(Safe.RefCusCondition));
			safeDataProviderMock.Setup(x => x.CloneExistingRecordChildrenIntoNewRecord<Safe.RefCusTariff>(It.IsAny<IEnumerable<CloneProcessObject>>()))
				.Returns(async () =>
				{
					await Task.Delay(1000);
					return Array.Empty<CloneProcessResult>().AsEnumerable();
				});

			serviceFactoryMock.Setup(x => x.GetStagingDataProvider(It.IsAny<bool>(), It.IsAny<int>())).Returns(() => new StagingDataProvider(new StagingRepository(stagingDbConnectionString)));

			using (var sw = new StringWriter())
			{
				var defErrOut = Console.Error;
				try
				{
					Console.SetError(sw);

					try
					{
						var cloneProcessor = new CloneProcessor(serviceFactoryMock.Object, cacheProviderMock.Object, sourceData, maxNoOfRetries, maxNoOfBatches);
						_ = await cloneProcessor.Process(batchSize);
					}
					catch (Exception ex)
					{
						Console.Error.WriteLine(ex.Message);
					}
					Assert.That(sw.ToString(), !Does.Contain("\"ClassName\":\"System.InvalidOperationException\",\"Message\":\"The connection is already in a transaction and cannot participate in another transaction.\""));
				}
				finally
				{
					Console.SetError(defErrOut);
				}
			}
		}

		Mock<IStagingDataProvider> stagingDataProviderMock;
		Mock<ISafeDataProvider> safeDataProviderMock;
		Mock<ICacheProvider> cacheProviderMock;
		ConcurrentDictionary<string, Type> tableNameAndTypeDic;
		Mock<IServiceFactory> serviceFactoryMock;

		[SetUp]
		public void SetUp()
		{
			tableNameAndTypeDic = new ConcurrentDictionary<string, Type>();
			stagingDataProviderMock = new Mock<IStagingDataProvider>();
			safeDataProviderMock = new Mock<ISafeDataProvider>();
			cacheProviderMock = new Mock<ICacheProvider>();
			cacheProviderMock.Setup(x => x.TableNameAndTypeCache).Returns(tableNameAndTypeDic);
			serviceFactoryMock = new Mock<IServiceFactory>();
			serviceFactoryMock.Setup(x => x.GetStagingDataProvider(It.IsAny<bool>(), It.IsAny<int>())).Returns(stagingDataProviderMock.Object);
			serviceFactoryMock.Setup(x => x.GetSafeDataProvider(It.IsAny<ICacheProvider>())).Returns(safeDataProviderMock.Object);
		}
	}
}
