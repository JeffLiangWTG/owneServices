using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.RefDbRepo.Common.Infrastructure.Test;
using CargoWise.RefDbRepo.Staging.Schema_New;
using NUnit.Framework;
using Stage = CargoWise.RefDbRepo.Staging.Schema_New;
using Safe = CargoWise.RefDbRepo.Common.SafeDataClient.CargoWise.RefDbRepo.Service.Schema_0_9_New;

namespace CargoWise.RefDbRepo.Staging.UniversalXMLStagingDataProcessor.Test;

class Test_XMLCouldRunIfDependencyIsDUP : IXmlProcessIntegrationTest
{
	public string[] FileNames =>
	[
		"TestFiles\\Test_Dependence_1.xml",
		"TestFiles\\Test_Dependence_2.xml",
		"TestFiles\\Test_XMLCouldExecuteIfDependenceIsDUP.xml"
	];

	public string TestDescription =>
		"Test xml could Run if dependency is DUP";

	public XmlProcessIntegrationTestAssertResultHandler[] AssertResults =>
	[
		AssertResult_AfterProcessingXml1, AssertResult_AfterProcessingXml2,AssertResult_AfterProcessingXml3
	];

	public void PrepareData(IDbCommand stagingCommand, IDbCommand safeCommand)
	{
		Console.WriteLine("Start Preparing Data");

		var safeSql = @"
INSERT INTO RefDataSetInformation (RDS_PK, RDS_DataSetId, RDS_TableName, RDS_DataSetName, RDS_DataSetTableCode, RDS_PriorityLevel)
VALUES (NEWID(), 10, 'RefCusCodeList', 'RefCusCodeList', 'ZZD', 0);

INSERT INTO RefDataGrouping (ZZZ_PK, ZZZ_DataGrouping, ZZZ_Description, ZZZ_ZZZ_NKGrouping)
VALUES (NEWID(), 'DIE', 'Test', NULL);
";
		safeCommand.CommandText = safeSql;
		safeCommand.ExecuteNonQuery();

		Console.WriteLine("Preparing Data Successfully");
	}

	void AssertResult_AfterProcessingXml1(IDbCommand stagingCommand, IDbCommand safeCommand)
	{
		Assert.Multiple(() => AssertResult_StagingDb_SourceData_SDA_Status_AfterProcessingXml1(stagingCommand));
	}

	void AssertResult_AfterProcessingXml2(IDbCommand stagingCommand, IDbCommand safeCommand)
	{
		Assert.Multiple(() => AssertResult_StagingDb_SourceData_SDA_Status_AfterProcessingXml2(stagingCommand));
	}

	void AssertResult_AfterProcessingXml3(IDbCommand stagingCommand, IDbCommand safeCommand)
	{
		Assert.Multiple(() =>
		{
			AssertResult_StagingDb_SourceData_SDA_Status_AfterProcessingXml3(stagingCommand);
			AssertResult_SafeDb_RefCusCodeList_Exists_AfterProcessingXml3(safeCommand);
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

	void AssertResult_StagingDb_SourceData_SDA_Status_AfterProcessingXml2(IDbCommand stagingCommand)
	{
		var sourceDatas = new List<SourceData>();
		stagingCommand.CommandText = "SELECT * FROM dbo.SourceData";
		using (var reader = stagingCommand.ExecuteReader())
		{
			while (reader.Read())
			{
				var sourceData = new SourceData();
				sourceData.SDA_Status = reader[nameof(SourceData.SDA_Status)].ToString();
				sourceDatas.Add(sourceData);
			}
		}

		Assert.That(sourceDatas, Has.Count.EqualTo(2));
		Assert.That(sourceDatas.Select(x => x.SDA_Status), Is.EquivalentTo(new[] { "MER", "DUP" }));
	}

	void AssertResult_StagingDb_SourceData_SDA_Status_AfterProcessingXml3(IDbCommand stagingCommand)
	{
		var sourceDatas = new List<SourceData>();
		stagingCommand.CommandText = "SELECT * FROM dbo.SourceData";
		using (var reader = stagingCommand.ExecuteReader())
		{
			while (reader.Read())
			{
				var sourceData = new SourceData();
				sourceData.SDA_Status = reader[nameof(SourceData.SDA_Status)].ToString();
				sourceDatas.Add(sourceData);
			}
		}

		Assert.That(sourceDatas, Has.Count.EqualTo(3));
		Assert.That(sourceDatas.Select(x => x.SDA_Status), Is.EquivalentTo(new[] { "MER", "DUP", "MER" }));
	}

	void AssertResult_SafeDb_RefCusCodeList_Exists_AfterProcessingXml3(IDbCommand safeCommand)
	{
		safeCommand.CommandText = "SELECT * FROM RefCusCodeList";
		var rows = new List<Safe.RefCusCodeList>();
		using (var reader = safeCommand.ExecuteReader())
		{
			while (reader.Read())
			{
				var row = new Safe.RefCusCodeList();
				row.ZZD_Code = reader[nameof(Safe.RefCusCodeList.ZZD_Code)].ToString();
				rows.Add(row);
			}
		}
		Assert.That(rows, Has.Count.EqualTo(1));
		Assert.That(rows.Select(x => x.ZZD_Code), Is.EqualTo(new[] { "C624" }));
	}
}
