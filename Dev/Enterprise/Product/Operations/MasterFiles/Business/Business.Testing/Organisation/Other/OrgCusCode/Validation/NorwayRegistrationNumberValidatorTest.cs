using CargoWise.ComponentModel;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(NorwayRegistrationNumberValidator))]
	sealed class NorwayRegistrationNumberValidatorTest : TestCaseWithDummy
	{
		const string ValidateFormatErrorMessageForGBR = "Registration Number / Code: Norway Government Business Code should be 9 digits NNNNNNNNN and compiled with last digit check sum calculation to ensure its validity. An error in this field indicates that the entered value is invalid - please double check you have inputted the correct values.";
		const string ValidateFormatErrorMessageForMVA = "Registration Number / Code: Norway MVA (VAT Tax ID) should be 9 digits NNNNNNNNN and compiled with last digit check sum calculation to ensure its validity. An error in this field indicates that the entered value is invalid - please double check you have inputted the correct values.";

		readonly ZString[] CodesWithFormatError = new ZString[] { "1", "12345678a", "123456789", "jashdgj876876", "123445678pol*", "@#$%^asjdh" };
		readonly ZString[] ValidCodes = new ZString[] { "123456785", "930615390", "975368211", "958995369" };

		[TestDate(2023, 4, 1)]
		public void TestNorwayVATValidationRule()
		{
			foreach (var code in CodesWithFormatError)
			{
				AssertNorwayGBRValidation(code, NotificationType.Error, ValidateFormatErrorMessageForGBR);
				AssertNorwayMVAValidation(code, NotificationType.Error, ValidateFormatErrorMessageForMVA);
			}

			foreach (var code in ValidCodes)
			{
				AssertNorwayGBRValidation(code, NotificationType.Error, ZString.Empty);
				AssertNorwayMVAValidation(code, NotificationType.Error, ZString.Empty);
			}
		}

		public void TestGetValidateNotifyInformation()
		{
			CombineAssertions(() =>
			{
				AssertGetValidateNotifyInformation("1", ValidateFormatErrorMessageForGBR);
				AssertGetValidateNotifyInformation("1234567", ValidateFormatErrorMessageForGBR);
				AssertGetValidateNotifyInformation("1234567a", ValidateFormatErrorMessageForGBR);
				AssertGetValidateNotifyInformation("123456789", ValidateFormatErrorMessageForGBR);
				AssertGetValidateNotifyInformation("12345678a", ValidateFormatErrorMessageForGBR);
				AssertGetValidateNotifyInformation("abababdd", ValidateFormatErrorMessageForGBR);
				AssertGetValidateNotifyInformation("10458576", ValidateFormatErrorMessageForGBR);
				AssertGetValidateNotifyInformation("12345678", ValidateFormatErrorMessageForGBR);
				AssertGetValidateNotifyInformation("123456785", ZString.Empty);
				AssertGetValidateNotifyInformation("930615390", ZString.Empty);
				AssertGetValidateNotifyInformation("876543", ValidateFormatErrorMessageForGBR);
				AssertGetValidateNotifyInformation("8765432", ValidateFormatErrorMessageForGBR);
				AssertGetValidateNotifyInformation("87654321", ValidateFormatErrorMessageForGBR);
				AssertGetValidateNotifyInformation("87654321a", ValidateFormatErrorMessageForGBR);
				AssertGetValidateNotifyInformation("９９９９９９９９", ValidateFormatErrorMessageForGBR);
				AssertGetValidateNotifyInformation("975368211", ZString.Empty);
				AssertGetValidateNotifyInformation("958995369", ZString.Empty);
			});
		}

		void AssertNorwayGBRValidation(ZString codeToTest, INotificationType notificationType, ZString expectdMessage)
		{
			NorwayRegistrationNumberValidator.ValidateGBR(Dummy.Z0_DescriptionInfo, codeToTest, notificationType);
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
			}
			Dummy.Z0_DescriptionInfo.ClearAllNotifications();
		}
		void AssertNorwayMVAValidation(ZString codeToTest, INotificationType notificationType, ZString expectdMessage)
		{
			NorwayRegistrationNumberValidator.ValidateMVA(Dummy.Z0_DescriptionInfo, codeToTest, notificationType);
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
			}
			Dummy.Z0_DescriptionInfo.ClearAllNotifications();
		}

		void AssertGetValidateNotifyInformation(ZString codeToTest, ZString expectdMessage)
		{
			var result = NorwayRegistrationNumberValidator.GetValidateMVAGBRNotifyInformation(codeToTest, "GBR");
			AssertEquals($"Format Validation error message for {codeToTest} should be ", expectdMessage, result);
		}

		public void TestNorwegianSocialSecurityNumbers()
		{
			var errorMessageWrongLength = "Wrong length. Norwegian Social Security Numbers should be 11 digits.";
			var errorMessageGeneric = "Not a valid Norwegian Social Security Number.";
			var errorMessageOnlyDigits = "Norwegian Social Security Numbers can only contain digits.";

			var listOfValidSSN = new ZString[] { "08052621187", "01110836399", "19011233360", "20101138829", "11053336886", "12114423309", "10070343334", "14093948247", "07043538963", "28014503937", "19070428914", "01110540457" };

			var listOfInvalidSSN = new (string ssn, string errorMessage, string assertMessage)[]
			{
				("1234567890", errorMessageWrongLength, "Only 10 digits"),
				("123456789012", errorMessageWrongLength, "12 digits"),
				("XYZ45678901", errorMessageOnlyDigits, "Letters and numbers"),
				("01029955555", errorMessageGeneric, "Generic Error"),
				("29020112345", errorMessageGeneric, "not a leap year"),
				("13097248032", errorMessageGeneric, "checksums don't match"),
				("13097.48032", errorMessageOnlyDigits, "punctuation, dot"),
				("13097,48032", errorMessageOnlyDigits, "punctuation, comma"),
			};

			CombineAssertions(() =>
			{
				foreach (var pnr in listOfValidSSN)
				{
					var result = NorwayPersonalNumberValidator.Validate(pnr);
					AssertEquals($"{pnr}", true, result.Success);
				}

				foreach (var (ssn, errorMessage, assertMessage) in listOfInvalidSSN)
				{
					var result = NorwayPersonalNumberValidator.Validate(ssn);
					AssertEquals($"{ssn} - {assertMessage}", false, result.Success);
					AssertEquals($"{ssn} - {assertMessage}", errorMessage, result.ErrorMessage);
				}
			});
		}

		public void TestNorwayEMDValidation()
		{
			const string message = "The length of Registration Number / Code for Type 'EMD' cannot exceed 10 characters.";

			CombineAssertions(() =>
			{
				NorwayRegistrationNumberValidator.ValidateEMD(Dummy.Z0_DescriptionInfo, "1234567890", NotificationType.Warning);
				AssertNoMessageErrors("Should not have this message error.", Dummy.Z0_DescriptionInfo);
				NorwayRegistrationNumberValidator.ValidateEMD(Dummy.Z0_DescriptionInfo, "12345678901", NotificationType.Warning);
				AssertHasMessageError("Should have this message error.", Dummy.Z0_DescriptionInfo, message);
			});

			Dummy.Z0_DescriptionInfo.ClearAllNotifications();
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
