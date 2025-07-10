using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.ResourceStrings.Grammar;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Security;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.TW.Business
{
	public abstract class LicensingMessageSendingObjectParentValidation : MessageSendingValidation
	{
		public JobDeclaration Declaration { get; }
		public LicensingMessageSendingObjectParent Parent { get; }

		protected internal LicensingMessageSendingObjectParentValidation(LicensingMessageSendingObjectParent parent, JobDeclaration declaration, IEnumerable<INotification> messageErrors, SecurityCheckpoint sendMessageWithErrorsSecurityCheckpoint, bool refreshValidation = true)
			: base(declaration, messageErrors, sendMessageWithErrorsSecurityCheckpoint, refreshValidation)
		{
			this.Declaration = Argument.NotNull(declaration, nameof(declaration));
			this.Parent = Argument.NotNull(parent, nameof(parent));
		}

		string GetHumanReadableName(ZPropertyInfo propertyInfo, string columnName)
		{
			if (string.IsNullOrEmpty(columnName))
			{
				columnName = MandatoryValidation.GetErrorFieldFromProperyInfo(propertyInfo);
			}
			return columnName;
		}

		protected void AddErrorOnProperty(IValidationInternals validator, ZPropertyInfo propertyInfo, string errorMessage)
		{
			validator.Validate(propertyInfo, () => propertyInfo.AddMessageError(errorMessage));
		}

		protected void AddNotInListError(MessageSendingNotificationCollection errorColelction, IValidationInternals validator, ZPropertyInfo propertyInfo, string prefix, string errorMessage = null, string columnName = null)
		{
			if (string.IsNullOrEmpty(errorMessage))
			{
				columnName = GetHumanReadableName(propertyInfo, columnName);
				errorMessage = $"{columnName}: {ListValidation.InvalidCodeMessageError}";
			}
			AddErrorOnProperty(validator, propertyInfo, errorMessage);
			errorColelction.AddError($"{prefix}: {errorMessage}");
		}

		protected void AddErrorAfterValidation(MessageSendingNotificationCollection errorColelction, Action validateAction, INotificationProvider notifications, string prefix)
		{
			validateAction();
			foreach (var notification in notifications.GetErrors())
			{
				errorColelction.AddError($"{prefix}: {notification.Message}");
			}
			foreach (var notification in notifications.GetMessageErrors())
			{
				errorColelction.AddError($"{prefix}: {notification.Message}");
			}
		}

		protected bool AddErrorIfEmpty(MessageSendingNotificationCollection errorColelction, IValidationInternals validator, ZPropertyInfo propertyInfo, string prefix, string errorMessage = null, string columnName = null)
		{
			var result = false;
			if (propertyInfo is ZPropertyInfo<ZString> stringProperty && stringProperty.Value.IsEmpty)
			{
				result = true;
				if (string.IsNullOrEmpty(errorMessage))
				{
					columnName = GetHumanReadableName(stringProperty, columnName);
					errorMessage = MandatoryValidation.YouHaveNotEnteredMessage(columnName);
				}
				AddErrorOnProperty(validator, propertyInfo, errorMessage);
				errorColelction.AddError($"{prefix}: {errorMessage}");
			}
			return result;
		}

		protected void AddErrorIfEmptyOrNotInList(MessageSendingNotificationCollection errorColelction, IValidationInternals validator, ZPropertyInfo propertyInfo, CodeDescriptionPairList list, string prefix, string errorMessageForEmpty = null, string errorMessageForNotInList = null, string columnName = null)
		{
			if (propertyInfo is ZPropertyInfo<ZString> stringProperty)
			{
				if (!AddErrorIfEmpty(errorColelction, validator, stringProperty, prefix, errorMessageForEmpty, columnName) && !list.ContainsCode(stringProperty.Value))
				{
					AddNotInListError(errorColelction, validator, propertyInfo, prefix, errorMessageForNotInList, columnName);
				}
			}
		}

		protected void AddErrorIfEmptyOrNotInList(MessageSendingNotificationCollection errorColelction, IValidationInternals validator, ZPropertyInfo propertyInfo, IEnumerable<ZString> list, string prefix, string errorMessageForEmpty = null, string errorMessageForNotInList = null, string columnName = null)
		{
			if (propertyInfo is ZPropertyInfo<ZString> stringProperty)
			{
				if (!AddErrorIfEmpty(errorColelction, validator, stringProperty, prefix, errorMessageForEmpty, columnName) && !list.Contains(stringProperty.Value))
				{
					AddNotInListError(errorColelction, validator, propertyInfo, prefix, errorMessageForNotInList, columnName);
				}
			}
		}

		protected void AddErrorIfNotGreaterThan0(MessageSendingNotificationCollection errorColelction, IValidationInternals validator, ZPropertyInfo propertyInfo, string prefix, string errorMessage = null, string columnName = null)
		{
			var value = propertyInfo.Value;
			if (value.DataType.IsNumeric)
			{
				bool hasError;
				if (propertyInfo is ZPropertyInfoLong)
				{
					hasError = new ZLong(value) <= ZLong.Zero;
				}
				else
				{
					hasError = new ZDecimal(value) <= ZDecimal.Zero;
				}

				if (hasError)
				{
					if (string.IsNullOrEmpty(errorMessage))
					{
						columnName = GetHumanReadableName(propertyInfo, columnName);
						errorMessage = $"Please enter {Grammar.Instance.IndefiniteArticlePrefix(columnName)}'{columnName}' greater than 0.";
					}
					AddErrorOnProperty(validator, propertyInfo, errorMessage);
					errorColelction.AddError($"{prefix}: {errorMessage}");
				}
			}
		}
	}
}
