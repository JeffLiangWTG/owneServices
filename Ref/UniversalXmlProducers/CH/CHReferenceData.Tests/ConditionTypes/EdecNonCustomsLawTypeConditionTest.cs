using System;
using System.IO;
using System.Reflection;
using System.Xml.Linq;
using CargoWise.RefDbRepo.CHReferenceData.Business;
using CargoWise.RefDbRepo.CHReferenceData.Services;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.CHReferenceData.Tests.ConditionTypes
{
	[TestFixture]
	class EdecNonCustomsLawTypeConditionTest
	{
		[Test]
		public void TestCreateUniversalNonCustomsLawTypesConditionTypesXML()
		{
			DownloadResult download = new DownloadResult
			{
				Content = GetType().GetTestArray("TestFiles.Input.edecDomains_1_0.xml"),
			};

			var parser = new EdecNonCustomsLawTypeParser(download);

			parser.ConvertToRefXML(testOutputDir, new DateTime(2021, 4, 7));
			string outputFile = Path.Combine(testOutputDir, "RefCusConditionTypeZZ_CH_NCLT.xml");
			try
			{
				using (var actualStream = new FileStream(outputFile, FileMode.Open))
				using (var expectedStream = GetType().GetTestStream($"TestFiles.Output.edecDomainsConditionTypes_1_0_converted_NCLT.xml"))
				{
					var actualXml = TestHelper.RemoveIgnoreTagsFromXml(XDocument.Load(actualStream));;
					var expectedXml = XDocument.Load(expectedStream);
					Assert.IsTrue(XNode.DeepEquals(actualXml, expectedXml), $"File content does not match: NCLT ({outputFile})");
				}
			}
			finally
			{
				DeleteTestOutputFile(outputFile);
			}
		}

		void DeleteTestOutputFile(string outputFile)
		{
			if (File.Exists(outputFile))
			{
				File.Delete(outputFile);
			}
		}

		[OneTimeSetUp]
		public void OneTimeSetUp()
		{
			testOutputDir = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), @"CH\TestFiles\NonCustomsLawTypes");
		}

		[TearDown]
		public void TearDown()
		{
			TestHelper.DeleteLogFiles();
		}

		protected string testOutputDir;
	}
}
