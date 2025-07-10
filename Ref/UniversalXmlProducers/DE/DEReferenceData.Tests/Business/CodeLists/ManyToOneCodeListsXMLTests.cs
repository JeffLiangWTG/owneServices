using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter;
using CargoWise.RefDbRepo.DEReferenceData.Testing;
using NUnit.Framework;
using RichardSzalay.MockHttp;

namespace CargoWise.RefDbRepo.DEReferenceData.Business.CodeLists.Testing
{
	[TestFixture]
	public abstract class ManyToOneCodeListsXMLTests<T, TKeyValues>
		where T : RefDataRepoModelEntityType
		where TKeyValues : IKeyValues
	{
		protected abstract string System { get; }

		protected abstract string[] CustomsCodeListIdentifiers { get; }

		protected abstract string CodeType { get; }

		protected abstract (string url, string system, string customsCodeListIdentifier)[] DownloadUrls { get; }

		protected abstract Func<string[], ManyToOneCodeListsParserXML<T, TKeyValues>> ParserToRun { get; }

		[Test]
		public void TestCodeTypeIsInValidCodeLists()
		{
			Assert.That(CodeListsConstants.ValidCodeLists, Does.Contain(CodeType));
		}

		[Test]
		public void CreateRefCusCodeListXMLTest()
		{
			var client = CreateMockHttpHandler(false);
			ParserToRun(DownLoadList).DownloadAndConvertToRefCusCodeListXML(client, outputPath).Wait();
			var expectedXML = TestHelper.ReadManifestResourceContent($"{CodeListsTestHelper.TestFilesManifestBasePath}.{System}.TestFiles.Output.{CodeListsConstants.OutputFileNamePrefix}{CodeType}.xml");
			var actualUniversalXml = File.ReadAllText(outputFile);
			Assert.That(actualUniversalXml, Is.EqualTo(expectedXML));
		}

		[Test]
		public void EmptyCodeErrorMessagePerListTest()
		{
			if (this is ITestManyEmptyCodeErrorMessages parserTester)
			{
				foreach (var expectedEmptyCodeErrorMessage in parserTester.ExpectedEmptyCodeErrorMessages)
				{
					AssertCorrectErrorMessage(true, expectedEmptyCodeErrorMessage);
				}
			}
		}

		[Test]
		public void EmptyDescriptionErrorMessagePerListTest()
		{
			if (this is ITestManyEmptyDescriptionErrorMessages parserTester)
			{
				foreach (var expectedEmptyDescriptionErrorMessage in parserTester.ExpectedEmptyDescriptionErrorMessages)
				{
					AssertCorrectErrorMessage(true, expectedEmptyDescriptionErrorMessage);
				}
			}
		}

		[Test]
		public void InvalidDateErrorMessagePerListTest()
		{
			if (this is ITestManyInvalidDateErrorMessages parserTester)
			{
				foreach (var expectedInvalidDateErrorMessage in parserTester.ExpectedInvalidDateErrorMessages)
				{
					AssertCorrectErrorMessage(true, expectedInvalidDateErrorMessage);
				}
			}
		}

		protected void AssertCorrectErrorMessage(bool faulty, string expectedErrorMessage)
		{
			var client = CreateMockHttpHandler(faulty);
			var errors = ParserToRun(DownLoadList).DownloadAndConvertToRefCusCodeListXML(client, outputPath).Result;
			Assert.That(errors, Does.Contain(expectedErrorMessage));
		}

		string[] DownLoadList
		{
			get
			{
				if (downLoadList == null)
				{
					var result = Array.Empty<string>();
					switch (System)
					{
						case "Generic":
						case "Export":
						case "Import":
							result = CodeListsTestHelper.GetExportDownloadLinks().Concat(CodeListsTestHelper.GetImportDownloadLinks()).ToArray();
							break;
						case "Ncts":
							result = CodeListsTestHelper.GetNctsDownloadLinks();
							break;
					}
					downLoadList = result.ToArray();
				}
				return downLoadList;
			}
		}
		string[] downLoadList;

		HttpClient CreateMockHttpHandler(bool faulty)
		{
			using (var mockHttp = new MockHttpMessageHandler())
			{
				var postfix = faulty ? "_FAULTY" : "";
				foreach (var (url, system, customsCodeListIdentifier) in DownloadUrls)
				{
					mockHttp.When(url).Respond("application/xml", assembly.GetManifestResourceStream($"{CodeListsTestHelper.TestFilesManifestBasePath}.{system}.TestFiles.Input.{customsCodeListIdentifier}{postfix}.xml"));
				}

				return mockHttp.ToHttpClient();
			}
		}

		[SetUp]
		public void Setup()
		{
			assembly = Assembly.GetExecutingAssembly();
			outputPath = Path.Combine(Path.GetDirectoryName(assembly.Location), @"DE\CodeList\");
			outputFile = Path.Combine(outputPath, $"{CodeListsConstants.OutputFileNamePrefix}{CodeType}.xml");
		}
		Assembly assembly;
		string outputPath;
		string outputFile;

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
