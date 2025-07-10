using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.ZA.Business.Testing
{
	sealed class CusEntryLineValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckCL_LineNumber()
		{
			var entryLine = Factory.New<CusEntryLine>();
			entryLine.CL_LineNumber = 10000;
			AssertHasMessageErrorContaining(entryLine.CL_LineNumberInfo, ValidationConstants.Shared.EntryLineNumberExceedMax);
			entryLine.CL_LineNumber = 9999;
			AssertNoMessageErrorContaining(entryLine.CL_LineNumberInfo, ValidationConstants.Shared.EntryLineNumberExceedMax);
		}

		public void TestValidateDiamondLevyValueAndAmount()
		{
			var diamondLevyError = CusEntryLineValidation.DiamondLevyValueAndAmountRequiredError;
			var dec = Factory.New<JobDeclaration>();
			dec.JE_MessageType = "EXP";
			var entryLine = dec.CustomsEntryHeaders.AddNew().MergedLines.AddNew();
			var additionalInfo = entryLine.AdditionalInformationCodes.AddNew();
			AssertNoRowMessageErrorContaining(entryLine, diamondLevyError);
			additionalInfo.CY_Code = "DLV";
			AssertHasRowMessageErrorContaining(entryLine, diamondLevyError);
			additionalInfo.CY_Code = "ABC";
			AssertNoRowMessageErrorContaining(entryLine, diamondLevyError);
			var provisionalPayment = entryLine.ProvisionalPayments.AddNew();
			provisionalPayment.CY_Code = "DLA";
			additionalInfo.CY_Code = "DLV";
			AssertNoRowMessageErrorContaining(entryLine, diamondLevyError);
			entryLine.ProvisionalPayments.RemoveAndDelete(provisionalPayment);
			AssertHasRowMessageErrorContaining(entryLine, diamondLevyError);
			provisionalPayment = entryLine.ProvisionalPayments.AddNew();
			provisionalPayment.CY_Code = "DLA";
			AssertNoRowMessageErrorContaining(entryLine, diamondLevyError);
			entryLine.AdditionalInformationCodes.RemoveAndDelete(additionalInfo);
			AssertHasRowMessageErrorContaining(entryLine, diamondLevyError);
			dec.JE_MessageType = "IMP";
			entryLine.Validation.ValidateAll();
			AssertNoRowMessageErrorContaining(entryLine, diamondLevyError);
		}

		public void TestValidateNotMoreThanTenAdditionalInfos()
		{
			var noMoreThanTenAdditionalInfosError = CusEntryLineValidation.NoMoreThanTenAdditionalInfos;
			var dec = Factory.New<JobDeclaration>();
			dec.JE_MessageType = "EXP";
			var entryLine = dec.CustomsEntryHeaders.AddNew().MergedLines.AddNew();
			var additionalInfo = entryLine.AdditionalInformationCodes.AddNew();
			entryLine.Validation.ValidateAll();
			AssertNoRowMessageErrorContaining(entryLine, noMoreThanTenAdditionalInfosError);
			for (int i = 0; i < 10; i++)
			{
				additionalInfo = entryLine.AdditionalInformationCodes.AddNew();
			}

			entryLine.Validation.ValidateAll();
			AssertHasRowMessageErrorContaining(entryLine, noMoreThanTenAdditionalInfosError);
			entryLine.AdditionalInformationCodes.RemoveAndDelete(additionalInfo);
			entryLine.Validation.ValidateAll();
			AssertNoRowMessageErrorContaining(entryLine, noMoreThanTenAdditionalInfosError);
		}
	}
}
