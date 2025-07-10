using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Customs.Common;
using Enterprise.Environment;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.AWB.Business;
using Enterprise.Freight.Forwarding.AWB.Messaging;
using Enterprise.Freight.Forwarding.Registry;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;
using Enterprise.Registry.Business.Customs.US;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Core.Constants;

namespace Enterprise.Freight.Forwarding.Business.AWB.Testing
{
	sealed class FWBTest : Forwarding.AWB.Messaging.Testing.FBaseTest
	{
		public void TestFWB16()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.UnitedStates))
			{
				var expectedFWB16 = @"
FWB/16
006-96667465DFWFRA/T4K245
FLT/DL8226E/08/DL014/09
RTG/ATLDL/FRADL
SHP
/SEKO WORLDWIDE LLC - DALLAS
/1840 W.AIRFIELD DRIVE  100
/DFW AIRPORT/TX
/US/75261/TE/19725748283
CNE
/SEKO SYNERGY GMBH
/CARGO CITY SOUTH   BLDG 556 D
/FRANKFURT/HE
/DE/60549/TE/49069697125510
AGT//3360525/0151
/SEKO WORLDWIDE DFW
/DALLAS
NFY/MR ALSO NOTIFY
/NOTIFY ADDRESS
/AN PLACE/AN STATE
/KR/3321/PHO/556644
ACC/GEN/SPOT  1208107465
CVD/USD/PP/PP/NVD/NCV/XXX
RTD/1/P4/K245/CQ/W245/R3/T735
/NG/CONSOLIDATION AS PER
/2/NG/ATTACHED LIST
/3/NG/DIMS 13X12X15 IN X 2
/4/NG/DIMS 48X40X40 IN X 1
/5/NG/DIMS 12X11X16 IN X 1
/6/NG/GOODS ORIGIN  AU
/7/NG/4 SLAC
/8/ND//NDA
OTH/P/MYC196
/P/SCC36.75
/P/XBC29.4
PPD/WT735
/OC262.15/CT997.15
CER/SEKO WORLDWIDE DFW
ISU/08DEC10/DALLAS-FORT WORTH/JOSEPH RUFF
OSI/PLEASE NOTIFY CONSIGNEE UPON ARRIVAL
REF//C00141929/FFW/CWID" + GlbCompany.CurrentCompany.LicenceKeyIdentifier + @"/DFW
SPH/NSC/EAW/PEF/SCO
OCI/AU/ISS/RA/RANUM123
///ED/0115
///AC/DURBLE
///AC/BURBLE
///KC/JUBBLE
///ED/1299
//OSS/RA/GLUBBLE
///ED/1299
///L/BIO
///L/HLD
///SM/PRT
///SN/RUSSELL DUSSEL
///SD/13FEB151630
///ST/EAGLE DATAMATION INTERNATIONAL HAS
///ST/REVIEWED ALL AVAILABLE
///ST/DOCUMENTATION AND HAS DETERMINED
///ST/THAT NONE OF THE CARGO BEING
///ST/OFFERED IN THIS CONSIGNMENT OR
///ST/CONSOLIDATION HAS EITHER
///ST/ORIGINATED IN TRANSFERRED FROM OR
///ST/TRANSITED THROUGH ANY POINT IN
///ST/EGYPT SOMALIA SYRIAN ARAB REPUBLIC
///ST/YEMEN. THIS IS THE TEST ADDITIONAL
///ST/SECURITY INFORMATION IF ITS LENGTH
///ST/EXCEEDS 35 THEN IT SHOULD BE SPLIT
///ST/INTO MULTIPLE LINES
///ST/WRAPPED TO A NEW LINE FOR EACH 35
///ST/CHARACTERS
///ST/UNLESS A NEW LINE IS ACTUALLY
///ST/THERE   WHITESPACE   TESTING
///ST/THIS
/US/SHP/T/XYZ2233
/US/SHP/CT/19725748283
/DE/CNE/T/4455
/DE/CNE/CT/49069697125510
/KR/NFY/T/USC6677
/US/DNR/D/UN1234
/US/DNR/D/UN2345
";

				var expectedFWB16WithNatureAndQtyOfGoodsType = @"
FWB/16
006-96667465DFWFRA/T4K245
FLT/DL8226E/08/DL014/09
RTG/ATLDL/FRADL
SHP
/SEKO WORLDWIDE LLC - DALLAS
/1840 W.AIRFIELD DRIVE  100
/DFW AIRPORT/TX
/US/75261/TE/19725748283
CNE
/SEKO SYNERGY GMBH
/CARGO CITY SOUTH   BLDG 556 D
/FRANKFURT/HE
/DE/60549/TE/49069697125510
AGT//3360525/0151
/SEKO WORLDWIDE DFW
/DALLAS
NFY/MR ALSO NOTIFY
/NOTIFY ADDRESS
/AN PLACE/AN STATE
/KR/3321/PHO/556644
ACC/GEN/SPOT  1208107465
CVD/USD/PP/PP/NVD/NCV/XXX
RTD/1/P4/K245/CQ/W245/R3/T735
/NC/CONSOLIDATION AS PER
/2/NC/ATTACHED LIST
/3/ND//INH13-12-15/2
/4/ND//INH48-40-40/1
/5/ND//INH12-11-16/1
/6/NO/AU
/7/NS/4
OTH/P/MYC196
/P/SCC36.75
/P/XBC29.4
PPD/WT735
/OC262.15/CT997.15
CER/SEKO WORLDWIDE DFW
ISU/08DEC10/DALLAS-FORT WORTH/JOSEPH RUFF
OSI/PLEASE NOTIFY CONSIGNEE UPON ARRIVAL
REF//C00141929/FFW/CWID" + GlbCompany.CurrentCompany.LicenceKeyIdentifier + @"/DFW
SPH/NSC/EAW/PEF/SCO
OCI/AU/ISS/RA/RANUM123
///ED/0115
///AC/DURBLE
///AC/BURBLE
///KC/JUBBLE
///ED/1299
//OSS/RA/GLUBBLE
///ED/1299
///L/BIO
///L/HLD
///SM/PRT
///SN/RUSSELL DUSSEL
///SD/13FEB151630
///ST/EAGLE DATAMATION INTERNATIONAL HAS
///ST/REVIEWED ALL AVAILABLE
///ST/DOCUMENTATION AND HAS DETERMINED
///ST/THAT NONE OF THE CARGO BEING
///ST/OFFERED IN THIS CONSIGNMENT OR
///ST/CONSOLIDATION HAS EITHER
///ST/ORIGINATED IN TRANSFERRED FROM OR
///ST/TRANSITED THROUGH ANY POINT IN
///ST/EGYPT SOMALIA SYRIAN ARAB REPUBLIC
///ST/YEMEN. THIS IS THE TEST ADDITIONAL
///ST/SECURITY INFORMATION IF ITS LENGTH
///ST/EXCEEDS 35 THEN IT SHOULD BE SPLIT
///ST/INTO MULTIPLE LINES
///ST/WRAPPED TO A NEW LINE FOR EACH 35
///ST/CHARACTERS
///ST/UNLESS A NEW LINE IS ACTUALLY
///ST/THERE   WHITESPACE   TESTING
///ST/THIS
/US/SHP/T/XYZ2233
/US/SHP/CT/19725748283
/DE/CNE/T/4455
/DE/CNE/CT/49069697125510
/KR/NFY/T/USC6677
/US/DNR/D/UN1234
/US/DNR/D/UN2345";
				GlbBranch.CurrentBranch.GB_RL_NKHomePort = "USDFW";
				var dataCreator = new AWBHeaderTestDataCreator(Factory);
				var actualFWB16 = string.Empty;
				using (FreightDataRegistry.Instance.SendFWBNatureAndQuantityOfGoodsType.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
				{
					actualFWB16 = new FWB(new FWBMessageDetails(dataCreator.ConsolAWBHeader), FWB.Version.No16, true).ToString();
					AssertEquals(ConsolAWBHeader.AdditionalSecurityInformation, ConsolAWBHeader.EH_AdditionalSecurityInformation);
					AssertMultilineASCIIEquals("FWB Message as Version 16", expectedFWB16.Trim(), actualFWB16.Trim());
				}
				using (FreightDataRegistry.Instance.SendFWBNatureAndQuantityOfGoodsType.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
				{
					actualFWB16 = new FWB(new FWBMessageDetails(dataCreator.ConsolAWBHeader), FWB.Version.No16, true).ToString();
					AssertMultilineASCIIEquals("FWB Message as Version 16", expectedFWB16WithNatureAndQtyOfGoodsType.Trim(), actualFWB16.Trim());
				}

				var expectedFWB16WithoutSecurityDeclaration = @"
FWB/16
006-96667465DFWFRA/T4K245
FLT/DL8226E/08/DL014/09
RTG/ATLDL/FRADL
SHP
/SEKO WORLDWIDE LLC - DALLAS
/1840 W.AIRFIELD DRIVE  100
/DFW AIRPORT/TX
/US/75261/TE/19725748283
CNE
/SEKO SYNERGY GMBH
/CARGO CITY SOUTH   BLDG 556 D
/FRANKFURT/HE
/DE/60549/TE/49069697125510
AGT//3360525/0151
/SEKO WORLDWIDE DFW
/DALLAS
NFY/MR ALSO NOTIFY
/NOTIFY ADDRESS
/AN PLACE/AN STATE
/KR/3321/PHO/556644
ACC/GEN/SPOT  1208107465
CVD/USD/PP/PP/NVD/NCV/XXX
RTD/1/P4/K245/CQ/W245/R3/T735
/NC/CONSOLIDATION AS PER
/2/NC/ATTACHED LIST
/3/ND//INH13-12-15/2
/4/ND//INH48-40-40/1
/5/ND//INH12-11-16/1
/6/NO/AU
/7/NS/4
OTH/P/MYC196
/P/SCC36.75
/P/XBC29.4
PPD/WT735
/OC262.15/CT997.15
CER/SEKO WORLDWIDE DFW
ISU/08DEC10/DALLAS-FORT WORTH/JOSEPH RUFF
OSI/PLEASE NOTIFY CONSIGNEE UPON ARRIVAL
REF//C00141929/FFW/CWID" + GlbCompany.CurrentCompany.LicenceKeyIdentifier + @"/DFW
SPH/NSC/EAW/PEF/SCO
OCI///ST/EAGLE DATAMATION INTERNATIONAL HAS
///ST/REVIEWED ALL AVAILABLE
///ST/DOCUMENTATION AND HAS DETERMINED
///ST/THAT NONE OF THE CARGO BEING
///ST/OFFERED IN THIS CONSIGNMENT OR
///ST/CONSOLIDATION HAS EITHER
///ST/ORIGINATED IN TRANSFERRED FROM OR
///ST/TRANSITED THROUGH ANY POINT IN
///ST/EGYPT SOMALIA SYRIAN ARAB REPUBLIC
///ST/YEMEN.
/US/SHP/T/XYZ2233
/US/SHP/CT/19725748283
/DE/CNE/T/4455
/DE/CNE/CT/49069697125510
/KR/NFY/T/USC6677
/US/DNR/D/UN1234
/US/DNR/D/UN2345";

				actualFWB16 = new FWB(new FWBMessageDetails(dataCreator.ConsolAWBHeader), FWB.Version.No16, false).ToString();
				AssertMultilineASCIIEquals("FWB Message as Version 16, Security Declaration is not included. TSA Security Statement should be printed once, but Additional Security Information should not.", expectedFWB16WithoutSecurityDeclaration.Trim(), actualFWB16.Trim());
			}	
		}

		public void TestFWBAdditionalSecurityInformation_AU()
		{
			using (FreightDataRegistry.Instance.AWBSecurityStatementTextToUSAAndUSTerritories.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "this is the test security statement, it shoud not    contain more than one space.  Let's see the result!"))
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.Australia))
			{
				var expectedFWB16 = @"
FWB/16
006-96667465DFWFRA/T4K245
FLT/DL8226E/08/DL014/09
RTG/ATLDL/FRADL
SHP
/SEKO WORLDWIDE LLC - DALLAS
/1840 W.AIRFIELD DRIVE  100
/DFW AIRPORT/TX
/US/75261/TE/19725748283
CNE
/SEKO SYNERGY GMBH
/CARGO CITY SOUTH   BLDG 556 D
/FRANKFURT/HE
/DE/60549/TE/49069697125510
AGT//3360525/0151
/SEKO WORLDWIDE DFW
/DALLAS
NFY/MR ALSO NOTIFY
/NOTIFY ADDRESS
/AN PLACE/AN STATE
/KR/3321/PHO/556644
ACC/GEN/SPOT  1208107465
CVD/USD/PP/PP/NVD/NCV/XXX
RTD/1/P4/K245/CQ/W245/R3/T735
/NG/CONSOLIDATION AS PER
/2/NG/ATTACHED LIST
/3/NG/DIMS 13X12X15 IN X 2
/4/NG/DIMS 48X40X40 IN X 1
/5/NG/DIMS 12X11X16 IN X 1
/6/NG/GOODS ORIGIN  AU
/7/NG/4 SLAC
/8/ND//NDA
OTH/P/MYC196
/P/SCC36.75
/P/XBC29.4
PPD/WT735
/OC262.15/CT997.15
CER/SEKO WORLDWIDE DFW
ISU/08DEC10/DALLAS-FORT WORTH/JOSEPH RUFF
OSI/PLEASE NOTIFY CONSIGNEE UPON ARRIVAL
REF//C00141929/FFW/CWID" + GlbCompany.CurrentCompany.LicenceKeyIdentifier + @"/SYD
SPH/EAW/PEF/SCO
OCI/AU/ISS/RA/RANUM123
///ED/0115
///AC/DURBLE
///AC/BURBLE
///KC/JUBBLE
///ED/1299
//OSS/RA/GLUBBLE
///ED/1299
///L/BIO
///L/HLD
///SM/PRT
///SN/RUSSELL DUSSEL
///SD/13FEB151630
///ST/CARGO HAS BEEN EXAMINED IN
///ST/ACCORDANCE WITH A REGULATION
///ST/4.41JA NOTICE.
///ST/THIS IS THE TEST SECURITY
///ST/STATEMENT IT SHOUD NOT CONTAIN
///ST/MORE THAN ONE SPACE. LET S SEE THE
///ST/RESULT THIS IS THE TEST ADDITIONAL
///ST/SECURITY INFORMATION IF ITS LENGTH
///ST/EXCEEDS 35 THEN IT SHOULD BE SPLIT
///ST/INTO MULTIPLE LINES
///ST/WRAPPED TO A NEW LINE FOR EACH 35
///ST/CHARACTERS
///ST/UNLESS A NEW LINE IS ACTUALLY
///ST/THERE   WHITESPACE   TESTING
///ST/THIS
/US/SHP/T/XYZ2233
/US/SHP/CT/19725748283
/DE/CNE/T/4455
/DE/CNE/CT/49069697125510
/KR/NFY/T/USC6677
/AU/DNR/D/UN1234
/AU/DNR/D/UN2345";

				var dataCreator = new AWBHeaderTestDataCreator(Factory);
				GlbBranch.CurrentBranch.GB_RL_NKHomePort = "AUSYD";
				FreightDataRegistry.Instance.SendFWBNatureAndQuantityOfGoodsType.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
				var awbHeader = dataCreator.ConsolAWBHeader;
				awbHeader.EH_AdditionalSecurityInformationStatement = "Cargo has been examined in accordance with a regulation 4.41JA notice.";
				var actualFWB16 = new FWB(new FWBMessageDetails(dataCreator.ConsolAWBHeader), FWB.Version.No16, true).ToString();

				AssertMultilineASCIIEquals("FWB Message as Version 16, include Security Declaration", expectedFWB16.Trim(), actualFWB16.Trim());
			}
		}

		public void TestFWBAdditionalSecurityInformation_UK()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.UnitedKingdom))
			{
				var time = ZDateTimeOffset.Now;
				var expectedFWB16 = $@"
FWB/16
006-96667465DFWFRA/T4K245
FLT/DL8226E/08/DL014/09
RTG/ATLDL/FRADL
SHP
/SEKO WORLDWIDE LLC - DALLAS
/1840 W.AIRFIELD DRIVE  100
/DFW AIRPORT/TX
/US/75261/TE/19725748283
CNE
/SEKO SYNERGY GMBH
/CARGO CITY SOUTH   BLDG 556 D
/FRANKFURT/HE
/DE/60549/TE/49069697125510
AGT//3360525/0151
/SEKO WORLDWIDE DFW
/DALLAS
NFY/MR ALSO NOTIFY
/NOTIFY ADDRESS
/AN PLACE/AN STATE
/KR/3321/PHO/556644
ACC/GEN/SPOT  1208107465
CVD/USD/PP/PP/NVD/NCV/XXX
RTD/1/P4/K245/CQ/W245/R3/T735
/NG/CONSOLIDATION AS PER
/2/NG/ATTACHED LIST
/3/NG/DIMS 13X12X15 IN X 2
/4/NG/DIMS 48X40X40 IN X 1
/5/NG/DIMS 12X11X16 IN X 1
/6/NG/GOODS ORIGIN  AU
/7/NG/4 SLAC
/8/ND//NDA
OTH/P/MYC196
/P/SCC36.75
/P/XBC29.4
PPD/WT735
/OC262.15/CT997.15
CER/SEKO WORLDWIDE DFW
ISU/08DEC10/DALLAS-FORT WORTH/JOSEPH RUFF
OSI/PLEASE NOTIFY CONSIGNEE UPON ARRIVAL
REF//C00141929/FFW/CWID{GlbCompany.CurrentCompany.LicenceKeyIdentifier}/
SPH/EAW/PEF/SCO
OCI/AU/ISS/RA/RANUM123
///ED/0115
///AC/DURBLE
///AC/BURBLE
///KC/JUBBLE
///ED/1299
//OSS/RA/GLUBBLE
///ED/1299
///L/BIO
///L/HLD
///SM/PRT
///SN/RUSSELL DUSSEL
///SD/13FEB151630
///ST/DOC{time.ToString("ddMMMyy", CultureInfo.InvariantCulture).ToUpperInvariant()}
///ST/THIS IS THE TEST ADDITIONAL
///ST/SECURITY INFORMATION IF ITS LENGTH
///ST/EXCEEDS 35 THEN IT SHOULD BE SPLIT
///ST/INTO MULTIPLE LINES
///ST/WRAPPED TO A NEW LINE FOR EACH 35
///ST/CHARACTERS
///ST/UNLESS A NEW LINE IS ACTUALLY
///ST/THERE   WHITESPACE   TESTING
///ST/THIS
/US/SHP/T/XYZ2233
/US/SHP/CT/19725748283
/DE/CNE/T/4455
/DE/CNE/CT/49069697125510
/KR/NFY/T/USC6677
/GB/DNR/D/UN1234
/GB/DNR/D/UN2345
";
				var dataCreator = new AWBHeaderTestDataCreator(Factory);
				FreightDataRegistry.Instance.SendFWBNatureAndQuantityOfGoodsType.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
				var awbHeader = dataCreator.ConsolAWBHeader;
				awbHeader.EH_AdditionalSecurityInformation = "this is the test additional security information, if its length exceeds 35, then it should be split into multiple lines";
				awbHeader.EH_ScheduledArrivalDate = time;
				var actualFWB16 = new FWB(new FWBMessageDetails(dataCreator.ConsolAWBHeader), FWB.Version.No16, true).ToString();

				AssertMultilineASCIIEquals("FWB Message as Version 16, include Security Declaration", expectedFWB16.Trim(), actualFWB16.Trim());
			}
		}

		public void TestFWB16_TSAStatementShouldBePrintedOnce_WhenSecurityDeclarationIsNotIncluded()
		{
			using (FreightDataRegistry.Instance.AWBSecurityStatementTextToUSAAndUSTerritories.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "this is the test security statement, it shoud not    contain more than one space.  Let's see the result!"))
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.Australia))
			{
				var expectedFWB16 = @"
FWB/16
006-96667465DFWFRA/T4K245
FLT/DL8226E/08/DL014/09
RTG/ATLDL/FRADL
SHP
/SEKO WORLDWIDE LLC - DALLAS
/1840 W.AIRFIELD DRIVE  100
/DFW AIRPORT/TX
/US/75261/TE/19725748283
CNE
/SEKO SYNERGY GMBH
/CARGO CITY SOUTH   BLDG 556 D
/FRANKFURT/HE
/DE/60549/TE/49069697125510
AGT//3360525/0151
/SEKO WORLDWIDE DFW
/DALLAS
NFY/MR ALSO NOTIFY
/NOTIFY ADDRESS
/AN PLACE/AN STATE
/KR/3321/PHO/556644
ACC/GEN/SPOT  1208107465
CVD/USD/PP/PP/NVD/NCV/XXX
RTD/1/P4/K245/CQ/W245/R3/T735
/NG/CONSOLIDATION AS PER
/2/NG/ATTACHED LIST
/3/NG/DIMS 13X12X15 IN X 2
/4/NG/DIMS 48X40X40 IN X 1
/5/NG/DIMS 12X11X16 IN X 1
/6/NG/GOODS ORIGIN  AU
/7/NG/4 SLAC
/8/ND//NDA
OTH/P/MYC196
/P/SCC36.75
/P/XBC29.4
PPD/WT735
/OC262.15/CT997.15
CER/SEKO WORLDWIDE DFW
ISU/08DEC10/DALLAS-FORT WORTH/JOSEPH RUFF
OSI/PLEASE NOTIFY CONSIGNEE UPON ARRIVAL
REF//C00141929/FFW/CWIDEDIEDIDAT/SYD
SPH/EAW/PEF/SCO
OCI///ST/THIS IS THE TEST SECURITY
///ST/STATEMENT IT SHOUD NOT CONTAIN
///ST/MORE THAN ONE SPACE. LET S SEE THE
///ST/RESULT
/US/SHP/T/XYZ2233
/US/SHP/CT/19725748283
/DE/CNE/T/4455
/DE/CNE/CT/49069697125510
/KR/NFY/T/USC6677
/AU/DNR/D/UN1234
/AU/DNR/D/UN2345";

				var dataCreator = new AWBHeaderTestDataCreator(Factory);
				GlbBranch.CurrentBranch.GB_RL_NKHomePort = "AUSYD";
				FreightDataRegistry.Instance.SendFWBNatureAndQuantityOfGoodsType.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
				var awbHeader = dataCreator.ConsolAWBHeader;
				awbHeader.EH_AdditionalSecurityInformationStatement = "Cargo has been examined in accordance with a regulation 4.41JA notice.";
				var actualFWB16 = new FWB(new FWBMessageDetails(dataCreator.ConsolAWBHeader), FWB.Version.No16, includeSecurityDeclaration: false).ToString();

				AssertMultilineASCIIEquals("FWB Message as Version 16, Security Declaration is not included. TSA Security Statement should be printed once, but Additional Security Information should not.", expectedFWB16.Trim(), actualFWB16.Trim());
			}
		}

		internal class AWBHeaderTestDataCreator
		{
			public AWBHeaderTestDataCreator(BusinessObjectFactory factory)
			{
				this.factory = factory;

				SetupEnvironment();

				var consol = GetSetupConsol();
				var shipment = GetSetupShipment(consol);
				ConsolAWBHeader = GetSetupConsolAWBHeader(consol);
				ShipmentAWBHeader = GetSetupShipmentAWBHeader(shipment);
			}

			readonly BusinessObjectFactory factory;

			public readonly ConsolExportAWBHeader ConsolAWBHeader;
			public readonly ShipmentExportAWBHeader ShipmentAWBHeader;

			static void SetupEnvironment()
			{
				Env.Registry.Freight.AirWaybill.IssuingCarrierAgentAccountNumber = "";
				Env.Registry.Freight.AirWaybill.IssuingCarrierAgentIATACode = "33605250151";
				Env.Registry.Freight.AirWaybill.IssuingCarrierAgentName = "SEKO WORLDWIDE DFW";
				Env.Registry.Freight.AirWaybill.IssuingCarrierAgentCity = "DALLAS";
			}

			ForwardingConsol GetSetupConsol()
			{
				var consol = factory.New<ForwardingConsol>();
				consol.JK_TransportMode = Core.Constants.TransportModes.Air;
				consol.JK_UniqueConsignRef = "C00141929";
				consol.JK_RL_NKLoadPort = "USDFW";
				consol.JK_RL_NKDischargePort = "DEFRA";
				consol.JK_MasterBillNum = "00696667465";
				consol.JK_OA_SendingForwarderAddress = ZGuid.Empty;
				consol.JK_OA_ReceivingForwarderAddress = ZGuid.Empty;
				consol.Notes.AddNew(false, PredefinedNoteTypes.Instance.AdditionalSecurityInformation.Description, "this is the test additional security information, if its length exceeds 35, then it should be split into multiple lines");

				var transport1 = consol.Transports[0];
				transport1.JW_TransportMode = "AIR";
				transport1.JW_TransportType = "FL1";
				transport1.JW_RL_NKLoadPort = "USDFW";
				transport1.JW_RL_NKDiscPort = "USATL";
				transport1.JW_VoyageFlight = "DL8226E";
				transport1.JW_ETD = new ZDateTime(2010, 12, 8);
				transport1.JW_ETA = new ZDateTime(2010, 12, 9);

				var transport2 = consol.Transports.AddNew();
				transport2.JW_TransportMode = "AIR";
				transport2.JW_TransportType = "FL2";
				transport2.JW_RL_NKLoadPort = "USATL";
				transport2.JW_RL_NKDiscPort = "DEFRA";
				transport2.JW_VoyageFlight = "DL014";
				transport2.JW_ETD = new ZDateTime(2010, 12, 9);
				transport2.JW_ETA = new ZDateTime(2010, 12, 10);

				return consol;
			}

			ForwardingShipment GetSetupShipment(ForwardingConsol consol)
			{
				var undgIATASubstance = factory.NewWithValidTestData<UNDGSubstance>();
				undgIATASubstance.DG_Standard = UNDGSubstanceLookups.UNDGSubstanceStandardTypes.IATA;
				undgIATASubstance.DG_Code = "1234";
				var undgIATASubstance2 = factory.NewWithValidTestData<UNDGSubstance>();
				undgIATASubstance2.DG_Standard = UNDGSubstanceLookups.UNDGSubstanceStandardTypes.IATA;
				undgIATASubstance2.DG_Code = "2345A";

				var shipment = consol.Shipments.AddNew();

				var packLine1 = shipment.OuterPackLines.AddNew();
				packLine1.JL_PackageCount = 2;
				packLine1.JL_F3_NKPackType = "PCE";
				packLine1.JL_Length = 13;
				packLine1.JL_Width = 12;
				packLine1.JL_Height = 15;
				packLine1.JL_UnitOfDimension = "IN";
				packLine1.JL_ActualWeight = 145.00m;
				packLine1.UNDGs.AddNew().DI_DG = undgIATASubstance.PK;

				var packLine2 = shipment.OuterPackLines.AddNew();
				packLine2.JL_PackageCount = 1;
				packLine2.JL_F3_NKPackType = "PCE";
				packLine2.JL_Length = 48;
				packLine2.JL_Width = 40;
				packLine2.JL_Height = 40;
				packLine2.JL_UnitOfDimension = "IN";
				packLine2.JL_ActualWeight = 60.00m;
				packLine2.UNDGs.AddNew().DI_DG = undgIATASubstance.PK;

				var packLine3 = shipment.OuterPackLines.AddNew();
				packLine3.JL_PackageCount = 1;
				packLine3.JL_F3_NKPackType = "PCE";
				packLine3.JL_Length = 12;
				packLine3.JL_Width = 11;
				packLine3.JL_Height = 16;
				packLine3.JL_UnitOfDimension = "IN";
				packLine3.JL_ActualWeight = 40.00m;
				packLine3.UNDGs.AddNew().DI_DG = undgIATASubstance2.PK;

				shipment.JS_TransportMode = Core.Constants.TransportModes.Air;
				shipment.JS_HouseBill = "CVGA00176739";
				shipment.JS_RL_NKOrigin = "USDFW";
				shipment.JS_RL_NKDestination = "DEFRA";
				shipment.JS_ActualWeight = 245.00m;
				shipment.JS_ActualVolume = 1.371m;
				shipment.JS_OuterPacks = 4;
				shipment.JS_F3_NKPackType = "PCE";
				shipment.JS_GoodsDescription = "WILD CHERRY BAR";

				return shipment;
			}

			ConsolExportAWBHeader GetSetupConsolAWBHeader(ForwardingConsol consol)
			{
				var consolAWBHeader = factory.New<ConsolExportAWBHeader>();
				consolAWBHeader.EH_ParentID = consol.PK;
				consolAWBHeader.Populate();
				consolAWBHeader.EH_ShippingLoadAndCount = 4;
				consol.JK_OverrideWaybillDefaults = ZBool.True;

				consolAWBHeader.AWBSpecialHandlingItems.AddNew().EP_SpecialHandling = AWBSpecialHandlingCodeDescriptionPairList.Codes.EFreightConsignmentWithNoAccompanyingPaperDocuments;
				consolAWBHeader.AWBSpecialHandlingItems.AddNew().EP_SpecialHandling = AWBSpecialHandlingCodeDescriptionPairList.Codes.Flowers;

				consolAWBHeader.EH_ShipperName = "SEKO WORLDWIDE LLC - DALLAS";
				consolAWBHeader.EH_ShipperAddress = "1840 W.AIRFIELD DRIVE  100";
				consolAWBHeader.EH_ShipperPlace = "DFW AIRPORT";
				consolAWBHeader.EH_ShipperState = "TX";
				consolAWBHeader.EH_ShipperCountryCode = "US";
				consolAWBHeader.EH_ShipperPostCode = "75261";
				consolAWBHeader.EH_ShipperContactCode = "TE";
				consolAWBHeader.EH_ShipperContactDetail = "19725748283";
				consolAWBHeader.EH_ShipperTraderNoType = "XYZ";
				consolAWBHeader.EH_ShipperTraderNo = "2233";

				consolAWBHeader.EH_ConsigneeName = "SEKO SYNERGY GMBH";
				consolAWBHeader.EH_ConsigneeAddress = "CARGO CITY SOUTH   BLDG 556 D";
				consolAWBHeader.EH_ConsigneePlace = "FRANKFURT";
				consolAWBHeader.EH_ConsigneeState = "HE";
				consolAWBHeader.EH_ConsigneeCountryCode = "DE";
				consolAWBHeader.EH_ConsigneePostCode = "60549";
				consolAWBHeader.EH_ConsigneeContactCode = "TE";
				consolAWBHeader.EH_ConsigneeContactDetail = "49069697125510";
				consolAWBHeader.EH_ConsigneeTraderNo = "4455";

				consolAWBHeader.EH_AlsoNotifyName = "Mr Also Notify";
				consolAWBHeader.EH_AlsoNotifyAddress = "Notify Address";
				consolAWBHeader.EH_AlsoNotifyPlace = "AN Place";
				consolAWBHeader.EH_AlsoNotifyState = "AN State";
				consolAWBHeader.EH_AlsoNotifyCountryCode = "KR";
				consolAWBHeader.EH_AlsoNotifyPostCode = "3321";
				consolAWBHeader.EH_AlsoNotifyContactCode = "PHO";
				consolAWBHeader.EH_AlsoNotifyContactDetail = "+55 (66) 44";
				consolAWBHeader.EH_AlsoNotifyTraderNoType = "USC";
				consolAWBHeader.EH_AlsoNotifyTraderNo = "6677";

				consolAWBHeader.EH_HandlingInformation = "PLEASE NOTIFY CONSIGNEE UPON ARRIVAL";

				var accountingInfo1 = consolAWBHeader.AWBAccountingInformations.AddNew();
				accountingInfo1.EA_InformationID = "GEN";
				accountingInfo1.EA_Information = "SPOT  1208107465";

				consolAWBHeader.EH_Currency = "USD";
				consolAWBHeader.EH_ChargesCode = "PP";
				consolAWBHeader.EH_WeightVPPDCOL = ConsolExportAWBHeader.Constants.PrepaidCollect3CharCodes.Prepaid;
				consolAWBHeader.EH_OtherPPDCOL = ConsolExportAWBHeader.Constants.PrepaidCollect3CharCodes.Prepaid;
				consolAWBHeader.EH_DeclaredValue = 0M;
				consolAWBHeader.EH_CustomsValue = 0M;
				consolAWBHeader.EH_InsuranceValue = 0M;

				var rateLine1 = consolAWBHeader.AWBRateLines[0];
				rateLine1.ER_RateClass = "Q";
				rateLine1.ER_RateChargeOrDiscount = 3M;

				var otherCharge1 = consolAWBHeader.AWBOtherCharges.AddNew();
				otherCharge1.EO_ChargeCode = "MY";
				otherCharge1.EO_EntitlementCode = "C";
				otherCharge1.EO_Amount = 196.00M;

				var otherCharge2 = consolAWBHeader.AWBOtherCharges.AddNew();
				otherCharge2.EO_ChargeCode = "SC";
				otherCharge2.EO_EntitlementCode = "C";
				otherCharge2.EO_Amount = 36.75M;

				var otherCharge3 = consolAWBHeader.AWBOtherCharges.AddNew();
				otherCharge3.EO_ChargeCode = "XB";
				otherCharge3.EO_EntitlementCode = "C";
				otherCharge3.EO_Amount = 29.40M;

				consolAWBHeader.EH_ShippersSignature = "SEKO WORLDWIDE DFW";
				consolAWBHeader.EH_AWBIssueDate = new ZDateTime(2010, 12, 8);
				consolAWBHeader.EH_AWBIssuePlace = "DALLAS-FORT WORTH";
				consolAWBHeader.EH_AWBAgentsSignature = "JOSEPH RUFF";

				var originRateLine = consolAWBHeader.AWBRateLines.Cast<ExportAWBRateLine>().First(r => r.NatureAndQtyOfGoods.Text == "");
				originRateLine.ER_NatureAndQtyOfGoodsType = Core.Constants.AWB.NatureAndQtyOfGoodsTypes.CountryOfGoodsOrigin;
				originRateLine.NatureAndQtyOfGoods.Text = "Goods Origin: AU";

				consolAWBHeader.EH_RN_NKAgentApprovalCountryCode = "AU";
				consolAWBHeader.EH_AgentApprovalNumber = "RANUM123";
				consolAWBHeader.EH_AgentApprovalExpiryDate = new ZDateTime(2015, 1, 4);
				consolAWBHeader.CargoSecurityKnownShippers.RemoveAll();
				var acLine1 = consolAWBHeader.CargoSecurityKnownShippers.AddNew();
				acLine1.EAS_ApprovalCategory = "AC";
				acLine1.EAS_ApprovalNumber = "DURBLE";
				var acLine2 = consolAWBHeader.CargoSecurityKnownShippers.AddNew();
				acLine2.EAS_ApprovalCategory = "AC";
				acLine2.EAS_ApprovalNumber = "BURBLE";
				var kcLine = consolAWBHeader.CargoSecurityKnownShippers.AddNew();
				kcLine.EAS_ApprovalCategory = "KC";
				kcLine.EAS_ApprovalNumber = "JUBBLE";
				var raLine = consolAWBHeader.CargoSecurityKnownShippers.AddNew();
				raLine.EAS_ApprovalCategory = "RA";
				raLine.EAS_ApprovalNumber = "GLUBBLE";
				consolAWBHeader.CargoSecurityExemptionGrounds.RemoveAll();
				consolAWBHeader.CargoSecurityExemptionGrounds.AddNew().EAS_ExemptionGround = "BIO";
				consolAWBHeader.CargoSecurityExemptionGrounds.AddNew().EAS_ExemptionGround = "HLD";
				consolAWBHeader.CargoSecurityScreeningMethods.RemoveAll();
				consolAWBHeader.CargoSecurityScreeningMethods.AddNew().EAS_ScreeningMethod = "PRT";
				consolAWBHeader.EH_SecurityStatusIssuedBy = "RUSSELL DUSSEL";
				consolAWBHeader.EH_SecurityStatusIssueDate = new ZDateTime(2015, 2, 13, 16, 30, 0);
				consolAWBHeader.AWBSpecialHandlingItems.AddNew().EP_SpecialHandling = AWBSpecialHandlingCodeDescriptionPairList.Codes.CargoSecureForAllCargoAircraftOnly;
				consolAWBHeader.EH_AdditionalScreeningMethods = "WRAPPED TO A NEW LINE FOR EACH 35 CHARACTERS\r\nUNLESS A NEW LINE IS ACTUALLY THERE   WHITESPACE   TESTING   THIS";

				return consolAWBHeader;
			}

			ShipmentExportAWBHeader GetSetupShipmentAWBHeader(ForwardingShipment shipment)
			{
				var awbHeader = factory.New<ShipmentExportAWBHeader>();
				awbHeader.EH_ParentID = shipment.PK;

				awbHeader.AWBRateLines[0].ER_WeightInLBsOrKGs = Core.Constants.AWB.RateLineUQ.Kilos;
				awbHeader.Populate();
				awbHeader.EH_ShippingLoadAndCount = 4;
				awbHeader.Shipment.JS_OverrideWaybillDefaults = ZBool.True;

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

				awbHeader.EH_AlsoNotifyName = "Sun";
				awbHeader.EH_AlsoNotifyAddress = "Notify Address";
				awbHeader.EH_AlsoNotifyPlace = "AN Place";
				awbHeader.EH_AlsoNotifyState = "AN State";
				awbHeader.EH_AlsoNotifyCountryCode = "KR";
				awbHeader.EH_AlsoNotifyPostCode = "3321";
				awbHeader.EH_AlsoNotifyContactCode = "PHO";
				awbHeader.EH_AlsoNotifyContactDetail = "+55 (66) 44";
				awbHeader.EH_AlsoNotifyTraderNo = "9900";

				awbHeader.EH_Currency = "USD";
				awbHeader.EH_WeightVPPDCOL = ExportAWBHeader.Constants.PrepaidCollect3CharCodes.Collect;
				awbHeader.EH_OtherPPDCOL = ExportAWBHeader.Constants.PrepaidCollect3CharCodes.Collect;
				awbHeader.EH_DeclaredValue = 0.00M;
				awbHeader.EH_CustomsValue = 4289.68M;
				awbHeader.EH_InsuranceValue = 0.00M;

				return awbHeader;
			}
		}

		public void TestOSICovers3LinesMax()
		{
			ConsolAWBHeader.EH_HandlingInformation = "THIS IS A REALLY LONG LINE OF TEXT THAT SHOULD ONLY BE 3 LINES LONG, BUT IF THAT 33333 FINDS A WAY IN THE OSI IT WILL REACH A 4TH LINE";
			string fwbMessage = new FWB(new FWBMessageDetails(ConsolAWBHeader), FWB.Version.No16).ToString();
			AssertNotContains("RA number should not be included with OSI", "OSI/33333", fwbMessage);
			string expectedOSI = @"OSI/THIS IS A REALLY LONG LINE OF TEXT THAT SHOULD ONLY BE 3 LINES LO
/NG  BUT IF THAT 33333 FINDS A WAY IN THE OSI IT WILL REACH A 4TH
/LINE";
			AssertContains("OSI should go straight to handling instructions", expectedOSI, fwbMessage);
		}

		public void TestSSRWithKnownConsignorCode()
		{
			GlbBranch.CurrentBranch.OrgProxy.MainAddress.KnownShipperDetails.DeleteAll();

			using (GlbCompany.CurrentCompany.TemporarilySetCountry("SG"))
			{
				GlbBranch.CurrentBranch.HomePort.RL_IATA = "AYC";
				var consol = ConsolAWBHeader.Consol;
				consol.Transports[0].JW_RL_NKLoadPort = "SGSIN";
				var shipment1 = consol.Shipments[0];
				var shipment2 = consol.Shipments.AddNew();
				shipment1.JS_InspectionTypeCode = "UNK";
				shipment2.JS_InspectionTypeCode = "UNK";

				SSR = "SSR/RCAR-UC\r\n";
				REF = "REF//C121212/FFW/CWID" + GlbCompany.CurrentCompany.LicenceKeyIdentifier + "/AYC\r\n";
				AssertMessageCorrect();

				shipment1.JS_InspectionTypeCode = "APP";
				shipment2.JS_InspectionTypeCode = "XRY";

				SSR = "SSR/RCAR-KC\r\n";
				AssertMessageCorrect();
			}
		}

		public void TestOtherServiceInformation_SsrNfyOsiCorLimits()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry("SG"))
			{
				#region Setup

				const string shortOsiValue = "ONE-FIFTY-SEVEN-CHARS...................................................................................................................ONE-FIFTY-SEVEN-CHARS";
				const string shortOsiUntruncated =
											  "OSI/ONE-FIFTY-SEVEN-CHARS............................................\r\n" +
											  "/.................................................................\r\n" +
											  "/......ONE-FIFTY-SEVEN-CHARS\r\n";
				const string osiValue = "ONE-NINETY-TWO-CHARS........................................................................................................................................................ONE-NINETY-TWO-CHARS";
				const string osiUntruncated = "OSI/ONE-NINETY-TWO-CHARS.............................................\r\n" +
											  "/.................................................................\r\n" +
											  "/..........................................ONE-NINETY-TWO-CHARS\r\n";
				const string osiTruncated = "OSI/ONE-NINETY-TWO-CHARS.............................................\r\n" +
											  "/.................................................................\r\n" +
											  "/..........................................ONE-NINETY-\r\n";

				GlbBranch.CurrentBranch.OrgProxy.MainAddress.KnownShipperDetails.DeleteAll();
				GlbBranch.CurrentBranch.HomePort.RL_IATA = "AYC";

				ConsolAWBHeader.EH_SpecialHandlingCode = "";
				ConsolAWBHeader.EH_AlsoNotifyPlace = "";
				ConsolAWBHeader.EH_AlsoNotifyState = "";
				ConsolAWBHeader.EH_AlsoNotifyCountryCode = "";
				ConsolAWBHeader.EH_AlsoNotifyPostCode = "";
				ConsolAWBHeader.EH_AlsoNotifyContactCode = "";
				ConsolAWBHeader.EH_AlsoNotifyContactDetail = "";

				COR = "";
				SSR = "";
				NFY = "";
				OSI = "";
				REF = "REF//C121212/FFW/CWID" + GlbCompany.CurrentCompany.LicenceKeyIdentifier + "/AYC\r\n";

				#endregion

				// Test NFY doesn't get removed with a shorter OSI
				ConsolAWBHeader.EH_HandlingInformation = shortOsiValue;
				ConsolAWBHeader.EH_AlsoNotifyName = "OLIVER MCFAKINGTON";
				ConsolAWBHeader.EH_AlsoNotifyAddress = "72 ORIORDAN ST";
				OSI = shortOsiUntruncated;
				NFY = "NFY/OLIVER MCFAKINGTON\r\n" +
					  "/72 ORIORDAN ST\r\n" +
					  "/\r\n" +
					  "/\r\n";
				AssertMessageCorrect();

				// Test OSI doesn't get truncated unnecessarily when NFY can be dropped
				ConsolAWBHeader.EH_HandlingInformation = osiValue;
				OSI = osiUntruncated;
				NFY = "";
				AssertMessageCorrect();

				// Test that OSI truncates when SSR/COR/NFY are set
				var flight1 = ConsolAWBHeader.Consol.Transports[0];
				flight1.JW_TransportMode = "AIR";
				flight1.JW_RL_NKLoadPort = "SGSIN";
				flight1.JW_RL_NKDiscPort = "USLAX";
				flight1.JW_IsCargoOnly = false;

				ConsolAWBHeader.EH_SpecialHandlingCode = "AB";

				OSI = osiTruncated;
				COR = "COR/AB\r\n";
				SSR = "SSR/RCAR-UC\r\n";
				NFY = "";
				AssertMessageCorrect();
			}
		}

		public void TestToStringAllData()
		{
			AssertMessageCorrect();
		}

		public void TestToStringNoFlightInfo()
		{
			ConsolAWBHeader.EH_Booking2ndCarrier = "QF";
			ConsolAWBHeader.EH_Booking2ndFlight = "";
			ConsolAWBHeader.EH_Booking2ndFlightDate = "";
			FLT = "FLT/QF123/02\r\n";
			AssertMessageCorrect();

			ConsolAWBHeader.EH_Booking2ndCarrier = "";
			ConsolAWBHeader.EH_Booking2ndFlight = "111";
			FLT = "FLT/QF123/02\r\n";
			AssertMessageCorrect();

			ConsolAWBHeader.EH_Booking2ndFlight = "";
			ConsolAWBHeader.EH_Booking2ndFlightDate = "2";
			FLT = "FLT/QF123/02\r\n";
			AssertMessageCorrect();

			ConsolAWBHeader.EH_Booking2ndFlightDate = "";
			FLT = "FLT/QF123/02\r\n";
			AssertMessageCorrect();

			FLT = "";
			ConsolAWBHeader.EH_Booking1stCarrier = "";
			AssertMessageCorrect();

			ConsolAWBHeader.EH_Booking1stCarrier = "QF";
			ConsolAWBHeader.EH_Booking1stFlight = "";
			AssertMessageCorrect();

			ConsolAWBHeader.EH_Booking1stFlight = "111";
			ConsolAWBHeader.EH_Booking1stFlightDate = "";
			AssertMessageCorrect();

			ConsolAWBHeader.EH_Booking1stFlight = "";
			AssertMessageCorrect();
		}

		public void TestToStringRoutingInfo()
		{
			ConsolAWBHeader.EH_By3rd = "";
			RTG = "RTG/LAXQF/CPTBA/SIN\r\n";
			AssertMessageCorrect();

			ConsolAWBHeader.EH_To2nd = "";
			ConsolAWBHeader.EH_By2nd = "";
			RTG = "RTG/LAXQF/SIN\r\n";
			AssertMessageCorrect();

			ConsolAWBHeader.EH_To3rd = "";
			RTG = "RTG/LAXQF\r\n";
			AssertMessageCorrect();

			ConsolAWBHeader.EH_To1st = "";
			RTG = "RTG/QF\r\n";
			AssertMessageCorrect();
		}

		public void TestToStringGetPartyElements()
		{
			ConsolAWBHeader.EH_ShipperAccount = "";
			SHP = "SHP\r\n/MR SHIPPER\r\n/SHIPPER ADDRESS\r\n/PLACE/STATE\r\n/SG/1234/FAX/12345678\r\n";
			AssertMessageCorrect();

			ConsolAWBHeader.EH_ShipperPostCode = "";
			SHP = "SHP\r\n/MR SHIPPER\r\n/SHIPPER ADDRESS\r\n/PLACE/STATE\r\n/SG//FAX/12345678\r\n";
			AssertMessageCorrect();

			ConsolAWBHeader.EH_ShipperContactCode = "";
			ConsolAWBHeader.EH_ShipperContactDetail = "";
			SHP = "SHP\r\n/MR SHIPPER\r\n/SHIPPER ADDRESS\r\n/PLACE/STATE\r\n/SG\r\n";
			AssertMessageCorrect();
		}

		public void TestToStringGetPartyElementsDoesntIncludePartialContactDetails()
		{
			SHP = "SHP/BRECPT\r\n/MR SHIPPER\r\n/SHIPPER ADDRESS\r\n/PLACE/STATE\r\n/SG/1234/FAX/12345678\r\n";
			AssertMessageCorrect();

			ConsolAWBHeader.EH_ShipperContactCode = "";
			SHP = "SHP/BRECPT\r\n/MR SHIPPER\r\n/SHIPPER ADDRESS\r\n/PLACE/STATE\r\n/SG/1234\r\n";
			AssertMessageCorrect();

			ConsolAWBHeader.EH_ShipperContactCode = "FAX";
			ConsolAWBHeader.EH_ShipperContactDetail = "";
			AssertMessageCorrect();

			ConsolAWBHeader.EH_ShipperContactDetail = "12345678";
			SHP = "SHP/BRECPT\r\n/MR SHIPPER\r\n/SHIPPER ADDRESS\r\n/PLACE/STATE\r\n/SG/1234/FAX/12345678\r\n";
			AssertMessageCorrect();
		}

		public void TestToStringRTDSegment()
		{
			RTD = "RTD/1/P3/K2980/CU/W2400/R7571/T22713\r\n" +
				"/NG/COSMETICS\r\n" +
				"/2/NG/NOT RESTRICTED\r\n" +
				"/3/CE/W580/R8.75/T5075\r\n" +
				"/4/NG/10 SLAC\r\n" +
				"/5/K378/CX/S8\r\n" +
				"/NU/AVM2381XX\r\n" +
				"/6/NG/7 SLAC\r\n" +
				"/7/CX/S8\r\n" +
				"/NU/AVM2482XX\r\n" +
				"/8/NG/12 SLAC\r\n" +
				"/9/CX/S8\r\n" +
				"/NU/AVM1875XX\r\n" +
				"/10/NG/5 SLAC\r\n";

			PopulateRateLine(0, "3", "K", 2980M, "U", "", 2400M, 7571M);
			ConsolAWBHeader.AWBRateLine1.NatureAndQtyOfGoodsDescription = "Cosmetics";
			PopulateRateLine(1, "0", "", 0M, "", "", 0M, 0M);
			ConsolAWBHeader.AWBRateLine2.NatureAndQtyOfGoodsDescription = "NOT RESTRICTED";
			PopulateRateLine(2, "0", "", 0M, "E", "", 580M, 8.75M);
			ConsolAWBHeader.AWBRateLine3.NatureAndQtyOfGoodsDescription = "";
			PopulateRateLine(3, "0", "", 0M, "", "", 0M, 0M);
			ConsolAWBHeader.AWBRateLine4.NatureAndQtyOfGoodsDescription = "10 SLAC";
			PopulateRateLine(4, "0", "", 378M, "X", "8", 0M, 0M);
			ConsolAWBHeader.AWBRateLine5.NatureAndQtyOfGoodsDescription = "AVM2381XX";
			PopulateRateLine(5, "0", "", 0M, "", "", 0M, 0M);
			ConsolAWBHeader.AWBRateLine6.NatureAndQtyOfGoodsDescription = "7 SLAC";
			PopulateRateLine(6, "0", "", 0M, "X", "", 0M, 0M);
			ConsolAWBHeader.AWBRateLine7.NatureAndQtyOfGoodsDescription = "AVM2482XX";
			PopulateRateLine(7, "0", "", 0M, "", "", 0M, 0M);
			ConsolAWBHeader.AWBRateLine8.NatureAndQtyOfGoodsDescription = "12 SLAC";
			PopulateRateLine(8, "0", "", 0M, "X", "", 0M, 0M);
			ConsolAWBHeader.AWBRateLine9.NatureAndQtyOfGoodsDescription = "AVM1875XX";

			COL = "COL/WT27788/VC2/TX55.99\r\n/CT27845.99\r\n";
			CGN = "081-12345678BNESIN/T3K3358\r\n";

			AssertMessageCorrect();
		}

		public void TestToStringRTDSegment_EmptySegmentAddedToMessageBUG()
		{
			RTD = "RTD/1/P3/K2980/CU/W2400/R7571/T22713\r\n" +
					"/NG/COSMETICS\r\n" +
					"/2/PJED\r\n" +
					"/3/NG/5 SLAC\r\n";

			PopulateRateLine(0, "3", "K", 2980M, "U", "", 2400M, 7571M);
			ConsolAWBHeader.AWBRateLine1.NatureAndQtyOfGoodsDescription = "Cosmetics";

			PopulateRateLine(1, "0", "", 0M, "", "", 0M, 0M);
			ConsolAWBHeader.AWBRateLine2.NatureAndQtyOfGoodsDescription = " ";

			COL = "COL/WT22713/VC2/TX55.99\r\n/CT22770.99\r\n";
			CGN = "081-12345678BNESIN/T3K2980\r\n";

			AssertMessageCorrect();
		}

		public void TestToStringAccountingInformation()
		{
			Action<ZString, ZString> newEntryNo = (type, num) =>
			{
				CusEntryNumber entryNumber = Factory.New<CusEntryNumber>();
				entryNumber.CE_EntryType = type;
				entryNumber.CE_EntryNum = num;
				entryNumber.CE_ParentID = ConsolAWBHeader.Consol.PK;
				entryNumber.CE_ParentTable = ConsolAWBHeader.Consol.TableName;
				entryNumber.CE_EntryIsSystemGenerated = true;
				entryNumber.CE_RN_NKCountryCode = "AU";
			};

			newEntryNo("ECN", "3B042401543XPC");
			newEntryNo("ITN", "X123456ABCDEFG");
			newEntryNo("ITN", "X987654ZYXWVUT");

			ACC = "ACC/GEN/ECN 3B042401543XPC\r\n/GEN/ITN X123456ABCDEFG\r\n/GEN/ITN X987654ZYXWVUT\r\n/ASD/SOME TEXT ASD\r\n/GEN/SOME TEXT QWE\r\n/FGT/SOME TEXT FGT\r\n";

			AssertMessageCorrect();
		}

		public void TestEmptyCusEntryNumsAreNotSent()
		{
			Action<ZString, ZString> newEntryNo = (type, num) =>
			{
				CusEntryNumber entryNumber = Factory.New<CusEntryNumber>();
				entryNumber.CE_EntryType = type;
				entryNumber.CE_EntryNum = num;
				entryNumber.CE_ParentID = ConsolAWBHeader.Consol.PK;
				entryNumber.CE_ParentTable = ConsolAWBHeader.Consol.TableName;
				entryNumber.CE_EntryIsSystemGenerated = true;
				entryNumber.CE_RN_NKCountryCode = "AU";
			};

			newEntryNo("X", "");

			ACC = "ACC/ASD/SOME TEXT ASD\r\n/GEN/SOME TEXT QWE\r\n/FGT/SOME TEXT FGT\r\n/RTR/SOME TEXT RTR\r\n/QQQ/SOME TEXT QQQ\r\n/QQQ/SOME TEXT QQQ\r\n";

			AssertMessageCorrect();
		}

		public void TestToStringAccountingInformation_EmptyItems()
		{
			ConsolAWBHeader.AWBAccountingInformations.RemoveAndDeleteAll();
			Factory.Save();

			ACC = "";
			AssertMessageCorrect();
		}

		public void TestToStringAccountingInformation_NetRateIncluded()
		{
			ACC = "ACC/GEN/ECN 3B042401543XPC\r\n/GEN/NETRATE TESTRATE\r\n/ASD/SOME TEXT ASD\r\n/GEN/SOME TEXT QWE\r\n/FGT/SOME TEXT FGT\r\n/RTR/SOME TEXT RTR\r\n";

			CusEntryNumber newEntryNumber = Factory.New<CusEntryNumber>();
			newEntryNumber.CE_EntryType = "ECN";
			newEntryNumber.CE_EntryNum = "3B042401543XPC";
			newEntryNumber.CE_ParentID = ConsolAWBHeader.Consol.PK;
			newEntryNumber.CE_ParentTable = ConsolAWBHeader.Consol.TableName;
			newEntryNumber.CE_EntryIsSystemGenerated = true;
			newEntryNumber.CE_RN_NKCountryCode = "AU";
			Factory.Save();

			ConsolAWBHeader.EH_NetRateCode = "TESTRATE";

			AssertMessageCorrect();
		}

		public void TestToStringChargeSummary()
		{
			PPD = "PPD\r\n/OA120/OC140/CT260\r\n";

			ConsolAWBHeader.EH_TaxesPPD = 0;
			ConsolAWBHeader.EH_ValuationPPD = 0;

			AssertMessageCorrect();
		}

		public void TestToStringChargeSummary_CTisIncluded_COL()
		{
			COL = "";
			PPD = "";
			OTH = "";
			CGN = "081-12345678BNESIN/T00\r\n";
			RTD = "RTD/1/NG/EGGS\r\n";

			ConsolAWBHeader.EH_TaxesCOL = 0;
			ConsolAWBHeader.EH_ValuationCOL = 0;
			ConsolAWBHeader.EH_TaxesPPD = 0;
			ConsolAWBHeader.EH_ValuationPPD = 0;
			ConsolAWBHeader.AWBRateLines.RemoveAll();

			ExportAWBRateLine rateLine = ConsolAWBHeader.AWBRateLines.AddNew();
			rateLine.NatureAndQtyOfGoods.Text = "EGGS";

			ConsolAWBHeader.AWBOtherCharges.RemoveAll();

			AssertMessageCorrect();

			COL = "COL\r\n/CT0\r\n";
			ForwardingConfigurationRegistry.Instance.ForceSendingOfFWBCVDSegment.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			AssertMessageCorrect();
		}

		public void TestToStringChargeSummary_CTisIncluded_PPD()
		{
			COL = "";
			PPD = "";
			OTH = "";
			CGN = "081-12345678BNESIN/T00\r\n";
			RTD = "RTD/1/NG/EGGS\r\n";
			CVD = "CVD/ZAR/TE/PP/123.54/NCV/XXX\r\n";

			ConsolAWBHeader.EH_WeightVPPDCOL = ExportAWBHeader.Constants.PrepaidCollect1CharCodes.Prepaid;
			ConsolAWBHeader.EH_TaxesCOL = 0;
			ConsolAWBHeader.EH_ValuationCOL = 0;
			ConsolAWBHeader.EH_TaxesPPD = 0;
			ConsolAWBHeader.EH_ValuationPPD = 0;
			ConsolAWBHeader.AWBRateLines.RemoveAll();

			ExportAWBRateLine rateLine = ConsolAWBHeader.AWBRateLines.AddNew();
			rateLine.NatureAndQtyOfGoods.Text = "EGGS";

			ConsolAWBHeader.AWBOtherCharges.RemoveAll();

			AssertMessageCorrect();

			PPD = "PPD\r\n/CT0\r\n";
			ForwardingConfigurationRegistry.Instance.ForceSendingOfFWBCVDSegment.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			AssertMessageCorrect();
		}

		public void TestAGTNoCassCodeWorksProperly()
		{
			AGT = "AGT/ACCT NO/0238855\r\n/MR AGENT\r\n/IM HERE\r\n";

			Env.Registry.Freight.AirWaybill.IssuingCarrierAgentIATACode = "0238855";
			ConsolAWBHeader.EH_AgentParticipantIdentifier = "";

			AssertMessageCorrect();
		}

		public void TestCMRExemptionCode()
		{
			AssertEquals("Precondition - Should be One Shiment Only", 1, ConsolAWBHeader.Consol.Shipments.Count);

			ACC = "ACC/GEN/CAN EXPE\r\n/ASD/SOME TEXT ASD\r\n/GEN/SOME TEXT QWE\r\n/FGT/SOME TEXT FGT\r\n/RTR/SOME TEXT RTR\r\n/QQQ/SOME TEXT QQQ\r\n";

			CusEntryNumber newEntryNumber = Factory.New<CusEntryNumber>();
			newEntryNumber.CE_EntryType = "XPE";
			newEntryNumber.CE_EntryNum = "";
			newEntryNumber.CE_ParentID = ConsolAWBHeader.Consol.Shipments[0].PK;
			newEntryNumber.CE_ParentTable = ConsolAWBHeader.Consol.Shipments[0].TableName;
			newEntryNumber.CE_EntryIsSystemGenerated = false;
			newEntryNumber.CE_RN_NKCountryCode = "AU";
			Factory.Save();

			AssertMultilineEquals("Check the strings.", Expected, CargoIMPMessage.ToString(), '\r');
		}

		public void TestCMRCANOnShipment()
		{
			AssertEquals("Precondition - Should be One Shiment Only", 1, ConsolAWBHeader.Consol.Shipments.Count);

			ACC = "ACC/GEN/CAN A76GG9FEG\r\n/ASD/SOME TEXT ASD\r\n/GEN/SOME TEXT QWE\r\n/FGT/SOME TEXT FGT\r\n/RTR/SOME TEXT RTR\r\n/QQQ/SOME TEXT QQQ\r\n";

			CusEntryNumber newEntryNumber = Factory.New<CusEntryNumber>();
			newEntryNumber.CE_EntryType = "CAN";
			newEntryNumber.CE_EntryNum = "A76GG9FEG";
			newEntryNumber.CE_ParentID = ConsolAWBHeader.Consol.Shipments[0].PK;
			newEntryNumber.CE_ParentTable = ConsolAWBHeader.Consol.Shipments[0].TableName;
			newEntryNumber.CE_EntryIsSystemGenerated = false;
			newEntryNumber.CE_RN_NKCountryCode = "AU";
			Factory.Save();

			AssertMultilineEquals("Check the strings.", Expected, CargoIMPMessage.ToString(), '\r');
		}

		public void TestCMRContingencyCode()
		{
			AssertEquals("Precondition - Should be One Shiment Only", 1, ConsolAWBHeader.Consol.Shipments.Count);

			ACC = "ACC/GEN/CAN EXLV\r\n/ASD/SOME TEXT ASD\r\n/GEN/SOME TEXT QWE\r\n/FGT/SOME TEXT FGT\r\n/RTR/SOME TEXT RTR\r\n/QQQ/SOME TEXT QQQ\r\n";

			CusEntryNumber newEntryNumber = Factory.New<CusEntryNumber>();
			newEntryNumber.CE_EntryType = Enterprise.Customs.Common.AU.CMR.CMRExportExemptionCodes.Get3CharCode(Enterprise.Customs.Common.AU.CMR.CMRExportExemptionCodes.EXLV.Code);
			newEntryNumber.CE_EntryNum = "1SCEDN04123456";
			newEntryNumber.CE_ParentID = ConsolAWBHeader.Consol.Shipments[0].PK;
			newEntryNumber.CE_ParentTable = ConsolAWBHeader.Consol.Shipments[0].TableName;
			newEntryNumber.CE_EntryIsSystemGenerated = false;
			newEntryNumber.CE_RN_NKCountryCode = "AU";
			Factory.Save();

			AssertMultilineEquals("Check the strings.", Expected, CargoIMPMessage.ToString(), '\r');
		}

		public void TestUnknownCustomsEntryCode()
		{
			AssertEquals("Precondition - Should be One Shiment Only", 1, ConsolAWBHeader.Consol.Shipments.Count);

			ACC = "ACC/GEN/XYZ 1SCEDN04123456\r\n/ASD/SOME TEXT ASD\r\n/GEN/SOME TEXT QWE\r\n/FGT/SOME TEXT FGT\r\n/RTR/SOME TEXT RTR\r\n/QQQ/SOME TEXT QQQ\r\n";

			CusEntryNumber newEntryNumber = Factory.New<CusEntryNumber>();
			newEntryNumber.CE_EntryType = "XYZ";
			newEntryNumber.CE_EntryNum = "1SCEDN04123456";
			newEntryNumber.CE_ParentID = ConsolAWBHeader.Consol.Shipments[0].PK;
			newEntryNumber.CE_ParentTable = ConsolAWBHeader.Consol.Shipments[0].TableName;
			newEntryNumber.CE_EntryIsSystemGenerated = false;
			newEntryNumber.CE_RN_NKCountryCode = "AU";
			Factory.Save();

			AssertMultilineEquals("Check the strings.", Expected, CargoIMPMessage.ToString(), '\r');
		}

		public void TestCANOnConsol()
		{
			ACC = "ACC/GEN/CAN A76GG9FEG\r\n/ASD/SOME TEXT ASD\r\n/GEN/SOME TEXT QWE\r\n/FGT/SOME TEXT FGT\r\n/RTR/SOME TEXT RTR\r\n/QQQ/SOME TEXT QQQ\r\n";

			CusEntryNumber newEntryNumber = Factory.New<CusEntryNumber>();
			newEntryNumber.CE_EntryType = "CAN";
			newEntryNumber.CE_EntryNum = "A76GG9FEG";
			newEntryNumber.CE_ParentID = ConsolAWBHeader.Consol.PK;
			newEntryNumber.CE_ParentTable = ConsolAWBHeader.Consol.TableName;
			newEntryNumber.CE_EntryIsSystemGenerated = true;
			newEntryNumber.CE_RN_NKCountryCode = "AU";
			Factory.Save();

			AssertMultilineEquals("Check the strings.", Expected, CargoIMPMessage.ToString(), '\r');
		}

		public void TestCMRCRNOnConsol()
		{
			ACC = "ACC/GEN/CAN 3B042401543XPC\r\n/ASD/SOME TEXT ASD\r\n/GEN/SOME TEXT QWE\r\n/FGT/SOME TEXT FGT\r\n/RTR/SOME TEXT RTR\r\n/QQQ/SOME TEXT QQQ\r\n";

			CusEntryNumber newEntryNumber = Factory.New<CusEntryNumber>();
			newEntryNumber.CE_EntryType = "CRN";
			newEntryNumber.CE_EntryNum = "3B042401543XPC";
			newEntryNumber.CE_ParentID = ConsolAWBHeader.Consol.PK;
			newEntryNumber.CE_ParentTable = ConsolAWBHeader.Consol.TableName;
			newEntryNumber.CE_EntryIsSystemGenerated = true;
			newEntryNumber.CE_RN_NKCountryCode = "AU";
			Factory.Save();

			AssertMultilineEquals("Check the strings.", Expected, CargoIMPMessage.ToString(), '\r');
		}

		public void TestCMRECNOnShipment()
		{
			AssertEquals("Precondition - Should be One Shiment Only", 1, ConsolAWBHeader.Consol.Shipments.Count);

			ACC = "ACC/GEN/ECN 3B042401543XPC\r\n/ASD/SOME TEXT ASD\r\n/GEN/SOME TEXT QWE\r\n/FGT/SOME TEXT FGT\r\n/RTR/SOME TEXT RTR\r\n/QQQ/SOME TEXT QQQ\r\n";

			CusEntryNumber newEntryNumber = Factory.New<CusEntryNumber>();
			newEntryNumber.CE_EntryType = "ECN";
			newEntryNumber.CE_EntryNum = "3B042401543XPC";
			newEntryNumber.CE_ParentID = ConsolAWBHeader.Consol.Shipments[0].PK;
			newEntryNumber.CE_ParentTable = ConsolAWBHeader.Consol.Shipments[0].TableName;
			newEntryNumber.CE_EntryIsSystemGenerated = false;
			newEntryNumber.CE_RN_NKCountryCode = "AU";
			Factory.Save();

			AssertMultilineEquals("Check the strings.", Expected, CargoIMPMessage.ToString(), '\r');
		}

		public void TestOSI()
		{
			ConsolAWBHeader.EH_HandlingInformation = @"DANGEROUS GOODS AS PER ASSOCIATED SHIPPER'S DECLARATION            

				PLS NTY CNEE";
			OSI = "OSI/DANGEROUS GOODS AS PER ASSOCIATED SHIPPER S DECLARATION\r\n/PLS NTY CNEE\r\n";

			AssertMessageCorrect();
		}

		public void TestRTDCountsNonemptyLines()
		{
			RTD = "RTD/1/P3/K2980/CU/W2400/R7571/T22713\r\n" +
					"/NG/COSMETICS\r\n" +
					"/2/CX\r\n" +
					"/3/NG/AVM2482XX\r\n" +
					"/4/NG/5 SLAC\r\n";

			PopulateRateLine(0, "3", "K", 2980M, "U", "", 2400M, 7571M);
			ConsolAWBHeader.AWBRateLine1.NatureAndQtyOfGoodsDescription = "Cosmetics";
			PopulateRateLine(1, "0", "", 0M, "", "", 0M, 0M);
			ConsolAWBHeader.AWBRateLine2.NatureAndQtyOfGoodsDescription = ""; //Should be ignored as it is empty
			PopulateRateLine(2, "0", "", 0M, "X", "", 0M, 0M);
			ConsolAWBHeader.AWBRateLine3.NatureAndQtyOfGoodsDescription = "";
			PopulateRateLine(3, "0", "", 0M, "", "", 0M, 0M);
			ConsolAWBHeader.AWBRateLine4.NatureAndQtyOfGoodsDescription = "AVM2482XX";

			COL = "COL/WT22713/VC2/TX55.99\r\n/CT22770.99\r\n";
			CGN = "081-12345678BNESIN/T3K2980\r\n";

			AssertMessageCorrect();
		}

		public void TestCOR()
		{
			ConsolAWBHeader.EH_SpecialHandlingCode = @"T2";
			COR = "COR/T2\r\n";

			AssertMessageCorrect();
		}

		public void TestFWB_ConsigneeTradeTradeNo_ForImportToChina()
		{
			var header = CreateEmptyAWBHeaderWithConsol();

			header.EH_ConsigneeTraderNo = "1234567";
			header.EH_ConsigneeCountryCode = CountryCodes.China;
			header.EH_ConsigneeTraderNoCountryCode = CountryCodes.China;
			header.EH_ConsigneeTraderNoType = "USCI";
			var message = new FWB(new FWBMessageDetails(header), FWB.Version.No16).ToString();
			AssertContains("CNE/T/USCI1234567", message);
		}

		public void TestFWB_AlsoNotifyTradeTradeNo_ForImportToChina()
		{
			var header = CreateEmptyAWBHeaderWithConsol();

			header.EH_AlsoNotifyTraderNo = "1234567";
			header.EH_AlsoNotifyCountryCode = CountryCodes.China;
			header.EH_AlsoNotifyTraderNoCountryCode = CountryCodes.China;
			header.EH_AlsoNotifyTraderNoType = "USCI";
			var message = new FWB(new FWBMessageDetails(header), FWB.Version.No16).ToString();
			AssertContains("NFY/T/USCI1234567", message);
		}

		public void TestConsigneeIsInChina_OCIIdEmpty_ConsigneeContactNameEmpty_NoOCI()
		{
			var airline = Factory.New<RefAirline>();
			airline.RM_TwoCharacterCode = "XX";
			airline.RM_EagleAddedAirlinePrefixOrAccountingCode = "XX";

			var header = CreateEmptyAWBHeaderWithConsol();
			header.IsImportToChinaForTest = true;
			header.EH_ConsigneeContactName = ZString.Empty;
			header.EH_By1st = "XX";

			AssertSegment(header, FWB.Version.No16, "OCI", string.Empty);
		}

		public void TestConsigneeIsInChina_OCIIdEmpty_ConsigneeContactNameNotEmpty_NoOCI()
		{
			var airline = Factory.New<RefAirline>();
			airline.RM_TwoCharacterCode = "XX";
			airline.RM_EagleAddedAirlinePrefixOrAccountingCode = "XX";

			var header = CreateEmptyAWBHeaderWithConsol();
			header.IsImportToChinaForTest = true;
			header.EH_ConsigneeContactName = "IGOR";
			header.EH_By1st = "XX";

			AssertSegment(header, FWB.Version.No16, "OCI", string.Empty);
		}

		public void TestConsigneeIsNotInChina_OCIIdNotEmpty_ConsigneeContactNameNotEmpty_NoOCIForCN()
		{
			var airline = Factory.New<RefAirline>();
			airline.RM_TwoCharacterCode = "XX";
			airline.RM_EagleAddedAirlinePrefixOrAccountingCode = "XX";
			airline.RM_ContactNameOCIIdentifier = "AB";

			var header = CreateEmptyAWBHeaderWithConsol();
			header.EH_ConsigneeContactName = "IGOR";
			header.EH_By1st = "XX";

			AssertSegment(header, FWB.Version.No16, "OCI", "OCI//CNE/CP/IGOR");
		}

		public void TestConsigneeIsInChina_OCIIdNotEmpty_ConsigneeContactNameEmpty_NoOCI()
		{
			var airline = Factory.New<RefAirline>();
			airline.RM_TwoCharacterCode = "XX";
			airline.RM_EagleAddedAirlinePrefixOrAccountingCode = "XX";
			airline.RM_ContactNameOCIIdentifier = "AB";

			var header = CreateEmptyAWBHeaderWithConsol();
			header.IsImportToChinaForTest = true;
			header.EH_ConsigneeContactName = ZString.Empty;
			header.EH_By1st = "XX";

			AssertSegment(header, FWB.Version.No16, "OCI", string.Empty);
		}

		public void TestConsigneeIsInChina_OCIIdNotEmpty_ConsigneeContactNameNotEmpty_OCIPresent()
		{
			var airline = Factory.New<RefAirline>();
			airline.RM_TwoCharacterCode = "XX";
			airline.RM_EagleAddedAirlinePrefixOrAccountingCode = "XX";
			airline.RM_ContactNameOCIIdentifier = "AB";

			var header = CreateEmptyAWBHeaderWithConsol();
			header.IsImportToChinaForTest = true;
			header.EH_ConsigneeContactName = "IGOR";
			header.EH_By1st = "XX";

			const string expected = @"OCI/CN/CNE/AB/IGOR";
			AssertSegment(header, FWB.Version.No16, "OCI", expected);
		}

		public void TestConsigneeIsInChina_OCIIdEmpty_ConsigneeContactDetailEmpty_NoOCI()
		{
			var airline = Factory.New<RefAirline>();
			airline.RM_TwoCharacterCode = "XX";
			airline.RM_EagleAddedAirlinePrefixOrAccountingCode = "XX";

			var header = CreateEmptyAWBHeaderWithConsol();
			header.IsImportToChinaForTest = true;
			header.EH_ConsigneeContactDetail = ZString.Empty;
			header.EH_By1st = "XX";

			AssertSegment(header, FWB.Version.No16, "OCI", string.Empty);
		}

		public void TestConsigneeIsInChina_OCIIdEmpty_ConsigneeContactDetailNotEmpty_NoOCI()
		{
			var airline = Factory.New<RefAirline>();
			airline.RM_TwoCharacterCode = "XX";
			airline.RM_EagleAddedAirlinePrefixOrAccountingCode = "XX";

			var header = CreateEmptyAWBHeaderWithConsol();
			header.IsImportToChinaForTest = true;
			header.EH_ConsigneeContactDetail = "111";
			header.EH_By1st = "XX";

			AssertSegment(header, FWB.Version.No16, "OCI", string.Empty);
		}

		public void TestConsigneeIsNotInChina_OCIIdNotEmpty_ConsigneeContactDetailNotEmpty_NoOCIForCN()
		{
			var airline = Factory.New<RefAirline>();
			airline.RM_TwoCharacterCode = "XX";
			airline.RM_EagleAddedAirlinePrefixOrAccountingCode = "XX";
			airline.RM_ContactPhoneOCIIdentifier = "AB";

			var header = CreateEmptyAWBHeaderWithConsol();
			header.EH_ConsigneeContactDetail = "111";
			header.EH_By1st = "XX";

			AssertSegment(header, FWB.Version.No16, "OCI", "OCI//CNE/CT/111");
		}

		public void TestConsigneeIsInChina_OCIIdNotEmpty_ConsigneeContactDetailEmpty_NoOCI()
		{
			var airline = Factory.New<RefAirline>();
			airline.RM_TwoCharacterCode = "XX";
			airline.RM_EagleAddedAirlinePrefixOrAccountingCode = "XX";
			airline.RM_ContactPhoneOCIIdentifier = "AB";

			var header = CreateEmptyAWBHeaderWithConsol();
			header.IsImportToChinaForTest = true;
			header.EH_ConsigneeContactDetail = ZString.Empty;
			header.EH_By1st = "XX";

			AssertSegment(header, FWB.Version.No16, "OCI", string.Empty);
		}

		public void TestConsigneeIsInChina_OCIIdNotEmpty_ConsigneeContactDetailNotEmpty_ConsigneeContactCodeFax_NoOCI()
		{
			var airline = Factory.New<RefAirline>();
			airline.RM_TwoCharacterCode = "XX";
			airline.RM_EagleAddedAirlinePrefixOrAccountingCode = "XX";
			airline.RM_ContactPhoneOCIIdentifier = "AB";

			var header = CreateEmptyAWBHeaderWithConsol();
			header.IsImportToChinaForTest = true;
			header.EH_ConsigneeContactDetail = "111";
			header.EH_ConsigneeContactCode = Core.Constants.AWB.ContactCodes.FAX;
			header.EH_By1st = "XX";

			AssertSegment(header, FWB.Version.No16, "OCI", string.Empty);
		}

		public void TestConsigneeIsInChina_OCIIdNotEmpty_ConsigneeContactDetailNotEmpty_OCIPresent()
		{
			var airline = Factory.New<RefAirline>();
			airline.RM_TwoCharacterCode = "XX";
			airline.RM_EagleAddedAirlinePrefixOrAccountingCode = "XX";
			airline.RM_ContactPhoneOCIIdentifier = "AB";

			var header = CreateEmptyAWBHeaderWithConsol();
			header.IsImportToChinaForTest = true;
			header.EH_ConsigneeContactDetail = "111";
			header.EH_By1st = "XX";

			const string expected = @"OCI/CN/CNE/AB/111";
			AssertSegment(header, FWB.Version.No16, "OCI", expected);
		}

		public void TestConsigneeIsInChina_ConsigneeTraderNoUsesTIdentifier()
		{
			var header = CreateEmptyAWBHeaderWithConsol();
			header.IsImportToChinaForTest = true;
			header.EH_ConsigneeCountryCode = CountryCodes.China;
			header.EH_ConsigneeTraderNo = "111";

			const string expected = @"OCI/CN/CNE/T/111";
			AssertSegment(header, FWB.Version.No16, "OCI", expected);
		}

		public void TestConsigneeIsInIndonesia_ConsigneeTraderNoUsesTIdentifier()
		{
			var header = CreateEmptyAWBHeaderWithConsol();
			header.EH_ConsigneeCountryCode = CountryCodes.Indonesia;
			header.EH_AirportOfDestinationCode = CountryCodes.Indonesia;
			header.DestinationCountryForTest = Factory.LoadFromNaturalKey<RefCountry>(RefCountrySchema.RN_Code, CountryCodes.Indonesia);
			header.EH_ConsigneeTraderNo = "111";

			const string expected = @"OCI/ID/CNE/T/111";
			AssertSegment(header, FWB.Version.No16, "OCI", expected);
		}

		public void TestShipperIsInIndonesia_ShipperTraderNoUsesTIdentifier()
		{
			var header = CreateEmptyAWBHeaderWithConsol();
			header.EH_ShipperCountryCode = CountryCodes.Indonesia;
			header.OriginCountryForTest = Factory.LoadFromNaturalKey<RefCountry>(RefCountrySchema.RN_Code, CountryCodes.Indonesia);
			header.EH_ShipperTraderNo = "111";

			const string expected = @"OCI/ID/SHP/T/111";
			AssertSegment(header, FWB.Version.No16, "OCI", expected);
		}

		public void TestConsigneeIsInKenya_ConsigneeTraderNoUsesTIdentifier()
		{
			var header = CreateEmptyAWBHeaderWithConsol();
			header.EH_ConsigneeCountryCode = CountryCodes.Kenya;
			header.EH_AirportOfDestinationCode = CountryCodes.Kenya;
			header.DestinationCountryForTest = Factory.LoadFromNaturalKey<RefCountry>(RefCountrySchema.RN_Code, CountryCodes.Kenya);
			header.EH_ConsigneeTraderNo = "111";

			const string expected = @"OCI/KE/CNE/T/111";
			AssertSegment(header, FWB.Version.No16, "OCI", expected);
		}

		public void TestConsigneeIsInKenya_OCIModifiesConsigneeTraderNoTypeIfPIN()
		{
			var header = CreateEmptyAWBHeaderWithConsol();
			header.EH_ConsigneeCountryCode = CountryCodes.Kenya;
			header.DestinationCountryForTest = Factory.LoadFromNaturalKey<RefCountry>(RefCountrySchema.RN_Code, CountryCodes.Kenya);
			header.EH_ConsigneeTraderNoType = "PIN";
			header.EH_ConsigneeTraderNo = "111";

			const string expected = @"OCI/KE/CNE/T/111";
			AssertSegment(header, FWB.Version.No16, "OCI", expected);
		}

		public void TestConsigneeIsInKenya_OCIIncludesConsigneeTraderNoTypeIfNotPIN()
		{
			var header = CreateEmptyAWBHeaderWithConsol();
			header.EH_ConsigneeCountryCode = CountryCodes.Kenya;
			header.DestinationCountryForTest = Factory.LoadFromNaturalKey<RefCountry>(RefCountrySchema.RN_Code, CountryCodes.Kenya);
			header.EH_ConsigneeTraderNoType = "XYZ";
			header.EH_ConsigneeTraderNo = "111";

			const string expected = @"OCI/KE/CNE/T/XYZ111";
			AssertSegment(header, FWB.Version.No16, "OCI", expected);
		}

		public void TestConsigneeIsInBrazil_ConsigneeTraderNoDoesNotContainSpecialCharacters()
		{
			var header = CreateEmptyAWBHeaderWithConsol();
			header.EH_ConsigneeCountryCode = CountryCodes.Brazil;
			header.DestinationCountryForTest = Factory.LoadFromNaturalKey<RefCountry>(RefCountrySchema.RN_Code, CountryCodes.Brazil);
			header.EH_ConsigneeTraderNoType = "CNPJ";
			header.EH_ConsigneeTraderNo = "13.339. 532/0001-08";

			const string expected = "OCI/BR/CNE/T/CNPJ13339532000108";
			AssertSegment(header, FWB.Version.No16, "OCI", expected);
		}

		public void TestOCILineConsigneeTraderNo_DoesNotIncludeBINPrefix_Bangladesh()
		{
			var header = CreateEmptyAWBHeaderWithConsol();
			header.EH_ConsigneeCountryCode = CountryCodes.Bangladesh;
			header.DestinationCountryForTest = Factory.LoadFromNaturalKey<RefCountry>(RefCountrySchema.RN_Code, CountryCodes.Bangladesh);
			header.EH_ConsigneeTraderNoType = OrgCusCode.BangladeshCodeTypes.BIN;
			header.EH_ConsigneeTraderNo = "111111111111111";

			const string expected = "OCI/BD/CNE/T/111111111111111";
			AssertSegment(header, FWB.Version.No16, "OCI", expected);
		}

		public void TestOCILineAlsoNotifyTraderNo_DoesNotIncludeBINPrefix_Bangladesh()
		{
			var header = CreateEmptyAWBHeaderWithConsol();
			header.EH_AlsoNotifyTraderNo = "111111111111111";
			header.EH_AlsoNotifyCountryCode = CountryCodes.Bangladesh;
			header.EH_AlsoNotifyTraderNoType = OrgCusCode.BangladeshCodeTypes.BIN;

			const string expected = @"OCI/BD/NFY/T/111111111111111";
			var message = new FWB(new FWBMessageDetails(header), FWB.Version.No16).ToString();
			AssertContains(expected, message);
		}

		public void TestOCILineConsigneeTraderNo_IncludeAINPrefix_Bangladesh()
		{
			var header = CreateEmptyAWBHeaderWithConsol();
			header.EH_ConsigneeCountryCode = CountryCodes.Bangladesh;
			header.DestinationCountryForTest = Factory.LoadFromNaturalKey<RefCountry>(RefCountrySchema.RN_Code, CountryCodes.Bangladesh);
			header.EH_ConsigneeTraderNoType = OrgCusCode.BangladeshCodeTypes.AIN;
			header.EH_ConsigneeTraderNo = "111111111111111";

			const string expected = "OCI/BD/CNE/T/AIN111111111111111";
			AssertSegment(header, FWB.Version.No16, "OCI", expected);
		}

		public void TestOCILineAlsoNotifyTraderNo_IncludeAINPrefix_Bangladesh()
		{
			var header = CreateEmptyAWBHeaderWithConsol();
			header.EH_AlsoNotifyTraderNo = "111111111111111";
			header.EH_AlsoNotifyCountryCode = CountryCodes.Bangladesh;
			header.EH_AlsoNotifyTraderNoType = OrgCusCode.BangladeshCodeTypes.AIN;

			const string expected = @"OCI/BD/NFY/T/AIN111111111111111";
			var message = new FWB(new FWBMessageDetails(header), FWB.Version.No16).ToString();
			AssertContains(expected, message);
		}

		public void TestOCIIncludes_ICEasConsigneeTraderNoType_Morocco()
		{
			var header = CreateEmptyAWBHeaderWithConsol();
			header.EH_ConsigneeCountryCode = CountryCodes.Morocco;
			header.DestinationCountryForTest = Factory.LoadFromNaturalKey<RefCountry>(RefCountrySchema.RN_Code, CountryCodes.Morocco);
			header.EH_ConsigneeTraderNoType = "ICE";
			header.EH_ConsigneeTraderNo = "111111111111111";

			const string expected = @"OCI/MA/CNE/T/111111111111111";
			AssertSegment(header, FWB.Version.No16, "OCI", expected);
		}

		public void TestOCIIncludes_ICEisMissing_ConsigneeTraderNo_HasDefaultValue_Morocco()
		{
			var header = CreateEmptyAWBHeaderWithConsol();
			header.EH_ConsigneeCountryCode = CountryCodes.Morocco;
			header.DestinationCountryForTest = Factory.LoadFromNaturalKey<RefCountry>(RefCountrySchema.RN_Code, CountryCodes.Morocco);
			header.EH_ConsigneeTraderNoType = "ICE";

			const string expected = @"OCI/MA/CNE/T/000000000000000";
			AssertSegment(header, FWB.Version.No16, "OCI", expected);
		}

		public void TestOCIIncludes_AlsoNotify_ICEasConsigneeTraderNoType_Morocco()
		{
			var header = CreateEmptyAWBHeaderWithConsol();
			header.EH_AlsoNotifyTraderNo = "111111111111111";
			header.EH_AlsoNotifyCountryCode = CountryCodes.Morocco;
			header.EH_AlsoNotifyTraderNoCountryCode = CountryCodes.Morocco;
			header.EH_AlsoNotifyTraderNoType = "ICE";

			const string expected = @"OCI/MA/NFY/T/111111111111111";
			var message = new FWB(new FWBMessageDetails(header), FWB.Version.No16).ToString();
			AssertContains(expected, message);
		}

		public void TestOCIIncludes_AlsoNotify_ICEisMissing_ConsigneeTraderNo_HasDefaultValue_Morocco()
		{
			var header = CreateEmptyAWBHeaderWithConsol();
			header.EH_AlsoNotifyCountryCode = CountryCodes.Morocco;
			header.EH_AlsoNotifyTraderNoCountryCode = CountryCodes.Morocco;
			header.EH_AlsoNotifyTraderNoType = "ICE";

			const string expected = @"OCI/MA/NFY/T/000000000000000";
			var message = new FWB(new FWBMessageDetails(header), FWB.Version.No16).ToString();
			AssertContains(expected, message);
		}

		public void TestOCIIncludes_NITasConsigneeTraderNoType_Bolivia()
		{
			var header = CreateEmptyAWBHeaderWithConsol();
			header.EH_ConsigneeCountryCode = CountryCodes.Bolivia;
			header.DestinationCountryForTest = Factory.LoadFromNaturalKey<RefCountry>(RefCountrySchema.RN_Code, CountryCodes.Bolivia);
			header.EH_ConsigneeTraderNoType = "NIT";
			header.EH_ConsigneeTraderNo = "886644221";

			const string expected = @"OCI/BO/CNE/T/NIT886644221";
			AssertSegment(header, FWB.Version.No16, "OCI", expected);
		}

		public void TestOCIIncludes_NITasShipperTraderNoType_Bolivia()
		{
			var header = CreateEmptyAWBHeaderWithConsol();
			header.EH_ShipperCountryCode = CountryCodes.Bolivia;
			header.OriginCountryForTest = Factory.LoadFromNaturalKey<RefCountry>(RefCountrySchema.RN_Code, CountryCodes.Bolivia);
			header.EH_ShipperTraderNoType = "NIT";
			header.EH_ShipperTraderNo = "886644221";

			const string expected = @"OCI/BO/SHP/T/NIT886644221";
			AssertSegment(header, FWB.Version.No16, "OCI", expected);
		}

		public void TestOCIIncludesConsigneeTraderNoType()
		{
			var header = CreateEmptyAWBHeaderWithConsol();
			header.EH_ConsigneeCountryCode = CountryCodes.Egypt;
			header.DestinationCountryForTest = Factory.LoadFromNaturalKey<RefCountry>(RefCountrySchema.RN_Code, CountryCodes.Egypt);
			header.EH_ConsigneeTraderNoType = "PIN";
			header.EH_ConsigneeTraderNo = "111";

			const string expected = @"OCI/EG/CNE/T/PIN111";
			AssertSegment(header, FWB.Version.No16, "OCI", expected);
		}

		public void TestACAS_CustomerAccountShippingFrequency_ShouldNotThrowNullRefException()
		{
			// A shipment attached to both road and air consols should not throw exception for ACAS
			// Create shipment(Beijing - Los Angeles)
			var shipper = Factory.NewWithValidTestData<OrgHeader>();
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.JS_TransportMode = TransportModes.Air;
			shipment.JS_RL_NKLoadPort = "CNBJS";
			shipment.JS_RL_NKDestination = "USLAX";
			shipment.ConsignorPK = shipper.PK;

			// Create Road consol(Beijing - Shanghai) and attach shipment
			var roadConsol = Factory.NewWithValidTestData<ForwardingConsol>();
			roadConsol.JK_AgentType = "DRT";
			roadConsol.JK_TransportMode = TransportModes.Road;
			roadConsol.JK_RL_NKLoadPort = "CNBJS";
			roadConsol.JK_RL_NKDischargePort = "CNSGH";
			roadConsol.Shipments.Add(shipment);

			// Create Air consol(Shanghai - Los Angeles) and attach same shipment
			var airConsolHeader = CreateEmptyAWBHeaderWithConsol();
			airConsolHeader.EH_WeightPrepaidCollect = "P";
			var airConsol = airConsolHeader.Consol;
			airConsol.JK_AgentType = "DRT";
			airConsol.JK_TransportMode = TransportModes.Air;
			airConsol.JK_RL_NKLoadPort = "CNSGH";
			airConsol.JK_RL_NKDischargePort = "USLAX";
			airConsol.JK_OverrideWaybillDefaults = true;
			airConsol.Shipments.Add(shipment);

			AssertNoExceptionThrown("Should not have Null Ref Exception", () =>
			{
				var message = new FWB(new FWBMessageDetails(airConsolHeader), FWB.Version.No16).ToString();
				AssertContains("/US/CUS/AF/O", message);
			});
		}

		public void TestOCI_CustomerAccountHolderAndName_NonDirectConsol()
		{
			var header = CreateEmptyAWBHeaderWithConsol();
			header.EH_ShipperCountryCode = "AU";
			header.EH_ShipperName = "Shipper Corp";
			header.EH_ConsigneeCountryCode = "US";
			header.EH_ConsigneeName = "Consignee Office";
			header.Consol.JK_RL_NKDischargePort = "USLAX";

			var expected = @"/US/CUS/AH/3
/US/CUS/AN/SHIPPER CORP";
			var message = new FWB(new FWBMessageDetails(header), FWB.Version.No16).ToString();
			AssertContains(expected, message);
		}

		public void TestOCI_CustomerAccountHolderAndName_DirectConsol()
		{
			var dataCreator = new AWBHeaderTestDataCreator(Factory);
			var consolHeader = dataCreator.ConsolAWBHeader;
			var consol = consolHeader.Consol;
			var shipmentHeader = dataCreator.ShipmentAWBHeader;
			var shipment = shipmentHeader.Shipment;
			string expected, message;

			consol.JK_RL_NKLoadPort = shipment.JS_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = shipment.JS_RL_NKDischargePort = "USLAX";
			consol.JK_AgentType = "DRT";
			consolHeader.EH_ShipperName = "Consol Shipper Name";
			consolHeader.EH_ConsigneeName = "Consol Consignee Name";
			shipmentHeader.EH_ShipperName = "Shipment Shipper Name";
			shipmentHeader.EH_ConsigneeName = "Shipment Consignee Name";

			var controllingCustomer = Factory.NewWithValidTestData<OrgHeader>();
			controllingCustomer.OH_FullName = "Controlling Customer Org Name";
			shipment.ControllingCustomerAddress.OrganisationPK = controllingCustomer.PK;
			expected = @"/US/CUS/AH/3
/US/CUS/AN/CONTROLLING CUSTOMER ORG NAME";
			message = new FWB(new FWBMessageDetails(consolHeader), FWB.Version.No16).ToString();
			AssertContains(expected, message);

			shipment.ControllingCustomerAddress.OrganisationPK = ZGuid.Empty;
			consolHeader.EH_WeightVPPDCOL = "PPD";
			expected = @"/US/CUS/AH/S
/US/CUS/AN/CONSOL SHIPPER NAME";
			message = new FWB(new FWBMessageDetails(consolHeader), FWB.Version.No16).ToString();
			AssertContains(expected, message);

			consolHeader.EH_WeightVPPDCOL = "COL";
			expected = @"/US/CUS/AH/C
/US/CUS/AN/CONSOL CONSIGNEE NAME";
			message = new FWB(new FWBMessageDetails(consolHeader), FWB.Version.No16).ToString();
			AssertContains(expected, message);

			consolHeader.EH_WeightPrepaidCollect = "";
			consolHeader.EH_WeightVPPDCOL = "";
			shipmentHeader.EH_WeightVPPDCOL = "PPD";
			expected = @"/US/CUS/AH/S
/US/CUS/AN/SHIPMENT SHIPPER NAME";
			message = new FWB(new FWBMessageDetails(consolHeader), FWB.Version.No16).ToString();
			AssertContains(expected, message);

			shipmentHeader.EH_WeightVPPDCOL = "COL";
			expected = @"/US/CUS/AH/C
/US/CUS/AN/SHIPMENT CONSIGNEE NAME";
			message = new FWB(new FWBMessageDetails(consolHeader), FWB.Version.No16).ToString();
			AssertContains(expected, message);

			shipmentHeader.EH_WeightVPPDCOL = "BTH";
			shipment.JS_INCO = IncoTerms.FreeOnBoard;
			expected = @"/US/CUS/AH/C
/US/CUS/AN/SHIPMENT CONSIGNEE NAME";
			message = new FWB(new FWBMessageDetails(consolHeader), FWB.Version.No16).ToString();
			AssertContains(expected, message);

			shipment.JS_INCO = IncoTerms.DeliveredAtPlace;
			expected = @"/US/CUS/AH/S
/US/CUS/AN/SHIPMENT SHIPPER NAME";
			message = new FWB(new FWBMessageDetails(consolHeader), FWB.Version.No16).ToString();
			AssertContains(expected, message);
		}

		public void TestOCI_CustomerAccountIssuerAndNumber_NonDirectConsol()
		{
			var refAirline = Factory.NewWithValidTestData<RefAirline>();
			refAirline.RM_TwoCharacterCode = "LH";
			refAirline.RM_EagleAddedAirlinePrefixOrAccountingCode = "020";

			var carrierOrg = Factory.NewWithValidTestData<OrgHeader>();
			carrierOrg.CompanyData.OB_APAirlineAccountNumber = "Air Acc Num";
			carrierOrg.MiscServ.OM_RM_Airline = refAirline.PK;

			var header = CreateEmptyAWBHeaderWithConsol();
			header.Consol.JK_OA_ShippingLineAddress = carrierOrg.MainAddress.PK;
			header.Consol.JK_RL_NKDischargePort = "USLAX";

			var expected = @"/US/CUS/AI/020
/US/CUS/AR/AIRACCNUM";
			var message = new FWB(new FWBMessageDetails(header), FWB.Version.No16).ToString();
			AssertContains(expected, message);
			AssertNotContains("/US/CUS/AI/LH", message);

			var orgAirlineBranchAccount = carrierOrg.OrgAirlineBranchAccounts.AddNew();
			orgAirlineBranchAccount.OAA_GB_Branch = GlbBranch.CurrentBranch.PK;
			orgAirlineBranchAccount.OAA_APAirlineAccountNumber = "AccNum-CurBrh";
			refAirline.RM_TwoCharacterCode = "CN";
			refAirline.RM_EagleAddedAirlinePrefixOrAccountingCode = "999";

			expected = @"/US/CUS/AI/999
/US/CUS/AR/ACCNUMCURBRH";
			message = new FWB(new FWBMessageDetails(header), FWB.Version.No16).ToString();
			AssertContains(expected, message);
			AssertNotContains("/US/CUS/AI/CN", message);

			refAirline.RM_EagleAddedAirlinePrefixOrAccountingCode = "";
			message = new FWB(new FWBMessageDetails(header), FWB.Version.No16).ToString();
			AssertNotContains("/US/CUS/AI", message);
			AssertNotContains("/US/CUS/AR", message);
		}

		public void TestOCI_CustomerAccountShippingFrequency_NonDirectConsol()
		{
			// 1 consol - occasional
			var sendingAgent = Factory.NewWithValidTestData<OrgHeader>();
			var header = CreateConsols(1, DateTime.Now.AddDays(1)).First().AWBHeader;
			var message = new FWB(new FWBMessageDetails(header), FWB.Version.No16).ToString();
			AssertContains("/US/CUS/AF/O", message);

			// Add 10 consols - occasional
			CreateConsols(10, DateTime.Now.AddDays(-5));
			message = new FWB(new FWBMessageDetails(header), FWB.Version.No16).ToString();
			AssertContains("/US/CUS/AF/O", message);

			// Add 30 consols with future ETD. This should not affect the result
			CreateConsols(30, DateTime.Now.AddDays(5));
			message = new FWB(new FWBMessageDetails(header), FWB.Version.No16).ToString();
			AssertContains("/US/CUS/AF/O", message);

			// Add up to 30 consols - regular
			CreateConsols(20, DateTime.Now.AddDays(-5));
			message = new FWB(new FWBMessageDetails(header), FWB.Version.No16).ToString();
			AssertContains("/US/CUS/AF/B", message);

			// Add another 30 consols with future ETD. This should not affect the result
			CreateConsols(30, DateTime.Now.AddDays(5));
			message = new FWB(new FWBMessageDetails(header), FWB.Version.No16).ToString();
			AssertContains("/US/CUS/AF/B", message);

			// Add to > 60 consols - high
			CreateConsols(31, DateTime.Now.AddDays(-5));
			message = new FWB(new FWBMessageDetails(header), FWB.Version.No16).ToString();
			AssertContains("/US/CUS/AF/R", message);

			// Immediate Transaction, no matter how many consols the org has
			sendingAgent.OH_IsTempAccount = true;
			message = new FWB(new FWBMessageDetails(header), FWB.Version.No16).ToString();
			AssertContains("/US/CUS/AF/I", message);

			List<ForwardingConsol> CreateConsols(int numbOfConsol, ZDateTime consolETD)
			{
				var consols = new List<ForwardingConsol>();
				for (var i = 0; i < numbOfConsol; i++)
				{
					var consolHeader = CreateEmptyAWBHeaderWithConsol();
					consolHeader.Consol.JK_AgentType = "AGT";
					consolHeader.Consol.JK_TransportMode = "AIR";
					consolHeader.Consol.JK_RL_NKDischargePort = "USLAX";

					consolHeader.Consol.JK_OA_SendingForwarderAddress = sendingAgent.MainAddress.PK;
					consolHeader.Consol.Transports[0].JW_ETD = consolETD;
					consols.Add(consolHeader.Consol);
				}
				Factory.Save();
				return consols;
			}
		}

		public void TestOCI_CustomerAccountShippingFrequency_DirectConsol()
		{
			var controllingCustomer = Factory.NewWithValidTestData<OrgHeader>();
			var shipper = Factory.NewWithValidTestData<OrgHeader>();
			var consignee = Factory.NewWithValidTestData<OrgHeader>();
			var currentConsol = CreateConsols(1, DateTime.Now.AddDays(1)).First();

			// 1 shipment
			var message = new FWB(new FWBMessageDetails(currentConsol.AWBHeader), FWB.Version.No16).ToString();
			AssertContains("/US/CUS/AF/O", message);

			// Add 30 shipments in future ETD.This should not affect result
			CreateConsols(30, DateTime.Now.AddDays(5));
			message = new FWB(new FWBMessageDetails(currentConsol.AWBHeader), FWB.Version.No16).ToString();
			AssertContains("/US/CUS/AF/O", message);

			// add 30 shipments
			CreateConsols(30, DateTime.Now.AddDays(-5));
			message = new FWB(new FWBMessageDetails(currentConsol.AWBHeader), FWB.Version.No16).ToString();
			AssertContains("/US/CUS/AF/B", message);

			// Add another 30 shipments in future ETD.This should not affect result
			CreateConsols(30, DateTime.Now.AddDays(5));
			message = new FWB(new FWBMessageDetails(currentConsol.AWBHeader), FWB.Version.No16).ToString();
			AssertContains("/US/CUS/AF/B", message);

			// Greater than 60 shipments 
			CreateConsols(31, DateTime.Now.AddDays(-5));
			message = new FWB(new FWBMessageDetails(currentConsol.AWBHeader), FWB.Version.No16).ToString();
			AssertContains("/US/CUS/AF/R", message);

			// Immediate Transaction
			controllingCustomer.OH_IsTempAccount = true;
			message = new FWB(new FWBMessageDetails(currentConsol.AWBHeader), FWB.Version.No16).ToString();
			AssertContains("/US/CUS/AF/I", message);

			List<ForwardingConsol> CreateConsols(int numOfConsol, ZDateTime consolETD)
			{
				var consols = new List<ForwardingConsol>();
				for (var i = 0; i < numOfConsol; i++)
				{
					var header = CreateEmptyAWBHeaderWithConsol();
					var consol = header.Consol;
					consol.JK_AgentType = "DRT";
					consol.JK_TransportMode = "AIR";
					consol.JK_RL_NKDischargePort = "USLAX";
					consol.Transports[0].JW_ETD = consolETD;

					var shipment = consol.Shipments.AddNew();
					shipment.JS_TransportMode = Core.Constants.TransportModes.Air;
					shipment.JS_RL_NKDestination = "USLAX";
					shipment.ControllingCustomerAddress.OrganisationPK = controllingCustomer.PK;
					shipment.ConsignorPK = shipper.PK;
					shipment.ConsigneePK = consignee.PK;
					consols.Add(header.Consol);
				}
				Factory.Save();
				return consols;
			}
		}

		public void TestOCI_CustomerAccountIssuerAndNumber_DirectConsol()
		{
			var dataCreator = new AWBHeaderTestDataCreator(Factory);
			var consolHeader = dataCreator.ConsolAWBHeader;
			var consol = consolHeader.Consol;
			var shipmentHeader = dataCreator.ShipmentAWBHeader;
			var shipment = shipmentHeader.Shipment;
			string expected, message;

			consol.JK_RL_NKLoadPort = shipment.JS_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = shipment.JS_RL_NKDischargePort = "USLAX";
			consol.JK_AgentType = "DRT";
			consolHeader.EH_ShipperAccount = "Shipper Acc";
			consolHeader.EH_ConsigneeAccount = "Consignee Acc";

			var controllingCustomer = Factory.NewWithValidTestData<OrgHeader>();
			shipment.ControllingCustomerAddress.OrganisationPK = controllingCustomer.PK;
			controllingCustomer.OH_Code = "CtrlCustCode";

			var proxyOrgCusCode = GlbBranch.CurrentBranch.OrgProxy.CustomsCodes.AddNew();
			proxyOrgCusCode.OK_RN_NKCodeCountry = "US";
			proxyOrgCusCode.OK_CodeType = "CCA";
			proxyOrgCusCode.SecuredCustomsRegNo = "USCCAREGNO";

			expected = @"/US/CUS/AI/USCCAREGNO
/US/CUS/AR/CTRLCUSTCODE";
			message = new FWB(new FWBMessageDetails(consolHeader), FWB.Version.No16).ToString();
			AssertContains(expected, message);

			shipment.ControllingCustomerAddress.OrganisationPK = ZGuid.Empty;
			proxyOrgCusCode.OK_RN_NKCodeCountry = "AU";
			consolHeader.EH_WeightPrepaidCollect = "P";
			expected = @"/US/CUS/AI/33605250151
/US/CUS/AR/SHIPPERACC";
			message = new FWB(new FWBMessageDetails(consolHeader), FWB.Version.No16).ToString();
			AssertContains(expected, message);

			consolHeader.EH_WeightPrepaidCollect = "C";
			expected = @"/US/CUS/AI/33605250151
/US/CUS/AR/CONSIGNEEACC";
			message = new FWB(new FWBMessageDetails(consolHeader), FWB.Version.No16).ToString();
			AssertContains(expected, message);

			consolHeader.EH_WeightPrepaidCollect = "";
			shipmentHeader.EH_WeightVPPDCOL = "PPD";
			expected = @"/US/CUS/AI/33605250151
/US/CUS/AR/SHIPPERACC";
			message = new FWB(new FWBMessageDetails(consolHeader), FWB.Version.No16).ToString();
			AssertContains(expected, message);

			shipmentHeader.EH_WeightVPPDCOL = "COL";
			expected = @"/US/CUS/AI/33605250151
/US/CUS/AR/CONSIGNEEACC";
			message = new FWB(new FWBMessageDetails(consolHeader), FWB.Version.No16).ToString();
			AssertContains(expected, message);

			shipmentHeader.EH_WeightVPPDCOL = "BTH";
			shipment.JS_INCO = IncoTerms.DeliveredAtPlace;
			expected = @"/US/CUS/AI/33605250151
/US/CUS/AR/SHIPPERACC";
			message = new FWB(new FWBMessageDetails(consolHeader), FWB.Version.No16).ToString();
			AssertContains(expected, message);

			shipment.JS_INCO = IncoTerms.FreeOnBoard;
			expected = @"/US/CUS/AI/33605250151
/US/CUS/AR/CONSIGNEEACC";
			message = new FWB(new FWBMessageDetails(consolHeader), FWB.Version.No16).ToString();
			AssertContains(expected, message);
		}

		public void TestOCI_CustomerAccountEstablishmentDateAndBillingType_NonDirectConsol()
		{
			string oldDefaultPaymentType = AccountingConfigurationRegistry.Instance.DefaultPaymentType.Value;
			try
			{
				AccountingConfigurationRegistry.Instance.DefaultPaymentType.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, "CHQ");
				var sendingAgent = Factory.NewWithValidTestData<OrgHeader>();
				var header = CreateEmptyAWBHeaderWithConsol();
				header.Consol.JK_AgentType = "AGT";
				header.Consol.JK_TransportMode = "AIR";
				header.Consol.JK_RL_NKDischargePort = "USLAX";
				header.Consol.JK_OA_SendingForwarderAddress = sendingAgent.MainAddress.PK;
				header.Consol.Transports[0].JW_ETD = DateTime.Now.AddDays(-5);

				var message = new FWB(new FWBMessageDetails(header), FWB.Version.No16).ToString();
				AssertNotContains("/US/CUS/AE", message);
				AssertNotContains("/US/CUS/BT", message);

				sendingAgent.OH_IsCreditor = false;
				sendingAgent.CompanyData.OB_APCreditAgreedPaymentMethod = "CBC";

				message = new FWB(new FWBMessageDetails(header), FWB.Version.No16).ToString();
				AssertNotContains("/US/CUS/AE", message);
				AssertNotContains("/US/CUS/BT", message);

				sendingAgent.OH_IsCreditor = true;
				message = new FWB(new FWBMessageDetails(header), FWB.Version.No16).ToString();
				AssertNotContains("/US/CUS/AE", message);

				sendingAgent.OH_SystemCreateTimeUtc = new ZDateTime(2024, 06, 12, 12, 30, 00);
				var expected = @"/US/CUS/AE/12JUN24
/US/CUS/BT/CSH";
				message = new FWB(new FWBMessageDetails(header), FWB.Version.No16).ToString();
				AssertContains(expected, message);

				sendingAgent.CompanyData.OB_APCreditAgreedPaymentMethod = "INV";
				expected = @"/US/CUS/AE/12JUN24
/US/CUS/BT/EFT";
				message = new FWB(new FWBMessageDetails(header), FWB.Version.No16).ToString();
				AssertContains(expected, message);

				sendingAgent.CompanyData.OB_APCreditAgreedPaymentMethod = "";
				expected = @"/US/CUS/AE/12JUN24
/US/CUS/BT/CHQ";
				message = new FWB(new FWBMessageDetails(header), FWB.Version.No16).ToString();
				AssertContains(expected, message);

				sendingAgent.OH_SystemCreateTimeUtc = new ZDateTime(2023, 07, 09, 12, 30, 00);
				expected = @"/US/CUS/AE/09JUL23
/US/CUS/BT/CHQ";
				message = new FWB(new FWBMessageDetails(header), FWB.Version.No16).ToString();
				AssertContains(expected, message);

				sendingAgent.OH_SystemCreateTimeUtc = ZDateTime.Empty;
				message = new FWB(new FWBMessageDetails(header), FWB.Version.No16).ToString();
				AssertNotContains("/US/CUS/AE", message);
			}
			finally
			{
				AccountingConfigurationRegistry.Instance.DefaultPaymentType.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, oldDefaultPaymentType);
			}
		}

		public void TestOCI_CustomerAccountEstablishmentDateAndBillingType_DirectConsol()
		{
			var dataCreator = new AWBHeaderTestDataCreator(Factory);
			var consolHeader = dataCreator.ConsolAWBHeader;
			var consol = consolHeader.Consol;
			var shipmentHeader = dataCreator.ShipmentAWBHeader;
			var shipment = shipmentHeader.Shipment;
			string expected, message;

			consol.JK_RL_NKLoadPort = shipment.JS_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = shipment.JS_RL_NKDischargePort = "USLAX";
			consol.JK_AgentType = "DRT";

			var controllingCustomer = Factory.NewWithValidTestData<OrgHeader>();
			shipment.ControllingCustomerAddress.OrganisationPK = controllingCustomer.PK;
			controllingCustomer.OH_Code = "CtrlCustCode";
			controllingCustomer.OH_IsCreditor = false;
			controllingCustomer.CompanyData.OB_APCreditAgreedPaymentMethod = "CCD";

			var consignor = Factory.NewWithValidTestData<OrgHeader>();
			shipment.ConsignorPK = consignor.PK;
			consignor.OH_Code = "Consignor";
			consignor.OH_IsCreditor = false;
			consignor.CompanyData.OB_APCreditAgreedPaymentMethod = "CHK";

			var consignee = Factory.NewWithValidTestData<OrgHeader>();
			shipment.ConsigneePK = consignee.PK;
			consignee.OH_Code = "Consignee";
			consignee.OH_IsCreditor = false;
			consignee.CompanyData.OB_APCreditAgreedPaymentMethod = "TRF";

			var proxyOrgCusCode = GlbBranch.CurrentBranch.OrgProxy.CustomsCodes.AddNew();
			proxyOrgCusCode.OK_RN_NKCodeCountry = "US";
			proxyOrgCusCode.OK_CodeType = "CCA";
			proxyOrgCusCode.SecuredCustomsRegNo = "USCCAREGNO";

			message = new FWB(new FWBMessageDetails(consolHeader), FWB.Version.No16).ToString();
			AssertNotContains("/US/CUS/AE", message);
			AssertNotContains("/US/CUS/BT", message);

			controllingCustomer.OH_IsCreditor = true;
			message = new FWB(new FWBMessageDetails(consolHeader), FWB.Version.No16).ToString();
			AssertNotContains("/US/CUS/AE", message);

			controllingCustomer.OH_SystemCreateTimeUtc = new ZDateTime(2024, 06, 12, 12, 30, 00);
			consignor.OH_SystemCreateTimeUtc = new ZDateTime(2024, 06, 13, 12, 30, 00);
			consignee.OH_SystemCreateTimeUtc = new ZDateTime(2024, 06, 14, 12, 30, 00);
			expected = @"/US/CUS/AE/12JUN24
/US/CUS/BT/CC";
			message = new FWB(new FWBMessageDetails(consolHeader), FWB.Version.No16).ToString();
			AssertContains(expected, message);

			shipment.ControllingCustomerAddress.OrganisationPK = ZGuid.Empty;
			consolHeader.EH_WeightPrepaidCollect = "P";

			expected = @"/US/CUS/AE/13JUN24";
			message = new FWB(new FWBMessageDetails(consolHeader), FWB.Version.No16).ToString();
			AssertContains(expected, message);
			AssertNotContains("/US/CUS/BT", message);

			consignor.OH_IsCreditor = true;
			expected = @"/US/CUS/AE/13JUN24
/US/CUS/BT/CHQ";
			message = new FWB(new FWBMessageDetails(consolHeader), FWB.Version.No16).ToString();
			AssertContains(expected, message);

			consolHeader.EH_WeightPrepaidCollect = "C";
			expected = @"/US/CUS/AE/14JUN24";
			message = new FWB(new FWBMessageDetails(consolHeader), FWB.Version.No16).ToString();
			AssertContains(expected, message);
			AssertNotContains("/US/CUS/BT", message);

			consignee.OH_IsCreditor = true;
			expected = @"/US/CUS/AE/14JUN24
/US/CUS/BT/EFT";
			message = new FWB(new FWBMessageDetails(consolHeader), FWB.Version.No16).ToString();
			AssertContains(expected, message);

			consolHeader.EH_WeightPrepaidCollect = "";
			shipmentHeader.EH_WeightVPPDCOL = "PPD";
			expected = @"/US/CUS/AE/13JUN24
/US/CUS/BT/CHQ";
			message = new FWB(new FWBMessageDetails(consolHeader), FWB.Version.No16).ToString();
			AssertContains(expected, message);

			shipmentHeader.EH_WeightVPPDCOL = "COL";
			expected = @"/US/CUS/AE/14JUN24
/US/CUS/BT/EFT";
			message = new FWB(new FWBMessageDetails(consolHeader), FWB.Version.No16).ToString();
			AssertContains(expected, message);

			shipmentHeader.EH_WeightVPPDCOL = "BTH";
			shipment.JS_INCO = IncoTerms.DeliveredAtPlace;
			expected = @"/US/CUS/AE/13JUN24
/US/CUS/BT/CHQ";
			message = new FWB(new FWBMessageDetails(consolHeader), FWB.Version.No16).ToString();
			AssertContains(expected, message);

			shipment.JS_INCO = IncoTerms.FreeOnBoard;
			expected = @"/US/CUS/AE/14JUN24
/US/CUS/BT/EFT";
			message = new FWB(new FWBMessageDetails(consolHeader), FWB.Version.No16).ToString();
			AssertContains(expected, message);

			consignee.OH_SystemCreateTimeUtc = new ZDateTime(2023, 07, 09, 12, 30, 00);
			expected = @"/US/CUS/AE/09JUL23
/US/CUS/BT/EFT";
			message = new FWB(new FWBMessageDetails(consolHeader), FWB.Version.No16).ToString();
			AssertContains(expected, message);

			consignee.OH_SystemCreateTimeUtc = ZDateTime.Empty;
			message = new FWB(new FWBMessageDetails(consolHeader), FWB.Version.No16).ToString();
			AssertNotContains("/US/CUS/AE", message);
		}

		public void TestOCI_BiographicData()
		{
			// Setup orgs
			var controllingCustomer = Factory.NewWithValidTestData<OrgHeader>();
			var shipper = Factory.NewWithValidTestData<OrgHeader>();
			var consignee = Factory.NewWithValidTestData<OrgHeader>();
			controllingCustomer.OH_Category = shipper.OH_Category = consignee.OH_Category = "NAT";

			var cusCode = controllingCustomer.CustomsCodes.AddNew();
			cusCode.OK_RN_NKCodeCountry = "US";
			cusCode.OK_CodeType = OrgCusCode.CodeTypes.PassportID;
			cusCode.SecuredCustomsRegNo = "UsPassport123";

			cusCode = shipper.CustomsCodes.AddNew();
			cusCode.OK_RN_NKCodeCountry = "AU";
			cusCode.OK_CodeType = OrgCusCode.CodeTypes.PassportID;
			cusCode.SecuredCustomsRegNo = "AuPassport456";

			cusCode = consignee.CustomsCodes.AddNew();
			cusCode.OK_RN_NKCodeCountry = "NZ";
			cusCode.OK_CodeType = OrgCusCode.CodeTypes.DriverLicenceID;
			cusCode.SecuredCustomsRegNo = "NzDriverLicense000";

			// Direct consol
			var consolHeader = CreateEmptyAWBHeaderWithConsol();
			var consol = consolHeader.Consol;
			consol.JK_OverrideWaybillDefaults = true;
			consol.JK_AgentType = "DRT";
			consol.JK_TransportMode = TransportModes.Air;
			consol.JK_RL_NKDischargePort = "USLAX";

			var shipment = consol.Shipments.AddNew();
			shipment.JS_TransportMode = TransportModes.Air;
			shipment.JS_RL_NKOrigin = consol.JK_RL_NKLoadPort = "AUSYD";
			shipment.JS_RL_NKDestination = "USLAX";
			shipment.ControllingCustomerAddress.OrganisationPK = controllingCustomer.PK;
			shipment.ConsignorPK = shipper.PK;
			shipment.ConsigneePK = consignee.PK;

			// Bio data from ctrl customer
			var expected = "US/CUS/PI/PPT-US-USPASSPORT123";
			var message = new FWB(new FWBMessageDetails(consolHeader), FWB.Version.No16).ToString();
			AssertContains(expected, message);

			// Bio data from shipper
			shipment.ControllingCustomerAddress.OrganisationPK = ZGuid.Empty;
			consolHeader.EH_WeightVPPDCOL = "PPD";
			expected = "US/CUS/PI/PPT-AU-AUPASSPORT456";
			message = new FWB(new FWBMessageDetails(consolHeader), FWB.Version.No16).ToString();
			AssertContains(expected, message);

			// Bio data from consignee
			consolHeader.EH_WeightVPPDCOL = "COL";
			expected = "US/CUS/PI/DL-NZ-NZDRIVERLICENSE000";
			message = new FWB(new FWBMessageDetails(consolHeader), FWB.Version.No16).ToString();
			AssertContains(expected, message);

			// Non-direct consol should not have bio data line
			consol.JK_AgentType = "AGT";
			message = new FWB(new FWBMessageDetails(consolHeader), FWB.Version.No16).ToString();
			AssertNotContains("US/CUS/PI/", message);
		}

		public void TestOCI_IncludeShipperAndConsigneeEmail_ByDestination()
		{
			TestCase("USLAX", true);
			TestCase("AUSYD", false);
			TestCase("PRCAG", true);
			TestCase("VIAGL", true);
			TestCase("GUDED", true);
			TestCase("MPROP", true);
			TestCase("ASAPI", true);

			void TestCase(string destCountryCode, bool shouldIncludeEmail)
			{
				var header = CreateEmptyAWBHeaderWithConsol();
				header.EH_ShipperCountryCode = "AU";
				header.EH_ShipperContactEmail = "shipper!\"#$%&'()*+,:;@abc[-:]!.com.au";
				header.EH_ConsigneeCountryCode = "US";
				header.EH_ConsigneeContactEmail = "consignee<=>?[\\]^_`{|}~@bcd[-:]!.com";
				header.Consol.JK_RL_NKDischargePort = destCountryCode;

				var expected = @"OCI/US/SHP/MU/SHIPPER!""#$%&'()*+,:;
/US/SHP/MD/ABC[-:].COM.AU
/US/CNE/MU/CONSIGNEE<=>?[\]^_`{|}~
/US/CNE/MD/BCD[-:].COM";
				var message = new FWB(new FWBMessageDetails(header), FWB.Version.No16).ToString();
				if (shouldIncludeEmail)
				{
					AssertContains(expected, message);
				}
				else
				{
					AssertNotContains(expected, message);
				}
			}
		}

		public void TestOCI_IncludeShipperAndConsigneeEmail_ByTransport()
		{
			TestCase(new string[] { "AUSYD", "USLAX", "NLABC" }, true);
			TestCase(new string[] { "AUSYD", "HKHKG", "NLABC" }, false);
			TestCase(new string[] { "AUSYD", "HKHKG", "USATL", "NLABC" }, true);
			TestCase(new string[] { "AUSYD", "HKHKG", "PRCAG", "NLABC" }, true);
			TestCase(new string[] { "AUSYD", "HKHKG", "VIAGL", "NLABC" }, true);
			TestCase(new string[] { "AUSYD", "HKHKG", "GUDED", "NLABC" }, true);
			TestCase(new string[] { "AUSYD", "HKHKG", "MPROP", "NLABC" }, true);
			TestCase(new string[] { "AUSYD", "ASAPI", "AUMEL", "NLABC" }, true);

			void TestCase(string[] ports, bool shouldIncludeEmail)
			{
				var header = CreateEmptyAWBHeaderWithConsol();
				header.EH_ShipperCountryCode = "AU";
				header.EH_ShipperContactEmail = "shipper!\"#$%&'()*+,:;@abc[-:]!.com.au";
				header.EH_ConsigneeCountryCode = "US";
				header.EH_ConsigneeContactEmail = "consignee<=>?[\\]^_`{|}~@bcd[-:]!.com";
				header.Consol.JK_RL_NKLoadPort = ports[0];
				header.Consol.JK_RL_NKDischargePort = ports.Last();

				for (var i = 0; i < ports.Length - 1; i++)
				{
					var transport = header.Consol.Transports.AddNew();
					transport.JW_RL_NKLoadPort = ports[i];
					transport.JW_RL_NKDiscPort = ports[i + 1];
				}

				var expected = @"OCI/US/SHP/MU/SHIPPER!""#$%&'()*+,:;
/US/SHP/MD/ABC[-:].COM.AU
/US/CNE/MU/CONSIGNEE<=>?[\]^_`{|}~
/US/CNE/MD/BCD[-:].COM";
				var message = new FWB(new FWBMessageDetails(header), FWB.Version.No16).ToString();
				if (shouldIncludeEmail)
				{
					AssertContains(expected, message);
				}
				else
				{
					AssertNotContains(expected, message);
				}
			}
		}

		public void TestOCI_VerifiedKnownConsignor()
		{
			TestCase("AUSYD", new string[] { "APP" }, "Y", shouldOnlyCheckLocalClient: false, hasValidKnownStatus: true, shouldIncludeVKC: false, "The route does not pass through the United States, so there will be no indicators in the OCI segment.");
			TestCase("USLAX", new string[] { "APP" }, "Y", shouldOnlyCheckLocalClient: false, hasValidKnownStatus: true, shouldIncludeVKC: true, "The route passes through the United States and the Inspection Type is APP, so the indicator appears in the OCI segment and is Y.");
			TestCase("USLAX", new string[] { "APP", "APP" }, "Y", shouldOnlyCheckLocalClient: false, hasValidKnownStatus: false, shouldIncludeVKC: true, "The route passes through the United States and the Inspection Type Codes are APP, so the indicator appears in the OCI segment and is Y.");
			TestCase("USLAX", new string[] { "UKN", "PHS" }, "Y", shouldOnlyCheckLocalClient: false, hasValidKnownStatus: true, shouldIncludeVKC: true, "The route passes through the United States and the Inspection Type Codes are not APP, but Consignor has a valid Known status, so the indicator appears in the OCI segment and is Y");
			TestCase("USLAX", new string[] { "APP", "UKN" }, "N", shouldOnlyCheckLocalClient: false, hasValidKnownStatus: false, shouldIncludeVKC: true, "The route passes through the United States and the Inspection Type Codes are not APP, but neither Consignor nor Local Client has a valid Known status, so the indicator appears in the OCI segment and is N.");
			TestCase("USLAX", new string[] { "APP", "UKN" }, "Y", shouldOnlyCheckLocalClient: true, hasValidKnownStatus: true, shouldIncludeVKC: true, "The route passes through the United States and the Inspection Type Codes are not APP, but Local Client has valid Known status, so the indicator appears in the OCI segment and is Y.");
			TestCase("USLAX", new string[] { "APP", "UKN" }, "N", shouldOnlyCheckLocalClient: true, hasValidKnownStatus: false, shouldIncludeVKC: true, "The route passes through the United States and the Inspection Type Codes are not APP, but Local Client doesn't have a valid Known status, so the indicator appears in the OCI segment and is N.");
			
			void TestCase(string destPortCode, string[] inspectionTypes, string knownConsignorCode, bool shouldOnlyCheckLocalClient, bool hasValidKnownStatus, bool shouldIncludeVKC, string reason)
			{
				var settings = FreightDataRegistry.Instance.ShipmentInspectionOrganisationToUse_Australia.Value;
				if (shouldOnlyCheckLocalClient)
				{
					((SupplyChainSecurityOrganisationToUse)settings.FindByCode(SupplyChainSecurityOrganisationTypes.Consignor)).ValidationCode = SupplyChainSecurityOrganisationToUse.ValidationCodes.No;
					((SupplyChainSecurityOrganisationToUse)settings.FindByCode(SupplyChainSecurityOrganisationTypes.LocalClient)).ValidationCode = SupplyChainSecurityOrganisationToUse.ValidationCodes.Yes;
				}
				else
				{
					((SupplyChainSecurityOrganisationToUse)settings.FindByCode(SupplyChainSecurityOrganisationTypes.Consignor)).ValidationCode = SupplyChainSecurityOrganisationToUse.ValidationCodes.Yes;
					((SupplyChainSecurityOrganisationToUse)settings.FindByCode(SupplyChainSecurityOrganisationTypes.LocalClient)).ValidationCode = SupplyChainSecurityOrganisationToUse.ValidationCodes.Yes;
				}

				using (FreightDataRegistry.Instance.ShipmentInspectionOrganisationToUse_Australia.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, settings))
				{
					var header = CreateEmptyAWBHeaderWithConsol();
					header.EH_ShipperCountryCode = "AU";
					header.EH_ConsigneeCountryCode = "US";
					header.Consol.JK_RL_NKDischargePort = destPortCode;

					var consignor = Factory.NewWithValidTestData<OrgHeader>();
					consignor.OH_Code = "CON";

					var localClient = Factory.NewWithValidTestData<OrgHeader>();
					localClient.OH_Code = "LOC";

					if (hasValidKnownStatus)
					{
						consignor.MainAddress.OA_RN_NKCountryCode = CountryCodes.Australia;

						var addressCountryData1 = consignor.MainAddress.KnownShipperDetails.AddNew();
						addressCountryData1.OV_OH_OrgHeader = consignor.PK;
						addressCountryData1.OV_EXApprovedOrMajorExporter = AviationSecuritySchemeMembership.Codes.KnownConsignor;
						addressCountryData1.OV_EXApprovalExpiryDate = ZDate.Today.AddDays(1);

						localClient.MainAddress.OA_RN_NKCountryCode = CountryCodes.Australia;

						var addressCountryData2 = localClient.MainAddress.KnownShipperDetails.AddNew();
						addressCountryData2.OV_OH_OrgHeader = localClient.PK;
						addressCountryData2.OV_EXApprovedOrMajorExporter = AviationSecuritySchemeMembership.Codes.KnownConsignor;
						addressCountryData2.OV_EXApprovalExpiryDate = ZDate.Today.AddDays(1);
					}

					for (var i = 0; i < inspectionTypes.Length; i++)
					{
						var shipment = header.Consol.Shipments.AddNew();
						shipment.JS_RL_NKOrigin = "AUSYD";
						shipment.JS_RL_NKDestination = "USLAX";
						shipment.CreateShipmentJobHeaderWithMutex();
						shipment.ConsignorDocumentaryAddress.OrganisationPK = consignor.PK;
						shipment.ShipmentJobHeader.JH_OA_LocalChargesAddr_ZAddress.OrgPK = localClient.PK;
						shipment.JS_InspectionTypeCode = inspectionTypes[i];
					}

					var expected = @"/US/CUS/KP/";
					var message = new FWB(new FWBMessageDetails(header), FWB.Version.No16).ToString();
					if (shouldIncludeVKC)
					{
						expected += knownConsignorCode;
						AssertContains(expected, message);
					}
					else
					{
						AssertNotContains(expected, message);
					}
					header.Consol.Shipments.OfType<ForwardingShipment>().ForEach(s => s.Job.Dispose());
				}
			}
		}

		public void TestOCI_VerifiedKnownConsignorWithWarning()
		{
			TestCase("USLAX", new string[] { "APP" }, "Y", hasValidKnownStatus: true, shouldIncludeVKC: true, "The route passes through the United States and the Inspection Type is APP, so the indicator appears in the OCI segment and is Y.");
			TestCase("USLAX", new string[] { "UKN", "PHS" }, "Y", hasValidKnownStatus: true, shouldIncludeVKC: true, "The route passes through the United States and the Inspection Type Codes are not APP, but Consignor has a valid Known status, so the indicator appears in the OCI segment and is Y");

			void TestCase(string destPortCode, string[] inspectionTypes, string knownConsignorCode, bool hasValidKnownStatus, bool shouldIncludeVKC, string reason)
			{
				var settings = FreightDataRegistry.Instance.ShipmentInspectionOrganisationToUse_Australia.Value;
				((SupplyChainSecurityOrganisationToUse)settings.FindByCode(SupplyChainSecurityOrganisationTypes.Consignor)).ValidationCode = SupplyChainSecurityOrganisationToUse.ValidationCodes.Warning;

				using (FreightDataRegistry.Instance.ShipmentInspectionOrganisationToUse_Australia.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, settings))
				{
					var header = CreateEmptyAWBHeaderWithConsol();
					header.EH_ShipperCountryCode = "AU";
					header.EH_ConsigneeCountryCode = "US";
					header.Consol.JK_RL_NKDischargePort = destPortCode;

					var consignor = Factory.NewWithValidTestData<OrgHeader>();
					consignor.OH_Code = "CON";

					var localClient = Factory.NewWithValidTestData<OrgHeader>();
					localClient.OH_Code = "LOC";

					if (hasValidKnownStatus)
					{
						consignor.MainAddress.OA_RN_NKCountryCode = CountryCodes.Australia;

						var addressCountryData1 = consignor.MainAddress.KnownShipperDetails.AddNew();
						addressCountryData1.OV_OH_OrgHeader = consignor.PK;
						addressCountryData1.OV_EXApprovedOrMajorExporter = AviationSecuritySchemeMembership.Codes.KnownConsignor;
						addressCountryData1.OV_EXApprovalExpiryDate = ZDate.Today.AddDays(1);

						localClient.MainAddress.OA_RN_NKCountryCode = CountryCodes.Australia;

						var addressCountryData2 = localClient.MainAddress.KnownShipperDetails.AddNew();
						addressCountryData2.OV_OH_OrgHeader = localClient.PK;
						addressCountryData2.OV_EXApprovedOrMajorExporter = AviationSecuritySchemeMembership.Codes.KnownConsignor;
						addressCountryData2.OV_EXApprovalExpiryDate = ZDate.Today.AddDays(1);
					}

					for (var i = 0; i < inspectionTypes.Length; i++)
					{
						var shipment = header.Consol.Shipments.AddNew();
						shipment.JS_RL_NKOrigin = "AUSYD";
						shipment.JS_RL_NKDestination = "USLAX";
						shipment.CreateShipmentJobHeaderWithMutex();
						shipment.ConsignorDocumentaryAddress.OrganisationPK = consignor.PK;
						shipment.ShipmentJobHeader.JH_OA_LocalChargesAddr_ZAddress.OrgPK = localClient.PK;
						shipment.JS_InspectionTypeCode = inspectionTypes[i];
					}

					var expected = @"/US/CUS/KP/";
					var message = new FWB(new FWBMessageDetails(header), FWB.Version.No16).ToString();
					if (shouldIncludeVKC)
					{
						expected += knownConsignorCode;
						AssertContains(expected, message);
					}
					else
					{
						AssertNotContains(expected, message);
					}
					header.Consol.Shipments.OfType<ForwardingShipment>().ForEach(s => s.Job.Dispose());
				}
			}
		}

		public void TestOCI_VerifiedKnownConsignorAndLocalClientWithWarning()
		{
			TestCase("USLAX", new string[] { "EDS", "PHS" }, "Y", consignorWithWarning: true, consignorWithYes: false, hasValidKnownStatus: true, shouldIncludeVKC: true, "The route passes through the United States and the Inspection Type Codes are EDS and PHS and Consignor and Local Client are set to Yes/Warning, but Consignor has valid Known status, so the indicator appears in the OCI segment and is Y");
			TestCase("USLAX", new string[] { "EDS", "PHS" }, "N", consignorWithWarning: true, consignorWithYes: false, hasValidKnownStatus: false, shouldIncludeVKC: true, "The route passes through the United States and the Inspection Type Codes are EDS and PHS and Consignor and Local Client are set to Yes/Warning, but Consignor doesn't have valid Known status so the indicator appears in the OCI segment and is N.");

			TestCase("USLAX", new string[] { "EDS", "PHS" }, "Y", consignorWithWarning: false, consignorWithYes: true, hasValidKnownStatus: true, shouldIncludeVKC: true, "The route passes through the United States and the Inspection Type Codes are EDS and PHS and Consignor and Local Client are set to Yes/Warning, but Consignor has valid Known status, so the indicator appears in the OCI segment and is Y.");
			TestCase("USLAX", new string[] { "EDS", "PHS" }, "N", consignorWithWarning: false, consignorWithYes: true, hasValidKnownStatus: false, shouldIncludeVKC: true, "The route passes through the United States and the Inspection Type Codes are EDS and PHS and Consignor and Local Client are set to Yes/Warning, but Consignor doesn't have valid Known status so the indicator appears in the OCI segment and is N.");

			TestCase("USLAX", new string[] { "APP", "APP" }, "Y", consignorWithWarning: false, consignorWithYes: false, hasValidKnownStatus: true, shouldIncludeVKC: true, "The route passes through the United States and the Inspection Type is APP and Consignor is set as No and Local Client is set as WARN, so the indicator appears in the OCI segment and is Y.");
			TestCase("USLAX", new string[] { "EDS", "PHS" }, "Y", consignorWithWarning: false, consignorWithYes: false, hasValidKnownStatus: true, shouldIncludeVKC: true, "The route passes through the United States and the Inspection Type Codes are EDS and PHS and Consignor is set as No and Local Client is set as WARN, but Consignor has valid Known status, so the indicator appears in the OCI segment and is Y.");

			void TestCase(string destPortCode, string[] inspectionTypes, string knownConsignorCode, bool consignorWithWarning,bool consignorWithYes, bool hasValidKnownStatus, bool shouldIncludeVKC, string reason)
			{
				var settings = FreightDataRegistry.Instance.ShipmentInspectionOrganisationToUse_UK.Value;
				((SupplyChainSecurityOrganisationToUse)settings.FindByCode(SupplyChainSecurityOrganisationTypes.LocalClient)).ValidationCode = SupplyChainSecurityOrganisationToUse.ValidationCodes.Warning;
				if (consignorWithWarning)
				{
					((SupplyChainSecurityOrganisationToUse)settings.FindByCode(SupplyChainSecurityOrganisationTypes.Consignor)).ValidationCode = SupplyChainSecurityOrganisationToUse.ValidationCodes.Warning;
				}
				else if (consignorWithYes)
				{
					((SupplyChainSecurityOrganisationToUse)settings.FindByCode(SupplyChainSecurityOrganisationTypes.Consignor)).ValidationCode = SupplyChainSecurityOrganisationToUse.ValidationCodes.Yes;
				}

				using (FreightDataRegistry.Instance.ShipmentInspectionOrganisationToUse_Australia.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, settings))
				{
					var header = CreateEmptyAWBHeaderWithConsol();
					header.EH_ShipperCountryCode = "AU";
					header.EH_ConsigneeCountryCode = "US";
					header.Consol.JK_RL_NKDischargePort = destPortCode;

					var consignor = Factory.NewWithValidTestData<OrgHeader>();
					consignor.OH_Code = "CON";

					var localClient = Factory.NewWithValidTestData<OrgHeader>();
					localClient.OH_Code = "LOC";

					if (hasValidKnownStatus)
					{
						consignor.MainAddress.OA_RN_NKCountryCode = CountryCodes.Australia;

						var addressCountryData1 = consignor.MainAddress.KnownShipperDetails.AddNew();
						addressCountryData1.OV_OH_OrgHeader = consignor.PK;
						addressCountryData1.OV_EXApprovedOrMajorExporter = AviationSecuritySchemeMembership.Codes.KnownConsignor;
						addressCountryData1.OV_EXApprovalExpiryDate = ZDate.Today.AddDays(1);
					}

					localClient.MainAddress.OA_RN_NKCountryCode = CountryCodes.Australia;

					var addressCountryData2 = localClient.MainAddress.KnownShipperDetails.AddNew();
					addressCountryData2.OV_OH_OrgHeader = localClient.PK;
					addressCountryData2.OV_EXApprovedOrMajorExporter = AviationSecuritySchemeMembership.Codes.KnownConsignor;
					addressCountryData2.OV_EXApprovalExpiryDate = ZDate.Today.AddDays(1);

					for (var i = 0; i < inspectionTypes.Length; i++)
					{
						var shipment = header.Consol.Shipments.AddNew();
						shipment.JS_RL_NKOrigin = "AUSYD";
						shipment.JS_RL_NKDestination = "USLAX";
						shipment.CreateShipmentJobHeaderWithMutex();
						shipment.ConsignorDocumentaryAddress.OrganisationPK = consignor.PK;
						shipment.ShipmentJobHeader.JH_OA_LocalChargesAddr_ZAddress.OrgPK = localClient.PK;
						shipment.JS_InspectionTypeCode = inspectionTypes[i];
					}

					var expected = @"/US/CUS/KP/";
					var message = new FWB(new FWBMessageDetails(header), FWB.Version.No16).ToString();
					if (shouldIncludeVKC)
					{
						expected += knownConsignorCode;
						AssertContains(expected, message);
					}
					else
					{
						AssertNotContains(expected, message);
					}
					header.Consol.Shipments.OfType<ForwardingShipment>().ForEach(s => s.Job.Dispose());
				}
			}
		}

		public void TestOCIIncludes_RTNasConsigneeTraderNoType_Honduras()
		{
			var header = CreateEmptyAWBHeaderWithConsol();
			header.EH_ConsigneeCountryCode = CountryCodes.Honduras;
			header.EH_ConsigneeTraderNoType = "RTN";
			header.EH_ConsigneeTraderNo = "5678999";
			header.DestinationCountryForTest = Factory.LoadFromNaturalKey<RefCountry>(RefCountrySchema.RN_Code, CountryCodes.Honduras);

			const string expected = @"OCI/HN/CNE/T/RTN5678999";
			AssertSegment(header, FWB.Version.No16, "OCI", expected);
		}

		public void TestShipperTrader_EnableChinaCustomsTaxNumberTable()
		{
			var header = CreateEmptyAWBHeaderWithConsol();
			header.EH_ShipperCountryCode = CountryCodes.Kenya;
			header.EH_AirportOfDestinationCode = CountryCodes.Kenya;
			header.DestinationCountryForTest = Factory.LoadFromNaturalKey<RefCountry>(RefCountrySchema.RN_Code, CountryCodes.Kenya);
			header.EH_ShipperTraderNoType = "TRADE REGISTER NUMBER";
			header.EH_ShipperTraderNo = "111";

			const string expected = @"OCI/KE/SHP/T/TRADEREGISTERNUMBER111";
			AssertSegment(header, FWB.Version.No16, "OCI", expected);
		}

		public void TestConsigneeTrader_EnableChinaCustomsTaxNumberTable()
		{
			var header = CreateEmptyAWBHeaderWithConsol();
			header.EH_ConsigneeCountryCode = CountryCodes.Egypt;
			header.EH_AirportOfDestinationCode = CountryCodes.Egypt;
			header.DestinationCountryForTest = Factory.LoadFromNaturalKey<RefCountry>(RefCountrySchema.RN_Code, CountryCodes.Egypt);
			header.EH_ConsigneeTraderNoType = "TRADE REGISTER NUMBER";
			header.EH_ConsigneeTraderNo = "111";

			const string expected = @"OCI/EG/CNE/T/TRADEREGISTERNUMBER111";
			AssertSegment(header, FWB.Version.No16, "OCI", expected);
		}

		public void TestAlsoNotifyTrader_EnableChinaCustomsTaxNumberTable()
		{
			var header = CreateEmptyAWBHeaderWithConsol();
			header.EH_AlsoNotifyCountryCode = CountryCodes.Kenya;
			header.EH_AirportOfDestinationCode = CountryCodes.Kenya;
			header.DestinationCountryForTest = Factory.LoadFromNaturalKey<RefCountry>(RefCountrySchema.RN_Code, CountryCodes.Kenya);
			header.EH_AlsoNotifyTraderNoType = "TRADE REGISTER NUMBER";
			header.EH_AlsoNotifyTraderNo = "111";

			const string expected = @"OCI/KE/NFY/T/TRADEREGISTERNUMBER111";
			AssertSegment(header, FWB.Version.No16, "OCI", expected);
		}

		public void TestAlsoNotifyIsInChina_OCIIdEmpty_AlsoNotifyContactNameEmpty_NoOCI()
		{
			var airline = Factory.New<RefAirline>();
			airline.RM_TwoCharacterCode = "XX";
			airline.RM_EagleAddedAirlinePrefixOrAccountingCode = "XX";

			var header = CreateEmptyAWBHeaderWithConsol();
			header.IsImportToChinaForTest = true;
			header.EH_AlsoNotifyContactName = ZString.Empty;
			header.EH_By1st = "XX";

			AssertSegment(header, FWB.Version.No16, "OCI", string.Empty);
		}

		public void TestAlsoNotifyIsNotInChina_OCIIdNotEmpty_AlsoNotifyContactNameNotEmpty_NoOCIForCN()
		{
			var airline = Factory.New<RefAirline>();
			airline.RM_TwoCharacterCode = "XX";
			airline.RM_EagleAddedAirlinePrefixOrAccountingCode = "XX";
			airline.RM_ContactNameOCIIdentifier = "AB";

			var header = CreateEmptyAWBHeaderWithConsol();
			header.EH_AlsoNotifyContactName = "IGOR";
			header.EH_By1st = "XX";

			AssertSegment(header, FWB.Version.No16, "OCI", "OCI//NFY/CP/IGOR");
		}

		public void TestAlsoNotifyIsInChina_OCIIdNotEmpty_AlsoNotifyContactNameEmpty_NoOCI()
		{
			var airline = Factory.New<RefAirline>();
			airline.RM_TwoCharacterCode = "XX";
			airline.RM_EagleAddedAirlinePrefixOrAccountingCode = "XX";
			airline.RM_ContactNameOCIIdentifier = "AB";

			var header = CreateEmptyAWBHeaderWithConsol();
			header.IsImportToChinaForTest = true;
			header.EH_AlsoNotifyContactName = ZString.Empty;
			header.EH_By1st = "XX";

			AssertSegment(header, FWB.Version.No16, "OCI", string.Empty);
		}

		public void TestAlsoNotifyIsInChina_OCIIdEmpty_AlsoNotifyContactDetailEmpty_NoOCI()
		{
			var airline = Factory.New<RefAirline>();
			airline.RM_TwoCharacterCode = "XX";
			airline.RM_EagleAddedAirlinePrefixOrAccountingCode = "XX";

			var header = CreateEmptyAWBHeaderWithConsol();
			header.IsImportToChinaForTest = true;
			header.EH_AlsoNotifyContactDetail = ZString.Empty;
			header.EH_By1st = "XX";

			AssertSegment(header, FWB.Version.No16, "OCI", string.Empty);
		}

		public void TestAlsoNotifyIsNotInChina_OCIIdNotEmpty_AlsoNotifyContactDetailNotEmpty_NoOCIForCN()
		{
			var airline = Factory.New<RefAirline>();
			airline.RM_TwoCharacterCode = "XX";
			airline.RM_EagleAddedAirlinePrefixOrAccountingCode = "XX";
			airline.RM_ContactPhoneOCIIdentifier = "AB";

			var header = CreateEmptyAWBHeaderWithConsol();
			header.EH_AlsoNotifyContactDetail = "111";
			header.EH_By1st = "XX";

			AssertSegment(header, FWB.Version.No16, "OCI", "OCI//NFY/CT/111");
		}

		public void TestAlsoNotifyIsInChina_OCIIdNotEmpty_AlsoNotifyContactDetailEmpty_NoOCI()
		{
			var airline = Factory.New<RefAirline>();
			airline.RM_TwoCharacterCode = "XX";
			airline.RM_EagleAddedAirlinePrefixOrAccountingCode = "XX";
			airline.RM_ContactPhoneOCIIdentifier = "AB";

			var header = CreateEmptyAWBHeaderWithConsol();
			header.IsImportToChinaForTest = true;
			header.EH_AlsoNotifyContactDetail = ZString.Empty;
			header.EH_By1st = "XX";

			AssertSegment(header, FWB.Version.No16, "OCI", string.Empty);
		}

		public void TestAlsoNotifyIsInChina_OCIIdNotEmpty_AlsoNotifyContactDetailNotEmpty_AlsoNotifyContactCodeFax_NoOCI()
		{
			var airline = Factory.New<RefAirline>();
			airline.RM_TwoCharacterCode = "XX";
			airline.RM_EagleAddedAirlinePrefixOrAccountingCode = "XX";
			airline.RM_ContactPhoneOCIIdentifier = "AB";

			var header = CreateEmptyAWBHeaderWithConsol();
			header.IsImportToChinaForTest = true;
			header.EH_AlsoNotifyContactDetail = "111";
			header.EH_AlsoNotifyContactCode = Core.Constants.AWB.ContactCodes.FAX;
			header.EH_By1st = "XX";

			AssertSegment(header, FWB.Version.No16, "OCI", string.Empty);
		}

		public void TestAlsoNotifyIsInChina_AlsoNotifyTraderNoUsesTIdentifier()
		{
			var header = CreateEmptyAWBHeaderWithConsol();
			header.IsImportToChinaForTest = true;
			header.EH_AlsoNotifyCountryCode = CountryCodes.China;
			header.EH_AlsoNotifyTraderNo = "111";

			const string expected = @"OCI/CN/NFY/T/111";
			AssertSegment(header, FWB.Version.No16, "OCI", expected);
		}

		public void TestDestinationIsInChina_ShipperTraderNoUsesTIdentifier()
		{
			var header = CreateEmptyAWBHeaderWithConsol();
			header.IsImportToChinaForTest = true;
			header.EH_ShipperTraderNo = "111";
			header.EH_ShipperCountryCode = CountryCodes.Australia;

			const string expected = @"OCI/AU/SHP/T/111";
			AssertSegment(header, FWB.Version.No16, "OCI", expected);
		}

		public void TestShipperIsInChina_ShipperTraderNoUsesTIdentifier()
		{
			var header = CreateEmptyAWBHeaderWithConsol();
			header.EH_ShipperTraderNo = "111";
			header.EH_ShipperCountryCode = CountryCodes.China;

			const string expected = @"OCI/CN/SHP/T/111";
			AssertSegment(header, FWB.Version.No16, "OCI", expected);
		}

		public void TestShipperIsNotInChina_ShipperTraderNoDoesntUseTIdentifier()
		{
			var header = CreateEmptyAWBHeaderWithConsol();
			header.EH_ShipperTraderNo = "111";
			header.EH_ShipperCountryCode = CountryCodes.Australia;

			const string expected = @"OCI/AU/SHP/T/111";
			AssertSegment(header, FWB.Version.No16, "OCI", expected);
		}

		public void TestOCISegment_CorrectCNEAndNFYOrder()
		{
			var airline = Factory.New<RefAirline>();
			airline.RM_TwoCharacterCode = "XX";
			airline.RM_EagleAddedAirlinePrefixOrAccountingCode = "XX";
			airline.RM_ContactNameOCIIdentifier = "AB";
			airline.RM_ContactPhoneOCIIdentifier = "PH";

			var header = CreateEmptyAWBHeaderWithConsol();
			header.EH_ConsigneeContactName = "IGOR";
			header.EH_By1st = "XX";
			header.EH_ShipperTraderNo = "ze number lelel";
			header.EH_ConsigneeTraderNo = "ze cons number pepehands";
			header.EH_ConsigneeContactDetail = "1300655506";
			header.EH_AlsoNotifyTraderNo = "Y33T XD W00T UwU*";
			header.EH_AlsoNotifyContactName = "Clubber lang";
			header.EH_AlsoNotifyContactCode = "CL";
			header.EH_AlsoNotifyContactDetail = "H3110";

			header.IsImportToChinaForTest = true;
			string expected = @"OCI//SHP/T/ZE NUMBER LELEL
//CNE/T/ZE CONS NUMBER PEPEHANDS
/CN/CNE/AB/IGOR
/CN/CNE/PH/1300655506
//NFY/T/Y33T XD W00T UWU 
/CN/NFY/AB/CLUBBER LANG
";
			AssertSegment(header, FWB.Version.No16, "OCI", expected);

			header.IsImportToChinaForTest = false;
			header.IsTransitingThroughChinaForTest = true;
			expected = @"OCI//SHP/T/ZE NUMBER LELEL
//CNE/T/ZE CONS NUMBER PEPEHANDS
/CN/CNE/AB/IGOR
/CN/CNE/PH/1300655506
//NFY/T/Y33T XD W00T UWU 
/CN/NFY/AB/CLUBBER LANG
";
			AssertSegment(header, FWB.Version.No16, "OCI", expected);

			header.IsTransitingThroughChinaForTest = false;
			expected = @"OCI//SHP/T/ZE NUMBER LELEL
//CNE/T/ZE CONS NUMBER PEPEHANDS
//CNE/CP/IGOR
//CNE/CT/1300655506
//NFY/T/Y33T XD W00T UWU 
//NFY/CP/CLUBBER LANG
";
			AssertSegment(header, FWB.Version.No16, "OCI", expected);
		}

		public void TestOCISegment_MaximumLength_MovementReferenceNumber()
		{
			var awbHeader = CreateEmptyAWBHeaderWithConsol();
			awbHeader.EH_AgentApprovalNumber = "TEST123";
			awbHeader.SetTSASecurityStatement("Lorem ipsum");

			awbHeader.MovementReferenceNumbersExposed = new List<MovementReferenceNumber>();
			for (var i = 1; i < 30; i++)
			{
				var movementNumber = new MovementReferenceNumber
				{
					CountryOfIssue = "GB",
					MovementCode = MovementReferenceCode.Codes.CustomsImport
				};

				movementNumber.Numbers.Add(string.Format("14PL45612354752{0:D2}", i));

				movementNumber.RelatedNumbers.AddRange(new List<EntryNumber>
				{
					new EntryNumber
					{
						Type = RelatedMovementReferenceNumberType.Codes.HouseWaybillNumber,
						Number = string.Format("AAA{0:D5}", i)
					},
					new EntryNumber
					{
						Type = RelatedMovementReferenceNumberType.Codes.HouseWaybillNumber,
						Number = string.Format("BBB{0:D5}", i)
					},
					new EntryNumber
					{
						Type = RelatedMovementReferenceNumberType.Codes.HouseWaybillNumber,
						Number = string.Format("CCC{0:D5}", i)
					}
				});

				awbHeader.MovementReferenceNumbersExposed.Add(movementNumber);
			}

			var expected = @"OCI/GB/IMP/M/14PL4561235475201
//HWB/I/AAA00001
//HWB/I/BBB00001
//HWB/I/CCC00001
/GB/IMP/M/14PL4561235475202
//HWB/I/AAA00002
//HWB/I/BBB00002
//HWB/I/CCC00002
/GB/IMP/M/14PL4561235475203
//HWB/I/AAA00003
//HWB/I/BBB00003
//HWB/I/CCC00003
/GB/IMP/M/14PL4561235475204
//HWB/I/AAA00004
//HWB/I/BBB00004
//HWB/I/CCC00004
/GB/IMP/M/14PL4561235475205
//HWB/I/AAA00005
//HWB/I/BBB00005
//HWB/I/CCC00005
/GB/IMP/M/14PL4561235475206
//HWB/I/AAA00006
//HWB/I/BBB00006
//HWB/I/CCC00006
/GB/IMP/M/14PL4561235475207
//HWB/I/AAA00007
//HWB/I/BBB00007
//HWB/I/CCC00007
/GB/IMP/M/14PL4561235475208
//HWB/I/AAA00008
//HWB/I/BBB00008
//HWB/I/CCC00008
/GB/IMP/M/14PL4561235475209
//HWB/I/AAA00009
//HWB/I/BBB00009
//HWB/I/CCC00009
/GB/IMP/M/14PL4561235475210
//HWB/I/AAA00010
//HWB/I/BBB00010
//HWB/I/CCC00010
/GB/IMP/M/14PL4561235475211
//HWB/I/AAA00011
//HWB/I/BBB00011
//HWB/I/CCC00011
/GB/IMP/M/14PL4561235475212
//HWB/I/AAA00012
//HWB/I/BBB00012
//HWB/I/CCC00012
/GB/IMP/M/14PL4561235475213
//HWB/I/AAA00013
//HWB/I/BBB00013
//HWB/I/CCC00013
/GB/IMP/M/14PL4561235475214
//HWB/I/AAA00014
//HWB/I/BBB00014
//HWB/I/CCC00014
/GB/IMP/M/14PL4561235475215
//HWB/I/AAA00015
//HWB/I/BBB00015
//HWB/I/CCC00015
/GB/IMP/M/14PL4561235475216
//HWB/I/AAA00016
//HWB/I/BBB00016
//HWB/I/CCC00016
/GB/IMP/M/14PL4561235475217
//HWB/I/AAA00017
//HWB/I/BBB00017
//HWB/I/CCC00017
/GB/IMP/M/14PL4561235475218
//HWB/I/AAA00018
//HWB/I/BBB00018
//HWB/I/CCC00018
/GB/IMP/M/14PL4561235475219
//HWB/I/AAA00019
//HWB/I/BBB00019
//HWB/I/CCC00019
/GB/IMP/M/14PL456123";
			AssertSegment(awbHeader, FWB.Version.No16, "OCI", expected, maxLength: FBase.OCISectionMaxLength);
		}

		public void TestConsigneeIsInArgentina_ConsigneeTraderNoHasSpecialCharsRemoved()
		{
			var header = CreateEmptyAWBHeaderWithConsol();
			header.EH_ConsigneeCountryCode = CountryCodes.Argentina;
			header.EH_AirportOfDestinationCode = CountryCodes.Argentina;
			header.DestinationCountryForTest = Factory.LoadFromNaturalKey<RefCountry>(RefCountrySchema.RN_Code, CountryCodes.Argentina);
			header.EH_ConsigneeTraderNo = "_11@1-1,1%1-";

			const string expected = @"OCI/AR/CNE/T/111111";
			AssertSegment(header, FWB.Version.No16, "OCI", expected);
		}

		public void TestFreightForwarderOrCarringCodeInCanada()
		{
			var header = CreateEmptyAWBHeaderWithConsol();
			header.IsImportToCanadaForTest = true;
			header.FreightForwarderOrCarrierCodeForTest = "8010";

			var expected = string.Empty;
			AssertSegment(header, FWB.Version.No16, "OCI", expected);
		}

		public void TestConsigneeIsInEgypt_ShipperIsInAustralia_DocNotesIsRegistrationID()
		{
			SetupEgyptRefDocOrgCusCodeWithNotes(OrgCusCode.CodeTypes.CorporationCode, "ACN", "ACN", "Registration ID");
			Factory.Save();

			var header = CreateEmptyAWBHeaderWithConsol();
			header.EH_ConsigneeCountryCode = CountryCodes.Egypt;
			header.EH_ConsigneeTraderNoType = OrgCusCode.CodeTypes.VATCode;

			header.EH_ShipperCountryCode = CountryCodes.Australia;
			header.OriginCountryForTest = Factory.LoadFromNaturalKey<RefCountry>(RefCountrySchema.RN_Code, CountryCodes.Australia);
			header.EH_ShipperTraderNo = "1111";
			header.EH_ShipperTraderNoType = "ACN";
			header.DestinationShipperCommentForTest = "Registration ID";

			const string expected = @"OCI/AU/SHP/T/AU-02-1111";
			AssertSegment(header, FWB.Version.No16, "OCI", expected);
		}

		public void TestConsigneeIsInEgypt_ShipperIsInAustralia_DocNotesIsTaxID()
		{
			SetupEgyptRefDocOrgCusCodeWithNotes(OrgCusCode.AustraliaCodeTypes.AustralianBusinessNumber, "ABN", "ABN", "Tax ID");
			Factory.Save();

			var header = CreateEmptyAWBHeaderWithConsol();
			header.EH_ConsigneeCountryCode = CountryCodes.Egypt;
			header.EH_ConsigneeTraderNoType = OrgCusCode.CodeTypes.VATCode;

			header.EH_ShipperCountryCode = CountryCodes.Australia;
			header.OriginCountryForTest = Factory.LoadFromNaturalKey<RefCountry>(RefCountrySchema.RN_Code, CountryCodes.Australia);
			header.EH_ShipperTraderNo = "23112936991";
			header.EH_ShipperTraderNoType = "ABN";
			header.DestinationShipperCommentForTest = "Tax ID";

			const string expected = @"OCI/AU/SHP/T/AU-01-23112936991";
			AssertSegment(header, FWB.Version.No16, "OCI", expected);
		}

		public void TestUSTerritories_PRF_ExportStatementConfigSet()
		{
			TestUSExport_PRF_ExportStatementConfigSet_FWB_OCI_Contains_PRFdetails(CountryCodes.UnitedStates, "USHOU", "AUSYD");
			TestUSExport_PRF_ExportStatementConfigSet_FWB_OCI_Contains_PRFdetails(CountryCodes.Guam, "GUGUM", "AUSYD");
			TestUSExport_PRF_ExportStatementConfigSet_FWB_OCI_Contains_PRFdetails(CountryCodes.PuertoRico, "PRABS", "AUSYD");
			TestUSExport_PRF_ExportStatementConfigSet_FWB_OCI_Contains_PRFdetails(CountryCodes.VirginIslands, "VIAGL", "AUSYD");
			TestUSExport_PRF_ExportStatementConfigSet_FWB_OCI_Contains_PRFdetails(CountryCodes.NorthernMarianaIslands, "MPTIQ", "AUSYD");
			TestUSExport_PRF_ExportStatementConfigSet_FWB_OCI_Contains_PRFdetails(CountryCodes.AmericanSamoa, "ASAPI", "AUSYD");

			void TestUSExport_PRF_ExportStatementConfigSet_FWB_OCI_Contains_PRFdetails(string countryCode, string origin, string destination)
			{
				using (GlbCompany.CurrentCompany.TemporarilySetCountry(countryCode))
				{
					CountryExportStatementSettingCollection
						defaultValue = new CountryExportStatementSettingCollection();
					CountryExportStatementSetting exportStatementSetting = defaultValue.AddNew();
					exportStatementSetting.CountryCode = countryCode;
					exportStatementSetting.Statements.Add(new ExportStatementSetting(exportStatementSetting, "PRF",
						"AES",
						"AES Proof of Filing Citation", CusEntryNumberTypes.UnitedStates.ITN, "", "UDF", true, true,
						true,
						true, true, true));
					FreightDataRegistry.Instance.ExportStatementSettings.SetValue(Guid.Empty, Guid.Empty, Guid.Empty,
						defaultValue);

					var header = CreateEmptyAWBHeaderWithConsol();
					header.Consol.JK_TransportMode = Core.Constants.TransportModes.Air;
					var shipment1 = header.Consol.Shipments.AddNew();
					shipment1.JS_RL_NKOrigin = origin;
					shipment1.JS_RL_NKDestination = destination;
					shipment1.DocsAndCartage.JP_ExportStatement = "PRF";

					var cusEntryNumber1 = shipment1.CusEntryNumbers.AddNew();
					cusEntryNumber1.CE_EntryType = CusEntryNumberTypes.UnitedStates.ITN;
					cusEntryNumber1.CE_EntryNum = "X20100101987654";

					var agentTypes = new List<string>
					{
						"DRT",
						"AGT",
						"CLD",
						"CHT",
						"COU",
						"OTH",
						"CLA",
						"CLM"
					};

					foreach (var agentType in agentTypes)
					{
						header.Consol.JK_AgentType = agentType;
						header.Populate();

						var expected = $@"OCI/{countryCode}/EXP/M/AES X20100101987654";
						var message = new FWB(new FWBMessageDetails(header), FWB.Version.No16).ToString();
						AssertContains(expected, message);
					}
				}
			}
		}

		public void TestUSTerritories_PDU_ExportStatementConfigSet()
		{
			TestUSExport_PDU_ExportStatementConfigSet_FWB_OCI_Contains_PDUdetails(CountryCodes.UnitedStates, "USHOU", "AUSYD");
			TestUSExport_PDU_ExportStatementConfigSet_FWB_OCI_Contains_PDUdetails(CountryCodes.Guam, "GUGUM", "AUSYD");
			TestUSExport_PDU_ExportStatementConfigSet_FWB_OCI_Contains_PDUdetails(CountryCodes.PuertoRico, "PRABS", "AUSYD");
			TestUSExport_PDU_ExportStatementConfigSet_FWB_OCI_Contains_PDUdetails(CountryCodes.VirginIslands, "VIAGL", "AUSYD");
			TestUSExport_PDU_ExportStatementConfigSet_FWB_OCI_Contains_PDUdetails(CountryCodes.NorthernMarianaIslands, "MPTIQ", "AUSYD");
			TestUSExport_PDU_ExportStatementConfigSet_FWB_OCI_Contains_PDUdetails(CountryCodes.AmericanSamoa, "ASAPI", "AUSYD");

			void TestUSExport_PDU_ExportStatementConfigSet_FWB_OCI_Contains_PDUdetails(string countryCode, string origin, string destination)
			{
				using (GlbCompany.CurrentCompany.TemporarilySetCountry(countryCode))
				{
					CountryExportStatementSettingCollection
						defaultValue = new CountryExportStatementSettingCollection();
					CountryExportStatementSetting exportStatementSetting = defaultValue.AddNew();
					exportStatementSetting.CountryCode = countryCode;
					exportStatementSetting.Statements.Add(new ExportStatementSetting(exportStatementSetting, "PDU",
						"AESPOST", "Postdeparture Citation-USPPI", "SHP", "DOE", "UDF", true, true, true, true, true,
						true));
					FreightDataRegistry.Instance.ExportStatementSettings.SetValue(Guid.Empty, Guid.Empty, Guid.Empty,
						defaultValue);

					var header = CreateEmptyAWBHeaderWithConsol();
					header.Consol.JK_TransportMode = Core.Constants.TransportModes.Air;

					var shipment1 = header.Consol.Shipments.AddNew();
					shipment1.JS_RL_NKOrigin = origin;
					shipment1.JS_RL_NKDestination = destination;
					shipment1.DocsAndCartage.JP_ExportStatement = "PDU";
					shipment1.JS_E_DEP = new ZDateTime(2010, 10, 01);

					var consignor = Factory.New<OrgHeader>();
					consignor.CustomsCodes.AddNew(OrgCusCode.USACodeTypes.EmployerIdentificationNumber, "12345678912");
					shipment1.ConsignorPK = consignor.PK;

					var agentTypes = new List<string>
					{
						"DRT",
						"AGT",
						"CLD",
						"CHT",
						"COU",
						"OTH",
						"CLA",
						"CLM"
					};
					foreach (var agentType in agentTypes)
					{
						header.Consol.JK_AgentType = agentType;
						header.Populate();

						var expected = $@"OCI/{countryCode}/EXP/M/PDF 12345678912 20101001";
						var message = new FWB(new FWBMessageDetails(header), FWB.Version.No16).ToString();
						AssertContains(expected, message);
					}
				}
			}
		}

		public void TestUSTerritories_DWN_EntryFilerIdSetInRegistry_ExportStatementConfigSet()
		{
			TestUSExport_DWN_ExportStatementConfigSet_FWB_OCI_Contains_DWNdetails(CountryCodes.UnitedStates, "USHOU", "AUSYD");
			TestUSExport_DWN_ExportStatementConfigSet_FWB_OCI_Contains_DWNdetails(CountryCodes.PuertoRico, "PRABS", "AUSYD");
			TestUSExport_DWN_ExportStatementConfigSet_FWB_OCI_Contains_DWNdetails(CountryCodes.NorthernMarianaIslands, "MPTIQ", "AUSYD");

			void TestUSExport_DWN_ExportStatementConfigSet_FWB_OCI_Contains_DWNdetails(string countryCode, string origin, string destination)
			{
				using (GlbCompany.CurrentCompany.TemporarilySetCountry(countryCode))
				{
					var filer = new ExportEntryFilerID();
					filer.EntryFilerID = "111111111";
					filer.EntryFilerIDType = "D";
					ObjectFactory.Get<Enterprise.Integration.Customs.US.IUSCustomsDataRegistry>().ExportEntryFilerID.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, filer);

					var defaultValue = new CountryExportStatementSettingCollection();
					var exportStatementSetting = defaultValue.AddNew();
					exportStatementSetting.CountryCode = countryCode;
					exportStatementSetting.Statements.Add(new ExportStatementSetting(exportStatementSetting, "DWN",
						"AESDOWN", "AES Downtime Citation", "FIL", "DOE", "UDF", true, true, true, true, true, true));
					FreightDataRegistry.Instance.ExportStatementSettings.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, defaultValue);

					var header = CreateEmptyAWBHeaderWithConsol();
					header.Consol.JK_TransportMode = Core.Constants.TransportModes.Air;
					var shipment1 = header.Consol.Shipments.AddNew();
					shipment1.JS_RL_NKOrigin = origin;
					shipment1.JS_RL_NKDestination = destination;
					shipment1.JS_E_DEP = new ZDateTime(2010, 10, 01);
					shipment1.DocsAndCartage.JP_ExportStatement = "DWN";

					var agentTypes = new List<string>
					{
						"DRT",
						"AGT",
						"CLD",
						"CHT",
						"COU",
						"OTH",
						"CLA",
						"CLM"
					};

					foreach (var agentType in agentTypes)
					{
						header.Consol.JK_AgentType = agentType;
						header.Populate();

						var expected = $@"OCI/{countryCode}/EXP/M/AED 111111111 20101001";
						var message = new FWB(new FWBMessageDetails(header), FWB.Version.No16).ToString();
						AssertContains(expected, message);
					}
				}
			}
		}

		public void TestUSTerritories_DWN_EntryFilerIdNotSetInRegistry_ExportStatementConfigSet()
		{
			TestUSExport_DWN_ExportStatementConfigSet_FWB_OCI_Contains_DWNdetails(CountryCodes.Guam, "GUGUM", "AUSYD");
			TestUSExport_DWN_ExportStatementConfigSet_FWB_OCI_Contains_DWNdetails(CountryCodes.NorthernMarianaIslands, "MPTIQ", "AUSYD");
			TestUSExport_DWN_ExportStatementConfigSet_FWB_OCI_Contains_DWNdetails(CountryCodes.AmericanSamoa, "ASAPI", "AUSYD");

			void TestUSExport_DWN_ExportStatementConfigSet_FWB_OCI_Contains_DWNdetails(string countryCode, string origin, string destination)
			{
				using (GlbCompany.CurrentCompany.TemporarilySetCountry(countryCode))
				{
					CountryExportStatementSettingCollection
						defaultValue = new CountryExportStatementSettingCollection();
					CountryExportStatementSetting exportStatementSetting = defaultValue.AddNew();
					exportStatementSetting.CountryCode = countryCode;
					exportStatementSetting.Statements.Add(new ExportStatementSetting(exportStatementSetting, "DWN",
						"AESDOWN", "AES Downtime Citation", "FIL", "DOE", "UDF", true, true, true, true, true, true));
					FreightDataRegistry.Instance.ExportStatementSettings.SetValue(Guid.Empty, Guid.Empty, Guid.Empty,
						defaultValue);

					var header = CreateEmptyAWBHeaderWithConsol();
					header.Consol.JK_TransportMode = Core.Constants.TransportModes.Air;
					var shipment1 = header.Consol.Shipments.AddNew();
					shipment1.JS_RL_NKOrigin = origin;
					shipment1.JS_RL_NKDestination = destination;
					shipment1.JS_E_DEP = new ZDateTime(2010, 10, 01);
					shipment1.DocsAndCartage.JP_ExportStatement = "DWN";

					var agentTypes = new List<string>
					{
						"DRT",
						"AGT",
						"CLD",
						"CHT",
						"COU",
						"OTH",
						"CLA",
						"CLM"
					};

					foreach (var agentType in agentTypes)
					{
						header.Consol.JK_AgentType = agentType;
						header.Populate();

						var expected = $@"OCI/{countryCode}/EXP/M/AED 20101001";
						var message = new FWB(new FWBMessageDetails(header), FWB.Version.No16).ToString();
						AssertContains(expected, message);
					}
				}
			}
		}

		public void TestUSTerritories_LOW_ExportStatementConfigSet()
		{
			TestUSExport_LOW_ExportStatementConfigSet_FWB_OCI_Contains_LOWdetails(CountryCodes.UnitedStates, "USHOU", "AUSYD");
			TestUSExport_LOW_ExportStatementConfigSet_FWB_OCI_Contains_LOWdetails(CountryCodes.Guam, "GUGUM", "AUSYD");
			TestUSExport_LOW_ExportStatementConfigSet_FWB_OCI_Contains_LOWdetails(CountryCodes.PuertoRico, "PRABS", "AUSYD");
			TestUSExport_LOW_ExportStatementConfigSet_FWB_OCI_Contains_LOWdetails(CountryCodes.VirginIslands, "VIAGL", "AUSYD");
			TestUSExport_LOW_ExportStatementConfigSet_FWB_OCI_Contains_LOWdetails(CountryCodes.NorthernMarianaIslands, "MPTIQ", "AUSYD");
			TestUSExport_LOW_ExportStatementConfigSet_FWB_OCI_Contains_LOWdetails(CountryCodes.AmericanSamoa, "ASAPI", "AUSYD");

			void TestUSExport_LOW_ExportStatementConfigSet_FWB_OCI_Contains_LOWdetails(string countryCode, string origin, string destination)
			{
				using (GlbCompany.CurrentCompany.TemporarilySetCountry(countryCode))
				{
					CountryExportStatementSettingCollection
						defaultValue = new CountryExportStatementSettingCollection();
					CountryExportStatementSetting exportStatementSetting = defaultValue.AddNew();
					exportStatementSetting.CountryCode = countryCode;
					exportStatementSetting.Statements.Add(new ExportStatementSetting(exportStatementSetting, "LOW",
						"NOEEI §30.37(a)", "NOEEI §30.37(a) - Low Value (<$2501)", "", "", "UDF", true, true, true,
						true, true, true));
					FreightDataRegistry.Instance.ExportStatementSettings.SetValue(Guid.Empty, Guid.Empty, Guid.Empty,
						defaultValue);

					var header = CreateEmptyAWBHeaderWithConsol();
					header.Consol.JK_TransportMode = Core.Constants.TransportModes.Air;
					var shipment1 = header.Consol.Shipments.AddNew();
					shipment1.JS_RL_NKOrigin = origin;
					shipment1.JS_RL_NKDestination = destination;
					shipment1.DocsAndCartage.JP_ExportStatement = "LOW";

					var agentTypes = new List<string>
					{
						"DRT",
						"AGT",
						"CLD",
						"CHT",
						"COU",
						"OTH",
						"CLA",
						"CLM"
					};
					foreach (var agentType in agentTypes)
					{
						header.Consol.JK_AgentType = agentType;
						header.Populate();

						var expected = $@"OCI/{countryCode}/EXP/M/AES NOEEI EXC";
						var message = new FWB(new FWBMessageDetails(header), FWB.Version.No16).ToString();
						AssertContains(expected, message);
					}
				}
			}
		}

		public void TestUSTerritories_ExportStatementConfigSet_MultipleShipmentAttached()
		{
			TestUSExport_ExportStatementConfigSet_MultipleShipmentAttached_FWB_OCI_Contains_AllExportDetails(CountryCodes.UnitedStates, "USHOU", "AUSYD");
			TestUSExport_ExportStatementConfigSet_MultipleShipmentAttached_FWB_OCI_Contains_AllExportDetails(CountryCodes.Guam, "GUGUM", "AUSYD");
			TestUSExport_ExportStatementConfigSet_MultipleShipmentAttached_FWB_OCI_Contains_AllExportDetails(CountryCodes.PuertoRico, "PRABS", "AUSYD");
			TestUSExport_ExportStatementConfigSet_MultipleShipmentAttached_FWB_OCI_Contains_AllExportDetails(CountryCodes.VirginIslands, "VIAGL", "AUSYD");
			TestUSExport_ExportStatementConfigSet_MultipleShipmentAttached_FWB_OCI_Contains_AllExportDetails(CountryCodes.NorthernMarianaIslands, "MPTIQ", "AUSYD");
			TestUSExport_ExportStatementConfigSet_MultipleShipmentAttached_FWB_OCI_Contains_AllExportDetails(CountryCodes.AmericanSamoa, "ASAPI", "AUSYD");

			void TestUSExport_ExportStatementConfigSet_MultipleShipmentAttached_FWB_OCI_Contains_AllExportDetails(string countryCode, string origin, string destination)
			{
				using (GlbCompany.CurrentCompany.TemporarilySetCountry(countryCode))
				{
					var filer = new ExportEntryFilerID();
					filer.EntryFilerID = "111111111";
					filer.EntryFilerIDType = "D";
					ObjectFactory.Get<Enterprise.Integration.Customs.US.IUSCustomsDataRegistry>().ExportEntryFilerID.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, filer);

					var defaultValue = new CountryExportStatementSettingCollection();
					var exportStatementSetting = defaultValue.AddNew();
					exportStatementSetting.CountryCode = countryCode;
					exportStatementSetting.Statements.Add(new ExportStatementSetting(exportStatementSetting, "PRF",
						"AES", "AES Proof of Filing Citation", CusEntryNumberTypes.UnitedStates.ITN, "", "UDF", true,
						true, true, true, true, true));
					exportStatementSetting.Statements.Add(new ExportStatementSetting(exportStatementSetting, "PDU",
						"AESPOST", "Postdeparture Citation-USPPI", "SHP", "DOE", "UDF", true, true, true, true, true,
						true));
					exportStatementSetting.Statements.Add(new ExportStatementSetting(exportStatementSetting, "DWN",
						"AESDOWN", "AES Downtime Citation", "FIL", "DOE", "UDF", true, true, true, true, true, true));
					exportStatementSetting.Statements.Add(new ExportStatementSetting(exportStatementSetting, "LOW",
						"NOEEI §30.37(a)", "NOEEI §30.37(a) - Low Value (<$2501)", "", "", "UDF", true, true, true,
						true, true, true));
					FreightDataRegistry.Instance.ExportStatementSettings.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, defaultValue);

					var header = CreateEmptyAWBHeaderWithConsol();
					header.Consol.JK_TransportMode = Core.Constants.TransportModes.Air;
					var shipment1 = header.Consol.Shipments.AddNew();
					shipment1.JS_RL_NKOrigin = origin;
					shipment1.JS_RL_NKDestination = destination;
					shipment1.DocsAndCartage.JP_ExportStatement = "PRF";

					var cusEntryNumber1 = shipment1.CusEntryNumbers.AddNew();
					cusEntryNumber1.CE_EntryType = CusEntryNumberTypes.UnitedStates.ITN;
					cusEntryNumber1.CE_EntryNum = "X20100101987654";

					var shipment2 = header.Consol.Shipments.AddNew();
					shipment2.JS_RL_NKOrigin = origin;
					shipment2.JS_RL_NKDestination = destination;
					shipment2.DocsAndCartage.JP_ExportStatement = "PDU";
					shipment2.JS_E_DEP = new ZDateTime(2010, 10, 01);

					var consignor = Factory.New<OrgHeader>();
					consignor.CustomsCodes.AddNew(OrgCusCode.USACodeTypes.EmployerIdentificationNumber, "12345678912");
					shipment2.ConsignorPK = consignor.PK;

					var shipment3 = header.Consol.Shipments.AddNew();
					shipment3.JS_RL_NKOrigin = origin;
					shipment3.JS_RL_NKDestination = destination;
					shipment3.DocsAndCartage.JP_ExportStatement = "DWN";
					shipment3.JS_E_DEP = new ZDateTime(2010, 10, 01);

					var shipment4 = header.Consol.Shipments.AddNew();
					shipment4.JS_RL_NKOrigin = origin;
					shipment4.JS_RL_NKDestination = destination;
					shipment4.DocsAndCartage.JP_ExportStatement = "LOW";
					shipment4.JS_E_DEP = new ZDateTime(2010, 10, 01);

					var shipment5 = header.Consol.Shipments.AddNew();
					shipment5.JS_RL_NKOrigin = origin;
					shipment5.JS_RL_NKDestination = destination;
					shipment5.DocsAndCartage.JP_ExportStatement = "PRF";

					var cusEntryNumber2 = shipment5.CusEntryNumbers.AddNew();
					cusEntryNumber2.CE_EntryType = CusEntryNumberTypes.UnitedStates.ITN;
					cusEntryNumber2.CE_EntryNum = "X20100101987654";

					header.Populate();

					var message = new FWB(new FWBMessageDetails(header), FWB.Version.No16).ToString();
					var expectedMessage = $@"
OCI/{countryCode}/EXP/M/AES X20100101987654
/{countryCode}/EXP/M/PDF 12345678912 20101001
/{countryCode}/EXP/M/AED 111111111 20101001
/{countryCode}/EXP/M/AES NOEEI EXC";

					var duplicateMessage = $@"
OCI/{countryCode}/EXP/M/AES X20100101987654
/{countryCode}/EXP/M/PDF 12345678912 20101001
/{countryCode}/EXP/M/AED 111111111 20101001
/{countryCode}/EXP/M/AES NOEEI EXC 
/{countryCode}/EXP/M/AES X20100101987654";

					AssertContains(expectedMessage.Trim(), message.Trim());
					AssertNotContains(duplicateMessage.Trim(), message.Trim());
				}
			}
		}

		void SetupEgyptRefDocOrgCusCodeWithNotes(string codeType, string shortLabel, string longLabel, string notes)
		{
			var refDocOrgCusCode = Factory.NewWithValidTestData<RefDocOrgCusCode>();
			refDocOrgCusCode.DOC_RN_NKRegulatingCountry = CountryCodes.Egypt;
			refDocOrgCusCode.DOC_RN_NKCodeCountry = CountryCodes.Australia;
			refDocOrgCusCode.DOC_CodeType = codeType;
			refDocOrgCusCode.DOC_DocumentType = "AWB";
			refDocOrgCusCode.DOC_Priority = 1;
			refDocOrgCusCode.DOC_ShortLabel = shortLabel;
			refDocOrgCusCode.DOC_LongLabel = longLabel;
			refDocOrgCusCode.DOC_Notes = notes;
		}

		public void TestConsigneeIsInEgypt_OCIModifiesConsigneeTraderNoTypeIsVAT()
		{
			var header = CreateEmptyAWBHeaderWithConsol();
			header.EH_ConsigneeCountryCode = CountryCodes.Egypt;
			header.DestinationCountryForTest = Factory.LoadFromNaturalKey<RefCountry>(RefCountrySchema.RN_Code, CountryCodes.Egypt);
			header.EH_ConsigneeTraderNoType = OrgCusCode.CodeTypes.VATCode;
			header.EH_ConsigneeTraderNo = "88995566";

			const string expected = @"OCI/EG/CNE/T/88995566";
			AssertSegment(header, FWB.Version.No16, "OCI", expected);
		}

		public void TestNotifyPartyIsInEgypt_OCIModifiesNotifyPartyTypeIsVAT()
		{
			var header = CreateEmptyAWBHeaderWithConsol();
			header.EH_AlsoNotifyCountryCode = CountryCodes.Egypt;
			header.DestinationCountryForTest = Factory.LoadFromNaturalKey<RefCountry>(RefCountrySchema.RN_Code, CountryCodes.Egypt);
			header.EH_AlsoNotifyTraderNoType = OrgCusCode.CodeTypes.VATCode;
			header.EH_AlsoNotifyTraderNo = "88995566";

			const string expected = @"OCI/EG/NFY/T/88995566";
			AssertSegment(header, FWB.Version.No16, "OCI", expected);
		}

		public void TestShipperTradeIsInEgypt_OCIModifiesShipperTradeTypeIsVAT()
		{
			var header = CreateEmptyAWBHeaderWithConsol();
			header.EH_ShipperCountryCode = CountryCodes.Egypt;
			header.DestinationCountryForTest = Factory.LoadFromNaturalKey<RefCountry>(RefCountrySchema.RN_Code, CountryCodes.Egypt);
			header.EH_ShipperTraderNoType = OrgCusCode.CodeTypes.VATCode;
			header.EH_ShipperTraderNo = "88995566";

			const string expected = @"OCI/EG/SHP/T/88995566";
			AssertSegment(header, FWB.Version.No16, "OCI", expected);
		}

		public void TestOCIInformationIdentifierShouldChangeWhenIsICS2SelfFilling()
		{
			var header = CreateEmptyAWBHeaderWithConsol();
			header.EH_ConsigneeCountryCode = CountryCodes.Netherlands;
			header.DestinationCountryForTest = Factory.LoadFromNaturalKey<RefCountry>(RefCountrySchema.RN_Code, CountryCodes.Netherlands);
			header.EH_ConsigneeTraderNoType = "PIN";
			header.EH_ConsigneeTraderNo = "111";

			header.Consol.JK_TransportMode = TransportModes.Air;
			header.Consol.JK_RL_NKLoadPort = "AUSYD";
			header.Consol.JK_RL_NKDischargePort = "NLABC";

			var transport = header.Consol.Transports.AddNew();
			transport.JW_RL_NKLoadPort = "AUSYD";
			transport.JW_RL_NKDiscPort = "NLABC";

			var expected1 = @"OCI/NL/CNE/T/PIN111";
			AssertSegment(header, FWB.Version.No16, "OCI", expected1);

			var receivingForwarder = Factory.NewWithValidTestData<OrgHeader>();
			header.Consol.JK_OA_ReceivingForwarderAddress = receivingForwarder.MainAddress.PK;
			var misc = receivingForwarder.MiscServ;
			misc.OM_FWAdvanceCargoReportingSelfFiler = true;

			var expected2 = @"OCI/NL/DCL/T/PIN111";
			header.Populate();
			Assert(header.HasInboundToICS2Zone);
			Assert(header.EH_IsConsigneeDeclarantForAdvanceCargoReporting);

			header.EH_ConsigneeCountryCode = CountryCodes.Netherlands;
			header.DestinationCountryForTest = Factory.LoadFromNaturalKey<RefCountry>(RefCountrySchema.RN_Code, CountryCodes.Netherlands);
			header.EH_ConsigneeTraderNoType = "PIN";
			header.EH_ConsigneeTraderNo = "111";
			AssertSegment(header, FWB.Version.No16, "OCI", expected2);
		}

		public void TestFWBDestinationCode_MultiRoutes()
		{
			var header = Factory.New<ConsolExportAWBHeaderTest>();
			var consol = Factory.New<ForwardingConsol>();
			header.EH_ParentID = consol.PK;
			consol.JK_TransportMode = Core.Constants.TransportModes.Air;
			consol.JK_MasterBillNum = "00696667465";
			consol.JK_OverrideWaybillDefaults = ZBool.True;

			header.Populate();

			var expectedContainingString = $@"006-96667465/T00";
			var message = new FWB(new FWBMessageDetails(header), FWB.Version.No16).ToString();
			AssertContains(expectedContainingString, message);

			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "USLAX";

			header.Populate();

			expectedContainingString = $@"006-96667465SYDLAX/T00";
			message = new FWB(new FWBMessageDetails(header), FWB.Version.No16).ToString();
			AssertContains(expectedContainingString, message);

			var flight1 = consol.Transports[0];
			flight1.JW_RL_NKLoadPort = "AUSYD";
			flight1.JW_RL_NKDiscPort = "NZAKL";
			flight1.JW_TransportMode = Core.Constants.TransportModes.Air;

			var flight2 = consol.Transports.AddNew();
			flight2.JW_RL_NKLoadPort = "NZAKL";
			flight2.JW_RL_NKDiscPort = "USLAX";
			flight2.JW_TransportMode = Core.Constants.TransportModes.Road;

			header.Populate();

			expectedContainingString = $@"006-96667465SYDAKL/T00";
			message = new FWB(new FWBMessageDetails(header), FWB.Version.No16).ToString();
			AssertContains(expectedContainingString, message);
		}

		public void TestDGVariantNotShown()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.UnitedStates))
			{
				var dataCreator = new AWBHeaderTestDataCreator(Factory);
				var message = new FWB(new FWBMessageDetails(dataCreator.ConsolAWBHeader), FWB.Version.No16).ToString();
				AssertMatchingPrefix(message, "/US/DNR/D/", "UN1234", "UN2345");
			}
		}

		#region Implementation

		public static void AssertMatchingPrefix(string message, string prefix, params string[] match)
		{
			var found = message.Replace("\r", "").Split('\n').Where(s => s.StartsWith(prefix)).Select(s => s.Substring(prefix.Length)).ToArray();
			Array.Sort(match);
			Array.Sort(found);
			AssertArrayEqualsByElements($"Match {prefix}", match, found);
		}

		void AssertSegment(ExportAWBHeader header, FWB.Version version, string segmentIdentifier, string expected, bool includeSecurityDeclaration = false, string message = "", int maxLength = -1)
		{
			var detailsProvider = new FWBMessageDetails(header);
			var fwbMessage = new FWB(detailsProvider, version, includeSecurityDeclaration).ToString();

			var locationOfOCI = fwbMessage.IndexOf(string.Concat(segmentIdentifier, "/"), StringComparison.InvariantCultureIgnoreCase);

			var actual = locationOfOCI > 0
				? fwbMessage.Substring(locationOfOCI, fwbMessage.Length - locationOfOCI - 1)
				: string.Empty;

			if (maxLength >= 0)
			{
				var actualTrimmed = actual.Trim('\r', '\n');
				AssertLessThanOrEqualTo($"Length of {segmentIdentifier} segment should not exceed {maxLength}.", actualTrimmed.Length, maxLength);
			}

			AssertMultilineASCIIEquals(string.Format("{0} segment. {1}", segmentIdentifier, message),
				expected, actual);
		}

		ConsolExportAWBHeaderTest CreateEmptyAWBHeaderWithConsol()
		{
			var header = Factory.New<ConsolExportAWBHeaderTest>();
			var consol = Factory.New<ForwardingConsol>();
			header.EH_ParentID = consol.PK;
			return header;
		}

		class ConsolExportAWBHeaderTest : ConsolExportAWBHeader
		{
			public ConsolExportAWBHeaderTest(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{
			}

			public List<MovementReferenceNumber> MovementReferenceNumbersExposed { get; set; }

			protected override IEnumerable<MovementReferenceNumber> GetMovementReferenceNumbers()
			{
				return MovementReferenceNumbersExposed;
			}

			public override ZString TSASecurityStatement
			{
				get { return tsaSecurityStatement; }
			}

			public override ZString EH_ConsigneeContactEmail { get; set; }

			public override ZString EH_ShipperContactEmail { get; set; }

			public void SetTSASecurityStatement(ZString statement)
			{
				tsaSecurityStatement = statement;
			}

			ZString tsaSecurityStatement;

			public override RefCountry DestinationCountry => DestinationCountryForTest;
			public RefCountry DestinationCountryForTest { get; set; }

			public override RefCountry OriginCountry => OriginCountryForTest;
			public RefCountry OriginCountryForTest { get; set; }

			public override bool IsImportToChina => IsImportToChinaForTest;
			public bool IsImportToChinaForTest { get; set; }

			public override bool IsImportToCanada => IsImportToCanadaForTest;
			public bool IsImportToCanadaForTest { get; set; }

			public override ZString FreightForwarderOrCarrierCode => FreightForwarderOrCarrierCodeForTest;
			public ZString FreightForwarderOrCarrierCodeForTest { get; set; }

			public override bool IsTransitingThroughChina => IsTransitingThroughChinaForTest;
			public bool IsTransitingThroughChinaForTest { get; set; }

			public override ZString DestinationShipperComment => DestinationShipperCommentForTest;
			public ZString DestinationShipperCommentForTest { get; set; }
		}

		protected override CargoIMP CargoIMPMessage
		{
			get { return new FWB(new FWBMessageDetails(ConsolAWBHeader), FWB.Version.No10); }
		}

		protected override string Expected
		{
			get { return string.Format(ExpectedFormatString, new object[] { CGN, FLT, RTG, SHP, CNE, AGT, SSR, NFY, ACC, CVD, RTD, OTH, PPD, COL, CER, ISU, OSI, REF, COR }); }
		}

		new ConsolExportAWBHeader ConsolAWBHeader
		{
			get { return (ConsolExportAWBHeader)base.ConsolAWBHeader; }
		}

		protected override Forwarding.AWB.Business.ExportAWBHeader CreateTemplateAWBHeader()
		{
			ConsolExportAWBHeader awbHeader = Factory.New<ConsolExportAWBHeader>();
			ForwardingConsol consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = Core.Constants.TransportModes.Air;

			CommonShipment shipment = consol.Shipments.AddNew();
			shipment.JS_InspectionTypeCode = ScreeningMethods.Codes.VisualCheck;

			awbHeader.EH_ParentID = consol.PK;

			Transport transport = consol.Transports[0];
			transport.JW_RL_NKDiscPort = "USLAX";
			transport.JW_IsCargoOnly = false;

			awbHeader.EH_ShippingLoadAndCount = 5;
			awbHeader.Consol.JK_OverrideWaybillDefaults = true;

			awbHeader.Consol.JK_MasterBillNum = "08112345678";
			awbHeader.Populate();

			PopulateHeaderInfo(awbHeader);

			CGN = "081-12345678BNESIN/T13L14.3\r\n";
			FLT = "FLT/QF123/02/SQ856/12\r\n";
			RTG = "RTG/LAXQF/CPTBA/SINSQ\r\n";
			SHP = "SHP/BRECPT\r\n/MR SHIPPER\r\n/SHIPPER ADDRESS\r\n/PLACE/STATE\r\n/SG/1234/FAX/12345678\r\n";
			CNE = "CNE/BREUSA\r\n/MR CONSIGNEE\r\n/CONSIGNEE ADDRESS\r\n/C PLACE/C STATE\r\n/US/4321/TLX/987654321\r\n";
			AGT = "AGT/ACCT NO/0238855/3223/PI\r\n/MR AGENT\r\n/IM HERE\r\n";
			NFY = "NFY/MR ALSO NOTIFY\r\n/NOTIFY ADDRESS\r\n/AN PLACE/AN STATE\r\n/KR/3321/PHO/556644\r\n";
			ACC = "ACC/ASD/SOME TEXT ASD\r\n/GEN/SOME TEXT QWE\r\n/FGT/SOME TEXT FGT\r\n/RTR/SOME TEXT RTR\r\n/QQQ/SOME TEXT QQQ\r\n/QQQ/SOME TEXT QQQ\r\n";
			CVD = "CVD/ZAR/TE/CP/123.54/NCV/XXX\r\n";
			RTD = "RTD/1/P5/L5.3/CQ/S12345/W34.5/R12.22/T421.59\r\n/NG/EGGS\r\n/2/P8/L9\r\n/3/PJED\r\n/4/NG/5 SLAC\r\n";
			OTH = "OTH/P/AWA120\r\n/P/XYZ130\r\n/P/ABC140\r\n/P/QWE190.12\r\n";
			PPD = "PPD/VC1/TX12.2\r\n/OA120/OC140/CT273.2\r\n";
			COL = "COL/WT421.59/VC2/TX55.99\r\n/CT479.58\r\n";
			CER = "CER/SIGNATURE\r\n";
			ISU = "ISU/02JUL04/CAPE TOWN/AGENT SIGNATURE\r\n";
			OSI = "OSI/SPECIAL SERVICE REQUEST THAT IS LONGER THAN 65 CHARS TO CHECK THE\r\n/REPEATED SEGMENTS SH1T IM RUNNING OUT\r\n";
			REF = "REF//C121212/FFW/CWID" + GlbCompany.CurrentCompany.LicenceKeyIdentifier + "/BNE\r\n";
			COR = "";

			awbHeader.ResetSupplyChainSecurityConfigurationForTesting();

			return awbHeader;
		}

		protected override Forwarding.AWB.Business.ExportAWBHeader GetNewFWBHeader() => Factory.New<ConsolExportAWBHeader>();

		void PopulateHeaderInfo(ConsolExportAWBHeader awbHeader)
		{
			awbHeader.EH_AWBOriginCode = "BNE";
			awbHeader.EH_AirportOfDestinationCode = "SIN";
			awbHeader.AWBRateLines[0].ER_NoOfPiecesOrRCP = "5";
			awbHeader.AWBRateLines[0].ER_GrossWeight = 5.3m;

			awbHeader.EH_ShippingLoadAndCount = 5;

			awbHeader.AWBRateLines[1].ER_NoOfPiecesOrRCP = "8";
			awbHeader.AWBRateLines[1].ER_GrossWeight = 9.0m;

			awbHeader.AWBRateLines[2].ER_NoOfPiecesOrRCP = "JED";

			awbHeader.AWBRateLines[0].ER_WeightInLBsOrKGs = Core.Constants.AWB.RateLineUQ.Pounds;

			awbHeader.EH_Booking1stCarrier = "QF";
			awbHeader.EH_Booking1stFlight = "123";
			awbHeader.EH_Booking1stFlightDate = "02";
			awbHeader.EH_Booking2ndCarrier = "SQ";
			awbHeader.EH_Booking2ndFlight = "856";
			awbHeader.EH_Booking2ndFlightDate = "12";

			awbHeader.EH_To1st = "LAX";
			awbHeader.EH_By1st = "QF";

			awbHeader.EH_To2nd = "CPT";
			awbHeader.EH_By2nd = "BA";

			awbHeader.EH_To3rd = "SIN";
			awbHeader.EH_By3rd = "SQ";

			awbHeader.EH_ShipperAccount = "BRECPT";
			awbHeader.EH_ShipperName = "Mr Shipper";
			awbHeader.EH_ShipperAddress = "Shipper Address";
			awbHeader.EH_ShipperPlace = "Place";
			awbHeader.EH_ShipperState = "State";
			awbHeader.EH_ShipperCountryCode = "SG";
			awbHeader.EH_ShipperPostCode = "1234";
			awbHeader.EH_ShipperContactCode = "FAX";
			awbHeader.EH_ShipperContactDetail = "+1 23(4)5+6-78";

			awbHeader.EH_ConsigneeAccount = "BREUSA";
			awbHeader.EH_ConsigneeName = "Mr Consignee";
			awbHeader.EH_ConsigneeAddress = "Consignee Address";
			awbHeader.EH_ConsigneePlace = "C Place";
			awbHeader.EH_ConsigneeState = "C State";
			awbHeader.EH_ConsigneeCountryCode = "US";
			awbHeader.EH_ConsigneePostCode = "4321";
			awbHeader.EH_ConsigneeContactCode = "TLX";
			awbHeader.EH_ConsigneeContactDetail = "+9 (876) 54321";

			Env.Registry.Freight.AirWaybill.IssuingCarrierAgentAccountNumber = "ACCT NO";

			Env.Registry.Freight.AirWaybill.IssuingCarrierAgentIATACode = "02388553223";
			awbHeader.EH_AgentParticipantIdentifier = "PI";
			Env.Registry.Freight.AirWaybill.IssuingCarrierAgentName = "Mr Agent";
			Env.Registry.Freight.AirWaybill.IssuingCarrierAgentCity = "Im Here";

			awbHeader.EH_HandlingInformation = "Special Service Request that is longer than 65 chars to check the repeated segments sh1t im running out";

			awbHeader.EH_AlsoNotifyName = "Mr Also Notify";
			awbHeader.EH_AlsoNotifyAddress = "Notify Address";
			awbHeader.EH_AlsoNotifyPlace = "AN Place";
			awbHeader.EH_AlsoNotifyState = "AN State";
			awbHeader.EH_AlsoNotifyCountryCode = "KR";
			awbHeader.EH_AlsoNotifyPostCode = "3321";
			awbHeader.EH_AlsoNotifyContactCode = "PHO";
			awbHeader.EH_AlsoNotifyContactDetail = "+55 (66) 44";

			ExportAWBAccountingInformation accInfo = awbHeader.AWBAccountingInformations.AddNew();
			accInfo.EA_InformationID = "asd";
			accInfo.EA_Information = "some text asd";

			accInfo = awbHeader.AWBAccountingInformations.AddNew();
			accInfo.EA_InformationID = "";
			accInfo.EA_Information = "some text qwe";

			accInfo = awbHeader.AWBAccountingInformations.AddNew();
			accInfo.EA_InformationID = "fgt";
			accInfo.EA_Information = "some text fgt";

			accInfo = awbHeader.AWBAccountingInformations.AddNew();
			accInfo.EA_InformationID = "rtr";
			accInfo.EA_Information = "some text rtr";

			accInfo = awbHeader.AWBAccountingInformations.AddNew();
			accInfo.EA_InformationID = "qqq";
			accInfo.EA_Information = "some text qqq";

			accInfo = awbHeader.AWBAccountingInformations.AddNew();
			accInfo.EA_InformationID = "qqq";
			accInfo.EA_Information = "some text qqq";

			accInfo = awbHeader.AWBAccountingInformations.AddNew();
			accInfo.EA_InformationID = "qqq";
			accInfo.EA_Information = "some text qqq";

			accInfo = awbHeader.AWBAccountingInformations.AddNew();
			accInfo.EA_InformationID = "qqq";
			accInfo.EA_Information = "some text qqq";

			awbHeader.EH_Currency = "ZAR";
			awbHeader.EH_ChargesCode = "TE";
			awbHeader.EH_WeightVPPDCOL = ConsolExportAWBHeader.Constants.PrepaidCollect3CharCodes.Collect;
			awbHeader.EH_OtherPPDCOL = ConsolExportAWBHeader.Constants.PrepaidCollect3CharCodes.Prepaid;
			awbHeader.EH_DeclaredValue = 123.54M;
			awbHeader.EH_CustomsValue = 0M;
			awbHeader.EH_InsuranceValue = 0M;

			ExportAWBRateLine rateLine = awbHeader.AWBRateLines[0];
			rateLine.ER_RateClass = "Q";
			rateLine.ER_CommodityItemNumber = "12345";
			rateLine.ER_ChargeableWeight = 34.5M;
			rateLine.ER_RateChargeOrDiscount = 12.22M;
			awbHeader.AWBRateLine1.NatureAndQtyOfGoodsDescription = "eggs";
			awbHeader.AWBRateLine2.NatureAndQtyOfGoodsDescription = "";
			awbHeader.AWBRateLine3.NatureAndQtyOfGoodsDescription = "";
			awbHeader.AWBRateLine4.NatureAndQtyOfGoodsDescription = "";

			ExportAWBOtherCharges otherCharge = awbHeader.AWBOtherCharges.AddNew();
			otherCharge.EO_ChargeCode = "AW";
			otherCharge.EO_EntitlementCode = "A";
			otherCharge.EO_Amount = 120M;

			otherCharge = awbHeader.AWBOtherCharges.AddNew();
			otherCharge.EO_ChargeCode = "XY";
			otherCharge.EO_EntitlementCode = "Z";
			otherCharge.EO_Amount = 130M;

			otherCharge = awbHeader.AWBOtherCharges.AddNew();
			otherCharge.EO_ChargeCode = "AB";
			otherCharge.EO_EntitlementCode = "C";
			otherCharge.EO_Amount = 140M;

			otherCharge = awbHeader.AWBOtherCharges.AddNew();
			otherCharge.EO_ChargeCode = "QW";
			otherCharge.EO_EntitlementCode = "E";
			otherCharge.EO_Amount = 190.12M;

			awbHeader.EH_TaxesPPD = 12.2M;
			awbHeader.EH_TaxesCOL = 55.99M;
			awbHeader.EH_ValuationPPD = 1M;
			awbHeader.EH_ValuationCOL = 2M;

			awbHeader.EH_ShippersSignature = "Signature";
			awbHeader.EH_AWBIssueDate = new ZDateTime(2004, 7, 2);
			awbHeader.EH_AWBIssuePlace = "cape town";
			awbHeader.EH_AWBAgentsSignature = "AGENT SIGNATURE";

			awbHeader.Consol.JK_UniqueConsignRef = "C121212";
		}

		void PopulateRateLine(int index, string noPieces, string weightUnit, ZDecimal grossWeight, string rateClass, string commodityItemNumber, ZDecimal chargeableWeight, ZDecimal rateChargeOrDiscount)
		{
			ExportAWBRateLine rateLine = ConsolAWBHeader.AWBRateLines[index];
			rateLine.ER_NoOfPiecesOrRCP = noPieces;
			rateLine.ER_WeightInLBsOrKGs = weightUnit;
			rateLine.ER_GrossWeight = grossWeight;
			rateLine.ER_RateClass = rateClass;
			rateLine.ER_CommodityItemNumber = commodityItemNumber;
			rateLine.ER_ChargeableWeight = chargeableWeight;
			rateLine.ER_RateChargeOrDiscount = rateChargeOrDiscount;
		}

		ZString CurrentCompanyCountryCode;
		ZString CurrentBranchPort;

		protected override void SetUp()
		{
			CurrentCompanyCountryCode = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			CurrentBranchPort = GlbBranch.CurrentBranch.GB_RL_NKHomePort;
			GlbBranch.CurrentBranch.GB_RL_NKHomePort = "AUBNE";
			GlbCompany.CurrentCompany.SetCountry("AU");
			base.SetUp();
		}

		protected override void TearDown()
		{
			base.TearDown();
			GlbCompany.CurrentCompany.GC_RN_NKCountryCode = CurrentCompanyCountryCode;
			GlbBranch.CurrentBranch.GB_RL_NKHomePort = CurrentBranchPort;
		}

		#region Expected Values

		string CGN = "";
		string FLT = "";
		string RTG = "";
		string SHP = "";
		string CNE = "";
		string AGT = "";
		string SSR = "";
		string NFY = "";
		string ACC = "";
		string CVD = "";
		string RTD = "";
		string OTH = "";
		string PPD = "";
		string COL = "";
		string CER = "";
		string ISU = "";
		string OSI = "";
		string REF = "";
		string COR = "";

		const string ExpectedFormatString =
			"FWB/10\r\n" +
			"{0}" + //CGN
			"{1}" + //FLT
			"{2}" + //RTG
			"{3}" + //SHP
			"{4}" + //CNE
			"{5}" + //AGT
			"{6}" + //SSR
			"{7}" + //NFY
			"{8}" + //ACC
			"{9}" + //CVD
			"{10}" + //RTD
			"{11}" + //OTH
			"{12}" + //PPD
			"{13}" + //COL
			"{14}" + //CER
			"{15}" + //ISU
			"{16}" + //OSI
			"{17}" + //REF			
			"{18}"; //COR

		#endregion

		#endregion
	}
}
