using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Universal.Testing;
using Enterprise.Customs.US.AMS.Messaging.Business;
using Enterprise.Customs.US.Business;

namespace Enterprise.Customs.US.AMS.Business.Testing
{
	sealed class CusInBondContainerValidationTest : BusinessObjectValidationTestCase
	{
		public void TestValidateAtLeastOneCommodityExist()
		{
			bill.ValidationModes = ValidationModes.InventoryRecord;
			var commodity = container.Commodities.AddNew();
			container.Validation.ValidateAll();
			AssertNoRowMessageError(container, ValidationConstants.Container.AtLeastOneCommodityIsRequired);

			commodity.Delete();
			container.Validation.ValidateAll();
			AssertHasRowMessageError(container, ValidationConstants.Container.AtLeastOneCommodityIsRequired);

			bill.ValidationModes = ValidationModes.None;
			container.Validation.ValidateAll();
			AssertNoRowMessageError(container, ValidationConstants.Container.AtLeastOneCommodityIsRequired);
		}

		public void TestValidateMaximumNumberOfSealNumber()
		{
			bill.ValidationModes = ValidationModes.InventoryRecord;
			var commodity = container.Commodities.AddNew();
			container.Validation.ValidateAll();
			container.BC_Seal1 = "AAAvvvSSS";
			container.BC_Seal2 = "BBBxxxPPP";
			AssertNoMessageError(container.BC_Seal1Info, ValidationConstants.Container.MaxLengthOfSealNoExceeded.ToString());
			AssertNoMessageError(container.BC_Seal2Info, ValidationConstants.Container.MaxLengthOfSealNoExceeded.ToString());

			container.BC_Seal1 = "AAAAAvvvvvSSSSSppppp";
			container.BC_Seal2 = "BBBBBxxxxxPPPPPfffff";
			AssertHasMessageError(container.BC_Seal1Info, ValidationConstants.Container.MaxLengthOfSealNoExceeded.ToString());
			AssertHasMessageError(container.BC_Seal2Info, ValidationConstants.Container.MaxLengthOfSealNoExceeded.ToString());
		}

		public void TestValidateMaximumNumberOfUNDG()
		{
			bill.ValidationModes = ValidationModes.InventoryRecord;
			var commodity = container.Commodities.AddNew();
			container.Validation.ValidateAll();
			AssertNoRowMessageError(container, ValidationConstants.Container.MaximumNumberOfUNDGExceeded);
			for (var i = 1; i < 100; i++)
			{
				container.UNDGs.AddNew();
				container.Validation.ValidateAll();
				AssertNoRowMessageError(container, ValidationConstants.Container.MaximumNumberOfUNDGExceeded);
			}
			var undg1 = container.UNDGs.AddNew();
			container.Validation.ValidateAll();
			AssertHasRowMessageError(container, ValidationConstants.Container.MaximumNumberOfUNDGExceeded);
			var undg2 = container.UNDGs.AddNew();
			container.Validation.ValidateAll();
			AssertHasRowMessageError(container, ValidationConstants.Container.MaximumNumberOfUNDGExceeded);

			undg1.Delete();
			container.Validation.ValidateAll();
			AssertHasRowMessageError(container, ValidationConstants.Container.MaximumNumberOfUNDGExceeded);

			bill.ValidationModes = ValidationModes.None;
			container.Validation.ValidateAll();
			AssertNoRowMessageError(container, ValidationConstants.Container.MaximumNumberOfUNDGExceeded);

			bill.ValidationModes = ValidationModes.InventoryRecord;
			container.Validation.ValidateAll();
			AssertHasRowMessageError(container, ValidationConstants.Container.MaximumNumberOfUNDGExceeded);

			undg2.Delete();
			container.Validation.ValidateAll();
			AssertNoRowMessageError(container, ValidationConstants.Container.MaximumNumberOfUNDGExceeded);
		}

		public void TestCheckBC_ContainerNum()
		{
			bill.ValidationModes = ValidationModes.InventoryRecord;
			container.BC_ContainerNum = ZString.Empty;
			AssertHasMessageError(container.BC_ContainerNumInfo, ValidationConstants.Container.ContainerNumberIsRequired.ToString());

			container.BC_ContainerNum = "TURE123456";
			AssertNoMessageError(container.BC_ContainerNumInfo, ValidationConstants.Container.ContainerNumberIsRequired.ToString());

			var container2 = moveDetail.Containers.AddNew();
			container2.BC_ContainerNum = "TURE123456";
			AssertHasError(container2.BC_ContainerNumInfo, ValidationConstants.Container.ContainerNumberIsDuplicated);

			container2.BC_ContainerNum = "TURE123589";
			AssertNoError(container2.BC_ContainerNumInfo, ValidationConstants.Container.ContainerNumberIsDuplicated);
			AssertHasWarning(container.BC_ContainerNumInfo, "Container number does not conform to ISO standard of 4 letters followed by 6 digits and a check digit.");

			container.BC_ContainerNum = "    F ALSE";
			AssertHasMessageError(container.BC_ContainerNumInfo, ValidationConstants.Container.ContainerNumberCanNotContainLeadingSpaces.ToString());
			container.BC_ContainerNum = " F ALSE";
			AssertHasMessageError(container.BC_ContainerNumInfo, ValidationConstants.Container.ContainerNumberCanNotContainLeadingSpaces.ToString());
			container.BC_ContainerNum = "F ALSE";
			AssertNoMessageError(container.BC_ContainerNumInfo, ValidationConstants.Container.ContainerNumberCanNotContainLeadingSpaces.ToString());

			container.BC_ContainerNum = "!";
			AssertHasMessageError(container.BC_ContainerNumInfo, ValidationConstants.Container.MustOnlyContainAlphaNumerics.ToString());
			AssertNoWarning(container.BC_ContainerNumInfo, "Container number does not conform to ISO standard of 4 letters followed by 6 digits and a check digit.");

			container.BC_ContainerNum = "NC NC1";
			AssertNoMessageError("Should allow spaces", container.BC_ContainerNumInfo, ValidationConstants.Container.MustOnlyContainAlphaNumerics.ToString());

			container.BC_ContainerNum = "TURE3923422";
			var invalidCheckDigit = "Container number does not have a valid check (last) digit. The check digit should be 4.";
			AssertNoWarning(container.BC_ContainerNumInfo, "Container number does not conform to ISO standard of 4 letters followed by 6 digits and a check digit.");
			AssertHasWarning(container.BC_ContainerNumInfo, invalidCheckDigit);
			container.BC_ContainerNum = "TURE3923424";
			AssertNoWarning(container.BC_ContainerNumInfo, invalidCheckDigit);

			container.BC_ContainerNum = "NC";
			AssertNoNotifications(container.BC_ContainerNumInfo);

			bill.ValidationModes = ValidationModes.None;
			container2.BC_ContainerNum = "TURE123456";
			container.BC_ContainerNum = ZString.Empty;
			AssertNoMessageError(container.BC_ContainerNumInfo, ValidationConstants.Container.ContainerNumberIsRequired.ToString());
			container.BC_ContainerNum = "TURE123456";
			AssertHasError(container.BC_ContainerNumInfo, ValidationConstants.Container.ContainerNumberIsDuplicated);
			container.BC_ContainerNum = "TURE123589";
			AssertNoWarning(container.BC_ContainerNumInfo, "Container number does not conform to ISO standard of 4 letters followed by 6 digits and a check digit.");
			container.BC_ContainerNum = "TURE3923422";
			AssertNoWarning(container.BC_ContainerNumInfo, invalidCheckDigit);
			container.BC_ContainerNum = "!";
			AssertNoMessageError(container.BC_ContainerNumInfo, ValidationConstants.Container.MustOnlyContainAlphaNumerics.ToString());
		}

		public void TestCheckBC_SealNumbers()
		{
			container.BC_Seal1 = "TURE123456";
			container.BC_Seal2 = "TURE123422";
			AssertNoWarning("conatainer1 seal1", container.BC_Seal1Info, "Container Seal 1 : US Customs only accepts standard English alphabetic characters; invalid characters, including some punctuation and foreign characters, will be replaced with a question mark '?'.");
			AssertNoWarning("conatainer1 seal2", container.BC_Seal2Info, "Container Seal 2 : US Customs only accepts standard English alphabetic characters; invalid characters, including some punctuation and foreign characters, will be replaced with a question mark '?'.");

			container.BC_Seal1 = "TUR¢123422";
			container.BC_Seal2 = "TUR¢123423";
			AssertHasWarning("conatainer2 seal1", container.BC_Seal1Info, "Container Seal 1 : US Customs only accepts standard English alphabetic characters; invalid characters, including some punctuation and foreign characters, will be replaced with a question mark '?'.");
			AssertHasWarning("conatainer2 seal2", container.BC_Seal2Info, "Container Seal 2 : US Customs only accepts standard English alphabetic characters; invalid characters, including some punctuation and foreign characters, will be replaced with a question mark '?'.");
		}

		public void TestCheckBC_RC()
		{
			var carrier1 = Factory.NewWithValidTestData<USCarrierCombined>();
			carrier1.UI_Code = "NCKZ";

			var carrier2 = Factory.NewWithValidTestData<USCarrierCombined>();
			carrier2.UI_Code = "NCA";

			var carrier3 = Factory.NewWithValidTestData<USCarrierCombined>();
			carrier3.UI_Code = "ADNC";

			Factory.Save();

			bill.ValidationModes = ValidationModes.InventoryRecord;
			var containerType = Factory.New<MasterFiles.Business.RefContainer>();
			containerType.RC_Code = "40ZZ";

			container.BC_RC = ZGuid.Invalid;
			AssertHasErrorContaining(container.BC_RCInfo, ListValidation.InvalidCodeError);

			container.BC_RC = containerType.PK;
			container.BC_ContainerNum = "TURE2123432";
			AssertNoErrorContaining(container.BC_RCInfo, ListValidation.InvalidCodeError);

			container.BC_RC = ZGuid.Empty;
			AssertNoErrorContaining(container.BC_RCInfo, ListValidation.InvalidCodeError);

			container.BC_ContainerNum = CusInBondContainer.NonContainerizedNumber;
			AssertNoErrorContaining(container.BC_RCInfo, ListValidation.InvalidCodeError);

			container.BC_RC = containerType.PK;
			AssertNoErrorContaining(container.BC_RCInfo, ListValidation.InvalidCodeError);

			bill.ValidationModes = ValidationModes.None;
			container.BC_RC = ZGuid.Invalid;
			AssertHasErrorContaining(container.BC_RCInfo, ListValidation.InvalidCodeError);

			container.BC_RC = ZGuid.Empty;
			AssertNoErrorContaining(container.BC_RCInfo, ListValidation.InvalidCodeError);

			bill.ValidationModes = ValidationModes.InventoryRecord;
			container.BC_ContainerNum = "APLU123456";
			container.BC_RC = containerType.PK;
			container.BC_RC = ZGuid.Empty;
			AssertHasMessageError(container.BC_RCInfo, ValidationConstants.Container.MissingUSContainerCode.ToString());
			container.BC_RC = containerType.PK;
			AssertHasMessageError(container.BC_RCInfo, ValidationConstants.Container.MissingUSContainerCode.ToString());

			container.BC_ContainerNum = "NCA4564785";
			container.BC_RC = ZGuid.Empty;
			container.BC_RC = containerType.PK;
			AssertNoMessageError(container.BC_RCInfo, ValidationConstants.Container.MissingUSContainerCode.ToString());

			container.BC_ContainerNum = "NCAD123456";
			container.BC_RC = ZGuid.Empty;
			container.BC_RC = containerType.PK;
			AssertNoMessageError(container.BC_RCInfo, ValidationConstants.Container.MissingUSContainerCode.ToString());

			container.BC_ContainerNum = "ADNC";
			container.BC_RC = ZGuid.Empty;
			container.BC_RC = containerType.PK;
			AssertHasMessageError(container.BC_RCInfo, ValidationConstants.Container.MissingUSContainerCode.ToString());

			container.BC_ContainerNum = "NC#####";
			container.BC_RC = ZGuid.Empty;
			container.BC_RC = containerType.PK;
			AssertNoMessageError(container.BC_RCInfo, ValidationConstants.Container.MissingUSContainerCode.ToString());

			container.BC_ContainerNum = CusInBondContainer.NonContainerizedNumber;
			container.BC_RC = ZGuid.Empty;
			container.BC_RC = containerType.PK;
			AssertNoMessageError(container.BC_RCInfo, ValidationConstants.Container.MissingUSContainerCode.ToString());

			container.BC_ContainerNum = "NCKZ123456";
			container.BC_RC = ZGuid.Empty;
			container.BC_RC = containerType.PK;
			AssertHasMessageError(container.BC_RCInfo, ValidationConstants.Container.MissingUSContainerCode.ToString());

			container.BC_ContainerNum = "NCKZ";
			containerType.SetCountrySpecificContainerCode(USContainerCodeList.Codes._20, Enterprise.Core.Constants.CountryCodes.UnitedStates);
			container.BC_RC = ZGuid.Empty;
			container.BC_RC = containerType.PK;
			AssertNoMessageError(container.BC_RCInfo, ValidationConstants.Container.MissingUSContainerCode.ToString());
		}

		public void TestCheckBC_ForeignPortKCode()
		{
			bill.ValidationModes = ValidationModes.InventoryRecord;
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Port, "PORT");
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Port, "Z11Z3", "Test", ZDateTime.BrettsBirthday, ZDateTime.MaxSmallDateTime);
			Factory.Save();

			container.BC_IsEmpty = ZBool.True;
			container.BC_ForeignPortKCode = ZString.Empty;
			AssertHasMessageError(container.BC_ForeignPortKCodeInfo, ValidationConstants.Container.ForeignPortSchKIsRequired.ToString());
			AssertNoMessageErrorContaining(container.BC_ForeignPortKCodeInfo, ListValidation.InvalidCodeMessageError);

			container.BC_IsEmpty = ZBool.False;
			AssertNoMessageError(container.BC_ForeignPortKCodeInfo, ValidationConstants.Container.ForeignPortSchKIsRequired.ToString());
			AssertNoMessageErrorContaining(container.BC_ForeignPortKCodeInfo, ListValidation.InvalidCodeMessageError);

			container.BC_ForeignPortKCode = "Z!@3!";
			AssertNoMessageError(container.BC_ForeignPortKCodeInfo, ValidationConstants.Container.ForeignPortSchKIsRequired.ToString());
			AssertHasMessageErrorContaining(container.BC_ForeignPortKCodeInfo, ListValidation.InvalidCodeMessageError);

			container.BC_IsEmpty = ZBool.True;
			AssertNoMessageError(container.BC_ForeignPortKCodeInfo, ValidationConstants.Container.ForeignPortSchKIsRequired.ToString());
			AssertHasMessageErrorContaining(container.BC_ForeignPortKCodeInfo, ListValidation.InvalidCodeMessageError);

			container.BC_ForeignPortKCode = "Z11Z3";
			AssertNoMessageError(container.BC_ForeignPortKCodeInfo, ValidationConstants.Container.ForeignPortSchKIsRequired.ToString());
			AssertNoMessageErrorContaining(container.BC_ForeignPortKCodeInfo, ListValidation.InvalidCodeMessageError);

			bill.ValidationModes = ValidationModes.None;
			container.BC_IsEmpty = ZBool.True;
			container.BC_ForeignPortKCode = ZString.Empty;
			AssertNoMessageError(container.BC_ForeignPortKCodeInfo, ValidationConstants.Container.ForeignPortSchKIsRequired.ToString());
			AssertNoMessageErrorContaining(container.BC_ForeignPortKCodeInfo, ListValidation.InvalidCodeMessageError);

			container.BC_ForeignPortKCode = "Z!@3!";
			AssertNoMessageError(container.BC_ForeignPortKCodeInfo, ValidationConstants.Container.ForeignPortSchKIsRequired.ToString());
			AssertNoMessageErrorContaining(container.BC_ForeignPortKCodeInfo, ListValidation.InvalidCodeMessageError);
		}

		public void TestCheckBC_IsEmpty()
		{
			bill.ValidationModes = ValidationModes.InventoryRecord;
			bill.B0_BillStatus = BillOfLadingStatusIndicatorList.Codes.EmptyEquipmentInstrumentsOfInternationalTrade;
			container.BC_IsEmpty = ZBool.False;
			AssertHasMessageError(container.BC_IsEmptyInfo, ValidationConstants.Container.NotEmptyWhenBillStatusIsEmptyContainer.ToString());
			container.BC_IsEmpty = ZBool.True;
			AssertNoMessageError(container.BC_IsEmptyInfo, ValidationConstants.Container.NotEmptyWhenBillStatusIsEmptyContainer.ToString());
		}

		public void TestCheckBC_RL_NKForeignPort()
		{
			bill.ValidationModes = ValidationModes.InventoryRecord;
			container.BC_RL_NKForeignPort = ZString.Empty;
			AssertNoMessageErrors(container.BC_RL_NKForeignPortInfo);

			container.BC_RL_NKForeignPort = "AU!!@";
			AssertNoMessageErrors(container.BC_RL_NKForeignPortInfo);
			AssertHasWarningContaining(container.BC_RL_NKForeignPortInfo, ListValidation.InvalidCodeMessage);

			container.BC_RL_NKForeignPort = "AUSYD";
			AssertNoMessageErrors(container.BC_RL_NKForeignPortInfo);
			AssertNoWarningContaining(container.BC_RL_NKForeignPortInfo, ListValidation.InvalidCodeMessage);

			bill.ValidationModes = ValidationModes.None;
			container.BC_RL_NKForeignPort = "AU!!@";
			AssertNoMessageErrors(container.BC_RL_NKForeignPortInfo);
			AssertNoWarningContaining(container.BC_RL_NKForeignPortInfo, ListValidation.InvalidCodeMessage);
		}

		public void TestCheckBC_TypeOfService()
		{
			bill.ValidationModes = ValidationModes.InventoryRecord;
			container.BC_TypeOfService = ZString.Empty;
			AssertNoMessageErrors(container);

			container.BC_TypeOfService = "Z!";
			AssertHasMessageErrorContaining(container.BC_TypeOfServiceInfo, ListValidation.InvalidCodeMessageError);

			container.BC_TypeOfService = ServiceTypeList.Codes.HeadloadOrDevanning;
			AssertNoMessageErrorContaining(container.BC_TypeOfServiceInfo, ListValidation.InvalidCodeMessageError);
			AssertNoMessageErrors(container);

			bill.ValidationModes = ValidationModes.None;
			container.BC_TypeOfService = "Z!";
			AssertNoMessageErrorContaining(container.BC_TypeOfServiceInfo, ListValidation.InvalidCodeMessageError);
		}

		CusInBondHeader header;
		CusInBondBill bill;
		CusInBondMoveDetail moveDetail;
		CusInBondContainer container;
		protected override void SetUp()
		{
			base.SetUp();
			header = Factory.New<CusInBondHeader>();
			bill = header.Bills.AddNew();
			moveDetail = bill.MovementDetail;
			container = moveDetail.Containers.AddNew();
		}
	}
}
