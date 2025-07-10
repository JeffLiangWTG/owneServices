using System;
using System.IO;
using CargoWise.RefDbRepo.ILReferenceData.Business;
using NUnit.Framework;
using XMLTools;

namespace CargoWise.RefDbRepo.ILReferenceData.Tests.Business
{
	public abstract class CustomsResponseProcessorTestBase<TProcessor> where TProcessor : BaseCustomsResponseProcessor
	{
		[Test]
		public void TestGenerateFilesDataSet()
		{
			var testFilesFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"TestFiles\");
			var dataSetFileName = $"{DataSource}_DataSet.xml";
			var dataSetFilePath = Path.Combine(testFilesFolder, dataSetFileName);
			var dataSet = File.ReadAllText(dataSetFilePath);
			var processor = CreateProcessor();
			processor.GenerateFiles(PublicationDateForTest, TableNameForTest, dataSet);
			var outputFileName = $"IL_{DataSource}.xml";
			var outputFilePath = Path.Combine(OutputFolderPath, $"{ExpectedOutputFileName}.xml");

			if (ExpectedSupportDataSetProcessing)
			{
				Assert.IsTrue(File.Exists(outputFilePath));
				var expectedFileContent = File.ReadAllText(Path.Combine(testFilesFolder, outputFileName));
				var actualFileContent = File.ReadAllText(outputFilePath);
				XmlComparer xmlComparer = new XmlComparer();
				xmlComparer.CompareXml(expectedFileContent, actualFileContent, true);
			}
			else
			{
				Assert.IsFalse(File.Exists(outputFilePath));
			}
		}

		protected virtual string ExpectedOutputFileName => $"IL_{DataSource}";

		protected abstract TProcessor CreateProcessor();
		protected abstract bool ExpectedSupportDataSetProcessing { get; }
		protected abstract bool ExpectedSupportTableDataProcessing { get; }
		protected abstract string OutputFolderPath { get; }
		protected abstract string DataSource { get; }
		protected abstract DateTime PublicationDateForTest { get; }
		protected abstract string TableNameForTest { get; }
	}
}
