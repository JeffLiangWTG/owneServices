using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.TR.Business.Testing
{
	public class TraderProviderTest : TestCaseWithFactory
	{
		public void TestTraderMembersByOthers()
		{
			using (var helper = new CusEntryHeaderProviderTestHelper(Factory))
			{
				var headerJobDeclaration = helper.GetProviderHeader();
				var declaration = new CusEntryHeaderMessageProvider(headerJobDeclaration.CusEntryHeader, TRMessageTypes.Codes.DKO);

				CombineAssertions("Traders has Tax No Provider 1 Test", () =>
				{
					AssertEquals("ShipperTaxNo", "8890024379", declaration.ShipperTaxNo);
					AssertEquals("ConsigneeTaxNo", "8890024399", declaration.ConsigneeTaxNo);
					AssertEquals("TraderCount", 2, declaration.Traders.Count);
				});

				var otherBuyer = declaration.Traders.ToArray()[0];
				CombineAssertions("Only Traders Provider 2 Test", () =>
				{
					AssertEquals("Type", "DigerAlici", otherBuyer.Type);
					AssertEquals("NameAndTitle", "xTrader Full Name", otherBuyer.NameAndTitle);
					AssertEquals("PostalCode", "340305", otherBuyer.PostalCode);
					AssertEquals("IdentityType", "1", otherBuyer.IdentityType);
					AssertEquals("Fax", "02122122600", otherBuyer.Fax);
					AssertEquals("Number", "3234567890", otherBuyer.Number);
					AssertEquals("StreetNo", "xTraderAdress1 xTraderAdress2", otherBuyer.StreetNo);
					AssertEquals("Telephone", "02122122699", otherBuyer.Telephone);
					AssertEquals("CountryCode", "011", otherBuyer.CountryCode);
					AssertEquals("ProvinceAndDistrict", "MADRID", otherBuyer.ProvinceAndDistrict);
				});

				var otherSender = declaration.Traders.ToArray()[1];
				CombineAssertions("Only Traders Provider 3 Test", () =>
				{
					AssertEquals("Type", "DigerGonderici", otherSender.Type);
					AssertEquals("NameAndTitle", "xTrader2 Full Name", otherSender.NameAndTitle);
					AssertEquals("PostalCode", "340306", otherSender.PostalCode);
					AssertEquals("IdentityType", "1", otherSender.IdentityType);
					AssertEquals("Fax", "02122122602", otherSender.Fax);
					AssertEquals("Number", ZString.Empty, otherSender.Number);
					AssertEquals("StreetNo", "xTrader2Adress1 xTrader2Adress2", otherSender.StreetNo);
					AssertEquals("Telephone", "02122122601", otherSender.Telephone);
					AssertEquals("CountryCode", "624", otherSender.CountryCode);
					AssertEquals("ProvinceAndDistrict", "TEL AVIV", otherSender.ProvinceAndDistrict);
				});
			}
		}

		public void TestTraderMembersByImport()
		{
			using (var helper = new CusEntryHeaderProviderTestHelper(Factory))
			{
				var headerJobDeclaration = helper.GetProviderHeader();
				headerJobDeclaration.JE_MessageType = "IMP";
				var orgSupplier = headerJobDeclaration.Factory.New<OrgHeader>();
				orgSupplier.OH_Code = "xSupplier2";
				orgSupplier.OH_FullName = "xSupplier2 Full Name";
				orgSupplier.OH_RL_NKClosestPort = "TR";
				var addressSupplier = orgSupplier.MainAddress;
				addressSupplier.OA_OH = orgSupplier.PK;
				addressSupplier.CompanyName = "xSupplier2 Company Name";
				addressSupplier.Address1 = "xSupplierAdress1";
				addressSupplier.Address2 = "xSupplierAdress2";
				addressSupplier.OA_Phone = "02122122691";
				addressSupplier.OA_Fax = "02122122692";
				addressSupplier.City = "ISTANBUL";
				addressSupplier.Postcode = "340301";
				addressSupplier.OA_RN_NKCountryCode = "TR";
				headerJobDeclaration.JE_OH_Supplier = orgSupplier.PK;

				var declaration = new CusEntryHeaderMessageProvider(headerJobDeclaration.CusEntryHeader, TRMessageTypes.Codes.DKO);

				CombineAssertions("Traders has Tax No Provider 1 Test", () =>
				{
					AssertEquals("ShipperTaxNo", ZString.Empty, declaration.ShipperTaxNo);
					AssertEquals("ConsigneeTaxNo", "8890024399", declaration.ConsigneeTaxNo);
					AssertEquals("TraderCount", 3, declaration.Traders.Count);
				});

				var sender = declaration.Traders.ToArray()[0];
				CombineAssertions("Sender Traders Provider Test 2", () =>
				{
					AssertEquals("Type", "Gonderici", sender.Type);
					AssertEquals("NameAndTitle", "xSupplier2 Full Name", sender.NameAndTitle);
					AssertEquals("PostalCode", "340301", sender.PostalCode);
					AssertEquals("IdentityType", "1", sender.IdentityType);
					AssertEquals("Fax", "+90 212 212 26 92", sender.Fax);
					AssertEquals("Number", ZString.Empty, sender.Number);
					AssertEquals("StreetNo", "xSupplierAdress1 xSupplierAdress2", sender.StreetNo);
					AssertEquals("Telephone", "+90 212 212 26 91", sender.Telephone);
					AssertEquals("CountryCode", "052", sender.CountryCode);
					AssertEquals("ProvinceAndDistrict", "ISTANBUL", sender.ProvinceAndDistrict);
				});

				var otherBuyer = declaration.Traders.ToArray()[1];
				var otherSender = declaration.Traders.ToArray()[2];
				CombineAssertions("Other Traders Provider 3 Test", () =>
				{
					AssertEquals("Type", "DigerAlici", otherBuyer.Type);
					AssertEquals("NameAndTitle", "xTrader Full Name", otherBuyer.NameAndTitle);

					AssertEquals("Type", "DigerGonderici", otherSender.Type);
					AssertEquals("NameAndTitle", "xTrader2 Full Name", otherSender.NameAndTitle);
				});

				var orgSupplier3 = headerJobDeclaration.Factory.New<OrgHeader>();
				orgSupplier3.OH_Code = "xSupplier3";
				orgSupplier3.OH_FullName = "xSupplier3 Full Name";
				orgSupplier3.OH_RL_NKClosestPort = "TR";
				orgSupplier3.CustomsCodes.AddNew(TurkeyOrgCusCodeInfo.OrgCusCodes.YFK, "121212121212");
				var addressSupplier3 = orgSupplier3.MainAddress;
				addressSupplier3.OA_OH = orgSupplier3.PK;
				addressSupplier3.CompanyName = "xSupplier2 Company Name";
				addressSupplier3.Address1 = "xSupplierAdress1";
				addressSupplier3.Address2 = "xSupplierAdress2";
				addressSupplier3.OA_Phone = "02122122691";
				addressSupplier3.OA_Fax = "02122122692";
				addressSupplier3.City = "ISTANBUL";
				addressSupplier3.Postcode = "340301";
				addressSupplier3.OA_RN_NKCountryCode = "TR";
				headerJobDeclaration.JE_OH_Supplier = orgSupplier3.PK;

				declaration = new CusEntryHeaderMessageProvider(headerJobDeclaration.CusEntryHeader, TRMessageTypes.Codes.DKO);

				var sender3 = declaration.Traders.ToArray()[0];
				CombineAssertions("Sender Traders Provider Test 4", () =>
				{
					AssertEquals("Type", "Gonderici", sender3.Type);
					AssertEquals("NameAndTitle", ZString.Empty, sender3.NameAndTitle);
					AssertEquals("PostalCode", "340301", sender3.PostalCode);
					AssertEquals("IdentityType", "1", sender3.IdentityType);
					AssertEquals("Fax", "+90 212 212 26 92", sender3.Fax);
					AssertEquals("Number", "121212121212/YKFS", sender3.Number);
					AssertEquals("StreetNo", "xSupplierAdress1 xSupplierAdress2", sender3.StreetNo);
					AssertEquals("Telephone", "+90 212 212 26 91", sender3.Telephone);
					AssertEquals("CountryCode", ZString.Empty, sender3.CountryCode);
					AssertEquals("ProvinceAndDistrict", "ISTANBUL", sender3.ProvinceAndDistrict);
				});
			}
		}

		public void TestTraderMembersByExport()
		{
			using (var helper = new CusEntryHeaderProviderTestHelper(Factory))
			{
				var headerJobDeclaration = helper.GetProviderHeader();
				headerJobDeclaration.JE_MessageType = "EXP";
				var orgConsignee = headerJobDeclaration.Factory.New<OrgHeader>();
				orgConsignee.OH_Code = "xConsignee2";
				orgConsignee.OH_FullName = "xConsignee2 Full Name";
				var addressConsignee = orgConsignee.MainAddress;
				addressConsignee.OA_OH = orgConsignee.PK;
				addressConsignee.CompanyName = "xConsignee2 Company Name";
				addressConsignee.Address1 = "xConsigneeAdress1";
				addressConsignee.Address2 = "xConsigneeAdress2";
				addressConsignee.OA_Phone = "02122122693";
				addressConsignee.OA_Fax = "02122122694";
				addressConsignee.City = "IZMIR";
				addressConsignee.Postcode = "340302";
				addressConsignee.OA_RN_NKCountryCode = "TR";
				headerJobDeclaration.JE_OH_Importer = orgConsignee.PK;

				var declaration = new CusEntryHeaderMessageProvider(headerJobDeclaration.CusEntryHeader, TRMessageTypes.Codes.DKO);

				CombineAssertions("Traders has Tax No Provider 1 Test", () =>
				{
					AssertEquals("ShipperTaxNo", "8890024379", declaration.ShipperTaxNo);
					AssertEquals("ConsigneeTaxNo", ZString.Empty, declaration.ConsigneeTaxNo);
					AssertEquals("TraderCount", 3, declaration.Traders.Count);
				});

				var buyer = declaration.Traders.ToArray()[0];
				CombineAssertions("Buyer Traders Provider Test 2", () =>
				{
					AssertEquals("Type", "Alici", buyer.Type);
					AssertEquals("NameAndTitle", "xConsignee2 Full Name", buyer.NameAndTitle);
					AssertEquals("PostalCode", "340302", buyer.PostalCode);
					AssertEquals("IdentityType", "1", buyer.IdentityType);
					AssertEquals("Fax", "+90 212 212 26 94", buyer.Fax);
					AssertEquals("Number", ZString.Empty, buyer.Number);
					AssertEquals("StreetNo", "xConsigneeAdress1 xConsigneeAdress2", buyer.StreetNo);
					AssertEquals("Telephone", "+90 212 212 26 93", buyer.Telephone);
					AssertEquals("CountryCode", "052", buyer.CountryCode);
					AssertEquals("ProvinceAndDistrict", "IZMIR", buyer.ProvinceAndDistrict);
				});

				var otherBuyer = declaration.Traders.ToArray()[1];
				var otherSender = declaration.Traders.ToArray()[2];
				CombineAssertions("Other Traders Provider 3 Test", () =>
				{
					AssertEquals("Type", "DigerAlici", otherBuyer.Type);
					AssertEquals("NameAndTitle", "xTrader Full Name", otherBuyer.NameAndTitle);

					AssertEquals("Type", "DigerGonderici", otherSender.Type);
					AssertEquals("NameAndTitle", "xTrader2 Full Name", otherSender.NameAndTitle);
				});
			}
		}
	}
}
