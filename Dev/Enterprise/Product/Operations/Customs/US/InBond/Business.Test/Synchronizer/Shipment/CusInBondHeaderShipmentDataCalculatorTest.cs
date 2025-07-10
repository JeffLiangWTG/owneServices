using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.US.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.US.InBond.Business.Testing
{
	sealed class CusInBondHeaderShipmentDataCalculatorTest : Customs.Business.Testing.SynchroniserTestCase
	{
		public void TestRelevantConsol()
		{
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "USCHI";
			consol.JK_RL_NKLoadPort = ZString.Empty;
			consol.JK_RL_NKDischargePort = ZString.Empty;
			AssertNull(calculator.RelevantConsol);
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "AUMEL";
			AssertNull(calculator.RelevantConsol);
			var consol2 = shipment.Consols.AddNew();
			consol2.JK_RL_NKLoadPort = ZString.Empty;
			consol2.JK_RL_NKDischargePort = ZString.Empty;
			AssertNull(calculator.RelevantConsol);
			consol2.JK_RL_NKLoadPort = "AUMEL";
			consol2.JK_RL_NKDischargePort = "USLAX";
			AssertEquals(consol2, calculator.RelevantConsol);
		}

		public void TestGetInfosAffectingRelevantConsol()
		{
			var list = new List<ZPropertyInfo>(calculator.GetInfosAffectingRelevantConsol());
			AssertEquals(4, list.Count);
			AssertCollectionContains(shipment.JS_RL_NKOriginInfo, list);
			AssertCollectionContains(shipment.JS_RL_NKDestinationInfo, list);
			AssertCollectionContains(consol.JK_RL_NKLoadPortInfo, list);
			AssertCollectionContains(consol.JK_RL_NKDischargePortInfo, list);
			var consol2 = shipment.Consols.AddNew();
			list = new List<ZPropertyInfo>(calculator.GetInfosAffectingRelevantConsol());
			AssertEquals(6, list.Count);
			AssertCollectionContains(shipment.JS_RL_NKOriginInfo, list);
			AssertCollectionContains(shipment.JS_RL_NKDestinationInfo, list);
			AssertCollectionContains(consol.JK_RL_NKLoadPortInfo, list);
			AssertCollectionContains(consol.JK_RL_NKDischargePortInfo, list);
			AssertCollectionContains(consol2.JK_RL_NKLoadPortInfo, list);
			AssertCollectionContains(consol2.JK_RL_NKDischargePortInfo, list);
		}

		public void TestMasterBill()
		{
			AssertEquals(ZString.Empty, calculator.MasterBill);
			declaration.JE_MasterBill = "OBDG323423";
			consol.JK_MasterBillNum = "532342332";
			var org = Factory.New<OrgHeader>();
			org.OH_Code = "aaa";
			org.OH_FullName = "bbb";
			var cusCode = org.CustomsCodes.AddNew();
			cusCode.OK_CodeType = OrgCusCode.CodeTypes.CarrierCode;
			cusCode.OK_CustomsRegNo = "OTD2";
			cusCode.OK_RN_NKCodeCountry = "US";
			consol.MasterBillIssuingPartyDocumentaryAddress.OrganisationPK = org.PK;
			var shippingLine = Factory.NewWithValidTestData<OrgHeader>();
			shippingLine.OH_IsCreditor = true;
			var carrierAddress = Factory.NewWithValidTestData<OrgAddress>();
			carrierAddress.OA_Code = "CarrierAdr";
			carrierAddress.OA_OH = shippingLine.PK;
			var shippingLineCarrierCode = shippingLine.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.CodeTypes.CarrierCode, "SCAC", Core.Constants.CountryCodes.UnitedStates);
			consol.JK_OA_ShippingLineAddress = carrierAddress.PK;
			AssertEquals("OBDG323423", calculator.MasterBill);
			declaration.JE_JS = ZGuid.Empty;
			Factory.Save();
			var carrier = Factory.New<USCarrierCombined>();
			carrier.UI_Code = "SCAC";
			carrier.UI_ModeOfTransportation = US.Messaging.Business.TransportModeCodes.Codes.RailContainer;
			var carrier2 = Factory.New<USCarrierCombined>();
			carrier2.UI_Code = "SCAC";
			carrier2.UI_ModeOfTransportation = US.Messaging.Business.TransportModeCodes.Codes.VesselContainer;
			header = Factory.New<CusInBondHeader>();
			header.BH_ParentID = shipment.PK;
			header.BH_ParentTableCode = shipment.TablePrefix;
			header.BH_ImportTransportMode = InBondTransportModeCodes.Codes.VesselContainer;
			calculator = new CusInBondHeaderShipmentDataCalculator(shipment, header);
			AssertEquals("532342332", calculator.MasterBill);
			consol.JK_MasterBillNum = "OTD2";
			AssertEquals("OTD2", calculator.MasterBill);
			consol.JK_MasterBillNum = "532342332";
			AssertEquals("532342332", calculator.MasterBill);
			consol.JK_MasterBillNum = "SCACBN001";
			AssertEquals("BN001", calculator.MasterBill);
			consol.JK_TransportMode = Enterprise.Core.Constants.TransportModes.Rail;
			consol.JK_MasterBillNum = "AABN002";
			AssertEquals("AABN002", calculator.MasterBill);
			consol.JK_MasterBillNum = "SCACBN002";
			AssertEquals("BN002", calculator.MasterBill);
		}

		public void TestGetInfosAffectingMasterBill()
		{
			var list = new List<ZPropertyInfo>(calculator.GetInfosAffectingMasterBill());
			AssertEquals(1, list.Count);
			AssertCollectionContains(declaration.JE_MasterBillInfo, list);
			declaration.JE_JS = ZGuid.Empty;
			Factory.Save();
			header = Factory.New<CusInBondHeader>();
			header.BH_ParentID = shipment.PK;
			header.BH_ParentTableCode = shipment.TablePrefix;
			calculator = new CusInBondHeaderShipmentDataCalculator(shipment, header);
			list = new List<ZPropertyInfo>(calculator.GetInfosAffectingMasterBill());
			AssertEquals(6, list.Count);
			AssertCollectionContains(shipment.JS_RL_NKOriginInfo, list);
			AssertCollectionContains(shipment.JS_RL_NKDestinationInfo, list);
			AssertCollectionContains(shipment.JS_TransportModeInfo, list);
			AssertCollectionContains(consol.JK_RL_NKLoadPortInfo, list);
			AssertCollectionContains(consol.JK_RL_NKDischargePortInfo, list);
			AssertCollectionContains(consol.JK_MasterBillNumInfo, list);
			var consol2 = shipment.Consols.AddNew();
			list = new List<ZPropertyInfo>(calculator.GetInfosAffectingMasterBill());
			AssertEquals(8, list.Count);
			AssertCollectionContains(shipment.JS_RL_NKOriginInfo, list);
			AssertCollectionContains(shipment.JS_RL_NKDestinationInfo, list);
			AssertCollectionContains(shipment.JS_TransportModeInfo, list);
			AssertCollectionContains(consol.JK_RL_NKLoadPortInfo, list);
			AssertCollectionContains(consol.JK_RL_NKDischargePortInfo, list);
			AssertCollectionContains(consol.JK_MasterBillNumInfo, list);
			AssertCollectionContains(consol2.JK_RL_NKLoadPortInfo, list);
			AssertCollectionContains(consol2.JK_RL_NKDischargePortInfo, list);
		}

		public void TestMasterBillIssuerCode()
		{
			var shippingLine = Factory.NewWithValidTestData<OrgHeader>();
			shippingLine.OH_IsCreditor = true;
			var carrierAddress = Factory.NewWithValidTestData<OrgAddress>();
			carrierAddress.OA_Code = "CarrierAdr";
			carrierAddress.OA_OH = shippingLine.PK;
			var shippingLineCarrierCode = shippingLine.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.CodeTypes.CarrierCode, "OTD2", Core.Constants.CountryCodes.UnitedStates);
			consol.JK_OA_ShippingLineAddress = carrierAddress.PK;
			AssertEquals(ZString.Empty, calculator.MasterBillIssuerCode);
			declaration.JE_MasterBillIssuerSCAC = "OBDG";
			consol.JK_MasterBillNum = "OTD2532342332";
			var org = Factory.New<OrgHeader>();
			org.OH_Code = "aaa";
			org.OH_FullName = "bbb";
			var cusCode = org.CustomsCodes.AddNew();
			cusCode.OK_CodeType = OrgCusCode.CodeTypes.CarrierCode;
			cusCode.OK_CustomsRegNo = "OTD2";
			cusCode.OK_RN_NKCodeCountry = "US";
			consol.MasterBillIssuingPartyDocumentaryAddress.OrganisationPK = org.PK;
			AssertEquals("OBDG", calculator.MasterBillIssuerCode);
			declaration.JE_JS = ZGuid.Empty;
			Factory.Save();
			var carrier = Factory.New<USCarrierCombined>();
			carrier.UI_Code = "OTD2";
			carrier.UI_ModeOfTransportation = US.Messaging.Business.TransportModeCodes.Codes.VesselContainer;
			header = Factory.New<CusInBondHeader>();
			header.BH_ParentID = shipment.PK;
			header.BH_ParentTableCode = shipment.TablePrefix;
			header.BH_ImportTransportMode = InBondTransportModeCodes.Codes.VesselContainer;
			calculator = new CusInBondHeaderShipmentDataCalculator(shipment, header);
			AssertEquals("OTD2", calculator.MasterBillIssuerCode);
			consol.JK_MasterBillNum = "OTD2BN001";
			AssertEquals("OTD2", calculator.MasterBillIssuerCode);
			var carrier2 = Factory.New<USCarrierCombined>();
			carrier2.UI_Code = "OTD2";
			carrier2.UI_ModeOfTransportation = US.Messaging.Business.TransportModeCodes.Codes.RailContainer;
			consol.JK_TransportMode = Enterprise.Core.Constants.TransportModes.Rail;
			consol.JK_MasterBillNum = "AABN002";
			AssertEquals("AABN002", calculator.MasterBill);
			consol.JK_MasterBillNum = "OTD2BN002";
			AssertEquals("OTD2", calculator.MasterBillIssuerCode);
		}

		public void TestGetInfosAffectingMasterBillIssuerCode()
		{
			var list = new List<ZPropertyInfo>(calculator.GetInfosAffectingMasterBillIssuerCode());
			AssertEquals(1, list.Count);
			AssertCollectionContains(declaration.JE_MasterBillIssuerSCACInfo, list);
			declaration.JE_JS = ZGuid.Empty;
			Factory.Save();
			header = Factory.New<CusInBondHeader>();
			header.BH_ParentID = shipment.PK;
			header.BH_ParentTableCode = shipment.TablePrefix;
			calculator = new CusInBondHeaderShipmentDataCalculator(shipment, header);
			list = new List<ZPropertyInfo>(calculator.GetInfosAffectingMasterBillIssuerCode());
			AssertEquals(6, list.Count);
			AssertCollectionContains(shipment.JS_RL_NKOriginInfo, list);
			AssertCollectionContains(shipment.JS_RL_NKDestinationInfo, list);
			AssertCollectionContains(shipment.JS_TransportModeInfo, list);
			AssertCollectionContains(consol.JK_RL_NKLoadPortInfo, list);
			AssertCollectionContains(consol.JK_RL_NKDischargePortInfo, list);
			AssertCollectionContains(consol.JK_MasterBillNumInfo, list);
			var consol2 = shipment.Consols.AddNew();
			list = new List<ZPropertyInfo>(calculator.GetInfosAffectingMasterBillIssuerCode());
			AssertEquals(8, list.Count);
			AssertCollectionContains(shipment.JS_RL_NKOriginInfo, list);
			AssertCollectionContains(shipment.JS_RL_NKDestinationInfo, list);
			AssertCollectionContains(shipment.JS_TransportModeInfo, list);
			AssertCollectionContains(consol.JK_RL_NKLoadPortInfo, list);
			AssertCollectionContains(consol.JK_RL_NKDischargePortInfo, list);
			AssertCollectionContains(consol.JK_MasterBillNumInfo, list);
			AssertCollectionContains(consol2.JK_RL_NKLoadPortInfo, list);
			AssertCollectionContains(consol2.JK_RL_NKDischargePortInfo, list);
		}

		public void TestGetConsolsInfos()
		{
			var list = new List<ZPropertyInfo>(calculator.GetConsolsInfos(ForwardingConsol.Schema.JK_AgentsReference, ForwardingShipment.Schema.JS_ActualVolume, ForwardingConsol.Schema.JK_AgentType));
			AssertEquals(2, list.Count);
			AssertCollectionContains(consol.JK_AgentsReferenceInfo, list);
			AssertCollectionContains(consol.JK_AgentTypeInfo, list);
			var consol2 = shipment.Consols.AddNew();
			list = new List<ZPropertyInfo>(calculator.GetConsolsInfos(ForwardingConsol.Schema.JK_AgentsReference, ForwardingShipment.Schema.JS_ActualVolume, ForwardingConsol.Schema.JK_AgentType));
			AssertEquals(4, list.Count);
			AssertCollectionContains(consol.JK_AgentsReferenceInfo, list);
			AssertCollectionContains(consol.JK_AgentTypeInfo, list);
			AssertCollectionContains(consol2.JK_AgentsReferenceInfo, list);
			AssertCollectionContains(consol2.JK_AgentTypeInfo, list);
		}

		public void TestMasterBillContainsSpecialCharacters()
		{
			declaration.JE_MasterBill = "MB1-323423";
			AssertEquals("MB1323423", calculator.MasterBill);
			header = Factory.New<CusInBondHeader>();
			header.BH_ParentID = shipment.PK;
			header.BH_ParentTableCode = shipment.TablePrefix;
			header.BH_ImportTransportMode = InBondTransportModeCodes.Codes.VesselContainer;
			declaration.JE_JS = ZGuid.Empty;
			calculator = new CusInBondHeaderShipmentDataCalculator(shipment, header);
			consol.JK_MasterBillNum = "1234-5323423";
			AssertEquals("12345323423", calculator.MasterBill);
		}

		ForwardingConsol consol;
		ForwardingShipment shipment;
		JobDeclaration declaration;
		CusInBondHeader header;
		CusInBondHeaderShipmentDataCalculator calculator;
		protected override void SetUp()
		{
			base.SetUp();
			consol = CreateFCLConsol();
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "USLAX";
			shipment = consol.Shipments.AddNew();
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "USLAX";
			declaration = Factory.New<JobDeclaration>();
			declaration.JE_JS = shipment.PK;
			header = Factory.New<CusInBondHeader>();
			header.BH_ParentID = shipment.PK;
			header.BH_ParentTableCode = shipment.TablePrefix;
			Factory.Save();
			calculator = new CusInBondHeaderShipmentDataCalculator(shipment, header);
		}

		protected override void TearDown()
		{
			calculator.Dispose();
			calculator = null;
			base.TearDown();
		}
	}
}
