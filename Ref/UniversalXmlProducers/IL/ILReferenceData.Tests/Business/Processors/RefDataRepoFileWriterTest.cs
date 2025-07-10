using System;
using System.Collections.Generic;
using System.IO;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;
using CargoWise.RefDbRepo.ILReferenceData.Business;
using NUnit.Framework;
using XMLTools;

namespace CargoWise.RefDbRepo.ILReferenceData.Tests.Business
{
	[TestFixture]
	public class RefDataRepoFileWriterTest
	{
		[Test]
		public void TestStoreDataCollection()
		{
			var testFilesFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"TestFiles\");
			var dataSource = "TestDataSource";
			var outputFileName = $"{dataSource}.xml";
			var outputFolderPath = Path.GetTempPath();
			var outputFilePath = Path.Combine(outputFolderPath, outputFileName);
			var xmlWriterConfiguration = new XmlWriterConfiguration();

			var codeList = new EntityTypeConfiguration<RefCusCodeList>(true);
			codeList.IncludeColumn(x => x.ZZD_Code, true);
			codeList.IncludeColumn(x => x.ZZD_Description, false);
			codeList.IncludeColumn(x => x.ZZD_StartDate, false);
			codeList.IncludeColumn(x => x.ZZD_EndDate, false);
			codeList.IncludeColumnWithConstantValue(x => x.ZZD_ZZK_NKCodeType, true, dataSource);
			codeList.IncludeColumnWithConstantValue(x => x.ZZD_ZZZ_NKDataGrouping, true, UniversalDataHelper.Constants.Israel);
			xmlWriterConfiguration.IncludeEntityTypeConfiguration(codeList);

			var list = new List<RefCusCodeList>()
			{
				new RefCusCodeList()
				{
					ZZD_Code = "Code1",
					ZZD_Description = "Description1",
					ZZD_StartDate = DateTime.Now,
					ZZD_EndDate = DateTime.Now.AddDays(1),
				},
				new RefCusCodeList()
				{
					ZZD_Code = "Code2",
					ZZD_Description = "Description2",
					ZZD_StartDate = DateTime.Now,
					ZZD_EndDate = DateTime.Now.AddDays(1),
				},
			};

			RefDataRepoFileWriter<RefCusCodeList>.StoreDataCollection(dataSource, DateTime.Now, outputFilePath, xmlWriterConfiguration, list);

			Assert.True(File.Exists(outputFilePath));

			var expectedFileContent = File.ReadAllText(Path.Combine(testFilesFolder, outputFileName));
			var actualFileContent = File.ReadAllText(outputFilePath);
			XmlComparer xmlComparer = new XmlComparer();
			xmlComparer.CompareXml(expectedFileContent, actualFileContent, true);
		}
	}
}
