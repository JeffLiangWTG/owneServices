using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.RefDbRepo.Common.Infrastructure.Test;
using CargoWise.RefDbRepo.Staging.Schema_New;
using NUnit.Framework;
using Safe = CargoWise.RefDbRepo.Common.SafeDataClient.CargoWise.RefDbRepo.Service.Schema_0_9_New;

namespace CargoWise.RefDbRepo.Staging.UniversalXMLStagingDataProcessor.Test;

[TestFixture]
class Test_BulkInsertSaveOrder_InsertAndDelete : IXmlProcessIntegrationTest
{
	public string[] FileNames =>
	[
		@"TestFiles\Test_BulkInsertSaveOrder_InsertAndDelete_1.xml", @"TestFiles\Test_BulkInsertSaveOrder_InsertAndDelete_2.xml"
	];
	public string TestDescription => "BulkInsert, Insert and Delete duplicated one, the expected save order is Modify -> Delete -> Insert.";

	public XmlProcessIntegrationTestAssertResultHandler[] AssertResults => [AssertResult_AfterProcessingXml1, AssertResult_AfterProcessingXml2];

	public void PrepareData(IDbCommand stagingCommand, IDbCommand safeCommand)
	{
		Console.WriteLine("Start Preparing Data");
		var safeSql = @"
insert into [dbo].[RefDataGrouping] (ZZZ_PK,ZZZ_DataGrouping,ZZZ_Description,ZZZ_ZZZ_NKGrouping)
values (newid(), 'CH', 'CH', null);
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

	void AssertResult_AfterProcessingXml2(IDbCommand stagingCommand, IDbCommand safeCommand)
	{
		Assert.Multiple(() =>
		{
			AssertResult_StagingDb_SourceData_SDA_Status_AfterProcessingXml2(stagingCommand);
			AssertResult_SafeDb_RefCusTradeGroupCountry_AfterProcessingXml2(safeCommand);
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

	void AssertResult_StagingDb_SourceData_SDA_Status_AfterProcessingXml2(IDbCommand stagingCommand)
	{
		var sourceDatas = new List<SourceData>();
		stagingCommand.CommandText = "SELECT * FROM dbo.SourceData";
		using var reader = stagingCommand.ExecuteReader();
		while (reader.Read())
		{
			var sourceData = new SourceData();
			sourceData.SDA_Status = reader[nameof(SourceData.SDA_Status)].ToString();
			sourceDatas.Add(sourceData);
		}
		Assert.That(sourceDatas.All(x => x.IsSourceDataMergedStatus()));
	}

	void AssertResult_SafeDb_RefCusTradeGroupCountry_AfterProcessingXml2(IDbCommand safeCommand)
	{
		var row = new Safe.RefCusTradeGroupCountry();
		safeCommand.CommandText = "SELECT * FROM dbo.RefCusTradeGroupCountry";
		using var reader = safeCommand.ExecuteReader();
		reader.Read();
		row.ZZB_RN_NKTradeGroupCountryCode = reader[nameof(Safe.RefCusTradeGroupCountry.ZZB_RN_NKTradeGroupCountryCode)].ToString();
		row.ZZB_StartDate = (DateTime)reader[nameof(Safe.RefCusTradeGroupCountry.ZZB_StartDate)];
		row.ZZB_EndDate = (DateTime)reader[nameof(Safe.RefCusTradeGroupCountry.ZZB_EndDate)];
		Assert.That(row.ZZB_RN_NKTradeGroupCountryCode, Is.EqualTo("MD"));
		Assert.That(row.ZZB_StartDate.ToString(), Does.StartWith("2025-04-01"));
		Assert.That(row.ZZB_EndDate.ToString(), Does.StartWith("2079-06-06"));
	}
}
