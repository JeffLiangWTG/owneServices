using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.RefDbRepo.Common.Infrastructure.Test;
using CargoWise.RefDbRepo.Staging.Schema_New;
using Microsoft.SqlServer.Types;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.Staging.UniversalXMLStagingDataProcessor.Test
{
	class Test_RefPortPolygon : IXmlProcessIntegrationTest
	{
		public string[] FileNames => ["TestFiles\\RefPortPolygon.xml"];

		public XmlProcessIntegrationTestAssertResultHandler[] AssertResults => [AssertResult_AfterProcessingXml1];

		public string TestDescription => "Test RefPortPolygon with geography field";

		public void PrepareData(IDbCommand stagingCommand, IDbCommand safeCommand)
		{
			var safeSql = $@"
insert into [dbo].[RefDataSetInformation] (RDS_PK,RDS_DataSetId,RDS_TableName,RDS_DataSetName,RDS_DataSetTableCode,RDS_PriorityLevel)
values (newid(), 44, 'RefPortPolygon', 'RefPortPolygon','RPP',0);
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
			stagingCommand.CommandText = "SELECT TOP 1 SDA_Status FROM dbo.SourceData WHERE SDA_SubSource='PortPolygons' ORDER BY SDA_CreatedTime DESC";
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
			var portPolygons = new List<LocalRefPortPolygon>();
			var sql = @"SELECT RPP_PortId,RPP_SerializedPolygon FROM dbo.RefPortPolygon;";
			stagingCommand.CommandText = sql;
			using (var reader = stagingCommand.ExecuteReader())
			{
				while (reader.Read())
				{
					var portPolygon = new LocalRefPortPolygon();
					portPolygon.RPP_PortId = reader.GetInt32(0);
					if (!reader.IsDBNull(1))
					{
						portPolygon.RPP_SerializedPolygon = (SqlGeography)reader[1];
					}
					portPolygons.Add(portPolygon);
				}
			}
			Assert.AreEqual(4, portPolygons.Count);
			Assert.That(portPolygons.First(x => x.RPP_PortId == 1).RPP_SerializedPolygon.ToString(), Is.EqualTo("GEOMETRYCOLLECTION EMPTY").IgnoreCase);
			Assert.That(portPolygons.First(x => x.RPP_PortId == 2).RPP_SerializedPolygon.ToString(), Is.EqualTo("POLYGON ((1 0, 1 2, 0 1, 0 0, 1 0))").IgnoreCase);
			Assert.That(portPolygons.First(x => x.RPP_PortId == 3).RPP_SerializedPolygon.ToString(), Is.EqualTo("GEOMETRYCOLLECTION EMPTY").IgnoreCase);
			Assert.That(portPolygons.First(x => x.RPP_PortId == 4).RPP_SerializedPolygon.ToString(), Is.EqualTo("POLYGON ((1 0, 2 1, 0 1, 0 0, 1 0))").IgnoreCase);
		}

		void AssertResult_SafeDb_RefPortPolygon_AfterProcessingXml(IDbCommand safeCommand)
		{
			var portPolygons = new List<LocalRefPortPolygon>();
			safeCommand.CommandText = "SELECT RPP_PortId,RPP_SerializedPolygon FROM dbo.RefPortPolygon;";
			using (var reader = safeCommand.ExecuteReader())
			{
				while (reader.Read())
				{
					var portPolygon = new LocalRefPortPolygon();
					portPolygon.RPP_PortId = reader.GetInt32(0);
					if (!reader.IsDBNull(1))
					{
						portPolygon.RPP_SerializedPolygon = (SqlGeography)reader[1];
					}
					portPolygons.Add(portPolygon);
				}
			}
			Assert.AreEqual(4, portPolygons.Count);
			Assert.That(portPolygons.First(x => x.RPP_PortId == 1).RPP_SerializedPolygon.ToString(), Is.EqualTo("GEOMETRYCOLLECTION EMPTY").IgnoreCase);
			Assert.That(portPolygons.First(x => x.RPP_PortId == 2).RPP_SerializedPolygon.ToString(), Is.EqualTo("POLYGON ((1 0, 1 2, 0 1, 0 0, 1 0))").IgnoreCase);
			Assert.That(portPolygons.First(x => x.RPP_PortId == 3).RPP_SerializedPolygon.ToString(), Is.EqualTo("GEOMETRYCOLLECTION EMPTY").IgnoreCase);
			Assert.That(portPolygons.First(x => x.RPP_PortId == 4).RPP_SerializedPolygon.ToString(), Is.EqualTo("POLYGON ((1 0, 2 1, 0 1, 0 0, 1 0))").IgnoreCase);
		}

		class LocalRefPortPolygon
		{
			public int RPP_PortId { get; set; }
			public SqlGeography RPP_SerializedPolygon { get; set; }
		}
	}
}
