using CargoWise.Types;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.US.Messaging.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.US.Business.Testing
{
	public class USHouseBillSynchroniserTest : SynchroniserTestCase
	{
		public void TestHouseBillNumberSync()
		{
			SetUpShipment();
			shipment.JS_HouseBill = "HWB123";
			declaration.ShipmentSynchroniser.Synchronise(new Customs.Business.SynchroniseEventArgs(Customs.Business.SynchroniseAction.Force));
			AssertEquals("HWB123", declaration.JE_HouseBill);

			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "USLAX";
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "USLAX";
			SetTransportMode(Core.Constants.TransportModes.Road);

			declaration.ShipmentSynchroniser.Synchronise(new Customs.Business.SynchroniseEventArgs(Customs.Business.SynchroniseAction.Force));
			AssertEquals("HWB123", declaration.JE_HouseBill);

			SetTransportMode(Core.Constants.TransportModes.Sea);
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.ShipmentSynchroniser.Synchronise(new Customs.Business.SynchroniseEventArgs(Customs.Business.SynchroniseAction.Force));
			AssertEquals("HWB123", declaration.JE_HouseBill);

			shipment.JS_HouseBill = "1005465230";
			AssertEquals("1005465230", declaration.JE_HouseBill);
			AssertEquals("1005465230", declaration.PrimaryHouseBill.CU_BillNum);

			shipment.JS_HouseBill = "465230";
			AssertEquals("465230", declaration.JE_HouseBill);
			AssertEquals("465230", declaration.PrimaryHouseBill.CU_BillNum);

			var org = Factory.New<OrgHeader>();
			org.OH_Code = "aaa";
			org.OH_FullName = "bbb";
			var cusCode = org.CustomsCodes.AddNew();
			cusCode.OK_CodeType = OrgCusCode.CodeTypes.CarrierCode;
			cusCode.OK_CustomsRegNo = "APLU";
			cusCode.OK_RN_NKCodeCountry = "US";
			shipment.HouseBillIssuingPartyDocumentaryAddress.OrganisationPK = org.PK;
			declaration.ShipmentSynchroniser.Synchronise(new Customs.Business.SynchroniseEventArgs(Customs.Business.SynchroniseAction.Force));
			AssertEquals("APLU", declaration.PrimaryHouseBill.US_UI_NKBillIssuerSCAC);
			Assert(declaration.PrimaryHouseBill.US_UI_NKBillIssuerSCACInfo.ReadOnly);

			SetTransportMode(Core.Constants.TransportModes.Rail);
			var carrier = Factory.New<USCarrierCombined>();
			carrier.UI_Code = "APLU";
			carrier.UI_ModeOfTransportation = TransportModeCodes.Codes.RailContainer;
			shipment.JS_HouseBill = "TYUI600890";
			AssertEquals("TYUI600890", declaration.JE_HouseBill);
			AssertEquals("TYUI600890", declaration.PrimaryHouseBill.CU_BillNum);
			AssertEquals("APLU", declaration.PrimaryHouseBill.US_UI_NKBillIssuerSCAC);
			Assert(declaration.PrimaryHouseBill.US_UI_NKBillIssuerSCACInfo.ReadOnly);

			shipment.JS_HouseBill = "56230124";
			AssertEquals("56230124", declaration.JE_HouseBill);
			AssertEquals("56230124", declaration.PrimaryHouseBill.CU_BillNum);
			AssertEquals("APLU", declaration.PrimaryHouseBill.US_UI_NKBillIssuerSCAC);
			Assert(declaration.PrimaryHouseBill.US_UI_NKBillIssuerSCACInfo.ReadOnly);

			SetTransportMode(Core.Constants.TransportModes.Air);
			shipment.JS_HouseBill = "IY465230";
			AssertEquals("IY465230", declaration.JE_HouseBill);
			AssertEquals("IY465230", declaration.PrimaryHouseBill.CU_BillNum);
			AssertEquals("", declaration.PrimaryHouseBill.US_UI_NKBillIssuerSCAC);
			Assert(declaration.PrimaryHouseBill.US_UI_NKBillIssuerSCACInfo.ReadOnly);

			SetTransportMode(Core.Constants.TransportModes.Rail);
			declaration.ShipmentSynchroniser.Synchronise(new Customs.Business.SynchroniseEventArgs(Customs.Business.SynchroniseAction.Force));
			AssertEquals("IY465230", declaration.JE_HouseBill);
		}

		public void TestHouseBillIssuerSCACSync()
		{
			var carrier = Factory.New<USCarrierCombined>();
			carrier.UI_Code = "APLU";
			carrier.UI_ModeOfTransportation = TransportModeCodes.Codes.RailContainer;

			SetUpShipment();
			shipment.JS_HouseBill = "HWB123";
			AssertEquals("", declaration.JE_HouseBillIssuerSCAC);

			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "USLAX";
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "USLAX";
			SetTransportMode(Core.Constants.TransportModes.Road);

			declaration.ShipmentSynchroniser.Synchronise(new Customs.Business.SynchroniseEventArgs(Customs.Business.SynchroniseAction.Force));
			AssertEquals("", declaration.JE_HouseBillIssuerSCAC);

			SetTransportMode(Core.Constants.TransportModes.Sea);

			var org1 = Factory.New<OrgHeader>();
			org1.OH_Code = "aaa";
			org1.OH_FullName = "org1";
			var cusCode = org1.CustomsCodes.AddNew();
			cusCode.OK_CodeType = OrgCusCode.CodeTypes.CarrierCode;
			cusCode.OK_CustomsRegNo = "APLU";
			cusCode.OK_RN_NKCodeCountry = "US";
			shipment.HouseBillIssuingPartyDocumentaryAddress.OrganisationPK = org1.PK;

			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.ShipmentSynchroniser.Synchronise(new Customs.Business.SynchroniseEventArgs(Customs.Business.SynchroniseAction.Force));
			AssertEquals("APLU", declaration.JE_HouseBillIssuerSCAC);

			SetTransportMode(Core.Constants.TransportModes.Air);
			shipment.JS_HouseBill = "IY465230";
			AssertEquals("", declaration.JE_HouseBillIssuerSCAC);

			SetTransportMode(Core.Constants.TransportModes.Rail);
			consol.JK_MasterBillNum = "TREW5002003";
			declaration.ShipmentSynchroniser.Synchronise(new Customs.Business.SynchroniseEventArgs(Customs.Business.SynchroniseAction.Force));
			AssertEquals("TREW5002003", declaration.JE_MasterBill);
			AssertEquals("", declaration.JE_MasterBillIssuerSCAC);

			SetTransportMode(Core.Constants.TransportModes.Rail);
			declaration.ShipmentSynchroniser.Synchronise(new Customs.Business.SynchroniseEventArgs(Customs.Business.SynchroniseAction.Force));
			AssertEquals("APLU", declaration.JE_HouseBillIssuerSCAC);
		}

		JobDeclaration declaration;
		ForwardingConsol consol;
		ForwardingShipment shipment;

		void SetUpShipment()
		{
			declaration = Factory.New<JobDeclaration>();
			consol = Factory.New<ForwardingConsol>();
			consol.JK_RL_NKLoadPort = "USLAX";
			consol.JK_RL_NKDischargePort = "AUSYD";
			shipment = consol.Shipments.AddNew();
			declaration.JE_JS = shipment.PK;
		}

		void SetTransportMode(ZString transportMode)
		{
			consol.JK_TransportMode = transportMode;
			shipment.JS_TransportMode = transportMode;
			declaration.JE_TransportMode = transportMode;
		}
	}
}
