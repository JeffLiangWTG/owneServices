using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

// Extracted from Warehouse\Transactions\Warehouse.Transactions.Business.Testing\SQLViewsFunctionsProcedures\WhsProductCategoryAndChildCategoriesTest.cs - refer to the original file for checking history
namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	class JobDocAddressFormatterTest : WhsTestCaseWithFactory
	{
		public void TestFunction()
		{
			var client = Helper.CreateClient();
			var whs = Helper.CreateWarehouse("Test");
			var order = Helper.CreateWhsOrder(client, whs);
			var orgAddress = Factory.New<OrgAddress>();
			var org = Factory.New<OrgHeader>();
			var docAddress = order.ConsigneeDocAddress;

			org.OH_Code = "TST";
			orgAddress.OA_OH = org.PK;
			order.ConsigneePK = org.PK;
			order.ConsigneeAddressPK = orgAddress.PK;

			AssertFormattedAddress(docAddress);

			docAddress.E2_AddressOverride = false;
			docAddress.Organisation.OH_RL_NKClosestPort = "ORD";

			var address = docAddress.Address;
			address.OA_Address1 = "123 Main Street1";
			address.OA_Address2 = "Suit 100";
			address.OA_City = "City1";
			address.OA_State = "State1";
			address.OA_PostCode = "10000";

			Factory.Save();

			AssertFormattedAddress(docAddress);

			docAddress.E2_AddressOverride = true;
			docAddress.E2_RN_NKCountryCode = "AU";
			docAddress.E2_Address1 = "123 Main Street2";
			docAddress.E2_Address2 = "Suit 200";
			docAddress.E2_City = "City2";
			docAddress.E2_State = "State2";
			docAddress.E2_Postcode = "20000";

			Factory.Save();

			AssertFormattedAddress(docAddress);
		}

		void AssertFormattedAddress(JobDocAddress docAddress)
		{
			AssertEquals("Formatted Address", GetFormattedAddress(docAddress),
				Formatter(docAddress.E2_ParentID, docAddress.E2_ParentTableCode, docAddress.E2_AddressType));
		}

		ZString GetFormattedAddress(JobDocAddress docAddress)
		{
			var result = new ZString("");
			var retChar = new ZString((char)10, 1);
			var delimiter = new ZString("");
			var address1 = "";
			var address2 = "";
			var city = "";
			var state = "";
			var postCode = "";
			var country = "";

			if (docAddress.E2_AddressOverride)
			{
				address1 = docAddress.E2_Address1.Trim();
				address2 = docAddress.E2_Address2.Trim();
				city = docAddress.E2_City.Trim();
				state = docAddress.E2_State.Trim();
				postCode = docAddress.E2_Postcode.Trim();
				if (docAddress.Country != null)
				{
					country = docAddress.Country.RN_Desc.Trim();
				}
			}
			else
			{
				var address = docAddress.Address;
				if (address != null)
				{
					address1 = address.OA_Address1.Trim();
					address2 = address.OA_Address2.Trim();
					city = address.OA_City.Trim();
					state = address.OA_State.Trim();
					postCode = address.OA_PostCode.Trim();
					if (docAddress.Organisation.ClosestPort != null)
					{
						country = docAddress.Organisation.ClosestPort.Country.RN_Desc.Trim();
					}
				}
			}

			if (!string.IsNullOrEmpty(address1))
			{
				result += delimiter + address1;
				delimiter = retChar;
			}

			if (!string.IsNullOrEmpty(address2))
			{
				result += delimiter + address2;
				delimiter = retChar;
			}

			if (!string.IsNullOrEmpty(city) || !string.IsNullOrEmpty(state) || !string.IsNullOrEmpty(postCode))
			{
				result += delimiter + city;
				delimiter = retChar;
				if (!string.IsNullOrEmpty(city) && (!string.IsNullOrEmpty(state) || !string.IsNullOrEmpty(postCode)))
				{
					result += ", ";
				}

				result += state;
				if (!string.IsNullOrEmpty(state))
				{
					result += " ";
				}

				result += postCode;
			}

			result += delimiter + country;
			return result;
		}

		ZString Formatter(ZGuid pk, ZString tableCode, ZString addressType)
		{
			var result = new DynamicBusinessObjectCollection(Factory);
			var sql =
				@"select DocAddress AS Address from dbo.JobDocAddressFormatter(@ParentPK, @ParentTableCode, @AddressType, @AddressParts)";

			var sqlParams = new ZSqlParameterCollection();
			sqlParams.Add("@ParentPK", pk, JobDocAddressSchema.E2_ParentID);
			sqlParams.Add("@ParentTableCode", tableCode, JobDocAddressSchema.E2_ParentTableCode);
			sqlParams.Add("@AddressType", addressType, JobDocAddressSchema.E2_AddressType);
			sqlParams.Add("@AddressParts", "ALL", JobDocAddressSchema.E2_AddressType);

			result.Load(sql, sqlParams);
			if (result.Count > 0)
			{
				var dynamicObject = result[0];
				return (ZString)(dynamicObject["Address"]);
			}

			return ZString.Empty;
		}
	}
}
