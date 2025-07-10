using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(SerbiaRegistrationNumberValidator))]
	sealed class SerbiaRegistrationNumberValidatorTest : TestCaseWithFactory
	{
		OrgCusCode CreateCode(ZString codeType)
		{
			var cusCode = Factory.New<OrgCusCode>();
			cusCode.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.Serbia;
			cusCode.OK_CodeType = codeType;
			return cusCode;
		}

		public void TestValidateAEO()
		{
			var cusCode = CreateCode(SerbiaOrgCusCodeInfo.OrgCusCodes.AEO);

			const string invalidCodeMessage = "RS AEO number should start with \"AEOF\" or \"AEOS\" followed by 24 digits.";

			cusCode.OK_CustomsRegNo = "123456789012345678901234";
			AssertHasError("Not start with AEOF or AEOS", cusCode.OK_CustomsRegNoInfo, invalidCodeMessage);

			cusCode.OK_CustomsRegNo = "AEO123456789012345678901234";
			AssertHasError("Not start with AEOF or AEOS", cusCode.OK_CustomsRegNoInfo, invalidCodeMessage);

			cusCode.OK_CustomsRegNo = "F123456789012345678901234";
			AssertHasError("Not start with AEOF or AEOS", cusCode.OK_CustomsRegNoInfo, invalidCodeMessage);

			cusCode.OK_CustomsRegNo = "AEOS12345678901234567890123";
			AssertHasError("Invalid Length", cusCode.OK_CustomsRegNoInfo, invalidCodeMessage);

			cusCode.OK_CustomsRegNo = "AEOF12345678901234567890123";
			AssertHasError("Invalid Length", cusCode.OK_CustomsRegNoInfo, invalidCodeMessage);

			cusCode.OK_CustomsRegNo = "AEOS12345678901234567890123X";
			AssertHasError("Invalid character", cusCode.OK_CustomsRegNoInfo, invalidCodeMessage);

			cusCode.OK_CustomsRegNo = "AEOF12345678901234567890123X";
			AssertHasError("Invalid character", cusCode.OK_CustomsRegNoInfo, invalidCodeMessage);

			cusCode.OK_CustomsRegNo = "AEOF123456789012345678901234";
			AssertNoError("Valid Code", cusCode.OK_CustomsRegNoInfo, invalidCodeMessage);

			cusCode.OK_CustomsRegNo = "AEOS123456789012345678901234";
			AssertNoError("Valid Code", cusCode.OK_CustomsRegNoInfo, invalidCodeMessage);
		}

		public void TestValidateJBK()
		{
			var cusCode = CreateCode(SerbiaOrgCusCodeInfo.OrgCusCodes.JBK);

			const string invalidFormatMessage = "RS JBKJS number should start with \"JBKJS\", followed by 5 digits.";

			cusCode.OK_CustomsRegNo = string.Empty;
			AssertHasError("Must not be blank.", cusCode.OK_CustomsRegNoInfo, "Please enter a Registration Number / Code.");

			cusCode.OK_CustomsRegNo = "JBK123";
			AssertHasError("Invalid code, wrong prefix, too short", cusCode.OK_CustomsRegNoInfo, invalidFormatMessage);

			cusCode.OK_CustomsRegNo = "JBKJS123";
			AssertHasError("Invalid code, too short", cusCode.OK_CustomsRegNoInfo, invalidFormatMessage);

			cusCode.OK_CustomsRegNo = "JBK1234567";
			AssertHasError("Invalid code, wrong prefix", cusCode.OK_CustomsRegNoInfo, invalidFormatMessage);

			cusCode.OK_CustomsRegNo = "JBKJS123456";
			AssertHasError("Invalid code, too long", cusCode.OK_CustomsRegNoInfo, invalidFormatMessage);

			cusCode.OK_CustomsRegNo = "JBKJS00000";
			AssertNoNotifications("Valid code", cusCode);

			cusCode.OK_CustomsRegNo = "JBKJS12345";
			AssertNoNotifications("Valid code", cusCode);

			cusCode.OK_CustomsRegNo = "JBKJS99999";
			AssertNoNotifications("Valid code", cusCode);
		}
	}
}
