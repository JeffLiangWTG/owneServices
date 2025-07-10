using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.RefDbRepo.Common.Infrastructure.Test;
using CargoWise.RefDbRepo.Staging.Schema_New;
using NUnit.Framework;
using Safe = CargoWise.RefDbRepo.Common.SafeDataClient.CargoWise.RefDbRepo.Service.Schema_0_9_New;

namespace CargoWise.RefDbRepo.Staging.UniversalXMLStagingDataProcessor.Test;

class Test_BulkInsertDuplicateKeyExceptionHandler_RefVesselZZ : IXmlProcessIntegrationTest
{
	public string[] FileNames => ["TestFiles\\Test_BulkInsertDuplicateKeyExceptionHandler_RefVesselZZ.xml"];

	public XmlProcessIntegrationTestAssertResultHandler[] AssertResults => [
		AssertResult_AfterProcessingXml1
	];

	public string TestDescription => "Test BulkInsertDuplicateKeyExceptionHandler can delete RefVesselZZ and its Pivot table";

	public void PrepareData(IDbCommand stagingCommand, IDbCommand safeCommand)
	{
		Console.WriteLine("Start Preparing Data");

		safeCommand.CommandText = @"
INSERT INTO RefDataGrouping(ZZZ_PK, ZZZ_DataGrouping, ZZZ_Description)
VALUES(newid(), 'ZA', 'South Africa')

INSERT INTO RefVesselZZ(ZZO_PK, ZZO_Code, ZZO_RadioCallSign, ZZO_VesselType, ZZO_RN_NKCountryOfReg, ZZO_LloydsNumber, ZZO_ZZZ_NKDataGrouping)
VALUES('93A307DB-4C9D-44E6-A08B-73F2060B1AFF', 'Hong Kong Spirit', 'VRLO2', 'CV', '', '', 'ZA')

INSERT INTO RefCarrierCode(zz4_PK, ZZ4_Code, ZZ4_Description, ZZ4_IsSea, ZZ4_IsRoad, ZZ4_IsRail, ZZ4_IsAir, ZZ4_ZZZ_NKDataGrouping)
VALUES('F57B9F5E-A5B9-42AB-906A-EA0C0A531E9B', 'SNGS', 'Songa Shipping', 1, 0, 0, 0, 'ZA')

insert into RefCarrierVesselPivot(ZZQ_PK, ZZQ_ZZ4, ZZQ_ZZO)
VALUES(newid(), 'F57B9F5E-A5B9-42AB-906A-EA0C0A531E9B', '93A307DB-4C9D-44E6-A08B-73F2060B1AFF')

UPDATE RefDbVersionControl SET RVC_Deleted = 1 WHERE RVC_ParentPK = '93A307DB-4C9D-44E6-A08B-73F2060B1AFF'";
		safeCommand.ExecuteNonQuery();
	}

	void AssertResult_AfterProcessingXml1(IDbCommand stagingCommand, IDbCommand safeCommand)
	{
		Assert.Multiple(() =>
		{
			AssertResult_StagingDb_SourceData_SDA_Status_AfterProcessingXml1(stagingCommand);
			AssertResult_StagingDb_DataProcessingResult_AfterProcessingXml1(stagingCommand);

			AssertResult_SafeDb_RefVesselZZ_AfterProcessingXml1(safeCommand);
		});
	}

	void AssertResult_SafeDb_RefVesselZZ_AfterProcessingXml1(IDbCommand safeCommand)
	{
		var vesselZZs = new List<Safe.RefVesselZZ>();
		safeCommand.CommandText = "SELECT * FROM dbo.RefVesselZZ";
		using (var reader = safeCommand.ExecuteReader())
		{
			while (reader.Read())
			{
				var vesselZZ = new Safe.RefVesselZZ
				{
					ZZO_Code = reader[nameof(Safe.RefVesselZZ.ZZO_Code)].ToString(),
					ZZO_RadioCallSign = reader[nameof(Safe.RefVesselZZ.ZZO_RadioCallSign)].ToString()
				};
				vesselZZs.Add(vesselZZ);
			}
		}

		Assert.That(vesselZZs, Has.Count.EqualTo(1));
		Assert.That(vesselZZs.First().ZZO_Code, Is.EqualTo("Hong Kong Spirit"));
		Assert.That(vesselZZs.First().ZZO_RadioCallSign, Is.EqualTo("VRLO2"));
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

		Assert.That(dataProcessingResults, Has.Count.EqualTo(1));
		Assert.That(dataProcessingResults.All(x => x.DPR_Status == "QUE"));
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
}
