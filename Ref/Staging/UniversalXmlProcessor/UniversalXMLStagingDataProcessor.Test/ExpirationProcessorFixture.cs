using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using CargoWise.RefDbRepo.Staging.Schema_New;
using Microsoft.OData.Client;
using Moq;
using NUnit.Framework;
using Safe = CargoWise.RefDbRepo.Common.SafeDataClient.CargoWise.RefDbRepo.Service.Schema_0_9_New;

namespace CargoWise.RefDbRepo.Staging.UniversalXMLStagingDataProcessor.Test
{
	[TestFixture]
	class ExpirationProcessorFixture
	{
		[TestCase(true)]
		[TestCase(false)]
		public async Task Expire_ProcessOldPublishedData(bool expireResult)
		{
			var serviceFactory = new Mock<IServiceFactory>();
			var metadataProvider = new Mock<IMetadataProvider>();
			var stagingProvider = new Mock<IStagingDataProvider>();
			var sourceData = new SourceData() { SDA_SourceTime = new DateTime(2018, 01, 01), SDA_ContentText = "AA" };
			metadataProvider.Setup(x => x.IsData(nameof(RefCusTariff))).Returns(true);
			stagingProvider.Setup(x => x.IsAutoExpiredEnabled(sourceData)).Returns(true);
			stagingProvider.Setup(x => x.GetDataProcessingInformation(sourceData)).Returns(new[] { new DataProcessingInformation { DPI_Status = "PRS", DPI_ParentTableCode = "AA" } }.AsQueryable());
			stagingProvider.Setup(x => x.GetTypeFromTblPrefix("AA")).Returns(typeof(RefCusTariff));

			var safeProvider = new Mock<ISafeDataProvider>();
			safeProvider.Setup(x => x.GetTypeFromTblPrefix(It.IsAny<string>())).Returns(typeof(Safe.RefCusTariff));
			serviceFactory.Setup(x => x.GetSafeDataProvider(It.IsAny<ICacheProvider>())).Returns(safeProvider.Object);

			var result1 = new DataProcessingResult { DPR_ParentPK = Guid.NewGuid(), DPR_ParentTableCode = "AA", DPR_PublicationTime = new DateTime(2018, 01, 01), DPR_Status = "QUE", DPR_DatasetPK = Guid.NewGuid(), DPR_PK = Guid.NewGuid() };
			var result2 = new DataProcessingResult { DPR_ParentPK = Guid.NewGuid(), DPR_ParentTableCode = "AA", DPR_PublicationTime = new DateTime(2017, 01, 01), DPR_Status = "QUE", DPR_DatasetPK = Guid.NewGuid(), DPR_PK = Guid.NewGuid() };
			var result3 = new DataProcessingResult { DPR_ParentPK = Guid.NewGuid(), DPR_ParentTableCode = "AA", DPR_PublicationTime = new DateTime(2017, 01, 01), DPR_Status = "QUE", DPR_ExpirationTime = new DateTime(2017, 10, 10), DPR_DatasetPK = Guid.NewGuid(), DPR_PK = Guid.NewGuid() };
			stagingProvider.Setup(x => x.GetProcessingResults(sourceData)).Returns(new[] { result1, result2, result3 }.AsQueryable());
			if (!expireResult)
			{
				safeProvider.Setup(x => x.BatchExpire<Safe.RefCusTariff>(new[] { result2.DPR_ParentPK }, It.Is<DateTime>(dt => dt == new DateTime(2018, 01, 01))))
					.Throws(new Exception());
			}

			var expiration = new ExpirationProcessor(sourceData, stagingProvider.Object, safeProvider.Object, metadataProvider.Object, overlappingCalculator);
			await expiration.UpdateResultsAndAutoExpire();
			safeProvider.Verify(x => x.BatchExpire<Safe.RefCusTariff>(new[] { result2.DPR_ParentPK }, It.Is<DateTime>(dt => dt == new DateTime(2018, 01, 01))));
			safeProvider.Verify(x => x.BatchExpire<Safe.RefCusTariff>(new[] { result3.DPR_ParentPK }, It.Is<DateTime>(dt => dt == new DateTime(2017, 10, 10))));
		}

		[Test]
		public async Task Expire_WithErrorDPR()
		{
			var serviceFactory = new Mock<IServiceFactory>();
			var metadataProvider = new Mock<IMetadataProvider>();
			var stagingProvider = new Mock<IStagingDataProvider>();
			var newGuid1 = Guid.NewGuid();
			var sourceData = new SourceData() { SDA_SourceTime = new DateTime(2018, 01, 01), SDA_ContentText = "AA" };
			metadataProvider.Setup(x => x.IsData(nameof(RefCusTariff))).Returns(true);
			stagingProvider.Setup(x => x.IsAutoExpiredEnabled(sourceData)).Returns(true);
			stagingProvider.Setup(x => x.GetDataProcessingInformation(sourceData)).Returns(new[] { new DataProcessingInformation { DPI_Status = "PRS", DPI_ParentTableCode = "AA" }, new DataProcessingInformation { DPI_Status = "ERR", DPI_ParentTableCode = "AA", DPI_HasDPRRecordWhenError = true }, new DataProcessingInformation { DPI_Status = "ERR", DPI_ParentTableCode = "AA", DPI_HasDPRRecordWhenError = true } }.AsQueryable());
			stagingProvider.Setup(x => x.GetTypeFromTblPrefix("AA")).Returns(typeof(RefCusTariff));

			var safeProvider = new Mock<ISafeDataProvider>();
			safeProvider.Setup(x => x.GetTypeFromTblPrefix(It.IsAny<string>())).Returns(typeof(Safe.RefCusTariff));
			serviceFactory.Setup(x => x.GetSafeDataProvider(It.IsAny<ICacheProvider>())).Returns(safeProvider.Object);

			var result1 = new DataProcessingResult { DPR_ParentPK = newGuid1, DPR_ParentTableCode = "AA", DPR_PublicationTime = new DateTime(2018, 01, 01), DPR_Status = "ERR", DPR_DatasetPK = newGuid1 };
			stagingProvider.Setup(x => x.GetProcessingResults(sourceData)).Returns(new[] { result1 }.AsQueryable());

			var expiration = new ExpirationProcessor(sourceData, stagingProvider.Object, safeProvider.Object, metadataProvider.Object, overlappingCalculator);
			await expiration.UpdateResultsAndAutoExpire();
			Assert.AreEqual(DataProcessingStatus.ERR.ToString(), result1.DPR_Status);
		}

		[Test]
		public async Task Expire_WithIsActiveColumn()
		{
			var serviceFactory = new Mock<IServiceFactory>();
			var metadataProvider = new Mock<IMetadataProvider>();
			var stagingProvider = new Mock<IStagingDataProvider>();
			var sourceData = new SourceData() { SDA_SourceTime = new DateTime(2022, 01, 01), SDA_SubSource = "Compliance List" };
			metadataProvider.Setup(x => x.IsData(nameof(RefComplianceList))).Returns(true);
			stagingProvider.Setup(x => x.IsAutoExpiredEnabled(sourceData)).Returns(true);
			stagingProvider.Setup(x => x.GetDataProcessingInformation(sourceData)).Returns(new[] { new DataProcessingInformation { DPI_Status = "PRS", DPI_ParentTableCode = "RCL" } }.AsQueryable());
			stagingProvider.Setup(x => x.GetTypeFromTblPrefix("RCL")).Returns(typeof(RefComplianceList));

			var safeProvider = new Mock<ISafeDataProvider>();
			safeProvider.Setup(x => x.GetTypeFromTblPrefix(It.IsAny<string>())).Returns(typeof(Safe.RefComplianceList));
			serviceFactory.Setup(x => x.GetSafeDataProvider(It.IsAny<ICacheProvider>())).Returns(safeProvider.Object);

			var result1 = new DataProcessingResult { DPR_ParentPK = Guid.NewGuid(), DPR_ParentTableCode = "RCL", DPR_PublicationTime = new DateTime(2022, 01, 01), DPR_Status = "QUE", DPR_DatasetPK = Guid.NewGuid(), DPR_PK = Guid.NewGuid() };
			var result2 = new DataProcessingResult { DPR_ParentPK = Guid.NewGuid(), DPR_ParentTableCode = "RCL", DPR_PublicationTime = new DateTime(2021, 01, 01), DPR_Status = "QUE", DPR_DatasetPK = Guid.NewGuid(), DPR_PK = Guid.NewGuid() };
			var result3 = new DataProcessingResult { DPR_ParentPK = Guid.NewGuid(), DPR_ParentTableCode = "RCL", DPR_PublicationTime = new DateTime(2020, 01, 01), DPR_Status = "QUE", DPR_ExpirationTime = new DateTime(2020, 10, 10), DPR_DatasetPK = Guid.NewGuid(), DPR_PK = Guid.NewGuid() };
			stagingProvider.Setup(x => x.GetProcessingResults(sourceData)).Returns(new[] { result1, result2, result3 }.AsQueryable());

			var expiration = new ExpirationProcessor(sourceData, stagingProvider.Object, safeProvider.Object, metadataProvider.Object, overlappingCalculator);
			await expiration.UpdateResultsAndAutoExpire();
			Assert.AreEqual(DataProcessingStatus.QUE.ToString(), result1.DPR_Status);
			Assert.AreEqual(new DateTime(2020, 10, 10), result3.DPR_ExpirationTime.Value);
			safeProvider.Verify(x => x.BatchInActive<Safe.RefComplianceList>(new[] { result2.DPR_ParentPK }));
			safeProvider.Verify(x => x.BatchInActive<Safe.RefComplianceList>(new[] { result3.DPR_ParentPK }));
		}

		[Test]
		public async Task Expire_WhenDataFalse()
		{
			var serviceFactory = new Mock<IServiceFactory>();
			var metadataProvider = new Mock<IMetadataProvider>();
			var stagingProvider = new Mock<IStagingDataProvider>();
			var sourceData = new SourceData() { SDA_SourceTime = new DateTime(2018, 01, 01), SDA_ContentText = "AA" };
			metadataProvider.Setup(x => x.IsData(nameof(RefCusTariff))).Returns(false);
			metadataProvider.Setup(x => x.IsData(nameof(RefCusCondition))).Returns(true);
			stagingProvider.Setup(x => x.IsAutoExpiredEnabled(sourceData)).Returns(true);
			stagingProvider.Setup(x => x.GetDataProcessingInformation(sourceData)).Returns(new[] { new DataProcessingInformation { DPI_Status = "PRS", DPI_ParentTableCode = "AA" } }.AsQueryable());
			stagingProvider.Setup(x => x.GetTypeFromTblPrefix("AA")).Returns(typeof(RefCusTariff));
			stagingProvider.Setup(x => x.GetTypeFromTblPrefix("ZX1")).Returns(typeof(RefCusCondition));
			stagingProvider.Setup(x => x.GetRelatedEntityTypes(nameof(RefCusTariff), metadataProvider.Object)).Returns(new Type[] { typeof(RefCusCondition) });

			var safeProvider = new Mock<ISafeDataProvider>();
			safeProvider.Setup(x => x.GetTypeFromTblPrefix("AA")).Returns(typeof(Safe.RefCusTariff));
			safeProvider.Setup(x => x.GetTypeFromTblPrefix("ZX1")).Returns(typeof(Safe.RefCusCondition));
			serviceFactory.Setup(x => x.GetSafeDataProvider(It.IsAny<ICacheProvider>())).Returns(safeProvider.Object);

			var result1 = new DataProcessingResult { DPR_ParentPK = Guid.NewGuid(), DPR_ParentTableCode = "AA", DPR_PublicationTime = new DateTime(2018, 01, 01), DPR_Status = "QUE", DPR_DatasetPK = Guid.NewGuid() };
			var result2 = new DataProcessingResult { DPR_ParentPK = Guid.NewGuid(), DPR_ParentTableCode = "AA", DPR_PublicationTime = new DateTime(2017, 01, 01), DPR_Status = "QUE" , DPR_DatasetPK = Guid.NewGuid() };
			var result3 = new DataProcessingResult { DPR_ParentPK = Guid.NewGuid(), DPR_ParentTableCode = "ZX1", DPR_PublicationTime = new DateTime(2018, 01, 01), DPR_Status = "QUE", DPR_DatasetPK = Guid.NewGuid() };
			var result5 = new DataProcessingResult { DPR_ParentPK = Guid.NewGuid(), DPR_ParentTableCode = "ZX1", DPR_PublicationTime = new DateTime(2017, 01, 01), DPR_Status = "QUE", DPR_DatasetPK = Guid.NewGuid() };
			stagingProvider.Setup(x => x.GetProcessingResults(sourceData)).Returns(new[] { result1, result2, result3 }.AsQueryable());

			var expiration = new ExpirationProcessor(sourceData, stagingProvider.Object, safeProvider.Object, metadataProvider.Object, overlappingCalculator);
			await expiration.UpdateResultsAndAutoExpire();

			Assert.AreEqual(DataProcessingStatus.QUE.ToString(), result1.DPR_Status);
			Assert.AreEqual(DataProcessingStatus.QUE.ToString(), result2.DPR_Status);
			Assert.AreEqual(DataProcessingStatus.QUE.ToString(), result3.DPR_Status);
			Assert.AreEqual(DataProcessingStatus.QUE.ToString(), result5.DPR_Status);
			Assert.AreEqual(null, result1.DPR_ExpirationTime);
			Assert.AreEqual(null, result3.DPR_ExpirationTime);
			Assert.AreEqual(null, result5.DPR_ExpirationTime);
			safeProvider.Verify(x => x.BatchExpire<Safe.RefCusTariff>(new[] { result1.DPR_ParentPK }, It.Is<DateTime>(dt => dt == new DateTime(2018, 01, 01))), Times.Never);
			safeProvider.Verify(x => x.BatchExpire<Safe.RefCusTariff>(new[] { result2.DPR_ParentPK }, It.Is<DateTime>(dt => dt == new DateTime(2018, 01, 01))), Times.Never);
			safeProvider.Verify(x => x.BatchExpire<Safe.RefCusCondition>(new[] { result3.DPR_ParentPK },It.Is<DateTime>(dt => dt == new DateTime(2018, 01, 01))), Times.Never);
			safeProvider.Verify(x => x.BatchExpire<Safe.RefCusCondition>(new[] { result5.DPR_ParentPK }, It.Is<DateTime>(dt => dt == new DateTime(2018, 01, 01))), Times.Never);
		}

		[Test]
		public async Task AutoExpireNotEnable()
		{
			var serviceFactory = new Mock<IServiceFactory>();
			var metadataProvider = new Mock<IMetadataProvider>();
			var stagingProvider = new Mock<IStagingDataProvider>();
			var safeProvider = new Mock<ISafeDataProvider>();
			var sourceData = new SourceData() { SDA_SourceTime = new DateTime(2018, 01, 01), SDA_ContentText = "AA" };
			stagingProvider.Setup(x => x.IsAutoExpiredEnabled(sourceData)).Returns(false);

			var expiration = new ExpirationProcessor(sourceData, stagingProvider.Object, safeProvider.Object, new Mock<IMetadataProvider>().Object, overlappingCalculator);
			await expiration.UpdateResultsAndAutoExpire();
			stagingProvider.Verify(x => x.UpdateAllExpiredDataProcessingResultAsync(sourceData));
		}

		[Test]
		public async Task ShouldRun_IfThereIsNoDPI()
		{
			var stagingProvider = new Mock<IStagingDataProvider>();
			stagingProvider.Setup(x => x.IsAutoExpiredEnabled(It.IsAny<SourceData>())).Returns(true);
			var sourceData = new SourceData() { SDA_SourceTime = new DateTime(2018, 01, 01), SDA_ContentText = "AA" };
			var safeProvider = new Mock<ISafeDataProvider>();

			var expiration = new ExpirationProcessor(sourceData, stagingProvider.Object, safeProvider.Object, new Mock<IMetadataProvider>().Object, overlappingCalculator);
			var result = await expiration.UpdateResultsAndAutoExpire();
			Assert.False(result);
		}

		[Test]
		public async Task ShouldRun_IfThereIsNoApplicableDPR()
		{
			var sourceData = new SourceData() { SDA_SourceTime = new DateTime(2018, 01, 01), SDA_ContentText = "AA" };
			var stagingProvider = new Mock<IStagingDataProvider>();
			stagingProvider.Setup(x => x.IsAutoExpiredEnabled(It.IsAny<SourceData>())).Returns(true);
			var result1 = new DataProcessingResult { DPR_ParentPK = Guid.NewGuid(), DPR_ParentTableCode = "AA", DPR_PublicationTime = new DateTime(2018, 01, 01), DPR_Status = "PRS", DPR_DatasetPK = Guid.NewGuid() };
			var result2 = new DataProcessingResult { DPR_ParentPK = Guid.NewGuid(), DPR_ParentTableCode = "AA", DPR_PublicationTime = new DateTime(2017, 01, 01), DPR_Status = "PRS", DPR_DatasetPK = Guid.NewGuid() };
			stagingProvider.Setup(x => x.GetProcessingResults(sourceData)).Returns(new[] { result1, result2 }.AsQueryable());
			stagingProvider.Setup(x => x.GetDataProcessingInformation(sourceData)).Returns(new[] {
				new DataProcessingInformation { DPI_Status = "PRS" }
			}.AsQueryable());
			var safeProvider = new Mock<ISafeDataProvider>();

			var expiration = new ExpirationProcessor(sourceData, stagingProvider.Object, safeProvider.Object, new Mock<IMetadataProvider>().Object, overlappingCalculator);
			var result = await expiration.UpdateResultsAndAutoExpire();
			Assert.That(result, Is.True);
		}

		[Test]
		public async Task ShouldRun_IfThereIsQUEDPI()
		{
			var sourceData = new SourceData() { SDA_SourceTime = new DateTime(2018, 01, 01), SDA_ContentText = "AA" };
			var stagingProvider = new Mock<IStagingDataProvider>();
			stagingProvider.Setup(x => x.IsAutoExpiredEnabled(It.IsAny<SourceData>())).Returns(true);
			var result1 = new DataProcessingResult { DPR_ParentPK = Guid.NewGuid(), DPR_ParentTableCode = "AA", DPR_PublicationTime = new DateTime(2018, 01, 01), DPR_Status = "PRS", DPR_DatasetPK = Guid.NewGuid() };
			var result2 = new DataProcessingResult { DPR_ParentPK = Guid.NewGuid(), DPR_ParentTableCode = "AA", DPR_PublicationTime = new DateTime(2017, 01, 01), DPR_Status = "QUE", DPR_DatasetPK = Guid.NewGuid() };
			stagingProvider.Setup(x => x.GetProcessingResults(sourceData)).Returns(new[] { result1, result2 }.AsQueryable());
			stagingProvider.Setup(x => x.GetDataProcessingInformation(sourceData)).Returns(new[] {
				new DataProcessingInformation { DPI_Status = "QUE" }
			}.AsQueryable());
			var safeProvider = new Mock<ISafeDataProvider>();

			var expiration = new ExpirationProcessor(sourceData, stagingProvider.Object, safeProvider.Object, new Mock<IMetadataProvider>().Object, overlappingCalculator);
			var result = await expiration.UpdateResultsAndAutoExpire();
			Assert.False(result);
		}

		[Test]
		public async Task ShouldRun_IfThereIsERRDPIWithFalseHasDPRRecord()
		{
			var sourceData = new SourceData() { SDA_SourceTime = new DateTime(2018, 01, 01), SDA_ContentText = "AA" };
			var stagingProvider = new Mock<IStagingDataProvider>();
			stagingProvider.Setup(x => x.IsAutoExpiredEnabled(It.IsAny<SourceData>())).Returns(true);
			stagingProvider.Setup(x => x.GetDataProcessingInformation(sourceData)).Returns(new[] {
				new DataProcessingInformation { DPI_Status = "PRS" },
				new DataProcessingInformation { DPI_Status = "ERR", DPI_HasDPRRecordWhenError = false },
			}.AsQueryable());
			var safeProvider = new Mock<ISafeDataProvider>();

			var expiration = new ExpirationProcessor(sourceData, stagingProvider.Object, safeProvider.Object, new Mock<IMetadataProvider>().Object, overlappingCalculator);
			var result = await expiration.UpdateResultsAndAutoExpire();
			Assert.False(result);
		}

		[Test]
		public async Task ShouldRun_IfThereIsERRDPIWithTrueHasDPRRecord()
		{
			var sourceData = new SourceData() { SDA_SourceTime = new DateTime(2018, 01, 01), SDA_ContentText = "AA" };
			var stagingProvider = new Mock<IStagingDataProvider>();
			stagingProvider.Setup(x => x.IsAutoExpiredEnabled(It.IsAny<SourceData>())).Returns(true);
			stagingProvider.Setup(x => x.GetDataProcessingInformation(sourceData)).Returns(new[] {
				new DataProcessingInformation { DPI_Status = "ERR", DPI_HasDPRRecordWhenError = true },
			}.AsQueryable());
			var result1 = new DataProcessingResult { DPR_ParentPK = Guid.NewGuid(), DPR_ParentTableCode = "AA", DPR_PublicationTime = new DateTime(2018, 01, 01), DPR_Status = "PRS", DPR_DatasetPK = Guid.NewGuid() };
			var result2 = new DataProcessingResult { DPR_ParentPK = Guid.NewGuid(), DPR_ParentTableCode = "AA", DPR_PublicationTime = new DateTime(2017, 01, 01), DPR_Status = "QUE", DPR_DatasetPK = Guid.NewGuid() };
			stagingProvider.Setup(x => x.GetProcessingResults(sourceData)).Returns(new[] { result1, result2 }.AsQueryable());
			var safeProvider = new Mock<ISafeDataProvider>();

			var expiration = new ExpirationProcessor(sourceData, stagingProvider.Object, safeProvider.Object, new Mock<IMetadataProvider>().Object, overlappingCalculator);
			var result = await expiration.UpdateResultsAndAutoExpire();
			Assert.True(result);
		}

		[TestCase("ZZ1", true, typeof(RefCusRate),true)]
		[TestCase("ZZ1", false, typeof(RefCusRate), true)]
		[TestCase("ZY2", true, typeof(RefCusApplicability), false)]
		[TestCase("ZY2", true, typeof(RefCusTariffAdditionalCodeLanguage), true)]
		[TestCase("ZY2", false, typeof(RefCusApplicability), true)]
		public void Test_ShouldExpireCurrentEntity(string tablePrefix, bool hasChildren, Type childType, bool shouldExpire)
		{
			// Arrange
			var sourceData = new SourceData { SDA_SourceTime = new DateTime(2025, 03, 10), SDA_ContentText = "AA" };
			var stagingProvider = new Mock<IStagingDataProvider>();
			stagingProvider.Setup(x => x.GetTypeFromTblPrefix("ZZ1")).Returns(typeof(RefCusTariff));
			stagingProvider.Setup(x => x.GetRelatedEntityTypes(nameof(RefCusTariff), It.IsAny<IMetadataProvider>())).Returns(hasChildren ? [childType] : Type.EmptyTypes);
			stagingProvider.Setup(x => x.GetTypeFromTblPrefix("ZY2")).Returns(typeof(RefCusTariffAdditionalCode));
			stagingProvider.Setup(x => x.GetRelatedEntityTypes(nameof(RefCusTariffAdditionalCode), It.IsAny<IMetadataProvider>())).Returns(hasChildren ? [childType] : Type.EmptyTypes);
			var safeProvider = new Mock<ISafeDataProvider>();

			// Act
			var expiration = new ExpirationProcessor(sourceData, stagingProvider.Object, safeProvider.Object, new Mock<IMetadataProvider>().Object, overlappingCalculator);
			var result = expiration.ShouldExpireCurrentEntity(tablePrefix);

			// Assert
			Assert.That(result, Is.EqualTo(shouldExpire));
		}

		IOverlappingCalculator overlappingCalculator;
		[SetUp]
		public void SetUp()
		{
			overlappingCalculator = new OverlappingCalculator(false);
		}
	}
}
