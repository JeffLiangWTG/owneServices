using System;
using System.Collections.Generic;
using System.Data;
using System.Text.Json;
using CargoWise.RefDbRepo.Client.Common;
using CargoWise.RefDbRepo.Common;
using CargoWise.RefDbRepo.Common.Contract_0_9;
using Moq;
using NUnit.Framework;

namespace CargoWise.RefDataRepo.Ent.Client.Test
{
	public class SRDbDataSetHelperTest : TestCase
	{
		public void TestCreateDataTable()
		{
			var helper = new Mock<IDBHelper>();
			SRDbDataSetHelper.CreateDataTable("ABC", helper.Object);
			helper.Verify(x => x.Fill("SELECT * FROM ABC WHERE 1=0", It.IsAny<DataTable>()));
			Assert(true);
		}

		public void TestAddServerData()
		{
			var tariff = new RefCusTariff
			{
				ZZ1_TariffCode = "AAA",
				RefCusTariffType = new RefCusTariffType
				{
					ZZI_TariffType = "BBB",
					ZZI_RN_CountryOrGrouping = "XX"
				},
				ZZ1_PublishedDate = null,
				RefCusRates = new[] {
					new RefCusRate {
						ZZ2_RateFormula = "A * B"
					}
				},
				ZZ1_RN_CountryOrGrouping = "YY",
				ZZ1_Description = "LL",
				ZZ1_StartDate = new DateTime(2021, 07, 28)
			};
			var tariffTable = new DataTable("RefCusTariff");
			tariffTable.Columns.Add("ZZ1_PK", typeof(Guid));
			tariffTable.Columns.Add("ZZ1_TariffCode", typeof(string));
			tariffTable.Columns.Add("ZZ1_RN_CountryOrGrouping", typeof(string));
			tariffTable.Columns.Add("ZZ1_StartDate", typeof(DateTime));
			tariffTable.Columns.Add("ZZ1_PublishedDate", typeof(DateTime));
			tariffTable.Columns.Add("ZZ1_Description", typeof(string));
			tariffTable.Columns.Add("ZZ1_ZZI_TariffType", typeof(Guid));
			var tariffTypeTable = new DataTable("RefCusTariffType");
			tariffTypeTable.Columns.Add("ZZI_PK", typeof(Guid));
			tariffTypeTable.Columns.Add("ZZI_TariffType", typeof(string));
			tariffTypeTable.Columns.Add("ZZI_RN_CountryOrGrouping", typeof(string));
			var rateTable = new DataTable("RefCusRate");
			rateTable.Columns.Add("ZZ2_PK", typeof(Guid));
			rateTable.Columns.Add("ZZ2_RateFormula", typeof(string));
			rateTable.Columns.Add("ZZ2_ZZ1_Tariff", typeof(Guid));
			var rateMapping = new DataTableMapping
			{
				TableName = "RefCusRate"
			};
			var tariffTypeMapping = new DataTableMapping
			{
				TableName = "RefCusTariffType"
			};
			var tariffMapping = new DataTableMapping
			{
				TableName = "RefCusTariff",
				RelatedTableNames = new Dictionary<string, DataTableMapping> {
					{ "RefCusTariffType", tariffTypeMapping },
					{ "RefCusRates", rateMapping }
				},
				RelatedFKColumnNames = new Dictionary<string, string> {
					{ "RefCusTariffType", "ZZ1_ZZI_TariffType" },
					{ "RefCusRates", "ZZ2_ZZ1_Tariff" }
				}
			};
			SRDbDataSetHelper.AddServerData(new[] { tariffTable, tariffTypeTable, rateTable }, tariffMapping, JsonSerializer.SerializeToNode(tariff).AsObject());
			var tariffTypePK = tariffTypeTable.Rows[0]["ZZI_PK"];
			AssertEquals("BBB", tariffTypeTable.Rows[0]["ZZI_TariffType"]);
			AssertEquals("XX", tariffTypeTable.Rows[0]["ZZI_RN_CountryOrGrouping"]);
			AssertEquals(tariffTypePK, tariffTable.Rows[0]["ZZ1_ZZI_TariffType"]);
			AssertEquals("AAA", tariffTable.Rows[0]["ZZ1_TariffCode"]);
			AssertEquals(new DateTime(2021, 07, 28), tariffTable.Rows[0]["ZZ1_StartDate"]);
			AssertEquals(DBNull.Value, tariffTable.Rows[0]["ZZ1_PublishedDate"]);
			AssertEquals("LL", tariffTable.Rows[0]["ZZ1_Description"]);
			var tariffPK = tariffTable.Rows[0]["ZZ1_PK"];
			AssertEquals(tariffPK, rateTable.Rows[0]["ZZ2_ZZ1_Tariff"]);
			AssertEquals("A * B", rateTable.Rows[0]["ZZ2_RateFormula"]);
		}

		public void TestAddServerData_NonGuidFK()
		{
			var configType = new RefSysConfigType
			{
				ZRT_ConfigCode = "AA",
			};
			var config = new RefSysConfig
			{
				ZRC_ZRT_NKConfigCode = "AA"
			};
			configType.RefSysConfigs = new[] { config };
			var typeTable = new DataTable("RefSysConfigType");
			typeTable.Columns.Add("ZZT_PK", typeof(Guid));
			typeTable.Columns.Add("ZRT_ConfigCode", typeof(string));
			var configTable = new DataTable("RefSysConfig");
			configTable.Columns.Add("ZRC_PK", typeof(Guid));
			configTable.Columns.Add("ZRC_ZRT_NKConfigCode", typeof(string));
			var configMapping = new DataTableMapping
			{
				TableName = "RefSysConfig",
			};
			var mapping = new DataTableMapping
			{
				TableName = "RefSysConfigType",
				RelatedTableNames = new Dictionary<string, DataTableMapping> {
			{ "RefSysConfigs", configMapping }
		},
				RelatedFKColumnNames = new Dictionary<string, string>()
			};
			SRDbDataSetHelper.AddServerData(new[] { typeTable, configTable }, mapping, JsonSerializer.SerializeToNode(configType).AsObject());
			AssertEquals(1, configTable.Rows.Count);
		}
	}
}
