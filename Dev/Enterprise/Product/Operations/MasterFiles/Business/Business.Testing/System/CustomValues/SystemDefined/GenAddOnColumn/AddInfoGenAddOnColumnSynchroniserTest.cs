using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business.CustomValues.Testing
{
	sealed class AddInfoGenAddOnColumnSynchroniserTest : TestCaseWithDummy
	{
		public void TestSynchronise()
		{
			DummyForTest dummy = Factory.New<DummyForTest>();
			dummy.Z0_Description = "";
			XmlAddInfoForTest testAddInfo = new XmlAddInfoForTest(dummy);
			testAddInfo.Z3_Int = 12;
			Factory.Save();

			var coll = new DynamicBusinessObjectCollection(Factory);
			coll.Load(string.Format("select * from dbo.GenAddOnColumn where {0} = '{1}'", GenAddOnColumnSchema.XA_ParentID.Name, dummy.PK));
			AssertEquals("There should be one GenAddOnColumn", 1, coll.Count);
			AssertEquals("Name", TestXmlAddInfoSchema.Z3_Int.Name, coll[0][GenAddOnColumnSchema.XA_Name]);
			AssertEquals("Type", "INT", coll[0][GenAddOnColumnSchema.XA_Type]);

			testAddInfo.Z3_Int = 0;
			testAddInfo.Z3_Date = DateTime.Today;
			Factory.Save();

			coll.Load(string.Format("select * from dbo.GenAddOnColumn where {0} = '{1}'", GenAddOnColumnSchema.XA_ParentID.Name, dummy.PK));
			AssertEquals("No GenAddOnColumn, because Z3_Date not in ColumnsForFastSearch", 0, coll.Count);
		}

		public void TestSynchroniseWihtDifferentName()
		{
			var dummy = Factory.New<DummyForTest>();
			dummy.Z0_Description = "";
			var testAddInfo = new XmlAddInfoForTest(dummy);
			testAddInfo.Z3_Int = 12;
			new AddInfoGenAddOnColumnSynchroniser(dummy, testAddInfo).Synchronise(new (string genAddOnColumnName, IZType addInfoValue)[]
			{
				("TestDate1", ZDateTime.BrettsBirthday),
				("TestDate2", ZDate.BrettsBirthday)
			});
			Factory.Save();

			var coll = new DynamicBusinessObjectCollection(Factory);
			coll.Load($"select * from dbo.GenAddOnColumn where {GenAddOnColumnSchema.Constants.XA_ParentID} = '{dummy.PK}' ORDER BY {GenAddOnColumnSchema.Constants.XA_Name}");
			AssertEquals("There should be 3 GenAddOnColumn", 3, coll.Count);
			CombineAssertions(() =>
			{
				AssertGenAddOnColumn(coll[0], "TestDate1", AddOnColumnDataType.Codes.Datetime, ZDateTime.BrettsBirthday.SqlFormat);
				AssertGenAddOnColumn(coll[1], "TestDate2", AddOnColumnDataType.Codes.Date, ZDate.BrettsBirthday.ToISO8601ShortDateString());
				AssertGenAddOnColumn(coll[2], TestXmlAddInfoSchema.Constants.Z3_Int, AddOnColumnDataType.Codes.Integer, "12");
			});
		}

		void AssertGenAddOnColumn(DynamicBusinessObject bizObj, string name, string type, string data)
		{
			AssertEquals("Name", name, bizObj[GenAddOnColumnSchema.XA_Name]);
			AssertEquals("Type", type, bizObj[GenAddOnColumnSchema.XA_Type]);
			AssertEquals("Data", data, bizObj[GenAddOnColumnSchema.XA_Data]);
		}
	}
}
