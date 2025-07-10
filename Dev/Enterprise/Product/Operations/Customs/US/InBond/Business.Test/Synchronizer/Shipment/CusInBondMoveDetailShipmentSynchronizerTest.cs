using CargoWise.Types;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.US.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.US.InBond.Business.Testing
{
	sealed class CusInBondMoveDetailShipmentSynchronizerTest : SynchroniserTestCase
	{
		public void TestSynchronize()
		{
			var org = Factory.New<OrgHeader>();
			org.OH_Code = "aaa";
			org.OH_FullName = "bbb";
			var cusCode = org.CustomsCodes.AddNew();
			cusCode.OK_CodeType = OrgCusCode.CodeTypes.CarrierCode;
			cusCode.OK_CustomsRegNo = "MAST";
			cusCode.OK_RN_NKCodeCountry = "US";
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "USLAX";
			consol.JK_MasterBillNum = "MASTERCONSOL";
			consol.MasterBillIssuingPartyDocumentaryAddress.OrganisationPK = org.PK;
			consol.JK_TransportMode = "SEA";
			var shipment = consol.Shipments.AddNew();
			var importer = Factory.NewWithValidTestData<OrgHeader>();
			importer.OH_Code = "IMP001";
			importer.OH_FullName = "Importer One Co. Ltd";
			shipment.ConsigneeDocumentaryAddress.OrganisationPK = importer.PK;
			shipment.JS_TransportMode = "SEA";
			shipment.JS_PackingMode = "FCL";
			shipment.JS_RL_NKOrigin = "AUMEL";
			shipment.JS_E_DEP = new ZDateTime(2011, 04, 03);
			shipment.JS_E_ARV = new ZDateTime(2011, 04, 15);
			Factory.Save();
			var carrier0 = Factory.New<USCarrierCombined>();
			carrier0.UI_Code = "MAST";
			carrier0.UI_ModeOfTransportation = US.Messaging.Business.TransportModeCodes.Codes.VesselContainer;
			var inBondHeader = Factory.New<CusInBondHeader>();
			inBondHeader.BH_ParentID = shipment.PK;
			inBondHeader.BH_ParentTableCode = shipment.TablePrefix;
			inBondHeader.Synchroniser.Synchronise(new Customs.Business.SynchroniseEventArgs(Customs.Business.SynchroniseAction.Force));
			AssertEquals(1, inBondHeader.MovementHeaders.Count);
			AssertEquals(1, inBondHeader.Bills.Count);
			var bill = inBondHeader.Bills[0];
			AssertEquals(1, bill.MoveDetails.Count);
			var moveDetail = bill.MoveDetails[0];
			IInBondBillDetails billDetails = moveDetail;
			AssertEquals("Issuer Code should be synchronized", "MAST", billDetails.MasterBillIssuerSCAC);
			AssertEquals("Bill Number should be synchronized", "ERCONSOL", billDetails.MasterBillNumber);
		}
	}
}
