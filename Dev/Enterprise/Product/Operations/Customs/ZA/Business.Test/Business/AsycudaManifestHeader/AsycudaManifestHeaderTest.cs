using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.Common.MessageBuilders;
using Enterprise.Customs.ManifestBase;
using Enterprise.Customs.Universal.Messaging;
using Enterprise.Customs.Universal.Testing;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.Messaging.Business;
using NUnit.Framework;
using Constants = Enterprise.Core.Constants;

namespace Enterprise.Customs.ZA.Business.Testing
{
	[TestedType(typeof(AsycudaManifestHeader))]
	sealed class AsycudaManifestHeaderTest : ManifestBase.Testing.AsycudaManifestHeaderTest
	{
		public void TestBills()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			AssertType<AsycudaBillCollection>(header.Bills);
		}

		public void TestFullyLoadedUnloadedDate()
		{
			var header = (AsycudaManifestHeader)GetNewBusinessObject();
			header.FullyLoadedUnloadedDate = new ZDateTime(2018, 3, 6);
			Factory.Save();

			header = new BusinessObjectFactory().Load<AsycudaManifestHeader>(header.PK);
			AssertEquals(new ZDateTime(2018, 3, 6), header.FullyLoadedUnloadedDate);
		}

		public void TestFullyLoadedUnloadedDate_ManifestTypeChanges()
		{
			var header = (AsycudaManifestHeader)GetNewBusinessObject();
			header.AMA_ManifestType = ManifestTypeList.Codes.BulkBreakBulkOutturnReport;
			header.FullyLoadedUnloadedDate = ZDateTime.Today;
			header.AMA_ManifestType = ZString.Empty;
			AssertEquals(ZDateTime.Empty, header.FullyLoadedUnloadedDate);

			header.AMA_ManifestType = ManifestTypeList.Codes.VesselOutturnReport;
			header.FullyLoadedUnloadedDate = ZDateTime.Today;
			header.AMA_ManifestType = ZString.Empty;
			AssertEquals(ZDateTime.Empty, header.FullyLoadedUnloadedDate);

			header.AMA_ManifestType = ManifestTypeList.Codes.AirExcessOutturnReport;
			header.FullyLoadedUnloadedDate = ZDateTime.Today;
			header.AMA_ManifestType = ZString.Empty;
			AssertEquals(ZDateTime.Empty, header.FullyLoadedUnloadedDate);

			header.AMA_ManifestType = ManifestTypeList.Codes.AirLoadDischarge;
			header.FullyLoadedUnloadedDate = ZDateTime.Today;
			header.AMA_ManifestType = ZString.Empty;
			AssertEquals(ZDateTime.Empty, header.FullyLoadedUnloadedDate);
		}

		public void TestFullyLoadedUnloadedDateVisible()
		{
			var header = (AsycudaManifestHeader)GetNewBusinessObject();
			header.AMA_ManifestType = ManifestTypeList.Codes.BulkBreakBulkOutturnReport;
			AssertEquals(true, header.FullyLoadedUnloadedDateVisible);

			header.AMA_ManifestType = ZString.Empty;
			AssertEquals(false, header.FullyLoadedUnloadedDateVisible);

			header.AMA_ContainerMode = Core.Constants.ContainerModes.BreakBulk;
			header.AMA_ManifestType = ManifestTypeList.Codes.VesselOutturnReport;
			AssertEquals(true, header.FullyLoadedUnloadedDateVisible);

			header.AMA_ContainerMode = Core.Constants.ContainerModes.Containerised;
			header.AMA_ManifestType = ManifestTypeList.Codes.VesselOutturnReport;
			AssertEquals(false, header.FullyLoadedUnloadedDateVisible);

			header.AMA_ManifestType = ZString.Empty;
			AssertEquals(false, header.FullyLoadedUnloadedDateVisible);

			header.AMA_ManifestType = ManifestTypeList.Codes.AirExcessOutturnReport;
			AssertEquals(true, header.FullyLoadedUnloadedDateVisible);

			header.AMA_ManifestType = ZString.Empty;
			AssertEquals(false, header.FullyLoadedUnloadedDateVisible);

			header.AMA_ManifestType = ManifestTypeList.Codes.AirLoadDischarge;
			AssertEquals(true, header.FullyLoadedUnloadedDateVisible);

			header.AMA_ManifestType = ZString.Empty;
			AssertEquals(false, header.FullyLoadedUnloadedDateVisible);

			header.AMA_ManifestType = ManifestTypeList.Codes.AirCargoOutturnReport;
			AssertEquals(true, header.FullyLoadedUnloadedDateVisible);
		}

		public void TestUnpackedDate()
		{
			var header = (AsycudaManifestHeader)GetNewBusinessObject();
			header.UnpackedDate = new ZDateTime(2018, 3, 6);
			Factory.Save();

			header = new BusinessObjectFactory().Load<AsycudaManifestHeader>(header.PK);
			AssertEquals(new ZDateTime(2018, 3, 6), header.UnpackedDate);
		}

		public void TestExcessIndicator()
		{
			var header = (AsycudaManifestHeader)GetNewBusinessObject();
			header.ExcessIndicator = "1";
			Factory.Save();

			header = new BusinessObjectFactory().Load<AsycudaManifestHeader>(header.PK);
			AssertEquals("1", header.ExcessIndicator);
		}

		public void TestExcessIndicator_ManifestTypeChanges()
		{
			var header = (AsycudaManifestHeader)GetNewBusinessObject();
			header.AMA_ManifestType = ManifestTypeList.Codes.VesselOutturnReport;
			header.ExcessIndicator = ExcessIndicatorList.Codes.Excess;
			header.AMA_ManifestType = ZString.Empty;
			AssertEquals(ZString.Empty, header.ExcessIndicator);

			header.AMA_ManifestType = ManifestTypeList.Codes.AirExcessOutturnReport;
			header.ExcessIndicator = ExcessIndicatorList.Codes.Excess;
			header.AMA_ManifestType = ZString.Empty;
			AssertEquals(ZString.Empty, header.ExcessIndicator);
		}

		public void TestExcessIndicatorVisible()
		{
			var header = (AsycudaManifestHeader)GetNewBusinessObject();
			header.AMA_ManifestType = ManifestTypeList.Codes.VesselOutturnReport;
			AssertEquals(true, header.ExcessIndicatorVisible);

			header.AMA_ManifestType = ZString.Empty;
			AssertEquals(false, header.ExcessIndicatorVisible);

			header.AMA_ManifestType = ManifestTypeList.Codes.AirExcessOutturnReport;
			AssertEquals(true, header.ExcessIndicatorVisible);
		}

		public void TestRegistrationNumber()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			header.RegistrationNumber = "123";

			var entryNum = CusEntryNumber.Load(header, CusEntryNumberTypes.ASYCUDA.AsycudaRegistration, Core.Constants.CountryCodes.SouthAfrica, false);
			AssertNotNull("CusEntryNumber created", entryNum);
			AssertEquals("CE_EntryNum is set", "123", entryNum.CE_EntryNum);
			AssertEquals("CE_EntryStatus is set", Common.Shared.AsycudaRegistrationStatuses.Codes.Registered, entryNum.CE_EntryStatus);
			Assert("CE_IssueDate is set", entryNum.CE_IssueDate.AddMinutes(1) > ZDateTime.Now);

			entryNum.CE_EntryNum = "456";
			AssertEquals("456", header.RegistrationNumber);
			entryNum.CE_EntryType = "";
			AssertEquals("", header.RegistrationNumber);
		}

		[TestDate(1986, 3, 12, 4, 27, 0)]
		public void TestRegistrationDetails()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();

			AssertEquals(ZDateTime.Empty, header.RegistrationDate);
			AssertEquals(ZString.Empty, header.RegistrationStatus);
			AssertEquals(ZString.Empty, header.RegistrationNumber);
			AssertEquals(true, header.RegistrationStatusInfo.ReadOnly);
			AssertEquals(true, header.RegistrationDateInfo.ReadOnly);
			AssertEquals(true, header.RegistrationNumberInfo.ReadOnly);

			header.RegistrationNumber = "Poop";
			AssertEquals(Common.Shared.AsycudaRegistrationStatuses.Codes.Registered, header.RegistrationStatus);
			AssertEquals(new ZDateTime(1986, 3, 12, 4, 27, 0), header.RegistrationDate);

			header.RegistrationDate = new ZDateTime(2015, 8, 22, 14, 0, 0);
			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			var headerReloaded = newFactory.Load<AsycudaManifestHeader>(header.PK);
			AssertEquals("Poop", headerReloaded.RegistrationNumber);
			AssertEquals(new ZDateTime(2015, 8, 22, 14, 0, 0), headerReloaded.RegistrationDate);
			AssertEquals(Common.Shared.AsycudaRegistrationStatuses.Codes.Registered, headerReloaded.RegistrationStatus);

			AssertEquals($"country.RegistrationStatusInfo.ReadOnly", true, headerReloaded.RegistrationStatusInfo.ReadOnly);
			AssertEquals($"country.RegistrationDateInfo.ReadOnly", true, headerReloaded.RegistrationDateInfo.ReadOnly);
			AssertEquals($"country.RegistrationNumberInfo.ReadOnly", true, headerReloaded.RegistrationNumberInfo.ReadOnly);
		}

		public void TestAMA_VesselName_Invalid()
		{
			var vessel = Factory.New<RefVessel>();
			vessel.RV_Name = "VESSEL";
			vessel.RV_LloydsNumber = "9832343";
			vessel.RV_RadioCallSign = "CALLME";
			vessel.RV_RN_NKCountryOfReg = Core.Constants.CountryCodes.Australia;
			var header = Factory.New<AsycudaManifestHeader>();
			header.DefaultVesselValues(vessel);
			CombineAssertions("Pre-condition", () =>
			{
				AssertEquals("AMA_VesselName", "VESSEL", header.AMA_VesselName);
				AssertEquals("AMA_LloydsNumber", "9832343", header.AMA_LloydsNumber);
				AssertEquals("AMA_RadioCallSign", "CALLME", header.AMA_RadioCallSign);
				AssertEquals("AMA_RN_NKConveyanceNationality", Core.Constants.CountryCodes.Australia, header.AMA_RN_NKConveyanceNationality);
			});
			header.AMA_VesselName = "!@#";
			CombineAssertions("Invalid", () =>
			{
				AssertEquals("AMA_VesselName", "!@#", header.AMA_VesselName);
				AssertEquals("AMA_LloydsNumber", ZString.Empty, header.AMA_LloydsNumber);
				AssertEquals("AMA_RadioCallSign", ZString.Empty, header.AMA_RadioCallSign);
				AssertEquals("AMA_RN_NKConveyanceNationality", ZString.Empty, header.AMA_RN_NKConveyanceNationality);
			});
		}

		public void TestJobNumber()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			header.AMA_JobReference = "123";
			AssertEquals("123", header.JobNumber);
		}

		public void TestAMA_RN_NKCountry()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			AssertEquals(Constants.CountryCodes.SouthAfrica, header.AMA_RN_NKCountry);
		}

		public void TestIsTranshipment()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			header.AMA_Nature = NatureList.Codes.Transhipment28;
			AssertEquals(true, header.IsTranshipment);

			header.AMA_Nature = NatureList.Codes.Transit24;
			AssertEquals(false, header.IsTranshipment);
		}

		public void TestIsDOR()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			header.AMA_ManifestType = ZString.Empty;
			AssertEquals(false, header.IsDOR);

			header.AMA_ManifestType = ManifestTypeList.Codes.DepotOutturnReport;
			AssertEquals(true, header.IsDOR);
		}

		public void TestIsBBB()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			header.AMA_ManifestType = ZString.Empty;
			AssertEquals(false, header.IsBBB);

			header.AMA_ManifestType = ManifestTypeList.Codes.BulkBreakBulkOutturnReport;
			AssertEquals(true, header.IsBBB);
		}

		public void TestIsVOR()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			header.AMA_ManifestType = ZString.Empty;
			AssertEquals(false, header.IsVOR);

			header.AMA_ManifestType = ManifestTypeList.Codes.VesselOutturnReport;
			AssertEquals(true, header.IsVOR);
		}

		public void TestIsAOR()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			header.AMA_ManifestType = ZString.Empty;
			AssertEquals(false, header.IsAOR);

			header.AMA_ManifestType = ManifestTypeList.Codes.AirCargoOutturnReport;
			AssertEquals(true, header.IsAOR);
		}

		public void TestIsEOR()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			header.AMA_ManifestType = ZString.Empty;
			AssertEquals(false, header.IsEOR);

			header.AMA_ManifestType = ManifestTypeList.Codes.AirExcessOutturnReport;
			AssertEquals(true, header.IsEOR);
		}

		public void TestIsALD()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			header.AMA_ManifestType = ZString.Empty;
			AssertEquals(false, header.IsALD);

			header.AMA_ManifestType = ManifestTypeList.Codes.AirLoadDischarge;
			AssertEquals(true, header.IsALD);
		}

		public void TestIsCOSTCO()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			foreach (var code in new ManifestTypeList().GetAllCodes())
			{
				header.AMA_ManifestType = code;
				AssertEquals($"When AMA_ManifestType = {code}", true, header.IsCOSTCO);
			}

			header.AMA_ManifestType = ZString.Empty;
			foreach (var code in new GateInOutMessageTypeCodeList().GetAllCodes())
			{
				header.GateInOutMessageType = code;
				AssertEquals($"When GateInOutMessageType = {code}", false, header.IsCOSTCO);
			}
		}

		public void TestIsGOVGIO_Properties()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			var values = new (string GateInOutMessageType, bool IsTGO, bool IsTGI, bool IsDGI, bool IsDGO, bool IsDCI, bool IsATI, bool IsADI, bool IsBGI)[]
			{
				("TGO", true, false, false, false, false, false, false, false),
				("TGI", false, true, false, false, false, false, false, false),
				("DGI", false, false, true, false, false, false, false, false),
				("DGO", false, false, false, true, false, false, false, false),
				("DCI", false, false, false, false, true, false, false, false),
				("ATI", false, false, false, false, false, true, false, false),
				("ADI", false, false, false, false, false, false, true, false),
				("BGI", false, false, false, false, false, false, false, true),
			};

			foreach (var (type, isTGO, isTGI, isDGI, isDGO, isDCI, isATI, isADI, isBGI) in values)
			{
				header.GateInOutMessageType = type;
				CombineAssertions($"When GateInOutMessageType = {type}", () =>
				{
					AssertEquals("IsTGO", isTGO, header.IsTGO);
					AssertEquals("IsTGI", isTGI, header.IsTGI);
					AssertEquals("IsDGI", isDGI, header.IsDGI);
					AssertEquals("IsDGO", isDGO, header.IsDGO);
					AssertEquals("IsDCI", isDCI, header.IsDCI);
					AssertEquals("IsATI", isATI, header.IsATI);
					AssertEquals("IsADI", isADI, header.IsADI);
					AssertEquals("IsBGI", isBGI, header.IsBGI);
				});
			}
		}

		public void TestIsGOVGIO()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			foreach (var code in new GateInOutMessageTypeCodeList().GetAllCodes())
			{
				header.GateInOutMessageType = code;
				AssertEquals($"When GateInOutMessageType = {code}", true, header.IsGOVGIO);
			}

			header.GateInOutMessageType = ZString.Empty;
			foreach (var code in new ManifestTypeList().GetAllCodes())
			{
				header.AMA_ManifestType = code;
				AssertEquals($"When AMA_ManifestType = {code}", false, header.IsGOVGIO);
			}
		}

		public void TestIsExport()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			header.AMA_Nature = ZString.Empty;
			AssertEquals(false, header.IsExport);

			header.AMA_Nature = NatureList.Codes.Export22;
			AssertEquals(true, header.IsExport);
		}

		public void TestIsImport()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			header.AMA_Nature = ZString.Empty;
			AssertEquals(false, header.IsImport);

			header.AMA_Nature = NatureList.Codes.Import23;
			AssertEquals(true, header.IsImport);
		}

		public void TestCustomsOfficeDescription()
		{
			var testHelper = new ZAUniversalReferenceTestDataHelper(Factory, setupBasicTariffData: false);
			testHelper.CreateCustomsOfficeCusCodeEntry("BBR");
			Factory.Save();
			var header = Factory.New<AsycudaManifestHeader>();
			header.AMA_CustomsOffice = "BBR";
			AssertEquals("Beit Bridge", header.CustomsOfficeDescription);
		}

		public void TestClearLRNIfNotEXP()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			header.AMA_JobReference = "123";
			header.AMA_Nature = NatureList.Codes.Export22;
			var bill = header.Bills.AddNew();
			bill.ABL_BillNumber = "456";
			bill.LRN = "123";

			header.AMA_Nature = NatureList.Codes.Import23;
			AssertEquals(ZString.Empty, bill.LRN);
		}

		public void TestClearCustomsCPCIfNotEXP()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			header.AMA_JobReference = "123";
			header.AMA_Nature = NatureList.Codes.Export22;
			var bill = header.Bills.AddNew();
			bill.ABL_BillNumber = "456";
			bill.CustomsCPC = "123";

			header.AMA_Nature = NatureList.Codes.Import23;
			AssertEquals(ZString.Empty, bill.CustomsCPC);
		}

		public void TestTerminalBerth()
		{
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			orgHeader.MainAddress.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.CodeTypes.TerminalControlledPremisesID, "ABD", Constants.CountryCodes.SouthAfrica);
			Factory.Save();

			var header = Factory.New<AsycudaManifestHeader>();
			header.AMA_OA_DischargeTerminalAddress = orgHeader.MainAddress.PK;
			AssertEquals("ABD", header.TerminalBerth);
		}

		public void TestIsCNT()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			header.AMA_ContainerMode = Constants.ContainerModes.Containerised;
			AssertEquals(true, header.IsCNT);

			header.AMA_ContainerMode = Constants.ContainerModes.AIR;
			AssertEquals(false, header.IsCNT);
		}

		public void TestClearInvalidGateInOutMessageTypeIfNeeded()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			header.AMA_TransportMode = Constants.TransportModes.Air;
			header.GateInOutMessageType = GateInOutMessageTypeCodeList.Codes.AirDepotGateIn;
			header.AMA_TransportMode = Constants.TransportModes.Sea;
			AssertEquals(ZString.Empty, header.GateInOutMessageType);
		}

		public void TestCarrierCode()
		{
			var org = Factory.New<OrgHeader>();
			org.OH_Code = "VVV";
			var address = org.Addresses.AddNew();
			org.CustomsCodes.AddNew(OrgCusCode.CodeTypes.CarrierCode, "VWG", Constants.CountryCodes.SouthAfrica);

			var header = Factory.New<AsycudaManifestHeader>();
			header.AMA_OA_Carrier = address.PK;

			AssertEquals("VWG", header.CarrierCode);
		}

		public void TestIInterchangeSenderIdProvider_SenderID()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Constants.Customs.Universal.RefCusCodeListTypes.Codes.Facilities, "desc.");
			var outturnProvider = helper.CreateNewOrGetExistingCusCodeList(Constants.CountryCodes.SouthAfrica, Constants.Customs.Universal.RefCusCodeListTypes.Codes.Facilities, "VW", "Dsc.", ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(2));
			helper.CreateNewOrGetExistingRefCusCodeListAttributeName(Constants.Customs.Universal.RefCusCodeList.Attributes.OutturnFacilityCode, "Desc.", Constants.Customs.Universal.RefCusCodeListTypes.Codes.Facilities, Core.Constants.CountryCodes.SouthAfrica);
			outturnProvider.Attributes.AddNew(Constants.Customs.Universal.RefCusCodeList.Attributes.OutturnFacilityCode, "00000001");
			Factory.Save();

			var header = Factory.New<AsycudaManifestHeader>();
			header.OutturnProvider = "12";
			AssertEquals("12", ((IInterchangeSenderIdProvider)header).SenderID);

			header.OutturnProvider = "VW";
			AssertEquals("00000001", ((IInterchangeSenderIdProvider)header).SenderID);
		}

		public void TestDocManagerInfo()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			AssertNotNull(((IDocManagerSupport)header).DocManagerInfo);
			AssertType<AsycudaManifestHeaderDocManagerInfo>(((IDocManagerSupport)header).DocManagerInfo);
		}

		public void TestIsRoad()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			AssertEquals(false, header.IsRoad);

			header.AMA_TransportMode = Constants.TransportModes.Road;
			AssertEquals(true, header.IsRoad);
		}

		public void TestAMA_VoyageVisible()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			AssertEquals(true, header.AMA_VoyageVisible);

			header.AMA_TransportMode = Constants.TransportModes.Road;
			AssertEquals(false, header.AMA_VoyageVisible);
		}

		public void TestAMA_ContainerModeVisible()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			AssertEquals(false, header.AMA_ContainerModeVisible);

			header.AMA_TransportMode = Constants.TransportModes.Sea;
			AssertEquals(true, header.AMA_ContainerModeVisible);
		}

		public void TestHumanReadableName()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			AssertEquals("Outturn & Gate In/Out", header.HumanReadableName);
			header.AMA_JobReference = "123";
			AssertEquals("123", header.HumanReadableName);
		}

		public void TestAMA_BillIssueDate()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			header.MasterBill.ABL_BillIssueDate = new ZDate(2018, 03, 06);
			AssertEquals(new ZDate(2018, 03, 06), header.AMA_IssueDate);
		}

		public void TestIsAir()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			header.AMA_TransportMode = ZString.Empty;
			AssertEquals(false, header.IsAir);
			header.AMA_TransportMode = Constants.TransportModes.Air;
			AssertEquals(true, header.IsAir);
		}

		public void TestIsSea()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			header.AMA_TransportMode = ZString.Empty;
			AssertEquals(false, header.IsSea);
			header.AMA_TransportMode = Constants.TransportModes.Sea;
			AssertEquals(true, header.IsSea);
		}

		public void TestVoyageFlightNoLable()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			header.AMA_TransportMode = ZString.Empty;
			AssertEquals("Flight/Voyage", header.VoyageFlightNoLabel);
			header.AMA_TransportMode = Constants.TransportModes.Sea;
			AssertEquals("Voyage", header.VoyageFlightNoLabel);
			header.AMA_TransportMode = Constants.TransportModes.Air;
			AssertEquals("Flight", header.VoyageFlightNoLabel);
		}

		public void TestOutturnProvider()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			header.MasterBill.ABL_GoodsLocation = "123";
			AssertEquals("123", header.OutturnProvider);
		}

		public void TestGateInOutMessageType()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			header.AMA_TransportMode = Constants.TransportModes.Sea;
			header.GateInOutMessageType = GateInOutMessageTypeCodeList.Codes.SeaDepotConsignmentGateIn;
			header.Factory.Save();

			header = new BusinessObjectFactory().Load<AsycudaManifestHeader>(header.PK);
			AssertEquals(GateInOutMessageTypeCodeList.Codes.SeaDepotConsignmentGateIn, header.GateInOutMessageType);

			header.GateInOutMessageType = ZString.Empty;
			header.Factory.Save();

			header = new BusinessObjectFactory().Load<AsycudaManifestHeader>(header.PK);
			AssertEquals(ZString.Empty, header.GateInOutMessageType);
		}

		public void TestGateInOutDate()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			header.AMA_TransportMode = Constants.TransportModes.Air;
			header.GateInOutDate = ZDateTime.BrettsBirthday;
			header.Factory.Save();

			header = new BusinessObjectFactory().Load<AsycudaManifestHeader>(header.PK);
			AssertEquals(ZDateTime.BrettsBirthday, header.GateInOutDate);

			header.GateInOutDate = ZDateTime.Empty;
			header.Factory.Save();

			header = new BusinessObjectFactory().Load<AsycudaManifestHeader>(header.PK);
			AssertEquals(ZDateTime.Empty, header.GateInOutDate);
		}

		public void TestGateInOutDate_Cleanup()
		{
			var header = Factory.New<AsycudaManifestHeader>();

			header.AMA_TransportMode = Constants.TransportModes.Air;
			header.GateInOutDate = ZDateTime.BrettsBirthday;
			AssertEquals(ZDateTime.BrettsBirthday, header.GateInOutDate);

			header.AMA_TransportMode = ZString.Empty;
			AssertEquals(ZDateTime.BrettsBirthday, header.GateInOutDate);

			header.AMA_TransportMode = Constants.TransportModes.Air;
			AssertEquals(ZDateTime.BrettsBirthday, header.GateInOutDate);

			header.AMA_TransportMode = Constants.TransportModes.Mail;
			AssertEquals(ZDateTime.BrettsBirthday, header.GateInOutDate);

			header.AMA_TransportMode = Constants.TransportModes.Road;
			AssertEquals(ZDateTime.BrettsBirthday, header.GateInOutDate);

			header.AMA_TransportMode = Constants.TransportModes.Sea;
			AssertEquals(ZDateTime.Empty, header.GateInOutDate);
		}

		public void TestAMA_MasterBill()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			header.MasterBill.ABL_BillNumber = "123";
			AssertEquals("123", header.AMA_MasterBill);
		}

		public void TestParentBill()
		{
			var header = (AsycudaManifestHeader)GetNewBusinessObject();
			header.ParentBill = "123";
			Factory.Save();
			header = new BusinessObjectFactory().Load<AsycudaManifestHeader>(header.PK);
			AssertEquals("123", header.ParentBill);
		}

		public void TestIEDIFACTMessageAttacheeAndEDIFACTMessageStatusCalculatorProviderMembers()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var cusof = helper.CreateNewOrGetExistingCusCodeType(Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsManifestStatus, "Customs Manifest Status");
			var abc = helper.CreateNewOrGetExistingCusCodeList(Constants.CountryCodes.SouthAfrica, Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsManifestStatus, "6", "Rejected", ZDateTime.Today.AddDays(-1), ZDateTime.Today.AddDays(1));
			var cde = helper.CreateNewOrGetExistingCusCodeList(Constants.CountryCodes.SouthAfrica, Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsManifestStatus, "8", "Proceed to Border", ZDateTime.Today.AddDays(-1), ZDateTime.Today.AddDays(1));

			Factory.Save();

			var header = (AsycudaManifestHeader)GetNewBusinessObject();
			var attachee = (IEDIFACTMessageAttacheeAndEDIFACTMessageStatusCalculatorProvider)header;

			var calculator = attachee.GetCalculator("");
			AssertType<MessageManagers.EDIFACTStatusCalculator>(calculator);
			AssertEquals("CUSCAR", calculator.MessageTypeDescription);

			AssertEquals(0, header.Messages.Count);
			var message = Factory.New<EDIMessage>();
			attachee.AddMessage(message);
			AssertEquals(header, message.EM_LinkedObject);
			AssertEquals(1, header.Messages.Count);

			AssertEquals("", header.AMA_MessageStatus);
			attachee.MessageStatus = "6";
			AssertEquals("6", header.AMA_MessageStatus);
			AssertEquals("6", attachee.MessageStatus);

			AssertEquals("", header.RegistrationStatus);
			attachee.JobStatus = "ACO";
			AssertEquals("ACO", header.RegistrationStatus);
			AssertEquals("ACO", attachee.JobStatus);

			header.AMA_MasterBill = "MAN001";
			AssertEquals("MAN001", attachee.JobIdentification);

			AssertEquals(header, attachee.TopLevelBusinessObject);
		}

		public void TestGateInOutCustomsStatusGenAddOnColumn()
		{
			var header = (AsycudaManifestHeader)GetNewBusinessObject();
			header.GateInOutCustomsStatus = "CLS";
			Factory.Save();

			AssertEquals("CLS", header.GetSystemDefinedValue<ZString>(AsycudaManifestHeader.Schema.GateInOutCustomsStatus));
		}

		public void TestBookingNumber()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			header.MasterBill.ABL_CarrierReference = "110063";
			AssertEquals("110063", header.BookingNumber);
		}

		public void TestAMA_ApplicationCode()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			AssertEquals(AsycudaManifestHeader.ApplicationCode_Out, header.AMA_ApplicationCode);
		}

		public void TestSavingSetsJobReference()
		{
			TestConnection.BeginTransaction();
			try
			{
				Env.NumberFountains.ManifestJobReference.SetNext(Factory, 6789);
				var header = Factory.New<AsycudaManifestHeader>();
				header.OnSaving();
				AssertEquals("OGM0000001", header.AMA_JobReference);
			}
			finally
			{
				TestConnection.RollbackTransaction();
			}
		}

		public void TestDeletionInCorrectOrder()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			header.MasterBill.ABL_BillNumber = "ZAMASTER1";
			var bill = header.Bills.AddNew();
			var message = header.Messages.AddNew();
			message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			Factory.Save();

			header.MasterBill.NotificationsChanged += (object sender, NotificationsChangedEventArgs e) =>
			{
				if (header.IsDeleted)
				{
					Assert("This is wrong MasterBill should not be deleted after the Header", false);
				}
			};
			bill.NotificationsChanged += (object sender, NotificationsChangedEventArgs e) =>
			{
				if (header.IsDeleted)
				{
					Assert("This is wrong Bill should not be deleted after the Header", false);
				}
			};
			message.NotificationsChanged += (object sender, NotificationsChangedEventArgs e) =>
			{
				if (header.IsDeleted)
				{
					Assert("This is wrong Messages should not be deleted after the Header", false);
				}
			};

			header.Delete();
			AssertNoExceptionThrown(Factory.Save);
			AssertEquals(true, message.IsDeleted); //There are already EDIMesage in the test database
			AssertEquals(0, Factory.GetDatabaseCount(typeof(AsycudaBill)));
			AssertEquals(0, Factory.GetDatabaseCount(typeof(AsycudaManifestHeader)));
		}

		public void TestMasterBill()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			AssertNotNull(header.MasterBill);
		}

		public void TestContainers()
		{
			var header = (AsycudaManifestHeader)GetNewBusinessObject();
			AssertEquals(0, header.Containers.Count);
			header.Containers.AddNew();
			AssertEquals(1, header.Containers.Count);
			Factory.Save();
			var headerReloaded = new BusinessObjectFactory().Load<AsycudaManifestHeader>(header.PK);
			AssertEquals(1, headerReloaded.Containers.Count);
		}

		public void TestValidation()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			AssertType<AsycudaManifestHeaderValidation>(header.Validation);
		}

		public void TestLookups()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			AssertType<AsycudaManifestHeaderLookups>(header.Lookups);
		}

		public void TestEmptyAMA_ManifestType_COSTCOPropertiesAreCleared()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			header.FullyLoadedUnloadedDate = ZDateTime.Today;
			header.UnpackedDate = ZDateTime.Today;
			header.ExcessIndicator = ExcessIndicatorList.Codes.Excess;
			header.AMA_ManifestType = ManifestTypeList.Codes.AirCargoOutturnReport;

			AssertEquals("FullyLoadedUnloadedDate not empty", false, header.FullyLoadedUnloadedDate.IsEmpty);
			AssertEquals("UnpackedDate not empty", false, header.UnpackedDate.IsEmpty);
			AssertEquals("ExcessIndicator not empty", false, header.ExcessIndicator.IsEmpty);

			header.AMA_ManifestType = ZString.Empty;

			AssertEquals("FullyLoadedUnloadedDate empty", true, header.FullyLoadedUnloadedDate.IsEmpty);
			AssertEquals("UnpackedDate empty", true, header.UnpackedDate.IsEmpty);
			AssertEquals("ExcessIndicator empty", true, header.ExcessIndicator.IsEmpty);
		}

		public void TestEmptyGateInOutMessageType_GOVGIOPropertiesAreCleared()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			header.GateInOutDate = ZDateTime.Today;
			header.GateInOutMessageType = GateInOutMessageTypeCodeList.Codes.AirDepotGateIn;

			AssertEquals("GateInOutDate not empty", false, header.GateInOutDate.IsEmpty);

			header.GateInOutMessageType = ZString.Empty;

			AssertEquals("GateInOutDate empty", true, header.GateInOutDate.IsEmpty);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			header.AMA_JobReference = "123";
			return header;
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory) => GetNewBusinessObject();

		protected override Type ExpectedTypeOfContainer => typeof(AsycudaContainerCollection<AsycudaContainer, AsycudaManifestHeader>);
	}
}
