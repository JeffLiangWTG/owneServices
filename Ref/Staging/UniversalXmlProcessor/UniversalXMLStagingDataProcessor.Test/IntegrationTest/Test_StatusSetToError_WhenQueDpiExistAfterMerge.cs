using System.Data;
using CargoWise.RefDbRepo.Common.Infrastructure.Test;
using CargoWise.RefDbRepo.Staging.Schema_New;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.Staging.UniversalXMLStagingDataProcessor.Test
{
	class Test_StatusSetToError_WhenQueDpiExistAfterMerge : IXmlProcessIntegrationTest
	{
		public string[] FileNames => ["TestFiles\\InvalidXml_ForTest.xml"];

		public XmlProcessIntegrationTestAssertResultHandler[] AssertResults => [AssertResult_AfterProcessingXml1];

		public string TestDescription => @"Assert that the status of SourceData is set to ERR if QUE DPI is found when merging is finished.
(InvalidXml_ForTest.xml is not relevant to this IntegrationTest, which is just used as a trigger for Merger.)";

		public void PrepareData(IDbCommand stagingCommand, IDbCommand safeCommand)
		{
			var stagingSql = """
insert into SourceData(SDA_PK,SDA_Source,SDA_Filename,SDA_Filetype,SDA_FileHash,SDA_Content,SDA_ContentText,SDA_Status,SDA_CreatedTime,SDA_ContentType,SDA_SourceTime,SDA_SubSource,SDA_NotProcessedUntil,SDA_Contacts)
VALUES('4CF7733D-B1C4-4D70-80C3-DA28DDBDCAF0','INT','D:\CodeBase2\RefDataRepo\RefDataRepo\Bin\UxmlFiles\RefStlScript.xml','XML','29641CC871A5D6ED32B1682BCFA47D6B475A136E70A16B847E220C31CBB8ABD7',NULL,
	'<UniversalReferenceData><DataSource>RefStlScript</DataSource><PublicationTime>2025-05-14T00:34:52</PublicationTime><AppName>CargoWise.RefDbRepo.STLBillingCollector.CmdLine.dll</AppName><UpdateType>Full</UpdateType><Schema><EntityType Name="RefStlScript" Data="true"><Key><PropertyRef Name="STL_ActiveOn" /><PropertyRef Name="STL_FeatureCode" /><PropertyRef Name="STL_MaxCW1Version" /><PropertyRef Name="STL_MinCW1Version" /></Key><Property Name="STL_ActiveOn" Type="char" MaxLength="3" /><Property Name="STL_AdditionalRefs" Type="nvarchar" MaxLength="1000" /><Property Name="STL_BillingReference1" Type="nvarchar" MaxLength="1000" /><Property Name="STL_BillingReference2" Type="nvarchar" MaxLength="1000" /><Property Name="STL_BillingReference3" Type="nvarchar" MaxLength="1000" /><Property Name="STL_BillingReference4" Type="nvarchar" MaxLength="1000" /><Property Name="STL_BranchCode" Type="nvarchar" MaxLength="1000" /><Property Name="STL_CollectionStartDateUtc" Type="datetime" /><Property Name="STL_CompanyCode" Type="nvarchar" MaxLength="1000" /><Property Name="STL_CreatingUserCode" Type="nvarchar" MaxLength="1000" /><Property Name="STL_DataGranularity" Type="char" MaxLength="3" /><Property Name="STL_DateType" Type="char" MaxLength="3" /><Property Name="STL_FeatureCode" Type="char" MaxLength="3" /><Property Name="STL_FeatureName" Type="nvarchar" MaxLength="75" /><Property Name="STL_FromClause" Type="nvarchar(max)" /><Property Name="STL_FunctionName" Type="nvarchar" MaxLength="50" /><Property Name="STL_GuidReference" Type="nvarchar" MaxLength="1000" /><Property Name="STL_MaxCW1Version" Type="nvarchar" MaxLength="20" /><Property Name="STL_MinCW1Version" Type="nvarchar" MaxLength="20" /><Property Name="STL_ModuleName" Type="nvarchar" MaxLength="50" /><Property Name="STL_PreparationScript" Type="nvarchar(max)" /><Property Name="STL_RoleName" Type="nvarchar" MaxLength="50" /><Property Name="STL_TransactionCount" Type="nvarchar" MaxLength="1000" /><Property Name="STL_TransactionDateUtc" Type="nvarchar" MaxLength="1000" /><Property Name="STL_UsedInBilling" Type="bit" /><Property Name="STL_WhereClause" Type="nvarchar(max)" /><Property Name="STL_WithOptionRecompile" Type="bit" /></EntityType></Schema><AppProgramArgs>STLCollector</AppProgramArgs></UniversalReferenceData>',
	'PRS','2025-05-23','URD','2025-05-14','RefStlScript',NULL,NULL);
insert into DataProcessingInformation(DPI_ID,DPI_Status,DPI_SourceId,DPI_ParentTableCode,DPI_ParentPk,DPI_HasDPRRecordWhenError)
values('ED50DFE4-78DE-EC9D-2A8D-08DD99DDCFA8','QUE','4CF7733D-B1C4-4D70-80C3-DA28DDBDCAF0','STL','EC50DFE4-78DE-EC9D-D5C0-08DD99DDCFA7',0);
""";
			var safeSql = $@"
insert into [dbo].[RefDataSetInformation] (RDS_PK,RDS_DataSetId,RDS_TableName,RDS_DataSetName,RDS_DataSetTableCode,RDS_PriorityLevel)
values (newid(), 51, 'RefStlScript', 'RefStlScript','STL',0);

";
			stagingCommand.CommandText = stagingSql;
			stagingCommand.ExecuteNonQuery();
			safeCommand.CommandText = safeSql;
			safeCommand.ExecuteNonQuery();
		}

		void AssertResult_AfterProcessingXml1(IDbCommand stagingCommand, IDbCommand safeCommand)
		{
			Assert.Multiple(() =>
			{
				AssertResult_StagingDb_SourceData_SDA_Status_AfterProcessingXml1(stagingCommand);
			});
		}

		void AssertResult_StagingDb_SourceData_SDA_Status_AfterProcessingXml1(IDbCommand stagingCommand)
		{
			var status = string.Empty;
			stagingCommand.CommandText = "SELECT TOP 1 SDA_Status FROM dbo.SourceData WHERE SDA_SubSource='RefStlScript' ORDER BY SDA_CreatedTime DESC";
			using (var reader = stagingCommand.ExecuteReader())
			{
				if (reader.Read())
				{
					status = reader.GetString(0);
				}
			}
			Assert.That(status, Is.EqualTo(StatusProvider.GetERRStatus()));
		}
	}
}
