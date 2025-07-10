using System.Linq;
using Enterprise.UniversalDataBuss.Management.Testing;

namespace Enterprise.Customs.SG.Access.Business.UniversalDataTransfer.Testing
{
	sealed class SGAsycudaManifestHeaderDataObjectWriterHelperTest : TestCaseWithFactoryAndMessagingHelpers
	{
		public void TestGetBillCountryAdditionalAddInfos()
		{
			var header = Factory.New<AsycudaManifestHeader>();

			var bill = header.Bills.AddNew();
			bill.SG_PayeeIndicator = "Y";
			bill.SG_PartyStatus = "A";
			bill.SG_PartyID = "PAR005520";
			bill.GSTNReferenceNo = "NR032234";

			var helper = new SGAsycudaManifestHeaderDataObjectWriterHelper(header);
			var infos = helper.GetBillCountryAdditionalAddInfos(bill).Select(c => $"{c.Key}|{c.Value}").ToArray();
			var expectedInfos = new[]
			{
				"PayeeIndicator|Y",
				"PartyStatus|A",
				"PartyIndicator|PAR005520",
				"GSTNReferenceNo|NR032234",
			};

			AssertArrayEqualsByElements(expectedInfos, infos);
		}

		public void TestGetPackedItemAdditionalAddInfos()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			var pack = bill.Packs.AddNew();

			var packItem = (AsycudaPackedItem)pack.PackedItems.AddNewPackedItem();
			packItem.GoodsType = "TST";
			packItem.GSTPaid = "N";

			var helper = new SGAsycudaManifestHeaderDataObjectWriterHelper(header);
			var infos = helper.GetPackedItemAdditionalAddInfos(packItem).Select(c => $"{c.Key}|{c.Value}").ToArray();
			var expectedInfos = new[]
			{
				"GoodsType|TST",
				"GSTPaymentIndicator|N",
			};

			AssertArrayEqualsByElements(expectedInfos, infos);
		}
	}
}
