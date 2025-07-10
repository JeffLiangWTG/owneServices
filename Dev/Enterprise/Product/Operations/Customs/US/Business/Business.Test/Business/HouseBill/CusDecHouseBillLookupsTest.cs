using Enterprise.Customs.Common.US;
using Enterprise.Customs.US.Messaging.Business;
using Enterprise.MasterFiles.Business;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.Business.Testing
{
	sealed class CusDecHouseBillLookupsTest : Customs.Business.Testing.CusDecHouseBillLookupsTest
	{
		public void TestHouseBill()
		{
			var parent = Factory.New<Bill>();
			AssertEquals(parent.Lookups.HouseBill, parent);
		}

		public void TestList()
		{
			var houseBill = Factory.New<Bill>();
			AssertEquals("WeightUQList", OLookUpEditType.Weight, houseBill.Lookups.WeightUQList.LookupEditType);
			AssertEquals("VolumeUQList", OLookUpEditType.Volume, houseBill.Lookups.VolumeUQList.LookupEditType);
			AssertEquals("UnitOfMeasureList", typeof(ShippingOrPackingingUnitList), houseBill.Lookups.NoOfPacksPackType_List.GetType());
			AssertEquals("HouseIssuers", typeof(BillIssuerOrganisationFindBoxCollection), houseBill.Lookups.HouseIssuers.GetType());
			AssertEquals("MasterIssuers", typeof(BillIssuerOrganisationFindBoxCollection), houseBill.Lookups.MasterIssuers.GetType());
			AssertEquals("ForeignShippers", typeof(BillIssuerOrganisationFindBoxCollection), houseBill.Lookups.ForeignShippers.GetType());
			AssertEquals("NotifyParies", typeof(OrganisationsFindBoxCollection), houseBill.Lookups.NotifyParties.GetType());
			AssertEquals("Carriers", typeof(USCarrierCombinedCollection), houseBill.Lookups.USCarrierList.GetType());
			AssertEquals("MessageStatusList", Factory.GetCachedValue<ImportMessageStatusList>(), houseBill.Lookups.MessageStatusList);
		}

		public void TestBillTypeList()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.FTZ;
			var houseBill = declaration.Bills.AddNew();
			Assert("Should not contain 'SubHouse Bill' type", !houseBill.Lookups.CU_BillTypeList.ContainsCode(Customs.Business.BillTypeList.Codes.SubHouseBill));
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			Assert("List should contains 'SubHouse Bill' type", houseBill.Lookups.CU_BillTypeList.ContainsCode(Customs.Business.BillTypeList.Codes.SubHouseBill));
		}

		public void TestYesNoList()
		{
			var houseBill = Factory.New<Bill>();
			AssertEquals(typeof(YesNoDefaultList), houseBill.Lookups.YesNoList.GetType());
			Assert("Should contain No", houseBill.Lookups.YesNoList.ContainsCode(YesNoDefaultList.Codes.No));
			Assert("Should contain Yes", houseBill.Lookups.YesNoList.ContainsCode(YesNoDefaultList.Codes.Yes));
			Assert("Should not contain Default", !houseBill.Lookups.YesNoList.ContainsCode(YesNoDefaultList.Codes.Default));
		}
	}
}
