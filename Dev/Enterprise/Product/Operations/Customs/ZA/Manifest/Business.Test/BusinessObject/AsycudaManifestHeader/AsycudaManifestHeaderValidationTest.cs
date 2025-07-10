using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.ManifestBase;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Helper;
using Enterprise.Customs.Universal.Messaging.CUSCAR;
using Enterprise.Customs.Universal.Testing;
using Enterprise.Customs.ZA.Business;
using Enterprise.MasterFiles.Business;
using ManifestValidationRuleCodes = Enterprise.Core.Constants.Customs.Universal.RefCusCodeList.ManifestValidationRuleCodes;
using RefCusCodeListTypes = Enterprise.Core.Constants.Customs.Universal.RefCusCodeListTypes;

namespace Enterprise.Customs.ZA.Manifest.Business.Testing
{
	class AsycudaManifestHeaderValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckAMA_CarrierCode()
		{
			var carrier1 = Factory.New<ZZRefCarrierCombined>();
			carrier1.ZZ4_Code = "VWG";
			carrier1.ZZ4_CountryOrGrouping = "ZA";
			carrier1.ZZ4_Description = "Daniel";
			carrier1.Attributes.AddNew(Core.Constants.Customs.Universal.RefCarrierAttributeNames.MASTER, ZString.Empty);
			carrier1.Attributes.AddNew("SEA", "SEA");

			var carrier2 = Factory.New<ZZRefCarrierCombined>();
			carrier2.ZZ4_Code = "ICC";
			carrier2.ZZ4_CountryOrGrouping = "ZA";
			carrier2.ZZ4_Description = "I CargoCarrier";
			carrier2.Attributes.AddNew(Core.Constants.Customs.Universal.RefCarrierAttributeNames.CARGOCARRIER, ZString.Empty);
			carrier2.Attributes.AddNew("SEA", "SEA");

			Factory.Save();
			var header = Factory.New<AsycudaManifestHeader>();
			header.AMA_TransportMode = "SEA";
			header.AMA_CarrierCode = ZString.Empty;
			AssertHasMessageErrorContaining(header.AMA_CarrierCodeInfo, MandatoryValidation.YouHaveNotEntered);
			header.AMA_CarrierCode = "A123";
			AssertHasMessageError(header.AMA_CarrierCodeInfo, "Carrier Code is not in the list of known ZA carrier codes.");
			header.AMA_CarrierCode = "VWG";
			AssertNoNotifications(header.AMA_CarrierCodeInfo);

			header.AMA_ManifestType = nameof(ManifestDocumentType.COH);
			header.AMA_AgentType = Core.Constants.AgentType.CoLoad;

			header.AMA_CarrierCode = "ICC";
			AssertHasMessageError(header.AMA_CarrierCodeInfo, "Carrier Code is not in the list of known ZA carrier codes.");
		}

		public void TestCheckAMA_VesselName_AMA_RadioCallSign_Entered()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.SouthAfrica);
			helper.CreateNewOrGetExistingCusCodeType(RefCusCodeListTypes.Codes.ManifestValidationRule, "ManifestValidationRule");
			var validationRule = helper.CreateCusCodeList("ZA", RefCusCodeListTypes.Codes.ManifestValidationRule, ManifestValidationRuleCodes.ZZVESSELANDCARRIER, "Description", ZDateTime.BrettsBirthday, ZDateTime.MaxSmallDateTimeValue);
			var attributeCarrier = helper.CreateCusCodeListAttribute(validationRule.PK, ManifestValidationRuleCodes.MANDATORYZZQUALITY, ManifestValidationRuleCodes.Carrier);
			helper.CreateTransportModeForCusCodeAttribute(attributeCarrier.PK, Core.Constants.TransportModes.Sea);
			var attributeRadio = helper.CreateCusCodeListAttribute(validationRule.PK, ManifestValidationRuleCodes.MANDATORYZZQUALITY, ManifestValidationRuleCodes.RadioCallSign);
			var attributeCccInList = helper.CreateCusCodeListAttribute(validationRule.PK, ManifestValidationRuleCodes.MANDATORYZZQUALITY, ManifestValidationRuleCodes.CccInList);
			helper.CreateTransportModeForCusCodeAttribute(attributeCccInList.PK, Core.Constants.TransportModes.Sea);
			var globalVessel = Factory.New<RefVessel>();
			globalVessel.RV_Name = "HMS Liana";
			globalVessel.RV_RadioCallSign = ZString.Empty;
			var globalVessel2 = Factory.New<RefVessel>();
			globalVessel2.RV_Name = "HMS Liana2";
			globalVessel2.RV_RadioCallSign = "111";
			Factory.Save();
			CombineAssertions(() =>
			{
				var header = Factory.New<AsycudaManifestHeader>();
				header.AMA_TransportMode = Core.Constants.TransportModes.Sea;
				header.AMA_RadioCallSign = ZString.Empty;
				header.AMA_VesselName = globalVessel.RV_Name;
				AssertHasMessageError("vessel without radio call sign", header.AMA_VesselNameInfo, "The vessel has no radio call sign to default to this manifest. Please enter the radio call sign here for this manifest. (And update the vessel reference details if desired).");
				header.AMA_RadioCallSign = "111";
				header.AMA_VesselName = globalVessel2.RV_Name;
				AssertNoNotifications("vessel with radio call sign", header.AMA_VesselNameInfo);
			});
		}

		public void TestCheckAMA_OA_Carrier_AMA_CarrierCode_Entered()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.SouthAfrica);
			helper.CreateNewOrGetExistingCusCodeType(RefCusCodeListTypes.Codes.ManifestValidationRule, "ManifestValidationRule");
			var validationRule = helper.CreateCusCodeList("ZA", RefCusCodeListTypes.Codes.ManifestValidationRule, ManifestValidationRuleCodes.ZZVESSELANDCARRIER, "Description", ZDateTime.BrettsBirthday, ZDateTime.MaxSmallDateTimeValue);
			var attributeCarrier = helper.CreateCusCodeListAttribute(validationRule.PK, ManifestValidationRuleCodes.MANDATORYZZQUALITY, ManifestValidationRuleCodes.Carrier);
			helper.CreateTransportModeForCusCodeAttribute(attributeCarrier.PK, Core.Constants.TransportModes.Sea);
			var attributeRadio = helper.CreateCusCodeListAttribute(validationRule.PK, ManifestValidationRuleCodes.MANDATORYZZQUALITY, ManifestValidationRuleCodes.RadioCallSign);
			var attributeCccInList = helper.CreateCusCodeListAttribute(validationRule.PK, ManifestValidationRuleCodes.MANDATORYZZQUALITY, ManifestValidationRuleCodes.CccInList);
			helper.CreateTransportModeForCusCodeAttribute(attributeCccInList.PK, Core.Constants.TransportModes.Sea);
			Factory.Save();
			var header = Factory.New<AsycudaManifestHeader>();
			header.AMA_TransportMode = Core.Constants.TransportModes.Sea;
			header.AMA_CarrierCode = ZString.Empty;
			header.AMA_OA_Carrier = ZGuid.Empty;
			header.Validation.ValidateAMA_OA_Carrier();
			AssertHasMessageError(header.AMA_OA_CarrierInfo, "Carrier requires CCC code for ZA.");
			header.AMA_CarrierCode = "123";
			header.AMA_OA_Carrier = ZGuid.Empty;
			header.Validation.ValidateAMA_OA_Carrier();
			AssertNoMessageError(header.AMA_OA_CarrierInfo, "Carrier requires CCC code for ZA.");
		}

		public void TestCheckAMA_RadioCallSign()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			header.AMA_TransportMode = "SEA";
			header.AMA_RadioCallSign = ZString.Empty;
			AssertHasMessageErrorContaining(header.AMA_RadioCallSignInfo, MandatoryValidation.YouHaveNotEntered);
			header.AMA_TransportMode = "AIR";
			header.AMA_RadioCallSign = ZString.Empty;
			AssertNoNotifications(header.AMA_RadioCallSignInfo);
		}

		public void TestCheckMasterCarrierCode()
		{
			var carrier = Factory.New<ZZRefCarrierCombined>();
			carrier.ZZ4_Code = "VWG";
			carrier.ZZ4_CountryOrGrouping = "ZA";
			carrier.ZZ4_Description = "Daniel";
			carrier.Attributes.AddNew(Core.Constants.Customs.Universal.RefCarrierAttributeNames.MASTER, ZString.Empty);
			carrier.Attributes.AddNew("SEA", "SEA");
			var carrier2 = Factory.New<ZZRefCarrierCombined>();
			carrier2.ZZ4_Code = "00505655";
			carrier2.ZZ4_CountryOrGrouping = "ZA";
			carrier2.ZZ4_Description = "Daniel";
			carrier2.Attributes.AddNew(Core.Constants.Customs.Universal.RefCarrierAttributeNames.CARGOCARRIER, ZString.Empty);
			carrier2.Attributes.AddNew("SEA", "SEA");
			Factory.Save();

			var header = Factory.New<AsycudaManifestHeader>();
			header.AMA_TransportMode = "SEA";
			header.MasterCarrierCode = ZString.Empty;
			AssertHasMessageErrorContaining(header.MasterCarrierCodeInfo, MandatoryValidation.YouHaveNotEntered);
			header.MasterCarrierCode = "#$";
			AssertHasMessageError(header.MasterCarrierCodeInfo, "Master Carrier Code is alpha numeric only.");
			header.MasterCarrierCode = "A123";
			AssertHasMessageError(header.MasterCarrierCodeInfo, "Master Carrier Code is not in the list of known ZA carrier codes.");
			header.MasterCarrierCode = "VWG";
			AssertNoNotifications(header.MasterCarrierCodeInfo);

			header.AMA_ManifestType = nameof(ManifestDocumentType.COH);
			header.AMA_AgentType = Core.Constants.AgentType.CoLoad;
			header.MasterCarrierCode = ZString.Empty;
			AssertHasMessageErrorContaining(header.MasterCarrierCodeInfo, MandatoryValidation.YouHaveNotEntered);
			header.MasterCarrierCode = "#$";
			AssertHasMessageError(header.MasterCarrierCodeInfo, "Master Carrier Code is alpha numeric only.");
			header.MasterCarrierCode = "A123";
			AssertHasWarning(header.MasterCarrierCodeInfo, "Sub-Master Carrier Code for COH Manifest, should have 8 characters.");
			AssertHasMessageError(header.MasterCarrierCodeInfo, "Sub-Master Carrier Code is not in the list of known ZA cargo carrier codes.");
			header.MasterCarrierCode = "00505655";
			AssertNoNotifications(header.MasterCarrierCodeInfo);
		}

		public void TestCheckAMA_MasterBill()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			header.AMA_MasterBill = ZString.Empty;
			AssertHasMessageError(header.AMA_MasterBillInfo, ASYCUDA.Business.ValidationConstants.ManifestNumberIsRequired(header.MasterBillLabel.Caption));
			header.AMA_MasterBill = "MB1";
			AssertNoMessageErrors(header.AMA_MasterBillInfo);
			header.AMA_ApplicationCode = ApplicationCodeTypeList.Codes.ShippingLine;
			header.AMA_MasterBill = ZString.Empty;
			AssertNoMessageErrors(header.AMA_MasterBillInfo);
			header.AMA_TransportMode = "AIR";
			header.AMA_MasterBill = "123-12345629";
			AssertHasWarningContaining(header.AMA_MasterBillInfo, "Invalid check digit. The last digit should be '0'");
			header.AMA_MasterBill = "123-12345620";
			AssertNoWarningContaining(header.AMA_MasterBillInfo, "Invalid check digit. The last digit should be '0'");
			header.AMA_TransportMode = "SEA";
			header.AMA_MasterBill = "123-12345629";
			AssertNoWarningContaining(header.AMA_MasterBillInfo, "Invalid check digit. The last digit should be '0'");
			header.AMA_TransportMode = "AIR";
			header.Validation.ValidateAMA_MasterBill();
			AssertHasWarningContaining(header.AMA_MasterBillInfo, "Invalid check digit. The last digit should be '0'");
			header.AMA_MasterBill = ZString.Empty;
			header.AMA_ManifestType = "FWB";
			header.Validation.ValidateAMA_MasterBill();
			AssertHasMessageErrorContaining(header.AMA_MasterBillInfo, "required.");
			header.AMA_MasterBill = "123-12345620";
			AssertNoMessageErrorContaining(header.AMA_MasterBillInfo, "required.");
			AssertHasMessageErrorContaining(header.AMA_MasterBillInfo, "A Bill matching");
			header.Bills.AddNew();
			header.Bills.AddNew();
			header.Validation.ValidateAMA_MasterBill();
			AssertHasMessageErrorContaining(header.AMA_MasterBillInfo, "Only one Bill is allowed");
			var bill = header.Bills.AddNew();
			var manifestTypes = new[] { nameof(ManifestDocumentType.ALH), nameof(ManifestDocumentType.COH), nameof(ManifestDocumentType.HAB) };
			foreach (var manifestType in manifestTypes)
			{
				header.AMA_ManifestType = manifestType;
				bill.ABL_BillNumber = "12345678916";
				header.AMA_MasterBill = "12345678916";
				header.AMA_TransportMode = Core.Constants.TransportModes.Sea;
				AssertHasWarningContaining(header.AMA_MasterBillInfo, "Master Transport Document Number cannot be the same as a Bill Number when the Manifest Type is ALH, COH or HAB.");
				header.AMA_TransportMode = Core.Constants.TransportModes.Air;
				AssertHasWarningContaining(header.AMA_MasterBillInfo, "Master Transport Document Number cannot be the same as a Bill Number when the Manifest Type is ALH, COH or HAB.");
				header.AMA_MasterBill = "10987654321";
				AssertNoWarningContaining(header.AMA_MasterBillInfo, "Master Transport Document Number cannot be the same as a Bill Number when the Manifest Type is ALH, COH or HAB.");
			}
		}

		public void TestCheckAMA_OA_DeconsolidateAddress()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			var container = header.Containers.AddNew();
			container.ACN_EmptyFullIndicator = "LCL";
			foreach (var nature in new[] { ShipmentTypeList.Codes.Import23, ShipmentTypeList.Codes.Transit24, ShipmentTypeList.Codes.Transhipment28 })
			{
				header.AMA_Nature = nature;
				header.AMA_ManifestType = nameof(ManifestDocumentType.ALH);
				header.AMA_TransportMode = Core.Constants.TransportModes.Sea;
				header.AMA_ContainerMode = Core.Constants.ContainerModes.Containerised;
				header.AMA_OA_DeconsolidateAddress = ZGuid.Empty;
				AssertHasMessageError(header.AMA_OA_DeconsolidateAddressInfo, "For Import Manifests of Type 'ALH' Deconsolidation address is Mandatory.");
				header.AMA_ManifestType = nameof(ManifestDocumentType.COH);
				header.AMA_OA_DeconsolidateAddress = ZGuid.Empty;
				AssertHasMessageError(header.AMA_OA_DeconsolidateAddressInfo, "For Import Manifests of Type 'COH' Deconsolidation address is Mandatory.");
				header.AMA_ManifestType = nameof(ManifestDocumentType.HAB);
				header.AMA_OA_DeconsolidateAddress = ZGuid.Empty;
				AssertHasMessageError(header.AMA_OA_DeconsolidateAddressInfo, "For Import Manifests of Type 'HAB' Deconsolidation address is Mandatory.");
				var org = Factory.NewWithValidTestData<OrgAddress>();
				header.AMA_OA_DeconsolidateAddress = org.PK;
				AssertHasMessageError(header.AMA_OA_DeconsolidateAddressInfo, "Organisation must have a code of type 'CPD' loaded.");
				org.Header.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.CodeTypes.DepotControlledPremisesID, "VWG", Core.Constants.CountryCodes.SouthAfrica);
				header.AMA_OA_DeconsolidateAddress = org.PK;
				AssertNoMessageError(header.AMA_OA_DeconsolidateAddressInfo, "Organisation must have a code of type 'CPD' loaded.");
				header.AMA_TransportMode = Core.Constants.TransportModes.Road;
				header.AMA_OA_DeconsolidateAddress = org.PK;
				AssertNoMessageErrorContaining(header.AMA_OA_DeconsolidateAddressInfo, "For Import Manifests of Type");
				header.AMA_TransportMode = Core.Constants.TransportModes.Sea;
				header.AMA_OA_DeconsolidateAddress = ZGuid.Empty;
				AssertHasMessageErrorContaining(header.AMA_OA_DeconsolidateAddressInfo, "For Import Manifests of Type");
				header.AMA_ContainerMode = Core.Constants.ContainerModes.Bulk;
				header.AMA_OA_DeconsolidateAddress = org.PK;
				AssertNoMessageErrorContaining(header.AMA_OA_DeconsolidateAddressInfo, "For Import Manifests of Type");
				header.AMA_ContainerMode = Core.Constants.ContainerModes.Containerised;
				header.AMA_OA_DeconsolidateAddress = ZGuid.Empty;
				AssertHasMessageErrorContaining(header.AMA_OA_DeconsolidateAddressInfo, "For Import Manifests of Type");
				header.AMA_ContainerMode = Core.Constants.ContainerModes.Bulk;
				header.AMA_OA_DeconsolidateAddress = ZGuid.Empty;
				AssertHasWarningContaining(header.AMA_OA_DeconsolidateAddressInfo, "For Import Manifests of Type");
			}
		}

		public void TestCheckAMA_OA_DischargeTerminalAddress()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			foreach (var nature in new[] { ShipmentTypeList.Codes.Import23, ShipmentTypeList.Codes.Transit24, ShipmentTypeList.Codes.Transhipment28 })
			{
				header.AMA_Nature = nature;
				header.AMA_ManifestType = nameof(ManifestDocumentType.BBB);
				header.AMA_OA_DischargeTerminalAddress = ZGuid.Empty;
				AssertNoNotifications(header.AMA_OA_DischargeTerminalAddressInfo);
				header.AMA_ManifestType = nameof(ManifestDocumentType.FWB);
				header.AMA_OA_DischargeTerminalAddress = ZGuid.Empty;
				AssertNoNotifications(header.AMA_OA_DischargeTerminalAddressInfo);
				header.AMA_ManifestType = nameof(ManifestDocumentType.HAB);
				header.AMA_OA_DischargeTerminalAddress = ZGuid.Empty;
				AssertNoNotifications(header.AMA_OA_DischargeTerminalAddressInfo);
				header.AMA_ManifestType = nameof(ManifestDocumentType.COH);
				header.AMA_OA_DischargeTerminalAddress = ZGuid.Empty;
				AssertNoNotifications(header.AMA_OA_DischargeTerminalAddressInfo);
				header.AMA_ManifestType = nameof(ManifestDocumentType.ALH);
				header.AMA_OA_DischargeTerminalAddress = ZGuid.Empty;
				AssertNoNotifications(header.AMA_OA_DischargeTerminalAddressInfo);
				var org = Factory.NewWithValidTestData<OrgAddress>();
				header.AMA_OA_DischargeTerminalAddress = org.PK;
				AssertHasMessageError(header.AMA_OA_DischargeTerminalAddressInfo, "Organisation must have a code of type 'CPT' loaded.");
				org.Header.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.CodeTypes.TerminalControlledPremisesID, "VWG", Core.Constants.CountryCodes.SouthAfrica);
				header.AMA_OA_DischargeTerminalAddress = org.PK;
				AssertNoMessageError(header.AMA_OA_DischargeTerminalAddressInfo, "Organisation must have a code of type 'CPT' loaded.");
			}
		}

		public void TestTransportMode_EmptyManifestTypeListForZA()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			header.AMA_TransportMode = Core.Constants.TransportModes.FixedTransportInstallations;
			AssertHasError(header.AMA_TransportModeInfo, "This transport mode is not supported for ZA.");
			header.AMA_TransportMode = Core.Constants.TransportModes.Air;
			AssertNoError(header.AMA_TransportModeInfo, "This transport mode is not supported for ZA.");
		}

		public void TestCheckMasterBOL()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(RefCusCodeListTypes.Codes.ManifestValidationRule, "ManifestValidationRule");
			var validationRule = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.SouthAfrica, RefCusCodeListTypes.Codes.ManifestValidationRule, ManifestValidationRuleCodes.MasterBOL, "MasterBOL", ZDateTime.BrettsBirthday, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingRefCusCodeListAttributeName(ManifestValidationRuleCodes.MANDATORYFORAGENTTYPE, "Desc.", RefCusCodeListTypes.Codes.ManifestValidationRule, Core.Constants.CountryCodes.SouthAfrica);
			helper.CreateNewOrGetExistingRefCusCodeListAttributeName(ManifestValidationRuleCodes.MANDATORYFORMESSAGETYPE, "Desc.", RefCusCodeListTypes.Codes.ManifestValidationRule, Core.Constants.CountryCodes.SouthAfrica);
			validationRule.Attributes.AddNew(ManifestValidationRuleCodes.MANDATORYFORAGENTTYPE, Core.Constants.AgentType.CoLoad);
			validationRule.Attributes.AddNew(ManifestValidationRuleCodes.MANDATORYFORMESSAGETYPE, nameof(ManifestDocumentType.COH));
			validationRule.Attributes.AddNew(ManifestValidationRuleCodes.MANDATORYFORMESSAGETYPE, nameof(ManifestDocumentType.HAB));
			validationRule.Attributes.AddNew(ManifestValidationRuleCodes.MANDATORYFORMESSAGETYPE, nameof(ManifestDocumentType.ALH));
			Factory.Save();
			var header = Factory.New<AsycudaManifestHeader>();
			header.AMA_ManifestType = nameof(ManifestDocumentType.COH);
			header.AMA_AgentType = Core.Constants.AgentType.CoLoad;
			header.MasterBOL = ZString.Empty;
			AssertHasMessageError(header.MasterBOLInfo, "Parent Bill required when Agent Type is CLD and Manifest Type is COH for ZA (South Africa).");
			header.AMA_AgentType = Core.Constants.AgentType.AWBMaster;
			header.MasterBOL = ZString.Empty;
			AssertNoNotifications(header.MasterBOLInfo);
		}

		public void TestCheckAMA_OA_Carrier_ZA_AIR()
		{
			const string ZA = Core.Constants.CountryCodes.SouthAfrica;
			ASYCUDA.Business.Testing.AsycudaManifestHeaderLookupsTest.EnsureOrCreateRefZZRecordsToControlVisibilityBasedOnModeAndCountry(Factory, new ASYCUDA.Business.Testing.ModeAndCountry(Core.Constants.TransportModes.Air, ZA), new ASYCUDA.Business.Testing.ModeAndCountry(Core.Constants.TransportModes.Road, ZA));
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(RefCusCodeListTypes.Codes.ManifestValidationRule, "ManifestValidationRule");
			var validationRule = helper.CreateCusCodeList(ZA, RefCusCodeListTypes.Codes.ManifestValidationRule, ManifestValidationRuleCodes.ZZVESSELANDCARRIER, "Description", ZDateTime.BrettsBirthday, ZDateTime.MaxSmallDateTimeValue);
			var attributeCarrier = helper.CreateCusCodeListAttribute(validationRule.PK, ManifestValidationRuleCodes.MANDATORYZZQUALITY, ManifestValidationRuleCodes.Carrier);
			helper.CreateTransportModeForCusCodeAttribute(attributeCarrier.PK, Core.Constants.TransportModes.Air);
			helper.CreateTransportModeForCusCodeAttribute(attributeCarrier.PK, Core.Constants.TransportModes.Road);
			var attributeCccInList = helper.CreateCusCodeListAttribute(validationRule.PK, ManifestValidationRuleCodes.MANDATORYZZQUALITY, ManifestValidationRuleCodes.CccInList);
			helper.CreateTransportModeForCusCodeAttribute(attributeCccInList.PK, Core.Constants.TransportModes.Road);
			Factory.Save();
			var carrierCode = helper.CreateCarrierCode("123", "desc", ZA);
			helper.CreateCarrierCodeAttribute(carrierCode.PK, Core.Constants.TransportModes.Air, Core.Constants.TransportModes.Air);
			helper.CreateCarrierCodeAttribute(carrierCode.PK, Core.Constants.TransportModes.Road, Core.Constants.TransportModes.Road);
			Factory.Save();
			var header = Factory.New<AsycudaManifestHeader>();
			header.AMA_TransportMode = Core.Constants.TransportModes.Road;
			var carrierAddresPK = Factory.LoadTop1<OrgAddress>(new ZQuery()).PK;
			header.AMA_OA_Carrier = carrierAddresPK;
			AssertHasMessageError(header.AMA_OA_CarrierInfo, ASYCUDA.Business.ValidationConstants.CarrierRequiresCCC(ZA));
			var cusCode = header.Carrier.Header.CustomsCodes.AddNew(OrgCusCode.CodeTypes.CarrierCode, "ABC123", ZA);
			header.Validation.ValidateAMA_OA_Carrier();
			AssertNoMessageError(header.AMA_OA_CarrierInfo, ASYCUDA.Business.ValidationConstants.CarrierRequiresCCC(ZA));
			AssertHasMessageError(header.AMA_OA_CarrierInfo, "Carrier's CCC is not in the list of known ZA carrier codes");
			header.AMA_TransportMode = Core.Constants.TransportModes.Air;
			header.Carrier.Header.CustomsCodes.RemoveAndDelete(cusCode);
			header.Validation.ValidateAMA_OA_Carrier();
			AssertHasMessageError(header.AMA_OA_CarrierInfo, ASYCUDA.Business.ValidationConstants.CarrierRequiresCCC(ZA));
			header.Carrier.Header.CustomsCodes.AddNew(OrgCusCode.CodeTypes.CarrierCode, "ABC123", ZA);
			header.Validation.ValidateAMA_OA_Carrier();
			AssertNoMessageError(header.AMA_OA_CarrierInfo, ASYCUDA.Business.ValidationConstants.CarrierRequiresCCC(ZA));
			AssertNoMessageError(header.AMA_OA_CarrierInfo, "Carrier's CCC is not in the list of known ZA carrier codes");
		}

		public void TestTransportModeROA_ZA()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(RefCusCodeListTypes.Codes.ManifestValidationRule, "ManifestValidationRule");
			var code = helper.CreateCusCodeList("ZA", "MVAL", ManifestValidationRuleCodes.Person, "At least one person must be captured for road manifests in the Manifest > Persons tab", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateTransportModeForCusCodeList(code.PK, Core.Constants.TransportModes.Road);
			Factory.Save();
			// In ZA, in Transport Mode of "ROA", Manifest must contain at least one Person.
			var header = Factory.New<AsycudaManifestHeader>();
			header.AMA_TransportMode = Core.Constants.TransportModes.Road;
			AssertHasMessageErrorContaining(header.AMA_TransportModeInfo, "At least one person must be captured for road manifests in the Manifest > Persons tab");
			header.Persons.AddNew();
			header.Validation.ValidateAMA_TransportMode();
			AssertNoMessageErrorContaining(header.AMA_TransportModeInfo, "At least one person must be captured for road manifests in the Manifest > Persons tab");
			header = Factory.New<AsycudaManifestHeader>();
			header.AMA_TransportMode = Core.Constants.TransportModes.Air;
			AssertNoMessageErrorContaining(header.AMA_TransportModeInfo, "At least one person must be captured for road manifests in the Manifest > Persons tab");
			var usHeader = (ASYCUDA.Business.AsycudaManifestHeader)Factory.New<Integration.Customs.ASYCUDA.ACEManifest.IAsycudaManifestHeader>();
			usHeader.AMA_TransportMode = Core.Constants.TransportModes.Road;
			AssertNoMessageErrorContaining(usHeader.AMA_TransportModeInfo, "At least one person must be captured for road manifests in the Manifest > Persons tab");
		}

		public void TestContainerModeCNT_ZA()
		{
			ASYCUDA.Business.Testing.AsycudaManifestHeaderTestHelper.EnsureOrCreateManifestTypesDataInZZ(Core.Constants.CountryCodes.SouthAfrica, Factory);
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(RefCusCodeListTypes.Codes.ManifestValidationRule, "ManifestValidationRule");
			var code = helper.CreateCusCodeList("ZA", "MVAL", ManifestValidationRuleCodes.Container, "Cont. Mode of 'CNT – Containers' has been selected but no container numbers have been captured in the Manifest > Containers tab.", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateCusCodeListAttribute(code.PK, "MANDATORYFORCONTAINERMODE", "CNT");
			helper.CreateCusCodeListAttribute(code.PK, "MANDATORYFORMESSAGETYPE", nameof(ManifestDocumentType.RFM));
			Factory.Save();
			// In ZA, in container mode of "CNT", Manifest must contain at least one Container.
			var header = Factory.New<AsycudaManifestHeader>();
			header.AMA_ManifestType = nameof(ManifestDocumentType.RFM);
			header.AMA_ContainerMode = ZString.Empty;
			header.Validation.ValidateAMA_ContainerMode();
			AssertHasMessageError(header.AMA_ContainerModeInfo, "Container Mode is compulsory when Manifest Type is RFM for ZA (South Africa).");
			header.AMA_ContainerMode = "111";
			AssertNoMessageError(header.AMA_ContainerModeInfo, "Container Mode is compulsory when Manifest Type is RFM for ZA (South Africa).");
			AssertHasMessageErrorContaining(header.AMA_ContainerModeInfo, "list");
			header.AMA_ContainerMode = Core.Constants.ContainerModes.Containerised;
			AssertHasMessageErrorContaining(header.AMA_ContainerModeInfo, "Cont. Mode of 'CNT – Containers' has been selected but no container numbers have been captured in the Manifest > Containers tab.");
			header.Containers.AddNew();
			header.Validation.ValidateAMA_ContainerMode();
			AssertNoMessageErrorContaining(header.AMA_ContainerModeInfo, "Cont. Mode of 'CNT – Containers' has been selected but no container numbers have been captured in the Manifest > Containers tab.");
			var usHeader = (ASYCUDA.Business.AsycudaManifestHeader)Factory.New<Integration.Customs.ASYCUDA.ACEManifest.IAsycudaManifestHeader>();
			usHeader.AMA_ContainerMode = Core.Constants.ContainerModes.Containerised;
			AssertNoMessageErrorContaining(usHeader.AMA_ContainerModeInfo, "Cont. Mode of 'CNT – Containers' has been selected but no container numbers have been captured in the Manifest > Containers tab.");
			header = Factory.New<AsycudaManifestHeader>();
			header.AMA_ContainerMode = Core.Constants.ContainerModes.FCL;
			header.Validation.ValidateAMA_ContainerMode();
			AssertNoMessageErrorContaining(header.AMA_ContainerModeInfo, "Cont. Mode of 'CNT – Containers' has been selected but no container numbers have been captured in the Manifest > Containers tab.");
		}

		public void TestCheckAMA_AgentType()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			string[] manifestTypeArray1 = { nameof(ManifestDocumentType.ALM), nameof(ManifestDocumentType.ALH), nameof(ManifestDocumentType.BBB) };
			string[] manifestTypeArray2 = { nameof(ManifestDocumentType.COM), nameof(ManifestDocumentType.BBB) };
			string[] natureArray = { ShipmentTypeList.Codes.Import23, ShipmentTypeList.Codes.Transhipment28, ShipmentTypeList.Codes.Transit24 };
			string[] agentTypeArray = { Core.Constants.AgentType.Agent, Core.Constants.AgentType.AWBCoload, Core.Constants.AgentType.AWBMaster, Core.Constants.AgentType.Charter, Core.Constants.AgentType.CoLoad, Core.Constants.AgentType.Courier, Core.Constants.AgentType.Direct, Core.Constants.AgentType.OnBoardCourier, Core.Constants.AgentType.Other };
			header.AMA_AgentType = "";
			AssertHasMessageErrorContaining(header.AMA_AgentTypeInfo, "You have not entered an Agent Type");
			header.AMA_AgentType = Core.Constants.AgentType.CoLoad;
			AssertNoMessageErrorContaining(header.AMA_AgentTypeInfo, "You have not entered an Agent Type");
			foreach (var nature in natureArray)
			{
				header.AMA_Nature = nature;
				header.AMA_ManifestType = nameof(ManifestDocumentType.COH);
				foreach (var agent in agentTypeArray)
				{
					header.AMA_AgentType = agent;
					if (header.AMA_AgentType == Core.Constants.AgentType.CoLoad)
					{
						AssertNoWarnings(header.AMA_AgentTypeInfo);
					}
					else
					{
						AssertHasWarningContaining(header.AMA_AgentTypeInfo, "For Import, Transit & Transhipment Manifests where the Manifest Type is COH, the Agent Type must be CLD.");
					}
				}

				foreach (var manifest in manifestTypeArray1)
				{
					header.AMA_ManifestType = manifest;
					foreach (var agent in agentTypeArray)
					{
						header.AMA_AgentType = agent;
						if (header.AMA_AgentType == Core.Constants.AgentType.Agent)
						{
							AssertNoWarnings(header.AMA_AgentTypeInfo);
						}
						else
						{
							AssertHasWarningContaining(header.AMA_AgentTypeInfo, "For Import, Transit & Transhipment Manifests where the Manifest Type is ALM, ALH or BBB the Agent Type must be AGT.");
						}
					}
				}
			}

			header.AMA_Nature = ShipmentTypeList.Codes.Export22;
			foreach (var manifest in manifestTypeArray2)
			{
				header.AMA_ManifestType = manifest;
				foreach (var agent in agentTypeArray)
				{
					header.AMA_AgentType = agent;
					if (header.AMA_AgentType == Core.Constants.AgentType.Agent)
					{
						AssertNoWarnings(header.AMA_AgentTypeInfo);
					}
					else
					{
						AssertHasWarningContaining(header.AMA_AgentTypeInfo, "For Export Manifests where the Manifest Type is COM or BBB the Agent Type must be AGT.");
					}
				}
			}
		}

		public void TestCheckAMA_Nature()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			header.AMA_ApplicationCode = ApplicationCodeTypeList.Codes.ShippingLine;
			header.AMA_Nature = ShipmentTypeList.Codes.Transhipment28;
			AssertNoMessageErrors(header.AMA_NatureInfo);
			header.AMA_ManifestType = nameof(ManifestDocumentType.RFM);
			header.Validation.ValidateAMA_Nature();
			AssertHasMessageErrorContaining(header.AMA_NatureInfo, "Manifest Nature cannot be TSS – Transhipment (28) for Road Manifests.");
			header.AMA_Nature = ZString.Empty;
			AssertHasMessageErrorContaining(header.AMA_NatureInfo, MandatoryValidation.YouHaveNotEntered);
			header.AMA_ManifestType = nameof(ManifestDocumentType.ALH);
			header.Validation.ValidateAMA_Nature();
			AssertNoMessageErrors(header.AMA_NatureInfo);
			header.AMA_Nature = ShipmentTypeList.Codes.Transhipment28;
			AssertNoMessageErrors(header.AMA_NatureInfo);
			header.AMA_ManifestType = nameof(ManifestDocumentType.RFM);
			header.AMA_Nature = ShipmentTypeList.Codes.Transit24;
			AssertNoMessageErrors(header.AMA_NatureInfo);
		}

		public void TestCheckPlaceOfEntry()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);

			helper.CreateNewOrGetExistingCusCodeType(RefCusCodeListTypes.Codes.ManifestCountry, "ManifestCountry");
			helper.CreateNewOrGetExistingCusCodeList("ZZ", RefCusCodeListTypes.Codes.ManifestCountry, "ZA", "South Africa", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);

			helper.CreateNewOrGetExistingCusCodeType(RefCusCodeListTypes.Codes.CustomsOffice, "CustomsOffice");
			var za1 = helper.CreateNewOrGetExistingCusCodeList("ZA", RefCusCodeListTypes.Codes.CustomsOffice, "JHB", "JOHANNESBURG", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);

			Factory.Save();

			var header = Factory.New<AsycudaManifestHeader>();
			header.AMA_TransportMode = Core.Constants.TransportModes.Road;
			header.AMA_Nature = ShipmentTypeList.Codes.Transhipment28;
			header.PlaceOfEntry = "";
			AssertHasMessageError(header.PlaceOfEntryInfo, ValidationConstants.PlaceOfEntryIsCompulsoryForRoadTranshipmentManifest);
			header.PlaceOfEntry = "ZZZ";
			AssertHasMessageError(header.PlaceOfEntryInfo, ListValidation.InvalidCodeMessageError);
			header.PlaceOfEntry = "JHB";
			AssertNoMessageError(header.PlaceOfEntryInfo, ListValidation.InvalidCodeMessageError);
		}

		public void TestCheckEstimatedTimeOfLoading()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			header.AMA_ManifestType = nameof(ManifestDocumentType.ALH);
			header.EstimatedTimeOfLoading = ZDateTime.Empty;
			AssertHasMessageErrorContaining(header.EstimatedTimeOfLoadingInfo, MandatoryValidation.YouHaveNotEntered);
			header.EstimatedTimeOfLoading = new ZDateTime(2017, 11, 2);
			AssertNoNotifications(header.EstimatedTimeOfLoadingInfo);
		}

		public void TestCheckPlaceOfExit()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);

			helper.CreateNewOrGetExistingCusCodeType(RefCusCodeListTypes.Codes.ManifestCountry, "ManifestCountry");
			helper.CreateNewOrGetExistingCusCodeList("ZZ", RefCusCodeListTypes.Codes.ManifestCountry, "ZA", "South Africa", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);

			helper.CreateNewOrGetExistingCusCodeType(RefCusCodeListTypes.Codes.CustomsOffice, "CustomsOffice");
			var za1 = helper.CreateNewOrGetExistingCusCodeList("ZA", RefCusCodeListTypes.Codes.CustomsOffice, "JHB", "JOHANNESBURG", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);

			Factory.Save();

			var header = Factory.New<AsycudaManifestHeader>();
			header.PlaceOfExit = "";
			AssertNoNotifications(header.PlaceOfExitInfo);
			header.PlaceOfExit = "ZZZ";
			AssertHasMessageError(header.PlaceOfExitInfo, ListValidation.InvalidCodeMessageError);
			header.PlaceOfExit = "JHB";
			AssertNoMessageError(header.PlaceOfExitInfo, ListValidation.InvalidCodeMessageError);
		}

		public void TestTSS_Vessel()
		{
			var vessel = Factory.NewWithValidTestData<RefVessel>();
			vessel.RV_Code = "UTVES";
			Factory.Save();

			var header = SetupHeader();

			header.TSS_Vessel = "";
			AssertHasMessageErrorContaining(header.TSS_VesselInfo, MandatoryValidation.YouHaveNotEntered);
			header.TSS_Vessel = "XXX-MD2";
			AssertHasMessageError(header.TSS_VesselInfo, ListValidation.InvalidCodeMessageError);
			header.TSS_Vessel = vessel.RV_Code;
			AssertNoNotifications(header.TSS_VesselInfo);
		}

		public void TestTSS_VoyageFlight()
		{
			var header = SetupHeader();

			header.TSS_VoyageFlight = "";
			AssertHasMessageErrorContaining(header.TSS_VoyageFlightInfo, MandatoryValidation.YouHaveNotEntered);
			header.TSS_VoyageFlight = "ABC123";
			AssertNoNotifications(header.TSS_VoyageFlightInfo);
		}

		public void TestTSS_RadioCallSign()
		{
			var header = SetupHeader();

			header.TSS_RadioCallSign = "";
			AssertHasMessageErrorContaining(header.TSS_RadioCallSignInfo, MandatoryValidation.YouHaveNotEntered);
			header.TSS_RadioCallSign = "MAYDAY";
			AssertNoNotifications(header.TSS_RadioCallSignInfo);
		}

		public void TestTSS_CargoCarrierPK()
		{
			var nonCarrierOrg = Factory.New<OrgHeader>();
			nonCarrierOrg.OH_Code = "NONCAR";
			var otherCarrierOrg = Factory.New<OrgHeader>();
			otherCarrierOrg.OH_Code = "OTHCAR";
			otherCarrierOrg.OH_IsShippingProvider = true;
			var airCarrierOrg = Factory.New<OrgHeader>();
			airCarrierOrg.OH_Code = "AIRCRR";
			airCarrierOrg.OH_IsShippingProvider = true;
			airCarrierOrg.OH_IsAirLine = true;
			var seaCarrierOrg = Factory.New<OrgHeader>();
			seaCarrierOrg.OH_Code = "SEACAR";
			seaCarrierOrg.OH_IsShippingProvider = true;
			seaCarrierOrg.OH_IsShippingLine = true;
			var seaCarrierOrg2 = Factory.New<OrgHeader>();
			seaCarrierOrg2.OH_Code = "SEACCC";
			seaCarrierOrg2.OH_IsShippingProvider = true;
			seaCarrierOrg2.OH_IsShippingLine = true;
			var ccc = seaCarrierOrg2.CustomsCodes.AddNew();
			ccc.OK_RN_NKCodeCountry = "ZA";
			ccc.OK_CodeType = "CCC";
			ccc.OK_CustomsRegNo = "12345678";

			Factory.Save();

			var header = SetupHeader();

			header.TSS_CargoCarrierPK = ZGuid.Empty;
			AssertHasMessageErrorContaining(header.TSS_CargoCarrierPKInfo, MandatoryValidation.YouHaveNotEntered);
			header.TSS_CargoCarrierPK = ZGuid.NewZGuid();
			AssertHasErrorContaining(header.TSS_CargoCarrierPKInfo, AsycudaManifestHeaderValidation.InvalidTranshipmentCarrier);
			header.TSS_CargoCarrierPK = nonCarrierOrg.PK;
			AssertHasError(header.TSS_CargoCarrierPKInfo, AsycudaManifestHeaderValidation.InvalidTranshipmentCarrier);
			header.TSS_CargoCarrierPK = otherCarrierOrg.PK;
			AssertHasError(header.TSS_CargoCarrierPKInfo, AsycudaManifestHeaderValidation.InvalidTranshipmentCarrier);
			header.TSS_CargoCarrierPK = airCarrierOrg.PK;
			AssertHasError(header.TSS_CargoCarrierPKInfo, AsycudaManifestHeaderValidation.InvalidTranshipmentCarrier);
			header.TSS_CargoCarrierPK = seaCarrierOrg.PK;
			AssertHasMessageError(header.TSS_CargoCarrierPKInfo, AsycudaManifestHeaderValidation.TranshipmentCarrierMissingCCC);
			header.TSS_CargoCarrierPK = seaCarrierOrg2.PK;
			AssertNoMessageError(header.TSS_CargoCarrierPKInfo, AsycudaManifestHeaderValidation.TranshipmentCarrierMissingCCC);
		}

		public void TestTSS_DateOfDeparture()
		{
			var header = SetupHeader();

			header.TSS_DateOfDeparture = ZDateTime.Empty;
			AssertHasMessageErrorContaining(header.TSS_DateOfDepartureInfo, MandatoryValidation.YouHaveNotEntered);
			header.TSS_DateOfDeparture = new ZDateTime(2019, 11, 12);
			AssertNoNotifications(header.TSS_DateOfDepartureInfo);
		}

		public void TestCallPurposeCode()
		{
			var header = SetupHeader();
			header.CallPurposeCode = CallPurposeCodeList.Codes.UnloadingCargo;
			AssertNoNotifications(header.CallPurposeCodeInfo);

			header.CallPurposeCode = ZString.Empty;
			AssertHasMessageErrorContaining(header.CallPurposeCodeInfo, MandatoryValidation.YouHaveNotEntered);

			header.CallPurposeCode = "ABC";
			AssertHasMessageErrorContaining(header.CallPurposeCodeInfo, ListValidation.InvalidCodeMessageError);
		}

		AsycudaManifestHeader SetupHeader()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			header.AMA_Nature = ShipmentTypeList.Codes.Transhipment28;
			header.AMA_TransportMode = "SEA";

			return header;
		}
	}
}
