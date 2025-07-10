using System;
using System.IO;
using System.Reflection;
using System.Xml.Linq;
using System.Xml.XPath;
using CargoWise.RefDbRepo.CHReferenceData.Business;
using CargoWise.RefDbRepo.CHReferenceData.Services;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.CHReferenceData.Tests.ConditionTypes
{
	[TestFixture]
	internal class EdecPermitAuthoritiesParserTest
	{
		[Test]
		public void TestParser()
		{
			DownloadResult download = new DownloadResult
			{
				Content = GetType().GetTestArray("TestFiles.Input.PermitAuthorities.xml"),
			};

			var parser = new EdecPermitAuthoritiesParser(download);

			parser.ConvertToRefXML(testOutputDir, new DateTime(2021, 4, 7));
			string outputFile = Path.Combine(testOutputDir, "RefCusConditionTypeZZ_CH_PA.xml");
			using (var actualStream = new FileStream(outputFile, FileMode.Open))
			using (var expectedStream = GetType().GetTestStream("TestFiles.Output.PermitAuthorities_converted.xml"))
			{
				var actualXml = TestHelper.RemoveIgnoreTagsFromXml(XDocument.Load(actualStream));;
				var expectedXml = XDocument.Load(expectedStream);
				Assert.IsTrue(XNode.DeepEquals(actualXml, expectedXml), $"File content does not match: PA ({outputFile})");
			}
		}

		[Test]
		public void TestValidityDates()
		{
			var parser = new EdecPermitAuthoritiesParser(new DownloadResult()
			{
				Content = GetType().GetTestArray($"TestFiles.Input.PermitAuthorities_{nameof(TestValidityDates)}.xml"),
			});

			parser.ConvertToRefXML(testOutputDir, new DateTime(2023, 3, 13));
			string outputFile = Path.Combine(testOutputDir, "RefCusConditionTypeZZ_CH_PA.xml");
			var xml = XDocument.Load(outputFile);

			Assert.Multiple(() =>
			{
				Assert.That(xml.XPathSelectElement("//RefCusConditionType[ZX2_ConditionType='PA1']"), Is.Not.Null, "valid today");
				Assert.That(xml.XPathSelectElement("//RefCusConditionType[ZX2_ConditionType='PA2']"), Is.Not.Null, "valid in the future");
				Assert.That(xml.XPathSelectElement("//RefCusConditionType[ZX2_ConditionType='PA3']"), Is.Not.Null, "valid in the past");
			});
		}

		[Test]
		public void TestActualDescription()
		{
			var parser = new EdecPermitAuthoritiesParser(new DownloadResult()
			{
				Content = GetType().GetTestArray($"TestFiles.Input.PermitAuthorities_{nameof(TestActualDescription)}.xml"),
			});

			parser.ConvertToRefXML(testOutputDir, new DateTime(2023, 3, 13));
			string outputFile = Path.Combine(testOutputDir, "RefCusConditionTypeZZ_CH_PA.xml");
			var xml = XDocument.Load(outputFile);

			Assert.Multiple(() =>
			{
				Assert.That(xml.XPathSelectElement("//RefCusConditionType[ZX2_ConditionType='PA1']/ZX2_Description")?.Value, Is.EqualTo("past"), "valid in the past only");
				Assert.That(xml.XPathSelectElement("//RefCusConditionType[ZX2_ConditionType='PA2']/ZX2_Description")?.Value, Is.EqualTo("actual"), "valid today");
				Assert.That(xml.XPathSelectElement("//RefCusConditionType[ZX2_ConditionType='PA3']/ZX2_Description")?.Value, Is.EqualTo("future"), "valid in the future only");
				Assert.That(xml.XPathSelectElement("//RefCusConditionType[ZX2_ConditionType='PA4']/ZX2_Description")?.Value, Is.EqualTo("actual"), "valid in the past and today");
				Assert.That(xml.XPathSelectElement("//RefCusConditionType[ZX2_ConditionType='PA5']/ZX2_Description")?.Value, Is.EqualTo("actual"), "valid today and in the future");
				Assert.That(xml.XPathSelectElement("//RefCusConditionType[ZX2_ConditionType='PA6']/ZX2_Description")?.Value, Is.EqualTo("actual"), "valid in the past, today and int e future");
			});
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
			testOutputDir = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), $@"CH\TestFiles\{nameof(EdecPermitAuthoritiesParserTest)}");
			TestHelper.DeleteTestDirectory(testOutputDir);
		}

		[TearDown]
		public void TearDown()
		{
			TestHelper.DeleteLogFiles();
			TestHelper.DeleteTestDirectory(testOutputDir);
		}

		protected string testOutputDir;
	}
}
