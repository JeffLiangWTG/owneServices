using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Customs.Common;
using Enterprise.DocumentVisualizer.DocDataObjects;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects.FR;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Forwarding.Documents.Testing.FR
{
	sealed class CINExportNotificationBuilderTest : TestCaseWithFactory
	{
		public void TestGeneral()
		{
			var shipment = CreateShipment();

			var builder = new CINExportNotificationBuilder(shipment);
			var exportNotificationBuilder = builder.Build();
			AssertNotNull(exportNotificationBuilder);

			AssertEquals("MasterBill", "11199999999", exportNotificationBuilder.MasterBill);
			AssertEquals("MasterBill", "111-99999999", exportNotificationBuilder.MasterBillWithPrefix);
			AssertEquals("HouseBill", "H001", exportNotificationBuilder.HouseBill);
			AssertEquals("ShipmentID", "SH0001000", exportNotificationBuilder.ShipmentID);
			AssertEquals("MRNNumbers", "MRN01, MRN02", exportNotificationBuilder.StrMRNNumbers);
			AssertEquals("CustomsOfficeCode", "COC01", exportNotificationBuilder.CustomsOfficeCode);
			AssertEquals("GrossWeight", 20M, exportNotificationBuilder.GrossWeight.Value);
			AssertEquals("PackCount", 1, exportNotificationBuilder.PackCount);
			AssertEquals("PackType", "PKG", exportNotificationBuilder.PackType.Code);

			AssertNoMessageErrors("MasterBill is no errors", exportNotificationBuilder.MasterBillInfo);
			AssertNoMessageErrors("HouseBill is no errors", exportNotificationBuilder.HouseBillInfo);
			AssertNoMessageErrors("ShipmentID is no errors", exportNotificationBuilder.ShipmentIDInfo);
			AssertNoMessageErrors("MRNNumbers is no errors", exportNotificationBuilder.StrMRNNumbersInfo);
			AssertNoMessageErrors("CustomsOfficeCode is no errors", exportNotificationBuilder.CustomsOfficeCodeInfo);
			AssertNoMessageErrors("GrossWeight is no errors", ((Measurement)exportNotificationBuilder.GrossWeight).ValueInfo);
			AssertNoMessageErrors("PackCount is no errors", exportNotificationBuilder.PackCountInfo);
			AssertNoMessageErrors("PackType is no errors", ((CodeDescription)exportNotificationBuilder.PackType).CodeInfo);

			shipment.Consols[0].JK_MasterBillNum = ZString.Empty;
			shipment.JS_HouseBill = ZString.Empty;
			shipment.Numbers.RemoveAndDeleteAll();
			shipment.JS_ActualWeight = 0;
			shipment.JS_OuterPacks = 0;
			shipment.JS_F3_NKPackType = ZString.Empty;
			exportNotificationBuilder = builder.Build();

			AssertHasMessageError("Master Air Waybill must be in format '999-99999999'", exportNotificationBuilder.MasterBillWithPrefixInfo, "Master Air Waybill is not valid. (See Consol > MAWB)");
			AssertHasMessageError("House Bill is required", exportNotificationBuilder.HouseBillInfo, "House Bill is required.");
			AssertHasMessageError("MRN Number is required", exportNotificationBuilder.StrMRNNumbersInfo, "MRN Number is required.");
			AssertHasMessageError("Customs Office Code is required", exportNotificationBuilder.CustomsOfficeCodeInfo, "Customs Office Code is required.");
			AssertHasMessageError("Gross Weight cannot be zero", ((Measurement)exportNotificationBuilder.GrossWeight).ValueInfo, "Gross Weight cannot be zero.");
			AssertHasMessageError("Packs is required", exportNotificationBuilder.PackCountInfo, "Packs is required.");
			AssertHasMessageError("Package Type is required", ((CodeDescription)exportNotificationBuilder.PackType).CodeInfo, "Package Type is required.");

			shipment.Consols[0].JK_MasterBillNum = "111222";
			exportNotificationBuilder = builder.Build();
			AssertHasMessageError("Master Air Waybill must be in format '999-99999999'", exportNotificationBuilder.MasterBillWithPrefixInfo, "Master Air Waybill is not valid. (See Consol > MAWB)");

			shipment.Consols[0].JK_AgentType = Constants.AgentType.Agent;
			exportNotificationBuilder = builder.Build();
			AssertEquals(1,exportNotificationBuilder.AgentTypes.Count);
			Assert(exportNotificationBuilder.AgentTypes.Any(pair => pair.Code == Constants.AgentType.Agent));

			var addedConsol = shipment.Consols.AddNew();
			addedConsol.JK_AgentType = Constants.AgentType.Direct;
			exportNotificationBuilder = builder.Build();
			AssertEquals(2, exportNotificationBuilder.AgentTypes.Count);
			Assert(exportNotificationBuilder.AgentTypes.Any(pair => pair.Code == Constants.AgentType.Agent));
			Assert(exportNotificationBuilder.AgentTypes.Any(pair => pair.Code == Constants.AgentType.Direct));
		}

		public void TestCarrierCIN()
		{
			var shipment = CreateShipment();

			var builder = new CINExportNotificationBuilder(shipment);
			var exportNotificationBuilder = builder.Build();
			AssertNotNull(exportNotificationBuilder);
			AssertEquals("CarrierCin", "AAA", exportNotificationBuilder.CarrierCIN);

			var airLegCarrier = shipment.TransportsInLegOrder.Cast<Freight.Business.Transport>().FirstOrDefault(transport => transport.IsAir).Carrier;
			var ci5Code1 = airLegCarrier.CustomsCodes.AddNew();
			ci5Code1.OK_CodeType = OrgCusCode.FranceCodeTypes.CIN;
			ci5Code1.OK_RN_NKCodeCountry = Constants.CountryCodes.France;
			ci5Code1.OK_CustomsRegNo = "C001";

			Factory.Save();

			exportNotificationBuilder = builder.Build();
			AssertNotNull(exportNotificationBuilder);
			AssertEquals("CarrierCin", "C001", exportNotificationBuilder.CarrierCIN);
		}

		public void TestAddressValidations()
		{
			var shipment = Factory.New<ForwardingShipment>();
			var builder = new CINExportNotificationBuilder(shipment);
			var data = builder.Build();

			AssertPartyNameAndAddressValidation(data.SendingParty, "Sending Party");
			AssertPartyNameAndAddressValidation(data.Carrier, "Carrier");
			AssertPartyNameAndAddressValidation(data.Warehouse, "CFS / Warehouse");
		}

		void AssertPartyNameAndAddressValidation(Address address, string partyName)
		{
			address.CompanyName = ZString.Empty;
			address.Country.Code = ZString.Empty;
			address.AddressLine1 = ZString.Empty;

			address.ValidateAllIncludingChildren();
			AssertHasMessageError(address.CompanyNameInfo, $"{partyName} name and address is required.");

			address.CompanyName = "Nier";
			address.Country.Code = "CN";
			address.AddressLine1 = "Nier Address1";

			address.ValidateAllIncludingChildren();
			AssertNoMessageError(address.CompanyNameInfo, $"{partyName} name and address is required.");
		}

		public void TestMRNNumbersFallBackToRegistrationNumbers()
		{
			var shipment = CreateShipment();
			var builder = new CINExportNotificationBuilder(shipment);
			var exportNotificationBuilder = builder.Build();
			AssertEquals("MRNNumbers", "MRN01, MRN02", exportNotificationBuilder.StrMRNNumbers);

			var number = shipment.CusEntryNumbers.AddNew();
			number.CE_EntryType = CusEntryNumberTypes.Standard.MovementReferenceNumber;
			number.CE_RN_NKCountryCode = Core.Constants.CountryCodes.France;
			number.CE_EntryNum = "MRN03";
			number.CE_Category = CusEntryNumber.Categories.CustomsPermitClearanceNumber;
			number.CE_ParentTable = shipment.TableName;
			Factory.Save();

			builder = new CINExportNotificationBuilder(shipment);
			exportNotificationBuilder = builder.Build();
			AssertEquals("MRNNumbers", "MRN03", exportNotificationBuilder.StrMRNNumbers);
		}

		#region Implementation

		ForwardingShipment CreateShipment()
		{
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_UniqueConsignRef = "SH0001000";
			shipment.JS_PackingMode = Constants.ContainerModes.LCL;
			shipment.JS_ReleaseType = Constants.ShipmentReleaseTypes.SeaWaybill;
			shipment.JS_RL_NKOrigin = "AUMEL";
			shipment.JS_RL_NKDischargePort = "FRMRS";
			shipment.JS_RL_NKDestination = "FRNCE";
			shipment.JS_RL_NKLoadPort = "FRPAR";
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

			var contact = Factory.NewWithValidTestData<OrgContact>();
			contact.OC_ContactName = "Handsome";
			contact.OC_Phone = "1234567";

			var consol = shipment.Consols.AddNew();
			consol.JK_TransportMode = Constants.TransportModes.Sea;
			consol.JK_AgentType = Constants.AgentType.Agent;
			consol.JK_ReleaseType = Constants.ShipmentReleaseTypes.OriginalReq;
			consol.JK_NoOriginalBills = 3;
			consol.JK_NoCopyBills = 4;
			consol.JK_RL_NKLoadPort = "AUMEL";
			consol.JK_RL_NKDischargePort = "FRNCE";
			consol.JK_BookingReference = "WhiskyTreasure";
			consol.JK_MasterBillIssueDate = new ZDateTime(2018, 10, 2);
			consol.JK_MasterBillNum = "11199999999";
			consol.JK_AgentsReference = "AGTREF";
			consol.JK_UniqueConsignRef = "CON0001";
			consol.JK_PrepaidCollect = Constants.PaymentType.Collect;

			var transport = consol.Transports.OfType<Freight.Business.Transport>().Single();
			transport.JW_LegOrder = 1;
			transport.JW_TransportMode = Constants.TransportModes.Air;
			transport.JW_TransportType = Constants.TransportPlanningType.MainVessel;
			transport.JW_RL_NKLoadPort = "AUMEL";
			transport.JW_RL_NKDiscPort = "AUSYD";
			transport.JW_Vessel = "Dragon";
			transport.JW_VoyageFlight = "666";
			transport.JW_ETD = ZDate.Today.AddDays(1);
			transport.JW_ETA = ZDate.Today.AddDays(6);

			var refAirline = Factory.New<RefAirline>();
			refAirline.RM_EagleAddedAirlinePrefixOrAccountingCode = "X1Z";
			refAirline.RM_TwoCharacterCode = "A1";
			refAirline.RM_ThreeLetterCode = "AAA";

			var transportCarrier = Factory.NewWithValidTestData<OrgHeader>();
			transportCarrier.OH_FullName = "transportCarrier";
			transportCarrier.OH_RL_NKClosestPort = "FRPAR";
			transportCarrier.MainAddress.Address1 = "Unit 15";
			transportCarrier.MainAddress.Address2 = "5 Lost Lane";
			transportCarrier.MainAddress.City = "Marseille";
			transportCarrier.MainAddress.Postcode = "2000";
			transportCarrier.MainAddress.OA_RN_NKCountryCode = "FR";
			transportCarrier.MainAddress.OA_RL_NKRelatedPortCode = "FRPAR";
			transportCarrier.MiscServ.OM_RM_Airline = refAirline.PK;

			transport.JW_OA_CarrierAddress = transportCarrier.MainAddress.PK;

			PopulateConsolAddresses(consol);

			var number1 = shipment.Numbers.AddNew();
			number1.CE_EntryType = CusEntryNumberTypes.Standard.MovementReferenceNumber;
			number1.CE_RN_NKCountryCode = Core.Constants.CountryCodes.France;
			number1.CE_EntryNum = "MRN01";

			var number2 = shipment.Numbers.AddNew();
			number2.CE_EntryType = CusEntryNumberTypes.Standard.MovementReferenceNumber;
			number2.CE_RN_NKCountryCode = Core.Constants.CountryCodes.France;
			number2.CE_EntryNum = "MRN02";

			var number3 = shipment.Numbers.AddNew();
			number3.CE_EntryType = "COC";
			number3.CE_RN_NKCountryCode = Core.Constants.CountryCodes.France;
			number3.CE_EntryNum = "COC01";

			PopulateShipmentAddresses(shipment);

			Factory.Save();

			return shipment;
		}

		void PopulateShipmentAddresses(ForwardingShipment shipment)
		{
			ZQuery query = new ZQuery(RefUNLOCOSchema.RL_Code, "FRPAR");
			var unloco = Factory.LoadTop1<RefUNLOCO>(query);
			unloco.RefLocoMaps.DeleteAll();

			var cfs = Factory.New<OrgHeader>();
			cfs.OH_FullName = "CONSPA";
			cfs.OH_RL_NKClosestPort = "FRPAR";
			cfs.MainAddress.Address1 = "Unit 15";
			cfs.MainAddress.Address2 = "5 Lost Lane";
			cfs.MainAddress.City = "Marseille";
			cfs.MainAddress.Postcode = "2000";
			cfs.MainAddress.OA_RN_NKCountryCode = "FR";
			cfs.MainAddress.OA_RL_NKRelatedPortCode = "FRPAR";
			shipment.JS_OA_ExportReceivingDepot = cfs.MainAddress.PK;

			var consignor = Factory.New<OrgHeader>();
			consignor.OH_FullName = "I'm consignor";
			consignor.OH_RL_NKClosestPort = "CNBSX";
			consignor.MainAddress.Address1 = "Unit 200";
			consignor.MainAddress.Address2 = "55 haha Lane";
			consignor.MainAddress.City = "wahaha Ave";
			consignor.MainAddress.Postcode = "10000";
			consignor.MainAddress.OA_RN_NKCountryCode = "CN";

			shipment.ConsignorDocumentaryAddress.E2_OA_Address = consignor.MainAddress.PK;

			var consignee = Factory.New<OrgHeader>();
			consignee.OH_FullName = "I'm consignee";
			consignee.OH_RL_NKClosestPort = "AUMEL";
			consignee.MainAddress.Address1 = "Unit 223";
			consignee.MainAddress.Address2 = "553 What Lane";
			consignee.MainAddress.City = "Melbourne";
			consignee.MainAddress.Postcode = "5023";
			consignee.MainAddress.OA_RN_NKCountryCode = "AU";

			shipment.ConsigneeDocumentaryAddress.E2_OA_Address = consignee.MainAddress.PK;

			var notifyParty = Factory.New<OrgHeader>();
			notifyParty.OH_FullName = "Notify Me";
			notifyParty.OH_RL_NKClosestPort = "NZAKL";
			notifyParty.MainAddress.Address1 = "Unit 888";
			notifyParty.MainAddress.Address2 = "8 What Lane";
			notifyParty.MainAddress.City = "Auckland";
			notifyParty.MainAddress.Postcode = "5012";
			notifyParty.MainAddress.OA_RN_NKCountryCode = "NZ";

			shipment.NotifyPartyDocumentaryAddress.E2_OA_Address = notifyParty.MainAddress.PK;

			var notifyParty2 = Factory.New<OrgHeader>();
			notifyParty2.OH_FullName = "Notify Me Two";
			notifyParty2.OH_RL_NKClosestPort = "NZAKL";
			notifyParty2.MainAddress.Address1 = "Unit 666";
			notifyParty2.MainAddress.Address2 = "8 How Lane";
			notifyParty2.MainAddress.City = "Auckland";
			notifyParty2.MainAddress.Postcode = "5032";
			notifyParty2.MainAddress.OA_RN_NKCountryCode = "NZ";

			shipment.NotifyParty2DocumentaryAddress.E2_OA_Address = notifyParty2.MainAddress.PK;

			var consignorPickupAddress = Factory.New<OrgHeader>();
			consignorPickupAddress.OH_FullName = "consignor Pickup org";
			consignorPickupAddress.OH_RL_NKClosestPort = "CNBZN";
			consignorPickupAddress.MainAddress.Address1 = "Unit 645";
			consignorPickupAddress.MainAddress.Address2 = "234 Drive";
			consignorPickupAddress.MainAddress.City = "unknown city";
			consignorPickupAddress.MainAddress.Postcode = "3243";
			consignorPickupAddress.MainAddress.OA_RN_NKCountryCode = "CN";

			shipment.ConsignorPickupAddress.E2_OA_Address = consignorPickupAddress.MainAddress.PK;

			var consigneeDeliveryAddress = Factory.New<OrgHeader>();
			consigneeDeliveryAddress.OH_FullName = "consignee delivery org";
			consigneeDeliveryAddress.OH_RL_NKClosestPort = "SGJUR";
			consigneeDeliveryAddress.MainAddress.Address1 = "Unit 563";
			consigneeDeliveryAddress.MainAddress.Address2 = "435 Drive";
			consigneeDeliveryAddress.MainAddress.City = "unknown city";
			consigneeDeliveryAddress.MainAddress.Postcode = "4356";
			consigneeDeliveryAddress.MainAddress.OA_RN_NKCountryCode = "SG";

			shipment.ConsigneeDeliveryAddress.E2_OA_Address = consigneeDeliveryAddress.MainAddress.PK;

			var pickupAgentAddress = Factory.New<OrgHeader>();
			pickupAgentAddress.OH_FullName = "pickup agent org";
			pickupAgentAddress.OH_RL_NKClosestPort = "FRNCE";
			pickupAgentAddress.MainAddress.Address1 = "Unit 283";
			pickupAgentAddress.MainAddress.Address2 = "283 Drive";
			pickupAgentAddress.MainAddress.City = "unknown city";
			pickupAgentAddress.MainAddress.Postcode = "2836";
			pickupAgentAddress.MainAddress.OA_RN_NKCountryCode = "FR";

			shipment.PickupAgentDocumentaryAddress.E2_OA_Address = pickupAgentAddress.MainAddress.PK;

			var docsAndCartage = shipment.DocsAndCartage;
			docsAndCartage.JP_OA_PickupCartageCoAddr = pickupAgentAddress.MainAddress.PK;
		}

		void PopulateConsolAddresses(ForwardingConsol consol)
		{
			var carrier = Factory.New<OrgHeader>();
			carrier.OH_FullName = "MAERSK";
			carrier.OH_RL_NKClosestPort = "DKAAL";
			carrier.OH_IsAirLine = true;
			carrier.MainAddress.Address1 = "Unit 13";
			carrier.MainAddress.Address2 = "4 Lost Lane";
			carrier.MainAddress.City = "Aalborg";
			carrier.MainAddress.Postcode = "2000";
			carrier.MainAddress.OA_RN_NKCountryCode = "DK";

			consol.JK_OA_ShippingLineAddress = carrier.MainAddress.PK;

			var sendingForwarder = Factory.New<OrgHeader>();
			sendingForwarder.OH_FullName = "I'm Sending Stuff";
			sendingForwarder.OH_RL_NKClosestPort = "CNNJI";
			sendingForwarder.MainAddress.Address1 = "Unit 200";
			sendingForwarder.MainAddress.Address2 = "55 Why Lane";
			sendingForwarder.MainAddress.City = "Conficious Ave";
			sendingForwarder.MainAddress.Postcode = "10000";
			sendingForwarder.MainAddress.OA_RN_NKCountryCode = "CN";
			sendingForwarder.CustomsCodes.AddNew(OrgCusCode.ChinaCodeTypes.USC, "1234567890", "CN");

			consol.JK_OA_SendingForwarderAddress = sendingForwarder.MainAddress.PK;

			var receivingForwarder = Factory.New<OrgHeader>();
			receivingForwarder.OH_FullName = "I'm Receiving Stuff";
			receivingForwarder.OH_RL_NKClosestPort = "AUSYD";
			receivingForwarder.MainAddress.Address1 = "Unit 399";
			receivingForwarder.MainAddress.Address2 = "50 What Lane";
			receivingForwarder.MainAddress.City = "Sydney";
			receivingForwarder.MainAddress.Postcode = "5023";
			receivingForwarder.MainAddress.OA_RN_NKCountryCode = "AU";
			receivingForwarder.CustomsCodes.AddNew(OrgCusCode.AustraliaCodeTypes.AustralianBusinessNumber, "410 10 10 10", "AU");

			consol.JK_OA_ReceivingForwarderAddress = receivingForwarder.MainAddress.PK;

			var notifyParty = Factory.New<OrgHeader>();
			notifyParty.OH_FullName = "consol notify party";
			notifyParty.OH_RL_NKClosestPort = "GBLON";
			notifyParty.MainAddress.Address1 = "Unit 400";
			notifyParty.MainAddress.Address2 = "443 How Lane";
			notifyParty.MainAddress.City = "Angel";
			notifyParty.MainAddress.Postcode = "8888";
			notifyParty.MainAddress.OA_RN_NKCountryCode = "GB";
			notifyParty.CustomsCodes.AddNew(OrgCusCode.CodeTypes.VATCode, "123 123 123", "GB");

			consol.NotifyPartyDocumentaryAddress.E2_OA_Address = notifyParty.MainAddress.PK;

			var notifyParty2 = Factory.New<OrgHeader>();
			notifyParty2.OH_FullName = "consol notify party 2";
			notifyParty2.OH_RL_NKClosestPort = "CNCAN";
			notifyParty2.MainAddress.Address1 = "Unit 460";
			notifyParty2.MainAddress.Address2 = "333 How Lane";
			notifyParty2.MainAddress.City = "Wonderland";
			notifyParty2.MainAddress.Postcode = "7777";
			notifyParty2.MainAddress.OA_RN_NKCountryCode = "CN";
			notifyParty2.CustomsCodes.AddNew(OrgCusCode.ChinaCodeTypes.USC, "1111111111", "CN");

			consol.NotifyParty2DocumentaryAddress.E2_OA_Address = notifyParty2.MainAddress.PK;

			var carrierHandlingAgent = Factory.New<OrgHeader>();
			carrierHandlingAgent.OH_FullName = "consol carrier handling agent";
			carrierHandlingAgent.OH_RL_NKClosestPort = "CNCAN";
			carrierHandlingAgent.MainAddress.Address1 = "Unit 990";
			carrierHandlingAgent.MainAddress.Address2 = "245 Drive";
			carrierHandlingAgent.MainAddress.City = "unknown city";
			carrierHandlingAgent.MainAddress.Postcode = "4689";
			carrierHandlingAgent.MainAddress.OA_RN_NKCountryCode = "CN";

			consol.CarrierHandlingAgentDocumentaryAddress.E2_OA_Address = carrierHandlingAgent.MainAddress.PK;

			var carrierBookingAgent = Factory.New<OrgHeader>();
			carrierBookingAgent.OH_FullName = "consol carrier booking agent";
			carrierBookingAgent.OH_RL_NKClosestPort = "CNCAN";
			carrierBookingAgent.MainAddress.Address1 = "Unit 990";
			carrierBookingAgent.MainAddress.Address2 = "245 Drive";
			carrierBookingAgent.MainAddress.City = "unknown city";
			carrierBookingAgent.MainAddress.Postcode = "4689";
			carrierBookingAgent.MainAddress.OA_RN_NKCountryCode = "CN";

			consol.CarrierBookingAgentDocumentaryAddress.E2_OA_Address = carrierBookingAgent.MainAddress.PK;

			var packDepotOrg = Factory.New<OrgHeader>();
			packDepotOrg.OH_FullName = "pack depot org";
			packDepotOrg.OH_RL_NKClosestPort = "CNCAN";
			packDepotOrg.MainAddress.Address1 = "Unit 888";
			packDepotOrg.MainAddress.Address2 = "111 Drive";
			packDepotOrg.MainAddress.City = "unknown city";
			packDepotOrg.MainAddress.Postcode = "4679";
			packDepotOrg.MainAddress.OA_RN_NKCountryCode = "CN";

			consol.JK_OA_PackDepotAddress = packDepotOrg.MainAddress.PK;

			var unpackDepotOrg = Factory.New<OrgHeader>();
			unpackDepotOrg.OH_FullName = "unpack depot org";
			unpackDepotOrg.OH_RL_NKClosestPort = "SGSIN";
			unpackDepotOrg.MainAddress.Address1 = "Unit 589";
			unpackDepotOrg.MainAddress.Address2 = "625 Drive";
			unpackDepotOrg.MainAddress.City = "unknown city";
			unpackDepotOrg.MainAddress.Postcode = "9541";
			unpackDepotOrg.MainAddress.OA_RN_NKCountryCode = "SG";

			consol.JK_OA_UnpackDepotAddress = unpackDepotOrg.MainAddress.PK;
		}

		#endregion
	}
}
