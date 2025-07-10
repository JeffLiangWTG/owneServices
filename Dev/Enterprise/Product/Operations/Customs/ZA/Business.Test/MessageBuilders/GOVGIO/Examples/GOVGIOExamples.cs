using CargoWise.EntityFramework;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.ZA.Business.MessageBuilders.Testing
{
	public static class GOVGIOExamples
	{
		public static AsycudaManifestHeader CreateAirDepotGateIn(BusinessObjectFactory factory)
		{
			var helper = new UniversalReferenceTestDataHelper(factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Facilities, "desc.");
			var outturnProvider = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.SouthAfrica, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Facilities, "06", "Dsc.", ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(2));
			helper.CreateNewOrGetExistingRefCusCodeListAttributeName(Core.Constants.Customs.Universal.RefCusCodeList.Attributes.OutturnFacilityCode, "Desc.", Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Facilities, Core.Constants.CountryCodes.SouthAfrica);
			outturnProvider.Attributes.AddNew(Core.Constants.Customs.Universal.RefCusCodeList.Attributes.OutturnFacilityCode, "00000001");
			factory.Save();
			CreateUNLOCO(factory, "GBAVO", "VOE");
			CreateUNLOCO(factory, "GBVWG", "VWG");
			var carrier = factory.New<OrgHeader>();
			carrier.OH_Code = "CAR001";
			carrier.CustomsCodes.AddNew(OrgCusCode.CodeTypes.CarrierCode, "CAR-CCC-001", Core.Constants.CountryCodes.SouthAfrica);
			var deconsolidator = factory.New<OrgHeader>();
			deconsolidator.OH_Code = "DC0001";
			deconsolidator.MainAddress.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.CodeTypes.DepotControlledPremisesID, "123", Core.Constants.CountryCodes.SouthAfrica);
			var terminal = factory.New<OrgHeader>();
			terminal.OH_Code = "TR0001";
			terminal.MainAddress.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.CodeTypes.TerminalControlledPremisesID, "456", Core.Constants.CountryCodes.SouthAfrica);
			var header = factory.New<AsycudaManifestHeader>();
			header.AMA_JobReference = "OGM0001";
			header.AMA_Nature = NatureList.Codes.Import23;
			header.AMA_TransportMode = Core.Constants.TransportModes.Air;
			header.GateInOutMessageType = GateInOutMessageTypeCodeList.Codes.AirDepotGateIn;
			header.MasterBill.ABL_RL_NKPortOfDischarge = "GBVWG";
			header.MasterBill.ABL_RL_NKPortOfLoading = "GBAVO";
			header.OutturnProvider = "06";
			header.AMA_Voyage = "ABCD123456";
			header.MasterBill.ABL_E_ARV = new ZDateTime(2018, 3, 15, 11, 30, 0);
			header.MasterBill.ABL_E_DEP = new ZDateTime(2018, 3, 25, 17, 30, 0);
			header.GateInOutDate = new ZDateTime(2018, 3, 24, 14, 55, 0);
			header.AMA_OA_Carrier = carrier.MainAddress.PK;
			header.AMA_OA_DeconsolidateAddress = deconsolidator.MainAddress.PK;
			header.AMA_OA_DischargeTerminalAddress = terminal.MainAddress.PK;
			header.AMA_ContainerMode = Core.Constants.ContainerModes.BreakBulk;
			header.MasterBill.ABL_BillNumber = "MB0123";
			var bill1 = header.Bills.AddNew();
			bill1.ABL_BillNumber = "HB001";
			bill1.ABL_BillIssuer = "BI001";
			bill1.MRN = "MRN1";
			var pack11 = bill1.Packs.AddNew();
			pack11.APA_PackQty = 10;
			var pack12 = bill1.Packs.AddNew();
			pack12.APA_PackQty = 1;
			var pack13 = bill1.Packs.AddNew();
			pack13.APA_PackQty = 2;
			var bill2 = header.Bills.AddNew();
			bill2.ABL_BillNumber = "HB002";
			bill2.ABL_BillIssuer = "BI002";
			bill2.MRN = "MRN2";
			var pack21 = bill2.Packs.AddNew();
			pack21.APA_PackQty = 4;
			var pack22 = bill2.Packs.AddNew();
			pack22.APA_PackQty = 8;
			return header;
		}

		public static AsycudaManifestHeader CreateSeaDepotGateIn(BusinessObjectFactory factory)
		{
			var helper = new UniversalReferenceTestDataHelper(factory);
			var cusof = helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "Customs Office");
			var abc = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.SouthAfrica, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "ABC", "ABC Desc.", ZDateTime.Today.AddDays(-1), ZDateTime.Today.AddDays(1));
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Facilities, "desc.");
			var outturnProvider = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.SouthAfrica, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Facilities, "06", "Dsc.", ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(2));
			helper.CreateNewOrGetExistingRefCusCodeListAttributeName(Core.Constants.Customs.Universal.RefCusCodeList.Attributes.OutturnFacilityCode, "Desc.", Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Facilities, Core.Constants.CountryCodes.SouthAfrica);
			outturnProvider.Attributes.AddNew(Core.Constants.Customs.Universal.RefCusCodeList.Attributes.OutturnFacilityCode, "00000001");
			factory.Save();
			var bi001 = helper.CreateCarrierCode("BI001", "desc.", Core.Constants.CountryCodes.SouthAfrica);
			helper.CreateCarrierCodeAttribute(bi001.PK, Core.Constants.Customs.Universal.RefCarrierAttributeNames.CARGOCARRIER, "CARGOCARRIER");
			helper.CreateCarrierCodeAttribute(bi001.PK, "SEA", "SEA");
			var bi002 = helper.CreateCarrierCode("BI002", "desc.", Core.Constants.CountryCodes.SouthAfrica);
			helper.CreateCarrierCodeAttribute(bi002.PK, Core.Constants.Customs.Universal.RefCarrierAttributeNames.CARGOCARRIER, "CARGOCARRIER");
			helper.CreateCarrierCodeAttribute(bi002.PK, "SEA", "SEA");
			factory.Save();
			CreateUNLOCO(factory, "GBAVO", "VOE");
			CreateUNLOCO(factory, "GBVWG", "VWG");
			var vessel = factory.New<RefVessel>();
			vessel.RV_Code = "VES001";
			vessel.RV_CarrierCode = "WXYZ";
			vessel.RV_RadioCallSign = "ATYU13";
			var carrier = factory.New<OrgHeader>();
			carrier.OH_Code = "CAR001";
			carrier.CustomsCodes.AddNew(OrgCusCode.CodeTypes.CarrierCode, "CAR-CCC-001", Core.Constants.CountryCodes.SouthAfrica);
			var deconsolidator = factory.New<OrgHeader>();
			deconsolidator.OH_Code = "DC0001";
			deconsolidator.MainAddress.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.CodeTypes.DepotControlledPremisesID, "123", Core.Constants.CountryCodes.SouthAfrica);
			var terminal = factory.New<OrgHeader>();
			terminal.OH_Code = "TR0001";
			terminal.MainAddress.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.CodeTypes.TerminalControlledPremisesID, "456", Core.Constants.CountryCodes.SouthAfrica);
			var header = factory.New<AsycudaManifestHeader>();
			header.AMA_Nature = NatureList.Codes.Import23;
			header.AMA_JobReference = "OGM0001";
			header.AMA_TransportMode = Core.Constants.TransportModes.Sea;
			header.AMA_ContainerMode = Core.Constants.ContainerModes.Containerised;
			header.GateInOutMessageType = GateInOutMessageTypeCodeList.Codes.DepotGateIn;
			header.MasterBill.ABL_RL_NKPortOfDischarge = "GBVWG";
			var outturnProviderList = header.Lookups.OutturnProviderList;
			outturnProviderList.Load();
			header.OutturnProvider = "06";
			header.AMA_Voyage = "ABCD123456";
			header.AMA_VesselName = vessel.RV_Code;
			header.AMA_OA_Carrier = carrier.MainAddress.PK;
			header.AMA_OA_DeconsolidateAddress = deconsolidator.MainAddress.PK;
			header.AMA_CustomsOffice = "ABC";
			header.MasterBill.CustomsCPC = "A0001";
			header.MasterBill.ABL_RL_NKPortOfLoading = "GBAVO";
			var cont1 = header.Containers.AddNew();
			cont1.ACN_ContainerNumber = "CNT-001";
			cont1.ACN_EmptyFullIndicator = EmptyFullList.Codes.EmptyContainer;
			cont1.ACN_Seal1 = "SEAL1";
			cont1.ACN_SealingPartyType = "CAR";
			cont1.GateInOutDate = new ZDateTime(2018, 3, 21, 12, 1, 0);
			cont1.ACN_RC_ContainerType = factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20HC").PK;
			var cont2 = header.Containers.AddNew();
			cont2.ACN_ContainerNumber = "CNT-002";
			cont2.ACN_EmptyFullIndicator = EmptyFullList.Codes.FullContainerLoad;
			cont2.ACN_Seal1 = "SEAL2";
			cont2.ACN_SealingPartyType = "CUS";
			cont2.GateInOutDate = new ZDateTime(2018, 3, 22, 12, 2, 0);
			cont2.ACN_RC_ContainerType = factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "40HC").PK;
			var cont3 = header.Containers.AddNew();
			cont3.ACN_ContainerNumber = "CNT-003";
			cont3.ACN_EmptyFullIndicator = EmptyFullList.Codes.LessThanFullContainerLoad;
			cont3.ACN_Seal1 = "SEAL3";
			cont3.GateInOutDate = new ZDateTime(2018, 3, 23, 12, 3, 0);
			cont3.ACN_RC_ContainerType = factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "40GP").PK;
			var bill1 = header.Bills.AddNew();
			bill1.ABL_BillNumber = "HB001";
			bill1.ABL_BillIssuer = "BI001";
			bill1.MRN = "MRN1";
			var pack11 = bill1.Packs.AddNew();
			pack11.APA_PackQty = 10;
			pack11.Outturn.C5_CargoType = CargoTypeList.Codes.LiquidBreak;
			var pack12 = bill1.Packs.AddNew();
			pack12.APA_PackQty = 50;
			pack12.Outturn.C5_CargoType = CargoTypeList.Codes.LiquidBreak;
			var pack13 = bill1.Packs.AddNew();
			pack13.APA_PackQty = 100;
			pack13.Outturn.C5_CargoType = CargoTypeList.Codes.LiquidBreak;
			var bill2 = header.Bills.AddNew();
			bill2.ABL_BillNumber = "HB002";
			bill2.ABL_BillIssuer = "BI002";
			bill2.MRN = "MRN2";
			var pack21 = bill2.Packs.AddNew();
			pack21.APA_PackQty = 20;
			pack21.Outturn.C5_CargoType = CargoTypeList.Codes.LiquidBreak;
			var pack22 = bill2.Packs.AddNew();
			pack22.APA_PackQty = 60;
			pack22.Outturn.C5_CargoType = CargoTypeList.Codes.LiquidBreak;
			return header;
		}

		public static AsycudaManifestHeader CreateSeaDepotGateOut(BusinessObjectFactory factory)
		{
			var helper = new UniversalReferenceTestDataHelper(factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Facilities, "desc.");
			var outturnProvider = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.SouthAfrica, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Facilities, "06", "Dsc.", ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(2));
			helper.CreateNewOrGetExistingRefCusCodeListAttributeName(Core.Constants.Customs.Universal.RefCusCodeList.Attributes.OutturnFacilityCode, "Desc.", Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Facilities, Core.Constants.CountryCodes.SouthAfrica);
			outturnProvider.Attributes.AddNew(Core.Constants.Customs.Universal.RefCusCodeList.Attributes.OutturnFacilityCode, "00000001");
			factory.Save();
			CreateUNLOCO(factory, "GBAVO", "VOE");
			CreateUNLOCO(factory, "GBVWG", "VWG");
			var vessel = factory.New<RefVessel>();
			vessel.RV_Code = "VES001";
			vessel.RV_CarrierCode = "WXYZ";
			vessel.RV_RadioCallSign = "ATYU13";
			var carrier = factory.New<OrgHeader>();
			carrier.OH_Code = "CAR001";
			carrier.CustomsCodes.AddNew(OrgCusCode.CodeTypes.CarrierCode, "CAR-CCC-001", Core.Constants.CountryCodes.SouthAfrica);
			var deconsolidator = factory.New<OrgHeader>();
			deconsolidator.OH_Code = "DC0001";
			deconsolidator.MainAddress.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.CodeTypes.DepotControlledPremisesID, "123", Core.Constants.CountryCodes.SouthAfrica);
			var terminal = factory.New<OrgHeader>();
			terminal.OH_Code = "TR0001";
			terminal.MainAddress.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.CodeTypes.TerminalControlledPremisesID, "456", Core.Constants.CountryCodes.SouthAfrica);
			var header = factory.New<AsycudaManifestHeader>();
			header.AMA_JobReference = "OGM0001";
			header.AMA_Nature = NatureList.Codes.Import23;
			header.AMA_TransportMode = Core.Constants.TransportModes.Sea;
			header.AMA_ContainerMode = Core.Constants.ContainerModes.Containerised;
			header.GateInOutMessageType = GateInOutMessageTypeCodeList.Codes.DepotGateOut;
			header.MasterBill.ABL_RL_NKPortOfDischarge = "GBVWG";
			header.MasterBill.ABL_RL_NKPortOfLoading = "GBAVO";
			header.OutturnProvider = "06";
			header.AMA_Voyage = "ABCD123456";
			header.AMA_VesselName = vessel.RV_Code;
			header.MasterBill.ABL_E_ARV = new ZDateTime(2018, 3, 15, 11, 30, 0);
			header.MasterBill.ABL_E_DEP = new ZDateTime(2018, 3, 25, 17, 30, 0);
			header.AMA_OA_Carrier = carrier.MainAddress.PK;
			header.AMA_OA_DeconsolidateAddress = deconsolidator.MainAddress.PK;
			header.AMA_OA_DischargeTerminalAddress = terminal.MainAddress.PK;
			header.MasterBill.ABL_BillNumber = "MB0123";
			var cont1 = header.Containers.AddNew();
			cont1.ACN_ContainerNumber = "CNT-001";
			cont1.ACN_EmptyFullIndicator = EmptyFullList.Codes.EmptyContainer;
			cont1.ACN_Seal1 = "SEAL1";
			cont1.ACN_SealingPartyType = "CAR";
			cont1.GateInOutDate = new ZDateTime(2018, 3, 21, 12, 1, 0);
			cont1.ACN_RC_ContainerType = factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20HC").PK;
			var cont2 = header.Containers.AddNew();
			cont2.ACN_ContainerNumber = "CNT-002";
			cont2.ACN_EmptyFullIndicator = EmptyFullList.Codes.FullContainerLoad;
			cont2.ACN_Seal1 = "SEAL2";
			cont2.ACN_SealingPartyType = "CUS";
			cont2.GateInOutDate = new ZDateTime(2018, 3, 22, 12, 2, 0);
			cont2.ACN_RC_ContainerType = factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "40HC").PK;
			var cont3 = header.Containers.AddNew();
			cont3.ACN_ContainerNumber = "CNT-003";
			cont3.ACN_EmptyFullIndicator = EmptyFullList.Codes.LessThanFullContainerLoad;
			cont3.GateInOutDate = new ZDateTime(2018, 3, 23, 12, 3, 0);
			cont3.ACN_RC_ContainerType = factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "40GP").PK;
			var bill1 = header.Bills.AddNew();
			bill1.ABL_BillNumber = "HB001";
			bill1.ABL_BillIssuer = "BI001";
			bill1.MRN = "MRN1";
			var pack11 = bill1.Packs.AddNew();
			pack11.ContainerPK = cont1.PK;
			pack11.APA_PackQty = 10;
			var pack12 = bill1.Packs.AddNew();
			pack12.ContainerPK = cont1.PK;
			pack12.APA_PackQty = 1;
			var pack13 = bill1.Packs.AddNew();
			pack13.ContainerPK = cont2.PK;
			pack13.APA_PackQty = 2;
			var bill2 = header.Bills.AddNew();
			bill2.ABL_BillNumber = "HB002";
			bill2.ABL_BillIssuer = "BI002";
			bill2.MRN = "MRN2";
			var pack21 = bill2.Packs.AddNew();
			pack21.ContainerPK = cont2.PK;
			pack21.APA_PackQty = 4;
			var pack22 = bill2.Packs.AddNew();
			pack22.ContainerPK = cont3.PK;
			pack22.APA_PackQty = 8;
			return header;
		}

		public static AsycudaManifestHeader CreateSeaDepotConsignmentGateIn(BusinessObjectFactory factory)
		{
			var helper = new UniversalReferenceTestDataHelper(factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Facilities, "desc.");
			var outturnProvider = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.SouthAfrica, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Facilities, "06", "Dsc.", ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(2));
			helper.CreateNewOrGetExistingRefCusCodeListAttributeName(Core.Constants.Customs.Universal.RefCusCodeList.Attributes.OutturnFacilityCode, "Desc.", Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Facilities, Core.Constants.CountryCodes.SouthAfrica);
			outturnProvider.Attributes.AddNew(Core.Constants.Customs.Universal.RefCusCodeList.Attributes.OutturnFacilityCode, "00000001");
			factory.Save();
			CreateUNLOCO(factory, "GBAVO", "VOE");
			CreateUNLOCO(factory, "GBVWG", "VWG");
			var vessel = factory.New<RefVessel>();
			vessel.RV_Code = "VES001";
			vessel.RV_CarrierCode = "WXYZ";
			vessel.RV_RadioCallSign = "ATYU13";
			var carrier = factory.New<OrgHeader>();
			carrier.OH_Code = "CAR001";
			carrier.CustomsCodes.AddNew(OrgCusCode.CodeTypes.CarrierCode, "CAR-CCC-001", Core.Constants.CountryCodes.SouthAfrica);
			var deconsolidator = factory.New<OrgHeader>();
			deconsolidator.OH_Code = "DC0001";
			deconsolidator.MainAddress.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.CodeTypes.DepotControlledPremisesID, "123", Core.Constants.CountryCodes.SouthAfrica);
			var terminal = factory.New<OrgHeader>();
			terminal.OH_Code = "TR0001";
			terminal.MainAddress.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.CodeTypes.TerminalControlledPremisesID, "456", Core.Constants.CountryCodes.SouthAfrica);
			var header = factory.New<AsycudaManifestHeader>();
			header.AMA_JobReference = "OGM0001";
			header.AMA_Nature = NatureList.Codes.Import23;
			header.AMA_TransportMode = Core.Constants.TransportModes.Sea;
			header.AMA_ContainerMode = Core.Constants.ContainerModes.Containerised;
			header.GateInOutMessageType = GateInOutMessageTypeCodeList.Codes.SeaDepotConsignmentGateIn;
			header.MasterBill.ABL_RL_NKPortOfDischarge = "GBVWG";
			header.MasterBill.ABL_RL_NKPortOfLoading = "GBAVO";
			header.OutturnProvider = "06";
			header.AMA_Voyage = "ABCD123456";
			header.AMA_VesselName = vessel.RV_Code;
			header.MasterBill.ABL_E_ARV = new ZDateTime(2018, 3, 15, 11, 30, 0);
			header.MasterBill.ABL_E_DEP = new ZDateTime(2018, 3, 25, 17, 30, 0);
			header.AMA_OA_Carrier = carrier.MainAddress.PK;
			header.AMA_OA_DeconsolidateAddress = deconsolidator.MainAddress.PK;
			header.AMA_OA_DischargeTerminalAddress = terminal.MainAddress.PK;
			header.MasterBill.ABL_BillNumber = "MB0123";
			var cont1 = header.Containers.AddNew();
			cont1.ACN_ContainerNumber = "CNT-001";
			cont1.ACN_EmptyFullIndicator = EmptyFullList.Codes.EmptyContainer;
			cont1.ACN_Seal1 = "SEAL1";
			cont1.ACN_SealingPartyType = "CAR";
			cont1.GateInOutDate = new ZDateTime(2018, 3, 21, 12, 1, 0);
			cont1.ACN_RC_ContainerType = factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20HC").PK;
			var cont2 = header.Containers.AddNew();
			cont2.ACN_ContainerNumber = "CNT-002";
			cont2.ACN_EmptyFullIndicator = EmptyFullList.Codes.FullContainerLoad;
			cont2.ACN_Seal1 = "SEAL2";
			cont2.ACN_SealingPartyType = "CUS";
			cont2.GateInOutDate = new ZDateTime(2018, 3, 22, 12, 2, 0);
			cont2.ACN_RC_ContainerType = factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "40HC").PK;
			var cont3 = header.Containers.AddNew();
			cont3.ACN_ContainerNumber = "CNT-003";
			cont3.ACN_EmptyFullIndicator = EmptyFullList.Codes.LessThanFullContainerLoad;
			cont3.GateInOutDate = new ZDateTime(2018, 3, 23, 12, 3, 0);
			cont3.ACN_RC_ContainerType = factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "40GP").PK;
			var bill1 = header.Bills.AddNew();
			bill1.ABL_BillNumber = "HB001";
			bill1.ABL_BillIssuer = "BI001";
			bill1.MRN = "MRN1";
			var pack11 = bill1.Packs.AddNew();
			pack11.ContainerPK = cont1.PK;
			var pack12 = bill1.Packs.AddNew();
			pack12.ContainerPK = cont1.PK;
			var pack13 = bill1.Packs.AddNew();
			pack13.ContainerPK = cont2.PK;
			var bill2 = header.Bills.AddNew();
			bill2.ABL_BillNumber = "HB002";
			bill2.ABL_BillIssuer = "BI002";
			bill2.MRN = "MRN2";
			var pack21 = bill2.Packs.AddNew();
			pack21.ContainerPK = cont2.PK;
			var pack22 = bill2.Packs.AddNew();
			pack22.ContainerPK = cont3.PK;
			return header;
		}

		public static AsycudaManifestHeader CreateAirTerminalGateIn(BusinessObjectFactory factory)
		{
			var helper = new UniversalReferenceTestDataHelper(factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Facilities, "desc.");
			var outturnProvider = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.SouthAfrica, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Facilities, "06", "Dsc.", ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(2));
			helper.CreateNewOrGetExistingRefCusCodeListAttributeName(Core.Constants.Customs.Universal.RefCusCodeList.Attributes.OutturnFacilityCode, "Desc.", Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Facilities, Core.Constants.CountryCodes.SouthAfrica);
			outturnProvider.Attributes.AddNew(Core.Constants.Customs.Universal.RefCusCodeList.Attributes.OutturnFacilityCode, "00000001");
			factory.Save();
			CreateUNLOCO(factory, "GBAVO", "VOE");
			CreateUNLOCO(factory, "GBVWG", "VWG");
			var carrier = factory.New<OrgHeader>();
			carrier.OH_Code = "CAR001";
			carrier.CustomsCodes.AddNew(OrgCusCode.CodeTypes.CarrierCode, "CAR-CCC-001", Core.Constants.CountryCodes.SouthAfrica);
			var deconsolidator = factory.New<OrgHeader>();
			deconsolidator.OH_Code = "DC0001";
			deconsolidator.MainAddress.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.CodeTypes.DepotControlledPremisesID, "123", Core.Constants.CountryCodes.SouthAfrica);
			var terminal = factory.New<OrgHeader>();
			terminal.OH_Code = "TR0001";
			terminal.MainAddress.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.CodeTypes.TerminalControlledPremisesID, "456", Core.Constants.CountryCodes.SouthAfrica);
			var header = factory.New<AsycudaManifestHeader>();
			header.AMA_JobReference = "OGM0001";
			header.AMA_Nature = NatureList.Codes.Import23;
			header.AMA_TransportMode = Core.Constants.TransportModes.Air;
			header.GateInOutMessageType = GateInOutMessageTypeCodeList.Codes.AirTerminalGateIn;
			header.MasterBill.ABL_RL_NKPortOfDischarge = "GBVWG";
			header.MasterBill.ABL_RL_NKPortOfLoading = "GBAVO";
			header.OutturnProvider = "06";
			header.AMA_Voyage = "ABCD123456";
			header.MasterBill.ABL_E_ARV = new ZDateTime(2018, 3, 15, 11, 30, 0);
			header.MasterBill.ABL_E_DEP = new ZDateTime(2018, 3, 25, 17, 30, 0);
			header.GateInOutDate = new ZDateTime(2018, 3, 24, 14, 55, 0);
			header.AMA_OA_Carrier = carrier.MainAddress.PK;
			header.AMA_OA_DeconsolidateAddress = deconsolidator.MainAddress.PK;
			header.AMA_OA_DischargeTerminalAddress = terminal.MainAddress.PK;
			header.AMA_ContainerMode = Core.Constants.ContainerModes.BreakBulk;
			header.MasterBill.ABL_BillNumber = "MB0123";
			var bill1 = header.Bills.AddNew();
			bill1.ABL_BillNumber = "HB001";
			bill1.ABL_BillIssuer = "BI001";
			bill1.MRN = "MRN1";
			var pack11 = bill1.Packs.AddNew();
			pack11.APA_PackQty = 10;
			var pack12 = bill1.Packs.AddNew();
			pack12.APA_PackQty = 1;
			var pack13 = bill1.Packs.AddNew();
			pack13.APA_PackQty = 2;
			var bill2 = header.Bills.AddNew();
			bill2.ABL_BillNumber = "HB002";
			bill2.ABL_BillIssuer = "BI002";
			bill2.MRN = "MRN2";
			var pack21 = bill2.Packs.AddNew();
			pack21.APA_PackQty = 4;
			var pack22 = bill2.Packs.AddNew();
			pack22.APA_PackQty = 8;
			return header;
		}

		public static AsycudaManifestHeader CreateSeaBreakBulkGateIn(BusinessObjectFactory factory)
		{
			var helper = new UniversalReferenceTestDataHelper(factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Facilities, "desc.");
			var outturnProvider = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.SouthAfrica, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Facilities, "06", "Dsc.", ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(2));
			helper.CreateNewOrGetExistingRefCusCodeListAttributeName(Core.Constants.Customs.Universal.RefCusCodeList.Attributes.OutturnFacilityCode, "Desc.", Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Facilities, Core.Constants.CountryCodes.SouthAfrica);
			outturnProvider.Attributes.AddNew(Core.Constants.Customs.Universal.RefCusCodeList.Attributes.OutturnFacilityCode, "00000001");
			factory.Save();
			CreateUNLOCO(factory, "GBAVO", "VOE");
			CreateUNLOCO(factory, "GBVWG", "VWG");
			var vessel = factory.New<RefVessel>();
			vessel.RV_Code = "VES001";
			vessel.RV_CarrierCode = "WXYZ";
			vessel.RV_RadioCallSign = "ATYU13";
			var carrier = factory.New<OrgHeader>();
			carrier.OH_Code = "CAR001";
			carrier.CustomsCodes.AddNew(OrgCusCode.CodeTypes.CarrierCode, "CAR-CCC-001", Core.Constants.CountryCodes.SouthAfrica);
			var deconsolidator = factory.New<OrgHeader>();
			deconsolidator.OH_Code = "DC0001";
			deconsolidator.MainAddress.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.CodeTypes.DepotControlledPremisesID, "123", Core.Constants.CountryCodes.SouthAfrica);
			var terminal = factory.New<OrgHeader>();
			terminal.OH_Code = "TR0001";
			terminal.MainAddress.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.CodeTypes.TerminalControlledPremisesID, "456", Core.Constants.CountryCodes.SouthAfrica);
			var header = factory.New<AsycudaManifestHeader>();
			header.AMA_JobReference = "OGM0001";
			header.AMA_Nature = NatureList.Codes.Import23;
			header.AMA_TransportMode = Core.Constants.TransportModes.Air;
			header.GateInOutMessageType = GateInOutMessageTypeCodeList.Codes.BreakBulkGateIn;
			header.MasterBill.ABL_RL_NKPortOfDischarge = "GBVWG";
			header.MasterBill.ABL_RL_NKPortOfLoading = "GBAVO";
			header.OutturnProvider = "06";
			header.AMA_Voyage = "ABCD123456";
			header.AMA_VesselName = vessel.RV_Code;
			header.MasterBill.ABL_E_ARV = new ZDateTime(2018, 3, 15, 11, 30, 0);
			header.MasterBill.ABL_E_DEP = new ZDateTime(2018, 3, 25, 17, 30, 0);
			header.GateInOutDate = new ZDateTime(2018, 3, 24, 14, 55, 0);
			header.AMA_OA_Carrier = carrier.MainAddress.PK;
			header.AMA_OA_DeconsolidateAddress = deconsolidator.MainAddress.PK;
			header.AMA_OA_DischargeTerminalAddress = terminal.MainAddress.PK;
			header.AMA_ContainerMode = Core.Constants.ContainerModes.BreakBulk;
			header.MasterBill.ABL_BillNumber = "MB0123";
			var bill1 = header.Bills.AddNew();
			bill1.ABL_BillNumber = "HB001";
			bill1.ABL_BillIssuer = "BI001";
			bill1.MRN = "MRN1";
			var pack11 = bill1.Packs.AddNew();
			pack11.APA_PackQty = 10;
			var pack12 = bill1.Packs.AddNew();
			pack12.APA_PackQty = 1;
			var pack13 = bill1.Packs.AddNew();
			pack13.APA_PackQty = 2;
			var bill2 = header.Bills.AddNew();
			bill2.ABL_BillNumber = "HB002";
			bill2.ABL_BillIssuer = "BI002";
			bill2.MRN = "MRN2";
			var pack21 = bill2.Packs.AddNew();
			pack21.APA_PackQty = 4;
			var pack22 = bill2.Packs.AddNew();
			pack22.APA_PackQty = 8;
			return header;
		}

		public static AsycudaManifestHeader CreateSeaTerminalGateIn(BusinessObjectFactory factory)
		{
			var helper = new UniversalReferenceTestDataHelper(factory);
			var cusof = helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "Customs Office");
			var abc = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.SouthAfrica, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "ABC", "ABC Desc.", ZDateTime.Today.AddDays(-1), ZDateTime.Today.AddDays(1));
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Facilities, "desc.");
			var outturnProvider = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.SouthAfrica, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Facilities, "06", "Dsc.", ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(2));
			helper.CreateNewOrGetExistingRefCusCodeListAttributeName(Core.Constants.Customs.Universal.RefCusCodeList.Attributes.OutturnFacilityCode, "Desc.", Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Facilities, Core.Constants.CountryCodes.SouthAfrica);
			outturnProvider.Attributes.AddNew(Core.Constants.Customs.Universal.RefCusCodeList.Attributes.OutturnFacilityCode, "00000001");
			factory.Save();
			var bi001 = helper.CreateCarrierCode("BI001", "desc.", Core.Constants.CountryCodes.SouthAfrica);
			helper.CreateCarrierCodeAttribute(bi001.PK, Core.Constants.Customs.Universal.RefCarrierAttributeNames.CARGOCARRIER, "CARGOCARRIER");
			helper.CreateCarrierCodeAttribute(bi001.PK, "SEA", "SEA");
			var bi002 = helper.CreateCarrierCode("BI002", "desc.", Core.Constants.CountryCodes.SouthAfrica);
			helper.CreateCarrierCodeAttribute(bi002.PK, Core.Constants.Customs.Universal.RefCarrierAttributeNames.CARGOCARRIER, "CARGOCARRIER");
			helper.CreateCarrierCodeAttribute(bi002.PK, "SEA", "SEA");
			factory.Save();
			CreateUNLOCO(factory, "GBAVO", "VOE");
			CreateUNLOCO(factory, "GBVWG", "VWG");
			var vessel = factory.New<RefVessel>();
			vessel.RV_Code = "VES001";
			vessel.RV_CarrierCode = "WXYZ";
			vessel.RV_RadioCallSign = "ATYU13";
			var carrier = factory.New<OrgHeader>();
			carrier.OH_Code = "CAR001";
			carrier.CustomsCodes.AddNew(OrgCusCode.CodeTypes.CarrierCode, "CAR-CCC-001", Core.Constants.CountryCodes.SouthAfrica);
			var deconsolidator = factory.New<OrgHeader>();
			deconsolidator.OH_Code = "DC0001";
			deconsolidator.MainAddress.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.CodeTypes.DepotControlledPremisesID, "123", Core.Constants.CountryCodes.SouthAfrica);
			var terminal = factory.New<OrgHeader>();
			terminal.OH_Code = "TR0001";
			terminal.MainAddress.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.CodeTypes.TerminalControlledPremisesID, "456", Core.Constants.CountryCodes.SouthAfrica);
			var header = factory.New<AsycudaManifestHeader>();
			header.AMA_JobReference = "OGM0001";
			header.AMA_Nature = NatureList.Codes.Import23;
			header.AMA_TransportMode = Core.Constants.TransportModes.Sea;
			header.AMA_ContainerMode = Core.Constants.ContainerModes.Containerised;
			header.GateInOutMessageType = GateInOutMessageTypeCodeList.Codes.TerminalGateIn;
			header.MasterBill.ABL_RL_NKPortOfDischarge = "GBVWG";
			header.MasterBill.ABL_RL_NKPortOfLoading = "GBAVO";
			var outturnProviderList = header.Lookups.OutturnProviderList;
			outturnProviderList.Load();
			header.OutturnProvider = "06";
			header.AMA_Voyage = "ABCD123456";
			header.AMA_VesselName = vessel.RV_Code;
			header.MasterBill.ABL_E_ARV = new ZDateTime(2018, 3, 25, 1, 30, 0);
			header.MasterBill.ABL_E_DEP = new ZDateTime(2018, 3, 15, 17, 30, 0);
			header.AMA_OA_Carrier = carrier.MainAddress.PK;
			header.AMA_OA_DeconsolidateAddress = deconsolidator.MainAddress.PK;
			header.AMA_OA_DischargeTerminalAddress = terminal.MainAddress.PK;
			header.FullyLoadedUnloadedDate = new ZDateTime(2018, 3, 17, 14, 10, 0);
			header.AMA_CustomsOffice = "ABC";
			header.MasterBill.ABL_BillNumber = "MB0123";
			header.MasterBill.ABL_CarrierReference = "B00069";
			header.MasterBill.CustomsCPC = "A0001";
			var cont1 = header.Containers.AddNew();
			cont1.ACN_ContainerNumber = "CNT-001";
			cont1.ACN_EmptyFullIndicator = EmptyFullList.Codes.EmptyContainer;
			cont1.ACN_Seal1 = "SEAL1";
			cont1.ACN_SealingPartyType = "CAR";
			cont1.GateInOutDate = new ZDateTime(2018, 3, 21, 12, 1, 0);
			cont1.ACN_RC_ContainerType = factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20HC").PK;
			var cont2 = header.Containers.AddNew();
			cont2.ACN_ContainerNumber = "CNT-002";
			cont2.ACN_EmptyFullIndicator = EmptyFullList.Codes.FullContainerLoad;
			cont2.ACN_Seal1 = "SEAL2";
			cont2.ACN_SealingPartyType = "CUS";
			cont2.GateInOutDate = new ZDateTime(2018, 3, 22, 12, 2, 0);
			cont2.ACN_RC_ContainerType = factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "40HC").PK;
			var cont3 = header.Containers.AddNew();
			cont3.ACN_ContainerNumber = "CNT-003";
			cont3.ACN_EmptyFullIndicator = EmptyFullList.Codes.LessThanFullContainerLoad;
			cont3.GateInOutDate = new ZDateTime(2018, 3, 23, 12, 3, 0);
			cont3.ACN_RC_ContainerType = factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "40GP").PK;
			var bill1 = header.Bills.AddNew();
			bill1.ABL_BillNumber = "HB001";
			bill1.CustomsCPC = "A0001";
			bill1.ABL_BillIssuer = "BI001";
			bill1.MRN = "MRN1";
			var pack11 = bill1.Packs.AddNew();
			pack11.ContainerPK = cont1.PK;
			pack11.APA_MarksAndNumbers = "NA";
			var pack12 = bill1.Packs.AddNew();
			pack12.ContainerPK = cont1.PK;
			pack12.APA_MarksAndNumbers = "NA";
			var pack13 = bill1.Packs.AddNew();
			pack13.ContainerPK = cont2.PK;
			pack13.APA_MarksAndNumbers = "NA";
			var bill2 = header.Bills.AddNew();
			bill2.ABL_BillNumber = "HB002";
			bill2.CustomsCPC = "A0002";
			bill2.ABL_BillIssuer = "BI002";
			bill2.MRN = "MRN2";
			var pack21 = bill2.Packs.AddNew();
			pack21.ContainerPK = cont2.PK;
			pack21.APA_MarksAndNumbers = "NA";
			var pack22 = bill2.Packs.AddNew();
			pack22.ContainerPK = cont3.PK;
			pack22.APA_MarksAndNumbers = "NA";
			return header;
		}

		public static AsycudaManifestHeader CreateSeaTerminalGateOut(BusinessObjectFactory factory)
		{
			var helper = new UniversalReferenceTestDataHelper(factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Facilities, "desc.");
			var outturnProvider = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.SouthAfrica, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Facilities, "06", "Dsc.", ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(2));
			helper.CreateNewOrGetExistingRefCusCodeListAttributeName(Core.Constants.Customs.Universal.RefCusCodeList.Attributes.OutturnFacilityCode, "Desc.", Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Facilities, Core.Constants.CountryCodes.SouthAfrica);
			outturnProvider.Attributes.AddNew(Core.Constants.Customs.Universal.RefCusCodeList.Attributes.OutturnFacilityCode, "00000001");
			factory.Save();
			CreateUNLOCO(factory, "GBAVO", "VOE");
			CreateUNLOCO(factory, "GBVWG", "VWG");
			var vessel = factory.New<RefVessel>();
			vessel.RV_Code = "VES001";
			vessel.RV_CarrierCode = "WXYZ";
			vessel.RV_RadioCallSign = "ATYU13";
			var carrier = factory.New<OrgHeader>();
			carrier.OH_Code = "CAR001";
			carrier.CustomsCodes.AddNew(OrgCusCode.CodeTypes.CarrierCode, "CAR-CCC-001", Core.Constants.CountryCodes.SouthAfrica);
			var deconsolidator = factory.New<OrgHeader>();
			deconsolidator.OH_Code = "DC0001";
			deconsolidator.MainAddress.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.CodeTypes.DepotControlledPremisesID, "123", Core.Constants.CountryCodes.SouthAfrica);
			var terminal = factory.New<OrgHeader>();
			terminal.OH_Code = "TR0001";
			terminal.MainAddress.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.CodeTypes.TerminalControlledPremisesID, "456", Core.Constants.CountryCodes.SouthAfrica);
			var header = factory.New<AsycudaManifestHeader>();
			header.AMA_JobReference = "OGM0001";
			header.AMA_Nature = NatureList.Codes.Import23;
			header.AMA_TransportMode = Core.Constants.TransportModes.Sea;
			header.AMA_ContainerMode = Core.Constants.ContainerModes.Containerised;
			header.GateInOutMessageType = GateInOutMessageTypeCodeList.Codes.TerminalGateOut;
			header.MasterBill.ABL_RL_NKPortOfDischarge = "GBVWG";
			header.MasterBill.ABL_RL_NKPortOfLoading = "GBAVO";
			header.OutturnProvider = "06";
			header.AMA_Voyage = "ABCD123456";
			header.AMA_VesselName = vessel.RV_Code;
			header.MasterBill.ABL_E_ARV = new ZDateTime(2018, 3, 15, 11, 30, 0);
			header.MasterBill.ABL_E_DEP = new ZDateTime(2018, 3, 25, 17, 30, 0);
			header.AMA_OA_Carrier = carrier.MainAddress.PK;
			header.AMA_OA_DeconsolidateAddress = deconsolidator.MainAddress.PK;
			header.AMA_OA_DischargeTerminalAddress = terminal.MainAddress.PK;
			header.MasterBill.ABL_BillNumber = "MB0123";
			header.MasterBill.ABL_CarrierReference = "B00069";
			var cont1 = header.Containers.AddNew();
			cont1.ACN_ContainerNumber = "CNT-001";
			cont1.ACN_EmptyFullIndicator = EmptyFullList.Codes.EmptyContainer;
			cont1.ACN_Seal1 = "SEAL1";
			cont1.ACN_SealingPartyType = "CAR";
			cont1.GateInOutDate = new ZDateTime(2018, 3, 21, 12, 1, 0);
			cont1.ACN_RC_ContainerType = factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20HC").PK;
			var cont2 = header.Containers.AddNew();
			cont2.ACN_ContainerNumber = "CNT-002";
			cont2.ACN_EmptyFullIndicator = EmptyFullList.Codes.FullContainerLoad;
			cont2.ACN_Seal1 = "SEAL2";
			cont2.ACN_SealingPartyType = "CUS";
			cont2.GateInOutDate = new ZDateTime(2018, 3, 22, 12, 2, 0);
			cont2.ACN_RC_ContainerType = factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "40HC").PK;
			var cont3 = header.Containers.AddNew();
			cont3.ACN_ContainerNumber = "CNT-003";
			cont3.ACN_EmptyFullIndicator = EmptyFullList.Codes.LessThanFullContainerLoad;
			cont3.GateInOutDate = new ZDateTime(2018, 3, 23, 12, 3, 0);
			cont3.ACN_RC_ContainerType = factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "40GP").PK;
			var bill1 = header.Bills.AddNew();
			bill1.ABL_BillNumber = "HB001";
			bill1.ABL_BillIssuer = "BI001";
			bill1.MRN = "MRN1";
			var pack11 = bill1.Packs.AddNew();
			pack11.ContainerPK = cont1.PK;
			pack11.APA_PackQty = 10;
			var pack12 = bill1.Packs.AddNew();
			pack12.ContainerPK = cont1.PK;
			pack12.APA_PackQty = 1;
			var pack13 = bill1.Packs.AddNew();
			pack13.ContainerPK = cont2.PK;
			pack13.APA_PackQty = 2;
			var bill2 = header.Bills.AddNew();
			bill2.ABL_BillNumber = "HB002";
			bill2.ABL_BillIssuer = "BI002";
			bill2.MRN = "MRN2";
			var pack21 = bill2.Packs.AddNew();
			pack21.ContainerPK = cont2.PK;
			pack21.APA_PackQty = 4;
			var pack22 = bill2.Packs.AddNew();
			pack22.ContainerPK = cont3.PK;
			pack22.APA_PackQty = 8;
			return header;
		}

		public static string GetExpectedAirDepotGateInText()
		{
			return GetTextFromResource("AirDepotGateIn.txt");
		}

		public static string GetExpectedSeaDepotGateInText()
		{
			return GetTextFromResource("SeaDepotGateIn.txt");
		}

		public static string GetExpectedSeaDepotGateOutText()
		{
			return GetTextFromResource("SeaDepotGateOut.txt");
		}

		public static string GetExpectedSeaDepotConsignmentGateInText()
		{
			return GetTextFromResource("SeaDepotConsignmentGateIn.txt");
		}

		public static string GetExpectedAirTerminalGateInText()
		{
			return GetTextFromResource("AirTerminalGateIn.txt");
		}

		public static string GetExpectedSeaBreakBulkGateInText()
		{
			return GetTextFromResource("SeaBreakBulkGateIn.txt");
		}

		public static string GetExpectedSeaTerminalGateInText()
		{
			return GetTextFromResource("SeaTerminalGateIn.txt");
		}

		public static string GetExpectedSeaTerminalGateOutText()
		{
			return GetTextFromResource("SeaTerminalGateOut.txt");
		}

		static void CreateUNLOCO(BusinessObjectFactory factory, ZString unloco, ZString iata)
		{
			var refUnloco = factory.LoadFromNaturalKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, unloco);
			if (refUnloco == null)
			{
				refUnloco = factory.New<RefUNLOCO>();
				refUnloco.RL_Code = unloco;
			}

			refUnloco.RL_IATA = iata;
		}

		static string GetTextFromResource(string filename)
		{
			var retiever = new EmbeddedResourceRetriever();
			return retiever.GetString("Enterprise.Customs.ZA.Business.Testing.MessageBuilders.GOVGIO.Examples." + filename);
		}
	}
}
