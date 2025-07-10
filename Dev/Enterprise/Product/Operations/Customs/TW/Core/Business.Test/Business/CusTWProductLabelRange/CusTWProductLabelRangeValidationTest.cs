using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Customs.TW.Business.Testing
{
	sealed class CusTWProductLabelRangeValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckTW0_Status()
		{
			var targetInfo = productLabelRange.TW0_StatusInfo;
			productLabelRange.TW0_Status = ProductLabelRangeStatusList.Codes._0;
			AssertNoMessageError(targetInfo, ListValidation.InvalidCodeMessageError);
			AssertNoMessageErrorContaining(targetInfo, MandatoryValidation.YouHaveNotEntered);
			productLabelRange.TW0_Status = "9";
			AssertHasMessageError(targetInfo, ListValidation.InvalidCodeMessageError);
			productLabelRange.TW0_Status = ZString.Empty;
			AssertHasMessageErrorContaining(targetInfo, MandatoryValidation.YouHaveNotEntered);
		}

		public void TestCheckTW0_EndNumber()
		{
			var targetInfo = productLabelRange.TW0_EndNumberInfo;
			productLabelRange.TW0_EndNumber = "11111111";
			AssertNoMessageError(targetInfo, ValidationConstants.ProductLabelRange.NumberMustBeEightCharacters(targetInfo.HumanReadableName));
			productLabelRange.TW0_EndNumber = "11";
			AssertHasMessageError(targetInfo, ValidationConstants.ProductLabelRange.NumberMustBeEightCharacters(targetInfo.HumanReadableName));
			productLabelRange.TW0_EndNumber = "@aaa";
			AssertHasMessageError(targetInfo, ValidationConstants.AlphanumericCharactersOnly(targetInfo.HumanReadableName));
			productLabelRange.TW0_EndNumber = "aa123";
			AssertNoMessageError(targetInfo, ValidationConstants.AlphanumericCharactersOnly(targetInfo.HumanReadableName));
		}

		public void TestCheckTW0_StartNumber()
		{
			var targetInfo = productLabelRange.TW0_StartNumberInfo;
			productLabelRange.TW0_StartNumber = "11111111";
			AssertNoMessageError(targetInfo, ValidationConstants.ProductLabelRange.NumberMustBeEightCharacters(targetInfo.HumanReadableName));
			productLabelRange.TW0_StartNumber = "11";
			AssertHasMessageError(targetInfo, ValidationConstants.ProductLabelRange.NumberMustBeEightCharacters(targetInfo.HumanReadableName));
			productLabelRange.TW0_StartNumber = "@aaa";
			AssertHasMessageError(targetInfo, ValidationConstants.AlphanumericCharactersOnly(targetInfo.HumanReadableName));
			productLabelRange.TW0_StartNumber = "aa123";
			AssertNoMessageError(targetInfo, ValidationConstants.AlphanumericCharactersOnly(targetInfo.HumanReadableName));
		}

		public void TestCheckTW0_RunNumber()
		{
			var targetInfo = productLabelRange.TW0_RunNumberInfo;
			productLabelRange.TW0_RunNumber = "@a";
			AssertHasMessageError(targetInfo, ValidationConstants.AlphanumericCharactersOnly(targetInfo.HumanReadableName));
			productLabelRange.TW0_RunNumber = "a1";
			AssertNoMessageError(targetInfo, ValidationConstants.AlphanumericCharactersOnly(targetInfo.HumanReadableName));
		}

		public void TestCheckTW0_Year()
		{
			var targetInfo = productLabelRange.TW0_YearInfo;
			productLabelRange.TW0_Year = "@a";
			AssertHasMessageError(targetInfo, ValidationConstants.AlphanumericCharactersOnly(targetInfo.HumanReadableName));
			productLabelRange.TW0_Year = "a1";
			AssertNoMessageError(targetInfo, ValidationConstants.AlphanumericCharactersOnly(targetInfo.HumanReadableName));
		}

		protected override void SetUp()
		{
			base.SetUp();
			var declaration = Factory.New<JobDeclaration>();
			var entryInstruction = declaration.CusEntryInstruction;
			var messageHeader = entryInstruction.ControllingMessageHeaders.AddNew();
			productLabelRange = messageHeader.ProductLabelRanges.AddNew();
		}

		CusTWProductLabelRange productLabelRange;
	}
}
