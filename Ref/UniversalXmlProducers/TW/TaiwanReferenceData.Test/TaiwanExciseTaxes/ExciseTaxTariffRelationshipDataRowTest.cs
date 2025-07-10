using System;
using System.Data;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.TaiwanReferenceData.Test
{
	[TestFixture]
	sealed class ExciseTaxTariffRelationshipDataRowTest
	{
		[Test]
		public void TestExciseTaxTariffRelationshipDataRowData()
		{
			var table = new DataTable();
			DataColumn column;
			DataRow row;

			column = new DataColumn();
			column.DataType = Type.GetType("System.String");
			column.ColumnName = "C1";
			table.Columns.Add(column);

			column = new DataColumn();
			column.DataType = Type.GetType("System.String");
			column.ColumnName = "C2";
			table.Columns.Add(column);

			column = new DataColumn();
			column.DataType = Type.GetType("System.String");
			column.ColumnName = "C3";
			table.Columns.Add(column);

			column = new DataColumn();
			column.DataType = Type.GetType("System.String");
			column.ColumnName = "C4";
			table.Columns.Add(column);
			row = table.NewRow();
			row["C1"] = "CT";
			row["C2"] = "ReciprocatingOver20HP";
			row["C3"] = "冷暖氣機 - 往復式中央系統型整台冷暖氣機，冷媒壓縮機二十馬力以上者";
			row["C4"] = "84143";
			var testData = ExciseTaxTariffRelationshipDataRow.New(row);
			Assert.AreEqual("CT", testData.ZZ1_ZZI_TariffType);
			Assert.AreEqual("ReciprocatingOver20HP", testData.ZZ1_TariffCode);
			Assert.AreEqual("冷暖氣機 - 往復式中央系統型整台冷暖氣機，冷媒壓縮機二十馬力以上者", testData.ZZ1_Description);
			Assert.AreEqual("84143", testData.ZZH_TariffCode);

			row = table.NewRow();
			row["C1"] = "CT";
			row["C2"] = "ReciprocatingOver20HP";
			row["C3"] = "冷暖氣機 - 往復式中央系統型整台冷暖氣機，冷媒壓縮機二十馬力以上者";
			row["C4"] = "";
			testData = ExciseTaxTariffRelationshipDataRow.New(row);
			Assert.IsNull(testData);

			row = table.NewRow();
			row["C1"] = "CT";
			row["C2"] = "ReciprocatingOver20HP";
			row["C3"] = "";
			row["C4"] = "84143";
			testData = ExciseTaxTariffRelationshipDataRow.New(row);
			Assert.IsNull(testData);
		}
	}
}
