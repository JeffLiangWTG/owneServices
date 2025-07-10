using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	[TestedType(typeof(USCDataVersion))]
	sealed class USCDataVersionTest : EnterpriseBusinessObjectTestCase
	{
		[TestDate(2014, 9, 11, 16, 35, 45)]
		public void TestGetLastHTSAttempt()
		{
			var data = Factory.LoadFromNaturalKey<USCDataVersion>(USCDataVersionSchema.UZ_Name, USCDataVersion.Constant.LastHTSAttempt);
			if (data != null)
			{
				data.Delete();
			}
			data = USCDataVersion.GetLastHTSAttempt(Factory);
			AssertEquals("data.UZ_Name", USCDataVersion.Constant.LastHTSAttempt, data.UZ_Name);
			AssertEquals("data.UZ_Version", 901, data.UZ_Version);
			AssertEquals("data.UZ_Note", "," + USCDataVersion.Constant.DatabaseNameIdentifier + Db.DatabaseName, data.UZ_Note);
			AssertEquals("data.UZ_UpdateTime", ZDateTime.MinSmallDateTimeValue, data.UZ_UpdateTime);
		}

		public void TestDataNeedReloadAfterLock()
		{
			var factory1 = new BusinessObjectFactory() { RefreshEnabled = false };//imitating different app domains
			var data1 = USCDataVersion.GetLastHTSAttempt(factory1);
			data1.UZ_Note = "JOE";
			data1.UZ_UpdateTime = new ZDateTime(2017, 5, 1, 11, 30, 35);
			factory1.Save();
			var factory2 = new BusinessObjectFactory() { RefreshEnabled = false };//imitating different app domains
			var data2 = USCDataVersion.GetLastHTSAttempt(factory2);
			var factory3 = new BusinessObjectFactory() { RefreshEnabled = false };//imitating different app domains
			var data3 = USCDataVersion.GetLastHTSAttempt(factory3);
			data3.PerformUpdate(() =>
			{
				data3.UZ_Note = "BOB";
				data3.UZ_UpdateTime = new ZDateTime(2017, 5, 2, 11, 30, 35);
				factory3.Save();
			});

			AssertEquals("data2.UZ_Note", "JOE", data2.UZ_Note);
			AssertEquals("data2.UZ_UpdateTime", new ZDateTime(2017, 5, 1, 11, 30, 35), data2.UZ_UpdateTime);

			data2.PerformUpdate(() =>
			{
				AssertEquals("data2.UZ_Note", "BOB", data2.UZ_Note);
				AssertEquals("data2.UZ_UpdateTime", new ZDateTime(2017, 5, 2, 11, 30, 35), data2.UZ_UpdateTime);
				AssertEquals("data3.UZ_Note", "BOB", data3.UZ_Note);
				AssertEquals("data3.UZ_UpdateTime", new ZDateTime(2017, 5, 2, 11, 30, 35), data3.UZ_UpdateTime);
				data2.UZ_Note = "WENDY";
				data2.UZ_UpdateTime = new ZDateTime(2017, 5, 3, 11, 30, 35);
				AssertNoExceptionThrown(() => factory2.Save());
			});

			AssertEquals("data2.UZ_Note", "WENDY", data2.UZ_Note);
			AssertEquals("data2.UZ_UpdateTime", new ZDateTime(2017, 5, 3, 11, 30, 35), data2.UZ_UpdateTime);
			AssertEquals("data3.UZ_Note", "BOB", data3.UZ_Note);
			AssertEquals("data3.UZ_UpdateTime", new ZDateTime(2017, 5, 2, 11, 30, 35), data3.UZ_UpdateTime);
		}

		[TestDate(2014, 9, 11, 16, 35, 45)]
		public void TestGetOrCreate()
		{
			var data = Factory.LoadFromNaturalKey<USCDataVersion>(USCDataVersionSchema.UZ_Name, "BOB");
			if (data != null)
			{
				data.Delete();
			}
			data = USCDataVersion.GetOrCreate(Factory, "BOB");
			AssertEquals("data.UZ_Name", "BOB", data.UZ_Name);
			AssertEquals("data.UZ_Version", 0, data.UZ_Version);
			AssertEquals("data.UZ_Note", "," + USCDataVersion.Constant.DatabaseNameIdentifier + Db.DatabaseName, data.UZ_Note);
			AssertEquals("data.UZ_UpdateTime", ZDateTime.MinSmallDateTimeValue, data.UZ_UpdateTime);
		}

		[TestDate(2014, 9, 11, 16, 35, 45)]
		public void TestSetNoteWithDatabaseDetailAndUpdateTime()
		{
			var data = USCDataVersion.GetLastHTSAttempt(Factory);
			data.SetNoteWithDatabaseDetailAndUpdateTime("HELLO");
			AssertEquals("data.UZ_Note", "HELLO," + USCDataVersion.Constant.DatabaseNameIdentifier + Db.DatabaseName, data.UZ_Note);
			AssertEquals("Database", Db.DatabaseName, data.GetDatabaseNameFromNote());
			AssertEquals("data.UZ_UpdateTime", new ZDateTime(2014, 9, 11, 16, 35, 45), data.UZ_UpdateTime);
		}

		[TestDate(2014, 9, 11, 16, 35, 45)]
		public void TestGetNoteWithoutDatabaseDetail()
		{
			var data = USCDataVersion.GetLastHTSAttempt(Factory);
			data.SetNoteWithDatabaseDetailAndUpdateTime("HELLO");
			AssertEquals("data.UZ_Note", "HELLO," + USCDataVersion.Constant.DatabaseNameIdentifier + Db.DatabaseName, data.UZ_Note);
			data.UZ_Note += ",BYE";
			AssertEquals("Note", "HELLO,BYE", data.GetNoteWithoutDatabaseDetail());
		}
	}
}
