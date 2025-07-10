using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Customs.Common;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects.NL;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Freight.Forwarding.Documents.NL.Testing
{
	sealed class CGNExportNotificationBuilderTest : TestCaseWithFactory
	{
		public void TestBuild()
		{
			var consol = CreateConsol();

			var exportNotification = new CGNExportNotificationBuilder(consol).Build();

			AssertEquals("MasterBill", "MAW-B01", exportNotification.MasterAirWaybill);
			AssertEquals("Sending Party CGN", "C001", exportNotification.SendingPartyCGN);
			AssertEquals("Sending Carrier CGN", "C002", exportNotification.CarrierCGN);
			AssertEquals("Operational Port", "NLAMS", exportNotification.OperationalPort.Code);
			AssertEquals("Shipments Count", 1, exportNotification.Shipments.Count);
			AssertEquals("Master Air Waybill", "MAW-B01", exportNotification.MasterAirWaybill);

			var shipment = exportNotification.Shipments.First();
			AssertEquals("Shipment No", "SH0001000", shipment.ShipmentID);
			AssertEquals("Shipment House Bill", "H001", shipment.HouseBillNumber);
			AssertEquals("Shipment MRN Number", "MRN01", shipment.MRNNumbers.First().Value);
			AssertEquals("Shipment Pack Count", 1, shipment.PackCount);
			AssertEquals("Shipment Pack Type", "PKG", shipment.PackType.Code);
			AssertEquals("Shipment Gross Weight", 20, shipment.GrossWeight.Value.ToZInt());
		}

		ForwardingConsol CreateConsol(string loadPort = "NLAMS", string dischargePort = "AUSYD")
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_UniqueConsignRef = "C00001000";
			consol.JK_TransportMode = Constants.TransportModes.Air;
			consol.JK_AgentType = Constants.AgentType.Agent;
			consol.JK_ConsolMode = Constants.ContainerModes.FCL;
			consol.JK_RL_NKLoadPort = loadPort;
			consol.JK_RL_NKDischargePort = dischargePort;
			consol.JK_MasterBillNum = "MAWB01";

			CreateAddresses(consol);
			CreateShipments(consol);
			return consol;
		}

		void CreateShipments(ForwardingConsol consol)
		{
			var number1 = Factory.New<CusEntryNumber>();
			number1.CE_RN_NKCountryCode = Constants.CountryCodes.Netherlands;
			number1.CE_EntryType = "MRN";
			number1.CE_EntryNum = "MRN01";

			var shipment = consol.Shipments.AddNew();
			shipment.Numbers.Add(number1);
			shipment.JS_UniqueConsignRef = "SH0001000";
			shipment.JS_PackingMode = Constants.ContainerModes.ULD;
			shipment.JS_TransportMode = Constants.TransportModes.Air;
			shipment.JS_RL_NKOrigin = "NLAMS";
			shipment.JS_RL_NKDestination = "AUSYD";
			shipment.JS_HouseBill = "H001";
			shipment.JS_ActualWeight = 20;
			shipment.JS_OuterPacks = 1;
			shipment.JS_F3_NKPackType = "PKG";
			shipment.JS_E_DEP = ZDate.Today.AddDays(1);
			shipment.JS_E_ARV = ZDate.Today.AddDays(5);
			shipment.JS_ConsolReference = "WhiskyTreasure";
			shipment.JS_GoodsDescription = "goods description";
			shipment.DetailedGoodsDescriptionNoteText = ZString.Empty;
			shipment.JS_MarksAndNumbers = "marks & numbers";
			shipment.JS_BookingReference = "BKG000001";
			shipment.JS_UnitOfWeight = "KG";
			shipment.JS_UnitOfVolume = "D3";
		}

		void CreateAddresses(ForwardingConsol consol)
		{
			var branchProxy = Factory.NewWithValidTestData<OrgHeader>();
			branchProxy.MainAddress.OA_RN_NKCountryCode = "NL";
			GlbBranch.CurrentBranch.GB_OH_OrgProxy = branchProxy.PK;

			Factory.Save();

			var cgnCode1 = GlbBranch.CurrentBranch.OrgProxy.CustomsCodes.AddNew();
			cgnCode1.OK_CodeType = OrgCusCode.NetherlandsCodeTypes.CargonautRegistationCode;
			cgnCode1.OK_RN_NKCodeCountry = Constants.CountryCodes.Netherlands;
			cgnCode1.OK_CustomsRegNo = "C001";

			var carrier = Factory.New<OrgHeader>();
			carrier.OH_FullName = "MAERSK";
			carrier.OH_RL_NKClosestPort = "DKAAL";
			carrier.OH_IsAirLine = true;
			carrier.MainAddress.Address1 = "Unit 13";
			carrier.MainAddress.Address2 = "4 Lost Lane";
			carrier.MainAddress.City = "Aalborg";
			carrier.MainAddress.Postcode = "2000";
			carrier.MainAddress.OA_RN_NKCountryCode = "NL";

			var firstTransportLeg = consol.Transports.Cast<Transport>().First();
			firstTransportLeg.JW_OA_CarrierAddress = carrier.MainAddress.PK;

			var cgnCode2 = carrier.CustomsCodes.AddNew();
			cgnCode2.OK_CodeType = OrgCusCode.NetherlandsCodeTypes.CargonautRegistationCode;
			cgnCode2.OK_RN_NKCodeCountry = Constants.CountryCodes.Netherlands;
			cgnCode2.OK_CustomsRegNo = "C002";

			Factory.Save();
		}
	}
}
