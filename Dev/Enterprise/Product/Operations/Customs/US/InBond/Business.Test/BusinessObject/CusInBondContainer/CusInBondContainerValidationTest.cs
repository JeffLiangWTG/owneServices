using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.US.InBond.Business.Testing
{
	sealed class CusInBondContainerValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckBC_ContainerNum()
		{
			CusInBondHeader header = Factory.New<CusInBondHeader>();
			header.BH_HeaderType = InBondHeaderTypeList.Codes.AMS;
			CusInBondMoveHeader moveHeader = header.MovementHeaders.AddNew();
			CusInBondMoveDetail moveDetail = moveHeader.MovementDetails.AddNew();
			CusInBondContainer container = moveDetail.Containers.AddNew();
			container.BC_ContainerNum = ZString.Empty;
			AssertNoMessageError(container.BC_ContainerNumInfo, ValidationConstants.Container.ContainerNumberIsRequired.ToString());
			header.BH_HeaderType = InBondHeaderTypeList.Codes.FullData;
			container.BC_ContainerNum = ZString.Empty;
			AssertHasMessageError(container.BC_ContainerNumInfo, ValidationConstants.Container.ContainerNumberIsRequired.ToString());
			container.BC_ContainerNum = "TURE123456";
			AssertNoMessageError(container.BC_ContainerNumInfo, ValidationConstants.Container.ContainerNumberIsRequired.ToString());
			CusInBondContainer container2 = moveDetail.Containers.AddNew();
			container2.BC_ContainerNum = "TURE123456";
			AssertHasError(container2.BC_ContainerNumInfo, ValidationConstants.Container.ContainerNumberIsDuplicated);
			container2.BC_ContainerNum = "TURE123589";
			AssertNoError(container2.BC_ContainerNumInfo, ValidationConstants.Container.ContainerNumberIsDuplicated);
			AssertHasWarning(container.BC_ContainerNumInfo, "Container number does not conform to ISO standard of 4 letters followed by 6 digits and a check digit.");
			container.BC_ContainerNum = "!";
			AssertHasMessageError(container.BC_ContainerNumInfo, ValidationConstants.Container.MustOnlyContainAlphaNumerics.ToString());
			AssertNoWarning(container.BC_ContainerNumInfo, "Container number does not conform to ISO standard of 4 letters followed by 6 digits and a check digit.");
			container.BC_ContainerNum = "TURE3923422";
			string invalidCheckDigit = "Container number does not have a valid check (last) digit. The check digit should be 4.";
			AssertNoWarning(container.BC_ContainerNumInfo, "Container number does not conform to ISO standard of 4 letters followed by 6 digits and a check digit.");
			AssertHasWarning(container.BC_ContainerNumInfo, invalidCheckDigit);
			container.BC_ContainerNum = "TURE3923424";
			AssertNoWarning(container.BC_ContainerNumInfo, invalidCheckDigit);
			container.BC_ContainerNum = "NC";
			AssertNoNotifications(container.BC_ContainerNumInfo);
		}

		public void TestValidateAtLeastOneMovementDetailsExist()
		{
			CusInBondHeader header = Factory.New<CusInBondHeader>();
			header.BH_HeaderType = InBondHeaderTypeList.Codes.FullData;
			CusInBondMoveHeader moveHeader = header.MovementHeaders.AddNew();
			CusInBondMoveDetail moveDetail = moveHeader.MovementDetails.AddNew();
			CusInBondContainer container = moveDetail.Containers.AddNew();
			container.Validation.ValidateAll();
			AssertHasRowMessageError(container, ValidationConstants.Container.AtLeastOneCommodityIsRequired);
			AssertNoRowWarningContaining(container, ValidationConstants.Container.AtLeastOneCommodityIsRequired);
			CusInBondCargoDesc commodity = container.Commodities.AddNew();
			container.Validation.ValidateAll();
			AssertNoRowMessageError(container, ValidationConstants.Container.AtLeastOneCommodityIsRequired);
			commodity.Delete();
			header.BH_HeaderType = InBondHeaderTypeList.Codes.AMS;
			container.Validation.ValidateAll();
			AssertHasRowWarning(container, ValidationConstants.Container.AtLeastOneCommodityIsRequired);
			AssertNoRowMessageError(container, ValidationConstants.Container.AtLeastOneCommodityIsRequired);
			commodity = container.Commodities.AddNew();
			container.Validation.ValidateAll();
			AssertNoRowWarningContaining(container, ValidationConstants.Container.AtLeastOneCommodityIsRequired);
			commodity.Delete();
			header.BH_HeaderType = InBondHeaderTypeList.Codes.DocumentOnly;
			container.Validation.ValidateAll();
			AssertHasRowWarning(container, ValidationConstants.Container.AtLeastOneCommodityIsRequired);
			AssertNoRowMessageError(container, ValidationConstants.Container.AtLeastOneCommodityIsRequired);
			commodity = container.Commodities.AddNew();
			container.Validation.ValidateAll();
			AssertNoRowWarningContaining(container, ValidationConstants.Container.AtLeastOneCommodityIsRequired);
		}

		public void TestValidateUNDGsNotExceedLimit()
		{
			CusInBondHeader header = Factory.New<CusInBondHeader>();
			CusInBondMoveHeader moveHeader = header.MovementHeaders.AddNew();
			CusInBondMoveDetail moveDetail = moveHeader.MovementDetails.AddNew();
			CusInBondContainer container = moveDetail.Containers.AddNew();
			container.Validation.ValidateAll();
			AssertNoRowMessageError(container, ValidationConstants.Container.MaximumNumberOfUNDGExceeded);
			for (int i = 1; i < 100; i++)
			{
				var undg = container.UNDGs.AddNew();
				container.Validation.ValidateAll();
				AssertNoRowMessageError(container, ValidationConstants.Container.MaximumNumberOfUNDGExceeded);
			}

			var undgExtra = container.UNDGs.AddNew();
			container.Validation.ValidateAll();
			AssertHasRowMessageError(container, ValidationConstants.Container.MaximumNumberOfUNDGExceeded);
		}

		public void TestCheckBC_RC()
		{
			RefContainer containerType = Factory.New<RefContainer>();
			containerType.RC_Code = "40ZZ";
			CusInBondHeader header = Factory.New<CusInBondHeader>();
			header.BH_HeaderType = InBondHeaderTypeList.Codes.FullData;
			CusInBondMoveHeader moveHeader = header.MovementHeaders.AddNew();
			CusInBondMoveDetail moveDetail = moveHeader.MovementDetails.AddNew();
			CusInBondContainer container = moveDetail.Containers.AddNew();
			container.BC_RC = containerType.PK;
			container.BC_ContainerNum = "TURE2123432";
			AssertNoMessageErrorContaining(container.BC_RCInfo, MandatoryValidation.YouHaveNotEntered);
			AssertNoErrorContaining(container.BC_RCInfo, ListValidation.InvalidCodeError);
			container.BC_RC = ZGuid.Invalid;
			AssertNoMessageErrorContaining(container.BC_RCInfo, MandatoryValidation.YouHaveNotEntered);
			AssertHasErrorContaining(container.BC_RCInfo, ListValidation.InvalidCodeError);
			container.BC_RC = ZGuid.Empty;
			AssertHasMessageErrorContaining(container.BC_RCInfo, MandatoryValidation.YouHaveNotEntered);
			AssertNoErrorContaining(container.BC_RCInfo, ListValidation.InvalidCodeError);
			container.BC_ContainerNum = CusInBondContainer.NonContainerizedNumber;
			AssertNoNotifications(container.BC_RCInfo);
			container.BC_RC = containerType.PK;
			AssertNoNotifications(container.BC_RCInfo);
			header.BH_HeaderType = InBondHeaderTypeList.Codes.AMS;
			container.BC_ContainerNum = "TURE2123432";
			container.BC_RC = ZGuid.Empty;
			AssertNoMessageErrorContaining(container.BC_RCInfo, MandatoryValidation.YouHaveNotEntered);
			AssertNoErrorContaining(container.BC_RCInfo, ListValidation.InvalidCodeError);
		}

		public void TestCheckBC_Seal1()
		{
			CusInBondHeader header = Factory.New<CusInBondHeader>();
			header.BH_HeaderType = InBondHeaderTypeList.Codes.AMS;
			CusInBondMoveHeader moveHeader = header.MovementHeaders.AddNew();
			CusInBondMoveDetail moveDetail = moveHeader.MovementDetails.AddNew();
			CusInBondContainer container = moveDetail.Containers.AddNew();
			container.BC_ContainerNum = "TURE2123432";
			container.BC_Seal1 = "SL123";
			AssertNoMessageErrorContaining(container.BC_Seal1Info, MandatoryValidation.YouHaveNotEntered);
			container.BC_Seal1 = ZString.Empty;
			AssertNoMessageErrorContaining(container.BC_Seal1Info, MandatoryValidation.YouHaveNotEntered);
			header.BH_HeaderType = InBondHeaderTypeList.Codes.FullData;
			container.BC_Seal1 = ZString.Empty;
			AssertHasMessageErrorContaining(container.BC_Seal1Info, MandatoryValidation.YouHaveNotEntered);
			container.BC_ContainerNum = CusInBondContainer.NonContainerizedNumber;
			AssertNoNotifications(container.BC_Seal1Info);
			container.BC_Seal1 = "SL123";
			AssertNoNotifications(container.BC_Seal1Info);
			container.BC_Seal1 = "AAAAABBBBBCCCCCDDDDD";
			AssertHasMessageErrorContaining(container.BC_Seal1Info, ValidationConstants.Container.MaxLengthOfSealExceeded.ToString());
		}

		public void TestCheckBC_Seal2()
		{
			CusInBondHeader header = Factory.New<CusInBondHeader>();
			CusInBondMoveHeader moveHeader = header.MovementHeaders.AddNew();
			CusInBondMoveDetail moveDetail = moveHeader.MovementDetails.AddNew();
			CusInBondContainer container = moveDetail.Containers.AddNew();
			container.BC_ContainerNum = CusInBondContainer.NonContainerizedNumber;
			container.BC_Seal2 = "SL223";
			AssertNoNotifications(container.BC_Seal2Info);
			container.BC_ContainerNum = "TURE2223432";
			AssertNoNotifications(container.BC_Seal2Info);
			container.BC_Seal2 = ZString.Empty;
			AssertNoNotifications(container.BC_Seal2Info);
			container.BC_Seal2 = "AAAAABBBBBCCCCCDDDDD";
			AssertHasMessageErrorContaining(container.BC_Seal2Info, ValidationConstants.Container.MaxLengthOfSealExceeded.ToString());
		}
	}
}
