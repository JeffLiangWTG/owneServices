using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;
using CargoWise.RefDbRepo.GBReferenceData.Business.Procedure;
using CargoWise.RefDbRepo.GBReferenceData.Services.Procedure.Models;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.GBReferenceData.Tests.Procedure
{

	[TestFixture]
	sealed class ProcedureBuilderTests
	{
		[Test]
		public void FilePrefix()
		{
			Assert.That(builder.FilePrefix, Is.EqualTo("GB_RefCusProcedure"));
		}

		[Test]
		public void ConvertModelToRefModel()
		{
			var models = builder.ConvertToRefModels(procedureCodeData, categoryProcedureMapping).ToList();
			Assert.That(models, Is.Not.Null);
			Assert.AreEqual(procedureCodeData.Count(), models.Count);
			Assert.Multiple(() =>
			{
				foreach (var procedureData in procedureCodeData)
				{
					var model = models.FirstOrDefault(x => x.ZZ6_Description == procedureData.Description);
					Assert.That(model, Is.Not.Null, procedureData.Description);
					Assert.Multiple(() =>
					{
						Assert.AreEqual(procedureData.Expected_ZZ6_Concession, model.ZZ6_Concession, procedureData.Description + ": ZZ6_Concession");
						Assert.AreEqual(procedureData.Expected_ZZ6_Group, model.ZZ6_Group, procedureData.Description + ": ZZ6_Group");
						Assert.AreEqual(procedureData.Expected_ZZ6_IntoWarehouse, model.ZZ6_IntoWarehouse, procedureData.Description + ": ZZ6_IntoWarehouse");
						Assert.AreEqual(procedureData.Expected_ZZ6_OutOfWarehouse, model.ZZ6_OutOfWarehouse, procedureData.Description + ": ZZ6_OutOfWarehouse");
						Assert.AreEqual(procedureData.Expected_ZZ6_PreviousProcedureCode, model.ZZ6_PreviousProcedureCode, procedureData.Description + ": ZZ6_PreviousProcedureCode");
						Assert.AreEqual(procedureData.Expected_ZZ6_ProcedureCode, model.ZZ6_ProcedureCode, procedureData.Description + ": ZZ6_ProcedureCode");
						Assert.AreEqual(procedureData.Expected_ZZ6_ShipmentType, model.ZZ6_ShipmentType, procedureData.Description + ": ZZ6_ShipmentType");
						Assert.AreEqual(procedureData.Expected_ZZ6_IntoTemporaryImport, model.ZZ6_IntoTemporaryImport, procedureData.Description + ": ZZ6_IntoTemporaryImport");
						Assert.AreEqual(procedureData.Expected_ZZ6_IntoTemporaryExport, model.ZZ6_IntoTemporaryExport, procedureData.Description + ": ZZ6_IntoTemporaryExport");
						Assert.AreEqual(procedureData.Expected_ZZ6_OutOfTemporaryImport, model.ZZ6_OutOfTemporaryImport, procedureData.Description + ": ZZ6_OutOfTemporaryImport");
						Assert.AreEqual(procedureData.Expected_ZZ6_OutOfTemporaryExport, model.ZZ6_OutOfTemporaryExport, procedureData.Description + ": ZZ6_OutOfTemporaryExport");
						Assert.AreEqual(procedureData.Expected_ZZ6_IntoInwardProcessing, model.ZZ6_IntoInwardProcessing, procedureData.Description + ": ZZ6_IntoInwardProcessing");
						Assert.AreEqual(procedureData.Expected_ZZ6_IntoOutwardProcessing, model.ZZ6_IntoOutwardProcessing, procedureData.Description + ": ZZ6_IntoOutwardProcessing");
						Assert.AreEqual(procedureData.Expected_ZZ6_OutOfInwardProcessing, model.ZZ6_OutOfInwardProcessing, procedureData.Description + ": ZZ6_OutOfInwardProcessing");
						Assert.AreEqual(procedureData.Expected_ZZ6_OutOfOutwardProcessing, model.ZZ6_OutofOutwardProcessing, procedureData.Description + ": ZZ6_OutofOutwardProcessing");
					});
				}
			});
		}

		[Test]
		public void XmlWriterConfig()
		{
			var config = builder.XmlWriterConfiguration();

			Assert.That(config, Is.Not.Null);
			var refType = typeof(RefCusProcedure);
			var entityConfig = config.GetConfiguration(refType);

			Assert.That(entityConfig, Is.Not.Null);
			Assert.That(entityConfig.IsIncluded(refType.GetProperty(nameof(RefCusProcedure.ZZ6_ProcedureCode))));
			Assert.That(entityConfig.IsIncluded(refType.GetProperty(nameof(RefCusProcedure.ZZ6_PreviousProcedureCode))));
			Assert.That(entityConfig.IsIncluded(refType.GetProperty(nameof(RefCusProcedure.ZZ6_Concession))));
			Assert.That(entityConfig.IsIncluded(refType.GetProperty(nameof(RefCusProcedure.ZZ6_Description))));
			Assert.That(entityConfig.IsIncluded(refType.GetProperty(nameof(RefCusProcedure.ZZ6_ZZZ_NKDataGrouping))));
			Assert.That(entityConfig.IsIncluded(refType.GetProperty(nameof(RefCusProcedure.ZZ6_ShipmentType))));
			Assert.That(entityConfig.IsIncluded(refType.GetProperty(nameof(RefCusProcedure.ZZ6_Group))));
			Assert.That(entityConfig.IsIncluded(refType.GetProperty(nameof(RefCusProcedure.ZZ6_IntoWarehouse))));
			Assert.That(entityConfig.IsIncluded(refType.GetProperty(nameof(RefCusProcedure.ZZ6_OutOfWarehouse))));
			Assert.That(entityConfig.IsIncluded(refType.GetProperty(nameof(RefCusProcedure.ZZ6_StartDate))));
			Assert.That(entityConfig.IsIncluded(refType.GetProperty(nameof(RefCusProcedure.ZZ6_EndDate))));
			Assert.That(entityConfig.IsIncluded(refType.GetProperty(nameof(RefCusProcedure.ZZ6_IntoTemporaryImport))));
			Assert.That(entityConfig.IsIncluded(refType.GetProperty(nameof(RefCusProcedure.ZZ6_IntoTemporaryExport))));
			Assert.That(entityConfig.IsIncluded(refType.GetProperty(nameof(RefCusProcedure.ZZ6_OutOfTemporaryImport))));
			Assert.That(entityConfig.IsIncluded(refType.GetProperty(nameof(RefCusProcedure.ZZ6_OutOfTemporaryExport))));
			Assert.That(entityConfig.IsIncluded(refType.GetProperty(nameof(RefCusProcedure.ZZ6_IntoInwardProcessing))));
			Assert.That(entityConfig.IsIncluded(refType.GetProperty(nameof(RefCusProcedure.ZZ6_IntoOutwardProcessing))));
			Assert.That(entityConfig.IsIncluded(refType.GetProperty(nameof(RefCusProcedure.ZZ6_OutOfInwardProcessing))));
			Assert.That(entityConfig.IsIncluded(refType.GetProperty(nameof(RefCusProcedure.ZZ6_OutofOutwardProcessing))));

			refType = typeof(RefCusProcedureAttribute);
			entityConfig = config.GetConfiguration(refType);

			Assert.That(entityConfig.IsIncluded(refType.GetProperty(nameof(RefCusProcedureAttribute.ZXB_Value))));
			Assert.That(entityConfig.IsIncluded(refType.GetProperty(nameof(RefCusProcedureAttribute.ZXB_Name))));
		}

		[Test]
		public void BuildXmlFile()
		{
			var errorCollector = new StringBuilder();
			var builder = new ProcedureBuilderTester(errorCollector);
			var publicationDate = new DateTime(2023, 04, 20, 23, 42, 15, 678, DateTimeKind.Utc);
			builder.BuildXml(publicationDate, procedureCodeData, categoryProcedureMapping, TempFolder);

			var fileName = Path.Combine(TempFolder, builder.GetOutputFileName(publicationDate));
			Assert.That(File.Exists(fileName));
			var xml = File.ReadAllText(fileName);
			var expectedXml = TestHelper.ReadManifestResourceContent("CargoWise.RefDbRepo.GBReferenceData.Tests.Procedure.TestFiles.Output.RefCusProcedure.xml");
			Assert.That(xml, Is.EqualTo(expectedXml));
		}

		[OneTimeSetUp]
		public void OneTimeSetup()
		{
			TempFolder = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
			errorCollector = new StringBuilder();
			builder = new ProcedureBuilderTester(errorCollector);
		}

		[OneTimeTearDown]
		public void TearDown()
		{
			if (Directory.Exists(TempFolder))
			{
				Directory.Delete(TempFolder, true);
			}
		}

		[SetUp]
		public void Setup()
		{
			CreateTestProcedureData();
			CreateTestCategoryMappingData();
			errorCollector.Clear();
		}

		void CreateTestCategoryMappingData()
		{
			categoryProcedureMapping = new List<CategoryProcedureMapping>
			{
				new CategoryProcedureMapping{ CategoryCode="BIRDS", ProcedureMapping="0020" },
				new CategoryProcedureMapping{ CategoryCode="C21i", ProcedureMapping="0002 - 0009" },
				new CategoryProcedureMapping{ CategoryCode="C21i", ProcedureMapping="0098 \u2013 0099" },
				new CategoryProcedureMapping{ CategoryCode="C21i EIDR NOP", ProcedureMapping="40, 44, 51, 53, 61, 71" },
				new CategoryProcedureMapping{ CategoryCode="C21e", ProcedureMapping="0012- 0019 " },
				new CategoryProcedureMapping{ CategoryCode="C21e EIDR NOP", ProcedureMapping="10, 23, 31" },
				new CategoryProcedureMapping{ CategoryCode="FSD", ProcedureMapping="0090" },
				new CategoryProcedureMapping{ CategoryCode="C1 C&F", ProcedureMapping="10, 11, 23, 31" },
				new CategoryProcedureMapping{ CategoryCode="C1 B&E", ProcedureMapping="10" },
				new CategoryProcedureMapping{ CategoryCode="I1 C&F", ProcedureMapping="01, 07, 40, 42, 44, 51, 53, 61, 71" },
				new CategoryProcedureMapping{ CategoryCode="I1 B&E", ProcedureMapping="40, 44" },
				new CategoryProcedureMapping{ CategoryCode="H2", ProcedureMapping="71" },
				new CategoryProcedureMapping{ CategoryCode="B1", ProcedureMapping="10, 11, 23, 31" },
				new CategoryProcedureMapping{ CategoryCode="B2", ProcedureMapping="21, 22" },
				new CategoryProcedureMapping{ CategoryCode="B4", ProcedureMapping="10, 23, 31" },
				new CategoryProcedureMapping{ CategoryCode="H1", ProcedureMapping="01, 07, 40, 42, 44, 61" },
				new CategoryProcedureMapping{ CategoryCode="H5", ProcedureMapping="07, 40, 42, 61" },
				new CategoryProcedureMapping{ CategoryCode="H7", ProcedureMapping="40" },
			};
		}

		void CreateTestProcedureData()
		{
			//add an entry to this list, for export with PreviousProcedureCode = "53" and OutOfTemporaryImport = "Y"
			procedureCodeData = new List<ProcedureCodeDataForTest> {
				new ProcedureCodeDataForTest{ Description = "TestProc1", ShipmentType = "IMP", ProcedureCode = "0020 ", AdditionalProcedureCode = "C01",
					Expected_ZZ6_Concession = "C01", Expected_ZZ6_Group = "21B", Expected_ZZ6_ShipmentType = "IMP",
					Expected_ZZ6_IntoWarehouse = "N", Expected_ZZ6_OutOfWarehouse = "N",
					Expected_ZZ6_ProcedureCode = "00", Expected_ZZ6_PreviousProcedureCode = "20",
					Expected_ZZ6_IntoTemporaryImport = "N", Expected_ZZ6_IntoTemporaryExport = "N",
					Expected_ZZ6_OutOfTemporaryImport = "N", Expected_ZZ6_OutOfTemporaryExport = "N",
					Expected_ZZ6_IntoInwardProcessing = "N", Expected_ZZ6_IntoOutwardProcessing = "N",
					Expected_ZZ6_OutOfInwardProcessing = "N", Expected_ZZ6_OutOfOutwardProcessing = "N"},
				new ProcedureCodeDataForTest{ Description = "TestProc2", ShipmentType = "IMP", ProcedureCode = " 0007", AdditionalProcedureCode = "D24",
					Expected_ZZ6_Concession = "D24", Expected_ZZ6_Group = "21I", Expected_ZZ6_ShipmentType = "IMP",
					Expected_ZZ6_IntoWarehouse = "N", Expected_ZZ6_OutOfWarehouse = "N",
					Expected_ZZ6_ProcedureCode = "00", Expected_ZZ6_PreviousProcedureCode = "07",
					Expected_ZZ6_IntoTemporaryImport = "N", Expected_ZZ6_IntoTemporaryExport = "N",
					Expected_ZZ6_OutOfTemporaryImport = "N", Expected_ZZ6_OutOfTemporaryExport = "N",
					Expected_ZZ6_IntoInwardProcessing = "N", Expected_ZZ6_IntoOutwardProcessing = "N",
					Expected_ZZ6_OutOfInwardProcessing = "N", Expected_ZZ6_OutOfOutwardProcessing = "N"},
				new ProcedureCodeDataForTest{ Description = "TestProc3", ShipmentType = "IMP", ProcedureCode = "7171", AdditionalProcedureCode = "1H7",
					Expected_ZZ6_Concession = "1H7", Expected_ZZ6_Group = "21N,H2,I1", Expected_ZZ6_ShipmentType = "IMP",
					Expected_ZZ6_IntoWarehouse = "Y", Expected_ZZ6_OutOfWarehouse = "Y",
					Expected_ZZ6_ProcedureCode = "71", Expected_ZZ6_PreviousProcedureCode = "71",
					Expected_ZZ6_IntoTemporaryImport = "N", Expected_ZZ6_IntoTemporaryExport = "N",
					Expected_ZZ6_OutOfTemporaryImport = "N", Expected_ZZ6_OutOfTemporaryExport = "N",
					Expected_ZZ6_IntoInwardProcessing = "N", Expected_ZZ6_IntoOutwardProcessing = "N",
					Expected_ZZ6_OutOfInwardProcessing = "N", Expected_ZZ6_OutOfOutwardProcessing = "N"},
				new ProcedureCodeDataForTest{ Description = "TestProc4", ShipmentType = "EXP", ProcedureCode = "0015", AdditionalProcedureCode = "2DP",
					Expected_ZZ6_Concession = "2DP", Expected_ZZ6_Group = "21E", Expected_ZZ6_ShipmentType = "EXP",
					Expected_ZZ6_IntoWarehouse = "N", Expected_ZZ6_OutOfWarehouse = "N",
					Expected_ZZ6_ProcedureCode = "00", Expected_ZZ6_PreviousProcedureCode = "15",
					Expected_ZZ6_IntoTemporaryImport = "N", Expected_ZZ6_IntoTemporaryExport = "N",
					Expected_ZZ6_OutOfTemporaryImport = "N", Expected_ZZ6_OutOfTemporaryExport = "N",
					Expected_ZZ6_IntoInwardProcessing = "N", Expected_ZZ6_IntoOutwardProcessing = "N",
					Expected_ZZ6_OutOfInwardProcessing = "N", Expected_ZZ6_OutOfOutwardProcessing = "N"},
				new ProcedureCodeDataForTest{ Description = "TestProc5", ShipmentType = "EXP", ProcedureCode = "7123", AdditionalProcedureCode = "95P",
					Expected_ZZ6_Concession = "95P", Expected_ZZ6_Group = "21N,H2,I1", Expected_ZZ6_ShipmentType = "EXP",
					Expected_ZZ6_IntoWarehouse = "Y", Expected_ZZ6_OutOfWarehouse = "N",
					Expected_ZZ6_ProcedureCode = "71", Expected_ZZ6_PreviousProcedureCode = "23",
					Expected_ZZ6_IntoTemporaryImport = "N", Expected_ZZ6_IntoTemporaryExport = "N",
					Expected_ZZ6_OutOfTemporaryImport = "N", Expected_ZZ6_OutOfTemporaryExport = "Y",
					Expected_ZZ6_IntoInwardProcessing = "N", Expected_ZZ6_IntoOutwardProcessing = "N",
					Expected_ZZ6_OutOfInwardProcessing = "N", Expected_ZZ6_OutOfOutwardProcessing = "N"},
				new ProcedureCodeDataForTest{ Description = "TestProc6", ShipmentType = "EXP", ProcedureCode = "2371", AdditionalProcedureCode = "000",
					Expected_ZZ6_Concession = "000", Expected_ZZ6_Group = "B1,B4,C1,CEN", Expected_ZZ6_ShipmentType = "EXP",
					Expected_ZZ6_IntoWarehouse = "N", Expected_ZZ6_OutOfWarehouse = "Y",
					Expected_ZZ6_ProcedureCode = "23", Expected_ZZ6_PreviousProcedureCode = "71",
					Expected_ZZ6_IntoTemporaryImport = "N", Expected_ZZ6_IntoTemporaryExport = "Y",
					Expected_ZZ6_OutOfTemporaryImport = "N", Expected_ZZ6_OutOfTemporaryExport = "N",
					Expected_ZZ6_IntoInwardProcessing = "N", Expected_ZZ6_IntoOutwardProcessing = "N",
					Expected_ZZ6_OutOfInwardProcessing = "N", Expected_ZZ6_OutOfOutwardProcessing = "N"},
				new ProcedureCodeDataForTest{ Description = "TestProc7", ShipmentType = "EXP", ProcedureCode = "0090", AdditionalProcedureCode = "001",
					Expected_ZZ6_Concession = "001", Expected_ZZ6_Group = "FS", Expected_ZZ6_ShipmentType = "EXP",
					Expected_ZZ6_IntoWarehouse = "N", Expected_ZZ6_OutOfWarehouse = "N",
					Expected_ZZ6_ProcedureCode = "00", Expected_ZZ6_PreviousProcedureCode = "90",
					Expected_ZZ6_IntoTemporaryImport = "N", Expected_ZZ6_IntoTemporaryExport = "N",
					Expected_ZZ6_OutOfTemporaryImport = "N", Expected_ZZ6_OutOfTemporaryExport = "N",
					Expected_ZZ6_IntoInwardProcessing = "N", Expected_ZZ6_IntoOutwardProcessing = "N",
					Expected_ZZ6_OutOfInwardProcessing = "N", Expected_ZZ6_OutOfOutwardProcessing = "N"},
				new ProcedureCodeDataForTest{ Description = "TestProc8", ShipmentType = "EXP", ProcedureCode = "1071", AdditionalProcedureCode = "041",
					Expected_ZZ6_Concession = "041", Expected_ZZ6_Group = "B1,B4,C1,CEN", Expected_ZZ6_ShipmentType = "EXP",
					Expected_ZZ6_IntoWarehouse = "N", Expected_ZZ6_OutOfWarehouse = "Y",
					Expected_ZZ6_ProcedureCode = "10", Expected_ZZ6_PreviousProcedureCode = "71",
					Expected_ZZ6_IntoTemporaryImport = "N", Expected_ZZ6_IntoTemporaryExport = "N",
					Expected_ZZ6_OutOfTemporaryImport = "N", Expected_ZZ6_OutOfTemporaryExport = "N",
					Expected_ZZ6_IntoInwardProcessing = "N", Expected_ZZ6_IntoOutwardProcessing = "N",
					Expected_ZZ6_OutOfInwardProcessing = "N", Expected_ZZ6_OutOfOutwardProcessing = "N"},
				new ProcedureCodeDataForTest{ Description = "TestProc9", ShipmentType = "EXP", ProcedureCode = "4030", AdditionalProcedureCode = "333",
					Expected_ZZ6_Concession = "333", Expected_ZZ6_Group = "21N,H1,H5,H7,I1", Expected_ZZ6_ShipmentType = "EXP",
					Expected_ZZ6_IntoWarehouse = "N", Expected_ZZ6_OutOfWarehouse = "N",
					Expected_ZZ6_ProcedureCode = "40", Expected_ZZ6_PreviousProcedureCode = "30",
					Expected_ZZ6_IntoTemporaryImport = "N", Expected_ZZ6_IntoTemporaryExport = "N",
					Expected_ZZ6_OutOfTemporaryImport = "N", Expected_ZZ6_OutOfTemporaryExport = "N",
					Expected_ZZ6_IntoInwardProcessing = "N", Expected_ZZ6_IntoOutwardProcessing = "N",
					Expected_ZZ6_OutOfInwardProcessing = "N", Expected_ZZ6_OutOfOutwardProcessing = "N"},
				new ProcedureCodeDataForTest{ Description = "TestProc10", ShipmentType = "IMP", ProcedureCode = " 0098", AdditionalProcedureCode = "D24",
					Expected_ZZ6_Concession = "D24", Expected_ZZ6_Group = "21I", Expected_ZZ6_ShipmentType = "IMP",
					Expected_ZZ6_IntoWarehouse = "N", Expected_ZZ6_OutOfWarehouse = "N",
					Expected_ZZ6_ProcedureCode = "00", Expected_ZZ6_PreviousProcedureCode = "98",
					Expected_ZZ6_IntoTemporaryImport = "N", Expected_ZZ6_IntoTemporaryExport = "N",
					Expected_ZZ6_OutOfTemporaryImport = "N", Expected_ZZ6_OutOfTemporaryExport = "N",
					Expected_ZZ6_IntoInwardProcessing = "N", Expected_ZZ6_IntoOutwardProcessing = "N",
					Expected_ZZ6_OutOfInwardProcessing = "N", Expected_ZZ6_OutOfOutwardProcessing = "N"},
				new ProcedureCodeDataForTest{ Description = "TestProc11", ShipmentType = "IMP", ProcedureCode = "0700", AdditionalProcedureCode = "F06",
					Expected_ZZ6_Concession = "F06", Expected_ZZ6_Group = "H1,H5,I1", Expected_ZZ6_ShipmentType = "IMP",
					Expected_ZZ6_IntoWarehouse = "Y", Expected_ZZ6_OutOfWarehouse = "N",
					Expected_ZZ6_ProcedureCode = "07", Expected_ZZ6_PreviousProcedureCode = "00",
					Expected_ZZ6_IntoTemporaryImport = "N", Expected_ZZ6_IntoTemporaryExport = "N",
					Expected_ZZ6_OutOfTemporaryImport = "N", Expected_ZZ6_OutOfTemporaryExport = "N",
					Expected_ZZ6_IntoInwardProcessing = "N", Expected_ZZ6_IntoOutwardProcessing = "N",
					Expected_ZZ6_OutOfInwardProcessing = "N", Expected_ZZ6_OutOfOutwardProcessing = "N"},
				new ProcedureCodeDataForTest{ Description = "TestProc12", ShipmentType = "IMP", ProcedureCode = "0771", AdditionalProcedureCode = "F06",
					Expected_ZZ6_Concession = "F06", Expected_ZZ6_Group = "H1,H5,I1", Expected_ZZ6_ShipmentType = "IMP",
					Expected_ZZ6_IntoWarehouse = "Y", Expected_ZZ6_OutOfWarehouse = "Y",
					Expected_ZZ6_ProcedureCode = "07", Expected_ZZ6_PreviousProcedureCode = "71",
					Expected_ZZ6_IntoTemporaryImport = "N", Expected_ZZ6_IntoTemporaryExport = "N",
					Expected_ZZ6_OutOfTemporaryImport = "N", Expected_ZZ6_OutOfTemporaryExport = "N",
					Expected_ZZ6_IntoInwardProcessing = "N", Expected_ZZ6_IntoOutwardProcessing = "N",
					Expected_ZZ6_OutOfInwardProcessing = "N", Expected_ZZ6_OutOfOutwardProcessing = "N"},
				new ProcedureCodeDataForTest{ Description = "TestProc13", ShipmentType = "IMP", ProcedureCode = "5321", AdditionalProcedureCode = "F06",
					Expected_ZZ6_Concession = "F06", Expected_ZZ6_Group = "21N,I1", Expected_ZZ6_ShipmentType = "IMP",
					Expected_ZZ6_IntoWarehouse = "N", Expected_ZZ6_OutOfWarehouse = "N",
					Expected_ZZ6_ProcedureCode = "53", Expected_ZZ6_PreviousProcedureCode = "21",
					Expected_ZZ6_IntoTemporaryImport = "Y", Expected_ZZ6_IntoTemporaryExport = "N",
					Expected_ZZ6_OutOfTemporaryImport = "N", Expected_ZZ6_OutOfTemporaryExport = "N",
					Expected_ZZ6_IntoInwardProcessing = "N", Expected_ZZ6_IntoOutwardProcessing = "N",
					Expected_ZZ6_OutOfInwardProcessing = "N", Expected_ZZ6_OutOfOutwardProcessing = "Y"},
				new ProcedureCodeDataForTest{ Description = "TestProc14", ShipmentType = "EXP", ProcedureCode = "2100", AdditionalProcedureCode = "F06",
					Expected_ZZ6_Concession = "F06", Expected_ZZ6_Group = "B2", Expected_ZZ6_ShipmentType = "EXP",
					Expected_ZZ6_IntoWarehouse = "N", Expected_ZZ6_OutOfWarehouse = "N",
					Expected_ZZ6_ProcedureCode = "21", Expected_ZZ6_PreviousProcedureCode = "00",
					Expected_ZZ6_IntoTemporaryImport = "N", Expected_ZZ6_IntoTemporaryExport = "N",
					Expected_ZZ6_OutOfTemporaryImport = "N", Expected_ZZ6_OutOfTemporaryExport = "N",
					Expected_ZZ6_IntoInwardProcessing = "N", Expected_ZZ6_IntoOutwardProcessing = "Y",
					Expected_ZZ6_OutOfInwardProcessing = "N", Expected_ZZ6_OutOfOutwardProcessing = "N"},
				new ProcedureCodeDataForTest{ Description = "TestProc15", ShipmentType = "EXP", ProcedureCode = "2200", AdditionalProcedureCode = "F06",
					Expected_ZZ6_Concession = "F06", Expected_ZZ6_Group = "B2", Expected_ZZ6_ShipmentType = "EXP",
					Expected_ZZ6_IntoWarehouse = "N", Expected_ZZ6_OutOfWarehouse = "N",
					Expected_ZZ6_ProcedureCode = "22", Expected_ZZ6_PreviousProcedureCode = "00",
					Expected_ZZ6_IntoTemporaryImport = "N", Expected_ZZ6_IntoTemporaryExport = "N",
					Expected_ZZ6_OutOfTemporaryImport = "N", Expected_ZZ6_OutOfTemporaryExport = "N",
					Expected_ZZ6_IntoInwardProcessing = "N", Expected_ZZ6_IntoOutwardProcessing = "Y",
					Expected_ZZ6_OutOfInwardProcessing = "N", Expected_ZZ6_OutOfOutwardProcessing = "N"},
				new ProcedureCodeDataForTest{ Description = "TestProc16", ShipmentType = "IMP", ProcedureCode = "0053", AdditionalProcedureCode = "F06",
					Expected_ZZ6_Concession = "F06", Expected_ZZ6_Group = "", Expected_ZZ6_ShipmentType = "IMP",
					Expected_ZZ6_IntoWarehouse = "N", Expected_ZZ6_OutOfWarehouse = "N",
					Expected_ZZ6_ProcedureCode = "00", Expected_ZZ6_PreviousProcedureCode = "53",
					Expected_ZZ6_IntoTemporaryImport = "N", Expected_ZZ6_IntoTemporaryExport = "N",
					Expected_ZZ6_OutOfTemporaryImport = "Y", Expected_ZZ6_OutOfTemporaryExport = "N",
					Expected_ZZ6_IntoInwardProcessing = "N", Expected_ZZ6_IntoOutwardProcessing = "N",
					Expected_ZZ6_OutOfInwardProcessing = "N", Expected_ZZ6_OutOfOutwardProcessing = "N"},
				new ProcedureCodeDataForTest{ Description = "TestProc17", ShipmentType = "IMP", ProcedureCode = "6121", AdditionalProcedureCode = "C01",
					Expected_ZZ6_Concession = "C01", Expected_ZZ6_Group = "21N,H1,H5,I1", Expected_ZZ6_ShipmentType = "IMP",
					Expected_ZZ6_IntoWarehouse = "N", Expected_ZZ6_OutOfWarehouse = "N",
					Expected_ZZ6_ProcedureCode = "61", Expected_ZZ6_PreviousProcedureCode = "21",
					Expected_ZZ6_IntoTemporaryImport = "N", Expected_ZZ6_IntoTemporaryExport = "N",
					Expected_ZZ6_OutOfTemporaryImport = "N", Expected_ZZ6_OutOfTemporaryExport = "N",
					Expected_ZZ6_IntoInwardProcessing = "N", Expected_ZZ6_IntoOutwardProcessing = "N",
					Expected_ZZ6_OutOfInwardProcessing = "N", Expected_ZZ6_OutOfOutwardProcessing = "Y"},
				new ProcedureCodeDataForTest{ Description = "TestProc18", ShipmentType = "IMP", ProcedureCode = "6122", AdditionalProcedureCode = "C01",
					Expected_ZZ6_Concession = "C01", Expected_ZZ6_Group = "21N,H1,H5,I1", Expected_ZZ6_ShipmentType = "IMP",
					Expected_ZZ6_IntoWarehouse = "N", Expected_ZZ6_OutOfWarehouse = "N",
					Expected_ZZ6_ProcedureCode = "61", Expected_ZZ6_PreviousProcedureCode = "22",
					Expected_ZZ6_IntoTemporaryImport = "N", Expected_ZZ6_IntoTemporaryExport = "N",
					Expected_ZZ6_OutOfTemporaryImport = "N", Expected_ZZ6_OutOfTemporaryExport = "N",
					Expected_ZZ6_IntoInwardProcessing = "N", Expected_ZZ6_IntoOutwardProcessing = "N",
					Expected_ZZ6_OutOfInwardProcessing = "N", Expected_ZZ6_OutOfOutwardProcessing = "Y"},
				new ProcedureCodeDataForTest{ Description = "TestProc19", ShipmentType = "IMP", ProcedureCode = "6123", AdditionalProcedureCode = "C01",
					Expected_ZZ6_Concession = "C01", Expected_ZZ6_Group = "21N,H1,H5,I1", Expected_ZZ6_ShipmentType = "IMP",
					Expected_ZZ6_IntoWarehouse = "N", Expected_ZZ6_OutOfWarehouse = "N",
					Expected_ZZ6_ProcedureCode = "61", Expected_ZZ6_PreviousProcedureCode = "23",
					Expected_ZZ6_IntoTemporaryImport = "N", Expected_ZZ6_IntoTemporaryExport = "N",
					Expected_ZZ6_OutOfTemporaryImport = "N", Expected_ZZ6_OutOfTemporaryExport = "Y",
					Expected_ZZ6_IntoInwardProcessing = "N", Expected_ZZ6_IntoOutwardProcessing = "N",
					Expected_ZZ6_OutOfInwardProcessing = "N", Expected_ZZ6_OutOfOutwardProcessing = "N"},
				new ProcedureCodeDataForTest{ Description = "TestProc20", ShipmentType = "IMP", ProcedureCode = "6120", AdditionalProcedureCode = "C01",
					Expected_ZZ6_Concession = "C01", Expected_ZZ6_Group = "21N,H1,H5,I1", Expected_ZZ6_ShipmentType = "IMP",
					Expected_ZZ6_IntoWarehouse = "N", Expected_ZZ6_OutOfWarehouse = "N",
					Expected_ZZ6_ProcedureCode = "61", Expected_ZZ6_PreviousProcedureCode = "20",
					Expected_ZZ6_IntoTemporaryImport = "N", Expected_ZZ6_IntoTemporaryExport = "N",
					Expected_ZZ6_OutOfTemporaryImport = "N", Expected_ZZ6_OutOfTemporaryExport = "N",
					Expected_ZZ6_IntoInwardProcessing = "N", Expected_ZZ6_IntoOutwardProcessing = "N",
					Expected_ZZ6_OutOfInwardProcessing = "N", Expected_ZZ6_OutOfOutwardProcessing = "N"},
				new ProcedureCodeDataForTest{ Description = "TestProc21", ShipmentType = "IMP", ProcedureCode = "5121", AdditionalProcedureCode = "C01",
					Expected_ZZ6_Concession = "C01", Expected_ZZ6_Group = "21N,I1", Expected_ZZ6_ShipmentType = "IMP",
					Expected_ZZ6_IntoWarehouse = "N", Expected_ZZ6_OutOfWarehouse = "N",
					Expected_ZZ6_ProcedureCode = "51", Expected_ZZ6_PreviousProcedureCode = "21",
					Expected_ZZ6_IntoTemporaryImport = "N", Expected_ZZ6_IntoTemporaryExport = "N",
					Expected_ZZ6_OutOfTemporaryImport = "N", Expected_ZZ6_OutOfTemporaryExport = "N",
					Expected_ZZ6_IntoInwardProcessing = "Y", Expected_ZZ6_IntoOutwardProcessing = "N",
					Expected_ZZ6_OutOfInwardProcessing = "N", Expected_ZZ6_OutOfOutwardProcessing = "Y"},
				new ProcedureCodeDataForTest{ Description = "TestProc22", ShipmentType = "IMP", ProcedureCode = "2121", AdditionalProcedureCode = "C01",
					Expected_ZZ6_Concession = "C01", Expected_ZZ6_Group = "B2", Expected_ZZ6_ShipmentType = "IMP",
					Expected_ZZ6_IntoWarehouse = "N", Expected_ZZ6_OutOfWarehouse = "N",
					Expected_ZZ6_ProcedureCode = "21", Expected_ZZ6_PreviousProcedureCode = "21",
					Expected_ZZ6_IntoTemporaryImport = "N", Expected_ZZ6_IntoTemporaryExport = "N",
					Expected_ZZ6_OutOfTemporaryImport = "N", Expected_ZZ6_OutOfTemporaryExport = "N",
					Expected_ZZ6_IntoInwardProcessing = "N", Expected_ZZ6_IntoOutwardProcessing = "Y",
					Expected_ZZ6_OutOfInwardProcessing = "N", Expected_ZZ6_OutOfOutwardProcessing = "Y"},
				new ProcedureCodeDataForTest{ Description = "TestProc23", ShipmentType = "IMP", ProcedureCode = "2221", AdditionalProcedureCode = "C01",
					Expected_ZZ6_Concession = "C01", Expected_ZZ6_Group = "B2", Expected_ZZ6_ShipmentType = "IMP",
					Expected_ZZ6_IntoWarehouse = "N", Expected_ZZ6_OutOfWarehouse = "N",
					Expected_ZZ6_ProcedureCode = "22", Expected_ZZ6_PreviousProcedureCode = "21",
					Expected_ZZ6_IntoTemporaryImport = "N", Expected_ZZ6_IntoTemporaryExport = "N",
					Expected_ZZ6_OutOfTemporaryImport = "N", Expected_ZZ6_OutOfTemporaryExport = "N",
					Expected_ZZ6_IntoInwardProcessing = "N", Expected_ZZ6_IntoOutwardProcessing = "Y",
					Expected_ZZ6_OutOfInwardProcessing = "N", Expected_ZZ6_OutOfOutwardProcessing = "Y"},
				new ProcedureCodeDataForTest{ Description = "TestProc24", ShipmentType = "IMP", ProcedureCode = "6151", AdditionalProcedureCode = "C01",
					Expected_ZZ6_Concession = "C01", Expected_ZZ6_Group = "21N,H1,H5,I1", Expected_ZZ6_ShipmentType = "IMP",
					Expected_ZZ6_IntoWarehouse = "N", Expected_ZZ6_OutOfWarehouse = "N",
					Expected_ZZ6_ProcedureCode = "61", Expected_ZZ6_PreviousProcedureCode = "51",
					Expected_ZZ6_IntoTemporaryImport = "N", Expected_ZZ6_IntoTemporaryExport = "N",
					Expected_ZZ6_OutOfTemporaryImport = "N", Expected_ZZ6_OutOfTemporaryExport = "N",
					Expected_ZZ6_IntoInwardProcessing = "N", Expected_ZZ6_IntoOutwardProcessing = "N",
					Expected_ZZ6_OutOfInwardProcessing = "Y", Expected_ZZ6_OutOfOutwardProcessing = "N"},
				new ProcedureCodeDataForTest{ Description = "TestProc25", ShipmentType = "IMP", ProcedureCode = "0021", AdditionalProcedureCode = "C01",
					Expected_ZZ6_Concession = "C01", Expected_ZZ6_Group = "", Expected_ZZ6_ShipmentType = "IMP",
					Expected_ZZ6_IntoWarehouse = "N", Expected_ZZ6_OutOfWarehouse = "N",
					Expected_ZZ6_ProcedureCode = "00", Expected_ZZ6_PreviousProcedureCode = "21",
					Expected_ZZ6_IntoTemporaryImport = "N", Expected_ZZ6_IntoTemporaryExport = "N",
					Expected_ZZ6_OutOfTemporaryImport = "N", Expected_ZZ6_OutOfTemporaryExport = "N",
					Expected_ZZ6_IntoInwardProcessing = "N", Expected_ZZ6_IntoOutwardProcessing = "N",
					Expected_ZZ6_OutOfInwardProcessing = "N", Expected_ZZ6_OutOfOutwardProcessing = "Y"},
				new ProcedureCodeDataForTest{ Description = "TestProc26", ShipmentType = "IMP", ProcedureCode = "0022", AdditionalProcedureCode = "C01",
					Expected_ZZ6_Concession = "C01", Expected_ZZ6_Group = "", Expected_ZZ6_ShipmentType = "IMP",
					Expected_ZZ6_IntoWarehouse = "N", Expected_ZZ6_OutOfWarehouse = "N",
					Expected_ZZ6_ProcedureCode = "00", Expected_ZZ6_PreviousProcedureCode = "22",
					Expected_ZZ6_IntoTemporaryImport = "N", Expected_ZZ6_IntoTemporaryExport = "N",
					Expected_ZZ6_OutOfTemporaryImport = "N", Expected_ZZ6_OutOfTemporaryExport = "N",
					Expected_ZZ6_IntoInwardProcessing = "N", Expected_ZZ6_IntoOutwardProcessing = "N",
					Expected_ZZ6_OutOfInwardProcessing = "N", Expected_ZZ6_OutOfOutwardProcessing = "Y"},
				new ProcedureCodeDataForTest{ Description = "TestProc27", ShipmentType = "EXP", ProcedureCode = "1007", AdditionalProcedureCode = "000",
					Expected_ZZ6_Concession = "000", Expected_ZZ6_Group = "B1,B4,C1,CEN", Expected_ZZ6_ShipmentType = "EXP",
					Expected_ZZ6_IntoWarehouse = "N", Expected_ZZ6_OutOfWarehouse = "Y",
					Expected_ZZ6_ProcedureCode = "10", Expected_ZZ6_PreviousProcedureCode = "07",
					Expected_ZZ6_IntoTemporaryImport = "N", Expected_ZZ6_IntoTemporaryExport = "N",
					Expected_ZZ6_OutOfTemporaryImport = "N", Expected_ZZ6_OutOfTemporaryExport = "N",
					Expected_ZZ6_IntoInwardProcessing = "N", Expected_ZZ6_IntoOutwardProcessing = "N",
					Expected_ZZ6_OutOfInwardProcessing = "N", Expected_ZZ6_OutOfOutwardProcessing = "N"},
				new ProcedureCodeDataForTest{ Description = "TestProc28", ShipmentType = "EXP", ProcedureCode = "3153", AdditionalProcedureCode = "000",
					Expected_ZZ6_Concession = "000", Expected_ZZ6_Group = "B1,B4,C1,CEN", Expected_ZZ6_ShipmentType = "EXP",
					Expected_ZZ6_IntoWarehouse = "N", Expected_ZZ6_OutOfWarehouse = "N",
					Expected_ZZ6_ProcedureCode = "31", Expected_ZZ6_PreviousProcedureCode = "53",
					Expected_ZZ6_IntoTemporaryImport = "N", Expected_ZZ6_IntoTemporaryExport = "N",
					Expected_ZZ6_OutOfTemporaryImport = "Y", Expected_ZZ6_OutOfTemporaryExport = "N",
					Expected_ZZ6_IntoInwardProcessing = "N", Expected_ZZ6_IntoOutwardProcessing = "N",
					Expected_ZZ6_OutOfInwardProcessing = "N", Expected_ZZ6_OutOfOutwardProcessing = "N"},
				new ProcedureCodeDataForTest{ Description = "TestProc29", ShipmentType = "IMP", ProcedureCode = "4078", AdditionalProcedureCode = "C01",
					Expected_ZZ6_Concession = "C01", Expected_ZZ6_Group = "21N,H1,H5,H7,I1", Expected_ZZ6_ShipmentType = "IMP",
					Expected_ZZ6_IntoWarehouse = "N", Expected_ZZ6_OutOfWarehouse = "Y",
					Expected_ZZ6_ProcedureCode = "40", Expected_ZZ6_PreviousProcedureCode = "78",
					Expected_ZZ6_IntoTemporaryImport = "N", Expected_ZZ6_IntoTemporaryExport = "N",
					Expected_ZZ6_OutOfTemporaryImport = "N", Expected_ZZ6_OutOfTemporaryExport = "N",
					Expected_ZZ6_IntoInwardProcessing = "N", Expected_ZZ6_IntoOutwardProcessing = "N",
					Expected_ZZ6_OutOfInwardProcessing = "N", Expected_ZZ6_OutOfOutwardProcessing = "N"},
			};
		}

		ProcedureBuilderTester builder;
		StringBuilder errorCollector;
		string TempFolder;
		IEnumerable<ProcedureCodeDataForTest> procedureCodeData;
		IEnumerable<CategoryProcedureMapping> categoryProcedureMapping;
	}
	internal class ProcedureBuilderTester : ProcedureBuilder
	{
		public ProcedureBuilderTester(StringBuilder errorCollector) : base(errorCollector) { }

		public new string FilePrefix => ProcedureBuilder.FilePrefix;
		public new string XMLWriterDataSource => ProcedureBuilder.XMLWriterDataSource;
		public new IEnumerable<RefCusProcedure> ConvertToRefModels(IEnumerable<ProcedureCodeData> data, IEnumerable<CategoryProcedureMapping> categoryProcedureMapping) => base.ConvertToRefModels(data, categoryProcedureMapping);
		public new XmlWriterConfiguration XmlWriterConfiguration() => ProcedureBuilder.XmlWriterConfiguration();
		public new string GetOutputFileName(DateTime publicationDate) => ProcedureBuilder.GetOutputFileName(publicationDate);
	}

	class ProcedureCodeDataForTest : ProcedureCodeData
	{
		public string Expected_ZZ6_ProcedureCode { get; set; }
		public string Expected_ZZ6_PreviousProcedureCode { get; set; }
		public string Expected_ZZ6_Concession { get; set; }
		public string Expected_ZZ6_ShipmentType { get; set; }
		public string Expected_ZZ6_Group { get; set; }
		public string Expected_ZZ6_IntoWarehouse { get; set; }
		public string Expected_ZZ6_OutOfWarehouse { get; set; }
		public string Expected_ZZ6_IntoTemporaryImport { get; set; }
		public string Expected_ZZ6_IntoTemporaryExport { get; set; }
		public string Expected_ZZ6_OutOfTemporaryImport { get; set; }
		public string Expected_ZZ6_OutOfTemporaryExport { get; set; }
		public string Expected_ZZ6_IntoInwardProcessing { get; set; }
		public string Expected_ZZ6_IntoOutwardProcessing { get; set; }
		public string Expected_ZZ6_OutOfInwardProcessing { get; set; }
		public string Expected_ZZ6_OutOfOutwardProcessing { get; set; }
	}
}
