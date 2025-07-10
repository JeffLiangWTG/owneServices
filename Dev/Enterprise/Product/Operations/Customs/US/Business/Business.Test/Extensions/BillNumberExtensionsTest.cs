using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.US.Messaging.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.US.Business.Testing
{
	sealed class BillNumberExtensionsTest : TestCaseWithFactory
	{
		public void TestGetSCAC()
		{
			var billNumber = (ZString)"SCACBN001";
			AssertEquals("SCAC", billNumber.GetSCAC(new List<ZString> { "SCAC" }));
		}

		public void TestGetBillNumberTrimSCAC()
		{
			var billNumber = (ZString)"SCACBN001";
			AssertEquals("BN001", billNumber.GetBillNumberTrimSCAC());
		}

		public void TestKeepValidBillNumberCharacters()
		{
			var billNumber = (ZString)"SCAC**BN001   ";
			AssertEquals("SCACBN001", billNumber.KeepValidBillNumberCharacters());
		}

		public void TestIsValidSCAC()
		{
			var carrier = Factory.New<USCarrierCombined>();
			carrier.UI_Code = "SCAC";
			carrier.UI_ModeOfTransportation = "10";
			var scac = (ZString)"SCAC";
			Assert("SEA", scac.IsValidSCAC(Factory, "SEA"));
			Assert("RAI", !scac.IsValidSCAC(Factory, "RAI"));
		}

		public void TestGetValidSCACIssuerCodes()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_AgentType = Core.Constants.AgentType.Agent;

			var carr01 = Factory.New<USCarrierCombined>();
			carr01.UI_Code = "TESA";
			carr01.UI_ModeOfTransportation = TransportModeCodes.Codes.VesselContainer;

			var carr02 = Factory.New<USCarrierCombined>();
			carr02.UI_Code = "TESB";
			carr02.UI_ModeOfTransportation = TransportModeCodes.Codes.VesselNonContainer;

			var carr03 = Factory.New<USCarrierCombined>();
			carr03.UI_Code = "TESC";
			carr03.UI_ModeOfTransportation = TransportModeCodes.Codes.AirContainer;

			var carr04 = Factory.New<USCarrierCombined>();
			carr04.UI_Code = "TESD";
			carr04.UI_ModeOfTransportation = TransportModeCodes.Codes.AirNonContainer;

			var forwarder = Factory.New<OrgHeader>();
			forwarder.OH_Code = "FORWARDER";
			var forwarderAddress = forwarder.MainAddress;
			forwarder.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.CodeTypes.CarrierCode, "TESA", Core.Constants.CountryCodes.UnitedStates);

			var creditor = Factory.New<OrgHeader>();
			creditor.OH_Code = "CREDITOR";
			var creditorAddress = creditor.MainAddress;
			creditor.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.CodeTypes.CarrierCode, "TESB", Core.Constants.CountryCodes.UnitedStates);

			var hbi = Factory.New<OrgHeader>();
			hbi.OH_Code = "HBIPARTY";
			var hbiAddress = hbi.MainAddress;
			hbi.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.CodeTypes.CarrierCode, "TESC", Core.Constants.CountryCodes.UnitedStates);

			var orgProxy = Factory.Load<OrgHeader>(GlbCompany.CurrentCompany.GC_OH_OrgProxy);
			orgProxy.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.CodeTypes.CarrierCode, "TESD", Core.Constants.CountryCodes.UnitedStates);

			var shipment = consol.Shipments.AddNew();
			shipment.JS_TransportMode = Core.Constants.TransportModes.Sea;
			var shipmentHBI = shipment.DocAddresses.AddNew();
			shipmentHBI.E2_AddressType = DocAddressTypes.Codes.HouseBillIssuingParty;
			shipmentHBI.E2_OA_Address = hbiAddress.PK;
			shipmentHBI.E2_AddressOverride = false;
			shipmentHBI.E2_AddressSequence = 0;

			consol.JK_OA_SendingForwarderAddress = forwarderAddress.PK;

			var codes = shipment.GetValidSCACIssuerCodes(Core.Constants.TransportModes.Sea);
			AssertEquals("Count of Valid Codes", 1, codes.Count());
			Assert(codes.Contains(carr01.UI_Code));

			shipment.JS_TransportMode = Core.Constants.TransportModes.Air;
			codes = shipment.GetValidSCACIssuerCodes(Core.Constants.TransportModes.Air);
			AssertEquals("Count of Valid Codes", 2, codes.Count());
			Assert(codes.Contains(carr03.UI_Code));
			Assert(codes.Contains(carr04.UI_Code));

			consol.JK_AgentType = Core.Constants.AgentType.CoLoad;
			consol.JK_OA_CreditorAddress = creditorAddress.PK;
			codes = shipment.GetValidSCACIssuerCodes(Core.Constants.TransportModes.Air);
			AssertEquals("Count of Valid Codes", 2, codes.Count());
			Assert(codes.Contains(carr03.UI_Code));
			Assert(codes.Contains(carr04.UI_Code));

			shipment.JS_TransportMode = Core.Constants.TransportModes.Sea;
			codes = shipment.GetValidSCACIssuerCodes(Core.Constants.TransportModes.Sea);
			AssertEquals("Should only match the forwarder", carr01.UI_Code, codes.Single());
		}
	}
}
