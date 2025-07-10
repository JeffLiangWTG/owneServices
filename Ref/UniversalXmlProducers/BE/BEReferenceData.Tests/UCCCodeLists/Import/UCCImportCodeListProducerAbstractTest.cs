using System;
using System.IO;
using System.Linq;
using System.Reflection;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;
using FlexCel.XlsAdapter;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.BEReferenceData.Business.Testing
{
	[TestFixture]
	public abstract class UCCImportCodeListProducerAbstractTest<T>
		where T : UCCImportCodeListProducer<RefCusCodeList>
	{
		protected abstract string CodeListType { get; }
		protected abstract string CodeType { get; }
		protected abstract string DataSource { get; }
		protected abstract string UccCodeListName { get; }
		protected virtual string Domain => "Import";

		[Test]
		public void TestCodeListDetail()
		{
			var codeListDetail = Producer.CodeListDetail;

			Assert.That(codeListDetail.CodeListType, Is.EqualTo(CodeListType));
			Assert.That(codeListDetail.CodeType, Is.EqualTo(CodeType));
			Assert.That(codeListDetail.DataSource, Is.EqualTo(DataSource));
			Assert.That(codeListDetail.Domain, Is.EqualTo(Domain));
			Assert.That(codeListDetail.XmlWriterConfiguration, !Is.Null);
		}

		[Test]
		public virtual void TestParseExcel()
		{
			var result = Producer.ParseExcel(xlsFile, UccCodeListName);
			var fileName = "RefCusCodeListZZ_BE_" + CodeType + ".xml";
			var outputFile = Path.Combine(outputPath, fileName);
			var publicationTime = new DateTime(2024, 3, 18);

			XMLGeneration.ExportToXMLFile(DataSource, outputFile, Producer.CodeListDetail.XmlWriterConfiguration, publicationTime, result);

			var fileContent = File.ReadAllText(outputFile);
			var expectedContent = File.ReadAllText(Path.Combine(testFilesOutputPath, fileName));

			BEReferenceDataTestHelper.AssertEqualXML(fileContent, expectedContent);
		}

		[SetUp]
		public void Setup()
		{
			Producer = (T)Activator.CreateInstance(typeof(T));
			var assembly = Assembly.GetExecutingAssembly();
			var testFilesInputPath = Path.Combine(Path.GetDirectoryName(assembly.Location), @"UCCCodeLists\Import\TestFiles\Input");
			var filenames = Directory.GetFiles(testFilesInputPath, "231010Import_Codelist.xlsx");
			xlsFile = new XlsFile(filenames.First(), false);

			outputPath = Path.Combine(Path.GetDirectoryName(assembly.Location), @"UCCCodeLists\Import\Output");
			testFilesOutputPath = Path.Combine(Path.GetDirectoryName(assembly.Location), @"UCCCodeLists\Import\TestFiles\Output");
		}

		internal UCCImportCodeListProducer<RefCusCodeList> Producer;
		internal XlsFile xlsFile;
		internal string outputPath;
		internal string testFilesOutputPath;
	}
}
