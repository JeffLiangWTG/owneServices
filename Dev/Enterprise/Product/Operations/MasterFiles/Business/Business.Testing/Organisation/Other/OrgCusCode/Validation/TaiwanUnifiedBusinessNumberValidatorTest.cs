using CargoWise.ComponentModel;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class TaiwanUnifiedBusinessNumberValidatorTest : TestCaseWithDummy
	{
		const string ValidateFormatErrorMessage = "The Taiwan VAT number must be an 8-digit number.";
		const string ValidateCodeErrorMessage = "The Taiwan VAT number is invalid.";

		readonly ZString[] CodesWithFormatError = new ZString[] { "1", "12345678a", "123456789" };
		readonly ZString[] InvalidCodes = new ZString[] { "96944492", "12345661" };
		readonly ZString[] ValidCodesForOldRule = new ZString[] { "55709318", "55709886", "56701927", "22276059", "22284804", "05806223", "23551336", "05815010",
			"76632060", "76632977", "86615012", "97238661", "86980382", "97262015", "04595257", "10458575", "19312376" };

		readonly ZString[] ValidCodesForNewRule = new ZString[] { "54595257", "60458575", "69312376" };

		[TestDate(2023, 3, 31)]
		public void TestTaiwanVATValidationOldRule()
		{
			foreach (var code in CodesWithFormatError)
			{
				AssertTaiwanVATValidation(code, NotificationType.Error, ValidateFormatErrorMessage);
			}

			foreach (var code in InvalidCodes)
			{
				AssertTaiwanVATValidation(code, NotificationType.Warning, ValidateCodeErrorMessage);
			}

			foreach (var code in ValidCodesForOldRule)
			{
				AssertTaiwanVATValidation(code, NotificationType.Error, ZString.Empty);
			}

			foreach (var code in ValidCodesForNewRule)
			{
				AssertTaiwanVATValidation(code, NotificationType.Warning, ValidateCodeErrorMessage);
			}
		}

		[TestDate(2023, 4, 1)]
		public void TestTaiwanVATValidationNewRule()
		{
			foreach (var code in CodesWithFormatError)
			{
				AssertTaiwanVATValidation(code, NotificationType.Error, ValidateFormatErrorMessage);
			}

			foreach (var code in InvalidCodes)
			{
				AssertTaiwanVATValidation(code, NotificationType.Warning, ValidateCodeErrorMessage);
			}

			foreach (var code in ValidCodesForOldRule)
			{
				AssertTaiwanVATValidation(code, NotificationType.Error, ZString.Empty);
			}

			foreach (var code in ValidCodesForNewRule)
			{
				AssertTaiwanVATValidation(code, NotificationType.Error, ZString.Empty);
			}
		}

		public void TestGetValidateNotifyInformation()
		{
			CombineAssertions(() =>
			{
				AssertGetValidateNotifyInformation("1", ValidateFormatErrorMessage);
				AssertGetValidateNotifyInformation("1234567", ValidateFormatErrorMessage);
				AssertGetValidateNotifyInformation("1234567a", ValidateFormatErrorMessage);
				AssertGetValidateNotifyInformation("123456789", ValidateFormatErrorMessage);
				AssertGetValidateNotifyInformation("12345678a", ValidateFormatErrorMessage);
				AssertGetValidateNotifyInformation("abababdd", ValidateFormatErrorMessage);
				AssertGetValidateNotifyInformation("10458576", ValidateCodeErrorMessage);
				AssertGetValidateNotifyInformation("12345678", ValidateCodeErrorMessage);
				AssertGetValidateNotifyInformation("12345676", ZString.Empty);
				AssertGetValidateNotifyInformation("12345675", ZString.Empty);
				AssertGetValidateNotifyInformation("876543", ValidateFormatErrorMessage);
				AssertGetValidateNotifyInformation("8765432", ValidateFormatErrorMessage);
				AssertGetValidateNotifyInformation("87654321", ValidateCodeErrorMessage);
				AssertGetValidateNotifyInformation("87654321a", ValidateFormatErrorMessage);
				AssertGetValidateNotifyInformation("９９９９９９９９", ValidateFormatErrorMessage);
				AssertGetValidateNotifyInformation("87654322", ZString.Empty);
				AssertGetValidateNotifyInformation("55711877", ZString.Empty);
				AssertGetValidateNotifyInformation("10458575", ZString.Empty);
			});
		}

		void AssertTaiwanVATValidation(ZString codeToTest, INotificationType notificationType, ZString expectdMessage)
		{
			TaiwanUnifiedBusinessNumberValidator.Validate(Dummy.Z0_DescriptionInfo, codeToTest, notificationType);
			if (expectdMessage.IsEmpty)
			{
				AssertNoNotifications($"{codeToTest} should not have any notifications.", Dummy.Z0_DescriptionInfo);
			}
			else
			{
				if (notificationType == NotificationType.Error)
				{
					AssertHasError($"{codeToTest} should have this error.", Dummy.Z0_DescriptionInfo, expectdMessage);
				}
				else if (notificationType == NotificationType.Warning)
				{
					AssertHasWarning($"{codeToTest} should have this warning.", Dummy.Z0_DescriptionInfo, expectdMessage);
				}
			}
			Dummy.Z0_DescriptionInfo.ClearAllNotifications();
		}

		void AssertGetValidateNotifyInformation(ZString codeToTest, ZString expectdMessage)
		{
			var result = TaiwanUnifiedBusinessNumberValidator.GetValidateNotifyInformation(codeToTest);
			AssertEquals($"Format Validation error message for {codeToTest} should be ", expectdMessage, result);
		}

		protected override void RunTest()
		{
			using (Dummy.SuspendValidationTesting())
			{
				base.RunTest();
			}
		}
	}
}
