using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business.Testing.Organisation.PhoneNumber
{
	class PhoneNumberFormatterAndValidatorTest : TestCaseWithFactory
	{
		public void TestGetCountryCode()
		{
			var phoneNumberFormatterAndValidator = new PhoneNumberFormatterAndValidator();
			RefUNLOCO defaultUnloco = GetFirstUnlocoByCountryCode("AU");
			var validInternationalPhoneNumber = "+8615601131981";
			var invalidInternationalPhoneNumber = "+861560113";
			var validLocalPhoneNumber = "0426829924";
			var invalidLocalPhoneNumber = "04268";

			var countryCode1 = phoneNumberFormatterAndValidator.GetCountryCode(validInternationalPhoneNumber, defaultUnloco);
			var countryCode2 = phoneNumberFormatterAndValidator.GetCountryCode(invalidInternationalPhoneNumber, defaultUnloco);
			var countryCode3 = phoneNumberFormatterAndValidator.GetCountryCode(validLocalPhoneNumber, defaultUnloco);
			var countryCode4 = phoneNumberFormatterAndValidator.GetCountryCode(invalidLocalPhoneNumber, defaultUnloco);
			var countryCode5 = phoneNumberFormatterAndValidator.GetCountryCode(invalidLocalPhoneNumber, (RefUNLOCO)null);
			var countryCode6 = phoneNumberFormatterAndValidator.GetCountryCode(invalidLocalPhoneNumber, (RefUNLOCO)null);
			var countryCode7 = phoneNumberFormatterAndValidator.GetCountryCode(validInternationalPhoneNumber, "AU");
			var countryCode8 = phoneNumberFormatterAndValidator.GetCountryCode(invalidInternationalPhoneNumber, "AU");
			var countryCode9 = phoneNumberFormatterAndValidator.GetCountryCode(validLocalPhoneNumber, "AU");
			var countryCode10 = phoneNumberFormatterAndValidator.GetCountryCode(invalidLocalPhoneNumber, "AU");

			AssertEquals("Should return the correct country code from a valid international phone number.", "CN", countryCode1);
			AssertEquals("Should return the correct country code from an invalid international phone number.", "CN", countryCode2);
			AssertEquals("Should return the correct country code from a valid local phone number with a default UNLOCO.", "AU", countryCode3);
			AssertEquals("Should return the correct country code from an invalid local phone number with a default UNLOCO.", "AU", countryCode4);
			AssertEquals("Should return an empty country code from a valid local phone number without a default UNLOCO.", ZString.Empty, countryCode5);
			AssertEquals("Should return an empty country code from an invalid local phone number without a default UNLOCO.", ZString.Empty, countryCode6);
			AssertEquals("Should return the correct country code from a valid international phone number.", "CN", countryCode7);
			AssertEquals("Should return the correct country code from an invalid international phone number.", "CN", countryCode8);
			AssertEquals("Should return the correct country code from a valid local phone number with a default UNLOCO.", "AU", countryCode9);
			AssertEquals("Should return the correct country code from an invalid local phone number with a default UNLOCO.", "AU", countryCode10);
		}

		public void TestValidate()
		{
			var testBusinessObject = Factory.New<DummyBusinessObjectForPhoneValidation>();
			testBusinessObject.Port = GetFirstUnlocoByCountryCode("AU");

			testBusinessObject.PhoneNumberInfo.ClearAllNotifications();
			testBusinessObject.PhoneNumber = "+8615601131981";
			AssertNoErrors("A valid international phone number should produce no error messages.", testBusinessObject.PhoneNumberInfo);

			testBusinessObject.PhoneNumberInfo.ClearAllNotifications();
			testBusinessObject.PhoneNumber = "+861560113";
			AssertHasErrors("An invalid international phone number should produce an error message.", testBusinessObject.PhoneNumberInfo);
			AssertContains("The error message should contain the local sample of the country where the number belongs.", "010 1234 5678 (local format)", testBusinessObject.Notifications.First().Message);
			AssertContains("The error message should contain the international sample of the country where the number belongs.", "+86 10 1234 5678 (international format)", testBusinessObject.Notifications.First().Message);

			testBusinessObject.PhoneNumberInfo.ClearAllNotifications();
			testBusinessObject.PhoneNumber = "0426829924";
			AssertNoErrors("A valid local phone number should produce no error messages.", testBusinessObject.PhoneNumberInfo);

			testBusinessObject.PhoneNumberInfo.ClearAllNotifications();
			testBusinessObject.PhoneNumber = "04268";
			AssertHasErrors("An invalid local phone number should produce an error message.", testBusinessObject.PhoneNumberInfo);
			AssertContains("The error message should contain the local sample of the country where the number belongs.", "(02) 1234 5678 (local format)", testBusinessObject.Notifications.First().Message);
			AssertContains("The error message should contain the international sample of the country where the number belongs.", "+61 2 1234 5678 (international format)", testBusinessObject.Notifications.First().Message);

			testBusinessObject.PhoneNumberInfo.ClearAllNotifications();
			testBusinessObject.PhoneNumber = "+8829896815";
			AssertHasErrors("An invalid phone number from region 001 should produce an error message.", testBusinessObject.PhoneNumberInfo);
			AssertContains("The error message should contain the local sample of region 001.", "(local format)", testBusinessObject.Notifications.First().Message);
			AssertContains("The error message should contain the international sample of region 001.", "(international format)", testBusinessObject.Notifications.First().Message);

			testBusinessObject.Port = null;
			testBusinessObject.PhoneNumberInfo.ClearAllNotifications();
			testBusinessObject.PhoneNumber = "ABCDEFG";
			AssertHasErrors("An invalid phone number should produce an error message.", testBusinessObject.PhoneNumberInfo);
			AssertNotContains("The error message should not contain the local sample of the country/region where the number belongs.", "(local format)", testBusinessObject.Notifications.First().Message);
			AssertNotContains("The error message should not contain the international sample of the country/region where the number belongs.", "(international format)", testBusinessObject.Notifications.First().Message);
			AssertContains("The error message should prompt the user no country identified.", "No country/region identified to format the number.", testBusinessObject.Notifications.First().Message);
			AssertContains("The error message should contain the sample of international number.", "Please enter the number in an international format. See below example:", testBusinessObject.Notifications.First().Message);

			testBusinessObject.PhoneNumberInfo.ClearAllNotifications();
			testBusinessObject.Port = null;
			testBusinessObject.PhoneNumber = "04268";
			AssertHasErrors("An invalid local phone number without port should produce an error message.", testBusinessObject.PhoneNumberInfo);

			testBusinessObject.PhoneNumberInfo.ClearAllNotifications();
			testBusinessObject.PhoneNumber_IsManuallyVerified = true;

			testBusinessObject.PhoneNumber = "+861560113";
			AssertNoErrors("An invalid international phone number should not produce an error message if it is manually verified.", testBusinessObject.PhoneNumberInfo);

			testBusinessObject.PhoneNumber = "04268";
			AssertNoErrors("An invalid local phone number should not produce an error message if it is manually verified.", testBusinessObject.PhoneNumberInfo);

			testBusinessObject.Port = null;
			testBusinessObject.PhoneNumber = "04268";
			AssertNoErrors("An invalid local phone number without port should not produce an error message if it is manually verified.", testBusinessObject.PhoneNumberInfo);
		}

		public void TestFormatInternational()
		{
			var phoneNumberFormatterAndValidator = new PhoneNumberFormatterAndValidator();
			RefUNLOCO defaultUnloco = GetFirstUnlocoByCountryCode("AU");
			var validInternationalPhoneNumber = "+8615601131981";
			var invalidInternationalPhoneNumber = "+861560113";
			var validLocalPhoneNumber = "0426829924";
			var invalidLocalPhoneNumber = "04268";

			var resultPhoneNumber1 = phoneNumberFormatterAndValidator.FormatInternational(validInternationalPhoneNumber, defaultUnloco);
			AssertEquals("+86 156 0113 1981", resultPhoneNumber1);

			var resultPhoneNumber2 = phoneNumberFormatterAndValidator.FormatInternational(invalidInternationalPhoneNumber, defaultUnloco);
			AssertEquals(ZString.Empty, resultPhoneNumber2);

			var resultPhoneNumber3 = phoneNumberFormatterAndValidator.FormatInternational(validLocalPhoneNumber, defaultUnloco);
			AssertEquals("+61 426 829 924", resultPhoneNumber3);

			var resultPhoneNumber4 = phoneNumberFormatterAndValidator.FormatInternational(invalidLocalPhoneNumber, defaultUnloco);
			AssertEquals(ZString.Empty, resultPhoneNumber4);
		}

		public void TestFormatLocal()
		{
			var phoneNumberFormatterAndValidator = new PhoneNumberFormatterAndValidator();
			RefUNLOCO defaultUnloco = GetFirstUnlocoByCountryCode("AU");
			var validInternationalPhoneNumber = "+8615601131981";
			var invalidInternationalPhoneNumber = "+861560113";
			var validLocalPhoneNumber = "0426829924";
			var invalidLocalPhoneNumber = "04268";

			var resultPhoneNumber1 = phoneNumberFormatterAndValidator.FormatLocal(validInternationalPhoneNumber, defaultUnloco);
			AssertEquals("156 0113 1981", resultPhoneNumber1);

			var resultPhoneNumber2 = phoneNumberFormatterAndValidator.FormatLocal(invalidInternationalPhoneNumber, defaultUnloco);
			AssertEquals(ZString.Empty, resultPhoneNumber2);

			var resultPhoneNumber3 = phoneNumberFormatterAndValidator.FormatLocal(validLocalPhoneNumber, defaultUnloco);
			AssertEquals("0426 829 924", resultPhoneNumber3);

			var resultPhoneNumber4 = phoneNumberFormatterAndValidator.FormatLocal(invalidLocalPhoneNumber, defaultUnloco);
			AssertEquals(ZString.Empty, resultPhoneNumber4);
		}

		public void TestNormalize()
		{
			var phoneNumberFormatterAndValidator = new PhoneNumberFormatterAndValidator();
			RefUNLOCO defaultUnloco = GetFirstUnlocoByCountryCode("AU");
			var validInternationalPhoneNumber = "+8615601131981";
			var invalidInternationalPhoneNumber = "+861560113";
			var validLocalPhoneNumber = "0426829924";
			var invalidLocalPhoneNumber = "04268";

			var resultPhoneNumber1 = phoneNumberFormatterAndValidator.Normalize(validInternationalPhoneNumber, defaultUnloco);
			AssertEquals("+8615601131981", resultPhoneNumber1);

			var resultPhoneNumber2 = phoneNumberFormatterAndValidator.Normalize(invalidInternationalPhoneNumber, defaultUnloco);
			AssertEquals(ZString.Empty, resultPhoneNumber2);

			var resultPhoneNumber3 = phoneNumberFormatterAndValidator.Normalize(validLocalPhoneNumber, defaultUnloco);
			AssertEquals("+61426829924", resultPhoneNumber3);

			var resultPhoneNumber4 = phoneNumberFormatterAndValidator.Normalize(invalidLocalPhoneNumber, defaultUnloco);
			AssertEquals(ZString.Empty, resultPhoneNumber4);
		}

		public void TestGetInvalidPhoneNumberFormatNotificationMessage()
		{
			var invalidPhoneNumberNotification = new Notification(CargoWise.EntityFramework.NotificationType.Error, PhoneNumberFormatterAndValidator.InvalidPhoneNumberFormat + " bla bla bla");
			var randomNotification1 = new Notification(CargoWise.EntityFramework.NotificationType.Error, "This is a random notification");
			var randomNotification2 = new Notification(CargoWise.EntityFramework.NotificationType.Error, "This is another random notification");

			var notifications = new List<INotification> { randomNotification1, randomNotification2 };
			AssertEquals(string.Empty, PhoneNumberFormatterAndValidator.GetInvalidPhoneNumberFormatNotificationMessage(notifications));

			notifications.Add(invalidPhoneNumberNotification);
			AssertEquals(invalidPhoneNumberNotification.Message, PhoneNumberFormatterAndValidator.GetInvalidPhoneNumberFormatNotificationMessage(notifications));
		}

		public void TestIsManuallyVerified()
		{
			var dummy = Factory.New<DummyBusinessObject>();
			var collection = new GenCustomAddOnRuleAckCollection(dummy);
			var ack1 = collection.AddNew();
			ack1.XK_RuleID = ZGuid.NewZGuid();

			var ack2 = collection.AddNew();
			ack2.XK_RuleID = Core.Constants.CargoWiseOneGenCustomAddOnRuleIDs.PhoneNumberFormatValidation;
			ack2.XK_ParentTableColumn = DummyBusinessObject.Schema.Z0_NVarChar;

			var ack3 = collection.AddNew();
			ack3.XK_RuleID = ZGuid.NewZGuid();

			Assert(PhoneNumberFormatterAndValidator.IsManuallyVerified(dummy, DummyBusinessObject.Schema.Z0_NVarChar, collection));

			ack2.XK_RuleID = ZGuid.NewZGuid();
			Assert(!PhoneNumberFormatterAndValidator.IsManuallyVerified(dummy, DummyBusinessObject.Schema.Z0_NVarChar, collection));

			ack2.XK_RuleID = Core.Constants.CargoWiseOneGenCustomAddOnRuleIDs.PhoneNumberFormatValidation;
			ack2.XK_ParentTableColumn = DummyBusinessObject.Schema.Z0_NVarCharMax;
			Assert(!PhoneNumberFormatterAndValidator.IsManuallyVerified(dummy, DummyBusinessObject.Schema.Z0_NVarChar, collection));
		}

		public void TestSetIsManuallyVerified()
		{
			var dummy = Factory.New<DummyBusinessObjectForPhoneValidation>();
			dummy.PhoneNumber = "+61 2 0202 0202";
			var collection = new GenCustomAddOnRuleAckCollection(dummy);
			Factory.Save();
			AssertEquals("precondition", 0, collection.Count);
			Assert("precondition", !dummy.HasChanges);

			bool actionDone = false;

			PhoneNumberFormatterAndValidator.SetIsManuallyVerified(dummy.PhoneNumber_IsManuallyVerifiedInfo, true, collection, dummy, "PhoneNumber", () => { actionDone = true; }, dummy.PhoneNumberInfo);

			Assert(PhoneNumberFormatterAndValidator.IsManuallyVerified(dummy, "PhoneNumber", collection));
			AssertEquals(1, collection.Count);
			Assert(actionDone);
			Assert(dummy.HasChanges);

			PhoneNumberFormatterAndValidator.SetIsManuallyVerified(dummy.PhoneNumber_IsManuallyVerifiedInfo, true, collection, dummy, "PhoneNumber", () => { actionDone = true; }, dummy.PhoneNumberInfo);
			AssertEquals("still 1 ack as the phone was already manually verified", 1, collection.Count);

			actionDone = false;
			dummy.HasChanges = false;

			PhoneNumberFormatterAndValidator.SetIsManuallyVerified(dummy.PhoneNumber_IsManuallyVerifiedInfo, false, collection, dummy, "PhoneNumber", () => { actionDone = true; }, dummy.PhoneNumberInfo);
			AssertEquals(0, collection.Count);
			Assert(actionDone);
			Assert(dummy.HasChanges);
		}

		public void TestFormatCountryCodeAUPhoneNumber()
		{
			var phoneNumberFormatterAndValidator = new PhoneNumberFormatterAndValidator();
			var countryAU = GetFirstUnlocoByCountryCode("AU");
			var testBusinessObject = Factory.New<DummyBusinessObjectForPhoneValidation>();
			testBusinessObject.Port = countryAU;

			var resultPhoneNumberInternaltional1 = phoneNumberFormatterAndValidator.FormatInternational("04 6022 4444", countryAU);
			AssertEquals("+61 460 224 444", resultPhoneNumberInternaltional1);
			var resultPhoneNumberlocal1 = phoneNumberFormatterAndValidator.FormatLocal("04 6022 4444", countryAU);
			AssertEquals("0460 224 444", resultPhoneNumberlocal1);

			var resultPhoneNumberInternaltional2 = phoneNumberFormatterAndValidator.FormatInternational("460224444", countryAU);
			AssertEquals("+61 460 224 444", resultPhoneNumberInternaltional2);
			var resultPhoneNumberlocal2 = phoneNumberFormatterAndValidator.FormatLocal("04 6022 4444", countryAU);
			AssertEquals("0460 224 444", resultPhoneNumberlocal2);

			var resultPhoneNumberInternaltional3 = phoneNumberFormatterAndValidator.FormatInternational("+61 0460224444", countryAU);
			AssertEquals("+61 460 224 444", resultPhoneNumberInternaltional3);
			var resultPhoneNumberlocal3 = phoneNumberFormatterAndValidator.FormatLocal("04 6022 4444", countryAU);
			AssertEquals("0460 224 444", resultPhoneNumberlocal3);

			var resultPhoneNumberInternaltional4 = phoneNumberFormatterAndValidator.FormatInternational("+61460224444", countryAU);
			AssertEquals("+61 460 224 444", resultPhoneNumberInternaltional4);
			var resultPhoneNumberlocal4 = phoneNumberFormatterAndValidator.FormatLocal("04 6022 4444", countryAU);
			AssertEquals("0460 224 444", resultPhoneNumberlocal4);
		}

		public void TestValidateCountryCodeAUPhoneNumber()
		{
			var countryAU = GetFirstUnlocoByCountryCode("AU");
			var testBusinessObject = Factory.New<DummyBusinessObjectForPhoneValidation>();
			testBusinessObject.Port = countryAU;

			AssertAUPhoneNumber("+61 460 000 000", expectedNoError: true);
			AssertAUPhoneNumber("+61 460 999 999", expectedNoError: true);
			AssertAUPhoneNumber("+61 461 000 000", expectedNoError: true);

			AssertAUPhoneNumber("+61 480 099 999", expectedNoError: true);
			AssertAUPhoneNumber("+61 480 100 000", expectedNoError: true);
			AssertAUPhoneNumber("+61 480 199 999", expectedNoError: true);
			AssertAUPhoneNumber("+61 480 200 000", expectedNoError: true);
			AssertAUPhoneNumber("+61 480 299 999", expectedNoError: true);
			AssertAUPhoneNumber("+61 480 300 000", expectedNoError: true);
			AssertAUPhoneNumber("+61 480 399 999", expectedNoError: true);
			AssertAUPhoneNumber("+61 480 400 000", expectedNoError: true);
			AssertAUPhoneNumber("+61 480 499 999", expectedNoError: true);
			AssertAUPhoneNumber("+61 480 500 000", expectedNoError: true);

			AssertAUPhoneNumber("+61 482 099 999", expectedNoError: true);
			AssertAUPhoneNumber("+61 482 100 000", expectedNoError: true);
			AssertAUPhoneNumber("+61 482 199 999", expectedNoError: true);
			AssertAUPhoneNumber("+61 482 200 000", expectedNoError: true);
			AssertAUPhoneNumber("+61 482 399 999", expectedNoError: true);
			AssertAUPhoneNumber("+61 482 400 000", expectedNoError: true);
			AssertAUPhoneNumber("+61 483 399 999", expectedNoError: true);
			AssertAUPhoneNumber("+61 483 400 000", expectedNoError: false);

			AssertAUPhoneNumber("+61 483 799 999", expectedNoError: false);
			AssertAUPhoneNumber("+61 483 800 000", expectedNoError: true);
			AssertAUPhoneNumber("+61 483 899 999", expectedNoError: true);
			AssertAUPhoneNumber("+61 483 900 000", expectedNoError: true);
			AssertAUPhoneNumber("+61 483 999 999", expectedNoError: true);
			AssertAUPhoneNumber("+61 484 000 000", expectedNoError: true);

			AssertAUPhoneNumber("+61 485 999 999", expectedNoError: true);
			AssertAUPhoneNumber("+61 486 000 000", expectedNoError: true);
			AssertAUPhoneNumber("+61 486 999 999", expectedNoError: true);

			AssertAUPhoneNumber("+61 492 799 999", expectedNoError: true);
			AssertAUPhoneNumber("+61 492 800 000", expectedNoError: true);
			AssertAUPhoneNumber("+61 492 999 999", expectedNoError: true);
			AssertAUPhoneNumber("+61 493 000 000", expectedNoError: true);
			AssertAUPhoneNumber("+61 493 199 999", expectedNoError: true);
			AssertAUPhoneNumber("+61 493 200 000", expectedNoError: true);
			AssertAUPhoneNumber("+61 493 499 999", expectedNoError: true);
			AssertAUPhoneNumber("+61 493 500 000", expectedNoError: true);
			AssertAUPhoneNumber("+61 493 699 999", expectedNoError: true);
			AssertAUPhoneNumber("+61 493 700 000", expectedNoError: false);

			void AssertAUPhoneNumber(string phoneNumber, bool expectedNoError)
			{
				testBusinessObject.PhoneNumberInfo.ClearAllNotifications();
				testBusinessObject.PhoneNumber = phoneNumber;

				if (expectedNoError)
				{
					AssertNoErrors(testBusinessObject.PhoneNumberInfo);
				}
				else
				{
					AssertHasErrors(testBusinessObject.PhoneNumberInfo);
					var errorMessage = testBusinessObject.Notifications.First().Message;
					AssertContains("(02) 1234 5678 (local format)", errorMessage);
					AssertContains("+61 2 1234 5678 (international format)", errorMessage);
				}
			}
		}

		public void TestValidateAndFormatCountryCodeBRPhoneNumber()
		{
			var phoneNumberFormatterAndValidator = new PhoneNumberFormatterAndValidator();
			var countryBR = GetFirstUnlocoByCountryCode("BR");
			var testBusinessObject = Factory.New<DummyBusinessObjectForPhoneValidation>();
			testBusinessObject.Port = countryBR;

			testBusinessObject.PhoneNumberInfo.ClearAllNotifications();
			testBusinessObject.PhoneNumber = "+55 31 99192-0583";
			AssertNoErrors("A valid international phone number should produce no error messages.", testBusinessObject.PhoneNumberInfo);

			var resultPhoneNumber1 = phoneNumberFormatterAndValidator.FormatInternational("+55 31 99192-0583", countryBR);
			AssertEquals("+55 31 99192-0583", resultPhoneNumber1);

			var resultPhoneNumber2 = phoneNumberFormatterAndValidator.FormatInternational("+5531991920583", countryBR);
			AssertEquals("+55 31 99192-0583", resultPhoneNumber2);

			testBusinessObject.PhoneNumberInfo.ClearAllNotifications();
			testBusinessObject.PhoneNumber = "+55 31 89192-0583";
			AssertHasErrors("An invalid international phone number should produce an error message.", testBusinessObject.PhoneNumberInfo);
			AssertContains("The error message should contain the local sample of the country where the number belongs.", "(11) 2345-6789 (local format)", testBusinessObject.Notifications.First().Message);
			AssertContains("The error message should contain the international sample of the country where the number belongs.", "+55 11 2345-6789 (international format)", testBusinessObject.Notifications.First().Message);
		}

		public void TestValidateAndFormatCountryCodePHPhoneNumber()
		{
			var phoneNumberFormatterAndValidator = new PhoneNumberFormatterAndValidator();
			var countryPH = GetFirstUnlocoByCountryCode("PH");
			var testBusinessObject = Factory.New<DummyBusinessObjectForPhoneValidation>();
			testBusinessObject.Port = countryPH;

			testBusinessObject.PhoneNumberInfo.ClearAllNotifications();
			testBusinessObject.PhoneNumber = "+63 2 3123 4567";
			AssertNoErrors(testBusinessObject.PhoneNumberInfo);

			testBusinessObject.PhoneNumberInfo.ClearAllNotifications();
			testBusinessObject.PhoneNumber = "+63 2 234 5678";
			AssertHasErrors("Seven Digits Not Valid", testBusinessObject.PhoneNumberInfo);

			testBusinessObject.PhoneNumber = "+63 2 23456";
			AssertNoErrors("Five Digits Still Work", testBusinessObject.PhoneNumberInfo);

			var resultPhoneNumber1 = phoneNumberFormatterAndValidator.FormatInternational("+63231234567", countryPH);
			AssertEquals(@"Eight Digits Works For Number Like +63 2[378]\d{7}", "+63 2 3123 4567", resultPhoneNumber1);

			var resultPhoneNumber2 = phoneNumberFormatterAndValidator.FormatInternational("278889999", countryPH);
			AssertEquals("+63 2 7888 9999", resultPhoneNumber2);

			var resultPhoneNumber3 = phoneNumberFormatterAndValidator.FormatInternational("+63 287654321", countryPH);
			AssertEquals("+63 2 8765 4321", resultPhoneNumber3);

			testBusinessObject.PhoneNumberInfo.ClearAllNotifications();
			testBusinessObject.PhoneNumber = "+63 2 2234 5678";
			CombineAssertions(() =>
			{
				AssertHasErrors("Invalid phone number", testBusinessObject.PhoneNumberInfo);
				AssertContains("(02) 3234 5678 (local format)", testBusinessObject.Notifications.First().Message);
				AssertContains("+63 2 3234 5678 (international format)", testBusinessObject.Notifications.First().Message);
			});

			testBusinessObject.PhoneNumber = "+63 2 3234 5678";
			AssertNoErrors(testBusinessObject.PhoneNumberInfo);
		}

		public void TestValidateAndFormatCountryCodeBDFixedLineNumber()
		{
			var phoneNumberFormatterAndValidator = new PhoneNumberFormatterAndValidator();
			var countryBD = GetFirstUnlocoByCountryCode("BD");
			var testBusinessObject = Factory.New<DummyBusinessObjectForPhoneValidation>();
			testBusinessObject.Port = countryBD;

			testBusinessObject.PhoneNumberInfo.ClearAllNotifications();
			testBusinessObject.PhoneNumber = "+880 2-7788893";
			AssertNoErrors("A valid international phone number should produce no error messages.", testBusinessObject.PhoneNumberInfo);

			var resultPhoneNumber1 = phoneNumberFormatterAndValidator.FormatInternational("+880 2-7788893", countryBD);
			AssertEquals("+880 2-7788893", resultPhoneNumber1);

			var resultPhoneNumber2 = phoneNumberFormatterAndValidator.FormatInternational("88027788893", countryBD);
			AssertEquals("+880 2-7788893", resultPhoneNumber2);

			var resultPhoneNumber3 = phoneNumberFormatterAndValidator.FormatInternational("+880 2 9298508", countryBD);
			AssertEquals("", resultPhoneNumber3);

			var resultPhoneNumber4 = phoneNumberFormatterAndValidator.FormatInternational("88029298372", countryBD);
			AssertEquals("", resultPhoneNumber4);

			testBusinessObject.PhoneNumberInfo.ClearAllNotifications();
			testBusinessObject.PhoneNumber = "+880 2-7888893";
			AssertHasErrors("An invalid international phone number should produce an error message.", testBusinessObject.PhoneNumberInfo);
			AssertContains("The error message should contain the local sample of the country where the number belongs.", "02-7111234 (local format)", testBusinessObject.Notifications.First().Message);
			AssertContains("The error message should contain the international sample of the country where the number belongs.", "+880 2-7111234 (international format)", testBusinessObject.Notifications.First().Message);
		}

		public void TestValidatePhoneNumberForCountryCode_MR()
		{
			var countryMR = GetFirstUnlocoByCountryCode("MR");
			var testBusinessObject = Factory.New<DummyBusinessObjectForPhoneValidation>();
			testBusinessObject.Port = countryMR;

			testBusinessObject.PhoneNumberInfo.ClearAllNotifications();
			testBusinessObject.PhoneNumber = "+222 42505040";
			AssertNoErrors("A valid international phone number should produce no error messages.", testBusinessObject.PhoneNumberInfo);

			testBusinessObject.PhoneNumberInfo.ClearAllNotifications();
			testBusinessObject.PhoneNumber = "42505040";
			AssertNoErrors("A valid national phone number should produce no error messages.", testBusinessObject.PhoneNumberInfo);
		}

		#region Implementations

		RefUNLOCO GetFirstUnlocoByCountryCode(string countryCode)
		{
			var query = new ZQuery(RefUNLOCOSchema.RL_RN_NKCountryCode, countryCode);
			return Factory.LoadTop1<RefUNLOCO>(query);
		}

		class DummyBusinessObjectForPhoneValidation : DummyBusinessObject, IObsoleteValidation
		{
			public DummyBusinessObjectForPhoneValidation(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{
			}

			PhoneNumberFormatterAndValidator PhoneNumberFormatterAndValidator
			{
				get { return phoneNumberFormatterAndValidator ?? (phoneNumberFormatterAndValidator = new PhoneNumberFormatterAndValidator()); }
			}

			public RefUNLOCO Port { private get; set; }

			PhoneNumberFormatterAndValidator phoneNumberFormatterAndValidator;

			#region PhoneNumber

			ZString phoneNumber;

			public ZString PhoneNumber
			{
				private get { return phoneNumber; }
				set
				{
					phoneNumber = value;
					ValidatePhoneNumber();
				}
			}

			public ZPropertyInfo PhoneNumberInfo
			{
				get { return GetZPropertyInfo(nameof(PhoneNumber)); }
			}

			public ZBool PhoneNumber_IsManuallyVerified { get; set; }

			public ZPropertyInfo PhoneNumber_IsManuallyVerifiedInfo
			{
				get { return GetZPropertyInfo(nameof(PhoneNumber_IsManuallyVerified)); }
			}

			void ValidatePhoneNumber()
			{
				PhoneNumberInfo.ClearAllNotifications();
				PhoneNumberFormatterAndValidator.Validate(PhoneNumberInfo, null, PhoneNumber_IsManuallyVerifiedInfo, Port);
			}

			#endregion
		}

		#endregion
	}
}
