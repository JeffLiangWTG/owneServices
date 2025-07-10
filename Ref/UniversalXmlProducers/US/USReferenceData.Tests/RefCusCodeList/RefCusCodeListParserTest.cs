using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using CargoWise.RefDbRepo.USReferenceData.Business;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.USReferenceData.Tests
{
	class RefCusCodeListParserTest
	{
		[Test]
		public void TestRefCusCodeList_NoDefaultDate()
		{
			AssertRefCodeListXML(new RefCusCodeList_NoDefaultDateParser(), "CargoWise.RefDbRepo.USReferenceData.Tests.RefCusCodeList.TestFiles.ExpectedRefCusCodeList_NoDefaultDate.xml");
		}

		[Test]
		public void TestRefCusCodeList_HasDefaultDate()
		{
			AssertRefCodeListXML(new RefCusCodeList_HasDefaultDateParser(), "CargoWise.RefDbRepo.USReferenceData.Tests.RefCusCodeList.TestFiles.ExpectedRefCusCodeList_HasDefaultDate.xml");
		}

		[Test]
		public void TestRefCusCodeList_NoDefaultDate_HasAttributes()
		{
			AssertRefCodeListXML(new RefCusCodeList_NoDefaultDate_HasAttributesParser(), "CargoWise.RefDbRepo.USReferenceData.Tests.RefCusCodeList.TestFiles.ExpectedRefCusCodeList_NoDefaultDate_HasAttributes.xml");
		}

		[Test]
		public void TestRefCusCodeList_HasDefaultDate_HasAttributes()
		{
			AssertRefCodeListXML(new RefCusCodeList_HasDefaultDate_HasAttributesParser(), "CargoWise.RefDbRepo.USReferenceData.Tests.RefCusCodeList.TestFiles.ExpectedRefCusCodeList_HasDefaultDate_HasAttributes.xml");
		}

		[Test]
		public void TestSuccessLog()
		{
			var parser = new RefCusCodeList_HasDefaultDateParser();
			var log = parser.ConvertCodeListToXML();
			Assert.That(log, Does.Contain("Start processing."));
			Assert.That(log, Does.Contain("XML file generation successful."));
			Assert.That(log, Does.Contain("Processing end..."));
		}

		[Test]
		public void TestFailureLog()
		{
			var parser = new RefCusCodeListParserForLog();
			var log = parser.ConvertCodeListToXML();
			Assert.That(log, Does.Contain("Start processing."));
			Assert.That(log, Does.Contain("The number of RefCusCodeList is 0, unable to generate XML file."));
			Assert.That(log, Does.Contain("Processing end..."));
		}

		void AssertRefCodeListXML<T>(RefCusCodeListParser_WithExposedOutputFilePath<T> parser, string exceptXMLPath)
			where T : CodeListProvider
		{
			parser.ConvertCodeListToXML();

			var exceptXML = TestHelper.ReadManifestResourceContent(exceptXMLPath);
			Assert.That(exceptXML, Is.EqualTo(File.ReadAllText(parser.OutputFilePath_Exposed)));
		}

		public abstract class RefCusCodeListParser_WithExposedOutputFilePath<T> : RefCusCodeListParser<T>
			where T : CodeListProvider
		{
			public string OutputFilePath_Exposed => OutputFilePath;
		}

		public class CodeListProvider : ICodeList
		{
			public string Code { get; set; }
			public string Description { get; set; }
		}

		public class CodeListWithDateProvider : CodeListProvider, IDateProvider
		{
			public DateTime StartDate { get; set; }
			public DateTime EndDate { get; set; }
		}

		public class CodeListAttributeProvider : ICodeListAttribute
		{
			public string Name { get; set; }
			public string Value { get; set; }
		}

		public class CodeListWithAttributesProvider : CodeListProvider, ICodeListAttributesProvider
		{
			public IEnumerable<ICodeListAttribute> Attributes { get; set; }
		}

		public class CodeListWithDateAndAttributesProvider : CodeListWithDateProvider, ICodeListAttributesProvider
		{
			public IEnumerable<ICodeListAttribute> Attributes { get; set; }
		}

		class RefCusCodeList_NoDefaultDateParser : RefCusCodeListParser_WithExposedOutputFilePath<CodeListWithDateProvider>
		{
			protected override Common.UniversalXmlWriter.XmlWriterConfiguration XmlWriterConfiguration => RefCusCodeListParserHelper.GetWriterConfiguration_NoDefaultDate("AAAAA");

			protected override string DataSourse => "RefCusCodeList_NoDefaultDate";

			protected override DateTime PublicationDateTime => new DateTime(2020, 04, 16);

			protected override Common.UniversalXmlWriter.UpdateType UpdateType => Common.UniversalXmlWriter.UpdateType.Full;

			protected override string OutputFilePath => Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), @"RefCusCodeList\TestFiles\RefCusCodeList_NoDefaultDate.xml");

			protected override List<CodeListWithDateProvider> GetCodeLists()
			{
				return new List<CodeListWithDateProvider>()
				{
					new CodeListWithDateProvider { Code = "AAA", Description = "Des AAA", StartDate = new DateTime(2021,1,1), EndDate = new DateTime(2021,4,1) },
					new CodeListWithDateProvider { Code = "BBB", Description = "", StartDate = new DateTime(2021,1,1), EndDate = new DateTime(2021,4,1) }
				};
			}
		}

		class RefCusCodeList_HasDefaultDateParser : RefCusCodeListParser_WithExposedOutputFilePath<CodeListProvider>
		{
			protected override Common.UniversalXmlWriter.XmlWriterConfiguration XmlWriterConfiguration => RefCusCodeListParserHelper.GetWriterConfiguration_HasDefaultDate("BBBBB", new DateTime(1900, 01, 01, 0, 0, 0), new DateTime(2079, 06, 06, 23, 59, 0));

			protected override string DataSourse => "RefCusCodeList_HasDefaultDate";

			protected override DateTime PublicationDateTime => new DateTime(2020, 04, 16);

			protected override Common.UniversalXmlWriter.UpdateType UpdateType => Common.UniversalXmlWriter.UpdateType.Full;

			protected override string OutputFilePath => Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), @"RefCusCodeList\TestFiles\RefCusCodeList_HasDefaultDate.xml");

			protected override List<CodeListProvider> GetCodeLists()
			{
				return new List<CodeListProvider>() {
					new CodeListProvider { Code = "AAA", Description = "Des AAA" },
					new CodeListProvider { Code = "BBB", Description = "" }
				};
			}
		}

		class RefCusCodeList_NoDefaultDate_HasAttributesParser : RefCusCodeListParser_WithExposedOutputFilePath<CodeListWithDateAndAttributesProvider>
		{
			protected override Common.UniversalXmlWriter.XmlWriterConfiguration XmlWriterConfiguration => RefCusCodeListParserHelper.GetWriterConfiguration_NoDefaultDate_HasAttributes("CCCCC");

			protected override string DataSourse => "RefCusCodeList Has No Default Date Has Attributes";

			protected override DateTime PublicationDateTime => new DateTime(2020, 04, 16);

			protected override Common.UniversalXmlWriter.UpdateType UpdateType => Common.UniversalXmlWriter.UpdateType.Full;

			protected override string OutputFilePath => Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), @"RefCusCodeList\TestFiles\RefCusCodeList_NoDefaultDate_HasAttributes.xml");

			protected override List<CodeListWithDateAndAttributesProvider> GetCodeLists()
			{
				return new List<CodeListWithDateAndAttributesProvider>() {
					new CodeListWithDateAndAttributesProvider { Code = "AAA", Description = "Des AAA", StartDate = new DateTime(2021,1,1), EndDate = new DateTime(2021,4,1), Attributes = new ICodeListAttribute[] { new CodeListAttributeProvider { Name = "CodeA", Value = "ValueA" } } },
					new CodeListWithDateAndAttributesProvider { Code = "BBB", Description = "", StartDate = new DateTime(2021,1,1), EndDate = new DateTime(2021,4,1), Attributes = new ICodeListAttribute[] { new CodeListAttributeProvider { Name = "CodeA", Value = "ValueA" }, new CodeListAttributeProvider { Name = "CodeA", Value = "ValueB" } } },
					new CodeListWithDateAndAttributesProvider { Code = "CCC", Description = "Des CCC", StartDate = new DateTime(2021,1,1), EndDate = new DateTime(2021,4,1), Attributes = new ICodeListAttribute[0] }
				};
			}
		}

		class RefCusCodeList_HasDefaultDate_HasAttributesParser : RefCusCodeListParser_WithExposedOutputFilePath<CodeListWithAttributesProvider>
		{
			protected override Common.UniversalXmlWriter.XmlWriterConfiguration XmlWriterConfiguration => RefCusCodeListParserHelper.GetWriterConfiguration_HasDefaultDate_HasAttributes("DDDDD", new DateTime(1900, 01, 01, 0, 0, 0), new DateTime(2079, 06, 06, 23, 59, 0));

			protected override string DataSourse => "RefCusCodeList Has Default Date And Attributes";

			protected override DateTime PublicationDateTime => new DateTime(2020, 04, 16);

			protected override Common.UniversalXmlWriter.UpdateType UpdateType => Common.UniversalXmlWriter.UpdateType.Full;

			protected override string OutputFilePath => Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), @"RefCusCodeList\TestFiles\RefCusCodeList_HasDefaultDate_HasAttributes.xml");

			protected override List<CodeListWithAttributesProvider> GetCodeLists()
			{
				return new List<CodeListWithAttributesProvider>() {
					new CodeListWithAttributesProvider { Code = "AAA", Description = "Des AAA", Attributes = new ICodeListAttribute[] { new CodeListAttributeProvider { Name = "CodeA", Value = "ValueA" } } },
					new CodeListWithAttributesProvider { Code = "BBB", Description = "", Attributes = new ICodeListAttribute[] { new CodeListAttributeProvider { Name = "CodeA", Value = "ValueA" }, new CodeListAttributeProvider { Name = "CodeA", Value = "ValueB" } } },
					new CodeListWithAttributesProvider { Code = "CCC", Description = "Des CCC", Attributes = new ICodeListAttribute[0] }
				};
			}
		}

		class RefCusCodeListParserForLog : RefCusCodeListParser_WithExposedOutputFilePath<CodeListProvider>
		{
			protected override Common.UniversalXmlWriter.XmlWriterConfiguration XmlWriterConfiguration => RefCusCodeListParserHelper.GetWriterConfiguration_HasDefaultDate("EEEEE", new DateTime(1900, 01, 01, 0, 0, 0), new DateTime(2079, 06, 06, 23, 59, 0));

			protected override string DataSourse => "RefCusCodeList_HasDefaultDate";

			protected override DateTime PublicationDateTime => new DateTime(2020, 04, 16);

			protected override Common.UniversalXmlWriter.UpdateType UpdateType => Common.UniversalXmlWriter.UpdateType.Full;

			protected override string OutputFilePath => Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), @"RefCusCodeList\TestFiles\RefCusCodeList.xml");

			protected override List<CodeListProvider> GetCodeLists()
			{
				return new List<CodeListProvider>();
			}
		}
	}
}
