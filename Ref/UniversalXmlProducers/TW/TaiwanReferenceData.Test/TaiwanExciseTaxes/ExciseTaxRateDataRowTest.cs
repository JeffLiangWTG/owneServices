using System;
using System.Data;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.TaiwanReferenceData.Test
{
	[TestFixture]
	sealed class ExciseTaxRateDataRowTest
	{
		[Test]
		public void TestExciseTaxRateDataRowData()
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

			column = new DataColumn();
			column.DataType = Type.GetType("System.String");
			column.ColumnName = "C5";
			table.Columns.Add(column);

			column = new DataColumn();
			column.DataType = Type.GetType("System.String");
			column.ColumnName = "C6";
			table.Columns.Add(column);

			column = new DataColumn();
			column.DataType = Type.GetType("System.String");
			column.ColumnName = "C7";
			table.Columns.Add(column);

			column = new DataColumn();
			column.DataType = Type.GetType("System.String");
			column.ColumnName = "C8";
			table.Columns.Add(column);

			column = new DataColumn();
			column.DataType = Type.GetType("System.String");
			column.ColumnName = "C9";
			table.Columns.Add(column);

			column = new DataColumn();
			column.DataType = Type.GetType("System.String");
			column.ColumnName = "C10";
			table.Columns.Add(column);

			row = table.NewRow();
			row["C1"] = "CT";
			row["C2"] = "COM";
			row["C3"] = "CTA";
			row["C4"] = "ReciprocatingOver20HP";
			row["C5"] = "冷暖氣機 - 往復式中央系統型整台冷暖氣機，冷媒壓縮機二十馬力以上者";
			row["C6"] = "0.7875*VFD";
			row["C7"] = "0.7875";
			row["C8"] = "1900-01-01 00:00:00";
			row["C9"] = "2079-06-06 23:59:00";
			row["C10"] = "ALL";
			var testData = ExciseTaxRateDataRow.New(row);
			Assert.AreEqual("CT", testData.ZZ1_ZZI_TariffType);
			Assert.AreEqual("COM", testData.ZZ2_ZY1_ZZR_NKRateType);
			Assert.AreEqual("CTA", testData.ZZ2_ZY1_NKRateCode);
			Assert.AreEqual("冷暖氣機 - 往復式中央系統型整台冷暖氣機，冷媒壓縮機二十馬力以上者", testData.ZZ1_Description);
			Assert.AreEqual("0.7875*VFD", testData.ZZ2_RateFormula);
			Assert.AreEqual("0.7875", testData.ZZ2_RateFormulaDerivedFrom);
			Assert.AreEqual("19000101000000", testData.ZZ2_StartDate.ToString("yyyyMMddHHmmss"));
			Assert.AreEqual("20790606235900", testData.ZZ2_EndDate.ToString("yyyyMMddHHmmss"));
			Assert.AreEqual("ALL", testData.ZZT_ZZA_NKTradeGroup);

			row = table.NewRow();
			row["C1"] = "CT";
			row["C2"] = "COM";
			row["C3"] = "CTA";
			row["C4"] = "ReciprocatingOver20HP";
			row["C5"] = "冷暖氣機 - 往復式中央系統型整台冷暖氣機，冷媒壓縮機二十馬力以上者";
			row["C6"] = "0.7875*VFD";
			row["C7"] = "0.7875";
			row["C8"] = "1900-01-36 00:00:00";
			row["C9"] = "2079-06-06 23:59:00";
			row["C10"] = "ALL";
			testData = ExciseTaxRateDataRow.New(row);
			Assert.IsNull(testData);

			row = table.NewRow();
			row["C1"] = "CT";
			row["C2"] = "COM";
			row["C3"] = "CTA";
			row["C4"] = "ReciprocatingOver20HP";
			row["C5"] = "";
			row["C6"] = "0.7875*VFD";
			row["C7"] = "0.7875";
			row["C8"] = "1900-01-01 00:00:00";
			row["C9"] = "2079-06-06 23:59:00";
			row["C10"] = "ALL";
			testData = ExciseTaxRateDataRow.New(row);
			Assert.IsNull(testData);
		}
	}
}
