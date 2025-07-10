using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Customs.Business.Testing
{
	[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1052:StaticHolderTypesShouldBeStaticOrNotInheritable", Justification = "External solution inherits this class implementation.")]
	public class ValidationTestHelper
	{
		#region List Validation

		#region Message Error

		public static void AssertInvalidCodeOrEmptyMessageError(ZPropertyInfo info, ZString invalidCode, ZString validCode)
		{
			AssertInvalidCodeOrEmptyMessageError(info, new[] { invalidCode }, new[] { validCode });
		}

		public static void AssertInvalidCodeOrEmptyMessageError(ZPropertyInfo info, ZString[] invalidCodes, ZString[] validCodes)
		{
			AssertInvalidCodeOrEmptyMessageError(info, invalidCodes, validCodes, ListValidation.InvalidCodeMessageError.ToString());
		}

		public static void AssertInvalidCodeOrEmptyMessageError(ZPropertyInfo info, ZString invalidCode, ZString validCode, string invalidNotificationText)
		{
			AssertInvalidCodeOrEmptyMessageError(info, new[] { invalidCode }, new[] { validCode }, invalidNotificationText);
		}

		public static void AssertInvalidCodeOrEmptyMessageError(ZPropertyInfo info, ZString[] invalidCodes, ZString[] validCodes, string invalidNotificationText)
		{
			AssertYouHaveNotEnteredMessageError(info);
			AssertInvalidCodeMessageErrorCore(info, invalidCodes, validCodes, invalidNotificationText);
		}

		public static void AssertInvalidCodeMessageError(ZPropertyInfo info, ZString invalidCode, ZString validCode)
		{
			AssertInvalidCodeMessageError(info, new[] { invalidCode }, new[] { validCode });
		}

		public static void AssertInvalidCodeMessageError(ZPropertyInfo info, ZString[] invalidCodes, ZString[] validCodes)
		{
			AssertInvalidCodeMessageError(info, invalidCodes, validCodes, ListValidation.InvalidCodeMessageError.ToString());
		}

		public static void AssertInvalidCodeMessageError(ZPropertyInfo info, ZString invalidCode, ZString validCode, string invalidNotificationText)
		{
			AssertInvalidCodeMessageError(info, new[] { invalidCode }, new[] { validCode }, invalidNotificationText);
		}

		public static void AssertInvalidCodeMessageError(ZPropertyInfo info, ZString[] invalidCodes, ZString[] validCodes, string invalidNotificationText)
		{
			AssertInvalidCodeMessageErrorCore(info, invalidCodes, validCodes, invalidNotificationText);
		}

		static void AssertInvalidCodeMessageErrorCore(ZPropertyInfo info, ZString[] invalidCodes, ZString[] validCodes, string invalidNotificationText)
		{
			if (info.PropertyType == typeof(ZString))
			{
				info.Value = ZString.Empty;
				foreach (var invalidCode in invalidCodes)
				{
					info.Value = invalidCode;
					TestCaseWithFactory.AssertHasMessageErrorContaining(info, invalidNotificationText);
				}
				foreach (var validCode in validCodes)
				{
					info.Value = validCode;
					TestCaseWithFactory.AssertNoMessageErrorContaining(info, invalidNotificationText);
				}
			}
			else
			{
				ThrowNotSupportedTypeException(info);
			}
		}

		#endregion

		#region Error

		public static void AssertErrorIfInvalidCode(ZPropertyInfo info, ZString invalidCode, ZString validCode, string invalidNotificationText = null)
		{
			AssertErrorIfInvalidValue(info, invalidCode, validCode, invalidNotificationText ?? ListValidation.InvalidCodeError);
		}

		public static void AssertErrorIfInvalidPK(ZPropertyInfo info, ZGuid invalidPk, ZGuid validPk, string invalidNotificationText = null)
		{
			AssertErrorIfInvalidValue(info, invalidPk, validPk, invalidNotificationText ?? ListValidation.InvalidCodeError);
		}

		static void AssertErrorIfInvalidValue(ZPropertyInfo info, IZType invalidValue, IZType validValue, string invalidNotificationText)
		{
			if (info.PropertyType == invalidValue.GetType() && info.PropertyType == validValue.GetType())
			{
				SetEmptyValue(info);
				info.Value = invalidValue;
				TestCaseWithFactory.AssertHasErrorContaining(info, invalidNotificationText);
				info.Value = validValue;
				TestCaseWithFactory.AssertNoErrorContaining(info, invalidNotificationText);
			}
			else
			{
				ThrowNotSupportedTypeException(info);
			}
		}

		#endregion

		#endregion

		#region Mandatory Validation

		#region Message Error

		public static void AssertYouHaveNotEnteredMessageError(ZPropertyInfo info)
		{
			AssertYouHaveNotEnteredMessageError(info, MandatoryValidation.YouHaveNotEntered);
		}

		public static void AssertYouHaveNotEnteredMessageError(ZPropertyInfo info, string notificationText)
		{
			AssertYouHaveNotEnteredMessageError(info, notificationText, null);
		}

		public static void AssertYouHaveNotEnteredMessageError(ZPropertyInfo info, string notificationText, string testCaseDescription)
		{
			SetNotEmptyValue(info);
			SetEmptyValue(info);
			TestCaseWithFactory.AssertHasMessageErrorContaining(testCaseDescription, info, notificationText);
			SetNotEmptyValue(info);
			TestCaseWithFactory.AssertNoMessageErrorContaining(testCaseDescription, info, notificationText);
		}

		public static void AssertFieldIsNotMandatory(ZPropertyInfo info)
		{
			AssertFieldIsNotMandatory(info, MandatoryValidation.YouHaveNotEntered);
		}

		public static void AssertFieldIsNotMandatory(ZPropertyInfo info, string notificationText)
		{
			AssertFieldIsNotMandatory(info, notificationText, info.GetMessageErrors, null);
		}

		public static void AssertFieldIsNotMandatory(ZPropertyInfo info, string notificationText, string testCaseDescription)
		{
			AssertFieldIsNotMandatory(info, notificationText, info.GetMessageErrors, testCaseDescription);
		}

		static void AssertFieldIsNotMandatory(ZPropertyInfo info, string notificationText, Func<IEnumerable<INotification>> getNotifications, string testCaseDescription)
		{
			testCaseDescription = string.IsNullOrWhiteSpace(testCaseDescription) ? null : string.Format("TestCase '{0}': ", testCaseDescription);
			SetNotEmptyValue(info);
			SetEmptyValue(info);
			var notifications = getNotifications();
			TestCaseWithFactory.Assert(string.Format("{0}The property '{1}' should not have '{2}' message error", testCaseDescription, info.Name, notificationText),
				!notifications.Any(n => n.Message.Contains(notificationText)));
		}

		public static void AssertFieldIsMandatory(ZPropertyInfo info, string notificationText)
		{
			AssertFieldIsMandatory(info, notificationText, info.GetMessageErrors, null);
		}

		static void AssertFieldIsMandatory(ZPropertyInfo info, string notificationText, Func<IEnumerable<INotification>> getNotifications, string testCaseDescription)
		{
			testCaseDescription = string.IsNullOrWhiteSpace(testCaseDescription) ? null : string.Format("TestCase '{0}': ", testCaseDescription);
			SetNotEmptyValue(info);
			SetEmptyValue(info);
			var notifications = getNotifications();
			TestCaseWithFactory.Assert(string.Format("{0}The property '{1}' should have '{2}' message error", testCaseDescription, info.Name, notificationText),
				notifications.Any(n => n.Message.Contains(notificationText)));
		}

		public static void AssertIfIsEnteredMessageError(ZPropertyInfo info)
		{
			AssertIfIsEnteredMessageError(info, MandatoryValidation.DoNotEntered);
		}

		public static void AssertIfIsEnteredMessageError(ZPropertyInfo info, string notificationText)
		{
			AssertIfIsEnteredMessageError(info, notificationText, null);
		}

		public static void AssertIfIsEnteredMessageError(ZPropertyInfo info, string notificationText, string testCaseDescription)
		{
			SetNotEmptyValue(info);
			TestCaseWithFactory.AssertHasMessageErrorContaining(testCaseDescription, info, notificationText);
			SetEmptyValue(info);
			TestCaseWithFactory.AssertNoMessageErrorContaining(testCaseDescription, info, notificationText);
		}

		#region (Message)ErrorIfNotEnteredWhenOtherPropertyHasValue

		public static void AssertErrorIfNotEnteredWhenOtherPropertyIsEntered(ZPropertyInfo propertyInfoToCheck, ZPropertyInfo otherPropertyInfo, bool combineAssertions = true)
		{
			if (combineAssertions)
			{
				TestCaseWithFactory.CombineAssertions(() =>
				{
					AssertErrorIfNotEnteredWhenOtherPropertyIsEntered(propertyInfoToCheck, otherPropertyInfo);
				});
			}
			else
			{
				AssertErrorIfNotEnteredWhenOtherPropertyIsEntered(propertyInfoToCheck, otherPropertyInfo);
			}
		}

		public static void AssertErrorIfNotEnteredWhenOtherPropertyHasValue(ZPropertyInfo propertyInfoToCheck, ZPropertyInfo otherPropertyInfo, IZType valueThatShouldTriggerValidation, bool combineAssertions = true)
		{
			AssertErrorIfNotEnteredWhenOtherPropertyHasValue(propertyInfoToCheck, otherPropertyInfo, new[] { valueThatShouldTriggerValidation }, combineAssertions);
		}

		public static void AssertErrorIfNotEnteredWhenOtherPropertyHasValue(ZPropertyInfo propertyInfoToCheck, ZPropertyInfo otherPropertyInfo, IEnumerable<IZType> valuesThatShouldTriggerValidation, bool combineAssertions = true)
		{
			if (combineAssertions)
			{
				TestCaseWithFactory.CombineAssertions(() =>
				{
					AssertErrorIfNotEnteredWhenOtherPropertyHasValue(propertyInfoToCheck, otherPropertyInfo, valuesThatShouldTriggerValidation);
				});
			}
			else
			{
				AssertErrorIfNotEnteredWhenOtherPropertyHasValue(propertyInfoToCheck, otherPropertyInfo, valuesThatShouldTriggerValidation);
			}
		}

		public static void AssertMessageErrorIfNotEnteredWhenOtherPropertyIsEntered(ZPropertyInfo propertyInfoToCheck, ZPropertyInfo otherPropertyInfo, bool combineAssertions = true, string expectedMessage = "")
		{
			if (combineAssertions)
			{
				TestCaseWithFactory.CombineAssertions(() =>
				{
					AssertMessageErrorIfNotEnteredWhenOtherPropertyIsEntered(propertyInfoToCheck, otherPropertyInfo, expectedMessage);
				});
			}
			else
			{
				AssertMessageErrorIfNotEnteredWhenOtherPropertyIsEntered(propertyInfoToCheck, otherPropertyInfo, expectedMessage);
			}
		}

		public static void AssertMessageErrorIfNotEnteredWhenOtherPropertyHasValue(ZPropertyInfo propertyInfoToCheck, ZPropertyInfo otherPropertyInfo, IZType valueThatShouldTriggerValidation, bool combineAssertions = true, string expectedMessage = MandatoryValidation.YouHaveNotEntered)
		{
			AssertMessageErrorIfNotEnteredWhenOtherPropertyHasValue(propertyInfoToCheck, otherPropertyInfo, new[] { valueThatShouldTriggerValidation }, combineAssertions, expectedMessage);
		}

		public static void AssertMessageErrorIfNotEnteredWhenOtherPropertyHasValue(ZPropertyInfo propertyInfoToCheck, ZPropertyInfo otherPropertyInfo, IEnumerable<IZType> valuesThatShouldTriggerValidation, bool combineAssertions = true, string expectedMessage = MandatoryValidation.YouHaveNotEntered)
		{
			if (combineAssertions)
			{
				TestCaseWithFactory.CombineAssertions(() =>
				{
					AssertMessageErrorIfNotEnteredWhenOtherPropertyHasValue(propertyInfoToCheck, otherPropertyInfo, valuesThatShouldTriggerValidation, expectedMessage);
				});
			}
			else
			{
				AssertMessageErrorIfNotEnteredWhenOtherPropertyHasValue(propertyInfoToCheck, otherPropertyInfo, valuesThatShouldTriggerValidation, expectedMessage);
			}
		}

		static void AssertErrorIfNotEnteredWhenOtherPropertyIsEntered(ZPropertyInfo propertyInfoToCheck, ZPropertyInfo otherPropertyInfo)
		{
			var error = MandatoryValidation.GetErrorWhenPropertyNotEnteredAndOtherPropertyIsEntered(propertyInfoToCheck, otherPropertyInfo);

			SetNotEmptyValue(otherPropertyInfo);
			SetNotEmptyValue(propertyInfoToCheck);
			TestCaseWithFactory.AssertNoError("Both are entered", propertyInfoToCheck, error);

			SetNotEmptyValue(otherPropertyInfo);
			SetEmptyValue(propertyInfoToCheck);
			TestCaseWithFactory.AssertHasError($"{propertyInfoToCheck.Description} is entered, so should the {propertyInfoToCheck.Description}", propertyInfoToCheck, error);

			SetEmptyValue(otherPropertyInfo);
			SetEmptyValue(propertyInfoToCheck);
			TestCaseWithFactory.AssertNoError("Neither is entered", propertyInfoToCheck, error);
		}

		static void AssertErrorIfNotEnteredWhenOtherPropertyHasValue(ZPropertyInfo propertyInfoToCheck, ZPropertyInfo otherPropertyInfo, IEnumerable<IZType> valuesThatShouldTriggerValidation)
		{
			SetNotEmptyValue(otherPropertyInfo);
			SetEmptyValue(propertyInfoToCheck);
			var errorPart = "Please enter";
			TestCaseWithFactory.AssertNoErrorContaining("Other property is entered, but not with a value that should trigger validation", propertyInfoToCheck, errorPart);

			foreach (var valueThatShouldTriggerValidation in valuesThatShouldTriggerValidation)
			{
				otherPropertyInfo.Value = valueThatShouldTriggerValidation;
				SetEmptyValue(propertyInfoToCheck);
				TestCaseWithFactory.AssertHasErrorContaining($"{otherPropertyInfo.HumanReadableName} is entered, with a value that should trigger validation ({valueThatShouldTriggerValidation})", propertyInfoToCheck, errorPart);
				SetNotEmptyValue(propertyInfoToCheck);
				TestCaseWithFactory.AssertNoErrorContaining($"{otherPropertyInfo.HumanReadableName} is entered, with a value that should trigger validation ({valueThatShouldTriggerValidation}), however, {propertyInfoToCheck.Description} is entered", propertyInfoToCheck, errorPart);
			}
		}

		static void AssertMessageErrorIfNotEnteredWhenOtherPropertyIsEntered(ZPropertyInfo propertyInfoToCheck, ZPropertyInfo otherPropertyInfo, string expectedMessage)
		{
			var messageError = string.IsNullOrEmpty(expectedMessage) ? MandatoryValidation.GetMessageErrorWhenPropertyNotEnteredAndOtherPropertyIsEntered(propertyInfoToCheck, otherPropertyInfo) : expectedMessage;

			SetNotEmptyValue(otherPropertyInfo);
			SetNotEmptyValue(propertyInfoToCheck);
			TestCaseWithFactory.AssertNoMessageError("Both are entered", propertyInfoToCheck, messageError);

			SetNotEmptyValue(otherPropertyInfo);
			SetEmptyValue(propertyInfoToCheck);
			TestCaseWithFactory.AssertHasMessageError($"{propertyInfoToCheck.Description} is entered, so should the {propertyInfoToCheck.Description}", propertyInfoToCheck, messageError);

			SetEmptyValue(otherPropertyInfo);
			SetEmptyValue(propertyInfoToCheck);
			TestCaseWithFactory.AssertNoMessageError("Neither is entered", propertyInfoToCheck, messageError);
		}

		static void AssertMessageErrorIfNotEnteredWhenOtherPropertyHasValue(ZPropertyInfo propertyInfoToCheck, ZPropertyInfo otherPropertyInfo, IEnumerable<IZType> valuesThatShouldTriggerValidation, string expectedMessage = MandatoryValidation.YouHaveNotEntered)
		{
			SetNotEmptyValue(otherPropertyInfo);
			SetEmptyValue(propertyInfoToCheck);
			TestCaseWithFactory.AssertNoMessageErrorContaining("Other property is entered, but not with a value that should trigger validation", propertyInfoToCheck, expectedMessage);

			foreach (var valueThatShouldTriggerValidation in valuesThatShouldTriggerValidation)
			{
				otherPropertyInfo.Value = valueThatShouldTriggerValidation;
				SetEmptyValue(propertyInfoToCheck);
				TestCaseWithFactory.AssertHasMessageErrorContaining($"{otherPropertyInfo.HumanReadableName} is entered, with a value that should trigger validation ({valueThatShouldTriggerValidation})", propertyInfoToCheck, expectedMessage);
				SetNotEmptyValue(propertyInfoToCheck);
				TestCaseWithFactory.AssertNoMessageErrorContaining($"{otherPropertyInfo.HumanReadableName} is entered, with a value that should trigger validation ({valueThatShouldTriggerValidation}), however, {propertyInfoToCheck.Description} is entered", propertyInfoToCheck, expectedMessage);
			}
		}

		#endregion

		#endregion

		#region Error

		public static void AssertErrorFieldIsNotMandatory(ZPropertyInfo info)
		{
			AssertFieldIsNotMandatory(info, MandatoryValidation.MustBeEntered, info.GetErrors, null);
		}

		public static void AssertErrorFieldIsNotMandatory(ZPropertyInfo info, string testCaseDescription)
		{
			AssertFieldIsNotMandatory(info, MandatoryValidation.MustBeEntered, info.GetErrors, testCaseDescription);
		}

		public static void AssertErrorIfNotEntered(ZPropertyInfo info, string notificationText = null)
		{
			SetNotEmptyValue(info);
			SetEmptyValue(info);
			TestCaseWithFactory.AssertHasErrorContaining(info, notificationText ?? MandatoryValidation.MustBeEntered);
			SetNotEmptyValue(info);
			TestCaseWithFactory.AssertNoErrorContaining(info, notificationText ?? MandatoryValidation.MustBeEntered);
		}

		public static void AssertErrorIfEntered(ZPropertyInfo info, string notificationText = null)
		{
			SetEmptyValue(info);
			SetNotEmptyValue(info);
			TestCaseWithFactory.AssertHasErrorContaining(info, notificationText ?? MandatoryValidation.DoNotEntered);
			SetEmptyValue(info);
			TestCaseWithFactory.AssertNoErrorContaining(info, notificationText ?? MandatoryValidation.DoNotEntered);
		}

		#endregion

		#region Warning

		public static void AssertWarningIfNotEntered(ZPropertyInfo info, string notificationText)
		{
			AssertWarningIfNotEntered(info, notificationText, null);
		}

		public static void AssertWarningIfNotEntered(ZPropertyInfo info, string notificationText, string testCaseDescription)
		{
			SetNotEmptyValue(info);
			SetEmptyValue(info);
			TestCaseWithFactory.AssertHasWarningContaining(testCaseDescription, info, notificationText);
			SetNotEmptyValue(info);
			TestCaseWithFactory.AssertNoWarningContaining(testCaseDescription, info, notificationText);
		}

		public static void AssertNoWarningIfNotEntered(ZPropertyInfo info, string notificationText)
		{
			AssertFieldIsNotMandatory(info, notificationText, info.GetWarnings, null);
		}

		public static void AssertNoWarningIfNotEntered(ZPropertyInfo info, string notificationText, string testCaseDescription)
		{
			AssertFieldIsNotMandatory(info, notificationText, info.GetWarnings, testCaseDescription);
		}

		#endregion

		#endregion

		#region Numeric Value Validation

		public static void AssertErrorIfValueIsNegative(ZPropertyInfo info)
		{
			AssertErrorIfValueIsNegative(info, null);
		}

		public static void AssertErrorIfValueIsNegative(ZPropertyInfo info, string testCaseDescription)
		{
			SetEmptyValue(info);
			SetNegativeValue(info);
			TestCaseWithFactory.AssertHasErrorContaining($"{testCaseDescription}; Negative", info, MandatoryValidation.ValueCannotBeNegative);
			SetNotEmptyValue(info);
			TestCaseWithFactory.AssertNoErrorContaining($"{testCaseDescription}; Not Empty", info, MandatoryValidation.ValueCannotBeNegative);
		}

		public static void AssertValueCannotBeNegativeMessageError(ZPropertyInfo info)
		{
			AssertValueCannotBeNegativeMessageError(info, null);
		}

		public static void AssertValueCannotBeNegativeMessageError(ZPropertyInfo info, string testCaseDescription)
		{
			SetEmptyValue(info);
			SetNegativeValue(info);
			TestCaseWithFactory.AssertHasMessageErrorContaining($"{testCaseDescription}; Negative", info, MandatoryValidation.ValueCannotBeNegative);
			SetNotEmptyValue(info);
			TestCaseWithFactory.AssertNoMessageErrorContaining($"{testCaseDescription}; Not Empty", info, MandatoryValidation.ValueCannotBeNegative);
		}

		public static void AssertValueCannotBeZeroMessageError(ZPropertyInfo info)
		{
			AssertValueCannotBeZeroMessageError(info, null);
		}

		public static void AssertValueCannotBeZeroMessageError(ZPropertyInfo info, string testCaseDescription)
		{
			SetNotEmptyValue(info);
			SetEmptyValue(info);
			TestCaseWithFactory.AssertHasMessageErrorContaining($"{testCaseDescription}; Empty", info, MandatoryValidation.ValueCannotBeZero);
			SetNotEmptyValue(info);
			TestCaseWithFactory.AssertNoMessageErrorContaining($"{testCaseDescription}; Entered", info, MandatoryValidation.ValueCannotBeZero);
		}

		#endregion

		#region Related Object Validation

		public static void AssertRelatedObjectMaxLengthValidation(ZPropertyInfo info, int maxLength, string description)
		{
			if (info.PropertyType == typeof(ZString))
			{
				var value = ZString.Empty;
				var exceededLength = maxLength + 2;
				var warning = string.Format("The length of data entered into {2} ({0}) exceeds the maximum allowed."
											+ " Only the first {1} characters will be transmitted to Customs.", exceededLength, maxLength, description);

				SetEmptyValue(info);
				info.Value = value.PadRight(exceededLength, '1');
				TestCaseWithFactory.AssertHasWarningContaining(info, warning);

				info.Value = value.PadRight(maxLength, '1');
				TestCaseWithFactory.AssertNoWarningContaining(info, warning);
			}
			else
			{
				ThrowNotSupportedTypeException(info);
			}
		}

		#endregion

		#region Implementation

		protected static void SetEmptyValue(ZPropertyInfo info)
		{
			if (info.PropertyType == typeof(ZString))
			{
				info.Value = ZString.Empty;
			}
			else if (info.PropertyType == typeof(ZInt))
			{
				info.Value = ZInt.Zero;
			}
			else if (info.PropertyType == typeof(ZDecimal))
			{
				info.Value = ZDecimal.Zero;
			}
			else if (info.PropertyType == typeof(ZDateTime))
			{
				info.Value = ZDateTime.Empty;
			}
			else if (info.PropertyType == typeof(ZDateTimeOffset))
			{
				info.Value = ZDateTimeOffset.Empty;
			}
			else if (info.PropertyType == typeof(ZDate))
			{
				info.Value = ZDate.Empty;
			}
			else if (info.PropertyType == typeof(ZGuid))
			{
				info.Value = ZGuid.Empty;
			}
			else if (info.PropertyType == typeof(ZBool))
			{
				info.Value = ZBool.False;
			}
			else if (info.PropertyType == typeof(ZShort))
			{
				info.Value = ZShort.Zero;
			}
			else if (info.PropertyType == typeof(ZByte))
			{
				info.Value = ZByte.Zero;
			}
			else
			{
				ThrowNotSupportedTypeException(info);
			}
		}

		protected static void SetNotEmptyValue(ZPropertyInfo info)
		{
			if (info.PropertyType == typeof(ZString))
			{
				info.Value = new ZString("A");
				if ((ZString)info.Value == ZString.Empty)
				{
					info.Value = new ZString("9");
				}
			}
			else if (info.PropertyType == typeof(ZInt))
			{
				info.Value = (ZInt)1;
			}
			else if (info.PropertyType == typeof(ZDecimal))
			{
				info.Value = (ZDecimal)1;
			}
			else if (info.PropertyType == typeof(ZDateTime))
			{
				info.Value = ZDateTime.Now;
			}
			else if (info.PropertyType == typeof(ZDateTimeOffset))
			{
				info.Value = ZDateTimeOffset.Now;
			}
			else if (info.PropertyType == typeof(ZDate))
			{
				info.Value = ZDate.Today;
			}
			else if (info.PropertyType == typeof(ZGuid))
			{
				info.Value = ZGuid.NewZGuid();
			}
			else if (info.PropertyType == typeof(ZBool))
			{
				info.Value = ZBool.True;
			}
			else if (info.PropertyType == typeof(ZShort))
			{
				info.Value = (ZShort)2;
			}
			else if (info.PropertyType == typeof(ZByte))
			{
				info.Value = (ZByte)1;
			}
			else
			{
				ThrowNotSupportedTypeException(info);
			}
		}

		static void SetNegativeValue(ZPropertyInfo info)
		{
			if (info.PropertyType == typeof(ZInt))
			{
				info.Value = (ZInt)(-10);
			}
			else if (info.PropertyType == typeof(ZDecimal))
			{
				info.Value = (ZDecimal)(-10);
			}
			else if (info.PropertyType == typeof(ZShort))
			{
				info.Value = (ZShort)(-10);
			}
			else
			{
				ThrowNotSupportedTypeException(info);
			}
		}

		protected static void ThrowNotSupportedTypeException(ZPropertyInfo info)
		{
			throw new NotSupportedException("Not supported type: " + info.PropertyType);
		}

		#endregion
	}
}
