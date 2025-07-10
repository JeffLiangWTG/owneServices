using System;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using CargoWise.RefDbRepo.Common.Utils;
using CargoWise.RefDbRepo.DataProcessingExplanation;
using CargoWise.RefDbRepo.Staging.Schema_New;
using CargoWise.RefDbRepo.UniversalXmlParser.Interfaces;
using CargoWise.RefDbRepo.UniversalXmlParser.Resources;
using Common.Logging;
using Microsoft.Data.SqlClient;
using Moq;
using NUnit.Framework;
using Unity;
using Unity.Resolution;

namespace CargoWise.RefDbRepo.UniversalXmlParser.Test.XmlParser
{
	[TestFixture]
	public class UniversalXmlParserFixtures : BaseUnitTestFixture
	{
		#region Member Variables

		const string UniversalReferenceDataXml = @"CargoWise.RefDbRepo.UniversalXmlParser.Test.TestFiles.UniversalReferenceData.xml";
		const string RootErrorTestFile = @"CargoWise.RefDbRepo.UniversalXmlParser.Test.TestFiles.UniversalReferenceData_RootError.xml";
		const string RefCusCodeListTestFileError = @"CargoWise.RefDbRepo.UniversalXmlParser.Test.TestFiles.UniversalReferenceData_CusCodeList_Error.xml";
		const string RefCusTariff_20Tariffs = @"CargoWise.RefDbRepo.UniversalXmlParser.Test.TestFiles.RefCusTariff_20Tariffs.xml";
		const string SchemaWithNoDataError = @"CargoWise.RefDbRepo.UniversalXmlParser.Test.TestFiles.SchemaWithNoData.xml";
		const string RefCusCodelist_NoPublishDate = @"CargoWise.RefDbRepo.UniversalXmlParser.Test.TestFiles.UniversalReferenceData_NoPublishDate.xml";
		const string RefCusCodelist_NoAppName = @"CargoWise.RefDbRepo.UniversalXmlParser.Test.TestFiles.UniversalReferenceData_NoAppName.xml";

		Mock<IStagingRepository> stagingRepositoryMock;
		Mock<IEntityValuesProvider> valuesProviderMock;
		Mock<ILog> loggerMock;
		Mock<IUniversalXmlSchemaHandler> universalXmlSchemaHandlerMock;
		Mock<IStagingRepositoryWrapper> stagingRepositoryWrapperMock;
		Mock<ISourceDataWriter> sourceDataWriterMock;
		Mock<IFileTrace> fileTraceMock;
		Mock<IParserConfig> parserConfigMock;

		#endregion

		[Test]
		public async Task TestParseAsync_VerifyMetaElement()
		{
			var parser = Container.Resolve<IUniversalXmlParser>();
			Assert.IsNotNull(parser);

			var xmlFileName = Path.GetTempFileName();
			File.WriteAllText(xmlFileName, "<UniversalReferenceData> <blah>Some text</blah> <RefCusTariff>Some tariff data</RefCusTariff> </UniversalReferenceData>");

			using (var stream = File.Open(xmlFileName, FileMode.Open))
			{
				Assert.That(() => parser.ParseAsync(stream, true), Throws.Exception.TypeOf<RefDataProcessingException>()
					.And.Message.StartsWith($@"The provided XML has errors in MetaData: DataSource element is missing.
ErrorCode: {ErrorCodes.IncorrectXmlMetaData}"));
			}

			File.WriteAllText(xmlFileName, "<UniversalReferenceData><DataSource>SomeText</DataSource> <OtherStuff>Lalala</OtherStuff> <RefCusTariff>Some tariff data</RefCusTariff> </UniversalReferenceData>");

			using (var stream = File.Open(xmlFileName, FileMode.Open))
			{
				Assert.That(() => parser.ParseAsync(stream, true), Throws.Exception.TypeOf<RefDataProcessingException>()
					.And.Message.StartsWith($@"The provided XML has errors in MetaData: PublicationTime element is missing.
ErrorCode: {ErrorCodes.IncorrectXmlMetaData}"));
			}

			File.WriteAllText(xmlFileName, "<root><DataSource>SomeText</DataSource> <PublicationTime>2017-01-01T00:00:00</PublicationTime></root>");

			using (var stream = File.Open(xmlFileName, FileMode.Open))
			{
				Assert.That(() => parser.ParseAsync(stream, true), Throws.Exception.TypeOf<RefDataProcessingException>()
					.And.Message.StartsWith($@"The root element of the provided xml is not UniversalReferenceData.
ErrorCode: {ErrorCodes.RootElementNotValid}"));
			}

			File.WriteAllText(xmlFileName, "<UniversalReferenceData><DataSource>SomeText</DataSource> <PublicationTime>2017-01-01T00:00:00</PublicationTime></UniversalReferenceData>");

			using (var stream = File.Open(xmlFileName, FileMode.Open))
			{
				Assert.That(() => parser.ParseAsync(stream, true), Throws.Exception.TypeOf<RefDataProcessingException>()
					.And.Message.StartsWith($@"The provided XML has errors in MetaData: Schema element is missing.
ErrorCode: {ErrorCodes.IncorrectXmlMetaData}"));
			}

			File.WriteAllText(xmlFileName, "<UniversalReferenceData><DataSource>SomeText</DataSource> <PublicationTime>2017-01-01T00:00:00</PublicationTime> <Schema> <anything> </anything> </Schema></UniversalReferenceData>");
			using (var stream = File.Open(xmlFileName, FileMode.Open))
			{
				await parser.ParseAsync(stream, true);
			}
			sourceDataWriterMock.Verify();
		}

		[Test]
		public async Task TestParserAsync_SaveToSourceDataWhenFindUnknownElementOnRootLevel()
		{
			var parser = Container.Resolve<IUniversalXmlParser>();
			Assert.IsNotNull(parser);

			using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(RootErrorTestFile))
			{
				if (stream != null)
				{
					await parser.ParseAsync(stream, true);
					stagingRepositoryWrapperMock.Verify(o => o.CreateRecord(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<Guid>()), Times.Once);
					sourceDataWriterMock.Verify(x => x.AddXmlContent(It.Is<string>(content => content.Contains("<Message>Error Tariffs</Message>"))));
				}
			}
		}

		[Test]
		public void TestParserAsync_NoPublishDate()
		{
			sourceDataWriterMock.Setup(
				x => x.CreateDataProcessingInformationError(It.IsAny<string>())).Verifiable();

			sourceDataWriterMock.Setup(x => x.SetFlagErrorOnRootLevel()).Verifiable();

			var parser = Container.Resolve<IUniversalXmlParser>();
			Assert.IsNotNull(parser);

			using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(RefCusCodelist_NoPublishDate))
			{
				if (stream != null)
				{
					Assert.That(() => parser.ParseAsync(stream, true),
						Throws.Exception.TypeOf<RefDataProcessingException>()
						.And.Message.StartsWith($@"The provided XML has errors in MetaData: PublicationTime element is missing.
ErrorCode: {ErrorCodes.IncorrectXmlMetaData}"));
					sourceDataWriterMock.Verify();
					stagingRepositoryWrapperMock.Verify();
				}
			}
		}

		[Test]
		public async Task TestParserAsync_SchemaOnlyWithNoDataError()
		{
			sourceDataWriterMock.Setup(
				x => x.SetFlagErrorOnRootLevel()).Verifiable();

			var parser = Container.Resolve<IUniversalXmlParser>();
			Assert.IsNotNull(parser);

			using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(SchemaWithNoDataError))
			{
				if (stream != null)
				{
					await parser.ParseAsync(stream, true);
					sourceDataWriterMock.Verify();
				}
			}
		}

		[Test]
		public async Task TestParserAsync_VerifyParsingXmlContent()
		{
			sourceDataWriterMock.Setup(
				x => x.AddContent(
					It.Is<string>(key => key == Constants.UniversalXmlMetadata.PublicationTime),
					It.IsAny<string>())).Verifiable();

			sourceDataWriterMock.Setup(
				x => x.AddContent(
					It.Is<string>(key => key == Constants.UniversalXmlMetadata.AppName),
					It.IsAny<string>())).Verifiable();

			sourceDataWriterMock.Setup(
				x => x.AddContent(
					It.Is<string>(key => key == Constants.UniversalXmlMetadata.UpdateType),
					It.IsAny<string>())).Verifiable();

			sourceDataWriterMock.Setup(
				x => x.AddContent(
					It.Is<string>(key => key == Constants.UniversalXmlMetadata.InclusiveEndDate),
					It.IsAny<string>())).Verifiable();

			sourceDataWriterMock.Setup(
				x => x.AddXmlContent(It.Is<string>(value => value.StartsWith("<Schema>")))).Verifiable();

			sourceDataWriterMock.Setup(
				x => x.AddXmlContent(It.Is<string>(value => value.StartsWith("<Dependency>")))).Verifiable();

			stagingRepositoryWrapperMock.Setup(
				x => x.CreateRecord(
					It.Is<string>(entityTypeName => entityTypeName == "RefCusTariff"),
					It.IsAny<string>(),
					It.IsAny<int>(),
					It.IsAny<Guid>())).Verifiable();

			sourceDataWriterMock.Setup(x => x.SetFlagAllRecordsAreProcessed()).Verifiable();

			var parser = Container.Resolve<IUniversalXmlParser>();
			Assert.IsNotNull(parser);

			using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(UniversalReferenceDataXml))
			{
				if (stream != null)
				{
					await parser.ParseAsync(stream, true);
					sourceDataWriterMock.Verify();
					stagingRepositoryWrapperMock.Verify();
				}
			}
		}

		[Test]
		public async Task TestParserAsync_NoAppNameInXml()
		{
			sourceDataWriterMock.Setup(
				x => x.AddContent(
					It.Is<string>(key => key == Constants.UniversalXmlMetadata.AppName),
					It.IsAny<string>())).Verifiable();
			var parser = Container.Resolve<IUniversalXmlParser>(new ParameterOverride("appName", "TestXmlPruduxer.exe"));
			Assert.IsNotNull(parser);

			using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(RefCusCodelist_NoAppName))
			{
				if (stream != null)
				{
					await parser.ParseAsync(stream, true);
					sourceDataWriterMock.Verify();
					stagingRepositoryWrapperMock.Verify();
				}
			}
		}

		[Test]
		public async Task TestParserAsync_StillProcessed_WhenErrorsAreNotAtRootLevel()
		{
			stagingRepositoryWrapperMock.Setup(
				x => x.CreateRecord(
					It.Is<string>(entityTypeName => entityTypeName == "RefCusCodeList"),
					It.IsAny<string>(),
					It.IsAny<int>(),
					It.IsAny<Guid>())).Verifiable();

			sourceDataWriterMock.Setup(x => x.SetFlagAllRecordsAreProcessed()).Verifiable();

			var parser = Container.Resolve<IUniversalXmlParser>();
			Assert.IsNotNull(parser);

			using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(RefCusCodeListTestFileError))
			{
				if (stream != null)
				{
					await parser.ParseAsync(stream, true);
					sourceDataWriterMock.Verify();
					stagingRepositoryWrapperMock.Verify();
				}
			}
		}

		[Test]
		public void TestParserAsync_ThrowsExceptionWhenTimeout()
		{
			var bulkInsertSize = 10;
			var count = 0;

			stagingRepositoryWrapperMock.Setup(
				x => x.CreateRecord(
					It.IsAny<string>(),
					It.IsAny<string>(),
					It.IsAny<int>(),
					It.IsAny<Guid>())).Callback(() =>
					{
						count++;
						if (count == bulkInsertSize)
						{
							stagingRepositoryWrapperMock.Setup(x => x.CheckBulkInsert).Returns(true);
						}
					});

			parserConfigMock.SetupProperty(x => x.BulkInsertSize, bulkInsertSize);
			sourceDataWriterMock.Setup(x => x.GetSourceName()).Returns("testing");
			var exception = new SqlExceptionBuilder().WithErrorNumber(KnownSqlExceptionsNumbers.Timeout).Build();
			stagingRepositoryWrapperMock.Setup(x => x.ExecuteBulkInsertAsync()).Throws(exception);

			var parser = Container.Resolve<IUniversalXmlParser>();
			Assert.IsNotNull(parser);

			using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(RefCusTariff_20Tariffs))
			{
				if (stream != null)
				{
					Assert.That(() => parser.ParseAsync(stream, true), Throws.Exception.TypeOf<SqlException>());
				}
			}
		}

		[Test]
		public async Task UniversalXmlParser_ParseAsync_SchemaAndContentMatch()
		{
			var schema = new Mock<IUniversalXmlSchemaHandler>();
			var sourceDataWriter = new Mock<ISourceDataWriter>();
			var fileTrace = new Mock<IFileTrace>();
			var parser = new UniversalXmlParser(schema.Object, stagingRepositoryWrapperMock.Object, sourceDataWriter.Object, fileTrace.Object);

			using (Stream fs = Assembly.GetExecutingAssembly().GetManifestResourceStream(@"CargoWise.RefDbRepo.UniversalXmlParser.Test.TestFiles.ScheamAndContentMatchTestFile.xml"))
			{
				await parser.ParseAsync(fs, false);
			}
			stagingRepositoryWrapperMock.Verify(x => x.CreateRecord("RefCusTariff", It.IsAny<string>(), It.IsAny<int>(), It.IsAny<Guid>()), Times.Once);
		}

		[Test]
		public async Task UniversalXmlParser_ParseAsync_SchemaAndContentDoesNotMatch()
		{
			var schema = new Mock<IUniversalXmlSchemaHandler>();
			var sourceDataWriter = new Mock<ISourceDataWriter>();
			var fileTrace = new Mock<IFileTrace>();
			var parser = new UniversalXmlParser(schema.Object, stagingRepositoryWrapperMock.Object, sourceDataWriter.Object, fileTrace.Object);

			using (Stream fs = Assembly.GetExecutingAssembly().GetManifestResourceStream(@"CargoWise.RefDbRepo.UniversalXmlParser.Test.TestFiles.ScheamAndContentDoesNotMatchTestFile.xml"))
			{
				await parser.ParseAsync(fs, false);
			}
			stagingRepositoryWrapperMock.Verify(x => x.CreateRecord("RefCusTariff", It.IsAny<string>(), It.IsAny<int>(), It.IsAny<Guid>()), Times.Once);
		}

		[Test]
		public async Task ParseAsync_WriteErrorIfNoData()
		{
			var originalWriter = Console.Out;
			var stringBuilder = new StringBuilder();
			using var stringWriter = new StringWriter(stringBuilder);
			try
			{
				Console.SetOut(stringWriter);
				var parser = new UniversalXmlParser(universalXmlSchemaHandlerMock.Object, stagingRepositoryWrapperMock.Object, sourceDataWriterMock.Object, fileTraceMock.Object);
				var xmlStr = @"<UniversalReferenceData><DataSource>Source A</DataSource><PublicationTime>2025-01-01T00:00:00</PublicationTime><UpdateType>Full</UpdateType><Schema>
<EntityType Name=""RefCusTariff"" Data=""true""><Key><PropertyRef Name=""ZZ1_TariffCode"" /></Key><Property Name=""ZZ1_TariffCode"" Type=""varchar"" /></EntityType></Schema></UniversalReferenceData>";
				using var stream1 = new MemoryStream(Encoding.UTF8.GetBytes(xmlStr));
				await parser.ParseAsync(stream1, false);
				sourceDataWriterMock.Verify(x => x.SetFlagErrorOnRootLevel(), Times.Once);
				Assert.That(stringBuilder.ToString(), Does.Contain("The provided XML doesn't have valid data"));

				stringBuilder.Clear();
				sourceDataWriterMock.Invocations.Clear();
				xmlStr = xmlStr.Replace("</UniversalReferenceData>", "<RefCusTariff><ZZ1_TariffCode>3926909790</ZZ1_TariffCode></RefCusTariff></UniversalReferenceData>");
				using var stream2 = new MemoryStream(Encoding.UTF8.GetBytes(xmlStr));
				await parser.ParseAsync(stream2, false);
				sourceDataWriterMock.Verify(x => x.SetFlagErrorOnRootLevel(), Times.Never);
				Assert.That(stringBuilder.ToString(), Does.Not.Contain("The provided XML doesn't have valid data"));
			}
			finally
			{
				Console.SetOut(originalWriter);
			}
		}

		[Test]
		public async Task ParseAsync_DuplicateXmls()
		{
			sourceDataWriterMock.Setup(x => x.CheckDuplicateExists(It.IsAny<string>(), It.IsAny<string>()))
				.Returns(true);
			var xmlStr = @"<UniversalReferenceData><DataSource>Source A</DataSource><PublicationTime>2025-01-01T00:00:00</PublicationTime><UpdateType>Full</UpdateType><Schema>
<EntityType Name=""RefCusTariff"" Data=""true""><Key><PropertyRef Name=""ZZ1_TariffCode"" /></Key><Property Name=""ZZ1_TariffCode"" Type=""varchar"" /></EntityType></Schema></UniversalReferenceData>";
			var parser = new UniversalXmlParser(universalXmlSchemaHandlerMock.Object, stagingRepositoryWrapperMock.Object, sourceDataWriterMock.Object, fileTraceMock.Object);
			using var stream1 = new MemoryStream(Encoding.UTF8.GetBytes(xmlStr));
			await parser.ParseAsync(stream1, false);
			sourceDataWriterMock.Verify(x => x.SetFlagDuplicated(), Times.AtLeastOnce);
		}

		#region Helpers

		protected override void OnSetup()
		{
			base.OnSetup();

			InitializeMockObjects();

			Container.RegisterInstance(stagingRepositoryMock.Object);
			Container.RegisterInstance(valuesProviderMock.Object);
			Container.RegisterInstance(loggerMock.Object);
			Container.RegisterInstance(universalXmlSchemaHandlerMock.Object);
			Container.RegisterInstance(stagingRepositoryWrapperMock.Object);
			Container.RegisterInstance(sourceDataWriterMock.Object);
			Container.RegisterInstance(fileTraceMock.Object);
			Container.RegisterInstance(parserConfigMock.Object);

			stagingRepositoryWrapperMock.Setup(x => x.ExecuteBulkInsertAsync()).Returns(() => Task.FromResult(true)).Verifiable();
			stagingRepositoryWrapperMock.Setup(o => o.IsDataElement(It.IsIn(new string[] { "RefCusTariff", "RefCusCodeList", "RefCusTariffRelationship" }))).Returns(true);
		}

		void InitializeMockObjects()
		{
			stagingRepositoryMock = new Mock<IStagingRepository>();
			valuesProviderMock = new Mock<IEntityValuesProvider>();
			loggerMock = new Mock<ILog>();
			universalXmlSchemaHandlerMock = new Mock<IUniversalXmlSchemaHandler>();
			stagingRepositoryWrapperMock = new Mock<IStagingRepositoryWrapper>();
			sourceDataWriterMock = new Mock<ISourceDataWriter>();
			fileTraceMock = new Mock<IFileTrace>();
			parserConfigMock = new Mock<IParserConfig>();
		}

		#endregion
	}
}
