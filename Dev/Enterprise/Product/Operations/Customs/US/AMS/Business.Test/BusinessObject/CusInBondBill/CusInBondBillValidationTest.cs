using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common.US.AMS;
using Enterprise.Customs.Universal.Testing;
using Enterprise.Customs.US.AMS.Messaging.Business;
using Enterprise.Customs.US.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.US.AMS.Business.Testing
{
	[TestedType(typeof(CusInBondBillValidation))]
	class CusInBondBillValidationTest : CommonCusInBondBillValidationTest
	{
		public void TestCheckB0_ForeignPortOfUnladingKCode()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Port, "PORT");
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Port, "12!23", "Test", ZDateTime.BrettsBirthday, ZDateTime.MaxSmallDateTime);
			Factory.Save();

			foreach (var code in new[] { BillOfLadingStatusIndicatorList.Codes.SimpleRegularBillFROBAndISF, BillOfLadingStatusIndicatorList.Codes.HouseFROBAndISF, BillOfLadingStatusIndicatorList.Codes.HouseBillAndISFWhereMasterBOLIs62_63InBond })
			{
				bill.B0_BillStatus = ZString.Empty;
				bill.ValidationModes = ValidationModes.InventoryRecord;
				bill.B0_ForeignPortOfUnladingKCode = ZString.Empty;
				AssertNoMessageErrorContaining(bill.B0_ForeignPortOfUnladingKCodeInfo, MandatoryValidation.YouHaveNotEntered);
				AssertNoMessageErrorContaining(bill.B0_ForeignPortOfUnladingKCodeInfo, ListValidation.InvalidCodeMessageError);
				bill.B0_ForeignPortOfUnladingKCode = "!@";
				AssertNoMessageErrorContaining(bill.B0_ForeignPortOfUnladingKCodeInfo, MandatoryValidation.YouHaveNotEntered);
				AssertNoMessageErrorContaining(bill.B0_ForeignPortOfUnladingKCodeInfo, ListValidation.InvalidCodeMessageError);
				bill.B0_BillStatus = code;
				AssertNoMessageErrorContaining(bill.B0_ForeignPortOfUnladingKCodeInfo, MandatoryValidation.YouHaveNotEntered);
				AssertHasMessageErrorContaining(bill.B0_ForeignPortOfUnladingKCodeInfo, ListValidation.InvalidCodeMessageError);
				bill.B0_ForeignPortOfUnladingKCode = ZString.Empty;
				AssertHasMessageErrorContaining(bill.B0_ForeignPortOfUnladingKCodeInfo, MandatoryValidation.YouHaveNotEntered);
				AssertNoMessageErrorContaining(bill.B0_ForeignPortOfUnladingKCodeInfo, ListValidation.InvalidCodeMessageError);
				bill.B0_ForeignPortOfUnladingKCode = "12!23";
				AssertNoMessageErrorContaining(bill.B0_ForeignPortOfUnladingKCodeInfo, MandatoryValidation.YouHaveNotEntered);
				AssertNoMessageErrorContaining(bill.B0_ForeignPortOfUnladingKCodeInfo, ListValidation.InvalidCodeMessageError);
				bill.ValidationModes = ValidationModes.PermitToTransfer;
				bill.B0_ForeignPortOfUnladingKCode = ZString.Empty;
				AssertNoMessageErrorContaining(bill.B0_ForeignPortOfUnladingKCodeInfo, MandatoryValidation.YouHaveNotEntered);
				AssertNoMessageErrorContaining(bill.B0_ForeignPortOfUnladingKCodeInfo, ListValidation.InvalidCodeMessageError);
				bill.B0_ForeignPortOfUnladingKCode = "!@";
				AssertNoMessageErrorContaining(bill.B0_ForeignPortOfUnladingKCodeInfo, MandatoryValidation.YouHaveNotEntered);
				AssertNoMessageErrorContaining(bill.B0_ForeignPortOfUnladingKCodeInfo, ListValidation.InvalidCodeMessageError);
			}
		}

		public void TestCheckB0_PlaceOfDelivery()
		{
			var port = Factory.New<RefUNLOCO>();
			port.RL_Code = "12!23";
			foreach (var code in new[] { BillOfLadingStatusIndicatorList.Codes.SimpleRegularBillFROBAndISF, BillOfLadingStatusIndicatorList.Codes.HouseFROBAndISF, BillOfLadingStatusIndicatorList.Codes.HouseBillAndISFWhereMasterBOLIs62_63InBond, BillOfLadingStatusIndicatorList.Codes.SimpleRegularInBondType62_63WithISF })
			{
				bill.B0_BillStatus = ZString.Empty;
				bill.ValidationModes = ValidationModes.InventoryRecord;
				bill.B0_PlaceOfDelivery = ZString.Empty;
				AssertNoMessageErrorContaining(bill.B0_PlaceOfDeliveryInfo, MandatoryValidation.YouHaveNotEntered);
				AssertNoMessageErrorContaining(bill.B0_PlaceOfDeliveryInfo, ListValidation.InvalidCodeMessageError);
				bill.B0_PlaceOfDelivery = "!@";
				AssertNoMessageErrorContaining(bill.B0_PlaceOfDeliveryInfo, MandatoryValidation.YouHaveNotEntered);
				AssertNoMessageErrorContaining(bill.B0_PlaceOfDeliveryInfo, ListValidation.InvalidCodeMessageError);
				bill.B0_BillStatus = code;
				AssertNoMessageErrorContaining(bill.B0_PlaceOfDeliveryInfo, MandatoryValidation.YouHaveNotEntered);
				AssertHasMessageErrorContaining(bill.B0_PlaceOfDeliveryInfo, ListValidation.InvalidCodeMessageError);
				bill.B0_PlaceOfDelivery = ZString.Empty;
				AssertHasMessageErrorContaining(bill.B0_PlaceOfDeliveryInfo, MandatoryValidation.YouHaveNotEntered);
				AssertNoMessageErrorContaining(bill.B0_PlaceOfDeliveryInfo, ListValidation.InvalidCodeMessageError);
				bill.B0_PlaceOfDelivery = "12!23";
				AssertNoMessageErrorContaining(bill.B0_PlaceOfDeliveryInfo, MandatoryValidation.YouHaveNotEntered);
				AssertNoMessageErrorContaining(bill.B0_PlaceOfDeliveryInfo, ListValidation.InvalidCodeMessageError);
				bill.ValidationModes = ValidationModes.PermitToTransfer;
				bill.B0_PlaceOfDelivery = ZString.Empty;
				AssertNoMessageErrorContaining(bill.B0_PlaceOfDeliveryInfo, MandatoryValidation.YouHaveNotEntered);
				AssertNoMessageErrorContaining(bill.B0_PlaceOfDeliveryInfo, ListValidation.InvalidCodeMessageError);
				bill.B0_PlaceOfDelivery = "!@";
				AssertNoMessageErrorContaining(bill.B0_PlaceOfDeliveryInfo, MandatoryValidation.YouHaveNotEntered);
				AssertNoMessageErrorContaining(bill.B0_PlaceOfDeliveryInfo, ListValidation.InvalidCodeMessageError);
			}
		}

		public void TestValidateOceanBillOfLadingIsEnteredForNVOCC()
		{
			bill.ValidationModes = ValidationModes.InventoryRecord;
			var billRef = bill.ShipmentReferenceDetails.AddNew(BillReferenceList.Codes.BN, "BN1232");
			bill.B0_BillStatus = BillOfLadingStatusIndicatorList.Codes.Sec321HouseBill;
			AssertNoRowMessageError(bill, ValidationConstants.Header.OceanBillOfLadingIsRequiredForNVOCC);
			bill.B0_BillStatus = BillOfLadingStatusIndicatorList.Codes.HouseBill;
			bill.Validation.ValidateAll();
			AssertHasRowMessageError(bill, ValidationConstants.Header.OceanBillOfLadingIsRequiredForNVOCC);
			billRef.BR_Qualifier = BillReferenceList.Codes.OB;
			bill.Validation.ValidateAll();
			AssertNoRowMessageError(bill, ValidationConstants.Header.OceanBillOfLadingIsRequiredForNVOCC);
			bill.B0_BillStatus = BillOfLadingStatusIndicatorList.Codes.HouseBill;
			bill.Validation.ValidateAll();
			AssertNoRowMessageError(bill, ValidationConstants.Header.OceanBillOfLadingIsRequiredForNVOCC);
			billRef.BR_Qualifier = BillReferenceList.Codes.CN;
			bill.Validation.ValidateAll();
			AssertHasRowMessageError(bill, ValidationConstants.Header.OceanBillOfLadingIsRequiredForNVOCC);
			bill.ValidationModes = ValidationModes.None;
			bill.Validation.ValidateAll();
			AssertNoRowMessageError(bill, ValidationConstants.Header.OceanBillOfLadingIsRequiredForNVOCC);
			header.BH_TransitDirection = DirectionTypeList.Codes.NVOCC;
			bill.ValidationModes = ValidationModes.InventoryRecord;
			bill.B0_BillStatus = BillOfLadingStatusIndicatorList.Codes.HouseBill;
			bill.Validation.ValidateAll();
			AssertNoRowMessageError(bill, ValidationConstants.Header.OceanBillOfLadingIsRequiredForNVOCC);
		}

		void AddCarrierCodeToCurrentCompany(BusinessObjectFactory factory, ZString carrierCode)
		{
			var currentCompany = GlbCompany.GetCurrentCompany(factory);
			var existingCarrierCode = currentCompany.OrgProxy.CustomsCodes.GetOrgCusCodeObjectForCodeAndCountry(OrgCusCode.CodeTypes.CarrierCode, Core.Constants.CountryCodes.UnitedStates);
			if (existingCarrierCode != null)
			{
				currentCompany.OrgProxy.CustomsCodes.RemoveAndDelete(existingCarrierCode);
			}

			var us = factory.Load<RefCountry>(Core.Constants.CountryGuids.UnitedStates);
			currentCompany.OrgProxy.CustomsCodes.AddNew(OrgCusCode.CodeTypes.CarrierCode, carrierCode, us);
			factory.Save();
		}

		public void TestCheckB0_IssuerCodeForNVOCCHeader()
		{
			var carrier = Factory.New<USCarrierCombined>();
			carrier.UI_Code = "SC1Z";
			AddCarrierCodeToCurrentCompany(Factory, carrier.UI_Code);
			var header = Factory.New<CusInBondHeader>();
			header.BH_CarrierSCAC = GlbBranch.CurrentBranch.GB_Code;
			CombineAssertions(() =>
			{
				var bill = header.Bills.AddNew();
				header.BH_TransitDirection = DirectionTypeList.Codes.NVOCC;
				header.BH_CarrierSCAC = "SC1Z";
				bill.B0_IssuerCode = "Z!Z";
				AssertHasWarning("be error with Z!Z", bill.B0_IssuerCodeInfo, ValidationConstants.Bill.ShouldBeIdenticalWithSCACCode.ToString());
				bill.B0_IssuerCode = "SC1Z";
				AssertNoWarning("no error with SC1Z", bill.B0_IssuerCodeInfo, ValidationConstants.Bill.ShouldBeIdenticalWithSCACCode.ToString());
				header.BH_TransitDirection = DirectionTypeList.Codes.MVOCC;
				bill.B0_IssuerCode = "Z!Z";
				AssertNoWarning("no error with Z!Z", bill.B0_IssuerCodeInfo, ValidationConstants.Bill.ShouldBeIdenticalWithSCACCode.ToString());
				bill.B0_IssuerCode = "SC1Z";
				AssertNoWarning("be not error with Z!Z", bill.B0_IssuerCodeInfo, ValidationConstants.Bill.ShouldBeIdenticalWithSCACCode.ToString());
			});
		}

		public void TestValidateMaximumNumberOfSecondaryNotifyParty()
		{
			bill.ValidationModes = ValidationModes.InventoryRecord;
			bill.Validation.ValidateAll();
			AssertHasRowMessageError(bill, ValidationConstants.SecondaryNotifyParty.FirstSNPShouldBeCarrierSCACForNVOCC);
			AssertNoRowMessageError(bill, ValidationConstants.SecondaryNotifyParty.MaximumNumberOfSecondaryNotifyPartyExceeded);
			for (var i = 1; i < 9; i++)
			{
				var snp = bill.SecondaryNotifyParties.AddNew();
				snp.CY_Data = "OTT" + i.ToString();
				bill.Validation.ValidateAll();
				AssertNoRowMessageError(bill, ValidationConstants.SecondaryNotifyParty.FirstSNPShouldBeCarrierSCACForNVOCC);
				AssertNoRowMessageError(bill, ValidationConstants.SecondaryNotifyParty.MaximumNumberOfSecondaryNotifyPartyExceeded);
			}

			var snp9 = bill.SecondaryNotifyParties.AddNew();
			bill.Validation.ValidateAll();
			AssertNoRowMessageError(bill, ValidationConstants.SecondaryNotifyParty.MaximumNumberOfSecondaryNotifyPartyExceeded);
			AssertNoRowMessageError(bill, ValidationConstants.SecondaryNotifyParty.FirstSNPShouldBeCarrierSCACForNVOCC);
			snp9.CY_Data = "OTT" + 9.ToString();
			bill.Validation.ValidateAll();
			AssertNoRowMessageError(bill, ValidationConstants.SecondaryNotifyParty.FirstSNPShouldBeCarrierSCACForNVOCC);
			AssertHasRowMessageError(bill, ValidationConstants.SecondaryNotifyParty.MaximumNumberOfSecondaryNotifyPartyExceeded);
			bill.ValidationModes = ValidationModes.None;
			bill.Validation.ValidateAll();
			AssertNoRowMessageError(bill, ValidationConstants.SecondaryNotifyParty.FirstSNPShouldBeCarrierSCACForNVOCC);
			AssertNoRowMessageError(bill, ValidationConstants.SecondaryNotifyParty.MaximumNumberOfSecondaryNotifyPartyExceeded);
			bill.ValidationModes = ValidationModes.InventoryRecord;
			bill.Validation.ValidateAll();
			AssertNoRowMessageError(bill, ValidationConstants.SecondaryNotifyParty.FirstSNPShouldBeCarrierSCACForNVOCC);
			AssertHasRowMessageError(bill, ValidationConstants.SecondaryNotifyParty.MaximumNumberOfSecondaryNotifyPartyExceeded);
			snp9.Delete();
			bill.Validation.ValidateAll();
			AssertNoRowMessageError(bill, ValidationConstants.SecondaryNotifyParty.FirstSNPShouldBeCarrierSCACForNVOCC);
			AssertNoRowMessageError(bill, ValidationConstants.SecondaryNotifyParty.MaximumNumberOfSecondaryNotifyPartyExceeded);
		}

		public void TestCheckB0_RL_NKForeignPortOfContract()
		{
			bill.ValidationModes = ValidationModes.InventoryRecord;
			bill.B0_RL_NKForeignPortOfContract = ZString.Empty;
			AssertNoMessageErrors(bill.B0_RL_NKForeignPortOfContractInfo);
			bill.B0_RL_NKForeignPortOfContract = "AU!!@";
			AssertNoMessageErrors(bill.B0_RL_NKForeignPortOfContractInfo);
			AssertHasWarningContaining(bill.B0_RL_NKForeignPortOfContractInfo, ListValidation.InvalidCodeMessage);
			bill.B0_RL_NKForeignPortOfContract = "AUSYD";
			AssertNoMessageErrors(bill.B0_RL_NKForeignPortOfContractInfo);
			AssertNoWarningContaining(bill.B0_RL_NKForeignPortOfContractInfo, ListValidation.InvalidCodeMessage);
			bill.ValidationModes = ValidationModes.None;
			bill.B0_RL_NKForeignPortOfContract = "AU!!@";
			AssertNoMessageErrors(bill.B0_RL_NKForeignPortOfContractInfo);
			AssertNoWarningContaining(bill.B0_RL_NKForeignPortOfContractInfo, ListValidation.InvalidCodeMessage);
		}

		public void TestValidateTotalManifestQtyIsEqualSumOfPieceCount()
		{
			bill.ValidationModes = ValidationModes.InventoryRecord;
			var container1 = moveDetail.Containers.AddNew();
			var commodity1 = container1.Commodities.AddNew();
			commodity1.BY_PieceCount = 40;
			var commodity2 = container1.Commodities.AddNew();
			commodity2.BY_PieceCount = 40;
			var container2 = moveDetail.Containers.AddNew();
			var commodity3 = container2.Commodities.AddNew();
			commodity3.BY_PieceCount = 21;
			bill.B0_ManifestQty = 100;
			AssertHasMessageError(bill.B0_ManifestQtyInfo, ValidationConstants.Bill.TotalManifestQtyNotEqualSumOfPieceCount.ToString());
			bill.B0_ManifestQty = 101;
			AssertNoMessageError(bill.B0_ManifestQtyInfo, ValidationConstants.Bill.TotalManifestQtyNotEqualSumOfPieceCount.ToString());
		}

		public void TestCheckB0_ForeignPortOfContractKCode()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Port, "PORT");
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Port, "Z1Z32", "Test", ZDateTime.BrettsBirthday, ZDateTime.MaxSmallDateTime);
			Factory.Save();

			bill.ValidationModes = ValidationModes.InventoryRecord;
			bill.B0_ForeignPortOfContractKCode = ZString.Empty;
			AssertNoMessageErrors(bill.B0_ForeignPortOfContractKCodeInfo);
			bill.B0_ForeignPortOfContractKCode = "Z!";
			AssertHasMessageErrorContaining(bill.B0_ForeignPortOfContractKCodeInfo, ListValidation.InvalidCodeMessageError);

			bill.B0_ForeignPortOfContractKCode = "Z1Z32";
			AssertNoMessageErrorContaining(bill.B0_ForeignPortOfContractKCodeInfo, ListValidation.InvalidCodeMessageError);
			bill.ValidationModes = ValidationModes.None;
			bill.B0_ForeignPortOfContractKCode = "Z!";
			AssertNoMessageErrorContaining(bill.B0_ForeignPortOfContractKCodeInfo, ListValidation.InvalidCodeMessageError);
		}

		public void TestCheckB0_TransportPaymentMethod()
		{
			bill.ValidationModes = ValidationModes.InventoryRecord;
			bill.B0_TransportPaymentMethod = ZString.Empty;
			AssertNoMessageErrors(bill.B0_TransportPaymentMethodInfo);
			bill.B0_TransportPaymentMethod = "Z!";
			AssertHasMessageErrorContaining(bill.B0_TransportPaymentMethodInfo, ListValidation.InvalidCodeMessageError);
			bill.B0_TransportPaymentMethod = PaymentMethodCodeList.Codes.BOP;
			AssertNoMessageErrorContaining(bill.B0_TransportPaymentMethodInfo, ListValidation.InvalidCodeMessageError);
			bill.ValidationModes = ValidationModes.None;
			bill.B0_TransportPaymentMethod = "Z!";
			AssertNoMessageErrorContaining(bill.B0_TransportPaymentMethodInfo, ListValidation.InvalidCodeMessageError);
		}

		public void TestCheckB0_TransportModeToPortOfLading()
		{
			bill.ValidationModes = ValidationModes.InventoryRecord;
			bill.B0_TransportModeToPortOfLading = ZString.Empty;
			AssertNoMessageErrors(bill.B0_TransportModeToPortOfLadingInfo);
			bill.B0_TransportModeToPortOfLading = "Z!";
			AssertHasMessageErrorContaining(bill.B0_TransportModeToPortOfLadingInfo, ListValidation.InvalidCodeMessageError);
			bill.B0_TransportModeToPortOfLading = US.AMS.Business.TransportTypeList.Codes.Rail;
			AssertNoMessageErrorContaining(bill.B0_TransportModeToPortOfLadingInfo, ListValidation.InvalidCodeMessageError);
			bill.ValidationModes = ValidationModes.None;
			bill.B0_TransportModeToPortOfLading = "Z!";
			AssertNoMessageErrorContaining(bill.B0_TransportModeToPortOfLadingInfo, ListValidation.InvalidCodeMessageError);
		}

		public void TestCheckB0_RL_NKLastForeignPort()
		{
			bill.ValidationModes = ValidationModes.InventoryRecord;
			bill.B0_RL_NKLastForeignPort = ZString.Empty;
			AssertNoMessageErrors(bill.B0_RL_NKLastForeignPortInfo);
			bill.B0_RL_NKLastForeignPort = "AU!!@";
			AssertNoMessageErrors(bill.B0_RL_NKLastForeignPortInfo);
			AssertHasWarningContaining(bill.B0_RL_NKLastForeignPortInfo, ListValidation.InvalidCodeMessage);
			bill.B0_RL_NKLastForeignPort = "AUSYD";
			AssertNoMessageErrors(bill.B0_RL_NKLastForeignPortInfo);
			AssertNoWarningContaining(bill.B0_RL_NKLastForeignPortInfo, ListValidation.InvalidCodeMessage);
			bill.ValidationModes = ValidationModes.None;
			bill.B0_RL_NKLastForeignPort = "AU!!@";
			AssertNoMessageErrors(bill.B0_RL_NKLastForeignPortInfo);
			AssertNoWarningContaining(bill.B0_RL_NKLastForeignPortInfo, ListValidation.InvalidCodeMessage);
		}

		public void TestCheckB0_LastForeignPortKCode()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Port, "PORT");
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Port, "Z1Z32", "Test", ZDateTime.BrettsBirthday, ZDateTime.MaxSmallDateTime);
			Factory.Save();

			bill.ValidationModes = ValidationModes.InventoryRecord;
			bill.B0_LastForeignPortKCode = ZString.Empty;
			AssertHasMessageErrorContaining(bill.B0_LastForeignPortKCodeInfo, MandatoryValidation.YouHaveNotEntered);
			bill.B0_LastForeignPortKCode = "Z!";
			AssertNoMessageErrorContaining(bill.B0_LastForeignPortKCodeInfo, MandatoryValidation.YouHaveNotEntered);
			AssertHasMessageErrorContaining(bill.B0_LastForeignPortKCodeInfo, ListValidation.InvalidCodeMessageError);
			bill.B0_LastForeignPortKCode = "Z1Z32";
			AssertNoMessageErrorContaining(bill.B0_LastForeignPortKCodeInfo, MandatoryValidation.YouHaveNotEntered);
			AssertNoMessageErrorContaining(bill.B0_LastForeignPortKCodeInfo, ListValidation.InvalidCodeMessageError);
			bill.ValidationModes = ValidationModes.None;
			bill.B0_LastForeignPortKCode = "Z!";
			AssertNoMessageErrorContaining(bill.B0_LastForeignPortKCodeInfo, MandatoryValidation.YouHaveNotEntered);
			AssertNoMessageErrorContaining(bill.B0_LastForeignPortKCodeInfo, ListValidation.InvalidCodeMessageError);
			bill.B0_LastForeignPortKCode = ZString.Empty;
			AssertNoMessageErrorContaining(bill.B0_LastForeignPortKCodeInfo, MandatoryValidation.YouHaveNotEntered);
			AssertNoMessageErrorContaining(bill.B0_LastForeignPortKCodeInfo, ListValidation.InvalidCodeMessageError);
		}

		public void TestCheckB0_PlaceOfReceipt()
		{
			bill.ValidationModes = ValidationModes.InventoryRecord;
			bill.B0_PlaceOfReceipt = ZString.Empty;
			AssertHasMessageErrorContaining(bill.B0_PlaceOfReceiptInfo, MandatoryValidation.YouHaveNotEntered);
			bill.B0_PlaceOfReceipt = "SYDNEY";
			AssertNoMessageErrorContaining(bill.B0_PlaceOfReceiptInfo, MandatoryValidation.YouHaveNotEntered);
			bill.ValidationModes = ValidationModes.None;
			bill.B0_PlaceOfReceipt = ZString.Empty;
			AssertNoMessageErrorContaining(bill.B0_PlaceOfReceiptInfo, MandatoryValidation.YouHaveNotEntered);
		}

		public void TestCheckB0_VolumeUQ()
		{
			bill.ValidationModes = ValidationModes.InventoryRecord;
			bill.B0_Volume = ZDecimal.Zero;
			bill.B0_VolumeUQ = ZString.Empty;
			AssertNoMessageErrorContaining(bill.B0_VolumeUQInfo, MandatoryValidation.YouHaveNotEntered);
			AssertNoMessageErrorContaining(bill.B0_VolumeUQInfo, ListValidation.InvalidCodeMessageError);
			bill.B0_Volume = 10m;
			bill.B0_VolumeUQ = ZString.Empty;
			AssertHasMessageErrorContaining(bill.B0_VolumeUQInfo, MandatoryValidation.YouHaveNotEntered);
			AssertNoMessageErrorContaining(bill.B0_VolumeUQInfo, ListValidation.InvalidCodeMessageError);
			bill.B0_VolumeUQ = "Z!";
			AssertNoMessageErrorContaining(bill.B0_VolumeUQInfo, MandatoryValidation.YouHaveNotEntered);
			AssertHasMessageErrorContaining(bill.B0_VolumeUQInfo, ListValidation.InvalidCodeMessageError);
			bill.B0_VolumeUQ = Core.Constants.Volume.CubicFeet;
			AssertNoMessageErrorContaining(bill.B0_VolumeUQInfo, MandatoryValidation.YouHaveNotEntered);
			AssertNoMessageErrorContaining(bill.B0_VolumeUQInfo, ListValidation.InvalidCodeMessageError);
			bill.ValidationModes = ValidationModes.None;
			bill.B0_VolumeUQ = ZString.Empty;
			AssertNoMessageErrorContaining(bill.B0_VolumeUQInfo, MandatoryValidation.YouHaveNotEntered);
			AssertNoMessageErrorContaining(bill.B0_VolumeUQInfo, ListValidation.InvalidCodeMessageError);
			bill.B0_VolumeUQ = "Z!";
			AssertNoMessageErrorContaining(bill.B0_VolumeUQInfo, MandatoryValidation.YouHaveNotEntered);
			AssertNoMessageErrorContaining(bill.B0_VolumeUQInfo, ListValidation.InvalidCodeMessageError);
		}

		public void TestCheckB0_Volume()
		{
			bill.ValidationModes = ValidationModes.InventoryRecord;
			bill.B0_Volume = ZDecimal.Zero;
			AssertNoMessageErrorContaining(bill.B0_VolumeInfo, MandatoryValidation.ValueCannotBeNegative);
			bill.B0_Volume = -10m;
			AssertHasMessageErrorContaining(bill.B0_VolumeInfo, MandatoryValidation.ValueCannotBeNegative);
			bill.B0_Volume = 10m;
			AssertNoMessageErrorContaining(bill.B0_VolumeInfo, MandatoryValidation.ValueCannotBeNegative);
			bill.ValidationModes = ValidationModes.None;
			bill.B0_Volume = -10m;
			AssertNoMessageErrorContaining(bill.B0_VolumeInfo, MandatoryValidation.ValueCannotBeNegative);
		}

		public void CheckB0_IssuerSCAC()
		{
			bill.ValidationModes = ValidationModes.InventoryRecord;
			bill.B0_IssuerSCAC = "Z!";
			AssertNoMessageErrorContaining(bill.B0_IssuerSCACInfo, ListValidation.InvalidCodeMessageError);
		}

		public void TestCheckB0_BillStatus()
		{
			bill.ValidationModes = ValidationModes.InventoryRecord;
			bill.B0_BillStatus = BillOfLadingStatusIndicatorList.Codes.SimpleRegularInBondType62_63WithISF;
			AssertNull(bill.MasterInBondMovement);
			AssertNoMessageErrorContaining(bill.B0_BillStatusInfo, ValidationConstants.Bill.BillStatusNotForPTT.ToString());
			bill.B0_BillStatus = ZString.Empty;
			AssertHasMessageErrorContaining(bill.B0_BillStatusInfo, MandatoryValidation.YouHaveNotEntered);
			bill.B0_BillStatus = "!";
			AssertNoMessageErrorContaining(bill.B0_BillStatusInfo, MandatoryValidation.YouHaveNotEntered);
			AssertHasMessageErrorContaining(bill.B0_BillStatusInfo, ListValidation.InvalidCodeMessageError);
			bill.B0_BillStatus = BillOfLadingStatusIndicatorList.Codes.NonAutomatedIntransitRail;
			AssertNoMessageErrorContaining(bill.B0_BillStatusInfo, MandatoryValidation.YouHaveNotEntered);
			AssertHasMessageErrorContaining(bill.B0_BillStatusInfo, ListValidation.InvalidCodeMessageError);
			bill.ValidationModes = ValidationModes.None;
			bill.B0_BillStatus = "!";
			AssertNoMessageErrorContaining(bill.B0_BillStatusInfo, MandatoryValidation.YouHaveNotEntered);
			AssertNoMessageErrorContaining(bill.B0_BillStatusInfo, ListValidation.InvalidCodeMessageError);
			bill.B0_BillStatus = ZString.Empty;
			AssertNoMessageErrorContaining(bill.B0_BillStatusInfo, MandatoryValidation.YouHaveNotEntered);
			AssertNoMessageErrorContaining(bill.B0_BillStatusInfo, ListValidation.InvalidCodeMessageError);
			bill.ValidationModes = ValidationModes.PermitToTransfer;
			bill.B0_BillStatus = ZString.Empty;
			AssertHasMessageErrorContaining(bill.B0_BillStatusInfo, MandatoryValidation.YouHaveNotEntered);
			bill.B0_BillStatus = "!";
			AssertNoMessageErrorContaining(bill.B0_BillStatusInfo, MandatoryValidation.YouHaveNotEntered);
			AssertHasMessageErrorContaining(bill.B0_BillStatusInfo, ListValidation.InvalidCodeMessageError);
			bill.B0_BillStatus = BillOfLadingStatusIndicatorList.Codes.InternationalMailDirectDischargeAtMailFacility;
			AssertHasMessageErrorContaining(bill.B0_BillStatusInfo, ValidationConstants.Bill.BillStatusNotForPTT.ToString());
			bill.B0_BillStatus = BillOfLadingStatusIndicatorList.Codes.MasterBill;
			AssertNoMessageErrorContaining(bill.B0_BillStatusInfo, ValidationConstants.Bill.BillStatusNotForPTT.ToString());
		}

		public void TestCheckB0_WeightUQ()
		{
			bill.ValidationModes = ValidationModes.InventoryRecord;
			bill.B0_WeightUQ = ZString.Empty;
			AssertHasMessageErrorContaining(bill.B0_WeightUQInfo, MandatoryValidation.YouHaveNotEntered);
			bill.B0_WeightUQ = "Z!";
			AssertNoMessageErrorContaining(bill.B0_WeightUQInfo, MandatoryValidation.YouHaveNotEntered);
			AssertHasMessageErrorContaining(bill.B0_WeightUQInfo, ListValidation.InvalidCodeMessageError);
			bill.B0_WeightUQ = Core.Constants.Weight.Kilograms;
			AssertNoMessageErrorContaining(bill.B0_WeightUQInfo, MandatoryValidation.YouHaveNotEntered);
			AssertNoMessageErrorContaining(bill.B0_WeightUQInfo, ListValidation.InvalidCodeMessageError);
			bill.ValidationModes = ValidationModes.None;
			bill.B0_WeightUQ = "Z!";
			AssertNoMessageErrorContaining(bill.B0_WeightUQInfo, MandatoryValidation.YouHaveNotEntered);
			AssertNoMessageErrorContaining(bill.B0_WeightUQInfo, ListValidation.InvalidCodeMessageError);
			bill.B0_WeightUQ = ZString.Empty;
			AssertNoMessageErrorContaining(bill.B0_WeightUQInfo, MandatoryValidation.YouHaveNotEntered);
			AssertNoMessageErrorContaining(bill.B0_WeightUQInfo, ListValidation.InvalidCodeMessageError);
		}

		public void TestCheckB0_Weight()
		{
			bill.ValidationModes = ValidationModes.InventoryRecord;
			bill.B0_Weight = ZDecimal.Zero;
			AssertNoMessageErrorContaining(bill.B0_WeightInfo, MandatoryValidation.ValueCannotBeNegative);
			AssertHasMessageErrorContaining(bill.B0_WeightInfo, MandatoryValidation.ValueCannotBeZero);
			bill.B0_Weight = -10m;
			AssertHasMessageErrorContaining(bill.B0_WeightInfo, MandatoryValidation.ValueCannotBeNegative);
			AssertNoMessageErrorContaining(bill.B0_WeightInfo, MandatoryValidation.ValueCannotBeZero);
			bill.B0_Weight = 10m;
			AssertNoMessageErrorContaining(bill.B0_WeightInfo, MandatoryValidation.ValueCannotBeNegative);
			AssertNoMessageErrorContaining(bill.B0_WeightInfo, MandatoryValidation.ValueCannotBeZero);
			bill.ValidationModes = ValidationModes.None;
			bill.B0_Weight = ZDecimal.Zero;
			AssertNoMessageErrorContaining(bill.B0_WeightInfo, MandatoryValidation.ValueCannotBeNegative);
			AssertNoMessageErrorContaining(bill.B0_WeightInfo, MandatoryValidation.ValueCannotBeZero);
			bill.B0_Weight = -10m;
			AssertNoMessageErrorContaining(bill.B0_WeightInfo, MandatoryValidation.ValueCannotBeNegative);
			AssertNoMessageErrorContaining(bill.B0_WeightInfo, MandatoryValidation.ValueCannotBeZero);
		}

		public void TestCheckB0_ManifestUQ()
		{
			bill.ValidationModes = ValidationModes.InventoryRecord;
			bill.B0_ManifestUQ = ZString.Empty;
			AssertHasMessageErrorContaining(bill.B0_ManifestUQInfo, MandatoryValidation.YouHaveNotEntered);
			bill.B0_ManifestUQ = "Z!";
			AssertNoMessageErrorContaining(bill.B0_ManifestUQInfo, MandatoryValidation.YouHaveNotEntered);
			AssertHasMessageErrorContaining(bill.B0_ManifestUQInfo, ListValidation.InvalidCodeMessageError);
			bill.B0_ManifestUQ = Core.Constants.PkgUnit.Pallet;
			AssertNoMessageErrorContaining(bill.B0_ManifestUQInfo, MandatoryValidation.YouHaveNotEntered);
			AssertNoMessageErrorContaining(bill.B0_ManifestUQInfo, ListValidation.InvalidCodeMessageError);
			bill.ValidationModes = ValidationModes.None;
			bill.B0_ManifestUQ = "Z!";
			AssertNoMessageErrorContaining(bill.B0_ManifestUQInfo, MandatoryValidation.YouHaveNotEntered);
			AssertNoMessageErrorContaining(bill.B0_ManifestUQInfo, ListValidation.InvalidCodeMessageError);
			bill.B0_ManifestUQ = ZString.Empty;
			AssertNoMessageErrorContaining(bill.B0_ManifestUQInfo, MandatoryValidation.YouHaveNotEntered);
			AssertNoMessageErrorContaining(bill.B0_ManifestUQInfo, ListValidation.InvalidCodeMessageError);
		}

		public void TestCheckB0_ManifestQty()
		{
			bill.ValidationModes = ValidationModes.InventoryRecord;
			bill.B0_ManifestQty = ZInt.Zero;
			AssertNoMessageErrorContaining(bill.B0_ManifestQtyInfo, MandatoryValidation.ValueCannotBeNegative);
			AssertHasMessageErrorContaining(bill.B0_ManifestQtyInfo, MandatoryValidation.ValueCannotBeZero);
			bill.B0_ManifestQty = -10;
			AssertHasMessageErrorContaining(bill.B0_ManifestQtyInfo, MandatoryValidation.ValueCannotBeNegative);
			AssertNoMessageErrorContaining(bill.B0_ManifestQtyInfo, MandatoryValidation.ValueCannotBeZero);
			bill.B0_ManifestQty = 10;
			AssertNoMessageErrorContaining(bill.B0_ManifestQtyInfo, MandatoryValidation.ValueCannotBeNegative);
			AssertNoMessageErrorContaining(bill.B0_ManifestQtyInfo, MandatoryValidation.ValueCannotBeZero);
			bill.ValidationModes = ValidationModes.None;
			bill.B0_ManifestQty = ZInt.Zero;
			AssertNoMessageErrorContaining(bill.B0_ManifestQtyInfo, MandatoryValidation.ValueCannotBeNegative);
			AssertNoMessageErrorContaining(bill.B0_ManifestQtyInfo, MandatoryValidation.ValueCannotBeZero);
			bill.B0_ManifestQty = -10;
			AssertNoMessageErrorContaining(bill.B0_ManifestQtyInfo, MandatoryValidation.ValueCannotBeNegative);
			AssertNoMessageErrorContaining(bill.B0_ManifestQtyInfo, MandatoryValidation.ValueCannotBeZero);
		}

		public void TestCheckB0_RL_NKPortOfLading()
		{
			bill.ValidationModes = ValidationModes.InventoryRecord;
			bill.B0_RL_NKPortOfLading = ZString.Empty;
			AssertNoMessageErrors(bill.B0_RL_NKPortOfLadingInfo);
			bill.B0_RL_NKPortOfLading = "AU!!@";
			AssertNoMessageErrors(bill.B0_RL_NKPortOfLadingInfo);
			AssertHasWarningContaining(bill.B0_RL_NKPortOfLadingInfo, ListValidation.InvalidCodeMessage);
			bill.B0_RL_NKPortOfLading = "AUSYD";
			AssertNoMessageErrors(bill.B0_RL_NKPortOfLadingInfo);
			AssertNoWarningContaining(bill.B0_RL_NKPortOfLadingInfo, ListValidation.InvalidCodeMessage);
			bill.ValidationModes = ValidationModes.None;
			bill.B0_RL_NKPortOfLading = "AU!!@";
			AssertNoMessageErrors(bill.B0_RL_NKPortOfLadingInfo);
			AssertNoWarningContaining(bill.B0_RL_NKPortOfLadingInfo, ListValidation.InvalidCodeMessage);
		}

		public void TestCheckB0_PortOfLadingKCode()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Port, "PORT");
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Port, "Z1Z32", "Test", ZDateTime.BrettsBirthday, ZDateTime.MaxSmallDateTime);
			Factory.Save();

			bill.ValidationModes = ValidationModes.InventoryRecord;
			bill.B0_PortOfLadingKCode = ZString.Empty;
			AssertHasMessageErrorContaining(bill.B0_PortOfLadingKCodeInfo, MandatoryValidation.YouHaveNotEntered);
			bill.B0_PortOfLadingKCode = "Z!";
			AssertNoMessageErrorContaining(bill.B0_PortOfLadingKCodeInfo, MandatoryValidation.YouHaveNotEntered);
			AssertHasMessageErrorContaining(bill.B0_PortOfLadingKCodeInfo, ListValidation.InvalidCodeMessageError);
			bill.B0_PortOfLadingKCode = "Z1Z32";
			AssertNoMessageErrorContaining(bill.B0_PortOfLadingKCodeInfo, MandatoryValidation.YouHaveNotEntered);
			AssertNoMessageErrorContaining(bill.B0_PortOfLadingKCodeInfo, ListValidation.InvalidCodeMessageError);
			bill.ValidationModes = ValidationModes.None;
			bill.B0_PortOfLadingKCode = "Z!";
			AssertNoMessageErrorContaining(bill.B0_PortOfLadingKCodeInfo, MandatoryValidation.YouHaveNotEntered);
			AssertNoMessageErrorContaining(bill.B0_PortOfLadingKCodeInfo, ListValidation.InvalidCodeMessageError);
			bill.B0_PortOfLadingKCode = ZString.Empty;
			AssertNoMessageErrorContaining(bill.B0_PortOfLadingKCodeInfo, MandatoryValidation.YouHaveNotEntered);
			AssertNoMessageErrorContaining(bill.B0_PortOfLadingKCodeInfo, ListValidation.InvalidCodeMessageError);
			bill.ValidationModes = ValidationModes.ChangeEstDateOfArrival;
			bill.B0_PortOfLadingKCode = ZString.Empty;
			AssertNoMessageErrorContaining(bill.B0_PortOfLadingKCodeInfo, MandatoryValidation.YouHaveNotEntered);
		}

		public void TestCheckB0_MasterBillNumber()
		{
			bill.ValidationModes = ValidationModes.InventoryRecord;
			bill.B0_MasterBillNumber = ZString.Empty;
			AssertHasMessageErrorContaining(bill.B0_MasterBillNumberInfo, MandatoryValidation.YouHaveNotEntered);
			bill.B0_MasterBillNumber = "HB201";
			AssertNoMessageErrorContaining(bill.B0_MasterBillNumberInfo, MandatoryValidation.YouHaveNotEntered);
			bill.ValidationModes = ValidationModes.None;
			bill.B0_MasterBillNumber = ZString.Empty;
			AssertNoMessageErrorContaining(bill.B0_MasterBillNumberInfo, MandatoryValidation.YouHaveNotEntered);
			var moveDetail = bill.MovementDetail;
			moveDetail.B9_CustomsStatus = AMSBillCustomsStatusList.Codes.OnFile;
			moveDetail.B9_MessageStatus = AMSBillMessageStatusList.Codes.Adding;
			var billOnFileMessage = ValidationConstants.Bill.ChangingBOLNumberWhenBillIsOnFile("HB201", "HB202");
			var messagingInProgressMessage = ValidationConstants.Bill.ChangingBOLNumberWhenMessagingIsInProgress("HB201", "HB202");
			bill.B0_MasterBillNumber = "HB201";
			bill.B0_MasterBillNumber = "HB202";
			AssertNoError(bill.B0_MasterBillNumberInfo, billOnFileMessage);
			AssertNoError(bill.B0_MasterBillNumberInfo, messagingInProgressMessage);
			bill.B0_MasterBillNumber = "HB201";
			Factory.Save();
			bill.B0_MasterBillNumber = "HB202";
			AssertHasError(bill.B0_MasterBillNumberInfo, billOnFileMessage);
			AssertNoError(bill.B0_MasterBillNumberInfo, messagingInProgressMessage);
			moveDetail.B9_CustomsStatus = AMSBillCustomsStatusList.Codes.NotOnFile;
			bill.Validation.ValidateB0_MasterBillNumber();
			AssertNoError(bill.B0_MasterBillNumberInfo, billOnFileMessage);
			AssertHasError(bill.B0_MasterBillNumberInfo, messagingInProgressMessage);
			moveDetail.B9_MessageStatus = AMSBillMessageStatusList.Codes.Error;
			bill.Validation.ValidateB0_MasterBillNumber();
			AssertNoError(bill.B0_MasterBillNumberInfo, billOnFileMessage);
			AssertNoError(bill.B0_MasterBillNumberInfo, messagingInProgressMessage);
			foreach (var mode in new[] { ValidationModes.ChangeEstDateOfArrival, ValidationModes.VesselArrival, ValidationModes.VesselDeparture })
			{
				bill.ValidationModes = mode;
				bill.B0_MasterBillNumber = ZString.Empty;
				AssertNoMessageErrorContaining(bill.B0_MasterBillNumberInfo, MandatoryValidation.YouHaveNotEntered);
			}

			bill.ValidationModes = ValidationModes.SubsequentInBond;
			bill.B0_MasterBillNumber = ZString.Empty;
			AssertHasMessageErrorContaining(bill.B0_MasterBillNumberInfo, MandatoryValidation.YouHaveNotEntered);
			var messageError = ValidationConstants.Bill.BillOfLadingNumberIsTooLong("1234567890123");
			bill.B0_MasterBillNumber = "1234567890123";
			AssertHasMessageError(bill.B0_MasterBillNumberInfo, messageError.ToString());
			bill.B0_MasterBillNumber = "123#$456789012";
			AssertNoMessageError(bill.B0_MasterBillNumberInfo, messageError.ToString());
			AssertHasMessageError(bill.B0_MasterBillNumberInfo, ValidationConstants.Bill.BillOfLadingNumberAlphanumeric.ToString());
			bill.B0_MasterBillNumber = "123456789012";
			AssertNoMessageErrors(bill.B0_MasterBillNumberInfo);
		}

		public void TestCheckB0_IssuerCode()
		{
			moveDetail.B9_CustomsStatus = AMSBillCustomsStatusList.Codes.OnFile;
			moveDetail.B9_MessageStatus = AMSBillMessageStatusList.Codes.Adding;
			bill.ValidationModes = ValidationModes.InventoryRecord;
			bill.B0_IssuerCode = ZString.Empty;
			AssertHasMessageErrorContaining(bill.B0_IssuerCodeInfo, MandatoryValidation.YouHaveNotEntered);
			bill.B0_IssuerCode = "Z!";
			AssertNoMessageErrorContaining(bill.B0_IssuerCodeInfo, MandatoryValidation.YouHaveNotEntered);
			AssertHasMessageErrorContaining(bill.B0_IssuerCodeInfo, ListValidation.InvalidCodeMessageError);
			var carrier = Factory.New<USCarrierCombined>();
			carrier.UI_Code = "Z1Z3";
			bill.B0_IssuerCode = "Z1Z3";
			AssertNoMessageError(bill.B0_IssuerCodeInfo, MandatoryValidation.YouHaveNotEntered);
			AssertNoMessageError(bill.B0_IssuerCodeInfo, ListValidation.InvalidCodeMessageError);
			bill.ValidationModes = ValidationModes.None;
			bill.B0_IssuerCode = "Z!";
			AssertNoMessageError(bill.B0_IssuerCodeInfo, MandatoryValidation.YouHaveNotEntered);
			AssertNoMessageError(bill.B0_IssuerCodeInfo, ListValidation.InvalidCodeMessageError);
			bill.B0_IssuerCode = ZString.Empty;
			AssertNoMessageError(bill.B0_IssuerCodeInfo, MandatoryValidation.YouHaveNotEntered);
			AssertNoMessageError(bill.B0_IssuerCodeInfo, ListValidation.InvalidCodeMessageError);
			var billOnFileMessage = ValidationConstants.Bill.ChangingIssuerCodeWhenBillIsOnFile("HB21", "HB22");
			var messagingInProgressMessage = ValidationConstants.Bill.ChangingIssuerCodeWhenMessagingIsInProgress("HB21", "HB22");
			bill.B0_IssuerCode = "HB21";
			bill.B0_IssuerCode = "HB22";
			AssertNoError(bill.B0_IssuerCodeInfo, billOnFileMessage);
			AssertNoError(bill.B0_IssuerCodeInfo, messagingInProgressMessage);
			bill.B0_IssuerCode = "HB21";
			Factory.Save();
			bill.B0_IssuerCode = "HB22";
			AssertHasError(bill.B0_IssuerCodeInfo, billOnFileMessage);
			AssertNoError(bill.B0_IssuerCodeInfo, messagingInProgressMessage);
			moveDetail.B9_CustomsStatus = AMSBillCustomsStatusList.Codes.NotOnFile;
			bill.Validation.ValidateB0_IssuerCode();
			AssertNoError(bill.B0_IssuerCodeInfo, billOnFileMessage);
			AssertHasError(bill.B0_IssuerCodeInfo, messagingInProgressMessage);
			moveDetail.B9_MessageStatus = AMSBillMessageStatusList.Codes.Error;
			bill.Validation.ValidateB0_IssuerCode();
			AssertNoError(bill.B0_IssuerCodeInfo, billOnFileMessage);
			AssertNoError(bill.B0_IssuerCodeInfo, messagingInProgressMessage);
			foreach (var mode in new[] { ValidationModes.ChangeEstDateOfArrival, ValidationModes.VesselArrival, ValidationModes.VesselDeparture })
			{
				bill.ValidationModes = mode;
				bill.B0_IssuerCode = ZString.Empty;
				AssertNoMessageErrorContaining(bill.B0_IssuerCodeInfo, MandatoryValidation.YouHaveNotEntered);
			}

			bill.ValidationModes = ValidationModes.SubsequentInBond;
			bill.B0_IssuerCode = ZString.Empty;
			AssertHasMessageErrorContaining(bill.B0_IssuerCodeInfo, MandatoryValidation.YouHaveNotEntered);
		}

		public void TestCheckB0_MasterInBondIndicator()
		{
			AssertNull(bill.MasterInBondMovement);
			bill.ValidationModes = ValidationModes.ChangeEstDateOfArrival;
			bill.B0_MasterInBondIndicator = ZBool.True;
			AssertNoMessageError(bill.B0_MasterInBondIndicatorInfo, ValidationConstants.Bill.MasterInBondIsRequiredWhenIndicatorIsTrue.ToString());
			bill.B0_MasterInBondIndicator = ZBool.False;
			AssertNoMessageError(bill.B0_MasterInBondIndicatorInfo, ValidationConstants.Bill.MasterInBondIsRequiredWhenIndicatorIsTrue.ToString());
			bill.ValidationModes = ValidationModes.InventoryRecord;
			bill.B0_MasterInBondIndicator = ZBool.True;
			AssertHasMessageError(bill.B0_MasterInBondIndicatorInfo, ValidationConstants.Bill.MasterInBondIsRequiredWhenIndicatorIsTrue.ToString());
			bill.B0_MasterInBondIndicator = ZBool.False;
			AssertNoMessageError(bill.B0_MasterInBondIndicatorInfo, ValidationConstants.Bill.MasterInBondIsRequiredWhenIndicatorIsTrue.ToString());
			var inbondMoveHeader = header.InBondMovementHeaders.AddNew();
			var inbondMoveDetail = inbondMoveHeader.MovementDetails.AddNew(bill.PK);
			AssertNull(bill.MasterInBondMovement);
			bill.B0_MasterInBondIndicator = ZBool.True;
			AssertEquals(inbondMoveHeader, bill.MasterInBondMovement);
			AssertNoMessageError(bill.B0_MasterInBondIndicatorInfo, ValidationConstants.Bill.MasterInBondIsRequiredWhenIndicatorIsTrue.ToString());
		}

		public void TestCheckB0_DateOfDischarge()
		{
			bill.B0_InBondPortOfDestDCode = "1111";
			AssertNoErrorContaining(bill.B0_DateOfDischargeInfo, "can only have one estimated unloading date");
			var bill2 = header.Bills.AddNew();
			bill2.B0_DateOfDischarge = new ZDate(2015, 3, 11);
			bill2.B0_InBondPortOfDestDCode = "1111";
			AssertHasErrorContaining(bill2.B0_DateOfDischargeInfo, "can only have one estimated unloading date");
			bill.B0_DateOfDischarge = new ZDate(2015, 3, 11);
			AssertNoErrorContaining(bill.B0_DateOfDischargeInfo, "can only have one estimated unloading date");
			bill.B0_DateOfDischarge = new ZDate(2015, 3, 12);
			AssertHasErrorContaining(bill.B0_DateOfDischargeInfo, "can only have one estimated unloading date");
			bill2.B0_InBondPortOfDestDCode = "2222";
			AssertNoErrorContaining(bill2.B0_DateOfDischargeInfo, "can only have one estimated unloading date");
		}

		public void TestCheckB0_MasterBillNumberWithDuplicate()
		{
			var inBondHeader = Factory.New<Integration.Customs.US.InBond.ICusInBondHeader>();
			var inBondBill = Factory.New<Integration.Customs.US.InBond.ICusInBondBill>();
			inBondBill.B0_IssuerCode = "Z1Z3";
			inBondBill.B0_MasterBillNumber = "123456789012";
			inBondBill.B0_BH = inBondHeader.PK;
			Factory.Save();
			var newHeader = Factory.New<CusInBondHeader>();
			var newBill = CreateBill(newHeader);
			newBill.B0_IssuerCode = "Z1Z3";
			newBill.B0_MasterBillNumber = "123456789012";
			Factory.Save();
			var message = ValidationConstants.Bill.BillOfLadingNumberIsDuplicated(header, newHeader.BH_JobReference, newHeader.Company.GC_Name, newHeader.Branch.GB_BranchName);
			AssertContains("already contains the house bill number", message.ToString());
			bill.B0_IssuerCode = "Z1Z3";
			bill.B0_MasterBillNumber = "123456789012";
			AssertHasMessageError(bill.B0_MasterBillNumberInfo, message.ToString());

			var parentShipment = Factory.New<ForwardingShipment>();
			parentShipment.JS_ShipmentType = Core.Constants.ShipmentTypes.HighVolumeLowValue;
			header.BH_ParentTableCode = parentShipment.TablePrefix;
			header.BH_ParentID = parentShipment.PK;
			Assert("Precondition: Parent is identified as HVLV Shipment", header.HasHVLVParent);
			bill.Validation.ValidateB0_MasterBillNumber();
			AssertNoMessageError("Duplication checking should be skipped for HVLV Shipments", bill.B0_MasterBillNumberInfo, message.ToString());

			header.BH_ParentTableCode = string.Empty;
			header.BH_ParentID = ZGuid.Empty;
			Assert("Sanity check: Parent is no longer identified as HVLV Shipment", !header.HasHVLVParent);

			bill.B0_MasterBillNumber = "123456789013";
			AssertNoMessageError(bill.B0_MasterBillNumberInfo, message.ToString());
			bill.B0_IssuerCode = "Z1Z2";
			bill.B0_MasterBillNumber = "123456789012";
			AssertNoMessageError(bill.B0_MasterBillNumberInfo, message.ToString());
			newHeader.BH_IsActive = false;
			Factory.Save();
			bill.B0_IssuerCode = "Z1Z3";
			bill.Validation.ValidateB0_MasterBillNumber();
			AssertNoMessageError(bill.B0_MasterBillNumberInfo, message.ToString());
			newHeader.BH_IsActive = true;
			var newBill2 = CreateBill(newHeader);
			newBill2.B0_IssuerCode = "Z1Z3";
			newBill2.B0_MasterBillNumber = "123456789012";
			AssertNoMessageErrors("Duplicated house bill numbers in one header, no errors.", newBill2.B0_MasterBillNumberInfo);
			newHeader.BH_TransitDirection = DirectionTypeList.Codes.NVOCC;
			newBill.B0_ShipmentType = CusInBondBill.OceanBillType;
			Factory.Save();
			var message2 = ValidationConstants.Bill.BillOfLadingNumberIsDuplicated(newHeader, header.BH_JobReference, header.Company.GC_Name, header.Branch.GB_BranchName);
			AssertContains("already contains the ocean bill and house bill numbers", message2.ToString());
			bill.Validation.ValidateB0_MasterBillNumber();
			AssertNoMessageError("VOCC house bill only check VOCC house bill", bill.B0_MasterBillNumberInfo, message.ToString());
			newBill.Validation.ValidateB0_MasterBillNumber();
			AssertNoMessageError("NVOCC house bill only check NVOCC house bill", newBill.B0_MasterBillNumberInfo, message2.ToString());
			newBill2.B0_IssuerCode = "Z1Z4";
			newBill2.B0_MasterBillNumber = "123456789014";
			header.BH_TransitDirection = DirectionTypeList.Codes.NVOCC;
			header.BH_ImportTransportMode = TransportTypeList.Codes.Rail;
			bill.B0_ShipmentType = CusInBondBill.OceanBillType;
			var bill2 = CreateBill(header);
			bill2.B0_IssuerCode = "Z1Z4";
			bill2.B0_MasterBillNumber = "123456789014";
			Factory.Save();
			message = ValidationConstants.Bill.BillOfLadingNumberIsDuplicated(header, newHeader.BH_JobReference, newHeader.Company.GC_Name, newHeader.Branch.GB_BranchName);
			AssertContains("already contains the master bill and house bill numbers", message.ToString());
			bill2.Validation.ValidateB0_MasterBillNumber();
			AssertHasMessageError("NVOCC house bill should match bill use issuer code and bill number and master bill (issuer code and number)", bill2.B0_MasterBillNumberInfo, message.ToString());
			newBill2.Validation.ValidateB0_MasterBillNumber();
			AssertHasMessageError("NVOCC house bill should match bill use issuer code and bill number and master bill (issuer code and number)", newBill2.B0_MasterBillNumberInfo, message2.ToString());
		}

		protected override CusInBondBill CreateBill(CusInBondHeader header)
		{
			return header.Bills.AddNew();
		}
	}
}
