using CargoWise.Types;
using Enterprise.Customs.US.AMS.Messaging.Business;
using Enterprise.Customs.US.Business;

namespace Enterprise.Customs.US.AMS.Business.Testing
{
	class CusInbondBillAddRefSynchroniserTest : AMSSynchroniserTestCase
	{
		public void TestSynchroniseBR_Qualifier()
		{
			AssertEquals(BillReferenceList.Codes.OB, billRef.BR_Qualifier);
		}

		public void TestSynchroniseBR_ReferenceNum()
		{
			var carrier = Factory.New<USCarrierCombined>();
			carrier.UI_Code = "ORG1";
			carrier.UI_ModeOfTransportation = "10";
			var carrier2 = Factory.New<USCarrierCombined>();
			carrier2.UI_Code = "ORG2";
			carrier2.UI_ModeOfTransportation = "10";
			consol.JK_MasterBillNum = "ABCD1235";
			AssertEquals("ABCD1235", billRef.BR_ReferenceNum);
			consol.MasterBillIssuingPartyDocumentaryAddress.OrganisationPK = org1.PK;
			AssertEquals(org1CarrierCode.OK_CustomsRegNo + "ABCD1235", billRef.BR_ReferenceNum);
			consol.JK_MasterBillNum = org1CarrierCode.OK_CustomsRegNo + "1235";
			AssertEquals(org1CarrierCode.OK_CustomsRegNo + "1235", billRef.BR_ReferenceNum);
			consol.MasterBillIssuingPartyDocumentaryAddress.OrganisationPK = org2.PK;
			AssertEquals(org2CarrierCode.OK_CustomsRegNo + org1CarrierCode.OK_CustomsRegNo + "1235", billRef.BR_ReferenceNum);
			consol.JK_MasterBillNum = "";
			AssertEquals("", billRef.BR_ReferenceNum);
			consol.JK_MasterBillNum = "ABCD1235";
			AssertEquals(org2CarrierCode.OK_CustomsRegNo + "ABCD1235", billRef.BR_ReferenceNum);
			consol.MasterBillIssuingPartyDocumentaryAddress.OrganisationPK = ZGuid.Empty;
			AssertEquals("ABCD1235", billRef.BR_ReferenceNum);
			synchroniser.SetEnabled(false, false);
			consol.JK_MasterBillNum = "ABCD4567";
			AssertEquals("ABCD1235", billRef.BR_ReferenceNum);
			synchroniser = new CusInbondBillAddRefSynchroniser(billRef, consol);
			synchroniser.Synchronise(true);
			AssertEquals("ABCD4567", billRef.BR_ReferenceNum);
			consol.MasterBillIssuingPartyDocumentaryAddress.OrganisationPK = org1.PK;
			AssertEquals(org1CarrierCode.OK_CustomsRegNo + "ABCD4567", billRef.BR_ReferenceNum);
			consol.JK_MasterBillNum = org1CarrierCode.OK_CustomsRegNo + "1235";
			AssertEquals(org1CarrierCode.OK_CustomsRegNo + "1235", billRef.BR_ReferenceNum);
		}

		CusInBondBill bill;
		CusInbondBillAddRef billRef;
		CusInbondBillAddRefSynchroniser synchroniser;
		protected override void SetUp()
		{
			base.SetUp();
			bill = header.Bills.AddNew();
			billRef = bill.ShipmentReferenceDetails.AddNew();
			synchroniser = new CusInbondBillAddRefSynchroniser(billRef, consol);
			synchroniser.Synchronise(true);
		}
	}
}
