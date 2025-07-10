using System;
using System.Data;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.TaiwanReferenceData.Test
{
	[TestFixture]
	sealed class ExciseTaxTariffDataRowTest
	{
		[Test]
		public void TestExciseTaxTariffDataRowData()
		{
			var table = new DataTable();
			DataColumn column;
			DataRow row;

			for (var i = 1; i <= 8; i++)
			{
				column = new DataColumn();
				column.DataType = Type.GetType("System.String");
				column.ColumnName = $"C{i}";
				table.Columns.Add(column);
			}

			row = table.NewRow();
			row["C1"] = "CT";
			row["C2"] = "ReciprocatingOver20HP";
			row["C3"] = "冷暖氣機 - 往復式中央系統型整台冷暖氣機，冷媒壓縮機二十馬力以上者";
			row["C4"] = "2023-06-14T00:00:00";
			row["C5"] = "2079-06-06T23:59:00";
			row["C6"] = "";
			row["C7"] = "CU1";
			row["C8"] = "KGM";
			var testData = ExciseTaxTariffDataRow.New(row);
			Assert.AreEqual("CT", testData.ZZ1_ZZI_TariffType);
			Assert.AreEqual("ReciprocatingOver20HP", testData.ZZ1_TariffCode);
			Assert.AreEqual("冷暖氣機 - 往復式中央系統型整台冷暖氣機，冷媒壓縮機二十馬力以上者", testData.ZZ1_Description);
			Assert.AreEqual("20230614000000", testData.ZZ1_StartDate.ToString("yyyyMMddHHmmss"));
			Assert.AreEqual("20790606235900", testData.ZZ1_EndDate.ToString("yyyyMMddHHmmss"));
			Assert.AreEqual("CU1", testData.ExciseTaxTariffUOMDataRow.ZZ8_Type);
			Assert.AreEqual("KGM", testData.ExciseTaxTariffUOMDataRow.ZZ8_UOM);

			row = table.NewRow();
			row["C1"] = "SS";
			row["C2"] = "AirplaneAndHelicopter";
			row["C3"] = "飛機、直昇機及超輕型載具";
			row["C4"] = "2023-06-34T00:00:00";
			row["C5"] = "2079-06-06T23:59:00";
			testData = ExciseTaxTariffDataRow.New(row);
			Assert.IsNull(testData);

			row = table.NewRow();
			row["C1"] = "SS";
			row["C2"] = "";
			row["C3"] = "飛機、直昇機及超輕型載具";
			row["C4"] = "2023-06-14T00:00:00";
			row["C5"] = "2079-06-06T23:59:00";
			testData = ExciseTaxTariffDataRow.New(row);
			Assert.IsNull(testData);
		}
	}
}
