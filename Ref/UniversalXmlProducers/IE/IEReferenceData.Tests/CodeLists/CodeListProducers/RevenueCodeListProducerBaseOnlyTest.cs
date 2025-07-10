using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using CargoWise.RefDbRepo.IEReferenceData.CodeLists.Business;
using CargoWise.RefDbRepo.IEReferenceData.CodeLists.Services;
using CargoWise.RefDbRepo.IEReferenceData.Services;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.IEReferenceData.CodeLists.Tests
{
	class RevenueCodeListProducerBaseOnlyTest
	{
		[Test]
		public void InvalidCodeListNoXmlProduced()
		{
			var codeListList = new List<RevenueCodeDescriptionPairProvider>()
			{
				new RevenueCodeDescriptionPairProvider("", "Des AAA"),
				new RevenueCodeDescriptionPairProvider("BBB", "")
			};
			var errors = producer.ConvertCodeListToXml(codeListList, DateTime.Today, outputPath, Constants.IECountryCode);
			Assert.That(errors, Does.EndWith($"There were no valid records for {TestCodeType}, unable to generate XML file.\r\n"));
		}

		[Test]
		public void EmptyCodeError()
		{
			const string expectedErrorMessage = @"Unable to import record from AISProducerBaseTest due to empty Code or Description. DETAILS:
Code: 
Description: Des BBB
";
			var codeListList = new List<RevenueCodeDescriptionPairProvider>()
			{
				new RevenueCodeDescriptionPairProvider("AAA", "Des AAA"),
				new RevenueCodeDescriptionPairProvider("", "Des BBB")
			};
			var errors = producer.ConvertCodeListToXml(codeListList, DateTime.Today, outputPath, Constants.IECountryCode);
			Assert.That(errors, Is.EqualTo(expectedErrorMessage));
		}

		[Test]
		public void EmptyDescriptionError()
		{
			const string expectedErrorMessage = @"Unable to import record from AISProducerBaseTest due to empty Code or Description. DETAILS:
Code: AAA
Description: 
";
			var codeListList = new List<RevenueCodeDescriptionPairProvider>()
			{
				new RevenueCodeDescriptionPairProvider("AAA", ""),
				new RevenueCodeDescriptionPairProvider("BBB", "Des BBB")
			};
			var errors = producer.ConvertCodeListToXml(codeListList, DateTime.Today, outputPath, Constants.IECountryCode);
			Assert.That(errors, Is.EqualTo(expectedErrorMessage));
		}

		[Test]
		public void MultipleErrors()
		{
			var expectedErrorMessage = $@"Unable to import record from AISProducerBaseTest due to empty Code or Description. DETAILS:
Code: AAA
Description: 
Unable to import record from AISProducerBaseTest due to empty Code or Description. DETAILS:
Code: 
Description: Des BBB
There were no valid records for {TestCodeType}, unable to generate XML file.
";
			var codeListList = new List<RevenueCodeDescriptionPairProvider>()
			{
				new RevenueCodeDescriptionPairProvider("AAA", ""),
				new RevenueCodeDescriptionPairProvider("", "Des BBB")
			};
			var errors = producer.ConvertCodeListToXml(codeListList, DateTime.Today, outputPath, Constants.IECountryCode);
			Assert.That(errors, Is.EqualTo(expectedErrorMessage));
		}

		[Test]
		public void OutputPathError()
		{
			var codeListList = new List<RevenueCodeDescriptionPairProvider>();
			var errors = producer.ConvertCodeListToXml(codeListList, DateTime.Today, string.Empty, Constants.IECountryCode);
			Assert.That(errors, Is.EqualTo($"Output file path is empty for Producer of type {TestCodeType}\r\n"));
		}

		[OneTimeSetUp]
		public void Setup()
		{
			assembly = Assembly.GetExecutingAssembly();
			outputPath = Path.Combine(Path.GetDirectoryName(assembly.Location), @"CodeLists\TestFiles\Output\");
			actualOutputFileNameAndPath = Path.Combine(outputPath, $"RefCusCodeListZZ_{Constants.IECountryCode}_{TestCodeType}.xml");
			producer = new RevenueRefCusCodeListProducer(TestCodeType);
		}
		Assembly assembly;
		string outputPath;
		string actualOutputFileNameAndPath;
		RevenueRefCusCodeListProducer producer;

		[TearDown]
		public void TearDown()
		{
			if (File.Exists(actualOutputFileNameAndPath))
			{
				File.Delete(actualOutputFileNameAndPath);
			}
		}

		const string TestCodeType = "AISProducerBaseTest";
	}
}
