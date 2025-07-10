using System;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business.Customs;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(CusRefPacks))]
	sealed class CusRefPacksTest : EnterpriseBusinessObjectTestCase
	{
		public void TestRP_Type_List()
		{
			RefPacks.RP_CustomsCountry = Core.Constants.CountryCodes.Eritrea;
			AssertSame(Factory.New<BaseRefPacks>().RP_Type_List, RefPacks.RP_Type_List);
		}

		public void TestRP_CustomsPack_List()
		{
			var customsCountry = RefPacks.RP_CustomsCountry;
			RefPacks.RP_Type = "";
			var list = RefPacks.RP_CustomsPack_List;

			AssertEquals("default list", 21, list.Count);
			Assert("default list", list.ContainsCode("NR"));

			RefPacks.RP_Type = "DTP";
			list = RefPacks.RP_CustomsPack_List;
			AssertEquals("DTP list", 21, list.Count);
			Assert("DTP list", list.ContainsCode("NR"));

			RefPacks.RP_Type = "CIP";
			list = RefPacks.RP_CustomsPack_List;
			AssertEquals("CIP list", 21, list.Count);
			Assert("CIP list", list.ContainsCode("NR"));

			RefPacks.RP_Type = "AMS";
			list = RefPacks.RP_CustomsPack_List;
			AssertEquals("AMS list", 139, list.Count);
			Assert("AMS list", list.ContainsCode("WRP"));

			RefPacks.RP_Type = "AFR";
			list = RefPacks.RP_CustomsPack_List;
			AssertEquals("AFR list", 75, list.Count);
			Assert("AFR list", list.ContainsCode("ZZ"));

			RefPacks.RP_CustomsCountry = customsCountry;

			RefPacks.RP_Type = "GMB";
			list = RefPacks.RP_CustomsPack_List;
			AssertEquals("GMB list", 76, list.Count);
			Assert("GMB list", list.ContainsCode("BOX"));

			RefPacks.RP_Type = "GMP";
			list = RefPacks.RP_CustomsPack_List;
			AssertEquals("GMP list", 76, list.Count);
			Assert("GMP list", list.ContainsCode("BOX"));
		}

		public void TestRP_CustomsPack_List_CIP()
		{
			RefPacks.RP_Type = "CIP";
			RefPacks.RP_CustomsCountry = Core.Constants.CountryCodes.UnitedStates;
			var list = RefPacks.RP_CustomsPack_List;
			AssertEquals("US CIPLIST", 117, list.Count);

			RefPacks.RP_CustomsCountry = Core.Constants.CountryCodes.Canada;
			list = RefPacks.RP_CustomsPack_List;
			AssertEquals(115, list.Count);

			RefPacks.RP_CustomsCountry = Core.Constants.CountryCodes.Australia;
			list = RefPacks.RP_CustomsPack_List;
			var expectedList = new BaseCusUQList();
			AssertEquals(21, list.Count);
			AssertContainsExactElementsInAnyOrder(expectedList, list);
		}

		public void TestRP_CustomsPack_List_ALL()
		{
			Factory.Load(ZZRefCusCodeListSchema.Constants.Prefix, ZGuid.NewZGuid()); // cause system to create DataTable
			var table = ((INeedDataSet)Factory).Data.Tables[ZZRefCusCodeListSchema.Constants.TableName];
			var row = table.NewRow();
			var pk = ZGuid.NewZGuid();
			row[ZZRefCusCodeListSchema.Constants.PK] = pk.ToGuid();
			row[ZZRefCusCodeListSchema.Constants.ZZD_CodeType] = Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.UnitedNationsPackageTypes;
			row[ZZRefCusCodeListSchema.Constants.ZZD_Code] = "TEST";
			row[ZZRefCusCodeListSchema.Constants.ZZD_Description] = "TEST";
			row[ZZRefCusCodeListSchema.Constants.ZZD_CountryOrGrouping] = Core.Constants.Customs.Universal.RefDataGrouping.Codes.UnitedNationsRecommendations;
			row[ZZRefCusCodeListSchema.Constants.ZZD_StartDate] = new DateTime(1900, 1, 1);
			row[ZZRefCusCodeListSchema.Constants.ZZD_EndDate] = new DateTime(2079, 6, 6);
			table.Rows.Add(row);
			Factory.Save();

			RefPacks.RP_Type = "";
			RefPacks.RP_CustomsCountry = Core.Constants.CountryCodes.UnitedKingdom;
			var list = RefPacks.RP_CustomsPack_List;
			Assert(list.ContainsCode("TEST"));
			RefPacks.RP_CustomsCountry = Core.Constants.CountryCodes.SouthAfrica;
			list = RefPacks.RP_CustomsPack_List;
			Assert(list.ContainsCode("TEST"));
		}

		public void TestCheckRP_CustomsPack()
		{
			RefPacks.RP_Type = "CIP";
			RefPacks.RP_CustomsCountry = "CN";
			Assert("NO Error", !RefPacks.RP_CustomsPackInfo.HasErrors());
			AssertEquals(false, RefPacks.RP_CustomsCountryReadOnly);

			RefPacks.RP_CustomsPack = "TT";
			Assert("Error Expected", RefPacks.RP_CustomsPackInfo.HasErrors());

			RefPacks.RP_CustomsPack = "KG";
			RefPacks.RP_CustomsCountry = "FR";
			RefPacks.RP_Type = "CIP";
			Assert("NO Error", !RefPacks.RP_CustomsPackInfo.HasErrors());

			RefPacks.RP_Type = "AMS";
			Assert("Error Expected when the pack unit is not in the list for changed type", RefPacks.RP_CustomsPackInfo.HasErrors());
			AssertEquals("US", RefPacks.RP_CustomsCountry);
			AssertEquals(true, RefPacks.RP_CustomsCountryReadOnly);

			RefPacks.RP_Type = "CIP";
			Assert("NO Error", !RefPacks.RP_CustomsPackInfo.HasErrors());

			RefPacks.RP_CustomsCountry = "CN";
			Assert("Error Expected when the pack unit is not in the list for changed country", RefPacks.RP_CustomsPackInfo.HasErrors());

			RefPacks.RP_CustomsPack = "";
			RefPacks.RP_Type = "AFR";
			AssertEquals("JP", RefPacks.RP_CustomsCountry);
			AssertEquals(true, RefPacks.RP_CustomsCountryReadOnly);
		}

		CusRefPacks RefPacks;
		protected override void SetUp()
		{
			base.SetUp();
			RefPacks = Factory.New<CusRefPacks>();
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			RefPacks = factory.New<CusRefPacks>();
			RefPacks.RP_Type = RPTypeList.Codes.CommercialInvoice;
			return RefPacks;
		}
	}
}
