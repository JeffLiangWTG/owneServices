using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Freight.Forwarding.AWB.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Freight.Forwarding.AWB.Messaging.Testing
{
	public class AWBTestDataCreator : AWBTestDataCreator<ExportAWBHeader>
	{
		public AWBTestDataCreator(BusinessObjectFactory factory) : base(factory)
		{
		}
	}

	public class AWBTestDataCreator<T> where T : ExportAWBHeader
	{
		public T HAWBHeader1
		{
			get;
			private set;
		}

		public const string HAWBSampleFHL1 = @"
FHL/4
MBI/006-96667465DFWFRA/T4K245
HBS/CVGA00176739/LAXHAM/2/K245682/4/WILD CHERRY BAR
/EAW/PEF
TXT/WILD CHERRY BAR THIS DAMN BAR IS SOOOOO GOOD IF YOU EAT THIS ONE 
/THERE S NO WAY YOU LL WANT ANOTHER   
OCI/US/SHP/CT/13175910000
/DE/CNE/CT/49665279340
SHP/HARRIS   FORD LLC
/9307 EAST 56TH STREET ORANGE COUNTY
/LOS ANGELES/CA
/US/46216/TE/13175910000
CNE/WELLA MANUFACTURING GMBH
/WELLASTRASE 2-4 SCHLESWIG-HOLSTEIN
/HAMBURG
/DE/36088/TE/49665279340
CVD/EUR/CC/2000/4277.68/4800
";

		public const string HAWBSampleFHLForNatureOfGoods = @"
FHL/4
MBI/006-96667465DFWFRA/T4K245
HBS/CVGA00176739/LAXHAM/2/K245682/4/TEST SHORT DECR
/EAW/PEF
TXT/WILD CHERRY BAR THIS DAMN BAR IS SOOOOO GOOD IF YOU EAT THIS ONE 
/THERE S NO WAY YOU LL WANT ANOTHER   
OCI/US/SHP/CT/13175910000
/DE/CNE/CT/49665279340
SHP/HARRIS   FORD LLC
/9307 EAST 56TH STREET ORANGE COUNTY
/LOS ANGELES/CA
/US/46216/TE/13175910000
CNE/WELLA MANUFACTURING GMBH
/WELLASTRASE 2-4 SCHLESWIG-HOLSTEIN
/HAMBURG
/DE/36088/TE/49665279340
CVD/EUR/CC/2000/4277.68/4800
";

		public const string HAWBSampleFHLForHarmonisedCodes = @"
FHL/4
MBI/006-96667465DFWFRA/T4K245
HBS/CVGA00176739/LAXHAM/2/K245682/4/WILD CHERRY BAR
/EAW/PEF
TXT/WILD CHERRY BAR THIS DAMN BAR IS SOOOOO GOOD IF YOU EAT THIS ONE 
/THERE S NO WAY YOU LL WANT ANOTHER   
HTS/001234
/022222
/333888
/444444
/555555
/666666
/777777
/888888
/999999
OCI/US/SHP/CT/13175910000
/DE/CNE/CT/49665279340
SHP/HARRIS   FORD LLC
/9307 EAST 56TH STREET ORANGE COUNTY
/LOS ANGELES/CA
/US/46216/TE/13175910000
CNE/WELLA MANUFACTURING GMBH
/WELLASTRASE 2-4 SCHLESWIG-HOLSTEIN
/HAMBURG
/DE/36088/TE/49665279340
CVD/EUR/CC/2000/4277.68/4800
";

		public T SetupHAWBHeader1()
		{
			var awbHeader = (T)MAWBHeader.ChildBills.AddNew(typeof(T));

			awbHeader.EH_WayBillNumber = "CVGA00176739";
			awbHeader.EH_AWBOriginCode = "LAX";
			awbHeader.EH_AirportOfDestinationCode = "HAM";

			awbHeader.EH_ShipperName = "HARRIS   FORD LLC";
			awbHeader.EH_ShipperAddress = "9307 EAST 56TH STREET";
			awbHeader.EH_ShipperAddress2 = "ORANGE COUNTY";
			awbHeader.EH_ShipperPlace = "LOS ANGELES";
			awbHeader.EH_ShipperState = "CA";
			awbHeader.EH_ShipperCountryCode = "US";
			awbHeader.EH_ShipperPostCode = "46216";
			awbHeader.EH_ShipperContactCode = "TE";
			awbHeader.EH_ShipperContactDetail = "13175910000";
			awbHeader.EH_ShipperContactEmail = "shipper@wtg.com";

			awbHeader.EH_ConsigneeName = "WELLA MANUFACTURING GMBH";
			awbHeader.EH_ConsigneeAddress = "WELLASTRASE 2-4";
			awbHeader.EH_ConsigneeAddress2 = "SCHLESWIG-HOLSTEIN";
			awbHeader.EH_ConsigneePlace = "HAMBURG";
			awbHeader.EH_ConsigneeState = "";
			awbHeader.EH_ConsigneeCountryCode = "DE";
			awbHeader.EH_ConsigneePostCode = "36088";
			awbHeader.EH_ConsigneeContactCode = "TE";
			awbHeader.EH_ConsigneeContactDetail = "49665279340";
			awbHeader.EH_ConsigneeContactEmail = "consignee@google.com";

			awbHeader.EH_Currency = "EUR";
			awbHeader.EH_WeightVPPDCOL = ExportAWBHeader.Constants.PrepaidCollect3CharCodes.Collect;
			awbHeader.EH_OtherPPDCOL = ExportAWBHeader.Constants.PrepaidCollect3CharCodes.Collect;
			awbHeader.EH_DeclaredValue = 2000.00M;
			awbHeader.EH_CustomsValue = 4277.68M;
			awbHeader.EH_InsuranceValue = 4800.00M;

			var rateLine1 = awbHeader.AWBRateLines[0];
			rateLine1.ER_GrossWeight = 245682;
			rateLine1.ER_WeightInLBsOrKGs = Core.Constants.AWB.RateLineUQ.Kilos;
			rateLine1.ER_NoOfPiecesOrRCP = "2";

			awbHeader.EH_ShippingLoadAndCount = 4;

			awbHeader.AWBRateLine1.NatureAndQtyOfGoodsDescription = "WILD CHERRY BAR";
			awbHeader.AWBRateLine2.NatureAndQtyOfGoodsDescription = "THIS DAMN BAR IS SOOOOO GOOD";
			awbHeader.AWBRateLine3.NatureAndQtyOfGoodsDescription = "IF YOU EAT THIS ONE THERE'S";
			awbHeader.AWBRateLine4.NatureAndQtyOfGoodsDescription = "NO WAY YOU'LL WANT ANOTHER!!!";

			HAWBHeader1 = awbHeader;

			return awbHeader;
		}

		public AWBTestDataCreator(BusinessObjectFactory factory)
		{
			this.factory = factory;

			SetupEnvironment();

			MAWBHeader = GetSetupMAWBHeader();
		}

		readonly BusinessObjectFactory factory;

		public readonly T MAWBHeader;

		static void SetupEnvironment()
		{
			GlbCompany.CurrentCompany.GC_RN_NKCountryCode = "US";
			GlbBranch.CurrentBranch.GB_RL_NKHomePort = "USDFW";

			Env.Registry.Freight.AirWaybill.IssuingCarrierAgentAccountNumber = "";
			Env.Registry.Freight.AirWaybill.IssuingCarrierAgentIATACode = "33605250151";
			Env.Registry.Freight.AirWaybill.IssuingCarrierAgentName = "SEKO WORLDWIDE DFW";
			Env.Registry.Freight.AirWaybill.IssuingCarrierAgentCity = "DALLAS";
		}

		T GetSetupMAWBHeader()
		{
			var awbHeader = factory.New<T>();

			awbHeader.EH_WayBillNumber = "006-96667465";
			awbHeader.EH_AWBOriginCode = "DFW";
			awbHeader.EH_AirportOfDestinationCode = "FRA";

			awbHeader.EH_To1st = "ATL";
			awbHeader.EH_By1st = "DL";
			awbHeader.EH_Booking1stCarrier = "DL";
			awbHeader.EH_Booking1stFlight = "8226E";
			awbHeader.EH_Booking1stFlightDate = "08";
			awbHeader.EH_To2nd = "FRA";
			awbHeader.EH_By2nd = "DL";
			awbHeader.EH_Booking2ndCarrier = "DL";
			awbHeader.EH_Booking2ndFlight = "014";
			awbHeader.EH_Booking2ndFlightDate = "09";

			awbHeader.AWBSpecialHandlingItems.AddNew().EP_SpecialHandling = AWBSpecialHandlingCodeDescriptionPairList.Codes.EFreightConsignmentWithNoAccompanyingPaperDocuments;
			awbHeader.AWBSpecialHandlingItems.AddNew().EP_SpecialHandling = AWBSpecialHandlingCodeDescriptionPairList.Codes.Flowers;

			awbHeader.EH_ShipperName = "SEKO WORLDWIDE LLC - DALLAS";
			awbHeader.EH_ShipperAddress = "1840 W.AIRFIELD DRIVE  100";
			awbHeader.EH_ShipperAddress2 = "DALLAS";
			awbHeader.EH_ShipperPlace = "DFW AIRPORT";
			awbHeader.EH_ShipperState = "TX";
			awbHeader.EH_ShipperCountryCode = "US";
			awbHeader.EH_ShipperPostCode = "75261";
			awbHeader.EH_ShipperContactCode = "TE";
			awbHeader.EH_ShipperContactDetail = "19725748283";

			awbHeader.EH_ConsigneeName = "SEKO SYNERGY GMBH";
			awbHeader.EH_ConsigneeAddress = "CARGO CITY SOUTH";
			awbHeader.EH_ConsigneeAddress2 = "BLDG 556 D";
			awbHeader.EH_ConsigneePlace = "FRANKFURT";
			awbHeader.EH_ConsigneeState = "HE";
			awbHeader.EH_ConsigneeCountryCode = "DE";
			awbHeader.EH_ConsigneePostCode = "60549";
			awbHeader.EH_ConsigneeContactCode = "TE";
			awbHeader.EH_ConsigneeContactDetail = "49069697125510";

			awbHeader.EH_HandlingInformation = "PLEASE NOTIFY CONSIGNEE UPON ARRIVAL";

			var accountingInfo1 = awbHeader.AWBAccountingInformations.AddNew();
			accountingInfo1.EA_InformationID = "GEN";
			accountingInfo1.EA_Information = "SPOT  1208107465";

			awbHeader.EH_Currency = "USD";
			awbHeader.EH_ChargesCode = "PP";
			awbHeader.EH_WeightVPPDCOL = ExportAWBHeader.Constants.PrepaidCollect3CharCodes.Prepaid;
			awbHeader.EH_OtherPPDCOL = ExportAWBHeader.Constants.PrepaidCollect3CharCodes.Prepaid;
			awbHeader.EH_DeclaredValue = 0M;
			awbHeader.EH_CustomsValue = 0M;
			awbHeader.EH_InsuranceValue = 0M;

			var rateLine1 = awbHeader.AWBRateLines[0];
			rateLine1.ER_NoOfPiecesOrRCP = "4";
			rateLine1.ER_GrossWeight = 245;
			rateLine1.ER_WeightInLBsOrKGs = Core.Constants.AWB.RateLineUQ.Kilos;
			rateLine1.ER_RateClass = "Q";
			rateLine1.ER_ChargeableWeight = 245;
			rateLine1.ER_NoOfPiecesOrRCP = "4";
			rateLine1.ER_RateChargeOrDiscount = 3M;
			rateLine1.ER_NoOfPiecesOrRCP = "4";
			rateLine1.NatureAndQtyOfGoods.Text = "CONSOLIDATION AS PER ATTACHED LIST";

			var rateLine2 = awbHeader.AWBRateLine2;
			rateLine2.NatureAndQtyOfGoodsDescription = "4 SLAC";

			var rateLine3 = awbHeader.AWBRateLine3;
			rateLine3.NatureAndQtyOfGoodsDescription = "DIMS 13X12X15 IN X 2";

			var rateLine4 = awbHeader.AWBRateLine4;
			rateLine4.NatureAndQtyOfGoodsDescription = "DIMS 48X40X40 IN X 1";

			var rateLine5 = awbHeader.AWBRateLine5;
			rateLine5.NatureAndQtyOfGoodsDescription = "DIMS 12X11X16 IN X 1";

			awbHeader.EH_ShippingLoadAndCount = 4;

			var otherCharge1 = awbHeader.AWBOtherCharges.AddNew();
			otherCharge1.EO_ChargeCode = "MY";
			otherCharge1.EO_EntitlementCode = "C";
			otherCharge1.EO_Amount = 196.00M;

			var otherCharge2 = awbHeader.AWBOtherCharges.AddNew();
			otherCharge2.EO_ChargeCode = "SC";
			otherCharge2.EO_EntitlementCode = "C";
			otherCharge2.EO_Amount = 36.75M;

			var otherCharge3 = awbHeader.AWBOtherCharges.AddNew();
			otherCharge3.EO_ChargeCode = "XB";
			otherCharge3.EO_EntitlementCode = "C";
			otherCharge3.EO_Amount = 29.40M;

			awbHeader.EH_ShippersSignature = "SEKO WORLDWIDE DFW";
			awbHeader.EH_AWBIssueDate = new ZDateTime(2010, 12, 8);
			awbHeader.EH_AWBIssuePlace = "DALLAS-FORT WORTH";
			awbHeader.EH_AWBAgentsSignature = "JOSEPH RUFF";

			return awbHeader;
		}

		public static readonly string MAWBSampleFWB = @"
FWB/16
006-96667465DFWFRA/T4K245
FLT/DL8226E/08/DL014/09
RTG/ATLDL/FRADL
SHP
/SEKO WORLDWIDE LLC - DALLAS
/1840 W.AIRFIELD DRIVE  100 DALLAS
/DFW AIRPORT/TX
/US/75261/TE/19725748283
CNE
/SEKO SYNERGY GMBH
/CARGO CITY SOUTH BLDG 556 D
/FRANKFURT/HE
/DE/60549/TE/49069697125510
AGT//3360525/0151
/SEKO WORLDWIDE DFW
/DALLAS
ACC/GEN/SPOT  1208107465
CVD/USD/PP/PP/NVD/NCV/XXX
RTD/1/P4/K245/CQ/W245/R3/T735
/NG/CONSOLIDATION AS PER
/2/NG/ATTACHED LIST
/3/NS/4
/4/ND//INH13-12-15/2
/5/ND//INH48-40-40/1
/6/ND//INH12-11-16/1
OTH/P/MYC196
/P/SCC36.75
/P/XBC29.4
PPD/WT735
/OC262.15/CT997.15
CER/SEKO WORLDWIDE DFW
ISU/08DEC10/DALLAS-FORT WORTH/JOSEPH RUFF
OSI/PLEASE NOTIFY CONSIGNEE UPON ARRIVAL
REF//MWB00696667465/FFW/CWID" + GlbCompany.CurrentCompany.LicenceKeyIdentifier + @"/DFW
SPH/EAW/PEF
OCI/US/SHP/CT/19725748283
/DE/CNE/CT/49069697125510
";

		public ExportAWBHeader HAWBHeader2
		{
			get;
			private set;
		}

		public const string HAWBSampleFHL2 = @"
FHL/4
MBI/006-96667465DFWFRA/T4K245
HBS/CVGA00177173/DFWFRA/2/K212/1/BOTANICAL EXTRA
/EAW/PEF
TXT/BOTANICAL EXTRA
OCI/US/SHP/CT/13175910001
/DE/CNE/CT/49665279341
SHP/SOMEONE ELSE
/789 WEST 10TH STREET
/INDIANAPOLIS/IN
/US/46217/TE/13175910001
CNE/WILLY MANUFACTURING GMBH
/WALLASTRASE 2-4
/HUENFELD
/DE/36089/TE/49665279341
CVD/USD/CP/NVD/3396.6/XXX";

		public ExportAWBHeader SetupHAWBHeader2()
		{
			var awbHeader = MAWBHeader.ChildBills.AddNew();

			awbHeader.EH_WayBillNumber = "CVGA00177173";
			awbHeader.EH_AWBOriginCode = "DFW";
			awbHeader.EH_AirportOfDestinationCode = "FRA";

			awbHeader.EH_ShipperName = "SOMEONE ELSE";
			awbHeader.EH_ShipperAddress = "789 WEST 10TH STREET";
			awbHeader.EH_ShipperPlace = "INDIANAPOLIS";
			awbHeader.EH_ShipperState = "IN";
			awbHeader.EH_ShipperCountryCode = "US";
			awbHeader.EH_ShipperPostCode = "46217";
			awbHeader.EH_ShipperContactCode = "TE";
			awbHeader.EH_ShipperContactDetail = "13175910001";

			awbHeader.EH_ConsigneeName = "WILLY MANUFACTURING GMBH";
			awbHeader.EH_ConsigneeAddress = "WALLASTRASE 2-4";
			awbHeader.EH_ConsigneePlace = "HUENFELD";
			awbHeader.EH_ConsigneeState = "";
			awbHeader.EH_ConsigneeCountryCode = "DE";
			awbHeader.EH_ConsigneePostCode = "36089";
			awbHeader.EH_ConsigneeContactCode = "TE";
			awbHeader.EH_ConsigneeContactDetail = "49665279341";

			awbHeader.EH_Currency = "USD";
			awbHeader.EH_WeightVPPDCOL = ExportAWBHeader.Constants.PrepaidCollect3CharCodes.Collect;
			awbHeader.EH_OtherPPDCOL = ExportAWBHeader.Constants.PrepaidCollect3CharCodes.Prepaid;
			awbHeader.EH_DeclaredValue = 0.00m;
			awbHeader.EH_CustomsValue = 3396.6M;
			awbHeader.EH_InsuranceValue = 0.00M;

			var rateLine1 = awbHeader.AWBRateLines[0];
			rateLine1.ER_GrossWeight = 212;
			rateLine1.ER_WeightInLBsOrKGs = Core.Constants.AWB.RateLineUQ.Kilos;
			rateLine1.ER_NoOfPiecesOrRCP = "2";

			awbHeader.EH_ShippingLoadAndCount = 1;
			awbHeader.AWBRateLine1.NatureAndQtyOfGoodsDescription = "BOTANICAL EXTRA";

			return HAWBHeader2 = awbHeader;
		}
	}
}
