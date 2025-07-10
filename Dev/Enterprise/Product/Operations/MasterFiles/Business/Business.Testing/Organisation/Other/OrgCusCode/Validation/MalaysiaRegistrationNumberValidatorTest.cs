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
	sealed class MalaysiaRegistrationNumberValidatorTest : TestCaseWithDummy
	{
		public void TestValidateTINNumber()
		{
			var message = "The MY TIN number '{0}' is invalid. It should be C/CS/D/E/F/FA/PT/TA/TC/TN/TR/TP/J/LE + 10/11 digits, or IG + 9/10/11 digits, or it should be in format 'NNNNNNNNNNNN'.";
			var code = OrganisationRegistry.RegistrationNumberFormatFields.MYTIN;
			Action<ZPropertyInfo> validation = x => MalaysiaRegistrationNumberValidator.ValidateTIN(x);

			var nonIndividualCodes = new[] { "C", "CS", "D", "E", "F", "FA", "PT", "TA", "TC", "TN", "TR", "TP", "J", "LE" };

			foreach (var nonIndividualCode in nonIndividualCodes)
			{
				AssertValidation(NotificationType.Error, $"{nonIndividualCode}012345678", message, validation);
				AssertValidation(NotificationType.Error, $"{nonIndividualCode}012345678901", message, validation);
				AssertValidation(null, $"{nonIndividualCode}0123456789", message, validation);
				AssertValidation(null, $"{nonIndividualCode}01234567890", message, validation);
			}

			AssertValidation(NotificationType.Error, "IG01234567", message, validation);
			AssertValidation(NotificationType.Error, "IG012345678901", message, validation);
			AssertValidation(null, "IG012345678", message, validation);
			AssertValidation(null, "IG0123456789", message, validation);
			AssertValidation(null, "IG01234567890", message, validation);

			AssertValidation(NotificationType.Error, "01234567890", message, validation);
			AssertValidation(NotificationType.Error, "0123456789012", message, validation);
			AssertValidation(null, "012345678901", message, validation);

			AssertValidation(null, "EI00000000010", message, validation);
			AssertValidation(null, "EI00000000020", message, validation);
			AssertValidation(null, "EI00000000030", message, validation);

			AssertForRegistry_StrictEnforcementOfRegistrationNumberFormats("123", code, message, validation);
		}

		public void TestValidateOtherBusinessCode()
		{
			var message = "The first character must be a valid 'other' code. Valid codes include:\r\n\r\n  C   = REGISTRAR OF SOCIETIES\r\n  D   = REGISTRAR OF COOPERATIVE\r\n  E   = NATIONAL REGISTRATION DEPARTMENT\r\n  F   = DEPARTMENT OF JUSTICE\r\n  G   = STATUTORY\r\n  H   = STATE ECONOMY DEVELOPMENT CORPOR\r\n  I   = INDIVIDUAL\r\n  J   = REGISTRAR OF PROF. BODIES\r\n  K   = FOREIGN NATIONAL (PASSPORT)\r\n  L   = POLICE\r\n  M   = ARMED FORCES\r\n  N   = DIPLOMATIC / AMBASSIES\r\n  P   = GOVERNMENT DEPARTMENT\r\n  R   = RULERS\r\n  Z   = OTHERS";
			var code = OrganisationRegistry.RegistrationNumberFormatFields.MYOTH;
			Action<ZPropertyInfo> validation = x => MalaysiaRegistrationNumberValidator.ValidateOtherBusinessCode(x);

			var validOtherBusinessCodes = new[] { "C", "D", "E", "F", "G", "H", "I", "J", "K", "L", "M", "N", "P", "R", "Z" };

			foreach (var validOtherBusinessCode in validOtherBusinessCodes)
			{
				AssertValidation(null, $"{validOtherBusinessCode}0123456789", message, validation);
			}

			AssertValidation(NotificationType.Error, "A01234567", message, validation);
			AssertValidation(NotificationType.Error, "01234567890", message, validation);

			AssertForRegistry_StrictEnforcementOfRegistrationNumberFormats("123", code, message, validation);
		}

		public void TestValidatePersonalIdentificationCardNumber()
		{
			var message = "The entered Personal Identification Card Number '{0}' is invalid. It should be in format NNNNNNNNNNNN with 12 numeric digits";
			var code = OrganisationRegistry.RegistrationNumberFormatFields.MYPIC;
			Action<ZPropertyInfo> validation = x => MalaysiaRegistrationNumberValidator.ValidatePersonalIdentificationCardNumber(x);

			AssertValidation(null, "012345678901", message, validation);
			AssertForRegistry_StrictEnforcementOfRegistrationNumberFormats("A01234567", code, message, validation);
			AssertForRegistry_StrictEnforcementOfRegistrationNumberFormats("01234567890", code, message, validation);
			AssertForRegistry_StrictEnforcementOfRegistrationNumberFormats("0123456789012", code, message, validation);
			AssertForRegistry_StrictEnforcementOfRegistrationNumberFormats("123", code, message, validation);
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

		void AssertValidation(INotificationType notificationType, ZString value, ZString message, Action<ZPropertyInfo> validation)
		{
			var dummy = (DummyBusinessObject)Factory.New(TypeOfDummy);
			using (dummy.SuspendValidationTesting())
			{
				AssertNoNotifications("Pre-condition", dummy.Z0_NVarCharMaxInfo);

				dummy.Z0_NVarCharMax = value;
				validation.Invoke(dummy.Z0_NVarCharMaxInfo);

				if (notificationType == NotificationType.Error)
				{
					AssertHasError(dummy.Z0_NVarCharMaxInfo, string.Format(message, value));
				}
				else if (notificationType == NotificationType.Warning)
				{
					AssertHasWarning(dummy.Z0_NVarCharMaxInfo, string.Format(message, value));
				}
				else
				{
					AssertNoNotifications(dummy.Z0_NVarCharMaxInfo);
				}
			}
		}
	}
}
