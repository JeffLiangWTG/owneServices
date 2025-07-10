using System;
using System.Data;
using CargoWise.RefDbRepo.Common.Infrastructure.Test;
using CargoWise.RefDbRepo.Staging.Schema_New;
using NUnit.Framework;
using Safe = CargoWise.RefDbRepo.Common.SafeDataClient.CargoWise.RefDbRepo.Service.Schema_0_9_New;

namespace CargoWise.RefDbRepo.Staging.UniversalXMLStagingDataProcessor.Test;

[TestFixture]
class Test_BulkInsertSaveOrder_TriggerDeleteDuplicatedRecord : IXmlProcessIntegrationTest
{
	public string[] FileNames =>
	[
		@"TestFiles\Test_BulkInsertSaveOrder_InsertAndDelete_1.xml"
	];
	public string TestDescription => "BulkInsert, when try to insert a duplicated one, it should trigger BulkInsertDuplicateKeyExceptionHandler.DeleteDuplicateRecord and retry again.";

	public XmlProcessIntegrationTestAssertResultHandler[] AssertResults => [AssertResult_AfterProcessingXml1];

	public void PrepareData(IDbCommand stagingCommand, IDbCommand safeCommand)
	{
		Console.WriteLine("Start Preparing Data");
		var safeSql = @"
insert into [dbo].[RefDataGrouping] (ZZZ_PK,ZZZ_DataGrouping,ZZZ_Description,ZZZ_ZZZ_NKGrouping)
values (newid(), 'CH', 'CH', null);

insert into [dbo].[RefCusTradeGroup] (ZZA_PK,ZZA_TradeGroup,ZZA_Description,ZZA_StartDate,ZZA_EndDate,ZZA_ZZZ_NKDataGrouping)
values ('1AD46B88-D9F8-45DC-9EF6-6C47F5106B4B', '100144', 'Test', '2072-06-06T00:00:00', '2079-06-06T23:59:00','CH')

insert into [dbo].[RefCusTradeGroupCountry] (ZZB_PK,ZZB_ZZA_TradeGroup,ZZB_RN_NKTradeGroupCountryCode,ZZB_StartDate,ZZB_EndDate,ZZB_Description)
values (newid(),'1AD46B88-D9F8-45DC-9EF6-6C47F5106B4B','MD','2072-06-06T00:00:00','2079-06-06T23:59:00','Test')

update [dbo].[RefDbVersionControl] set RVC_Deleted = 1, RVC_IsPublished = 0 where RVC_ParentPK = '1AD46B88-D9F8-45DC-9EF6-6C47F5106B4B' and RVC_ParentCode = 'ZZA'
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
			AssertResult_SafeDb_RefCusTradeGroupCountry_AfterProcessingXml1(safeCommand);
		});
	}

	void AssertResult_StagingDb_SourceData_SDA_Status_AfterProcessingXml1(IDbCommand stagingCommand)
	{
		var sourceData = new SourceData();
		stagingCommand.CommandText = "SELECT * FROM dbo.SourceData";
		using var reader = stagingCommand.ExecuteReader();
		reader.Read();
		sourceData.SDA_Status = reader[nameof(SourceData.SDA_Status)].ToString();
		Assert.That(sourceData.IsSourceDataMergedStatus());
	}

	void AssertResult_SafeDb_RefCusTradeGroupCountry_AfterProcessingXml1(IDbCommand safeCommand)
	{
		var row = new Safe.RefCusTradeGroupCountry();
		safeCommand.CommandText = "SELECT * FROM dbo.RefCusTradeGroupCountry";
		using var reader = safeCommand.ExecuteReader();
		reader.Read();
		row.ZZB_RN_NKTradeGroupCountryCode = reader[nameof(Safe.RefCusTradeGroupCountry.ZZB_RN_NKTradeGroupCountryCode)].ToString();
		row.ZZB_StartDate = (DateTime)reader[nameof(Safe.RefCusTradeGroupCountry.ZZB_StartDate)];
		row.ZZB_EndDate = (DateTime)reader[nameof(Safe.RefCusTradeGroupCountry.ZZB_EndDate)];
		Assert.That(row.ZZB_RN_NKTradeGroupCountryCode, Is.EqualTo("MD"));
		Assert.That(row.ZZB_StartDate.ToString(), Does.StartWith("2070-06-06"));
		Assert.That(row.ZZB_EndDate.ToString(), Does.StartWith("2079-06-06"));
	}
}
