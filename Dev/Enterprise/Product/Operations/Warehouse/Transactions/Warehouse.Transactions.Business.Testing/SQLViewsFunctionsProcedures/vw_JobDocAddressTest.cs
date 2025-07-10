using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

// Extracted from Warehouse\Transactions\Warehouse.Transactions.Business.Testing\SQLViewsFunctionsProcedures\WhsProductCategoryAndChildCategoriesTest.cs - refer to the original file for checking history
namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	class vw_JobDocAddressTest : WhsTestCaseWithFactory
	{
		public void TestView()
		{
			var client = Helper.CreateClient();
			var whs = Helper.CreateWarehouse("Test");

			var org = Helper.CreateClient("TST", "Full Name1");
			org.OH_RL_NKClosestPort = "NZAKL";
			var address = org.Addresses.AddNew();
			address.OA_Address1 = "123 Main Street1";
			address.OA_Address2 = "Add2";
			address.OA_City = "SYD";
			address.OA_State = "NSW1";
			address.OA_PostCode = "1234";
			var order = Helper.CreateWhsOrder(client, whs);
			order.ConsigneeDocAddress.E2_OA_Address = address.PK;
			Factory.Save();
			AssertCompanyName(order.ConsigneeDocAddress.PK, "CEA", org.PK, address.PK, order.PK, "WD", "Full Name1",
				"123 Main Street1", "Add2", "SYD", "NSW1", "1234", "NZ");

			address.OA_CompanyNameOverride = "AddressCoOverride";
			address.OA_RL_NKRelatedPortCode = "AUSYD";
			Factory.Save();
			AssertCompanyName(order.ConsigneeDocAddress.PK, "CEA", org.PK, address.PK, order.PK, "WD",
				"AddressCoOverride", "123 Main Street1", "Add2", "SYD", "NSW1", "1234", "AU");

			var cneDocAddress = order.ConsigneeDocAddress;
			cneDocAddress.E2_AddressOverride = true;
			cneDocAddress.E2_CompanyName = "Overridden Co";
			cneDocAddress.E2_Address1 = "Overridden Addy1";
			cneDocAddress.E2_Address2 = "Overridden Addy2";
			cneDocAddress.E2_City = "Overridden City";
			cneDocAddress.E2_State = "Overridden State";
			cneDocAddress.E2_Postcode = "112233";
			cneDocAddress.E2_RN_NKCountryCode = "US";
			Factory.Save();
			AssertCompanyName(order.ConsigneeDocAddress.PK, "CEA", ZGuid.Empty, ZGuid.Empty, order.PK, "WD",
				"Overridden Co", "Overridden Addy1", "Overridden Addy2", "Overridden City", "Overridden State",
				"112233", "US");
		}

		void AssertCompanyName(ZGuid docAddressPK, string addressType, ZGuid ohPK, ZGuid oaPK, ZGuid parentID,
			string parentTableCode, string companyName, string address1, string address2, string city, string state,
			string postCode, string countryCode)
		{
			var row = GetDocAddressViewRow(docAddressPK);

			AssertEquals("E2_AddressType", addressType, row["E2_AddressType"]);
			AssertEquals("OH PK", ohPK, row["OH_PK"]);
			AssertEquals("OA PK", oaPK, row["OA_PK"]);
			AssertEquals("E2_ParentID", parentID, row["E2_ParentID"]);
			AssertEquals("E2_ParentTableCode", parentTableCode, row["E2_ParentTableCode"]);
			AssertEquals("Company Name", companyName, row["CompanyName"]);
			AssertEquals("Address1", address1, row["Address1"]);
			AssertEquals("Address2", address2, row["Address2"]);
			AssertEquals("City", city, row["City"]);
			AssertEquals("State", state, row["State"]);
			AssertEquals("PostCode", postCode, row["PostCode"]);
			AssertEquals("Country", countryCode, row["CountryCode"]);
		}

		DynamicBusinessObject GetDocAddressViewRow(ZGuid pk)
		{
			var result = new DynamicBusinessObjectCollection(Factory);
			var sql = @"select * from dbo.vw_JobDocAddress where E2_PK = @PK";
			var sqlParams = new ZSqlParameterCollection();

			sqlParams.Add("@PK", pk, JobDocAddressSchema.PK);
			result.Load(sql, sqlParams);

			return result[0];
		}
	}
}
