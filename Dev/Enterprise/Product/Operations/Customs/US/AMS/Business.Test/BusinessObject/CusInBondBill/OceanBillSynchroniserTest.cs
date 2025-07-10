using CargoWise.Types;
using Enterprise.Customs.US.AMS.Messaging.Business;
using Enterprise.Customs.US.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.US.AMS.Business.Testing
{
	class OceanBillSynchroniserTest : AMSSynchroniserTestCase
	{
		public void TestSynchroniseB0_IssuerCode()
		{
			var carrier = Factory.New<USCarrierCombined>();
			carrier.UI_Code = "ORG1";
			carrier.UI_ModeOfTransportation = "11";
			var carrier2 = Factory.New<USCarrierCombined>();
			carrier2.UI_Code = "OTT1";
			carrier2.UI_ModeOfTransportation = "11";
			consol.MasterBillIssuingPartyDocumentaryAddress.OrganisationPK = org1.PK;
			synchroniser.Synchronise(true);
			AssertEquals(org1CarrierCode.OK_CustomsRegNo, bill.B0_IssuerCode);
			consol.MasterBillIssuingPartyDocumentaryAddress.OrganisationPK = ZGuid.Empty;
			synchroniser.Synchronise(true);
			AssertEquals("OTT1", bill.B0_IssuerCode);
			consol.JK_MasterBillNum = "OTT1123";
			synchroniser.Synchronise(true);
			AssertEquals("OTT1", bill.B0_IssuerCode);
			AssertEquals(true, bill.B0_IssuerCodeInfo.ReadOnly);
		}

		public void TestSynchroniseB0_MasterBillNumber()
		{
			consol.JK_MasterBillNum = "AOC123";
			AssertEquals("AOC123", bill.B0_MasterBillNumber);
			consol.JK_MasterBillNum = "";
			AssertEquals("", bill.B0_MasterBillNumber);
			consol.JK_MasterBillNum = "ABC12@34-56";
			AssertEquals("ABC123456", bill.B0_MasterBillNumber);
			consol.JK_MasterBillNum = "OTT134-56";
			synchroniser.Synchronise(true);
			AssertEquals("3456", bill.B0_MasterBillNumber);
			AssertEquals(true, bill.B0_MasterBillNumberInfo.ReadOnly);
		}

		public void TestSynchroniseB0_BillStatus()
		{
			AssertEquals(BillOfLadingStatusIndicatorList.Codes.MasterBill, bill.B0_BillStatus);
			AssertEquals(true, bill.B0_BillStatusInfo.ReadOnly);
		}

		CusInBondBill bill;
		OceanBillSynchroniser synchroniser;
		protected override void SetUp()
		{
			base.SetUp();
			header.BH_TransitDirection = DirectionTypeList.Codes.NVOCC;
			bill = header.OceanBill;
			var carrier = Factory.New<USCarrierCombined>();
			carrier.UI_Code = "SCAC";
			carrier.UI_ModeOfTransportation = "10";
			var shippingLine = Factory.NewWithValidTestData<OrgHeader>();
			shippingLine.OH_IsCreditor = true;
			var carrierAddress = Factory.NewWithValidTestData<OrgAddress>();
			carrierAddress.OA_Code = "CarrierAdr";
			carrierAddress.OA_OH = shippingLine.PK;
			var shippingLineCarrierCode = shippingLine.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.CodeTypes.CarrierCode, "OTT1", Core.Constants.CountryCodes.UnitedStates);
			consol.JK_OA_ShippingLineAddress = carrierAddress.PK;
			synchroniser = new OceanBillSynchroniser(bill, consol);
			synchroniser.Synchronise(true);
		}
	}
}
