using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common.US.AMS;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.US.AMS.Business.Testing
{
	class AMSStatusHelperTest : TestCaseWithFactory
	{
		public void TestGetAMSBillStatus()
		{
			Integration.Customs.US.USAMS.IAMSStatusHelper helper = new AMSStatusHelper();
			AssertEquals(ZString.Empty, helper.GetAMSBillStatus(null));
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = Core.Constants.TransportModes.Sea;
			consol.JK_RL_NKDischargePort = "USLAX";
			consol.JK_AgentType = Core.Constants.AgentType.Agent;
			consol.JK_OA_SendingForwarderAddress = org.MainAddress.PK;
			var shipment1 = consol.Shipments.AddNew();
			shipment1.JS_HouseBill = "OTTAHB123456789012";
			AssertEquals("There is no AMS done for this consol", AMSConsolBillCustomsStatusList.Codes.OutOfSync, helper.GetAMSBillStatus(consol));
			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			var consolInOtherFactory = newFactory.Load<ForwardingConsol>(consol.PK);
			var header = newFactory.New<CusInBondHeader>();
			header.BH_OverrideFreightDefaults = true; // stop synchronisation
			header.BH_ParentID = consol.PK;
			header.BH_ParentTableCode = consol.TablePrefix;
			var bill1 = header.Bills.AddNew();
			bill1.B0_IssuerCode = "OTTA";
			bill1.B0_MasterBillNumber = "OTTAHB123456789012";
			AssertEquals("Should not match as local cache doesn't have AMS", AMSConsolBillCustomsStatusList.Codes.OutOfSync, helper.GetAMSBillStatus(consol));
			AssertEquals("Should match to local cache", AMSConsolBillCustomsStatusList.Codes.NotOnFile, helper.GetAMSBillStatus(consolInOtherFactory));
			consolInOtherFactory.JK_TransportMode = Core.Constants.TransportModes.Air;
			AssertEquals("AMS currently only support Sea", ZString.Empty, helper.GetAMSBillStatus(consolInOtherFactory));

			consolInOtherFactory.JK_TransportMode = Core.Constants.TransportModes.Sea;
			consolInOtherFactory.JK_OA_SendingForwarderAddress = org.MainAddress.PK;
			AssertEquals(AMSConsolBillCustomsStatusList.Codes.NotOnFile, helper.GetAMSBillStatus(consolInOtherFactory));
			consolInOtherFactory.JK_RL_NKDischargePort = "AUSYD";
			AssertEquals("Consol is not AMS consol as it doesn't go via/to US", ZString.Empty, helper.GetAMSBillStatus(consolInOtherFactory));

			consolInOtherFactory.JK_RL_NKDischargePort = "USLAX";
			AssertEquals(AMSConsolBillCustomsStatusList.Codes.NotOnFile, helper.GetAMSBillStatus(consolInOtherFactory));

			var bill2 = header.Bills.AddNew();
			bill2.B0_IssuerCode = "OTTA";
			bill2.B0_MasterBillNumber = "OTTAHB234567890123";
			AssertEquals("Number of AMS Bill Of Ladings do not match Consol shipments", AMSConsolBillCustomsStatusList.Codes.OutOfSync, helper.GetAMSBillStatus(consolInOtherFactory));

			consolInOtherFactory.JK_TransportMode = Core.Constants.TransportModes.Rail;
			AssertEquals("Works for Rail", AMSConsolBillCustomsStatusList.Codes.OutOfSync, helper.GetAMSBillStatus(consolInOtherFactory));
			consolInOtherFactory.JK_RL_NKDischargePort = "PRSJU";
			AssertEquals("Works for PuertoRico", AMSConsolBillCustomsStatusList.Codes.OutOfSync, helper.GetAMSBillStatus(consolInOtherFactory));

			consolInOtherFactory.JK_OA_SendingForwarderAddress = org.MainAddress.PK;
			var shipment2InOtherFactory = consolInOtherFactory.Shipments.AddNew();
			shipment2InOtherFactory.JS_HouseBill = "OTTAHB234567890123";
			AssertEquals("All AMS Bill of Ladings are not on file", AMSConsolBillCustomsStatusList.Codes.NotOnFile, helper.GetAMSBillStatus(consolInOtherFactory));

			bill2.MovementDetail.B9_CustomsStatus = AMSBillCustomsStatusList.Codes.OnFile;
			AssertEquals("1 AMS Bill of Lading is not on file while the other is", AMSConsolBillCustomsStatusList.Codes.Multiple, helper.GetAMSBillStatus(consolInOtherFactory));

			bill1.MovementDetail.B9_CustomsStatus = AMSBillCustomsStatusList.Codes.OnFile;
			AssertEquals("All AMS Bill of Ladings are on file", AMSConsolBillCustomsStatusList.Codes.OnFile, helper.GetAMSBillStatus(consolInOtherFactory));

			var shipment1InOtherFactory = newFactory.Load<ForwardingShipment>(shipment1.PK);
			shipment1InOtherFactory.JS_HouseBill = "HB123456789012";
			shipment2InOtherFactory.JS_HouseBill = "HB234567890123";
			AssertEquals("Consol Shipment Bill Issuer Codes do not match AMS Bill Of Lading Issuer Codes", AMSConsolBillCustomsStatusList.Codes.OutOfSync, helper.GetAMSBillStatus(consolInOtherFactory));

			consolInOtherFactory.JK_AgentType = Core.Constants.AgentType.CoLoad;
			consolInOtherFactory.JK_OA_SendingForwarderAddress = org.MainAddress.PK;
			bill1.B0_IssuerCode = "OTTA";
			bill1.B0_MasterBillNumber = "HB123456789012";
			bill2.B0_IssuerCode = "OTTA";
			bill2.B0_MasterBillNumber = "HB234567890123";
			AssertEquals("All AMS Bill of Ladings are on file and should match Consol Shipments based on Sending Agent details", AMSConsolBillCustomsStatusList.Codes.OnFile, helper.GetAMSBillStatus(consolInOtherFactory));

			var bill3 = header.Bills.AddNew();
			bill3.B0_IssuerCode = "OTTA";
			bill3.B0_MasterBillNumber = "HB3";
			bill3.MovementDetail.B9_CustomsStatus = AMSBillCustomsStatusList.Codes.OnFile;
			AssertEquals("Number of AMS Bill Of Ladings do not match Consol shipments", AMSConsolBillCustomsStatusList.Codes.OutOfSync, helper.GetAMSBillStatus(consolInOtherFactory));

			var shipment3InOtherFactory = consolInOtherFactory.Shipments.AddNew();
			shipment3InOtherFactory.JS_HouseBill = "OTTAHB3";
			AssertEquals("All AMS Bill of Ladings are not on file", AMSConsolBillCustomsStatusList.Codes.OnFile, helper.GetAMSBillStatus(consolInOtherFactory));
		}

		public void TestGetAMSBillStatus_NoStripInvalidCharacter()
		{
			Integration.Customs.US.USAMS.IAMSStatusHelper helper = new AMSStatusHelper();
			var consol1 = Factory.New<ForwardingConsol>();
			consol1.JK_TransportMode = Core.Constants.TransportModes.Sea;
			consol1.JK_RL_NKDischargePort = "USLAX";
			consol1.JK_AgentType = Core.Constants.AgentType.Agent;
			var shipment1 = consol1.Shipments.AddNew();
			shipment1.JS_HouseBill = "OTTAHB1234567890-2";
			var header1 = Factory.New<CusInBondHeader>();
			header1.BH_OverrideFreightDefaults = true; // stop synchronisation
			header1.BH_ParentID = consol1.PK;
			header1.BH_ParentTableCode = consol1.TablePrefix;
			var bill1 = header1.Bills.AddNew();
			bill1.B0_IssuerCode = "OTTA";
			bill1.B0_MasterBillNumber = "HB12345678902";

			var consol2 = Factory.New<ForwardingConsol>();
			consol2.JK_TransportMode = Core.Constants.TransportModes.Sea;
			consol2.JK_RL_NKDischargePort = "USLAX";
			consol2.JK_AgentType = Core.Constants.AgentType.Agent;
			var shipment2 = consol2.Shipments.AddNew();
			shipment2.JS_HouseBill = "OTTAHB12345678902";
			var header2 = Factory.New<CusInBondHeader>();
			header2.BH_OverrideFreightDefaults = true; // stop synchronisation
			header2.BH_ParentID = consol2.PK;
			header2.BH_ParentTableCode = consol2.TablePrefix;
			var bill2 = header2.Bills.AddNew();
			bill2.B0_IssuerCode = "OTTA";
			bill2.B0_MasterBillNumber = "HB12345678902";

			AssertEquals("Should not match as stripping invalid character will increase performance issue in Consol Module Filter", AMSConsolBillCustomsStatusList.Codes.OutOfSync, helper.GetAMSBillStatus(consol1));
			AssertEquals("Should match", AMSConsolBillCustomsStatusList.Codes.NotOnFile, helper.GetAMSBillStatus(consol2));
		}

		public void TestGetAMSBillStatus_CoLoadMaster()
		{
			Integration.Customs.US.USAMS.IAMSStatusHelper helper = new AMSStatusHelper();
			AssertEquals(ZString.Empty, helper.GetAMSBillStatus(null));
			var org = Factory.NewWithValidTestData<OrgHeader>();

			var directAMSForwarder = Factory.New<OrgHeader>();
			directAMSForwarder.OH_IsForwarder = true;
			directAMSForwarder.MiscServ.OM_FWDirectAMSReporter = true;
			directAMSForwarder.OH_FullName = "AMS DIRECT FORWARDER";
			directAMSForwarder.OH_Code = "AMSDIRFORW1";
			directAMSForwarder.MainAddress.OA_Address1 = "ADDRESS1";

			var notDirectAMSForwarder = Factory.New<OrgHeader>();
			notDirectAMSForwarder.OH_IsForwarder = true;
			notDirectAMSForwarder.MiscServ.OM_FWDirectAMSReporter = false;
			notDirectAMSForwarder.OH_FullName = "NOT AMS DIRECT FORWARDER";
			notDirectAMSForwarder.OH_Code = "NOTAMSDIRFWD";
			notDirectAMSForwarder.MainAddress.OA_Address1 = "ADDRESS1";

			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = Core.Constants.TransportModes.Sea;
			consol.JK_RL_NKDischargePort = "USLAX";
			consol.JK_AgentType = Core.Constants.AgentType.Agent;
			consol.JK_OA_SendingForwarderAddress = org.MainAddress.PK;
			var shipment1 = consol.Shipments.AddNew();
			shipment1.JS_HouseBill = "OTT1HB123456789012";
			shipment1.JS_ShipmentType = Core.Constants.ShipmentTypes.CoLoadMaster;
			var coloadShipment1 = shipment1.CoLoadShipments.AddNew();
			coloadShipment1.JS_HouseBill = "OTT1HB234567890123";

			var header = Factory.New<CusInBondHeader>();
			header.BH_OverrideFreightDefaults = true; // stop synchronisation
			header.BH_ParentID = consol.PK;
			header.BH_ParentTableCode = consol.TablePrefix;
			var bill1 = header.Bills.AddNew();
			bill1.B0_IssuerCode = "OTT1";
			bill1.B0_MasterBillNumber = "HB234567890123";
			bill1.MovementDetail.B9_CustomsStatus = AMSBillCustomsStatusList.Codes.OnFile;
			Factory.Save();
			AssertEquals("All AMS Bill of Ladings are on file", AMSConsolBillCustomsStatusList.Codes.OnFile, helper.GetAMSBillStatus(consol));

			shipment1.JS_ShipmentType = Core.Constants.ShipmentTypes.StandardHouse;
			AssertEquals("Not all AMS Bill of Ladings have a status", AMSConsolBillCustomsStatusList.Codes.OutOfSync, helper.GetAMSBillStatus(consol));

			shipment1.JS_ShipmentType = Core.Constants.ShipmentTypes.AssemblyMaster;
			shipment1.CoLoadShipments.Add(coloadShipment1);
			AssertEquals("Only Assembly Master child Bill of Ladings should be included", AMSConsolBillCustomsStatusList.Codes.OnFile, helper.GetAMSBillStatus(consol));

			shipment1.ConsignorPK = directAMSForwarder.PK;
			AssertEquals("Direct AMS Forwarder's bill should not be included", AMSConsolBillCustomsStatusList.Codes.OutOfSync, helper.GetAMSBillStatus(consol));

			shipment1.ConsignorPK = notDirectAMSForwarder.PK;
			AssertEquals("Not Direct AMS Forwarder's bill should be included", AMSConsolBillCustomsStatusList.Codes.OnFile, helper.GetAMSBillStatus(consol));
		}
	}
}
