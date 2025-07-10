using System;
using System.Linq;
using CargoWise.RefDbRepo.Common.Infrastructure.Test;
using CargoWise.RefDbRepo.Staging.Schema_New;
using CargoWise.RefDbRepo.UniversalXmlParser.Interfaces;
using Moq;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.SourceDataBinaryScanner.Test
{
	[TestFixture]
	[TransactionedTestCase]
	[Property("DAT:CapabilityRequirements", "SQL2019+")]
	public class ScannerFixtures
	{
		[Test]
		public void TestUpdateNotProcessUntil()
		{
			var data1 = new SourceData()
			{
				SDA_PK = Guid.NewGuid(),
				SDA_Source = "UPL",
				SDA_Filetype = "XML",
				SDA_Status = "QUE",
				SDA_ContentType = "URD",
				SDA_SubSource = "BB",
				SDA_ContentText = string.Empty,
			};
			var data2 = new SourceData()
			{
				SDA_PK = Guid.NewGuid(),
				SDA_Source = "UPL",
				SDA_Filetype = "XML",
				SDA_Status = "QUE",
				SDA_ContentType = "URD",
				SDA_SubSource = "BB",
				SDA_ContentText = string.Empty,
				SDA_NotProcessedUntil = DateTime.UtcNow.AddMinutes(-6)
			};
			var data3 = new SourceData()
			{
				SDA_PK = Guid.NewGuid(),
				SDA_Source = "UPL",
				SDA_Filetype = "XML",
				SDA_Status = "QUE",
				SDA_ContentType = "URD",
				SDA_SubSource = "BB",
				SDA_ContentText = string.Empty,
				SDA_NotProcessedUntil = DateTime.UtcNow.AddMinutes(2)
			};

			using (var repo = new StagingRepository(connectionString))
			{
				repo.Add(data1);
				repo.Add(data2);
				repo.Add(data3);
				repo.SaveChanges();

				var scanner = new Scanner(_contentParserMock.Object, repo, _universalXmlSchemaHandlerMock.Object, _fileTraceMock.Object, _stagingRepositoryWrapperMock.Object, _configProviderMock.Object);
				scanner.Scan().Wait();
			}
			_contentParserMock.Verify(o => o.XmlContent(data1, It.IsAny<IUniversalXmlParser>(), It.IsAny<IStagingRepository>()), Times.Once);
			_contentParserMock.Verify(o => o.XmlContent(data2, It.IsAny<IUniversalXmlParser>(), It.IsAny<IStagingRepository>()), Times.Once);
			_contentParserMock.Verify(o => o.XmlContent(data3, It.IsAny<IUniversalXmlParser>(), It.IsAny<IStagingRepository>()), Times.Never);

			using (var repo = new StagingRepository(connectionString))
			{
				data1 = repo.Get<SourceData>().FirstOrDefault(x => x.SDA_PK == data1.SDA_PK);
				data2 = repo.Get<SourceData>().FirstOrDefault(x => x.SDA_PK == data2.SDA_PK);
			}
			Assert.GreaterOrEqual(data1.SDA_NotProcessedUntil, DateTime.UtcNow);
			Assert.GreaterOrEqual(data2.SDA_NotProcessedUntil, DateTime.UtcNow);
		}

		[Test]
		public void WhenHasNewRowItIsIdentified()
		{
			var sdaValid = new SourceData()
			{
				SDA_PK = Guid.NewGuid(),
				SDA_SubSource = "AA",
				SDA_ContentText = string.Empty,
				SDA_Source = "UPL",
				SDA_Filetype = "XML",
				SDA_Status = "QUE",
				SDA_ContentType = "URD",
			};

			using var repo = new StagingRepository(connectionString);
			repo.Add(sdaValid);
			repo.SaveChanges();

			var scanner = new Scanner(_contentParserMock.Object, repo, _universalXmlSchemaHandlerMock.Object, _fileTraceMock.Object, _stagingRepositoryWrapperMock.Object, _configProviderMock.Object);
			scanner.Scan().Wait();
			_contentParserMock.Verify(o => o.XmlContent(sdaValid, It.IsAny<IUniversalXmlParser>(), repo), Times.Once);
		}

		[Test]
		public void FlagAsErrorWhenExceedsTimeoutLimit()
		{
			var oldSda = new SourceData()
			{
				SDA_PK = Guid.NewGuid(),
				SDA_SubSource = "AA",
				SDA_ContentText = string.Empty,
				SDA_Source = "UPL",
				SDA_Filetype = "XML",
				SDA_Status = "QUE",
				SDA_ContentType = "URD",
			};

			using (var repo = new StagingRepository(connectionString))
			{
				repo.Add(oldSda);
				repo.SaveChanges();

				var configProviderMock = new Mock<IConfigProvider>();
				configProviderMock.Setup(x => x.ExecutionTimeOutInHours).Returns(-1);
				var scanner = new Scanner(_contentParserMock.Object, repo, _universalXmlSchemaHandlerMock.Object, _fileTraceMock.Object, _stagingRepositoryWrapperMock.Object, configProviderMock.Object);
				scanner.Scan().Wait();
			}

			using (var repo = new StagingRepository(connectionString))
			{
				var result = repo.Get<SourceData>().FirstOrDefault(x => x.SDA_PK == oldSda.SDA_PK);
				Assert.IsTrue(result.IsSourceDataErrorStatus());
			}
		}

		[Test]
		public void FlagAsDuplicatedWhenDuplicateOnDatabase()
		{
			var sourceTime = DateTime.UtcNow;
			var sdaAsFIN = new SourceData()
			{
				SDA_PK = Guid.NewGuid(),
				SDA_Source = "UPL",
				SDA_Filetype = "XML",
				SDA_Status = "FIN",
				SDA_ContentType = "URD",
				SDA_CreatedTime = DateTime.UtcNow.AddMonths(-3),
				SDA_SubSource = "ABC",
				SDA_ContentText = string.Empty,
				SDA_SourceTime = sourceTime
			};
			var sdaAsQUEPk = Guid.NewGuid();
			var sdaAsQUE = new SourceData()
			{
				SDA_PK = sdaAsQUEPk,
				SDA_Source = "UPL",
				SDA_Filetype = "XML",
				SDA_Status = "QUE",
				SDA_ContentType = "URD",
				SDA_CreatedTime = DateTime.UtcNow,
				SDA_SubSource = "ABC",
				SDA_ContentText = string.Empty,
				SDA_SourceTime = sourceTime
			};

			using var repo = new StagingRepository(connectionString);
			repo.Add(sdaAsFIN);
			repo.Add(sdaAsQUE);
			repo.SaveChanges();

			var scanner = new Scanner(_contentParserMock.Object, repo, _universalXmlSchemaHandlerMock.Object, _fileTraceMock.Object, _stagingRepositoryWrapperMock.Object, _configProviderMock.Object);
			scanner.Scan().Wait();
			Assert.That(sdaAsQUE.IsSourceDataDuplicatedStatus());
		}

		[SetUp]
		public void Setup()
		{
			_contentParserMock = new Mock<IContentParser>();
			_stagingRepositoryMock = new Mock<IStagingRepository>();
			_stagingRepositoryWrapperMock = new Mock<IStagingRepositoryWrapper>();
			_universalXmlSchemaHandlerMock = new Mock<IUniversalXmlSchemaHandler>();
			_fileTraceMock = new Mock<IFileTrace>();
			_configProviderMock = new Mock<IConfigProvider>();
			_configProviderMock.Setup(x => x.ExecutionTimeOutInHours).Returns(2);

			var dbName = TransactionedTestCaseAttribute.GetDbName(DbSchema.RefDbRepoStaging);
			connectionString = TestConnectionString.GetAdmin(dbName);
		}

		Mock<IContentParser> _contentParserMock;
		Mock<IStagingRepositoryWrapper> _stagingRepositoryWrapperMock;
		Mock<IFileTrace> _fileTraceMock;
		Mock<IUniversalXmlSchemaHandler> _universalXmlSchemaHandlerMock;
		Mock<IStagingRepository> _stagingRepositoryMock;
		Mock<IConfigProvider> _configProviderMock;
		string connectionString;
	}
}
