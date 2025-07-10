using System.IO;
using System.Reflection;
using System.Xml.Linq;
using CargoWise.RefDbRepo.CHReferenceData.Business.CodeLists;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.CHReferenceData.Tests.CodeLists
{
	abstract class BaseCodeListsTest<INPUT> where INPUT : IInputDoc
	{
		protected string InsertListTypeIntoFilePath(string outputFilePath, string listType)
		{
			return outputFilePath.Insert(outputFilePath.LastIndexOf("."), listType);
		}

		protected void CompareOutputFileContent(string actualBaseFilePath, string expectedResourceBasePath, string listType)
		{
			var actualFilePath = InsertListTypeIntoFilePath(actualBaseFilePath, listType);
			var actualResourcePath = InsertListTypeIntoFilePath(expectedResourceBasePath, listType);
			try
			{
				using (var actualStream = new FileStream(actualFilePath, FileMode.Open))
				using (var expectedStream = GetType().GetTestStream(actualResourcePath))
				{
					var actualXml = TestHelper.RemoveIgnoreTagsFromXml(XDocument.Load(actualStream));
					var expectedXml = XDocument.Load(expectedStream);
					Assert.IsTrue(XNode.DeepEquals(actualXml, expectedXml), $"File content does not match: {listType} ({actualBaseFilePath})");
				}
			}
			finally
			{
				DeleteTestOutputFile(actualBaseFilePath);
			}
		}

		protected void DeleteTestOutputFile(string outputFile)
		{
			if (File.Exists(outputFile))
			{
				File.Delete(outputFile);
			}
		}

		protected abstract BaseCodeListsParser<INPUT> CreateParser();

		protected abstract string ExpectedLogFileName { get; }

		[Test]
		public void TestLogFileName()
		{
			var parser = CreateParser();
			Assert.That(Path.GetDirectoryName(parser.LogFilePath), Is.EqualTo(Path.GetDirectoryName(parser.GetType().Assembly.Location)));
			Assert.That(Path.GetFileName(parser.LogFilePath), Is.EqualTo(ExpectedLogFileName));
		}

		[OneTimeSetUp]
		public void OneTimeSetUp()
		{
			testOutputDir = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), @"CH\TestFiles\CodeLists");
		}
		protected string testOutputDir;

		[SetUp]
		public void SetUp()
		{
			DeleteTestDir();
			TestHelper.DeleteLogFiles();
			Directory.CreateDirectory(testOutputDir);
		}

		[TearDown]
		public void TearDown()
		{
			DeleteTestDir();
			TestHelper.DeleteLogFiles();
		}

		void DeleteTestDir()
		{
			if (Directory.Exists(testOutputDir))
			{
				Directory.Delete(testOutputDir, true);
			}
		}
	}
}
