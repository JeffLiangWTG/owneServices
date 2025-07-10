using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading.Tasks;
using CargoWise.RefDbRepo.Common.Infrastructure.Test;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter;
using CargoWise.RefDbRepo.Staging.Schema_New;
using Moq;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.Staging.UniversalXMLStagingDataProcessor.Test
{
	[TestFixture]
	[CreateDatabase("17C71F9054DE408FA5559CBEE1E8AD43", DbSchema.RefDbRepoStaging, ActionTargets.Test)]
	[Property("DAT:CapabilityRequirements", "SQL2019+")]
	class MergeProcessorIntegrationFixture
	{

		[Test]
		public async Task TestProcessSingleBatchInTransaction()
		{
			const string stagingDbName = CreateDatabaseAttribute.DbNamePrefix + "17C71F9054DE408FA5559CBEE1E8AD43";
			var stagingDbConnectionString = TestConnectionString.GetAdmin(stagingDbName);
			var sourceData = new SourceData
			{
				SDA_PK = Guid.NewGuid(),
				SDA_Source = "AA",
				SDA_Filetype = "TXT",
				SDA_Status = "PRS",
				SDA_SubSource = "BB",
				SDA_ContentText = "",
				SDA_SourceTime = new DateTime(2023, 12, 11)
			};
			var refCusProcedure = new RefCusProcedure
			{
				ZZ6_PK = Guid.NewGuid(),
				ZZ6_Category = "IM",
				ZZ6_ProcedureCode = "30",
				ZZ6_PreviousProcedureCode = "70",
				ZZ6_Concession = "99M",
				ZZ6_Description = "Regeling veredeling",
				ZZ6_ZZZ_NKDataGrouping = "ZA",
				ZZ6_ShipmentType = "IMP",
				ZZ6_CalculateDuty = true,
				ZZ6_Group = "H3",
				ZZ6_LandedCost = true,
				ZZ6_IntoWarehouse = "N",
				ZZ6_OutOfWarehouse = "N",
				ZZ6_IntoInwardProcessing = "N",
				ZZ6_OutOfInwardProcessing = "N",
				ZZ6_IntoOutwardProcessing = "N",
				ZZ6_OutofOutwardProcessing = "N",
				ZZ6_IntoTemporaryImport = "N",
				ZZ6_OutOfTemporaryImport = "N",
				ZZ6_IntoTemporaryExport = "N",
				ZZ6_OutOfTemporaryExport = "Y",
				ZZ6_CalculateVAT = true,
				ZZ6_IsGuaranteeConsumed = "Y",
				ZZ6_IsGuaranteeReleased = "N",
				ZZ6_IsTransit = "Y",
				ZZ6_IntoVATWarehouse = "Y",
				ZZ6_OutOfVATWarehouse = "N",
				ZZ6_StartDate = new DateTime(2023, 12, 11),
				ZZ6_EndDate = new DateTime(2023, 12, 12)
			};
			var dataProcessingInfo = new DataProcessingInformation { DPI_ID = Guid.NewGuid(), DPI_SourceId = sourceData.SDA_PK, DPI_Status = "QUE", DPI_ParentTableCode = "ZZ6", DPI_ParentPk = refCusProcedure.ZZ6_PK };
			var serviceFactory = new Mock<IServiceFactory>();
			serviceFactory.Setup(x => x.GetStagingDataProvider(It.IsAny<bool>(), It.IsAny<int?>())).Returns(() => new StagingDataProvider(new StagingRepository(stagingDbConnectionString)));
			var cacheProvider = new Mock<ICacheProvider>();
			cacheProvider.Setup(x => x.MatchedSafeObjPKCache).Returns(new ConcurrentDictionary<string, Lazy<Guid[]>>());
			var metaProvider = new Mock<IMetadataProvider>();
			var odataUriProvider = new Mock<IOdataUriProvider>();
			var safeProvider = new Mock<ISafeDataProvider>();
			var wrapperFactory = new Mock<IStagingDataWrapperFactory>();
			wrapperFactory.Setup(x => x.CreateWrapper(It.IsAny<object>())).Returns(new[] { new Mock<IStagingDataWrapper>().Object });
			serviceFactory.Setup(x => x.GetSafeDataProvider(cacheProvider.Object)).Returns(safeProvider.Object);
			serviceFactory.Setup(x => x.GetStagingDataWrapperFactory(safeProvider.Object, metaProvider.Object, It.IsAny<IStagingDataProvider>())).Returns(wrapperFactory.Object);
			var updater = new Mock<ISafeObjectUpdater>();
			var updaterResult = new SafeObjectUpdaterResult { Action = ResultAction.Insert, ParentCode = "ZZ6", NewRecordForCloneActionPK = Guid.NewGuid() };
			updater.Setup(x => x.Update(It.IsAny<IStagingDataWrapper[]>())).Returns(new[] { updaterResult });
			serviceFactory.Setup(x => x.GetSafeObjectUpdater(safeProvider.Object, metaProvider.Object, false)).Returns(updater.Object);
			safeProvider.Setup(x => x.SaveChangesAsync()).Throws(new InvalidOperationException());
			using (var stagingRepo = new StagingRepository(stagingDbConnectionString))
			using (var stagingProvider = new StagingDataProvider(stagingRepo))
			{
				stagingRepo.Add(sourceData);
				stagingRepo.Add(dataProcessingInfo);
				stagingRepo.Add(refCusProcedure);
				await stagingRepo.SaveChangesAsync();
				var processor = new MergeProcessor(stagingProvider, serviceFactory.Object, metaProvider.Object, odataUriProvider.Object, sourceData, cacheProvider.Object, 1, 1, UpdateType.Full);
				await processor.Process(1);
				var dataProcessingResults = stagingRepo.Get<DataProcessingResult>().ToArray();
				Assert.AreEqual(0, dataProcessingResults.Length);
			}
		}
	}
}
