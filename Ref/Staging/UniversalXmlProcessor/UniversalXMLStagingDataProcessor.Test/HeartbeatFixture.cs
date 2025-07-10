using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using CargoWise.RefDbRepo.Common.Infrastructure.Test;
using CargoWise.RefDbRepo.Common.Utils;
using CargoWise.RefDbRepo.Staging.Schema_New;
using Moq;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.Staging.UniversalXMLStagingDataProcessor.Test
{
	[TestFixture]
	public class HeartbeatFixture
	{
		[Test]
		public void Heartbeat()
		{
			var stagingDataProviderMock = new Mock<IStagingDataProvider>();
			var dateTimeProviderMock = new Mock<IDateTimeProvider>();
			var heartbeatInterval = 0.1; //10seconds

			var sourceDataPk = Guid.NewGuid();
			dateTimeProviderMock.Setup(x => x.GetUTCNow()).Returns(DateTime.UtcNow);

			using (var heartbeat = new Heartbeat(heartbeatInterval, sourceDataPk, stagingDataProviderMock.Object, dateTimeProviderMock.Object, DateTime.UtcNow))
			{
				heartbeat.Start();
				Thread.Sleep(TimeSpan.FromMinutes(1));
			}

			stagingDataProviderMock.Verify(x => x.UpdateSDA_NotProcessedUntil(sourceDataPk, It.IsAny<DateTime>()), Times.Exactly(10));
			stagingDataProviderMock.Verify(x => x.GetSingleSourceData(sourceDataPk), Times.Exactly(10));
		}

		[Test]
		public void Heartbeat_ShouldReportErrorForUnexpectedNotProcessedUntil()
		{
			var stagingDataProviderMock = new Mock<IStagingDataProvider>();
			var heartbeatInterval = 0.1;
			var errors = string.Empty;
			var sourceDataPk = Guid.NewGuid();

			stagingDataProviderMock.Setup(x => x.GetSingleSourceData(It.IsAny<Guid>()))
				.Returns(sourceData);
			stagingDataProviderMock.Setup(x => x.UpdateSDA_NotProcessedUntil(It.IsAny<Guid>(), It.IsAny<DateTime>()))
				.Returns<Guid, DateTime>(MockUpdateSDA_NotProcessedUntil);
			var originalError = Console.Error;

			using (var sw = new StringWriter())
			using (var heartbeat = new Heartbeat(heartbeatInterval, sourceDataPk, stagingDataProviderMock.Object, new DateTimeProvider(), sourceData.SDA_NotProcessedUntil))
			using (var anotherHeartbeat = new Heartbeat(heartbeatInterval, sourceDataPk, stagingDataProviderMock.Object, new DateTimeProvider(), sourceData.SDA_NotProcessedUntil))
			{
				Console.SetError(sw);

				heartbeat.Start();
				Thread.Sleep(TimeSpan.FromSeconds(3));

				anotherHeartbeat.Start();
				Thread.Sleep(TimeSpan.FromMinutes(0.5));

				errors = sw.ToString();
			}

			Console.SetError(originalError);
			Console.Error.WriteLine(errors);
			Assert.That(errors.Contains($"SourceData {sourceDataPk} has unexpected SDA_NotProcessedUntil."));
		}

		[Test]
		public void Heartbeat_ShouldReportErrorForUnexpectedNotProcessedUntilAfterStart()
		{
			var stagingDataProviderMock = new Mock<IStagingDataProvider>();
			var heartbeatInterval = 0.1;
			var errors = string.Empty;
			var sourceDataPk = Guid.NewGuid();

			stagingDataProviderMock.Setup(x => x.GetSingleSourceData(It.IsAny<Guid>()))
				.Returns(sourceData);
			stagingDataProviderMock.Setup(x => x.UpdateSDA_NotProcessedUntil(It.IsAny<Guid>(), It.IsAny<DateTime>()))
				.Returns<Guid, DateTime>(MockUpdateSDA_NotProcessedUntil);
			var originalError = Console.Error;

			using (var sw = new StringWriter())
			using (var heartbeat = new Heartbeat(heartbeatInterval, sourceDataPk, stagingDataProviderMock.Object, new DateTimeProvider(), sourceData.SDA_NotProcessedUntil))
			{
				Console.SetError(sw);
				sourceData.SDA_NotProcessedUntil = DateTime.UtcNow.AddSeconds(-1);
				heartbeat.Start();
				Thread.Sleep(TimeSpan.FromMinutes(heartbeatInterval));
				errors = sw.ToString();
			}

			Console.SetError(originalError);
			Console.Error.WriteLine(errors);
			Assert.That(errors.Contains($"SourceData {sourceDataPk} has unexpected SDA_NotProcessedUntil."));
		}

		[Test]
		[TransactionedTestCase]
		[Property("DAT:CapabilityRequirements", "SQL2019+")]
		public void Heartbeat_ShouldUpdateNotProcessedUntilPrecisely()
		{
			var errors = string.Empty;
			var originalError = Console.Error;
			var stagingDbConnectionString = TestConnectionString.GetAdmin(TransactionedTestCaseAttribute.GetDbName(DbSchema.RefDbRepoStaging));
			var sourceData = new SourceData
			{
				SDA_PK = Guid.NewGuid(),
				SDA_Source = "INT",
				SDA_Filetype = "XML",
				SDA_Status = "PRS",
				SDA_SubSource = "AU Customs Exchange Rates",
				SDA_ContentText = "",
				SDA_SourceTime = DateTime.Now.AddDays(-1)
			};

			using (var sw = new StringWriter())
			using (var stagingRepo = new StagingRepository(stagingDbConnectionString))
			using (var stagingProvider = new StagingDataProvider(stagingRepo))
			using (var heartbeat = new Heartbeat(0.1, sourceData.SDA_PK, stagingProvider, new DateTimeProvider(), sourceData.SDA_NotProcessedUntil))
			{
				stagingRepo.Add(sourceData);
				stagingRepo.SaveChangesAsync().Wait();

				Console.SetError(sw);
				heartbeat.Start();
				Thread.Sleep(TimeSpan.FromMinutes(0.5));
				errors = sw.ToString();

				var notProcessUntil = DateTime.UtcNow.AddMinutes(5);
				stagingProvider.UpdateSDA_NotProcessedUntil(sourceData.SDA_PK, notProcessUntil).Wait();

				var sourceFromDb = stagingProvider.GetSingleSourceData(sourceData.SDA_PK);
				Assert.AreEqual(notProcessUntil, sourceFromDb.SDA_NotProcessedUntil);
			}

			Console.SetError(originalError);
			Console.Error.WriteLine(errors);
			Assert.IsFalse(errors.Contains($"SourceData {sourceData.SDA_PK} has unexpected SDA_NotProcessedUntil."));
		}

		SourceData sourceData = new();

		Task<int> MockUpdateSDA_NotProcessedUntil(Guid _, DateTime dateTime)
		{
			sourceData.SDA_NotProcessedUntil = dateTime;
			return Task.FromResult(1);
		}
	}
}
