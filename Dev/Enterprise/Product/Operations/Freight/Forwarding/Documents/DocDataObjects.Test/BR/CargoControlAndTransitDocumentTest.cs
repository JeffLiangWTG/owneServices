using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentVisualizer.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.Business.AWB;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using ExportAWBHeader = Enterprise.Freight.Forwarding.Business.AWB.ExportAWBHeader;

namespace Enterprise.Freight.Forwarding.Documents.Testing
{
	[TestDate(2023, 09, 20)]
	sealed class CargoControlAndTransitDocumentTest : DocumentVisualizer.Testing.StandardDocumentContentTest
	{
		public override void TestDocumentContent()
		{
			var consol = Factory.New<ForwardingConsol>();
			PopulateConsol(consol, "081001");

			var shipment = consol.Shipments.AddNew();
			PopulateShipment(shipment, "HAWB", 12, 25, "goods", "Signature", "AUSYD", "BRSAO", ZDateTime.UtcNow, "notes");

			PopulateRateLines(shipment);
			PopulatePrepaidAndCollectValues(shipment);

			var cctShipmentReportMenuItemQuery = new ZQuery(StmMenuItemSchema.SU_MenuName, "CCT Shipment Report");
			var cctShipmentReportMenuItem = Factory.Load<StmMenuItem>(cctShipmentReportMenuItemQuery);

			Assert("There's no menu with name 'CCT Shipment Report'", cctShipmentReportMenuItem.Length >= 1);
			AssertEquals("There's more than one menu with name 'CCT Shipment Report'", 1, cctShipmentReportMenuItem.Length);

			var cctShipmentReportMenuTemplatePivotQuery = new ZQuery(StmMenuTemplatePivotSchema.SI_SU, cctShipmentReportMenuItem.First().PK);
			var cctShipmentReportMenuTemplatePivot = Factory.Load<VisualizerMenuTemplatePivot>(cctShipmentReportMenuTemplatePivotQuery);

			using (FreightDataRegistry.Instance.AllowAutoCalculationOfInternationalTax.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				AssertContents(shipment, cctShipmentReportMenuTemplatePivot.Single(), CreateContent());
			}
		}

		string CreateContent()
		{
			return $@"[2,3] 081
[2,6] SYD
[2,9] 001
[2,37] HAWB No: HAWB
[4,3] Shipper's Name and Address
[4,30] Cargo Control and Transit (CCT)
[6,3] CONSIGNOR
UNIT52
DORCUS YAMADAI
SYDNEY NSW 2017
AUSTRALIA
[6,29] Special Handling Codes
[7,29] Code
[7,35] Description
[11,3] Consignee's Name and Address
[13,3] CONSIGNEE
801
PRISMOGNATHUS DELISLEI
SOMEWHERE CA 10043
UNITED STATES
[15,29] Special Service Request:
[16,29] Other Service Information:
[17,3] Import Agent
[17,18] CNPJ
[17,29] Export Agent
[22,3] Airport of Departure (Addr. of First Carrier) and Requested Routing
[22,40] Optional Shipping Information
[23,3] Sydney
[23,39] TERMS: FOB
[24,3] To
[24,28] Currency
[24,33] CHGS
[24,35] WT/VAL
[24,37] Other
[24,40] Declared Value for Carriage
[24,44] Declared Value for Customs
[25,33] Code
[25,35] PPD
[25,36] COLL
[25,37] PPD
[25,39] COLL
[26,3] SAO
[26,29] AUD
[26,33] CP
[26,36] C
[26,39] P
[26,40] NVD
[26,44] NCV
[27,3] Airport of Destination
[27,29] Amount of insurance
[27,38] Presence of Solid Wooden Parts and Pieces☐
[28,3] Sao Paulo
[28,29] 0.00 AUD
[34,3] No. Of
Pieces
RCP
[34,6] Gross 
Weight
[34,11] kg
lb
[34,13] Rate Class
[34,20] Chargeable
Weight
[34,25]      Rate

                    Charge
[34,32] Total
[34,42] Nature and Quantity of Goods
(incl. Dimensions or Volume)
[35,14] Commodity
Item No.
[36,3] 25
[36,6] 12.0
[36,11] K
[36,13] N
[36,20] 12.0
[36,42] notes
[48,2] TotalNoOfPieces
[48,3] 35.00
[48,6] 20.00
[48,32] 60.00
[50,5] Prepaid
[50,10] Weight Charge
[50,18] Collect
[50,23] AdditionalNumbers
Unique Consignment Reference(RUC):
[51,3] 0.00
[51,14] 60.00
[51,23] Destination WH Code:
[54,12] Tax
[55,3] 30.00
[55,14] 40.00
[60,23] CARGOWISE SUPPORT
[61,23] Signature of Shipper or his Agent
[62,5] Total Prepaid
[62,16] Total Collect
[63,3] 331.00
[63,14] 502.00
[64,23] 20-Sep-23
[64,32] BRISBANE
[66,23]  Executed on (date)
[66,35] at (place)
[66,43] Signature of Issuing Carrier or its Agent  
[70,3] Cargo 
[70,40] Created by
";
		}

		#region Implementation

		void PopulateConsol(ForwardingConsol consol, ZString mawb)
		{
			consol.JK_TransportMode = Core.Constants.TransportModes.Air;
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "BRSAO";
			consol.JK_MasterBillNum = mawb;
			consol.JK_UniqueConsignRef = "C00001004";
		}

		void PopulateShipment(ForwardingShipment shipment, ZString hawb, ZDecimal weight, ZInt packs, ZString goodsDescription, ZString signature, ZString origin, ZString destination, ZDateTime dateIssue, ZString notes)
		{
			var shipper = GetShipper();
			shipment.ConsignorDocumentaryAddress.E2_OA_Address = shipper.MainAddress.PK;

			var consignee = GetConsignee();
			shipment.ConsigneeDocumentaryAddress.E2_OA_Address = consignee.MainAddress.PK;

			shipment.JS_TransportMode = Core.Constants.TransportModes.Air;
			shipment.JS_RL_NKOrigin = origin;
			shipment.JS_RL_NKDestination = destination;
			shipment.JS_UniqueConsignRef = "S54625711";

			shipment.JS_HouseBill = hawb;
			shipment.JS_ActualWeight = weight;
			shipment.JS_UnitOfWeight = Core.Constants.Weight.Kilograms;
			shipment.JS_GoodsDescription = goodsDescription;
			shipment.JS_ShipmentType = Core.Constants.ShipmentTypes.StandardHouse;
			shipment.JS_OuterPacks = packs;
			shipment.Notes.AddNew(true, PredefinedNoteTypes.Instance.DetailedGoodsDescription.Description, notes);

			shipment.AWBHeader.EH_AWBOriginCode = "";
			shipment.AWBHeader.EH_AirportOfDepartureAndRequestRouteText = "";
			shipment.AWBHeader.EH_To1st = "";
			shipment.AWBHeader.EH_AirportOfDestinationText = "";
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

			shipment.AWBHeader.EH_AWBAgentsSignature = signature;
			shipment.AWBHeader.EH_AWBIssueDate = dateIssue;
			shipment.AWBHeader.EH_AWBIssuePlace = "";
			shipment.AWBHeader.EH_ShippersSignature = "Signature";
		}

		void PopulatePrepaidAndCollectValues(ForwardingShipment shipment)
		{
			var totalWeightPPDRateLine = (ShipmentExportAWBRateLine)shipment.AWBHeader.AWBRateLines.AddNew();
			totalWeightPPDRateLine.ER_RateClass = ExportAWBHeader.Constants.PrepaidCollect1CharCodes.Prepaid;
			totalWeightPPDRateLine.ER_Total = 20;

			var totalWeightCOLRateLine = (ShipmentExportAWBRateLine)shipment.AWBHeader.AWBRateLines.AddNew();
			totalWeightCOLRateLine.ER_RateClass = ExportAWBHeader.Constants.PrepaidCollect1CharCodes.Collect;
			totalWeightCOLRateLine.ER_Total = 21;

			shipment.AWBHeader.EH_ValuationPPD = 1;
			shipment.AWBHeader.EH_ValuationCOL = 2;

			shipment.AWBHeader.EH_TaxesPPD = 2;
			shipment.AWBHeader.EH_TaxesCOL = 3;

			var client = Factory.NewWithValidTestData<OrgHeader>();
			var debtor = Factory.NewWithValidTestData<OrgHeader>();

			var job = CreateJobHeader(shipment.PK, client, debtor);

			var taxRate = AccTaxRate.CreateTaxRate_ForTestOnly(Factory);
			taxRate.SetRate_ForTestOnly(10, 1, endTaxDate: ZDate.Today);
			taxRate.SetRate_ForTestOnly(20, 1, ZDate.Today.AddDays(1));

			var charge1 = AddCharge(job, Factory.New<AccChargeCode>().PK, 300);
			charge1.JR_RX_NKSellCurrency = "UAH";
			charge1.JR_InvoiceType = InvoiceTypesList.Codes.ForeignCurrencyInvoice;
			charge1.JR_AT_SellGSTRate = taxRate.PK;

			var charge2 = AddCharge(job, Factory.New<AccChargeCode>().PK, 400);
			charge2.JR_OH_SellAccount = job.AgentCollectPK;
			charge2.JR_RX_NKSellCurrency = "UAH";
			charge2.JR_InvoiceType = InvoiceTypesList.Codes.ForeignCurrencyInvoice;
			charge2.JR_AT_SellGSTRate = taxRate.PK;

			AddOtherChargeToAWBHeader(21.3m, Core.Constants.AWB.ChargeCodes.MB, "Test", EntitlementCodes.Agent, true, shipment.AWBHeader);
			AddOtherChargeToAWBHeader(22.4m, Core.Constants.AWB.ChargeCodes.MB, "Test", EntitlementCodes.Agent, false, shipment.AWBHeader);

			AddOtherChargeToAWBHeader(23.5m, Core.Constants.AWB.ChargeCodes.MB, "Test", EntitlementCodes.Carrier, true, shipment.AWBHeader);
			AddOtherChargeToAWBHeader(24.6m, Core.Constants.AWB.ChargeCodes.MB, "Test", EntitlementCodes.Carrier, false, shipment.AWBHeader);
		}

		OrgHeader GetShipper()
		{
			var shipper = Factory.New<OrgHeader>();

			shipper.OH_FullName = "Consignor";
			shipper.OH_RL_NKClosestPort = "AUSYD";
			shipper.MainAddress.Address1 = "Unit52";
			shipper.MainAddress.Address2 = "Dorcus yamadai";
			shipper.MainAddress.City = "Sydney";
			shipper.MainAddress.Postcode = "2017";
			shipper.MainAddress.OA_RN_NKCountryCode = "AU";

			return shipper;
		}

		OrgHeader GetConsignee()
		{
			var consignee = Factory.New<OrgHeader>();
			consignee.OH_FullName = "Consignee";
			consignee.OH_RL_NKClosestPort = "USLAX";
			consignee.MainAddress.Address1 = "801";
			consignee.MainAddress.Address2 = "Prismognathus delislei";
			consignee.MainAddress.City = "Somewhere";
			consignee.MainAddress.Postcode = "10043";
			consignee.MainAddress.OA_RN_NKCountryCode = "US";

			return consignee;
		}

		void PopulateRateLines(ForwardingShipment shipment)
		{
			AddRateLineToAWBHeader("3", 1, "K", "Q", "N", 256, 1, 5, "VOL 122.000 M3", shipment);
			AddRateLineToAWBHeader("2", 3, "K", "Q", "N", 124, 3, 11, "VOL 109.020 M3", shipment);
			AddRateLineToAWBHeader("5", 4, "K", "Q", "N", 81, 2, 3, "VOL 81.81 M3", shipment);
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

		void AddOtherChargeToAWBHeader(ZDecimal amount, ZString chargeCode, ZString description, EntitlementCodes entitlementCode, ZBool prePaid, ExportAWBHeader awbHeader)
		{
			var otherCharge = awbHeader.AWBOtherCharges.AddNew();
			otherCharge.EO_Amount = amount;
			otherCharge.EO_ChargeCode = chargeCode;
			otherCharge.EO_ChargeDescription = description;
			otherCharge.EO_EntitlementCode = entitlementCode == EntitlementCodes.Agent ? Core.Constants.AWB.EntitlementCode.Agent : Core.Constants.AWB.EntitlementCode.Carrier;
			otherCharge.EO_PPDCLT = prePaid ? "PPD" : "COL";
		}

		JobHeader CreateJobHeader(ZGuid shipmentPK, OrgHeader client, OrgHeader debtor)
		{
			var job = Factory.NewJobForTesting<JobHeader>();
			job.JH_ParentTableCode = JobShipmentSchema.Constants.Prefix;
			job.JH_ParentID = shipmentPK;
			job.JH_JobNum = "Phony number";
			job.JH_OA_AgentCollectAddr = debtor != null ? debtor.MainAddress.PK : ZGuid.Empty;
			job.JH_OA_LocalChargesAddr = client != null ? client.MainAddress.PK : ZGuid.Empty;
			job.JH_GB = GlbBranch.CurrentBranch.PK;
			job.JH_GE = GlbDepartment.CurrentDepartment.PK;

			return job;
		}

		JobCharge AddCharge(JobHeader job, ZGuid chargeCodePK, ZDecimal localSellAmt)
		{
			var charge = Factory.New<JobCharge>();
			charge.JR_JH = job.PK;
			charge.JR_AC = chargeCodePK;
			charge.JR_GB = job.JH_GB;
			charge.JR_GE = job.JH_GE;
			charge.JR_LocalSellAmt = localSellAmt;

			return charge;
		}

		#endregion
	}
}
