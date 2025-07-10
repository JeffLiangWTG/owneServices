using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Universal.Testing;
using RefCusCodeList = Enterprise.Core.Constants.Customs.Universal.RefCusCodeList;
using RefCusCodeListTypes = Enterprise.Core.Constants.Customs.Universal.RefCusCodeListTypes;
using RefDataGrouping = Enterprise.Core.Constants.Customs.Universal.RefDataGrouping;

namespace Enterprise.Customs.ZA.Business.Testing
{
	sealed class AsycudaPackLookupsTest : TestCaseWithFactory
	{
		public void TestPackUQList()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(RefCusCodeListTypes.Codes.UnitedNationsPackageTypes, "A");
			helper.CreateNewOrGetExistingCusCodeList(RefDataGrouping.Codes.UnitedNationsRecommendations, RefCusCodeListTypes.Codes.UnitedNationsPackageTypes,
				"AAA", "AAAAA", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			var bg = helper.CreateNewOrGetExistingCusCodeList(RefDataGrouping.Codes.UnitedNationsRecommendations, RefCusCodeListTypes.Codes.UnitedNationsPackageTypes,
				"BG", "BG DESC", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeListAttribute(bg.PK, RefCusCodeList.Attributes.Bulk, RefCusCodeList.AttributeValues.Bulk);
			helper.CreateNewOrGetExistingDataGrouping(RefDataGrouping.Codes.UnitedNationsRecommendations);
			Factory.Save();
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			var pack = bill.Packs.AddNew();
			var packUQList = pack.Lookups.PackUQList;
			Assert("AAA", packUQList.ContainsCode("AAA"));
			Assert("BG", packUQList.ContainsCode("BG"));
			Assert("BAG should not appear as it is not in the list", !packUQList.ContainsCode("BAG"));
			header.AMA_ContainerMode = Core.Constants.ContainerModes.Bulk;
			packUQList = pack.Lookups.PackUQList;
			Assert("AAA", !packUQList.ContainsCode("AAA"));
			Assert("BG", packUQList.ContainsCode("BG"));
			header.AMA_ContainerMode = Core.Constants.ContainerModes.BreakBulk;
			packUQList = pack.Lookups.PackUQList;
			Assert("AAA", !packUQList.ContainsCode("AAA"));
			Assert("BG", packUQList.ContainsCode("BG"));
		}

		public void TestWeightUQList()
		{
			var bill = Factory.New<AsycudaBill>();
			var pack = bill.Packs.AddNew();
			AssertEquals("DT, G, HG, KG, KT, LB, LT, MC, MG, OT, OZ, T, TL, TN", pack.Lookups.WeightUQList.CodesAsString);
		}

		public void TestVolumeUQList()
		{
			var bill = Factory.New<AsycudaBill>();
			var pack = bill.Packs.AddNew();
			AssertEquals("CC, CF, CI, CY, D3, GA, GI, L, M3, ML, TE", pack.Lookups.VolumeUQList.CodesAsString);
		}

		public void TestEverything()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			var cont = header.Containers.AddNew();
			var pack = bill.Packs.AddNew();
			pack.ContainerPK = cont.PK;
			var lu = pack.Lookups;
			AssertCollectionContains(cont, lu.Containers);
		}
	}
}
