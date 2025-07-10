using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.RefDbRepo.Common.Infrastructure.Test;
using CargoWise.RefDbRepo.Staging.Schema_New;
using NUnit.Framework;
using Safe = CargoWise.RefDbRepo.Common.SafeDataClient.CargoWise.RefDbRepo.Service.Schema_0_9_New;

namespace CargoWise.RefDbRepo.Staging.UniversalXMLStagingDataProcessor.Test;

class Test_AutoExpiration_BatchInActive : IXmlProcessIntegrationTest
{
	public string[] FileNames =>
	[
		"TestFiles\\Test_AutoExpire_BatchInActive_1.xml",
		"TestFiles\\Test_AutoExpire_BatchInActive_2.xml"
	];

	public string TestDescription =>
		"Test auto-expiration inactive correctly for type contains IsActive column";

	public XmlProcessIntegrationTestAssertResultHandler[] AssertResults =>
	[
		AssertResult_AfterProcessingXml1, AssertResult_AfterProcessingXml2
	];

	public void PrepareData(IDbCommand stagingCommand, IDbCommand safeCommand)
	{
		Console.WriteLine("Start Preparing Data");

		var stagingSql = @"
INSERT INTO DataSourceInformation (DSI_PK,DSI_SubSource,DSI_EnableAutoExpiration)
VALUES (NEWID(), 'Compliance List', 1)
";
		stagingCommand.CommandText = stagingSql;
		stagingCommand.ExecuteNonQuery();

		var safeSql = @"
INSERT INTO RefDataSetInformation (RDS_PK, RDS_DataSetId, RDS_TableName, RDS_DataSetName, RDS_DataSetTableCode, RDS_PriorityLevel)
VALUES (NEWID(), 42, 'RefComplianceList', 'RefComplianceList', 'RCL', 0);
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

			AssertResult_SafeDb_RefComplianceList_AfterProcessingXml1(safeCommand);
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

		Assert.IsTrue(sourceData.IsSourceDataMergedStatus());
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
					DPR_SubSource =
					reader[nameof(DataProcessingResult.DPR_SubSource)].ToString(),
					DPR_ParentTableCode =
					reader[nameof(DataProcessingResult.DPR_ParentTableCode)].ToString(),
					DPR_Status = reader[nameof(DataProcessingResult.DPR_Status)].ToString()
				};
				dataProcessingResults.Add(dataProcessingResult);
			}
		}

		Assert.That(dataProcessingResults, Has.Count.EqualTo(2));
		Assert.That(dataProcessingResults.All(x => x.DPR_Status == "QUE"));
	}

	void AssertResult_SafeDb_RefComplianceList_AfterProcessingXml1(IDbCommand safeCommand)
	{
		var complianceLists = new List<Safe.RefComplianceList>();
		safeCommand.CommandText = "SELECT * FROM dbo.RefComplianceList";
		using (var reader = safeCommand.ExecuteReader())
		{
			while (reader.Read())
			{
				var complianceList = new Safe.RefComplianceList
				{
					RCL_IsActive = (bool)reader[nameof(Safe.RefComplianceList.RCL_IsActive)],
					RCL_ListCode = reader[nameof(Safe.RefComplianceList.RCL_ListCode)].ToString()
				};
				complianceLists.Add(complianceList);
			}
		}

		Assert.That(complianceLists, Has.Count.EqualTo(2));
		Assert.That(complianceLists.First(x => x.RCL_ListCode == "CA-TEST2").RCL_IsActive, Is.True);
		Assert.That(complianceLists.First(x => x.RCL_ListCode == "US-TEST1").RCL_IsActive, Is.True);
	}

	void AssertResult_AfterProcessingXml2(IDbCommand stagingCommand, IDbCommand safeCommand)
	{
		Assert.Multiple(() =>
		{
			AssertResult_StagingDb_SourceData_SDA_Status_AfterProcessingXml2(stagingCommand);
			AssertResult_StagingDb_DataProcessingResult_AfterProcessingXml2(stagingCommand);

			AssertResult_SafeDb_RefComplianceList_AfterProcessingXml2(safeCommand);
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
				var sourceData = new SourceData();
				sourceData.SDA_PK = (Guid)reader[nameof(SourceData.SDA_PK)];
				sourceData.SDA_Status = reader[nameof(SourceData.SDA_Status)].ToString();
				sourceDatas.Add(sourceData);
			}
		}

		Assert.That(sourceDatas, Has.Count.EqualTo(2));
		Assert.That(sourceDatas.All(x => x.IsSourceDataMergedStatus()));
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
					DPR_ParentTableCode =
						reader[nameof(DataProcessingResult.DPR_ParentTableCode)].ToString(),
					DPR_Status = reader[nameof(DataProcessingResult.DPR_Status)].ToString()
				};
				dataProcessingResults.Add(dataProcessingResult);
			}
		}

		Assert.That(dataProcessingResults, Has.Count.EqualTo(2));
		Assert.That(
			dataProcessingResults.Count(x => x.DPR_Status.Equals("PRS", StringComparison.OrdinalIgnoreCase)),
			Is.EqualTo(1));
	}

	void AssertResult_SafeDb_RefComplianceList_AfterProcessingXml2(IDbCommand safeCommand)
	{
		var complianceLists = new List<Safe.RefComplianceList>();
		safeCommand.CommandText = "SELECT * FROM dbo.RefComplianceList";
		using (var reader = safeCommand.ExecuteReader())
		{
			while (reader.Read())
			{
				var complianceList = new Safe.RefComplianceList
				{
					RCL_IsActive = (bool)reader[nameof(Safe.RefComplianceList.RCL_IsActive)],
					RCL_ListCode = reader[nameof(Safe.RefComplianceList.RCL_ListCode)].ToString()
				};
				complianceLists.Add(complianceList);
			}
		}

		Assert.That(complianceLists, Has.Count.EqualTo(2));
		Assert.That(complianceLists.First(x => x.RCL_ListCode == "CA-TEST2").RCL_IsActive, Is.False);
		Assert.That(complianceLists.First(x => x.RCL_ListCode == "US-TEST1").RCL_IsActive, Is.True);
	}
}
