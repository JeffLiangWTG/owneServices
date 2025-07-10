using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Xml.Linq;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter;
using CargoWise.RefDbRepo.DEReferenceData.Testing;
using NUnit.Framework;
using RichardSzalay.MockHttp;

namespace CargoWise.RefDbRepo.DEReferenceData.Business.CodeLists.Testing
{
	[TestFixture]
	public abstract class CodeListsXMLTests<T, TKeyValues>
		where T : RefDataRepoModelEntityType
		where TKeyValues : IKeyValues
	{
		protected abstract string System { get; }

		protected abstract string CustomsCodeListIdentifier { get; }

		protected abstract string CodeType { get; }

		protected abstract string DownloadUrl { get; }

		protected abstract Func<string[], CodeListsParserXML<T, TKeyValues>> ParserToRun { get; }

		protected abstract string InputFileName { get; }

		protected XDocument GetResultAsXDocument()
		{
			if (resultAsXDocument == null)
			{
				using (var webServiceMockStream = assembly.GetManifestResourceStream($"{CodeListsTestHelper.TestFilesManifestBasePath}.{System}.TestFiles.Input.{InputFileName}.xml"))
				{
					var client = CreateMockHttpHandler(webServiceMockStream);
					ParserToRun(DownLoadList).DownloadAndConvertToRefCusCodeListXML(client, outputPath).Wait();
				}
				resultAsXDocument = XDocument.Load(outputFile);
			}
			return resultAsXDocument;
		}
		XDocument resultAsXDocument;


		[Test]
		public void TestCodeTypeIsInValidCodeLists()
		{
			Assert.That(CodeListsConstants.ValidCodeLists, Does.Contain(CodeType));
		}

		[Test]
		public void CreateRefCusCodeListXMLTest()
		{
			using (var webServiceMockStream = assembly.GetManifestResourceStream($"{CodeListsTestHelper.TestFilesManifestBasePath}.{System}.TestFiles.Input.{InputFileName}.xml"))
			{
				var client = CreateMockHttpHandler(webServiceMockStream);
				ParserToRun(DownLoadList).DownloadAndConvertToRefCusCodeListXML(client, outputPath).Wait();
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

		void AssertCorrectErrorMessage(string expectedErrorMessage)
		{
			using (var webServiceMockStream = assembly.GetManifestResourceStream($"{CodeListsTestHelper.TestFilesManifestBasePath}.{System}.TestFiles.Input.{InputFileName}_FAULTY.xml"))
			{
				var client = CreateMockHttpHandler(webServiceMockStream);
				var errors = ParserToRun(DownLoadList).DownloadAndConvertToRefCusCodeListXML(client, outputPath).Result;
				Assert.That(errors, Does.Contain(expectedErrorMessage));
			}
		}

		protected void AssertAttributeWithValue(string code, string attributeName, string attributeValue)
		{
			var codeNode = GetResultAsXDocument().Descendants("ZZD_Code").Where(n => n.Value == code).FirstOrDefault().Parent;
			var attributeNode = codeNode.Descendants("ZZE_ZXE_NKName").Where(n => n.Value == attributeName).FirstOrDefault()?.Parent;
			Assert.IsNotNull(attributeNode, "on code {0}, attribute {1} should exist", code, attributeName);
			Assert.AreEqual(attributeValue, attributeNode?.Element("ZZE_Value").Value, "on code {0}, attribute {1} should have value {2}", code, attributeName, attributeValue);
		}

		protected void AssertNoAttribute(string code, string attributeName)
		{
			var codeNode = GetResultAsXDocument().Descendants("ZZD_Code").Where(n => n.Value == code).FirstOrDefault().Parent;
			var attributeNode = codeNode.Descendants("ZZE_ZXE_NKName").Where(n => n.Value == attributeName).FirstOrDefault()?.Parent;
			Assert.IsNull(attributeNode, "on code {0}, attribute {1} should not exist", code, attributeName);
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
						case "Export":
							result = CodeListsTestHelper.GetExportDownloadLinks();
							break;
						case "Import":
							result = CodeListsTestHelper.GetImportDownloadLinks();
							break;
						case "Ncts":
							result = CodeListsTestHelper.GetNctsDownloadLinks();
							break;
					}
					downLoadList = result;
				}
				return downLoadList;
			}
		}
		string[] downLoadList;

		HttpClient CreateMockHttpHandler(Stream webServiceMockStream)
		{
			using (var mockHttp = new MockHttpMessageHandler())
			{
				mockHttp.When(DownloadUrl).Respond("application/xml", webServiceMockStream);
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
