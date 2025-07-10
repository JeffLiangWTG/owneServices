using System;
using System.Globalization;
using System.IO;
using System.Xml.Linq;
using CargoWise.RefDbRepo.EUReferenceData.Business;
using CargoWise.RefDbRepo.EUReferenceData.CommonXmlCoverter.Business;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.EUReferenceData.Tests
{
	abstract class CommonXmlDataParserTest<T> where T : CommonDataParser
	{
		[Test]
		public void TestParseXML()
		{
			var inputFile = GetXMLFile();
			var result = DataParser.ParseToRefCusCodeLists(XDocument.Load(inputFile));
			Assert.That(DataParser.ErrorStr, Is.Empty);
		}

		[Test]
		public void TestEmptyXmlfileErrorMessage()
		{
			string errorMsg = "XML file not found!";
			DataParser.ParseToRefCusCodeLists(null);
			Assert.That(DataParser.ErrorStr, Is.EqualTo(errorMsg));
		}

		[Test]
		public void TestInvalidErrorMessage()
		{
			var inputFile = GetInvalidXMLFile();
			string errorMsg = "This file is invalid!";
			DataParser.ParseToRefCusCodeLists(XDocument.Load(inputFile));
			Assert.That(DataParser.ErrorStr, Is.EqualTo(errorMsg));
		}

		[Test]
		public void TestNotFoundRDEntityErrorMessage()
		{
			var inputFile = GetRDEntityNotFoundErrorFile();
			string errorMsg = string.Format(CultureInfo.InvariantCulture, "Can not found RDEntity element name is {0}!", RDEntityAttributeValue);
			DataParser.ParseToRefCusCodeLists(XDocument.Load(inputFile));
			Assert.That(DataParser.ErrorStr, Is.EqualTo(errorMsg));
		}

		[Test]
		public void TestSupportAdditionalTranslations()
		{
			Assert.That(DataParser is IAdditionalTranslationSupporter, Is.EqualTo(IsAdditionalTranslationSupporter));
		}

		protected abstract Stream GetXMLFile();
		protected abstract Stream GetInvalidXMLFile();
		protected abstract Stream GetRDEntityNotFoundErrorFile();

		protected abstract string RDEntityAttributeValue { get; }

		protected virtual bool IsAdditionalTranslationSupporter { get; }

		[SetUp]
		public void Setup()
		{
			DataParser = (T)Activator.CreateInstance(typeof(T));
		}

		CommonDataParser DataParser;
	}
}
