using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.RefDbRepo.Common.Infrastructure.Test;
using NUnit.Framework;
using Safe = CargoWise.RefDbRepo.Common.SafeDataClient.CargoWise.RefDbRepo.Service.Schema_0_9_New;

namespace CargoWise.RefDbRepo.Staging.UniversalXMLStagingDataProcessor.Test
{
	class Test_UNLOCOIsDataFalse : IXmlProcessIntegrationTest
	{
		public string[] FileNames => ["TestFiles\\Test_UNLOCOIsDataFalse_1.xml", "TestFiles\\Test_UNLOCOIsDataFalse_2.xml"];

		public XmlProcessIntegrationTestAssertResultHandler[] AssertResults => [AssertResult_AfterProcessingXml1, AssertResult_AfterProcessingXml2];

		public string TestDescription => "Test IsData prop update while entity IsData=false";

		public void PrepareData(IDbCommand stagingCommand, IDbCommand safeCommand)
		{
			Console.WriteLine("Start Preparing Data");

			var safeSql = @"
insert into [dbo].[RefDataSetInformation] (RDS_PK,RDS_DataSetId,RDS_TableName,RDS_DataSetName,RDS_DataSetTableCode,RDS_PriorityLevel)
values (NEWID(), 29, 'RefUNLOCO', 'RefUNLOCO','RL',0);
";
			safeCommand.CommandText = safeSql;
			safeCommand.ExecuteNonQuery();

			Console.WriteLine("Preparing Data Successfully");
		}

		void AssertResult_AfterProcessingXml1(IDbCommand stagingCommand, IDbCommand safeCommand)
		{
			Assert.Multiple(() =>
			{
				AssertResult_SafeDb_RefUNLOCO_AfterProcessingXml1(safeCommand);
			});
		}

		void AssertResult_AfterProcessingXml2(IDbCommand stagingCommand, IDbCommand safeCommand)
		{
			Assert.Multiple(() =>
			{
				AssertResult_SafeDb_RefUNLOCO_AfterProcessingXml2(safeCommand);
			});
		}

		void AssertResult_SafeDb_RefUNLOCO_AfterProcessingXml1(IDbCommand safeCommand)
		{
			var uNLOCOs = new List<Safe.RefUNLOCO>();
			safeCommand.CommandText = "SELECT * FROM dbo.RefUNLOCO";
			using (var reader = safeCommand.ExecuteReader())
			{
				while (reader.Read())
				{
					var uNLOCO = new Safe.RefUNLOCO();
					uNLOCO.RL_CoOrdinates = reader[nameof(Safe.RefUNLOCO.RL_CoOrdinates)].ToString();
					uNLOCOs.Add(uNLOCO);
				}
			}
			Assert.That(uNLOCOs, Has.Count.EqualTo(1));
			Assert.AreEqual("", uNLOCOs.First().RL_CoOrdinates);
		}

		void AssertResult_SafeDb_RefUNLOCO_AfterProcessingXml2(IDbCommand safeCommand)
		{
			var uNLOCOs = new List<Safe.RefUNLOCO>();
			safeCommand.CommandText = "SELECT * FROM dbo.RefUNLOCO";
			using (var reader = safeCommand.ExecuteReader())
			{
				while (reader.Read())
				{
					var uNLOCO = new Safe.RefUNLOCO();
					uNLOCO.RL_CoOrdinates = reader[nameof(Safe.RefUNLOCO.RL_CoOrdinates)].ToString();
					uNLOCO.RL_Code = reader[nameof(Safe.RefUNLOCO.RL_Code)].ToString();
					uNLOCOs.Add(uNLOCO);
				}
			}
			Assert.That(uNLOCOs, Has.Count.EqualTo(1));
			Assert.AreEqual("1237S 01312E", uNLOCOs.First().RL_CoOrdinates);
		}
	}
}
