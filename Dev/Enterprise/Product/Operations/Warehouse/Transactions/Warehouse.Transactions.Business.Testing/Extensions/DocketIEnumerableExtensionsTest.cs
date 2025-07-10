using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Environment.Business;
using NUnit.Framework;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	abstract class DocketIEnumerableExtensionsTest<T> : WhsTestCaseWithFactory where T : WhsDocket
	{
		#region Find Docket

		#region TestFindDocket

		public void TestFindDocket()
		{
			var whs1 = Helper.CreateWarehouse("1", "A");
			var whs2 = Helper.CreateWarehouse("2", "A");
			var whs3 = Helper.CreateWarehouse("3", "A");
			var org1 = Helper.CreateClient("1");
			var org2 = Helper.CreateClient("2");
			var org3 = Helper.CreateClient("3");
			var cons1 = Helper.CreateClient("4");
			var cons2 = Helper.CreateClient("5");
			var collection = new List<T>();
			var docket1 = GetNewDocket(whs1, org1);
			var docket2 = GetNewDocket(whs1, org2);
			var docket3 = GetNewDocket(whs2, org1);
			var docket4 = GetNewDocket(whs2, org2);
			docket1.WD_ExternalReference = "1";
			docket2.WD_ExternalReference = "2";
			docket3.WD_ExternalReference = "3";
			docket4.WD_ExternalReference = "4";
			collection.Add(docket1);
			collection.Add(docket2);
			collection.Add(docket3);
			collection.Add(docket4);
			AssertEquals(whs1.PK, collection.FindDocket(whs1, org1).WD_WW_Whs);
			AssertEquals(whs2.PK, collection.FindDocket(whs2, org1).WD_WW_Whs);
			AssertEquals(org1.PK, collection.FindDocket(whs1, org1).WD_OH_Client);
			AssertEquals(org2.PK, collection.FindDocket(whs1, org2).WD_OH_Client);
			AssertEquals(whs1.PK, collection.FindDocket(whs1, org2).WD_WW_Whs);
			AssertEquals(whs2.PK, collection.FindDocket(whs2, org2).WD_WW_Whs);
			AssertEquals(org1.PK, collection.FindDocket(whs2, org1).WD_OH_Client);
			AssertEquals(org2.PK, collection.FindDocket(whs2, org2).WD_OH_Client);
			AssertNull(collection.FindDocket(whs1, org3));
			AssertNull(collection.FindDocket(whs2, org3));
			AssertNull(collection.FindDocket(whs3, org1));
			AssertNull(collection.FindDocket(whs3, org2));
			AssertEquals(whs1.PK, collection.FindDocket(whs1.PK, org1.PK).WD_WW_Whs);
			AssertEquals(whs2.PK, collection.FindDocket(whs2.PK, org1.PK).WD_WW_Whs);
			AssertEquals(org1.PK, collection.FindDocket(whs1.PK, org1.PK).WD_OH_Client);
			AssertEquals(org2.PK, collection.FindDocket(whs1.PK, org2.PK).WD_OH_Client);
			AssertEquals(whs1.PK, collection.FindDocket(whs1.PK, org2.PK).WD_WW_Whs);
			AssertEquals(whs2.PK, collection.FindDocket(whs2.PK, org2.PK).WD_WW_Whs);
			AssertEquals(org1.PK, collection.FindDocket(whs2.PK, org1.PK).WD_OH_Client);
			AssertEquals(org2.PK, collection.FindDocket(whs2.PK, org2.PK).WD_OH_Client);
			AssertEquals("1", collection.FindDocket(whs1.PK, org1.PK, "1").WD_ExternalReference);
			AssertEquals("3", collection.FindDocket(whs2.PK, org1.PK, "3").WD_ExternalReference);
			AssertNull(collection.FindDocket(whs1.PK, org3.PK));
			AssertNull(collection.FindDocket(whs2.PK, org3.PK));
			AssertNull(collection.FindDocket(whs3.PK, org1.PK));
			AssertNull(collection.FindDocket(whs3.PK, org2.PK));
			AssertNull(collection.FindDocket(whs1.PK, org1.PK, "2"));
		}

		#endregion

		#region TestFindDocketWithRequiredDate

		[TestDate(2012, 03, 20)]
		public void TestFindDocketWithRequiredDate()
		{
			var requiredDate = ZDateTimeOffset.Now;
			var data = new TestDataSimpleEnvironment(Factory);
			var collection = new List<T>();
			var docket = GetNewDocket(data.Whs1, data.Org1);
			docket.WD_RequiredDate = ZDateTimeOffset.Empty; // to reset required date
			docket.WD_RequiredDate = requiredDate;
			collection.Add(docket);
			AssertEquals("Precondition", 1, collection.Count);
			var matchingDocket = collection.FindDocket(data.Whs1.PK, data.Org1.PK, docket.WD_ExternalReference, null,
				requiredDate, true);
			AssertEquals("Docket should be matched regardless of the required date.", docket, matchingDocket);
			var noMatchingDocket =
				collection.FindDocket(data.Whs1.PK, data.Org1.PK, "", null, requiredDate.AddDays(1), true);
			AssertNull("No matching docket found.", noMatchingDocket);
		}

		#endregion

		#endregion

		#region Implementation

		T GetNewDocket(WhsWarehouse whs, OrgHeader client)
		{
			var docket = Factory.New<T>();
			docket.WD_WW_Whs = whs.PK;
			docket.WD_OH_Client = client.PK;
			return docket;
		}

		#endregion
	}
}
