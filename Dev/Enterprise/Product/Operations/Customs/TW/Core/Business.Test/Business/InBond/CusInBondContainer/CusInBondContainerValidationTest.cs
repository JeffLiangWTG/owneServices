using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.TW.Business.Testing
{
	sealed class CusInBondContainerValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckBC_ContainerNum()
		{
			RefContainer containerType = Factory.New<RefContainer>();
			containerType.RC_Code = "40ZZ";
			container.BC_Seal1 = ZString.Empty;
			container.BC_Mode = ZString.Empty;
			CombineAssertions("BC_RC", () =>
			{
				container.BC_RC = containerType.PK;
				container.BC_ContainerNum = ZString.Empty;
				AssertHasMessageErrorContaining(container.BC_ContainerNumInfo, MandatoryValidation.YouHaveNotEntered);
				container.BC_ContainerNum = "XX1111";
				AssertNoMessageErrorContaining(container.BC_ContainerNumInfo, MandatoryValidation.YouHaveNotEntered);
				container.BC_RC = ZGuid.Empty;
				container.BC_ContainerNum = ZString.Empty;
				AssertHasMessageErrorContaining(container.BC_ContainerNumInfo, MandatoryValidation.YouHaveNotEntered);
			}

			);
			container.BC_RC = ZGuid.Empty;
			container.BC_Mode = ZString.Empty;
			CombineAssertions("BC_Seal1", () =>
			{
				container.BC_Seal1 = "ABC111";
				container.BC_ContainerNum = ZString.Empty;
				AssertHasMessageErrorContaining(container.BC_ContainerNumInfo, MandatoryValidation.YouHaveNotEntered);
				container.BC_ContainerNum = "XX1111";
				AssertNoMessageErrorContaining(container.BC_ContainerNumInfo, MandatoryValidation.YouHaveNotEntered);
				container.BC_Seal1 = ZString.Empty;
				container.BC_ContainerNum = ZString.Empty;
				AssertHasMessageErrorContaining(container.BC_ContainerNumInfo, MandatoryValidation.YouHaveNotEntered);
			}

			);
			container.BC_RC = ZGuid.Empty;
			container.BC_Seal1 = ZString.Empty;
			CombineAssertions("BC_Mode", () =>
			{
				container.BC_Mode = CusInBondContainerModeList.Codes.FCL;
				container.BC_ContainerNum = ZString.Empty;
				AssertHasMessageErrorContaining(container.BC_ContainerNumInfo, MandatoryValidation.YouHaveNotEntered);
				container.BC_ContainerNum = "XX1111";
				AssertNoMessageErrorContaining(container.BC_ContainerNumInfo, MandatoryValidation.YouHaveNotEntered);
				container.BC_Mode = ZString.Empty;
				container.BC_ContainerNum = ZString.Empty;
				AssertHasMessageErrorContaining(container.BC_ContainerNumInfo, MandatoryValidation.YouHaveNotEntered);
			}

			);
			container.BC_ContainerNum = "~!ABC1";
			AssertHasMessageError(container.BC_ContainerNumInfo, ValidationConstants.CusInBondContainer.ContainerNumberAlphanumericCharactersOnly);
			var header = Factory.New<CusInBondHeader>();
			var moveDetail = header.MovementHeaders.AddNew().InBondMoveDetail;
			var container1 = moveDetail.Containers.AddNew();
			container1.BC_ContainerNum = "001";
			AssertNoMessageError(container1.BC_ContainerNumInfo, ValidationConstants.CusInBondContainer.ContainerNumberAlreadyExists);
			container1 = moveDetail.Containers.AddNew();
			container1.BC_ContainerNum = "001";
			AssertHasMessageError(container1.BC_ContainerNumInfo, ValidationConstants.CusInBondContainer.ContainerNumberAlreadyExists);
			var isoFormatWarning = "Container number does not conform to ISO standard of 4 letters followed by 6 digits and a check digit.";
			container.BC_ContainerNum = "123456123456";
			AssertHasWarning(container.BC_ContainerNumInfo, isoFormatWarning);
			container.BC_ContainerNum = "QWER1234561";
			AssertNoWarning(container.BC_ContainerNumInfo, isoFormatWarning);
		}

		public void TestCheckBC_RC()
		{
			RefContainer containerType = Factory.New<RefContainer>();
			containerType.RC_Code = "40ZZ";
			container.BC_RC = containerType.PK;
			container.BC_ContainerNum = "TURE2123432";
			AssertNoErrorContaining(container.BC_RCInfo, ListValidation.InvalidCodeError);
			container.BC_RC = ZGuid.Invalid;
			AssertHasErrorContaining(container.BC_RCInfo, ListValidation.InvalidCodeError);
			container.BC_RC = ZGuid.Empty;
			AssertNoErrorContaining(container.BC_RCInfo, ListValidation.InvalidCodeError);
			var targetInfo = container.BC_RCInfo;
			container.BC_ContainerNum = ZString.Empty;
			container.BC_Mode = ZString.Empty;
			container.BC_RC = ZGuid.Empty;
			container.Validation.ValidateBC_RC();
			AssertHasMessageErrorContaining(targetInfo, MandatoryValidation.YouHaveNotEntered);
			container.BC_ContainerNum = "111";
			container.BC_Mode = ZString.Empty;
			container.BC_RC = ZGuid.Empty;
			container.Validation.ValidateBC_RC();
			AssertHasMessageErrorContaining(targetInfo, MandatoryValidation.YouHaveNotEntered);
			container.BC_ContainerNum = ZString.Empty;
			container.BC_Mode = CusInBondContainerModeList.Codes.BCN;
			container.BC_RC = ZGuid.Empty;
			container.Validation.ValidateBC_RC();
			AssertHasMessageErrorContaining(targetInfo, MandatoryValidation.YouHaveNotEntered);
			container.BC_ContainerNum = "111";
			container.BC_Mode = CusInBondContainerModeList.Codes.BCN;
			container.BC_RC = ZGuid.Empty;
			container.Validation.ValidateBC_RC();
			AssertHasMessageErrorContaining(targetInfo, MandatoryValidation.YouHaveNotEntered);
			container.BC_RC = containerType.PK;
			container.Validation.ValidateBC_RC();
			AssertNoMessageErrorContaining(targetInfo, MandatoryValidation.YouHaveNotEntered);
		}

		public void TestCheckBC_Seal1()
		{
			container.BC_ContainerNum = "TURE2123432";
			container.BC_Seal1 = "SL123";
			AssertNoMessageErrorContaining(container.BC_Seal1Info, ValidationConstants.AlphanumericCharactersOnly(container.BC_Seal1Info.HumanReadableName));
			container.BC_Seal1 = "!~333A";
			AssertHasMessageErrorContaining(container.BC_Seal1Info, ValidationConstants.AlphanumericCharactersOnly(container.BC_Seal1Info.HumanReadableName));
		}

		public void TestCheckBC_Mode()
		{
			container.BC_Mode = "AX";
			AssertHasMessageError(container.BC_ModeInfo, ListValidation.InvalidCodeMessageError);
			container.BC_Mode = CusInBondContainerModeList.Codes.BCN;
			AssertNoMessageError(container.BC_ModeInfo, ListValidation.InvalidCodeMessageError);
			var targetInfo = container.BC_ModeInfo;
			container.BC_ContainerNum = ZString.Empty;
			container.BC_RC = ZGuid.Empty;
			container.BC_Mode = ZString.Empty;
			AssertHasMessageErrorContaining(targetInfo, MandatoryValidation.YouHaveNotEntered);
			container.BC_ContainerNum = "111";
			container.BC_RC = ZGuid.Empty;
			container.BC_Mode = ZString.Empty;
			AssertHasMessageErrorContaining(targetInfo, MandatoryValidation.YouHaveNotEntered);
			container.BC_ContainerNum = ZString.Empty;
			container.BC_RC = ZGuid.NewZGuid();
			container.BC_Mode = ZString.Empty;
			AssertHasMessageErrorContaining(targetInfo, MandatoryValidation.YouHaveNotEntered);
			container.BC_ContainerNum = "111";
			container.BC_RC = ZGuid.NewZGuid();
			container.BC_Mode = ZString.Empty;
			AssertHasMessageErrorContaining(targetInfo, MandatoryValidation.YouHaveNotEntered);
			container.BC_Mode = CusInBondContainerModeList.Codes.BCN;
			AssertNoMessageErrorContaining(targetInfo, MandatoryValidation.YouHaveNotEntered);
		}

		protected override void SetUp()
		{
			base.SetUp();
			header = Factory.New<CusInBondHeader>();
			moveHeader = header.MovementHeaders.AddNew();
			moveDetail = moveHeader.MovementDetails.AddNew();
			container = moveDetail.Containers.AddNew();
		}

		CusInBondHeader header;
		CusInBondMoveHeader moveHeader;
		CusInBondMoveDetail moveDetail;
		CusInBondContainer container;
	}
}
