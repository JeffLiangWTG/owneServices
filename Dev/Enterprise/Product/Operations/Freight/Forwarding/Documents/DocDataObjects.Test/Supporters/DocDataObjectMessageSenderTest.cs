using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.DocumentVisualizer.Business;
using Enterprise.DocumentVisualizer.Integration;
using Enterprise.Environment;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.Business.AWB;
using Enterprise.Freight.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.Documents.DataObjects.Testing
{
	abstract class DocDataObjectMessageSenderTest : TestCaseWithFactory
	{
		[TestDate]
		public virtual void TestSendMessage()
		{
			using (Factory.AddDisposableService())
			{
				TestDateAttribute.Date = new DateTime(2021, 1, 1);
				Env.Registry.Freight.AirWaybill.IssuingCarrierAgentName = "AGENT SIGNATURE";
				var bizObj = SetBusinessObject();
				var notifications = new NotificationsHandler();

				var res = MessageSender.SendMessage(bizObj, MenuItem, notifications);

				Assert("message has been sent", res);
				AssertEquals("no errors messages", 0, notifications.Notifications.Count);

				var documentDataStorageQuery = new ZQuery();
				documentDataStorageQuery.AddToFilter(JobDocumentDataSchema.JDD_Name, DocumentName);
				documentDataStorageQuery.AddToFilter(JobDocumentDataSchema.JDD_ParentID, bizObj.PK);

				var documentDataStorage = Factory.LoadTop1<IVisualizerDocumentData>(documentDataStorageQuery);

				AssertNotNull("document data has been created", documentDataStorage);

				var logs = (documentDataStorage as IStmALogParent)
					.Logs
					.GetAllLogs()
					.Cast<StmALog>()
					.Where(l => l.SL_SE_NKEvent == Events.MessageSentCode
						|| l.SL_SE_NKEvent == Events.DataExportCode)
					.ToArray();

				var msn = logs.Single(l => l.SL_SE_NKEvent == Events.MessageSentCode);
				AssertEquals("MSN reference", MSNReference, msn.SL_Reference);

				var dex = logs.Single(l => l.SL_SE_NKEvent == Events.DataExportCode);
				var message = dex.RelatedEDIMessage;
				AssertNotNull("EDI message has been created", message);
				AssertMultilineASCIIEquals("UXml", string.Format(ExpectedMessage, message.Message.Interchange.EI_SessionGUID, message.Message.Interchange.EI_InterchangeNum, message.Message.EM_MessageNum), message.Message.EM_MessageText);

				if (AllowSendMessageAmendment)
				{
					TestDateAttribute.Date = new DateTime(2021, 1, 2);

					res = MessageSender.SendMessage(bizObj, MenuItem, notifications);
					Assert("message has been sent", res);
					logs = (documentDataStorage as IStmALogParent)
						.Logs
						.GetAllLogs()
						.Cast<StmALog>()
						.OrderByDescending(log => log.SL_PostedTimeUtc)
						.ToArray();

					msn = logs.FirstOrDefault(l => l.SL_SE_NKEvent == Events.MessageSentCode);
					AssertEquals("MSN reference", MSNReference, msn.SL_Reference);

					dex = logs.FirstOrDefault(l => l.SL_SE_NKEvent == Events.DataExportCode);
					message = dex.RelatedEDIMessage;
					AssertNotNull("EDI message has been created", message);
					AssertMultilineASCIIEquals("UXml", string.Format(ExpectedAmendmentMessage, message.Message.Interchange.EI_SessionGUID, message.Message.Interchange.EI_InterchangeNum, message.Message.EM_MessageNum), message.Message.EM_MessageText);
				}
			}
		}

		#region Implementation
		protected abstract IDocDataObjectMessageSender MessageSender { get; }

		protected abstract BusinessObject SetBusinessObject();

		protected abstract ZGuid MenuItemPK { get; }

		protected IStmMenuItem MenuItem => menuItem ?? (menuItem = Factory.Load<VisualizerMenuItem>(MenuItemPK));
		IStmMenuItem menuItem;

		protected abstract ZString DocumentName { get; }

		protected abstract ZString MSNReference { get; }

		protected abstract ZString ExpectedMessage { get; }

		protected virtual ZString ExpectedAmendmentMessage => ExpectedMessage;

		protected virtual bool AllowSendMessageAmendment => false;

		protected ForwardingShipment CreateShipmentWithoutErrors(ZString awb, ZString uniqueConsignRef, ZString origin, ZString destination)
		{
			var consol = Factory.New<ForwardingConsol>();
			PopulateConsol(consol, awb, origin, destination, "C00001004");

			var shipment = consol.Shipments.AddNew();
			PopulateShipment(shipment, awb, origin, destination, uniqueConsignRef);
			PopulateAWBHeader(shipment);
			PopulateRateLines(shipment);
			PopulatePrepaidAndCollectValues(shipment);

			return shipment;
		}

		protected ForwardingConsol CreateConsolWithoutErrors(ZString awb, ZString uniqueConsignRef, ZString origin, ZString destination)
		{
			var consol = Factory.New<ForwardingConsol>();
			PopulateConsol(consol, awb, origin, destination, uniqueConsignRef);
			PopulateTransport(consol);
			var shipment = consol.Shipments.AddNew();
			PopulateShipment(shipment, awb, origin, destination, "S00001001");
			PopulateAWBHeader(shipment);
			PopulateRateLines(shipment);
			PopulatePrepaidAndCollectValues(shipment);
			var documentData = CreateDocumentData(shipment, "AdvancedCargoReportBR");
			AddLog("Advanced Cargo Report", documentData, Events.MessageAccepted, ZDateTimeOffset.UtcToday.AddHours(1));

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

		void AddLog(ZString docName, VisualizerDocumentData documentData, Event evenType, ZDateTimeOffset eventDateTime)
		{
			var paramList = new List<KeyValuePair<string, string>>
			{
				new KeyValuePair<string, string>("MST", docName),
				new KeyValuePair<string, string>("LOC", "BR"),
				new KeyValuePair<string, string>("DEP", "Customs")
			};

			documentData.Logs.AddNew(evenType, eventDateTime, paramList.ToArray());
		}

		VisualizerDocumentData CreateDocumentData(ForwardingShipment shipment, ZString docDataName)
		{
			var documentData = Factory.New<VisualizerDocumentData>();
			documentData.JDD_ParentTableCode = JobShipmentSchema.Constants.Prefix;
			documentData.JDD_ParentID = shipment.PK;
			documentData.JDD_Name = docDataName;

			return documentData;
		}

		void PopulateTransport(ForwardingConsol consol)
		{
			var transportLeg1 = consol.Transports[0];
			transportLeg1.JW_LegOrder = 1;
			transportLeg1.JW_TransportMode = Core.Constants.TransportModes.Air;
			transportLeg1.JW_RL_NKLoadPort = "AUSYD";
			transportLeg1.JW_RL_NKDiscPort = "NZAKL";

			var transportLeg2 = consol.Transports.AddNew();
			transportLeg2.JW_LegOrder = 2;
			transportLeg2.JW_TransportMode = Core.Constants.TransportModes.Air;
			transportLeg2.JW_RL_NKLoadPort = "NZAKL";
			transportLeg2.JW_RL_NKDiscPort = "CLSCL";

			var transportLeg3 = consol.Transports.AddNew();
			transportLeg3.JW_LegOrder = 3;
			transportLeg3.JW_TransportMode = Core.Constants.TransportModes.Air;
			transportLeg3.JW_RL_NKLoadPort = "CLSCL";
			transportLeg3.JW_RL_NKDiscPort = "BRSAO";
			transportLeg3.JW_ETD = ZDateTime.Today;
		}

		void PopulateConsol(ForwardingConsol consol, ZString mawb, ZString origin, ZString destination, ZString uniqueConsignRef)
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

		void PopulateShipment(ForwardingShipment shipment, ZString hawb, ZString origin, ZString destination, ZString uniqueConsignRef)
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

		void AddOtherChargeToAWBHeader(ZDecimal amount, ZString chargeCode, ZString description, EntitlementCodes entitlementCode, ZBool prePaid, AWB.Business.ExportAWBHeader awbHeader)
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
			var awbHeader = Factory.New<ShipmentExportAWBHeader>();
			awbHeader.EH_ParentID = shipment.PK;
			awbHeader.Populate();
			awbHeader.AWBRateLines[0].ER_WeightInLBsOrKGs = Core.Constants.AWB.RateLineUQ.Kilos;

			awbHeader.EH_ShipperName = "HARRIS   FORD LLC";
			awbHeader.EH_ShipperAddress = "9307 EAST 56TH STREET";
			awbHeader.EH_ShipperPlace = "INDIANAPOLIS";
			awbHeader.EH_ShipperState = "IN";
			awbHeader.EH_ShipperCountryCode = "US";
			awbHeader.EH_ShipperPostCode = "46216";
			awbHeader.EH_ShipperContactCode = "TE";
			awbHeader.EH_ShipperContactDetail = "13175910000";
			awbHeader.EH_ShipperTraderNoType = "USC";
			awbHeader.EH_ShipperTraderNo = "7788";

			awbHeader.EH_ConsigneeName = "WELLA MANUFACTURING GMBH";
			awbHeader.EH_ConsigneeAddress = "WELLASTRASE 2-4";
			awbHeader.EH_ConsigneePlace = "HUENFELD";
			awbHeader.EH_ConsigneeState = "";
			awbHeader.EH_ConsigneeCountryCode = "DE";
			awbHeader.EH_ConsigneePostCode = "36088";
			awbHeader.EH_ConsigneeContactCode = "TE";
			awbHeader.EH_ConsigneeContactDetail = "49665279340";
			awbHeader.EH_ConsigneeTraderNoType = "XYZ";
			awbHeader.EH_ConsigneeTraderNo = "8899";

			awbHeader.EH_AWBOriginCode = "AU";
			awbHeader.EH_AirportOfDepartureAndRequestRouteText = "Notify Address";
			awbHeader.EH_AirportOfDestinationCode = "AN";
			awbHeader.EH_AirportOfDestinationText = "AN State";
			awbHeader.EH_AlsoNotifyCountryCode = "KR";
			awbHeader.EH_AlsoNotifyPostCode = "3321";
			awbHeader.EH_AlsoNotifyContactCode = "PHO";
			awbHeader.EH_AlsoNotifyContactDetail = "+55 (66) 44";
			awbHeader.EH_AlsoNotifyTraderNo = "9900";

			awbHeader.EH_Currency = "USD";
			awbHeader.EH_WeightVPPDCOL = Forwarding.AWB.Business.ExportAWBHeader.Constants.PrepaidCollect3CharCodes.Collect;
			awbHeader.EH_OtherPPDCOL = Forwarding.AWB.Business.ExportAWBHeader.Constants.PrepaidCollect3CharCodes.Collect;
			awbHeader.EH_DeclaredValue = 0.00M;
			awbHeader.EH_CustomsValue = 4289.68M;
			awbHeader.EH_InsuranceValue = 0.00M;

			awbHeader.EH_ShippingLoadAndCount = 4;
			awbHeader.EH_To1st = "To";
			awbHeader.EH_OptionalShippingInformation = "TERMS: FOB";
			awbHeader.EH_OptionalShippingInformation2 = "Optonal Info 2";

			awbHeader.EH_Currency = "AUD";
			awbHeader.EH_ChargesCode = "PP";
			awbHeader.EH_WeightPrepaidCollect = "5";
			awbHeader.EH_OtherPrepaidCollect = "X";

			awbHeader.EH_DeclaredValue = 15;
			awbHeader.EH_HouseDeclaredValueCurrency = "USD";
			awbHeader.EH_CustomsValue = 16;
			awbHeader.EH_InsuranceValue = 12;

			awbHeader.EH_AWBIssueDate = Env.Time.GetUnlocoTimeFromUtc(shipment.JS_RL_NKDestination, ZDateTime.UtcNow.ToDateTime()).AddDays(-1);
		}
		#endregion
	}
}
