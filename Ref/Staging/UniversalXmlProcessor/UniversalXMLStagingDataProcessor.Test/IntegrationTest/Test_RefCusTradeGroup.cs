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
	class Test_RefCusTradeGroup : IXmlProcessIntegrationTest
	{
		readonly Guid languageTypePK1 = Guid.Parse("31BE09B9-8D92-48BC-81CF-CFD36EDED55A");
		readonly Guid languageTypePK2 = Guid.Parse("0D964E12-336E-4E6E-B9C7-B54EB507788C");

		public string[] FileNames => ["TestFiles\\Test_RefTradeGroups1.xml", "TestFiles\\Test_RefTradeGroups2.xml"];

		public XmlProcessIntegrationTestAssertResultHandler[] AssertResults => [AssertResult_AfterProcessingXml1, AssertResult_AfterProcessingXml2];

		public string TestDescription => "Test merging XML contains date type Column(ZZB_StartDate from RefCusTradeGroupCountry)";

		public void PrepareData(IDbCommand stagingCommand, IDbCommand safeCommand)
		{
			var safeSql = $@"
insert into [dbo].[RefDataSetInformation] (RDS_PK,RDS_DataSetId,RDS_TableName,RDS_DataSetName,RDS_DataSetTableCode,RDS_PriorityLevel)
values (newid(), 12, 'RefCusTradeGroup', 'RefCusTradeGroup','ZZA',0);

insert into [dbo].[RefDataGrouping] (ZZZ_PK,ZZZ_DataGrouping,ZZZ_Description,ZZZ_ZZZ_NKGrouping)
values (newid(), 'CH', 'Switzerland', null);

insert into [dbo].[RefLanguageType] (ZX6_PK,ZX6_Language,ZX6_Description)
values ('{languageTypePK1}', 'FR', 'French'),
	('{languageTypePK2}', 'IT', 'Italian');
";
			safeCommand.CommandText = safeSql;
			safeCommand.ExecuteNonQuery();
		}

		void AssertResult_AfterProcessingXml1(IDbCommand stagingCommand, IDbCommand safeCommand)
		{
			Assert.Multiple(() =>
			{
				AssertResult_StagingDb_SourceData_SDA_Status_AfterProcessingXml(stagingCommand);
				AssertResult_StagingDb_RefCusTradeGroup_AfterProcessingXml1(stagingCommand);

				AssertResult_SafeDb_RefCusTradeGroup_AfterProcessingXml1(safeCommand);
				AssertResult_SafeDb_RefCusTradeGroupCountry_AfterProcessingXml1(safeCommand);
				AssertResult_SafeDb_RefCusTradeGroupLanguage_AfterProcessingXml1(safeCommand);
			});
		}

		void AssertResult_AfterProcessingXml2(IDbCommand stagingCommand, IDbCommand safeCommand)
		{
			Assert.Multiple(() =>
			{
				AssertResult_StagingDb_SourceData_SDA_Status_AfterProcessingXml(stagingCommand);

				AssertResult_SafeDb_RefCusTradeGroupCountry_AfterProcessingXml2(safeCommand);
			});
		}

		void AssertResult_StagingDb_SourceData_SDA_Status_AfterProcessingXml(IDbCommand stagingCommand)
		{
			var status = string.Empty;
			stagingCommand.CommandText = "SELECT TOP 1 SDA_Status FROM dbo.SourceData WHERE SDA_SubSource='Trade Groups Test' ORDER BY SDA_CreatedTime DESC";
			using (var reader = stagingCommand.ExecuteReader())
			{
				if (reader.Read())
				{
					status = reader.GetString(0);
				}
			}
			Assert.That(status, Is.EqualTo(StatusProvider.GetMERStatus()));
		}

		void AssertResult_StagingDb_RefCusTradeGroup_AfterProcessingXml1(IDbCommand stagingCommand)
		{
			var tradeGroups = new List<RefCusTradeGroup>();
			var sql = @"SELECT ZZA_TradeGroup,ZZA_Description,ZZA_ZZZ_NKDataGrouping FROM dbo.RefCusTradeGroup;";
			stagingCommand.CommandText = sql;
			using (var reader = stagingCommand.ExecuteReader())
			{
				while (reader.Read())
				{
					var tradeGroup = new RefCusTradeGroup();
					tradeGroup.ZZA_TradeGroup = reader.GetString(0);
					tradeGroup.ZZA_Description = reader.GetString(1);
					tradeGroup.ZZA_ZZZ_NKDataGrouping = reader.GetString(2);
					tradeGroups.Add(tradeGroup);
				}
			}
			Assert.AreEqual(2, tradeGroups.Count);
			Assert.That(tradeGroups.Any(x => x.ZZA_TradeGroup == "100001" && x.ZZA_Description == "European Union"));
			Assert.That(tradeGroups.Any(x => x.ZZA_TradeGroup == "100003" && x.ZZA_Description == "Developing countries and territories"));
		}

		void AssertResult_SafeDb_RefCusTradeGroup_AfterProcessingXml1(IDbCommand safeCommand)
		{
			var tradeGroups = new List<Safe.RefCusTradeGroup>();
			safeCommand.CommandText = "SELECT ZZA_TradeGroup,ZZA_Description,ZZA_ZZZ_NKDataGrouping FROM dbo.RefCusTradeGroup;";
			using (var reader = safeCommand.ExecuteReader())
			{
				while (reader.Read())
				{
					var tradeGroup = new Safe.RefCusTradeGroup();
					tradeGroup.ZZA_TradeGroup = reader.GetString(0);
					tradeGroup.ZZA_Description = reader.GetString(1);
					tradeGroup.ZZA_ZZZ_NKDataGrouping = reader.GetString(2);
					tradeGroups.Add(tradeGroup);
				}
			}
			Assert.That(tradeGroups.Any(x => x.ZZA_TradeGroup == "100001" && x.ZZA_Description == "European Union"));
			Assert.That(tradeGroups.Any(x => x.ZZA_TradeGroup == "100003" && x.ZZA_Description == "Developing countries and territories"));
		}

		void AssertResult_SafeDb_RefCusTradeGroupCountry_AfterProcessingXml1(IDbCommand safeCommand)
		{
			var countries = new List<RefCusTradeGroupCountry_Test>();
			var sql = @"select top 10 ZZB_RN_NKTradeGroupCountryCode, ZZB_StartDate, ZZB_EndDate, ZZB_SysStartTime, ZZB_Description from RefCusTradeGroupCountry join RefCusTradeGroup on ZZA_PK=ZZB_ZZA_TradeGroup
where ZZA_TradeGroup in ('100001','100003');";
			safeCommand.CommandText = sql;
			using (var reader = safeCommand.ExecuteReader())
			{
				while (reader.Read())
				{
					var country = new RefCusTradeGroupCountry_Test();
					country.ZZB_RN_NKTradeGroupCountryCode = reader.GetString(0);
					country.ZZB_StartDate = reader.GetDateTime(1);
					country.ZZB_EndDate = reader.GetDateTime(2);
					country.ZZB_SysStartTime = reader.GetDateTime(3);
					country.ZZB_Description = reader.GetString(4);
					countries.Add(country);
				}
			}
			Assert.AreEqual(2, countries.Count);
			var country1 = countries.FirstOrDefault(x => x.ZZB_RN_NKTradeGroupCountryCode == "SS");
			var country2 = countries.FirstOrDefault(x => x.ZZB_RN_NKTradeGroupCountryCode == "BY");
			Assert.NotNull(country1);
			Assert.NotNull(country2);
			Assert.AreEqual(new DateTime(2011, 9, 1), country1.ZZB_StartDate);
			Assert.AreEqual(new DateTime(2079, 6, 6), country1.ZZB_EndDate);
			Assert.AreEqual("Belarus (usual form: Bielorussia)", country2.ZZB_Description);
			Assert.AreEqual(country1.ZZB_SysStartTime, country2.ZZB_SysStartTime);
		}

		void AssertResult_SafeDb_RefCusTradeGroupLanguage_AfterProcessingXml1(IDbCommand safeCommand)
		{
			var languages = new List<Safe.RefCusTradeGroupLanguage>();
			var sql = @"select top 10 ZXD_ZX6_NKLanguage, ZXD_Description from RefCusTradeGroupLanguage join RefCusTradeGroup on ZZA_PK=ZXD_ZZA_TradeGroup
where ZZA_TradeGroup in ('100001','100003');";
			safeCommand.CommandText = sql;
			using (var reader = safeCommand.ExecuteReader())
			{
				while (reader.Read())
				{
					var language = new Safe.RefCusTradeGroupLanguage();
					language.ZXD_ZX6_NKLanguage = reader.GetString(0);
					language.ZXD_Description = reader.GetString(1);
					languages.Add(language);
				}
			}
			Assert.AreEqual(2, languages.Count);
			Assert.That(languages.Any(x => x.ZXD_ZX6_NKLanguage == "FR" && x.ZXD_Description == "Pays et territoires en développement"));
			Assert.That(languages.Any(x => x.ZXD_ZX6_NKLanguage == "IT" && x.ZXD_Description == "Paesi e territori in sviluppo"));
		}

		void AssertResult_SafeDb_RefCusTradeGroupCountry_AfterProcessingXml2(IDbCommand safeCommand)
		{
			var countries = new List<RefCusTradeGroupCountry_Test>();
			var sql = @"select top 10 ZZB_RN_NKTradeGroupCountryCode, ZZB_StartDate, ZZB_EndDate, ZZB_SysStartTime, ZZB_Description from RefCusTradeGroupCountry join RefCusTradeGroup on ZZA_PK=ZZB_ZZA_TradeGroup
where ZZA_TradeGroup in ('100001','100003');";
			safeCommand.CommandText = sql;
			using (var reader = safeCommand.ExecuteReader())
			{
				while (reader.Read())
				{
					var country = new RefCusTradeGroupCountry_Test();
					country.ZZB_RN_NKTradeGroupCountryCode = reader.GetString(0);
					country.ZZB_StartDate = reader.GetDateTime(1);
					country.ZZB_EndDate = reader.GetDateTime(2);
					country.ZZB_SysStartTime = reader.GetDateTime(3);
					country.ZZB_Description = reader.GetString(4);
					countries.Add(country);
				}
			}
			Assert.AreEqual(3, countries.Count);
			var country1 = countries.Where(x => x.ZZB_RN_NKTradeGroupCountryCode == "SS").OrderBy(x=>x.ZZB_StartDate).FirstOrDefault();
			var country2 = countries.Where(x => x.ZZB_RN_NKTradeGroupCountryCode == "SS").OrderBy(x=>x.ZZB_StartDate).LastOrDefault();
			var country3 = countries.FirstOrDefault(x => x.ZZB_RN_NKTradeGroupCountryCode == "BY");
			Assert.NotNull(country1);
			Assert.NotNull(country2);
			Assert.NotNull(country3);
			Assert.AreEqual("South Sudan, Republic of", country1.ZZB_Description);
			Assert.AreEqual(new DateTime(2011, 9, 1), country1.ZZB_StartDate);
			Assert.AreEqual(new DateTime(2024, 8, 31), country1.ZZB_EndDate);
			Assert.AreEqual("South Sudan, Republic of Test", country2.ZZB_Description);
			Assert.AreEqual(new DateTime(2024, 9, 1), country2.ZZB_StartDate);
			Assert.AreEqual(new DateTime(2079, 6, 6), country2.ZZB_EndDate);
			Assert.AreEqual("Belarus (usual form: Bielorussia) Test", country3.ZZB_Description);
			Assert.AreEqual(new DateTime(2007, 4, 1), country3.ZZB_StartDate);
			Assert.AreEqual(new DateTime(2079, 6, 6), country3.ZZB_EndDate);
			Assert.That(country1.ZZB_SysStartTime, Is.EqualTo(country2.ZZB_SysStartTime));
			Assert.That(country1.ZZB_SysStartTime, Is.EqualTo(country3.ZZB_SysStartTime));
		}

		class RefCusTradeGroupCountry_Test
		{
			public string ZZB_RN_NKTradeGroupCountryCode { get; set; }
			public DateTime ZZB_StartDate { get; set; }
			public DateTime ZZB_EndDate { get; set; }
			public string ZZB_Description { get; set; }
			public DateTime ZZB_SysStartTime { get; set; }
		}
	}
}
