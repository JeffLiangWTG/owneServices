using System;
using System.IO;
using System.Reflection;
using System.Xml.Linq;
using CargoWise.RefDbRepo.CHReferenceData.Business;
using CargoWise.RefDbRepo.CHReferenceData.Services;
using NUnit.Framework;
using NUnit.Framework.Internal;

namespace CargoWise.RefDbRepo.CHReferenceData.Tests.ConditionTypes
{
	[TestFixture]
	class PassarConditionTypesParserTest
	{
		protected string TestInput => "TestFiles.Input.PassarConditionTypes.xml";

		protected string ExpectedTestOutput => "TestFiles.Output.PassarConditionTypes_converted.xml";

		protected string TestOutputDir => Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), $@"CH\TestFiles\{GetType().Name}");

		[Test]
		public void TestParser()
		{
			DownloadResult download = new DownloadResult
			{
				Content = GetType().GetTestArray(TestInput),
			};

			var parser = new PassarConditionTypesParser(download);

			parser.ConvertToRefXML(TestOutputDir, DefaultTestDate);
			string outputFile = Path.Combine(TestOutputDir, $"RefCusConditionTypeZZ_CH_Passar.xml");
			using (var actualStream = new FileStream(outputFile, FileMode.Open))
			using (var expectedStream = GetType().GetTestStream(ExpectedTestOutput))
			{
				var actualXml = TestHelper.RemoveIgnoreTagsFromXml(XDocument.Load(actualStream));
				var expectedXml = XDocument.Load(expectedStream);
				Assert.IsTrue(XNode.DeepEquals(actualXml, expectedXml), $"File content does not match ({outputFile})");
			}
		}

		[OneTimeSetUp]
		public void OneTimeSetUp()
		{
			TestHelper.DeleteTestDirectory(TestOutputDir);
		}

		[TearDown]
		public void TearDown()
		{
			TestHelper.DeleteLogFiles();
			TestHelper.DeleteTestDirectory(TestOutputDir);
		}

		readonly DateTime DefaultTestDate = new DateTime(2024, 1, 1);
	}
}
