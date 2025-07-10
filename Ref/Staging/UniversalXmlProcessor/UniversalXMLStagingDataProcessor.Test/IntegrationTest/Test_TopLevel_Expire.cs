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
	class Test_TopLevel_Expire : IXmlProcessIntegrationTest
	{
		public string[] FileNames =>
		[
			"TestFiles\\Test_TopLevel_Expire_1.xml",
			"TestFiles\\Test_TopLevel_Expire_2.xml"
		];

		public XmlProcessIntegrationTestAssertResultHandler[] AssertResults => [AssertResult_AfterProcessingXml1, AssertResult_AfterProcessingXml2];

		public string TestDescription => "Test SafeDb has one non-future records that conflicts with XML contains one future record on top level expirable dataset";

		public void PrepareData(IDbCommand stagingCommand, IDbCommand safeCommand)
		{
			Console.WriteLine("Start Preparing Data");

			var safeSql = @"
insert into [dbo].[RefDataSetInformation] (RDS_PK,RDS_DataSetId,RDS_TableName,RDS_DataSetName,RDS_DataSetTableCode,RDS_PriorityLevel)
values (NEWID(), 53, 'RefCusQuota', 'RefCusQuota', 'ZXQ', 0);

insert into [dbo].[RefDataGrouping] (ZZZ_PK,ZZZ_DataGrouping,ZZZ_Description,ZZZ_ZZZ_NKGrouping)
values (newid(), 'EUN', 'European Union', null);
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

				AssertResult_SafeDb_RefCusQuota_AfterProcessingXml1(safeCommand);
			});
		}

		void AssertResult_StagingDb_SourceData_SDA_Status_AfterProcessingXml1(IDbCommand stagingCommand)
		{
			var sourceData = new SourceData();
			stagingCommand.CommandText = "SELECT * FROM dbo.SourceData";
			using (var reader = stagingCommand.ExecuteReader())
			{
				while (reader.Read())
				{
					sourceData.SDA_PK = (Guid)reader[nameof(SourceData.SDA_PK)];
					sourceData.SDA_Status = reader[nameof(SourceData.SDA_Status)].ToString();
				}
			}
			Assert.That(sourceData.IsSourceDataMergedStatus());
		}

		void AssertResult_StagingDb_DataProcessingResult_AfterProcessingXml1(IDbCommand stagingCommand)
		{
			var dataProcessingResults = new List<DataProcessingResult>();
			stagingCommand.CommandText = "SELECT * FROM dbo.DataProcessingResult";
			using (var reader = stagingCommand.ExecuteReader())
			{
				while (reader.Read())
				{
					var dataProcessingResult = new DataProcessingResult
					{
						DPR_ParentTableCode = reader[nameof(DataProcessingResult.DPR_ParentTableCode)].ToString(),
						DPR_Status = reader[nameof(DataProcessingResult.DPR_Status)].ToString()
					};
					dataProcessingResults.Add(dataProcessingResult);
				}
			}
			Assert.That(dataProcessingResults, Has.Count.EqualTo(1));
			Assert.That(dataProcessingResults.All(x => x.DPR_Status == "QUE"));
			Assert.That(dataProcessingResults.Select(x => x.DPR_ParentTableCode).Distinct(), Is.EquivalentTo(new[] { "ZXQ" }));
		}

		void AssertResult_SafeDb_RefCusQuota_AfterProcessingXml1(IDbCommand safeCommand)
		{
			var cusQuotas = new List<Safe.RefCusQuota>();
			safeCommand.CommandText = "SELECT * FROM dbo.RefCusQuota";
			using (var reader = safeCommand.ExecuteReader())
			{
				while (reader.Read())
				{
					var cusQuota = new Safe.RefCusQuota
					{
						ZXQ_OrderNumber = reader[nameof(Safe.RefCusQuota.ZXQ_OrderNumber)].ToString(),
						ZXQ_UnitOfMeasure = reader[nameof(Safe.RefCusQuota.ZXQ_UnitOfMeasure)].ToString()
					};
					cusQuotas.Add(cusQuota);
				}
			}
			Assert.That(cusQuotas, Has.Count.EqualTo(1));
			Assert.That(cusQuotas.Select(x => x.ZXQ_OrderNumber).Distinct(), Is.EquivalentTo(new string[] { "098998" }));
		}

		void AssertResult_AfterProcessingXml2(IDbCommand stagingCommand, IDbCommand safeCommand)
		{
			Assert.Multiple(() =>
			{
				AssertResult_StagingDb_SourceData_SDA_Status_AfterProcessingXml2(stagingCommand);
				AssertResult_StagingDb_DataProcessingResult_AfterProcessingXml2(stagingCommand);
				AssertResult_StagingDb_RefCusQuota_AfterProcessingXml2(stagingCommand);
				
				AssertResult_SafeDb_RefCusQuota_AfterProcessingXml2(safeCommand);
			});
		}

		void AssertResult_StagingDb_SourceData_SDA_Status_AfterProcessingXml2(IDbCommand stagingCommand)
		{
			var sourceDatas = new List<SourceData>();
			stagingCommand.CommandText = "SELECT * FROM dbo.SourceData";
			using (var reader = stagingCommand.ExecuteReader())
			{
				while (reader.Read())
				{
					var sourceData = new SourceData
					{
						SDA_PK = (Guid)reader[nameof(SourceData.SDA_PK)],
						SDA_Status = reader[nameof(SourceData.SDA_Status)].ToString()
					};
					sourceDatas.Add(sourceData);
				}
			}
			Assert.That(sourceDatas, Has.Count.EqualTo(2));
			Assert.That(sourceDatas.Select(x => x.SDA_Status).Distinct(), Is.EquivalentTo(new[] { StatusProvider.GetMERStatus() }));
		}

		void AssertResult_StagingDb_DataProcessingResult_AfterProcessingXml2(IDbCommand stagingCommand)
		{
			var dataProcessingResults = new List<DataProcessingResult>();
			stagingCommand.CommandText = "SELECT * FROM dbo.DataProcessingResult";
			using (var reader = stagingCommand.ExecuteReader())
			{
				while (reader.Read())
				{
					var dataProcessingResult = new DataProcessingResult
					{
						DPR_ParentTableCode = reader[nameof(DataProcessingResult.DPR_ParentTableCode)].ToString(),
						DPR_Status = reader[nameof(DataProcessingResult.DPR_Status)].ToString()
					};
					dataProcessingResults.Add(dataProcessingResult);
				}
			}
			Assert.That(dataProcessingResults, Has.Count.EqualTo(2));
			Assert.That(dataProcessingResults.Select(x => x.DPR_ParentTableCode).Distinct(), Is.EquivalentTo(new[] { "ZXQ" }));
		}

		void AssertResult_StagingDb_RefCusQuota_AfterProcessingXml2(IDbCommand safeCommand)
		{
			var cusQuotas = new List<Safe.RefCusQuota>();
			safeCommand.CommandText = "SELECT * FROM dbo.RefCusQuota";
			using (var reader = safeCommand.ExecuteReader())
			{
				while (reader.Read())
				{
					var cusQuota = new Safe.RefCusQuota
					{
						ZXQ_OrderNumber = reader[nameof(Safe.RefCusQuota.ZXQ_OrderNumber)].ToString(),
						ZXQ_UnitOfMeasure = reader[nameof(Safe.RefCusQuota.ZXQ_UnitOfMeasure)].ToString(),
						ZXQ_StartDate = (DateTime)reader[nameof(RefCusQuota.ZXQ_StartDate)],
						ZXQ_EndDate = (DateTime)reader[nameof(RefCusQuota.ZXQ_EndDate)]
					};
					cusQuotas.Add(cusQuota);
				}
			}
			Assert.That(cusQuotas, Has.Count.EqualTo(2));
			Assert.That(cusQuotas.Select(x => x.ZXQ_OrderNumber).Distinct(), Is.EquivalentTo(new string[] { "098998" }));
			Assert.That(cusQuotas.Select(x => $"{x.ZXQ_StartDate:yyyy-MM-dd HH:mm:ss}").Distinct(), Is.EquivalentTo(new[]
			{
				"2022-10-01 00:00:00",
				"2042-11-01 00:00:00"
			}));
			Assert.That(cusQuotas.Select(x => $"{x.ZXQ_EndDate:yyyy-MM-dd HH:mm:ss}").Distinct(), Is.EquivalentTo(new[]
			{
				"2042-12-31 23:59:59",
				"2045-09-30 23:59:59"
			}));
		}

		void AssertResult_SafeDb_RefCusQuota_AfterProcessingXml2(IDbCommand safeCommand)
		{
			var cusQuotas = new List<Safe.RefCusQuota>();
			safeCommand.CommandText = "SELECT * FROM dbo.RefCusQuota";
			using (var reader = safeCommand.ExecuteReader())
			{
				while (reader.Read())
				{
					var cusQuota = new Safe.RefCusQuota
					{
						ZXQ_OrderNumber = reader[nameof(Safe.RefCusQuota.ZXQ_OrderNumber)].ToString(),
						ZXQ_UnitOfMeasure = reader[nameof(Safe.RefCusQuota.ZXQ_UnitOfMeasure)].ToString(),
						ZXQ_StartDate = (DateTime)reader[nameof(RefCusQuota.ZXQ_StartDate)],
						ZXQ_EndDate = (DateTime)reader[nameof(RefCusQuota.ZXQ_EndDate)]
					};
					cusQuotas.Add(cusQuota);
				}
			}
			Assert.That(cusQuotas, Has.Count.EqualTo(2));
			Assert.That(cusQuotas.Select(x => x.ZXQ_OrderNumber).Distinct(), Is.EquivalentTo(new string[] { "098998" }));
			Assert.That(cusQuotas.Select(x => $"{x.ZXQ_StartDate:yyyy-MM-dd HH:mm:ss}").Distinct(), Is.EquivalentTo(new[]
			{
				"2022-10-01 00:00:00",
				"2042-11-01 00:00:00"
			}));
			Assert.That(cusQuotas.Select(x => $"{x.ZXQ_EndDate:yyyy-MM-dd HH:mm:ss}").Distinct(), Is.EquivalentTo(new[]
			{
				"2042-10-31 23:59:00",
				"2045-09-30 23:59:59"
			}));
		}
	}
}
