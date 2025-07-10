using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.RefDbRepo.Common.Infrastructure.Test;
using CargoWise.RefDbRepo.Staging.Schema_New;
using NUnit.Framework;
using Safe = CargoWise.RefDbRepo.Common.SafeDataClient.CargoWise.RefDbRepo.Service.Schema_0_9_New;
using Stage = CargoWise.RefDbRepo.Staging.Schema_New;

namespace CargoWise.RefDbRepo.Staging.UniversalXMLStagingDataProcessor.Test
{
	class Test_AutoExpiration_BatchDelete : IXmlProcessIntegrationTest
	{
		public string[] FileNames =>
		[
			"TestFiles\\Test_AutoExpire_BatchDelete_1.xml",
			"TestFiles\\Test_AutoExpire_BatchDelete_2.xml"
		];

		public string TestDescription =>
			"Test auto-expiration delete correctly for non-expirable type";

		public XmlProcessIntegrationTestAssertResultHandler[] AssertResults =>
		[
			AssertResult_AfterProcessingXml1, AssertResult_AfterProcessingXml2
		];

		public void PrepareData(IDbCommand stagingCommand, IDbCommand safeCommand)
		{
			Console.WriteLine("Start Preparing Data");

			var stagingSql = @"
INSERT INTO DataSourceInformation (DSI_PK,DSI_SubSource,DSI_EnableAutoExpiration)
VALUES (NEWID(), 'DE DC44N', 1)
";
			stagingCommand.CommandText = stagingSql;
			stagingCommand.ExecuteNonQuery();

			var safeSql = @"
INSERT INTO RefDataSetInformation (RDS_PK, RDS_DataSetId, RDS_TableName, RDS_DataSetName, RDS_DataSetTableCode, RDS_PriorityLevel)
VALUES (NEWID(), 10, 'RefCusCodeList', 'RefCusCodeList', 'ZZD', 0);

INSERT INTO RefDataGrouping (ZZZ_PK, ZZZ_DataGrouping, ZZZ_Description, ZZZ_ZZZ_NKGrouping)
VALUES (NEWID(), 'EUN', 'European Union', NULL);

INSERT INTO RefDataGrouping (ZZZ_PK, ZZZ_DataGrouping, ZZZ_Description, ZZZ_ZZZ_NKGrouping)
VALUES (NEWID(), 'DE', 'Germany', 'EUN');

INSERT INTO RefCusCodeType (ZZK_CodeType, ZZK_Description, ZZK_IsReadonly, ZZK_MaxLength, ZZK_ZZZ_NKDataGrouping)
VALUES ('DC44N', 'Document Type (EU Box 44 NCTS)', '0', '0', 'DE')

INSERT INTO RefCusCodeListAttributeName (ZXE_Name, ZXE_Description, ZXE_ZZK_NKCodeType, ZXE_ZZZ_NKDataGrouping, ZXE_IsValueMandatory)
VALUES
('Complement', 'NCTS Supporting Document Complement', 'DC44N', 'DE', 1),
('Level', 'Where the Code can be used, Header, House or Item', 'DC44N', 'DE', 1)
";
			safeCommand.CommandText = safeSql;
			safeCommand.ExecuteNonQuery();

			Console.WriteLine("Preparing Data Successfully");
		}

		void AssertResult_AfterProcessingXml1(IDbCommand stagingCommand, IDbCommand safeCommand)
		{
			Assert.Multiple(() =>
			{
				AssertResult_StagingDb_SourceData_SDA_Status_AfterProcessingXml1(stagingCommand);
				AssertResult_StagingDb_DataProcessingResult_AfterProcessingXml1(stagingCommand);

				AssertResult_SafeDb_RefCusCodeList_AfterProcessingXml1(safeCommand);
				AssertResult_SafeDb_RefCusCodeListAttribute_AfterProcessingXml1(safeCommand);
			});
		}

		void AssertResult_StagingDb_SourceData_SDA_Status_AfterProcessingXml1(IDbCommand stagingCommand)
		{
			var sourceData = new Stage.SourceData();
			stagingCommand.CommandText = "SELECT * FROM dbo.SourceData";
			using (var reader = stagingCommand.ExecuteReader())
			{
				while (reader.Read())
				{
					sourceData.SDA_PK = (Guid)reader[nameof(Stage.SourceData.SDA_PK)];
					sourceData.SDA_Status = reader[nameof(Stage.SourceData.SDA_Status)].ToString();
				}
			}

			Assert.That(sourceData.IsSourceDataMergedStatus());
		}

		void AssertResult_StagingDb_DataProcessingResult_AfterProcessingXml1(IDbCommand stagingCommand)
		{
			var dataProcessingResults = new List<Stage.DataProcessingResult>();
			stagingCommand.CommandText = "SELECT * FROM dbo.DataProcessingResult";
			using (var reader = stagingCommand.ExecuteReader())
			{
				while (reader.Read())
				{
					var dataProcessingResult = new Stage.DataProcessingResult();
					dataProcessingResult.DPR_SubSource =
						reader[nameof(Stage.DataProcessingResult.DPR_SubSource)].ToString();
					dataProcessingResult.DPR_ParentTableCode =
						reader[nameof(Stage.DataProcessingResult.DPR_ParentTableCode)].ToString();
					dataProcessingResult.DPR_Status = reader[nameof(Stage.DataProcessingResult.DPR_Status)].ToString();
					dataProcessingResults.Add(dataProcessingResult);
				}
			}

			Assert.That(dataProcessingResults, Has.Count.EqualTo(3));
			Assert.That(dataProcessingResults.All(x => x.DPR_Status == "QUE"));
			Assert.That(dataProcessingResults.Select(x => x.DPR_ParentTableCode).Distinct(),
				Is.EquivalentTo(new[] { "ZZD", "ZZE" }));
		}

		void AssertResult_SafeDb_RefCusCodeList_AfterProcessingXml1(IDbCommand safeCommand)
		{
			var codeLists = new List<Safe.RefCusCodeList>();
			safeCommand.CommandText = "SELECT * FROM dbo.RefCusCodeList";
			using (var reader = safeCommand.ExecuteReader())
			{
				while (reader.Read())
				{
					var codeList = new Safe.RefCusCodeList();
					codeList.ZZD_EndDate = (DateTime)reader[nameof(Safe.RefCusCodeList.ZZD_EndDate)];
					codeLists.Add(codeList);
				}
			}

			Assert.That(codeLists, Has.Count.EqualTo(1));
			Assert.That(codeLists.Select(x => $"{x.ZZD_EndDate:yyyy-MM-dd HH:mm:ss}").Distinct(),
				Is.EquivalentTo(new[] { "2079-06-06 23:59:00" }));
		}

		void AssertResult_SafeDb_RefCusCodeListAttribute_AfterProcessingXml1(IDbCommand safeCommand)
		{
			var codeListAttributes = new List<Safe.RefCusCodeListAttribute>();
			safeCommand.CommandText = "SELECT * FROM dbo.RefCusCodeListAttribute";
			using (var reader = safeCommand.ExecuteReader())
			{
				while (reader.Read())
				{
					var codeListAttribute = new Safe.RefCusCodeListAttribute();
					if (reader[nameof(Safe.RefCusCodeListAttribute.ZZE_EndDate)] != DBNull.Value)
					{
						codeListAttribute.ZZE_EndDate =
							(DateTime)reader[nameof(Safe.RefCusCodeListAttribute.ZZE_EndDate)];
					}
					codeListAttribute.ZZE_Value = reader[nameof(Safe.RefCusCodeListAttribute.ZZE_Value)].ToString();
					codeListAttributes.Add(codeListAttribute);
				}
			}

			Assert.That(codeListAttributes, Has.Count.EqualTo(2));
			Assert.That(codeListAttributes.Count(x => x.ZZE_EndDate == null), Is.EqualTo(2));
			Assert.That(codeListAttributes.Select(x => x.ZZE_Value).Distinct(),
				Is.EquivalentTo(new[] { "N", "Item" }));
		}

		void AssertResult_AfterProcessingXml2(IDbCommand stagingCommand, IDbCommand safeCommand)
		{
			Assert.Multiple(() =>
			{
				AssertResult_StagingDb_SourceData_SDA_Status_AfterProcessingXml2(stagingCommand);
				AssertResult_StagingDb_DataProcessingResult_AfterProcessingXml2(stagingCommand);

				AssertResult_SafeDb_RefCusCodeList_AfterProcessingXml2(safeCommand);
				AssertResult_SafeDb_RefCusCodeListAttribute_AfterProcessingXml2(safeCommand);
			});
		}

		void AssertResult_StagingDb_SourceData_SDA_Status_AfterProcessingXml2(IDbCommand stagingCommand)
		{
			var sourceDatas = new List<Stage.SourceData>();
			stagingCommand.CommandText = "SELECT * FROM dbo.SourceData";
			using (var reader = stagingCommand.ExecuteReader())
			{
				while (reader.Read())
				{
					var sourceData = new Stage.SourceData();
					sourceData.SDA_PK = (Guid)reader[nameof(Stage.SourceData.SDA_PK)];
					sourceData.SDA_Status = reader[nameof(Stage.SourceData.SDA_Status)].ToString();
					sourceDatas.Add(sourceData);
				}
			}

			Assert.That(sourceDatas, Has.Count.EqualTo(2));
			Assert.That(sourceDatas.All(x => x.IsSourceDataMergedStatus()));
		}

		void AssertResult_StagingDb_DataProcessingResult_AfterProcessingXml2(IDbCommand stagingCommand)
		{
			var dataProcessingResults = new List<Stage.DataProcessingResult>();
			stagingCommand.CommandText = "SELECT * FROM dbo.DataProcessingResult";
			using (var reader = stagingCommand.ExecuteReader())
			{
				while (reader.Read())
				{
					var dataProcessingResult = new Stage.DataProcessingResult();
					dataProcessingResult.DPR_ParentTableCode =
						reader[nameof(Stage.DataProcessingResult.DPR_ParentTableCode)].ToString();
					dataProcessingResult.DPR_Status = reader[nameof(Stage.DataProcessingResult.DPR_Status)].ToString();
					dataProcessingResults.Add(dataProcessingResult);
				}
			}

			Assert.That(dataProcessingResults, Has.Count.EqualTo(3));
			Assert.That(dataProcessingResults.Select(x => $"{x.DPR_ParentTableCode}_{x.DPR_Status}").Distinct(),
				Is.EquivalentTo(new[] { "ZZD_QUE", "ZZE_QUE" }));
			Assert.That(
				dataProcessingResults.Count(x => x.DPR_Status.Equals("PRS", StringComparison.OrdinalIgnoreCase)),
				Is.EqualTo(0));
		}

		void AssertResult_SafeDb_RefCusCodeList_AfterProcessingXml2(IDbCommand safeCommand)
		{
			var codeLists = new List<Safe.RefCusCodeList>();
			safeCommand.CommandText = "SELECT * FROM dbo.RefCusCodeList";
			using (var reader = safeCommand.ExecuteReader())
			{
				while (reader.Read())
				{
					var codeList = new Safe.RefCusCodeList();
					codeList.ZZD_EndDate = (DateTime)reader[nameof(Safe.RefCusCodeList.ZZD_EndDate)];
					codeLists.Add(codeList);
				}
			}

			Assert.That(codeLists, Has.Count.EqualTo(1));
			Assert.That(codeLists.Select(x => $"{x.ZZD_EndDate:yyyy-MM-dd HH:mm:ss}").Distinct(),
				Is.EquivalentTo(new[] { "2079-06-06 23:59:00" }));
		}

		void AssertResult_SafeDb_RefCusCodeListAttribute_AfterProcessingXml2(IDbCommand safeCommand)
		{
			var codeListAttributes = new List<Safe.RefCusCodeListAttribute>();
			safeCommand.CommandText = "SELECT * FROM dbo.RefCusCodeListAttribute";
			using (var reader = safeCommand.ExecuteReader())
			{
				while (reader.Read())
				{
					var codeListAttribute = new Safe.RefCusCodeListAttribute();
					if (reader[nameof(Safe.RefCusCodeListAttribute.ZZE_EndDate)] != DBNull.Value)
					{
						codeListAttribute.ZZE_EndDate =
							(DateTime)reader[nameof(Safe.RefCusCodeListAttribute.ZZE_EndDate)];
					}

					codeListAttribute.ZZE_Value = reader[nameof(Safe.RefCusCodeListAttribute.ZZE_Value)].ToString();
					codeListAttributes.Add(codeListAttribute);
				}
			}

			Assert.That(codeListAttributes, Has.Count.EqualTo(2));
			Assert.That(codeListAttributes.Count(x => x.ZZE_EndDate == null), Is.EqualTo(2));
			Assert.That(codeListAttributes.Select(x => x.ZZE_Value).Distinct(),
				Is.EquivalentTo(new[] { "Y", "Item" }));
		}
	}
}
