using CargoWise.EntityFramework.Testing;
using Enterprise.Core;

namespace Enterprise.OceanCarrier.Business.Testing
{
	sealed class CarrierShipmentCargoLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestSealParty_List()
		{
			var cargo = Factory.NewWithValidTestData<CarrierShipmentCargo>();

			CombineAssertions("SealParty_List ", () =>
			{
				var sealPartyList = cargo.Lookups.SealParty_List;
				AssertEquals(5, sealPartyList.Count);
				Assert($"{Constants.ContainerSealParties.Descriptions.CarrierShippingLine} ({Constants.ContainerSealParties.Codes.CarrierShippingLine}) is not found.", sealPartyList.ContainsCode(Constants.ContainerSealParties.Codes.CarrierShippingLine));
				Assert($"{Constants.ContainerSealParties.Descriptions.ConsignorShipper} ({Constants.ContainerSealParties.Codes.ConsignorShipper}) is not found.", sealPartyList.ContainsCode(Constants.ContainerSealParties.Codes.ConsignorShipper));
				Assert($"{Constants.ContainerSealParties.Descriptions.Customs} ({Constants.ContainerSealParties.Codes.Customs}) is not found.", sealPartyList.ContainsCode(Constants.ContainerSealParties.Codes.Customs));
				Assert($"{Constants.ContainerSealParties.Descriptions.Quarantine} ({Constants.ContainerSealParties.Codes.Quarantine}) is not found.", sealPartyList.ContainsCode(Constants.ContainerSealParties.Codes.Quarantine));
				Assert($"{Constants.ContainerSealParties.Descriptions.Terminal} ({Constants.ContainerSealParties.Codes.Terminal}) is not found.", sealPartyList.ContainsCode(Constants.ContainerSealParties.Codes.Terminal));
			});
		}

		public void TestGrossWeightVerificationTypeList()
		{
			var cargo = Factory.NewWithValidTestData<CarrierShipmentCargo>();

			CombineAssertions("GrossWeightVerificationTypeList ", () =>
			{
				var grossWeightVerificationTypeList = cargo.Lookups.GrossWeightVerificationTypeList;
				AssertEquals(6, grossWeightVerificationTypeList.Count);
				Assert($"{Constants.ContainerGrossWeightVerificationTypes.Descriptions.NotVerified} ({Constants.ContainerGrossWeightVerificationTypes.Codes.NotVerified}) is not found.", grossWeightVerificationTypeList.ContainsCode(Constants.ContainerGrossWeightVerificationTypes.Codes.NotVerified));
				Assert($"{Constants.ContainerGrossWeightVerificationTypes.Descriptions.NotRequired} ({Constants.ContainerGrossWeightVerificationTypes.Codes.NotRequired}) is not found.", grossWeightVerificationTypeList.ContainsCode(Constants.ContainerGrossWeightVerificationTypes.Codes.NotRequired));
				Assert($"{Constants.ContainerGrossWeightVerificationTypes.Descriptions.Method1Container} ({Constants.ContainerGrossWeightVerificationTypes.Codes.Method1Container}) is not found.", grossWeightVerificationTypeList.ContainsCode(Constants.ContainerGrossWeightVerificationTypes.Codes.Method1Container));
				Assert($"{Constants.ContainerGrossWeightVerificationTypes.Descriptions.Method2Packages} ({Constants.ContainerGrossWeightVerificationTypes.Codes.Method2Packages}) is not found.", grossWeightVerificationTypeList.ContainsCode(Constants.ContainerGrossWeightVerificationTypes.Codes.Method2Packages));
				Assert($"{Constants.ContainerGrossWeightVerificationTypes.Descriptions.Method2Packages} ({Constants.ContainerGrossWeightVerificationTypes.Codes.Method2Packages}) is not found.", grossWeightVerificationTypeList.ContainsCode(Constants.ContainerGrossWeightVerificationTypes.Codes.Method2Packages));
				Assert($"{Constants.ContainerGrossWeightVerificationTypes.Descriptions.RationalMethod} ({Constants.ContainerGrossWeightVerificationTypes.Codes.RationalMethod}) is not found.", grossWeightVerificationTypeList.ContainsCode(Constants.ContainerGrossWeightVerificationTypes.Codes.RationalMethod));
			});
		}
	}
}
