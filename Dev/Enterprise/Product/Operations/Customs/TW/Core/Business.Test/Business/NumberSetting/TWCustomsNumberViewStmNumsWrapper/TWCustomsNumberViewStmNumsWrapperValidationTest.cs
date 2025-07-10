using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.TW.Business.Testing
{
	sealed class TWCustomsNumberViewStmNumsWrapperValidationTest : CargoWise.EntityFramework.Testing.BusinessObjectValidationTestCase
	{
		public void TestCheckMessageType()
		{
			var stmNum = Factory.New<CustomsNumberViewStmNums>();
			var wrapper = new TWCustomsNumberViewStmNumsWrapper(stmNum);
			wrapper.SN_Type = "A";
			wrapper.MessageType = ZString.Empty;
			AssertHasErrorContaining(wrapper.MessageTypeInfo, MandatoryValidation.MustBeEntered);
			wrapper.MessageType = "EEE";
			AssertNoErrorContaining(wrapper.MessageTypeInfo, MandatoryValidation.MustBeEntered);
			AssertHasErrorContaining(wrapper.MessageTypeInfo, ListValidation.InvalidCodeError);
			wrapper.MessageType = "IMP";
			AssertNoErrorContaining(wrapper.MessageTypeInfo, ListValidation.InvalidCodeError);
		}

		public void TestCheckRangeType()
		{
			Assert("TWCustomsNumberViewStmNumsCompanyProviderTest.TestCheckRangeType", true);
		}

		public void TestCheckStartNumber()
		{
			var stmNum = Factory.New<CustomsNumberViewStmNums>();
			stmNum.SN_FountainName = "TWEntryNum_IMP_C";
			var wrapper = new TWCustomsNumberViewStmNumsWrapper(stmNum);
			wrapper.StartNumber = "1111111";
			AssertHasErrorContaining(wrapper.StartNumberInfo, ValidationConstants.TWCustomsNumberViewStmNumsWrapper.InvalidNumber);
			AssertHasErrorContaining(wrapper.StartNumberInfo, "The character only accept digits and English Characters A/B/C/D/E/F/G/H/I/J/K/L/M/N/O/P/Q/R/S/T/U/V/W/X/Y/Z, the length is 5, and the last character must is digits.");
			wrapper.StartNumber = "00001";
			AssertNoErrorContaining(wrapper.StartNumberInfo, ValidationConstants.TWCustomsNumberViewStmNumsWrapper.InvalidNumber);
			AssertNoErrorContaining(wrapper.StartNumberInfo, "The character only accept digits and English Characters A/B/C/D/E/F/G/H/I/J/K/L/M/N/O/P/Q/R/S/T/U/V/W/X/Y/Z, the length is 5, and the last character must is digits.");
		}

		public void TestCheckEndNumber()
		{
			var stmNum = Factory.New<CustomsNumberViewStmNums>();
			stmNum.SN_FountainName = "TWEntryNum_IMP_A";
			var wrapper = new TWCustomsNumberViewStmNumsWrapper(stmNum);
			var messageText = "The character only accept digits and English Characters A/B/C/D/E/F/G/H/I/J/K/L/M/N/O/P/Q/R/S/T/U/V/W/X/Y/Z, the length is 5, and the last character must is digits.";
			wrapper.EndNumber = "999999";
			AssertHasErrorContaining(wrapper.EndNumberInfo, ValidationConstants.TWCustomsNumberViewStmNumsWrapper.InvalidNumber);
			AssertHasErrorContaining(wrapper.EndNumberInfo, messageText);
			wrapper.EndNumber = "E0001";
			AssertNoErrorContaining(wrapper.EndNumberInfo, ValidationConstants.TWCustomsNumberViewStmNumsWrapper.InvalidNumber);
			AssertNoErrorContaining(wrapper.EndNumberInfo, messageText);
			wrapper.StartNumber = "00002";
			wrapper.EndNumber = "00001";
			AssertHasErrorContaining(wrapper.EndNumberInfo, ValidationConstants.TWCustomsNumberViewStmNumsWrapper.EndNumberCannotBeSmallerThanStartNumber);
			wrapper.EndNumber = "00003";
			AssertNoErrorContaining(wrapper.EndNumberInfo, ValidationConstants.TWCustomsNumberViewStmNumsWrapper.EndNumberCannotBeSmallerThanStartNumber);
			wrapper.CurrentValue = "00005";
			wrapper.EndNumber = "00004";
			AssertHasErrorContaining(wrapper.EndNumberInfo, ValidationConstants.TWCustomsNumberViewStmNumsWrapper.HasUsedRange);
			wrapper.EndNumber = "00006";
			AssertNoErrorContaining(wrapper.EndNumberInfo, ValidationConstants.TWCustomsNumberViewStmNumsWrapper.HasUsedRange);
		}

		public void TestCheckCurrentValue()
		{
			var stmNum = Factory.New<CustomsNumberViewStmNums>();
			stmNum.SN_FountainName = "TWEntryNum_IMP_A";
			var wrapper = new TWCustomsNumberViewStmNumsWrapper(stmNum);
			CombineAssertions(() =>
			{
				wrapper.CurrentValue = "999999";
				AssertHasErrorContaining(wrapper.CurrentValueInfo, ValidationConstants.TWCustomsNumberViewStmNumsWrapper.InvalidNumber);
				AssertHasErrorContaining(wrapper.CurrentValueInfo, "The character only accept digits and English Characters A/B/C/D/E/F/G/H/I/J/K/L/M/N/O/P/Q/R/S/T/U/V/W/X/Y/Z, the length is 5, and the last character must is digits.");
				wrapper.CurrentValue = "E0001";
				AssertNoErrorContaining(wrapper.CurrentValueInfo, ValidationConstants.TWCustomsNumberViewStmNumsWrapper.InvalidNumber);
				AssertNoErrorContaining(wrapper.CurrentValueInfo, "The character only accept digits and English Characters A/B/C/D/E/F/G/H/I/J/K/L/M/N/O/P/Q/R/S/T/U/V/W/X/Y/Z, the length is 5, and the last character must is digits.");
			});
		}
	}
}
