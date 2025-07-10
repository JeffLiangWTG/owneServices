using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Universal.Testing;

namespace Enterprise.Customs.ZA.Business.Testing
{
	sealed class AsycudaBillLookupsTest : TestCaseWithFactory
	{
		public void TestParent()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			AssertType<AsycudaBill>(header.MasterBill.Lookups.Parent);
		}

		public void TestBillIssuers()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var carrierCode = helper.CreateCarrierCode("123", "desc.", Core.Constants.CountryCodes.SouthAfrica);
			helper.CreateCarrierCodeAttribute(carrierCode.PK, Core.Constants.Customs.Universal.RefCarrierAttributeNames.CARGOCARRIER, "CARGOCARRIER");

			Factory.Save();

			var header = Factory.New<AsycudaManifestHeader>();
			var masterBill = header.MasterBill;
			var billIssuers = masterBill.Lookups.BillIssuers;
			Assert(billIssuers.Any(x => x.ZZ4_Code == "123"));
		}

		public void TestCustomsStatusList()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsManifestStatus, "Customs Manifest Status");
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.SouthAfrica, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsManifestStatus, "6", "Rejected", ZDateTime.Today.AddDays(-1), ZDateTime.Today.AddDays(1));
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.SouthAfrica, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsManifestStatus, "8", "Proceed to Border", ZDateTime.Today.AddDays(-1), ZDateTime.Today.AddDays(1));

			Factory.Save();

			var header = Factory.New<AsycudaManifestHeader>();
			var statuses = header.MasterBill.Lookups.CustomsStatusList;
			Assert(statuses.ContainsCode("6"));
			Assert(statuses.ContainsCode("8"));
		}
	}
}
