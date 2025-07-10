using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.RefDbRepo.Common.Infrastructure.Test;
using CargoWise.RefDbRepo.Staging.Schema_New;
using Microsoft.SqlServer.Types;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.Staging.UniversalXMLStagingDataProcessor.Test
{
	class Test_RefUNLOCO : IXmlProcessIntegrationTest
	{
		public string[] FileNames => ["TestFiles\\RefUNLOCO_Updater.xml"];

		public XmlProcessIntegrationTestAssertResultHandler[] AssertResults => [AssertResult_AfterProcessingXml1];

		public string TestDescription => "Test RefUNLOCO with geoLocation";

		public void PrepareData(IDbCommand stagingCommand, IDbCommand safeCommand)
		{
			var safeSql = $@"
insert into [dbo].[RefDataSetInformation] (RDS_PK,RDS_DataSetId,RDS_TableName,RDS_DataSetName,RDS_DataSetTableCode,RDS_PriorityLevel)
values (newid(), 29, 'RefUNLOCO', 'RefUNLOCO','RL',0);
insert into [dbo].[RefUNLOCO] ([RL_PK],[RL_Code],[RL_IsActive],[RL_PortName],[RL_NameWithDiacriticals],[RL_IATA],[RL_CoOrdinates],[RL_HasAirport],[RL_HasSeaport]
		   ,[RL_HasRail],[RL_HasRoad],[RL_HasPost],[RL_HasCustomsLodge],[RL_HasUnload],[RL_HasStore],[RL_HasTerminal],[RL_HasDischarge],[RL_HasOutport],[RL_HasBorderCrossing]
		   ,[RL_R3],[RL_RN_NKCountryCode],[RL_RW],[RL_IATARegionCode],[RL_GeoLocation],[RL_UserOverride])
values (newid(), 'AOFBY', 1, 'Farta Test', 'Forta Bay', '', '1237S 01312E', 0, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, null, 'AO', null, '', GEOGRAPHY::STGeomFromText('POINT(30 10)', 4326),0);
";
			safeCommand.CommandText = safeSql;
			safeCommand.ExecuteNonQuery();
		}

		void AssertResult_AfterProcessingXml1(IDbCommand stagingCommand, IDbCommand safeCommand)
		{
			Assert.Multiple(() =>
			{
				AssertResult_StagingDb_SourceData_SDA_Status_AfterProcessingXml1(stagingCommand);
				AssertResult_StagingDb_RefPortPolygon_AfterProcessingXml(stagingCommand);

				AssertResult_SafeDb_RefPortPolygon_AfterProcessingXml(safeCommand);
			});
		}

		void AssertResult_StagingDb_SourceData_SDA_Status_AfterProcessingXml1(IDbCommand stagingCommand)
		{
			var status = string.Empty;
			stagingCommand.CommandText = "SELECT TOP 1 SDA_Status FROM dbo.SourceData WHERE SDA_SubSource='UNLOCO Updater' ORDER BY SDA_CreatedTime DESC";
			using (var reader = stagingCommand.ExecuteReader())
			{
				if (reader.Read())
				{
					status = reader.GetString(0);
				}
			}
			Assert.That(status, Is.EqualTo(StatusProvider.GetMERStatus()));
		}

		void AssertResult_StagingDb_RefPortPolygon_AfterProcessingXml(IDbCommand stagingCommand)
		{
			var unlocos = new List<LocalRefUNLOCO>();
			var sql = @"SELECT RL_PortName,RL_GeoLocation FROM dbo.RefUNLOCO;";
			stagingCommand.CommandText = sql;
			using (var reader = stagingCommand.ExecuteReader())
			{
				while (reader.Read())
				{
					var unloco = new LocalRefUNLOCO();
					unloco.RL_PortName = reader.GetString(0);
					if (!reader.IsDBNull(1))
					{
						unloco.RL_GeoLocation = (SqlGeography)reader[1];
					}
					unlocos.Add(unloco);
				}
			}
			Assert.AreEqual(1, unlocos.Count);
			Assert.That(unlocos.First().RL_PortName.ToString(), Is.EqualTo("Farta Test33").IgnoreCase);
			Assert.That(unlocos.First().RL_GeoLocation.ToString(), Is.EqualTo("POINT EMPTY").IgnoreCase);
		}

		void AssertResult_SafeDb_RefPortPolygon_AfterProcessingXml(IDbCommand safeCommand)
		{
			var unlocos = new List<LocalRefUNLOCO>();
			safeCommand.CommandText = "SELECT RL_PortName,RL_GeoLocation FROM dbo.RefUNLOCO;";
			using (var reader = safeCommand.ExecuteReader())
			{
				while (reader.Read())
				{
					var unloco = new LocalRefUNLOCO();
					unloco.RL_PortName = reader.GetString(0);
					if (!reader.IsDBNull(1))
					{
						unloco.RL_GeoLocation = (SqlGeography)reader[1];
					}
					unlocos.Add(unloco);
				}
			}
			Assert.AreEqual(1, unlocos.Count);
			Assert.That(unlocos.First().RL_PortName, Is.EqualTo("Farta Test33").IgnoreCase);
			Assert.That(unlocos.First().RL_GeoLocation.ToString(), Is.EqualTo("POINT (30 10)").IgnoreCase);
		}

		class LocalRefUNLOCO
		{
			public string RL_PortName { get; set; }
			public SqlGeography RL_GeoLocation { get; set; }
		}
	}
}
