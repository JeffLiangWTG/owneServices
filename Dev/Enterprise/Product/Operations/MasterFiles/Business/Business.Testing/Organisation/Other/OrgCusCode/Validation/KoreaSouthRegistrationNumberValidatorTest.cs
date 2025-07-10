using System;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Registry.Business;
using NotificationType = CargoWise.ComponentModel.NotificationType;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class KoreaSouthRegistrationNumberValidatorTest : TestCaseWithDummy
	{
		public void TestValidateVATNumber()
		{
			var message = "The KR VAT Registration number '{0}' is invalid.\r\nIt should be in format 'NNNNNNNNNN'.";
			var code = OrganisationRegistry.RegistrationNumberFormatFields.KRVAT;
			Action<ZPropertyInfo> validation = x => KoreaSouthRegistrationNumberValidator.ValidateVATNumber(x);

			AssertValidation(NotificationType.Error, "ABC", message, validation);
			AssertValidation(NotificationType.Error, "123", message, validation);

			AssertValidation(NotificationType.Error, "ABCDEABCDE", message, validation);
			AssertValidation(null, "1234567890", message, validation);

			AssertForRegistry_StrictEnforcementOfRegistrationNumberFormats("123", code, message, validation);
		}

		public void TestValidateOfficeID()
		{
			var message = "The KR 08 - Office Registration number '{0}' is invalid.\r\nIt should be in format 'NNNN'.";
			var code = OrganisationRegistry.RegistrationNumberFormatFields.KR08;
			Action<ZPropertyInfo> validation = x => KoreaSouthRegistrationNumberValidator.ValidateOfficeID(x);

			AssertValidation(NotificationType.Error, "ABC", message, validation);
			AssertValidation(NotificationType.Error, "123", message, validation);

			AssertValidation(NotificationType.Error, "ABCD", message, validation);
			AssertValidation(null, "1234", message, validation);

			AssertForRegistry_StrictEnforcementOfRegistrationNumberFormats("123", code, message, validation);
		}

		public void TestValidateKoreanRegNoForResident()
		{
			var message = "The KR 01 - Citizen Registration number '{0}' is invalid.\r\nIt should be in format 'NNNNNNNNNNNNN'.";
			var code = OrganisationRegistry.RegistrationNumberFormatFields.KR01;
			Action<ZPropertyInfo> validation = x => KoreaSouthRegistrationNumberValidator.ValidateKoreanRegNoForResident(x);

			AssertValidation(NotificationType.Error, "ABCDE", message, validation);
			AssertValidation(NotificationType.Error, "12345", message, validation);

			AssertValidation(NotificationType.Error, "ABCDEABCDEABC", message, validation);
			AssertValidation(null, "1234567890123", message, validation);

			AssertForRegistry_StrictEnforcementOfRegistrationNumberFormats("123", code, message, validation);
		}

		public void TestValidateKBT()
		{
			var message = "The KR KBT Registration number '{0}' is invalid.\r\nIt must not exceed 100 characters.";
			var code = OrganisationRegistry.RegistrationNumberFormatFields.KRKBT;
			Action<ZPropertyInfo> validation = x => KoreaSouthRegistrationNumberValidator.ValidateKBT(x);

			var string1With100Chars = "ABCDE67890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890";
			var string2With100Chars = "1234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890";

			AssertValidation(NotificationType.Error, string1With100Chars + "1", message, validation);
			AssertValidation(NotificationType.Error, string2With100Chars + "1", message, validation);

			AssertValidation(null, string1With100Chars, message, validation);
			AssertValidation(null, string2With100Chars, message, validation);

			AssertForRegistry_StrictEnforcementOfRegistrationNumberFormats(string1With100Chars + "1", code, message, validation);
		}

		public void TestValidateKBC()
		{
			var message = "The KR KBC Registration number '{0}' is invalid.\r\nIt must not exceed 100 characters.";
			var code = OrganisationRegistry.RegistrationNumberFormatFields.KRKBC;
			Action<ZPropertyInfo> validation = x => KoreaSouthRegistrationNumberValidator.ValidateKBC(x);

			var string1With100Chars = "ABCDE67890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890";
			var string2With100Chars = "1234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890";

			AssertValidation(NotificationType.Error, string1With100Chars + "1", message, validation);
			AssertValidation(NotificationType.Error, string2With100Chars + "1", message, validation);

			AssertValidation(null, string1With100Chars, message, validation);
			AssertValidation(null, string2With100Chars, message, validation);

			AssertForRegistry_StrictEnforcementOfRegistrationNumberFormats(string1With100Chars + "1", code, message, validation);
		}

		public void TestValidateAEO()
		{
			var message = "KR AEO number should consist of 7 alphanumeric characters.";
			Action<ZPropertyInfo> validation = x => KoreaSouthRegistrationNumberValidator.ValidateAEO(x);

			AssertValidation(CargoWise.EntityFramework.NotificationType.MessageError, "ABCDE", message, validation);
			AssertValidation(CargoWise.EntityFramework.NotificationType.MessageError, "12345", message, validation);

			AssertValidation(CargoWise.EntityFramework.NotificationType.MessageError, "123456~", message, validation);
			AssertValidation(null, "1234567", message, validation);
			AssertValidation(null, "ABCDEAB", message, validation);
		}

		void AssertForRegistry_StrictEnforcementOfRegistrationNumberFormats(ZString value, ZString code, ZString message, Action<ZPropertyInfo> validation)
		{
			using (SetStrictEnforcementOfRegistrationNumberFormats(code, true))
			{
				AssertValidation(NotificationType.Error, value, message, validation);
			}

			using (SetStrictEnforcementOfRegistrationNumberFormats(code, false))
			{
				AssertValidation(NotificationType.Warning, value, message, validation);
			}

			IDisposable SetStrictEnforcementOfRegistrationNumberFormats(ZString registryCode, bool isValidated)
			{
				var collection = OrganisationRegistry.Instance.StrictEnforcementOfRegistrationNumberFormats.Value;
				collection.Cast<CodeDescriptionBool>().Single(x => x.Code == registryCode).Bool = isValidated;

				return OrganisationRegistry.Instance.StrictEnforcementOfRegistrationNumberFormats.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, collection);
			}
		}

		void AssertValidation(INotificationType notificationType, ZString value, ZString messagee, Action<ZPropertyInfo> validation)
		{
			var dummy = (DummyBusinessObject)Factory.New(TypeOfDummy);
			using (dummy.SuspendValidationTesting())
			{
				AssertNoNotifications("Pre-condition", dummy.Z0_NVarCharMaxInfo);

				dummy.Z0_NVarCharMax = value;
				validation.Invoke(dummy.Z0_NVarCharMaxInfo);

				if (notificationType == NotificationType.Error)
				{
					AssertHasError(dummy.Z0_NVarCharMaxInfo, string.Format(messagee, value));
				}
				else if (notificationType == NotificationType.Warning)
				{
					AssertHasWarning(dummy.Z0_NVarCharMaxInfo, string.Format(messagee, value));
				}
				else if (notificationType == CargoWise.EntityFramework.NotificationType.MessageError)
				{
					AssertHasMessageError(dummy.Z0_NVarCharMaxInfo, string.Format(messagee, value));
				}
				else
				{
					AssertNoNotifications(dummy.Z0_NVarCharMaxInfo);
				}
			}
		}
	}
}
