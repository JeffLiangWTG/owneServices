using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.Universal.Testing;
using Enterprise.DocumentVisualizer.Testing;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;
using static Enterprise.Core.Constants.Customs.Universal;

namespace Enterprise.Freight.Forwarding.Documents.Testing.IL
{
	sealed class GatePassMovementDocumentTest : StandardDocumentContentTest
	{
		[TestDate(2024, 7, 24)]
		public override void TestDocumentContent()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);

			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Israel);

			helper.CreateNewOrGetExistingCusCodeType(RefCusCodeListTypes.Codes.ILCargoIdentifierType, "C1259", Core.Constants.CountryCodes.Israel);
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Israel, RefCusCodeListTypes.Codes.ILCargoIdentifierType, "11", "SeaDealImport", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);

			helper.CreateNewOrGetExistingCusCodeType(RefCusCodeListTypes.Codes.Facilities, "FAC");
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Israel, RefCusCodeListTypes.Codes.Facilities, "ILASH", "אשדוד", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Israel, RefCusCodeListTypes.Codes.Facilities, "ILTLV", "תל אביב", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);

			Factory.Save();

			var pivotPK = new ZGuid("1D9183BB-319E-40D2-A089-1616674506E1");
			var shipment = CreateShipment(Core.Constants.TransportModes.Sea);
			const string expectedContent =
@"[2,3] Gatepass Movement 
[3,3] General Data
[4,3] Gatepass movement Number
[4,5] 100000007
[5,3] Process Type
[5,5] 1
[6,3] Origin Site
[6,5] ILASH-אשדוד
[7,3] Destination Site 
[7,5] ILTLV-תל אביב
[8,3] Cargo Type
[8,5] FCL-Full Container Load
[9,3] Transport Method
[11,3] Cargo Data
[12,3] Cargo Identifier Type
[12,5] 11-SeaDealImport
[13,3] Cargo Identifier Key 1
[13,5] ARR123
[14,3] Cargo Identifier Key 2
[14,5] FDN456
[18,9] Created By";
			AssertContents(shipment, pivotPK, expectedContent);
		}

		ForwardingShipment CreateShipment(string transportMode, bool addReceivingForwarderVat = true, bool addImportBrokerVat = true, bool addImportReleaseDepot = true, bool addShipmentFdn = true)
		{
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_TransportMode = transportMode;
			shipment.JS_UniqueConsignRef = "UNIQ123";
			shipment.JS_RL_NKDestination = "ILASH";
			shipment.JS_GMN = "100000007";

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
			receivingForwarder.OH_FullName = "I'm Receiving Stuff";
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
			importBroker.OH_FullName = "I'm Import Broker";
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
				importReleaseDepot.MainAddress.CustomsCodes.AddNew(OrgCusCode.CodeTypes.ControlledPremisesID, "ILTLV", "IL");

				shipment.JS_OA_ImportReleaseDepot = importReleaseDepot.MainAddress.PK;
			}

			return shipment;
		}
	}
}

