using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.TW.Messaging;

namespace Enterprise.Customs.TW.Manifest.Business.Testing
{
	sealed class N5101HCommodityTest : TestCaseWithFactory
	{
		public void TestCargoDescription()
		{
			var bill = Factory.NewWithValidTestData<AsycudaBill>();
			var pack = bill.Packs.AddNew();
			pack.PackedItem.API_GoodsDescription = "XX1";
			IN5101HCommodity commodity = new N5101HCommodity(bill);
			AssertEquals("XX1", commodity.CargoDescription);
		}

		public void TestClassifications()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			header.AMA_TransportMode = Core.Constants.TransportModes.Air;
			var bill = header.Bills.AddNew();
			var pack = bill.Packs.AddNew();
			pack.PackedItem.API_Tariff = "123-4567 89";
			var dangerousGood = pack.UNDGs.AddNew();
			dangerousGood.DI_DG_NKSubs = "2900A";
			IN5101HCommodity commodity = new N5101HCommodity(bill);
			var classifications = commodity.Classifications.ToList();
			CombineAssertions(() =>
			{
				AssertEquals("Count: 2", 2, classifications.Count);
				Assert("IdentificationTypeCode: HS, ID: 123456", classifications.Any(x => x.ID == "123456" && x.IdentificationTypeCode == "HS"));
				Assert("IdentificationTypeCode: ZZZ, ID: 2900", classifications.Any(x => x.ID == "2900" && x.IdentificationTypeCode == "ZZZ"));
			});

			header.AMA_TransportMode = Core.Constants.TransportModes.Sea;
			classifications = commodity.Classifications.ToList();
			CombineAssertions(() =>
			{
				AssertEquals("Count: 2", 2, classifications.Count);
				Assert("IdentificationTypeCode: HS, ID: 123456", classifications.Any(x => x.ID == "123456" && x.IdentificationTypeCode == "HS"));
				Assert("IdentificationTypeCode: SSO, ID: 2900", classifications.Any(x => x.ID == "2900" && x.IdentificationTypeCode == "SSO"));
			});
		}
	}
}
