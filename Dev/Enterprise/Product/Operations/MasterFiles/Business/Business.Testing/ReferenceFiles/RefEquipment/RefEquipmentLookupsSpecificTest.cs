using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class RefEquipmentLookupsSpecificTest : TestCaseWithFactory
	{
		public void TestRQ_F3_NKPackType_List_ShouldHaveContainerType()
		{
			var equipemnt = Factory.NewWithValidTestData<RefEquipment>();
			AssertEquals(true, equipemnt.Lookups.RQ_F3_NKPackType_List.ContainsCode(RefPackTypeCollection.ReservedContainerType));
		}

		public void TestListCatched()
		{
			var equipment = Factory.New<RefEquipment>();
			equipment.RQ_ShortCode = "ZZZ";

			var fakeEquipmentGroupList = new CodeDescriptionPairList();
			fakeEquipmentGroupList.AddPair("AAA", "A A A");
			Env.Registry.ReferenceFiles.EquipmentGroup = fakeEquipmentGroupList;
			AssertEquals("RQ_EquipmentGroup_List", true, equipment.Lookups.RQ_EquipmentGroup_List.ContainsCode("AAA"));

			var refPackType = Factory.New<RefPackType>();
			refPackType.F3_Code = "KNZ";
			refPackType.F3_Description = "This is a Test";
			Factory.Save();
			AssertEquals("RQ_F3_NKPackType_List", true, equipment.Lookups.RQ_F3_NKPackType_List.ContainsCode("KNZ"));
			AssertEquals("GPSProviders", true, equipment.Lookups.GPSProviders.ContainsCode("WTG"));
			AssertNotNull("RQ_OH_OwnerList", equipment.Lookups.RQ_OH_OwnerList);
		}

		public void TestGPSProviders()
		{
			var providers = new GPSProviderList();
			AssertEquals("Should have 1 providers", 1, providers.Count);

			AssertEquals("Should have provider with this Code", true, providers.ContainsCode("WTG"));
			AssertEquals("Should have returned 'WiseTech Global Telematics'", "WiseTech Global Telematics", providers.GetDescriptionFromCode("WTG"));
		}

		public void TestRefCountryStatesList()
		{
			var bO = Factory.NewWithValidTestData<RefEquipment>();
			var organisation = Factory.NewWithValidTestData<OrgHeader>();
			organisation.OH_Code = "KNZTest";
			organisation.OH_RL_NKClosestPort = "AUSYD";

			var organisation2 = Factory.NewWithValidTestData<OrgHeader>();
			organisation2.OH_Code = "ABCTest";
			organisation2.OH_RL_NKClosestPort = "CHSAH";

			var aUStates = Factory.New<RefCountryStates>();
			aUStates.RW_Code = "AU1";
			var refCountry1 = Factory.Load<RefCountry>(new ZQuery(RefCountrySchema.RN_Code, "AU")).FirstOrDefault();
			aUStates.RW_RN_NKCountryCode = refCountry1 == null ? ZString.Empty : refCountry1.RN_Code;

			var cNStates = Factory.New<RefCountryStates>();
			cNStates.RW_Code = "CN1";
			var refCountry2 = Factory.Load<RefCountry>(new ZQuery(RefCountrySchema.RN_Code, "CN")).FirstOrDefault();
			cNStates.RW_RN_NKCountryCode = refCountry2 == null ? ZString.Empty : refCountry2.RN_Code;
			Factory.Save();

			bO.RQ_RN_NKRegistrationCountry = "AU";
			AssertNotNull(bO.Lookups.RefCountryStatesList);
			AssertEquals(bO.Lookups.RefCountryStatesList.Cast<RefCountryStates>().Any(x => x.RW_Code == "AU1"), true);

			bO.RQ_RN_NKRegistrationCountry = "CN";
			AssertNotNull(bO.Lookups.RefCountryStatesList);
			AssertEquals(bO.Lookups.RefCountryStatesList.Cast<RefCountryStates>().Any(x => x.RW_Code == "AU1"), false);
			AssertEquals(bO.Lookups.RefCountryStatesList.Cast<RefCountryStates>().Any(x => x.RW_Code == "CN1"), true);
		}

		public void TestEquipmentGroup()
		{
			var fakeEquipmentGroupList = new CodeDescriptionPairList();
			var equipment = Factory.New<RefEquipment>();
			fakeEquipmentGroupList.AddPair("AAA", "A A A");
			fakeEquipmentGroupList.AddPair("BBB", "B B B");
			fakeEquipmentGroupList.AddPair("CCC", "C C C");
			Env.Registry.ReferenceFiles.EquipmentGroup = fakeEquipmentGroupList;
			AssertEquals(3, equipment.Lookups.RQ_EquipmentGroup_List.Count);
			AssertEquals(true, equipment.Lookups.RQ_EquipmentGroup_List.ContainsCode("AAA"));
			AssertEquals(true, equipment.Lookups.RQ_EquipmentGroup_List.ContainsCode("BBB"));
			AssertEquals(true, equipment.Lookups.RQ_EquipmentGroup_List.ContainsCode("CCC"));
			AssertEquals(false, equipment.Lookups.RQ_EquipmentGroup_List.ContainsCode("DDD"));
		}
	}
}
