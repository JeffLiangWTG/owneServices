using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.RefDbRepo.Common.Infrastructure.Test;
using CargoWise.RefDbRepo.Staging.Schema_New;
using NUnit.Framework;
using Safe = CargoWise.RefDbRepo.Common.SafeDataClient.CargoWise.RefDbRepo.Service.Schema_0_9_New;

namespace CargoWise.RefDbRepo.Staging.UniversalXMLStagingDataProcessor.Test
{
	class XmlProcessIntegrationTestSample : IXmlProcessIntegrationTest
	{
		public void PrepareData(IDbCommand stagingCommand, IDbCommand safeCommand)
		{
			Console.WriteLine("Start Preparing Data");

			var safeSql = @"
insert into [dbo].[RefDataSetInformation] (RDS_PK,RDS_DataSetId,RDS_TableName,RDS_DataSetName,RDS_DataSetTableCode,RDS_PriorityLevel)
values (NEWID(), 23, 'RefCusTariff', 'RefCusTariff','ZZ1',0);

insert into [dbo].[RefDataGrouping] (ZZZ_PK,ZZZ_DataGrouping,ZZZ_Description,ZZZ_ZZZ_NKGrouping)
values (newid(), 'ZA', 'South Africa', null);

insert into [dbo].[RefCusRateType] (ZZR_PK,ZZR_RateType,ZZR_Description,ZZR_IsPayable,ZZR_ZZZ_NKDataGrouping,ZZR_RX_NKFormulaCurrency,ZZR_CustomsValueFormula,ZZR_IsExport)
values ('D2E07963-932F-4BB4-A2E1-9C90F8848C23','DTY', 'Duty',1, 'ZA', '', 'CV',0);

insert into [dbo].[RefCusTariffType] (ZZI_PK,ZZI_TariffType,ZZI_Description,ZZI_ZZ9_NKNomenclatureGroupType,ZZI_ZZZ_NKDataGrouping,ZZI_ZZR_RateType,ZZI_HasFormulaSpecificQuestions)
values ('D237A1A0-BAE4-4848-8F64-323B617BD6E4', '1P1', 'Schedule 1 Part 1', 'ZA', 'ZA', 'D2E07963-932F-4BB4-A2E1-9C90F8848C23', 0);
";
			safeCommand.CommandText = safeSql;
			safeCommand.ExecuteNonQuery();

			Console.WriteLine("Preparing Data Successfully");
		}

		public void AssertResult_AfterProcessingSampleXML1(IDbCommand stagingCommand, IDbCommand safeCommand)
		{
			AssertStagingDbResult_AfterProcessingSampleXML1(stagingCommand);
			AssertSafeDbResult_AfterProcessingSampleXML1(safeCommand);
		}

		public void AssertResult_AfterProcessingSampleXML2(IDbCommand stagingCommand, IDbCommand safeCommand)
		{
			AssertStagingDbResult_AfterProcessingSampleXML2(stagingCommand);
			AssertSafeDbResult_AfterProcessingSampleXML2(safeCommand);
		}

		public string[] FileNames => ["TestFiles\\Sample1.xml", "TestFiles\\Sample2.xml"];

		public XmlProcessIntegrationTestAssertResultHandler[] AssertResults => [AssertResult_AfterProcessingSampleXML1, AssertResult_AfterProcessingSampleXML2];

		public string TestDescription => "Sample Test for integration process";

		void AssertStagingDbResult_AfterProcessingSampleXML1(IDbCommand stagingCommand)
		{
			AssertStagingDb_SourceData_AfterProcessingSampleXML1(stagingCommand);
			AssertStagingDb_RefCusTariff_AfterProcessingSampleXML(stagingCommand);
			AssertStagingDb_RefCusTariffAttribute_AfterProcessingSampleXML1(stagingCommand);
		}

		void AssertStagingDbResult_AfterProcessingSampleXML2(IDbCommand stagingCommand)
		{
			AssertStagingDb_SourceData_AfterProcessingSampleXML2(stagingCommand);
			AssertStagingDb_RefCusTariff_AfterProcessingSampleXML(stagingCommand);
			AssertStagingDb_RefCusTariffAttribute_AfterProcessingSampleXML2(stagingCommand);
			AssertStagingDb_DataProcessingInformation_AfterProcessingSampleXML(stagingCommand);
		}

		void AssertSafeDbResult_AfterProcessingSampleXML1(IDbCommand safeCommand)
		{
			var refCusTariffAttributes = new List<Safe.RefCusTariffAttribute>();
			safeCommand.CommandText = "select * from RefCusTariffAttribute";
			using var reader = safeCommand.ExecuteReader();
			while (reader.Read())
			{
				var refCusTariffAttribute = new Safe.RefCusTariffAttribute
				{
					ZZ3_PK = (Guid)reader[nameof(Safe.RefCusTariffAttribute.ZZ3_PK)],
					ZZ3_Name = reader[nameof(Safe.RefCusTariffAttribute.ZZ3_Name)].ToString(),
					ZZ3_Value = reader[nameof(Safe.RefCusTariffAttribute.ZZ3_Value)].ToString()
				};
				refCusTariffAttributes.Add(refCusTariffAttribute);
			}
			Assert.That(refCusTariffAttributes.Count == 1);
			CollectionAssert.AreEquivalent(new List<string> { "CheckDigit" }, refCusTariffAttributes.Select(attribute => attribute.ZZ3_Name));
			CollectionAssert.AreEquivalent(new List<string> { "3" }, refCusTariffAttributes.Select(attribute => attribute.ZZ3_Value));
		}

		void AssertSafeDbResult_AfterProcessingSampleXML2(IDbCommand safeCommand)
		{
			var refCusTariffAttributes = new List<Safe.RefCusTariffAttribute>();
			safeCommand.CommandText = "select * from RefCusTariffAttribute";
			using var reader = safeCommand.ExecuteReader();
			while (reader.Read())
			{
				var refCusTariffAttribute = new Safe.RefCusTariffAttribute
				{
					ZZ3_PK = (Guid)reader[nameof(Safe.RefCusTariffAttribute.ZZ3_PK)],
					ZZ3_Name = reader[nameof(Safe.RefCusTariffAttribute.ZZ3_Name)].ToString(),
					ZZ3_Value = reader[nameof(Safe.RefCusTariffAttribute.ZZ3_Value)].ToString()
				};
				refCusTariffAttributes.Add(refCusTariffAttribute);
			}
			Assert.That(refCusTariffAttributes.Count == 2);
			CollectionAssert.AreEquivalent(new List<string> { "CheckDigit", "CheckDigit" }, refCusTariffAttributes.Select(attribute => attribute.ZZ3_Name));
			CollectionAssert.AreEquivalent(new List<string> { "3", "5" }, refCusTariffAttributes.Select(attribute => attribute.ZZ3_Value));
		}

		void AssertStagingDb_SourceData_AfterProcessingSampleXML1(IDbCommand stagingCommand)
		{
			var sourceData = new SourceData();
			stagingCommand.CommandText = "SELECT * FROM dbo.SourceData";
			using var reader = stagingCommand.ExecuteReader();
			while (reader.Read())
			{
				sourceData.SDA_PK = (Guid)reader[nameof(SourceData.SDA_PK)];
				sourceData.SDA_Source = reader[nameof(SourceData.SDA_Source)].ToString();
				sourceData.SDA_Filename = reader[nameof(SourceData.SDA_Filename)].ToString();
				sourceData.SDA_Filetype = reader[nameof(SourceData.SDA_Filetype)].ToString();
				sourceData.SDA_Status = reader[nameof(SourceData.SDA_Status)].ToString();
				sourceData.SDA_ContentType = reader[nameof(SourceData.SDA_ContentType)].ToString();
				if (!Convert.IsDBNull(reader[nameof(SourceData.SDA_SourceTime)]))
				{
					sourceData.SDA_SourceTime = (DateTime)reader[nameof(SourceData.SDA_SourceTime)];
				}
				sourceData.SDA_SubSource = reader[nameof(SourceData.SDA_SubSource)].ToString();
			}
			Assert.AreEqual(DataSourceConstants.Source.InternalWebsite, sourceData.SDA_Source);
			var currentAssemblyLocation = AppDomain.CurrentDomain.BaseDirectory;
			Assert.AreEqual(currentAssemblyLocation + FileNames[0], sourceData.SDA_Filename);
			Assert.AreEqual(DataSourceConstants.FileType.XML.ToString(), sourceData.SDA_Filetype);
			Assert.That(sourceData.IsSourceDataMergedStatus());
			Assert.AreEqual(DataSourceConstants.ContentType.UniversalXML, sourceData.SDA_ContentType);
			Assert.AreEqual(new DateTime(2024, 2, 16, 0, 6, 1, DateTimeKind.Utc), sourceData.SDA_SourceTime);
			Assert.AreEqual("SampleTariffs", sourceData.SDA_SubSource);
		}

		void AssertStagingDb_SourceData_AfterProcessingSampleXML2(IDbCommand stagingCommand)
		{
			var sourceDatas = new List<SourceData>();
			stagingCommand.CommandText = "SELECT * FROM dbo.SourceData";
			using var reader = stagingCommand.ExecuteReader();
			while (reader.Read())
			{
				var sourceData = new SourceData
				{
					SDA_PK = (Guid)reader[nameof(SourceData.SDA_PK)],
					SDA_Source = reader[nameof(SourceData.SDA_Source)].ToString(),
					SDA_Filename = reader[nameof(SourceData.SDA_Filename)].ToString(),
					SDA_Filetype = reader[nameof(SourceData.SDA_Filetype)].ToString(),
					SDA_Status = reader[nameof(SourceData.SDA_Status)].ToString(),
					SDA_ContentType = reader[nameof(SourceData.SDA_ContentType)].ToString(),
					SDA_SourceTime = !Convert.IsDBNull(reader[nameof(SourceData.SDA_SourceTime)]) ? (DateTime)reader[nameof(SourceData.SDA_SourceTime)] : null,
					SDA_SubSource = reader[nameof(SourceData.SDA_SubSource)].ToString()
				};
				sourceDatas.Add(sourceData);
			}
			Assert.That(sourceDatas.Count == 2);
			Assert.That(sourceDatas.All(sourceData => sourceData.SDA_Source == DataSourceConstants.Source.InternalWebsite));
			var currentAssemblyLocation = AppDomain.CurrentDomain.BaseDirectory;
			CollectionAssert.AreEquivalent(new List<string> { currentAssemblyLocation + FileNames[0], currentAssemblyLocation + FileNames[1] }, sourceDatas.Select(sourceData => sourceData.SDA_Filename));
			Assert.That(sourceDatas.All(sourceData => sourceData.SDA_Filetype == DataSourceConstants.FileType.XML.ToString()));
			Assert.That(sourceDatas.All(sourceData => sourceData.IsSourceDataMergedStatus()));
			Assert.That(sourceDatas.All(sourceData => sourceData.SDA_ContentType == DataSourceConstants.ContentType.UniversalXML));
			Assert.That(sourceDatas.All(sourceData => sourceData.SDA_SubSource == "SampleTariffs"));
		}

		void AssertStagingDb_RefCusTariffAttribute_AfterProcessingSampleXML1(IDbCommand stagingCommand)
		{
			var refCusTariffAttributeList = new List<RefCusTariffAttribute>();
			stagingCommand.CommandText = "SELECT * FROM dbo.RefCusTariffAttribute";
			using var reader = stagingCommand.ExecuteReader();
			while (reader.Read())
			{
				var refCusTariffAttribute = new RefCusTariffAttribute
				{
					ZZ3_PK = (Guid)reader[nameof(RefCusTariffAttribute.ZZ3_PK)],
					ZZ3_ZZ1_Tariff = (Guid?)reader[nameof(RefCusTariffAttribute.ZZ3_ZZ1_Tariff)],
					ZZ3_Name = reader[nameof(RefCusTariffAttribute.ZZ3_Name)].ToString(),
					ZZ3_Value = reader[nameof(RefCusTariffAttribute.ZZ3_Value)].ToString()
				};
				refCusTariffAttributeList.Add(refCusTariffAttribute);
			}
			CollectionAssert.AreEquivalent(new List<string> { "CheckDigit" }, refCusTariffAttributeList.Select(attribute => attribute.ZZ3_Name));
			CollectionAssert.AreEquivalent(new List<string> { "3" }, refCusTariffAttributeList.Select(attribute => attribute.ZZ3_Value));
		}

		void AssertStagingDb_RefCusTariffAttribute_AfterProcessingSampleXML2(IDbCommand stagingCommand)
		{
			var refCusTariffAttributeList = new List<RefCusTariffAttribute>();
			stagingCommand.CommandText = "SELECT * FROM dbo.RefCusTariffAttribute";
			using var reader = stagingCommand.ExecuteReader();
			while (reader.Read())
			{
				var refCusTariffAttribute = new RefCusTariffAttribute
				{
					ZZ3_PK = (Guid)reader[nameof(RefCusTariffAttribute.ZZ3_PK)],
					ZZ3_ZZ1_Tariff = (Guid?)reader[nameof(RefCusTariffAttribute.ZZ3_ZZ1_Tariff)],
					ZZ3_Name = reader[nameof(RefCusTariffAttribute.ZZ3_Name)].ToString(),
					ZZ3_Value = reader[nameof(RefCusTariffAttribute.ZZ3_Value)].ToString()
				};
				refCusTariffAttributeList.Add(refCusTariffAttribute);
			}
			CollectionAssert.AreEquivalent(new List<string> { "CheckDigit", "CheckDigit", "CheckDigit" }, refCusTariffAttributeList.Select(attribute => attribute.ZZ3_Name));
			CollectionAssert.AreEquivalent(new List<string> { "3", "3", "5" }, refCusTariffAttributeList.Select(attribute => attribute.ZZ3_Value));
		}

		void AssertStagingDb_RefCusTariff_AfterProcessingSampleXML(IDbCommand stagingCommand)
		{
			var refCusTariff = new RefCusTariff();
			stagingCommand.CommandText = "SELECT * FROM dbo.RefCusTariff";
			using var reader = stagingCommand.ExecuteReader();
			while (reader.Read())
			{
				refCusTariff.ZZ1_PK = (Guid)reader[nameof(RefCusTariff.ZZ1_PK)];
				refCusTariff.ZZ1_ZZI_NKTariffType = reader[nameof(RefCusTariff.ZZ1_ZZI_NKTariffType)].ToString();
				refCusTariff.ZZ1_TariffCode = reader[nameof(RefCusTariff.ZZ1_TariffCode)].ToString();
				refCusTariff.ZZ1_IAMUnique = (short)reader[nameof(RefCusTariff.ZZ1_IAMUnique)];
				refCusTariff.ZZ1_Description = reader[nameof(RefCusTariff.ZZ1_Description)].ToString();
				refCusTariff.ZZ1_StartDate = (DateTime)reader[nameof(RefCusTariff.ZZ1_StartDate)];
				refCusTariff.ZZ1_EndDate = (DateTime)reader[nameof(RefCusTariff.ZZ1_EndDate)];
				refCusTariff.ZZ1_ZZF_NKTaxOrFeeCode = reader[nameof(RefCusTariff.ZZ1_ZZF_NKTaxOrFeeCode)].ToString();
				refCusTariff.ZZ1_ZZZ_NKDataGrouping = reader[nameof(RefCusTariff.ZZ1_ZZZ_NKDataGrouping)].ToString();
				refCusTariff.ZZ1_ZZI_ZZZ_NKDataGrouping = reader[nameof(RefCusTariff.ZZ1_ZZI_ZZZ_NKDataGrouping)].ToString();
			}
			Assert.AreEqual("1P1", refCusTariff.ZZ1_ZZI_NKTariffType);
			Assert.AreEqual("030542", refCusTariff.ZZ1_TariffCode);
			Assert.AreEqual(0, refCusTariff.ZZ1_IAMUnique);
			Assert.AreEqual("HERRINGS (CLUPEA HARENGUS, CLUPEA PALLASII)", refCusTariff.ZZ1_Description);
			Assert.AreEqual(new DateTime(2016, 5, 6), refCusTariff.ZZ1_StartDate);
			Assert.AreEqual(new DateTime(2079, 6, 6, 23, 59, 0, DateTimeKind.Utc), refCusTariff.ZZ1_EndDate);
			Assert.AreEqual("VAT", refCusTariff.ZZ1_ZZF_NKTaxOrFeeCode);
			Assert.AreEqual("ZA", refCusTariff.ZZ1_ZZZ_NKDataGrouping);
			Assert.AreEqual("ZA", refCusTariff.ZZ1_ZZI_ZZZ_NKDataGrouping);
		}

		void AssertStagingDb_DataProcessingInformation_AfterProcessingSampleXML(IDbCommand stagingCommand)
		{
			var dataProcessingInformation = new DataProcessingInformation();
			stagingCommand.CommandText = "SELECT * FROM dbo.DataProcessingInformation";
			using var reader = stagingCommand.ExecuteReader();
			while (reader.Read())
			{
				dataProcessingInformation.DPI_Status = reader[nameof(DataProcessingInformation.DPI_Status)].ToString();
				dataProcessingInformation.DPI_ParentTableCode = reader[nameof(DataProcessingInformation.DPI_ParentTableCode)].ToString();
				dataProcessingInformation.DPI_SourceId = (Guid)reader[nameof(DataProcessingInformation.DPI_SourceId)];
				if (!Convert.IsDBNull(reader[nameof(DataProcessingInformation.DPI_ParentPk)]))
				{
					dataProcessingInformation.DPI_ParentPk = (Guid)reader[nameof(DataProcessingInformation.DPI_ParentPk)];
				}
			}
			Assert.That(dataProcessingInformation.IsDataProcessingInformationProcessedStatus());
			Assert.AreEqual("ZZ1", dataProcessingInformation.DPI_ParentTableCode);
		}
	}
}
