using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common.MessageBuilders;
using Enterprise.Customs.Universal.Messaging;
using Enterprise.Customs.Universal.Testing;
using Enterprise.Customs.ZA.Business.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.ZA.Business.MessageBuilders.Testing
{
	sealed class COSTCOHeaderTest : TestCaseWithFactory
	{
		public void TestPlaceOfLoading()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			header.MasterBill.ABL_RL_NKPortOfLoading = "VWG";
			AssertEquals("VWG", ((ICOSTCOMessageDataProvider)new COSTCOHeader(header)).PlaceOfLoading);
		}

		public void TestTerminalBerth()
		{
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			orgHeader.MainAddress.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.CodeTypes.TerminalControlledPremisesID, "ABD", Core.Constants.CountryCodes.SouthAfrica);
			Factory.Save();
			var header = Factory.New<AsycudaManifestHeader>();
			header.AMA_OA_DischargeTerminalAddress = orgHeader.MainAddress.PK;
			AssertEquals("ABD", ((ICOSTCOMessageDataProvider)new COSTCOHeader(header)).TerminalBerth);
		}

		public void TestIsDOR()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			header.AMA_ManifestType = ZString.Empty;
			AssertEquals(false, ((ICOSTCOMessageDataProvider)new COSTCOHeader(header)).IsDOR);
			header.AMA_ManifestType = ManifestTypeList.Codes.DepotOutturnReport;
			AssertEquals(true, ((ICOSTCOMessageDataProvider)new COSTCOHeader(header)).IsDOR);
		}

		public void TestIsBBB()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			header.AMA_ManifestType = ZString.Empty;
			AssertEquals(false, ((ICOSTCOMessageDataProvider)new COSTCOHeader(header)).IsBBB);
			header.AMA_ManifestType = ManifestTypeList.Codes.BulkBreakBulkOutturnReport;
			AssertEquals(true, ((ICOSTCOMessageDataProvider)new COSTCOHeader(header)).IsBBB);
		}

		public void TestIsVOR()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			header.AMA_ManifestType = ZString.Empty;
			AssertEquals(false, ((ICOSTCOMessageDataProvider)new COSTCOHeader(header)).IsVOR);
			header.AMA_ManifestType = ManifestTypeList.Codes.VesselOutturnReport;
			AssertEquals(true, ((ICOSTCOMessageDataProvider)new COSTCOHeader(header)).IsVOR);
		}

		public void TestIsAOR()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			header.AMA_ManifestType = ZString.Empty;
			AssertEquals(false, ((ICOSTCOMessageDataProvider)new COSTCOHeader(header)).IsAOR);
			header.AMA_ManifestType = ManifestTypeList.Codes.AirCargoOutturnReport;
			AssertEquals(true, ((ICOSTCOMessageDataProvider)new COSTCOHeader(header)).IsAOR);
		}

		public void TestIsEOR()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			header.AMA_ManifestType = ZString.Empty;
			AssertEquals(false, ((ICOSTCOMessageDataProvider)new COSTCOHeader(header)).IsEOR);
			header.AMA_ManifestType = ManifestTypeList.Codes.AirExcessOutturnReport;
			AssertEquals(true, ((ICOSTCOMessageDataProvider)new COSTCOHeader(header)).IsEOR);
		}

		public void TestIsALD()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			header.AMA_ManifestType = ZString.Empty;
			AssertEquals(false, ((ICOSTCOMessageDataProvider)new COSTCOHeader(header)).IsALD);
			header.AMA_ManifestType = ManifestTypeList.Codes.AirLoadDischarge;
			AssertEquals(true, ((ICOSTCOMessageDataProvider)new COSTCOHeader(header)).IsALD);
		}

		public void TestIsExport()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			header.AMA_Nature = ZString.Empty;
			AssertEquals(false, ((ICOSTCOMessageDataProvider)new COSTCOHeader(header)).IsExport);
			header.AMA_Nature = NatureList.Codes.Export22;
			AssertEquals(true, ((ICOSTCOMessageDataProvider)new COSTCOHeader(header)).IsExport);
		}

		public void TestIsImport()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			header.AMA_Nature = ZString.Empty;
			AssertEquals(false, ((ICOSTCOMessageDataProvider)new COSTCOHeader(header)).IsImport);
			header.AMA_Nature = NatureList.Codes.Import23;
			AssertEquals(true, ((ICOSTCOMessageDataProvider)new COSTCOHeader(header)).IsImport);
		}

		public void TestIEDIFACTMessageAttachee()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			var message = Factory.New<COSTCOEDIMessage>();
			var costcoHeader = new COSTCOHeader(header);
			((IEDIFACTMessageAttachee)costcoHeader).AddMessage(message);
			AssertEquals(message, ((IEDIFACTMessageAttachee)costcoHeader).Messages[0]);
			AssertEquals("", ((IEDIFACTMessageAttachee)costcoHeader).MessageStatus);
			AssertEquals("", ((IEDIFACTMessageAttachee)costcoHeader).JobStatus);
			AssertEquals("", ((IEDIFACTMessageAttachee)costcoHeader).JobIdentification);
			AssertEquals(header, ((IEDIFACTMessageAttachee)costcoHeader).TopLevelBusinessObject);
		}

		public void TestOutturnManifestType()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			header.AMA_ManifestType = ManifestTypeList.Codes.AirExcessOutturnReport;
			AssertEquals("EOR", ((ICOSTCOMessageDataProvider)new COSTCOHeader(header)).OutturnManifestType);
		}

		public void TestDocumentIssueDateTime()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			header.AMA_IssueDate = new ZDate(2017, 11, 2);
			AssertEquals(new ZDate(2017, 11, 2), ((ICOSTCOMessageDataProvider)new COSTCOHeader(header)).DocumentIssueDateTime);
		}

		public void TestActualArrivalDateTime()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			header.MasterBill.ABL_E_ARV = new ZDateTime(2018, 3, 21);
			AssertEquals(new ZDateTime(2018, 3, 21), ((ICOSTCOMessageDataProvider)new COSTCOHeader(header)).ActualArrivalDateTime);
		}

		public void TestEstimatedDateOfDeparture()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			header.MasterBill.ABL_E_DEP = new ZDateTime(2018, 3, 21);
			AssertEquals(new ZDateTime(2018, 3, 21), ((ICOSTCOMessageDataProvider)new COSTCOHeader(header)).EstimatedDateOfDeparture);
		}

		public void TestDateTimeFullyUnloadedLoaded()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			header.FullyLoadedUnloadedDate = new ZDateTime(2018, 3, 21);
			AssertEquals(new ZDateTime(2018, 3, 21), ((ICOSTCOMessageDataProvider)new COSTCOHeader(header)).DateTimeFullyUnloadedLoaded);
		}

		public void TestExcessIndicator()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			header.ExcessIndicator = "V";
			AssertEquals("V", ((ICOSTCOMessageDataProvider)new COSTCOHeader(header)).ExcessIndicator);
		}

		public void TestImportExportTranshipmentIndicator()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			header.AMA_Nature = NatureList.Codes.Import23;
			AssertEquals("929", ((ICOSTCOMessageDataProvider)new COSTCOHeader(header)).ImportExportTranshipmentIndicator);
			header.AMA_Nature = NatureList.Codes.Export22;
			AssertEquals("830", ((ICOSTCOMessageDataProvider)new COSTCOHeader(header)).ImportExportTranshipmentIndicator);
			header.AMA_Nature = NatureList.Codes.Transhipment28;
			AssertEquals("399", ((ICOSTCOMessageDataProvider)new COSTCOHeader(header)).ImportExportTranshipmentIndicator);
			header.AMA_Nature = NatureList.Codes.Transit24;
			AssertEquals("950", ((ICOSTCOMessageDataProvider)new COSTCOHeader(header)).ImportExportTranshipmentIndicator);
		}

		public void TestDocumentToBeAmended()
		{
			OutturnTestHelper.SetupZZ(Factory);
			var header = Factory.New<AsycudaManifestHeader>();
			var costco = Factory.New<COSTCOEDIMessage>();
			costco.EM_MessageNum = "123";
			costco.EM_MessageText = COSTCOTestMessage.Replace("\r\n", "");
			costco.EM_LinkUniqueID = header.PK;
			costco.EM_LinkTable = header.TablePrefix;
			var cusres = Factory.New<CUSRESEDIMessage>();
			cusres.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			cusres.EM_MessageText = CUSRESTestMessage.Replace("\r\n", "");
			cusres.EM_LinkUniqueID = header.PK;
			cusres.EM_LinkTable = header.TablePrefix;
			header.Messages.AddRange(costco, cusres);
			AssertEquals("B9C73560F3A54797909A498BE37010B5", ((ICOSTCOMessageDataProvider)new COSTCOHeader(header)).DocumentToBeAmended);
		}

		const string COSTCOTestMessage = @"UNH+316+COSTCO:D:16A:UN:RCG001'
BGM+788+B9C73560F3A54797909A498BE37010B5+9'
FTX+ADI'
TDT+20++++:172:20+++:103'
RFF+ACL'
LOC+11+:139:6+::ZZZ'
NAD+MS+::ZZZ'
NAD+RL+::ZZZ'
EQD+BB'
SEL+NO SEAL NO'
CNT+8:0'";
		const string CUSRESTestMessage = @"UNH+1+CUSRES:D:96B:UN:ZZZ01'
BGM+962+B9C73560F3A54797909A498BE37010B5:0'
DTM+132:20160331:102'
DTM+202:20160331:102'
TDT+20+SA123+4+++++::: '
LOC+22+JSA::ZZZ'
LOC+14+A2::ZZZ'
GIS+8:120:ZZZ:N'
NAD+AG+00626166'
RFF+BH:00626166HAWB123654'
DTM+137:20160331:102'
RFF+AAS:083-01203226'
DTM+137:20160331:102'
RFF+ABT:JSA201603315000938'
DTM+137:20160401:102'
RFF+ACD:123'
TAX+3+CUS:107:ZZZ'
MOA+161:2000'
CNT+7:120.00'
CNT+11:10'
UNT+21+1'";
		public void TestVoyageFlightNumber()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			header.AMA_Voyage = "VWG";
			AssertEquals("VWG", ((ICOSTCOMessageDataProvider)new COSTCOHeader(header)).VoyageFlightNumber);
		}

		public void TestTransportCode()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			header.AMA_TransportMode = Core.Constants.TransportModes.Sea;
			AssertEquals("1", ((ICOSTCOMessageDataProvider)new COSTCOHeader(header)).TransportCode);
			header.AMA_TransportMode = Core.Constants.TransportModes.Rail;
			AssertEquals("2", ((ICOSTCOMessageDataProvider)new COSTCOHeader(header)).TransportCode);
			header.AMA_TransportMode = Core.Constants.TransportModes.Road;
			AssertEquals("3", ((ICOSTCOMessageDataProvider)new COSTCOHeader(header)).TransportCode);
			header.AMA_TransportMode = Core.Constants.TransportModes.Air;
			AssertEquals("4", ((ICOSTCOMessageDataProvider)new COSTCOHeader(header)).TransportCode);
		}

		public void TestCarrierCode()
		{
			var org = Factory.New<OrgHeader>();
			org.OH_Code = "VVV";
			var address = org.Addresses.AddNew();
			org.CustomsCodes.AddNew(OrgCusCode.CodeTypes.CarrierCode, "VWG", Core.Constants.CountryCodes.SouthAfrica);
			var header = Factory.New<AsycudaManifestHeader>();
			header.AMA_OA_Carrier = address.PK;
			AssertEquals("VWG", ((ICOSTCOMessageDataProvider)new COSTCOHeader(header)).CarrierCode);
		}

		public void TestCallSign()
		{
			var vessel = Factory.New<RefVessel>();
			vessel.RV_Code = "123";
			vessel.RV_RadioCallSign = "VWG";
			Factory.Save();
			var header = Factory.New<AsycudaManifestHeader>();
			header.AMA_VesselName = "123";
			AssertEquals("VWG", ((ICOSTCOMessageDataProvider)new COSTCOHeader(header)).CallSign);
		}

		public void TestManifestType()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			header.AMA_Nature = NatureList.Codes.Import23;
			AssertEquals("23", ((ICOSTCOMessageDataProvider)new COSTCOHeader(header)).ManifestType);
			header.AMA_Nature = NatureList.Codes.Export22;
			AssertEquals("22", ((ICOSTCOMessageDataProvider)new COSTCOHeader(header)).ManifestType);
			header.AMA_Nature = NatureList.Codes.Transhipment28;
			AssertEquals("28", ((ICOSTCOMessageDataProvider)new COSTCOHeader(header)).ManifestType);
			header.AMA_Nature = NatureList.Codes.Transit24;
			AssertEquals("24", ((ICOSTCOMessageDataProvider)new COSTCOHeader(header)).ManifestType);
			header.AMA_Nature = NatureList.Codes.FreightRemainingOnBoard;
			AssertEquals("57", ((ICOSTCOMessageDataProvider)new COSTCOHeader(header)).ManifestType);
		}

		public void TestPrincipalCarrierConveyageNumber()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			header.AMA_Voyage = "VWG";
			AssertEquals("VWG", ((ICOSTCOMessageDataProvider)new COSTCOHeader(header)).PrincipalCarrierConveyageNumber);
		}

		public void TestPlaceOfDicharge()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			header.MasterBill.ABL_RL_NKPortOfDischarge = "VWG";
			AssertEquals("VWG", ((ICOSTCOMessageDataProvider)new COSTCOHeader(header)).PlaceOfDicharge);
		}

		public void TestTerminalDepotCode()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			header.MasterBill.ABL_GoodsLocation = "VWG";
			AssertEquals("VWG", ((ICOSTCOMessageDataProvider)new COSTCOHeader(header)).TerminalDepotCode);
		}

		public void TestMessageSender()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Facilities, "desc.");
			var outturnProvider = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.SouthAfrica, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Facilities, "VW", "Dsc.", ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(2));
			helper.CreateNewOrGetExistingRefCusCodeListAttributeName(Core.Constants.Customs.Universal.RefCusCodeList.Attributes.OutturnFacilityCode, "Desc.", Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Facilities, Core.Constants.CountryCodes.SouthAfrica);
			outturnProvider.Attributes.AddNew(Core.Constants.Customs.Universal.RefCusCodeList.Attributes.OutturnFacilityCode, "00000001");
			Factory.Save();
			var header = Factory.New<AsycudaManifestHeader>();
			header.OutturnProvider = "02";
			AssertEquals("02", ((ICOSTCOMessageDataProvider)new COSTCOHeader(header)).MessageSender);
			header.OutturnProvider = "VW";
			AssertEquals("00000001", ((IInterchangeSenderIdProvider)header).SenderID);
		}

		public void TestOutturnProviderCode()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			header.OutturnProvider = "02";
			AssertEquals("02", ((ICOSTCOMessageDataProvider)new COSTCOHeader(header)).OutturnProviderCode);
		}

		public void TestTotalNumberOfPackages()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			var bill1 = header.Bills.AddNew();
			var pack11 = bill1.Packs.AddNew();
			pack11.APA_PackQty = 1;
			pack11.Outturn.C5_PackagesOutturned = 2;
			var pack12 = bill1.Packs.AddNew();
			pack12.APA_PackQty = 2;
			pack12.Outturn.C5_PackagesOutturned = 3;
			var bill2 = header.Bills.AddNew();
			var pack21 = bill2.Packs.AddNew();
			pack21.APA_PackQty = 3;
			pack21.Outturn.C5_PackagesOutturned = 4;
			var pack22 = bill2.Packs.AddNew();
			pack22.APA_PackQty = 4;
			pack22.Outturn.C5_PackagesOutturned = 5;
			AssertEquals(14, ((ICOSTCOMessageDataProvider)new COSTCOHeader(header)).TotalNumberOfPackages);
		}

		public void TestBills()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			var bill1 = header.Bills.AddNew();
			var bill2 = header.Bills.AddNew();
			AssertEquals(2, ((ICOSTCOMessageDataProvider)new COSTCOHeader(header)).Bills.Count());
		}

		public void TestContainers()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			AssertEquals(1, ((ICOSTCOMessageDataProvider)new COSTCOHeader(header)).Containers.Count());
			AssertType<MockCOSTCOContainer>(((ICOSTCOMessageDataProvider)new COSTCOHeader(header)).Containers.First());
			header.Containers.AddNew();
			AssertEquals(1, ((ICOSTCOMessageDataProvider)new COSTCOHeader(header)).Containers.Count());
			AssertType<COSTCOContainer>(((ICOSTCOMessageDataProvider)new COSTCOHeader(header)).Containers.First());
		}
	}
}
