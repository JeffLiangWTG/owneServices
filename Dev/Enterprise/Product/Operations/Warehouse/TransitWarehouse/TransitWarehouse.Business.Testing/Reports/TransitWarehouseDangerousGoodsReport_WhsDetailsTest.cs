using System.Linq;
using CargoWise.Data;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Transit.Business.Testing
{
	class TransitWarehouseDangerousGoodsReport_WhsDetailsTest : WhsTransitTestCaseWithFactory
	{
		#region TestData_WhsAddress

		public void TestData_WhsAddress()
		{
			var warehouse = Helper.CreateWarehouse("WHS");
			warehouse.WarehouseAddress.Header.OH_RL_NKClosestPort = "AUSYD";
			warehouse.WarehouseAddress.Address1 = "Add-1";
			warehouse.WarehouseAddress.Address2 = "Add-2";
			warehouse.WarehouseAddress.City = "City1";
			warehouse.WarehouseAddress.State = "State1";
			warehouse.WarehouseAddress.Postcode = "1111";
			warehouse.WarehouseAddress.Address1 = "Add-1";
			Factory.Save();

			var expectedAddress = "Add-1\nAdd-2\nCity1, State1 1111\nAustralia";
			var actualAddress = ((IDbConnected)Factory).Connection.ExecuteScalar(string.Format("SELECT WhsAddress FROM Report_WhsTransitDangerousGoods_WhsDetails('{0}')", warehouse.PK));
			AssertEquals(expectedAddress, actualAddress);
		}

		#endregion

		#region TestData_DGContact

		public void TestData_DGContact()
		{
			var warehouse1 = Helper.CreateWarehouse("WHS");
			var org = warehouse1.WarehouseAddress.Header;
			var contact = org.Contacts.AddNew();
			contact.OC_ContactName = "BOB";
			contact.OC_HomePhone = "123";
			contact.OC_Mobile = "456";
			contact.OC_OtherPhone = "789";
			contact.OC_Phone = "101";

			warehouse1.WW_OC_DGContact = contact.PK;
			warehouse1.WW_DGContactPhoneType = PhoneTypeList.Codes.HOM;
			Factory.Save();

			AssertDGContactRow(warehouse1, "BOB", PhoneTypeList.Codes.HOM, "123");

			warehouse1.WW_DGContactPhoneType = PhoneTypeList.Codes.MOB;
			Factory.Save();
			AssertDGContactRow(warehouse1, "BOB", PhoneTypeList.Codes.MOB, "456");

			warehouse1.WW_DGContactPhoneType = PhoneTypeList.Codes.OTH;
			Factory.Save();
			AssertDGContactRow(warehouse1, "BOB", PhoneTypeList.Codes.OTH, "789");

			warehouse1.WW_DGContactPhoneType = PhoneTypeList.Codes.WRK;
			Factory.Save();
			AssertDGContactRow(warehouse1, "BOB", PhoneTypeList.Codes.WRK, "101");
		}

		void AssertDGContactRow(WhsWarehouse warehouse, string name, string contactType, string number)
		{
			var result = new DynamicBusinessObjectCollection(Factory);
			var parameters = new ZSqlParameterCollection();
			parameters.Add("@WarehousePK", warehouse.PK, WhsWarehouseSchema.PK);
			result.Load("SELECT * FROM Report_WhsTransitDangerousGoods_WhsDetails(@WarehousePK)", parameters);

			var row = result.First();
			CombineAssertions(() =>
				{
					AssertEquals("Contact Name", name, row["DGContactName"]);
					AssertEquals("Contact Type", contactType, row["DGPhoneType"]);
					AssertEquals("Phone Number", number, row["DGPhoneNumber"]);
				});
		}

		#endregion
	}
}
