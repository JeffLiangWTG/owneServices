using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Reflection;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter;
using CargoWise.RefDbRepo.DEReferenceData.Services;
using CargoWise.RefDbRepo.DEReferenceData.Testing;
using Moq;
using NUnit.Framework;
using RichardSzalay.MockHttp;

namespace CargoWise.RefDbRepo.DEReferenceData.Business.CodeLists.Testing
{
	[TestFixture]
	public abstract class CodeListsTSVTests<T, TKeyValues>
		where T : RefDataRepoModelEntityType
		where TKeyValues : IKeyValues
	{
		protected abstract string System { get; }

		protected abstract string CustomsCodeListIdentifier { get; }

		protected abstract string CodeType { get; }

		protected abstract string DownloadUrl { get; }

		protected abstract string InputFileName { get; }

		protected abstract Func<Dictionary<string, string>, CodeListsParserTSV<T, TKeyValues>> ParserToRun { get; }

		[Test]
		public void TestCodeTypeIsInValidCodeLists()
		{
			Assert.That(CodeListsConstants.ValidCodeLists, Does.Contain(CodeType));
		}

		[Test]
		public void CreateRefCusCodeListXMLTest()
		{
			using (var webServiceMockStream = assembly.GetManifestResourceStream($"{CodeListsTestHelper.TestFilesManifestBasePath}.{System}.TestFiles.Input.{InputFileName}.{CodeListsDownloadFormat}"))
			{
				var client = CreateMockHttpHandler(webServiceMockStream);
				ParserToRun(DownloadLinks).DownloadAndConvertToRefCusCodeListXML(client, outputPath, DateTimeProviderMock.Object);
				var expectedXML = TestHelper.ReadManifestResourceContent($"{CodeListsTestHelper.TestFilesManifestBasePath}.{System}.TestFiles.Output.{CodeListsConstants.OutputFileNamePrefix}{CodeType}.xml");
				var actualUniversalXml = File.ReadAllText(outputFile);
				Assert.That(actualUniversalXml, Is.EqualTo(expectedXML));
			}
		}

		[Test]
		public void EmptyCodeErrorMessageTest()
		{
			if (this is ITestEmptyCodeErrorMessage parserTester)
			{
				AssertCorrectErrorMessage(parserTester.ExpectedEmptyCodeErrorMessage);
			}
		}

		[Test]
		public void EmptyDescriptionErrorMessageTest()
		{
			if (this is ITestEmptyDescriptionErrorMessage parserTester)
			{
				AssertCorrectErrorMessage(parserTester.ExpectedEmptyDescriptionErrorMessage);
			}
		}

		[Test]
		public void InvalidDateErrorMessageTest()
		{
			if (this is ITestInvalidDateErrorMessage parserTester)
			{
				AssertCorrectErrorMessage(parserTester.ExpectedInvalidDateErrorMessage);
			}
		}

		protected virtual Dictionary<string, string> DownloadLinks => CodeListsTestHelper.EMCSDownloadCodeList($"{CodeListsTestHelper.TestFilesManifestBasePath}.Emcs.TestFiles.EMCS_CODELISTS_DOWNLOAD_PAGE.html");

		protected HttpClient CreateMockHttpHandler(Stream webServiceMockStream)
		{
			using (var mockHttp = new MockHttpMessageHandler())
			{
				mockHttp.When(DownloadUrl).Respond("application/zip", webServiceMockStream);
				return mockHttp.ToHttpClient();
			}
		}

		protected void AssertCorrectErrorMessage(string expectedErrorMessage)
		{
			using (var webServiceMockStream = assembly.GetManifestResourceStream($"{CodeListsTestHelper.TestFilesManifestBasePath}.{System}.TestFiles.Input.{InputFileName}_FAULTY.{CodeListsDownloadFormat}"))
			{
				var client = CreateMockHttpHandler(webServiceMockStream);
				var errors = ParserToRun(DownloadLinks).DownloadAndConvertToRefCusCodeListXML(client, outputPath, DateTimeProviderMock.Object);
				Assert.That(errors, Does.Contain(expectedErrorMessage));
			}
		}

		protected virtual string CodeListsDownloadFormat => CodeListsConstants.Emcs.CodeListsDownloadFormat;

		[SetUp]
		public void Setup()
		{
			assembly = Assembly.GetExecutingAssembly();
			outputPath = Path.Combine(Path.GetDirectoryName(assembly.Location), @"DE\CodeList\");
			outputFile = Path.Combine(outputPath, $"{CodeListsConstants.OutputFileNamePrefix}{CodeType}.xml");
			DateTimeProviderMock = new Mock<IDateTimeProvider>();
			DateTimeProviderMock.Setup(x => x.CurrentLocalDate).Returns(new DateTime(2019, 2, 26));
		}
		Assembly assembly;
		string outputPath;
		string outputFile;
		Mock<IDateTimeProvider> DateTimeProviderMock;

		[TearDown]
		public void TearDown()
		{
			if (File.Exists(outputFile))
			{
				File.Delete(outputFile);
			}
		}
	}
}
