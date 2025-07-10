using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.Universal.Testing;
using Enterprise.DocumentVisualizer.Testing;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using static Enterprise.Core.Constants.Customs.Universal;

namespace Enterprise.Freight.Forwarding.Documents.Testing.IL
{
	public class DeliveryOrderDocumentTest : StandardDocumentContentTest
	{
		public override void TestDocumentContent()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Israel);
			helper.CreateNewOrGetExistingCusCodeType(RefCusCodeListTypes.Codes.ILCargoIdentifierType, "C1259", Core.Constants.CountryCodes.Israel);
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Israel, RefCusCodeListTypes.Codes.ILCargoIdentifierType, "11", "SeaDealImport", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			Factory.Save();

			var pivotPK = new ZGuid("E08FE6ED-53A1-4CBB-9B6B-9CA7D9C6F4D9");
			var shipment = CreateShipment(Core.Constants.TransportModes.Sea);
			AssertContents(shipment, pivotPK, Content);
		}

		ForwardingShipment CreateShipment(string transportMode, bool addReceivingForwarderVat = true, bool addImportBrokerVat = true, bool addImportReleaseDepot = true, bool addShipmentFdn = true)
		{
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_TransportMode = transportMode;
			shipment.JS_UniqueConsignRef = "UNIQ123";
			shipment.JS_DLO = "100000007";

			if (addShipmentFdn)
			{
				var fdnShipment = shipment.Numbers.AddNew();
				fdnShipment.CE_EntryType = IsraelShipmentAdditionalReferenceNumberTypes.Codes.ForwarderDealNumber;
				fdnShipment.CE_EntryNum = "FDN456";
			}

			var consol = shipment.Consols.AddNew();
			consol.JK_RL_NKDischargePort = "ILASH";
			var fdnConsol = consol.CusEntryNums.AddNew();
			fdnConsol.CE_EntryType = IsraelShipmentAdditionalReferenceNumberTypes.Codes.ForwarderDealNumber;
			fdnConsol.CE_EntryNum = "FDN123";

			var receivingForwarder = Factory.New<OrgHeader>();
			receivingForwarder.MainAddress.CompanyName = "I'm Receiving Stuff";
			receivingForwarder.OH_RL_NKClosestPort = "ILASH";
			receivingForwarder.MainAddress.Address1 = "Unit 399";
			receivingForwarder.MainAddress.Address2 = "50 What Lane";
			receivingForwarder.MainAddress.City = "Ashdod";
			receivingForwarder.MainAddress.Postcode = "5023";
			receivingForwarder.MainAddress.OA_RN_NKCountryCode = "IL";
			if (addReceivingForwarderVat)
			{
				receivingForwarder.MainAddress.CustomsCodes.AddNew(OrgCusCode.CodeTypes.VATCode, "111222333", "IL");
			}

			consol.JK_OA_ReceivingForwarderAddress = receivingForwarder.MainAddress.PK;

			var transport = consol.Transports.AddNew();
			transport.JW_RL_NKDiscPort = "ILASH";
			transport.JW_ArrivalPortRouteId = "ARR123";

			var importBroker = Factory.New<OrgHeader>();
			importBroker.MainAddress.CompanyName = "I'm Import Broker";
			importBroker.OH_RL_NKClosestPort = "ILTLV";
			importBroker.MainAddress.Address1 = "Unit 400";
			importBroker.MainAddress.Address2 = "51 What Lane";
			importBroker.MainAddress.City = "Tel Aviv";
			importBroker.MainAddress.Postcode = "5024";
			importBroker.MainAddress.OA_RN_NKCountryCode = "IL";
			if (addImportBrokerVat)
			{
				importBroker.MainAddress.CustomsCodes.AddNew(OrgCusCode.CodeTypes.VATCode, "555666777", "IL");
			}

			shipment.JS_OH_ImportBroker = importBroker.PK;

			if (addImportReleaseDepot)
			{
				var importReleaseDepot = Factory.New<OrgHeader>();
				importReleaseDepot.OH_FullName = "I'm Import Release Depo";
				importReleaseDepot.OH_RL_NKClosestPort = "ILTLV";
				importReleaseDepot.MainAddress.Address1 = "Unit 500";
				importReleaseDepot.MainAddress.Address2 = "52 What Lane";
				importReleaseDepot.MainAddress.City = "Jerusalem";
				importReleaseDepot.MainAddress.Postcode = "5025";
				importReleaseDepot.MainAddress.OA_RN_NKCountryCode = "IL";
				importReleaseDepot.MainAddress.CustomsCodes.AddNew(OrgCusCode.CodeTypes.ControlledPremisesID, "1234", "IL");

				shipment.JS_OA_ImportReleaseDepot = importReleaseDepot.MainAddress.PK;
			}

			return shipment;
		}

		const string Content = @"[2,3] Delivery Order 
[3,3] Forwarder
[3,8] Customs Broker
[4,3] Name
[4,4] I'm Receiving Stuff
[4,8] Name
[4,9] I'm Import Broker
[5,3] Address
[5,4] Unit 399
[5,8] Address
[5,9] Unit 400
[6,3] Country
[6,4] IL
[6,8] Country
[6,9] IL
[7,3] IL VAT :
[7,4] 111222333
[7,8] IL VAT :
[7,9] 555666777
[11,3] General Data
[12,3] Delivery Order Number
[12,5] 100000007
[13,3] Delivery Site 
[13,5] 1234
[14,3] Receiver Type
[16,3] Cargo Data
[17,3] Cargo Identifier Type
[17,5] 11-SeaDealImport
[18,3] Manifest
[18,5] ARR123
[19,3] Deal Number
[19,5] FDN456
[23,9] Created By";
	}
}
