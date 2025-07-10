using System.Collections.Generic;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentVisualizer.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.Business.AWB;
using Enterprise.Freight.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Services.OperationalActions.Support.Testing;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Forwarding.GUI.Testing
{
	public abstract class DocDataObjectSendingMessageMethodApplicatorTest : OperationalActionMethodApplicatorTest
	{
		protected void TestNoSelected()
		{
			ApplyApplicator(System.Array.Empty<BusinessObject>(), NoSelectedError);
		}

		protected void TestMessageSender()
		{
			var processor = ObjectFactory.Get(MessageSenderContext);
			Assert(MessageSender.GetType() == processor.GetType());
		}

		#region Implementation

		protected DocDataObjectSendingMessageSettings Settings => settings ?? (settings = new DocDataObjectSendingMessageSettings());
		DocDataObjectSendingMessageSettings settings;

		protected abstract string NoSelectedError { get; }

		protected abstract string MessageSenderContext { get; }

		protected IDocDataObjectMessageSender MessageSender => ReportSendingProvider.MessageSender;

		protected DocDataObjectReportSendingProvider ReportSendingProvider => docDataObjectReportSendingProvider ?? (docDataObjectReportSendingProvider = GetDocDataObjectReportSendingProvider());

		DocDataObjectReportSendingProvider docDataObjectReportSendingProvider;

		protected abstract DocDataObjectReportSendingProvider GetDocDataObjectReportSendingProvider();

		protected ForwardingShipment CreateShipmentWithoutErrors(ZString awb, ZString uniqueConsignRef, ZString uniqueConsolConsignRef, ZString origin, ZString destination)
		{
			var consol = Factory.New<ForwardingConsol>();
			PopulateConsol(consol, awb, origin, destination, uniqueConsolConsignRef);

			var shipment = consol.Shipments.AddNew();
			PopulateShipment(shipment, awb, origin, destination, uniqueConsignRef);
			PopulateAWBHeader(shipment);
			PopulateRateLines(shipment);
			PopulatePrepaidAndCollectValues(shipment);

			return shipment;
		}

		protected ForwardingConsol CreateConsolWithoutErrors(ZString awb, ZString uniqueConsignRef, ZString origin, ZString destination, ZString docDataName, ZString country)
		{
			var consol = Factory.New<ForwardingConsol>();
			PopulateConsol(consol, awb, origin, destination, uniqueConsignRef);
			PopulateTransport(consol, origin, destination);
			var shipment = consol.Shipments.AddNew();
			PopulateShipment(shipment, awb, origin, destination, "S00001001");
			PopulateAWBHeader(shipment);
			PopulateRateLines(shipment);
			PopulatePrepaidAndCollectValues(shipment);
			var documentData = CreateDocumentData(shipment, docDataName);
			AddLog(documentData, Events.MessageAccepted, ZDateTimeOffset.UtcToday.AddHours(-1), $"MST=Advanced Cargo Report|LOC={country}");
			AddLog(documentData, Events.InterchangeSent, ZDateTimeOffset.UtcToday.AddHours(-11), $"MST=Advanced Cargo Report|LOC={country}");
			return consol;
		}

		OrgHeader ImportAgent
		{
			get { return importAgent ?? (importAgent = GetOrgHeader("TEST1", "ImportAgent", "", "ImportAgentAddress", "", "Sydney", "2021", "AU", "REG0001")); }
		}

		OrgHeader Carrier
		{
			get { return carrier ?? (carrier = GetOrgHeader("c001", "Carrier", "AUMEL", "Unit 000", "Hypocrea astronidii", "Mel", "2019", "AU", "REG3001")); }
		}

		OrgHeader ReceivingForwarder
		{
			get { return receivingForwarder ?? (receivingForwarder = GetOrgHeader("c002", "ReceivingForwarder", "BRSAO", "Av Paulista 291", "Consolacao", "Sao Paulo", "11157802", "CN", "REG3002")); }
		}

		OrgHeader importAgent;
		OrgHeader carrier;
		OrgHeader receivingForwarder;

		OrgHeader GetOrgHeader(ZString code, ZString fullname, ZString closestPort, ZString address1, ZString address2, ZString city, ZString postcode, ZString countryCode, ZString cusRegNo)
		{
			var shipper = Factory.New<OrgHeader>();
			shipper.OH_Code = code;
			shipper.OH_FullName = fullname;
			shipper.OH_RL_NKClosestPort = closestPort;
			shipper.MainAddress.Address1 = address1;
			shipper.MainAddress.Address2 = address2;
			shipper.MainAddress.City = city;
			shipper.MainAddress.Postcode = postcode;
			shipper.MainAddress.OA_RN_NKCountryCode = countryCode;

			var orgCusCode = shipper.CustomsCodes.AddNew();
			orgCusCode.OK_CodeType = BrazilOrgCusCodeInfo.OrgCusCodes.CNPJ;
			orgCusCode.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.Brazil;
			orgCusCode.OK_CustomsRegNo = cusRegNo;

			return shipper;
		}

		void AddLog(VisualizerDocumentData documentData, Event eventType, ZDateTimeOffset eventDate, string parameters)
		{
			var paramList = new List<KeyValuePair<string, string>>();
			foreach (var paramPair in parameters.Split('|'))
			{
				var pair = paramPair.Split('=');
				paramList.Add(new KeyValuePair<string, string>(pair[0], pair[1]));
			}

			documentData.Logs.AddNew(eventType, eventDate, paramList.ToArray());
		}

		VisualizerDocumentData CreateDocumentData(ForwardingShipment shipment, ZString docDataName)
		{
			var documentData = Factory.New<VisualizerDocumentData>();
			documentData.JDD_ParentTableCode = JobShipmentSchema.Constants.Prefix;
			documentData.JDD_ParentID = shipment.PK;
			documentData.JDD_Name = docDataName;

			return documentData;
		}

		void PopulateTransport(ForwardingConsol consol, ZString origin, ZString destination)
		{
			var transportLeg = consol.Transports[0];
			transportLeg.JW_LegOrder = 1;
			transportLeg.JW_TransportMode = Core.Constants.TransportModes.Air;
			transportLeg.JW_RL_NKLoadPort = origin;
			transportLeg.JW_RL_NKDiscPort = destination;
			transportLeg.JW_ETD = ZDateTime.Today;
		}

		protected void PopulateConsol(ForwardingConsol consol, ZString mawb, ZString origin, ZString destination, ZString uniqueConsignRef)
		{
			consol.JK_TransportMode = Core.Constants.TransportModes.Air;
			consol.JK_MasterBillNum = mawb;
			consol.JK_RL_NKLoadPort = origin;
			consol.JK_RL_NKDischargePort = destination;
			consol.JK_UniqueConsignRef = uniqueConsignRef;
			consol.JK_OA_ShippingLineAddress = Carrier.MainAddress.PK;
			consol.JK_OA_ReceivingForwarderAddress = ReceivingForwarder.MainAddress.PK;
			consol.JK_OA_ReceivingForwarderAddress = ImportAgent.MainAddress.PK;
		}

		protected void PopulateShipment(ForwardingShipment shipment, ZString hawb, ZString origin, ZString destination, ZString uniqueConsignRef)
		{
			var shipper = GetOrgHeader("TEST2", "Consignor", "AUSYD", "Unit52", "Dorcus yamadai", "Sydney", "2017", "AU", "REG1111");
			shipment.ConsignorDocumentaryAddress.E2_OA_Address = shipper.MainAddress.PK;
			var consignee = GetOrgHeader("TEST3", "Consignee", "USLAX", "801", "Prismognathus delislei", "Somewhere", "10043", "US", "REG2222");
			shipment.ConsigneeDocumentaryAddress.E2_OA_Address = consignee.MainAddress.PK;
			shipment.JS_TransportMode = Core.Constants.TransportModes.Air;
			shipment.JS_RL_NKOrigin = origin;
			shipment.JS_RL_NKDestination = destination;
			shipment.JS_UniqueConsignRef = uniqueConsignRef;

			shipment.JS_HouseBill = hawb;
			shipment.JS_ActualWeight = 20;
			shipment.JS_UnitOfWeight = Core.Constants.Weight.Kilograms;
			shipment.JS_GoodsDescription = "goodsDescription";
			shipment.JS_ShipmentType = Core.Constants.ShipmentTypes.StandardHouse;
			shipment.JS_OuterPacks = 2;
			shipment.JS_OverrideWaybillDefaults = ZBool.True;
			shipment.Notes.AddNew(true, PredefinedNoteTypes.Instance.DetailedGoodsDescription.Description, "notes");
		}

		void PopulateRateLines(ForwardingShipment shipment)
		{
			AddRateLineToAWBHeader("3", 1, "K", "Q", "N", 256, 1, 5, "VOL 122.000 M3", shipment);
		}

		void AddRateLineToAWBHeader(ZString noOfPiecesOrRcp, ZDecimal grossWeight, ZString weightInLBsOrKGs, ZString rateClass, ZString commodityItemNumber, ZDecimal chargeableWeight, ZDecimal rateChargeOrDiscount, ZDecimal total, ZString natureAndQtyOfGoodsText, ForwardingShipment shipment)
		{
			var rateLine = shipment.AWBHeader.AWBRateLines.AddNew();
			rateLine.ER_NoOfPiecesOrRCP = noOfPiecesOrRcp;
			rateLine.ER_GrossWeight = grossWeight;
			rateLine.ER_WeightInLBsOrKGs = weightInLBsOrKGs;
			rateLine.ER_RateClass = rateClass;
			rateLine.ER_CommodityItemNumber = commodityItemNumber;
			rateLine.ER_ChargeableWeight = chargeableWeight;
			rateLine.ER_RateChargeOrDiscount = rateChargeOrDiscount;
			rateLine.ER_Total = total;
			rateLine.NatureAndQtyOfGoodsText.Text = natureAndQtyOfGoodsText;
		}

		void AddOtherChargeToAWBHeader(ZDecimal amount, ZString chargeCode, ZString description, EntitlementCodes entitlementCode, ZBool prePaid, Forwarding.AWB.Business.ExportAWBHeader awbHeader)
		{
			var otherCharge = awbHeader.AWBOtherCharges.AddNew();
			otherCharge.EO_Amount = amount;
			otherCharge.EO_ChargeCode = chargeCode;
			otherCharge.EO_ChargeDescription = description;
			otherCharge.EO_EntitlementCode = entitlementCode == EntitlementCodes.Agent ? Core.Constants.AWB.EntitlementCode.Agent : Core.Constants.AWB.EntitlementCode.Carrier;
			otherCharge.EO_PPDCLT = prePaid ? "PPD" : "COL";
		}

		void PopulatePrepaidAndCollectValues(ForwardingShipment shipment)
		{
			var totalWeightPPDRateLine = (ShipmentExportAWBRateLine)shipment.AWBHeader.AWBRateLines.AddNew();
			totalWeightPPDRateLine.ER_RateClass = Forwarding.AWB.Business.ExportAWBHeader.Constants.PrepaidCollect1CharCodes.Prepaid;
			totalWeightPPDRateLine.ER_Total = 20;

			var totalWeightCOLRateLine = (ShipmentExportAWBRateLine)shipment.AWBHeader.AWBRateLines.AddNew();
			totalWeightCOLRateLine.ER_RateClass = Forwarding.AWB.Business.ExportAWBHeader.Constants.PrepaidCollect1CharCodes.Collect;
			totalWeightCOLRateLine.ER_Total = 21;

			shipment.AWBHeader.EH_ValuationPPD = 1;
			shipment.AWBHeader.EH_ValuationCOL = 2;

			shipment.AWBHeader.EH_TaxesPPD = 2;
			shipment.AWBHeader.EH_TaxesCOL = 3;

			AddOtherChargeToAWBHeader(21.3m, Core.Constants.AWB.ChargeCodes.MB, "Test", EntitlementCodes.Agent, true, shipment.AWBHeader);
			AddOtherChargeToAWBHeader(22.4m, Core.Constants.AWB.ChargeCodes.MB, "Test", EntitlementCodes.Agent, false, shipment.AWBHeader);

			AddOtherChargeToAWBHeader(23.5m, Core.Constants.AWB.ChargeCodes.MB, "Test", EntitlementCodes.Carrier, true, shipment.AWBHeader);
			AddOtherChargeToAWBHeader(24.6m, Core.Constants.AWB.ChargeCodes.MB, "Test", EntitlementCodes.Carrier, false, shipment.AWBHeader);
		}

		void PopulateAWBHeader(ForwardingShipment shipment)
		{
			shipment.AWBHeader.EH_ShipperName = "HARRIS   FORD LLC";
			shipment.AWBHeader.EH_ShipperAddress = "9307 EAST 56TH STREET";
			shipment.AWBHeader.EH_ShipperPlace = "INDIANAPOLIS";
			shipment.AWBHeader.EH_ShipperState = "IN";
			shipment.AWBHeader.EH_ShipperCountryCode = "US";
			shipment.AWBHeader.EH_ShipperPostCode = "46216";
			shipment.AWBHeader.EH_ShipperContactCode = "TE";
			shipment.AWBHeader.EH_ShipperContactDetail = "13175910000";
			shipment.AWBHeader.EH_ShipperTraderNoType = "USC";
			shipment.AWBHeader.EH_ShipperTraderNo = "7788";

			shipment.AWBHeader.EH_ConsigneeName = "WELLA MANUFACTURING GMBH";
			shipment.AWBHeader.EH_ConsigneeAddress = "WELLASTRASE 2-4";
			shipment.AWBHeader.EH_ConsigneePlace = "HUENFELD";
			shipment.AWBHeader.EH_ConsigneeState = "";
			shipment.AWBHeader.EH_ConsigneeCountryCode = "DE";
			shipment.AWBHeader.EH_ConsigneePostCode = "36088";
			shipment.AWBHeader.EH_ConsigneeContactCode = "TE";
			shipment.AWBHeader.EH_ConsigneeContactDetail = "49665279340";
			shipment.AWBHeader.EH_ConsigneeTraderNoType = "XYZ";
			shipment.AWBHeader.EH_ConsigneeTraderNo = "8899";

			shipment.AWBHeader.EH_AWBOriginCode = "AU";
			shipment.AWBHeader.EH_AirportOfDepartureAndRequestRouteText = "Notify Address";
			shipment.AWBHeader.EH_AirportOfDestinationCode = "AN";
			shipment.AWBHeader.EH_AirportOfDestinationText = "AN State";
			shipment.AWBHeader.EH_AlsoNotifyCountryCode = "KR";
			shipment.AWBHeader.EH_AlsoNotifyPostCode = "3321";
			shipment.AWBHeader.EH_AlsoNotifyContactCode = "PHO";
			shipment.AWBHeader.EH_AlsoNotifyContactDetail = "+55 (66) 44";
			shipment.AWBHeader.EH_AlsoNotifyTraderNo = "9900";

			shipment.AWBHeader.EH_Currency = "USD";
			shipment.AWBHeader.EH_WeightVPPDCOL = Forwarding.AWB.Business.ExportAWBHeader.Constants.PrepaidCollect3CharCodes.Collect;
			shipment.AWBHeader.EH_OtherPPDCOL = Forwarding.AWB.Business.ExportAWBHeader.Constants.PrepaidCollect3CharCodes.Collect;
			shipment.AWBHeader.EH_DeclaredValue = 0.00M;
			shipment.AWBHeader.EH_CustomsValue = 4289.68M;
			shipment.AWBHeader.EH_InsuranceValue = 0.00M;

			shipment.AWBHeader.EH_ShippingLoadAndCount = 4;
			shipment.AWBHeader.EH_To1st = "To";
			shipment.AWBHeader.EH_OptionalShippingInformation = "TERMS: FOB";
			shipment.AWBHeader.EH_OptionalShippingInformation2 = "Optonal Info 2";

			shipment.AWBHeader.EH_Currency = "AUD";
			shipment.AWBHeader.EH_ChargesCode = "PP";
			shipment.AWBHeader.EH_WeightPrepaidCollect = "5";
			shipment.AWBHeader.EH_OtherPrepaidCollect = "X";

			shipment.AWBHeader.EH_DeclaredValue = 15;
			shipment.AWBHeader.EH_HouseDeclaredValueCurrency = "USD";
			shipment.AWBHeader.EH_CustomsValue = 16;
			shipment.AWBHeader.EH_InsuranceValue = 12;
			shipment.AWBHeader.EH_AWBAgentsSignature = "222";
			shipment.AWBHeader.EH_ShippersSignature = "111";
			shipment.AWBHeader.EH_AWBIssueDate = new ZDateTime(2021, 01, 01);
			shipment.AWBHeader.EH_AWBIssuePlace = "TEST";
		}
		#endregion
	}
}
