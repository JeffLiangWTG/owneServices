using System;
using System.Collections.Generic;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWiseNotificationType = CargoWise.ComponentModel.NotificationType;

namespace Enterprise.Customs.Business
{
	public struct CusAuthorisationRuleRequirement
	{
		public CusAuthorisationRuleRequirement(string ruleType, int minRequired, int maxAllowed)
			: this(ruleType, minRequired, maxAllowed, null, null, CargoWiseNotificationType.Error)
		{
		}

		public CusAuthorisationRuleRequirement(string ruleType, int minRequired, int maxAllowed, Func<bool> additionalMinAuthorisationCheck, Func<string, string> additionalValidatorOnValue)
			: this(ruleType, minRequired, maxAllowed, additionalMinAuthorisationCheck, additionalValidatorOnValue, CargoWiseNotificationType.Error)
		{
		}

		public CusAuthorisationRuleRequirement(string ruleType, int minRequired, int maxAllowed, Func<bool> additionalMinAuthorisationCheck, Func<string, string> additionalValidatorOnValue, INotificationType notificationType)
		{
			RuleType = Argument.NotNullOrEmpty(ruleType, nameof(ruleType));
			MinRequired = minRequired;
			MaxAllowed = maxAllowed;
			AdditionalMinAuthorisationCheck = additionalMinAuthorisationCheck;
			NotificationType = notificationType ?? CargoWiseNotificationType.Error;
			var additionalValidatorOnValueCollection = new List<Func<string, AdditionalValidatorResult>>();
			if (additionalValidatorOnValue != null)
			{
				Argument.NotNull(notificationType, nameof(notificationType));
				additionalValidatorOnValueCollection.Add(ConvertAdditionalValidatorOnValueFunction(additionalValidatorOnValue, NotificationType));
			}
			AdditionalValidatorOnValueCollection = additionalValidatorOnValueCollection;
		}

		public string RuleType { get; }
		public int MaxAllowed { get; }
		public int MinRequired { get; }
		public Func<bool> AdditionalMinAuthorisationCheck { get; }
		public IEnumerable<Func<string, AdditionalValidatorResult>> AdditionalValidatorOnValueCollection { get; }
		public INotificationType NotificationType { get; }
		public override bool Equals(object obj)
		{
			var result = false;
			if (obj is CusAuthorisationRuleRequirement rhs)
			{
				result = RuleType == rhs.RuleType && MinRequired == rhs.MinRequired && MaxAllowed == rhs.MaxAllowed;
			}
			return result;
		}

		public override int GetHashCode()
		{
			return RuleType.GetHashCode() ^ MinRequired.GetHashCode() ^ MaxAllowed.GetHashCode();
		}

		public static bool operator ==(CusAuthorisationRuleRequirement lhs, CusAuthorisationRuleRequirement rhs)
		{
			return lhs.RuleType == rhs.RuleType && lhs.MinRequired == rhs.MinRequired && lhs.MaxAllowed == rhs.MaxAllowed;
		}

		public static bool operator !=(CusAuthorisationRuleRequirement lhs, CusAuthorisationRuleRequirement rhs)
		{
			return !(lhs == rhs);
		}

		public CusAuthorisationRuleRequirement AddAdditionalValidatorOnValue(Func<string, AdditionalValidatorResult> additionalValidatorOnValue)
		{
			if (additionalValidatorOnValue != null)
			{
				(AdditionalValidatorOnValueCollection as List<Func<string, AdditionalValidatorResult>>).Add(additionalValidatorOnValue);
			}
			return this;
		}

		static Func<string, AdditionalValidatorResult> ConvertAdditionalValidatorOnValueFunction(Func<string, string> additionalValidatorOnValue, INotificationType notificationType)
		{
			Func<string, AdditionalValidatorResult> additionalValidatorOnValueWithValidatorResult = null;
			if (additionalValidatorOnValue != null)
			{
				additionalValidatorOnValueWithValidatorResult = (string valueFrom) =>
				   {
					   var internalResult = additionalValidatorOnValue(valueFrom);
					   return new AdditionalValidatorResult(internalResult, notificationType ?? CargoWiseNotificationType.Error);
				   };
			}
			return additionalValidatorOnValueWithValidatorResult;
		}
	}

	public struct AdditionalValidatorResult
	{
		public AdditionalValidatorResult(string validationMessage, INotificationType notificationType)
		{
			ValidationMessage = validationMessage;
			NotificationType = Argument.NotNull(notificationType, nameof(notificationType));
		}

		public string ValidationMessage { get; }
		public INotificationType NotificationType { get; }

		public static implicit operator AdditionalValidatorResult((string ValidationMessage, INotificationType NotificationType) additionalValidationResult)
		{
			return new AdditionalValidatorResult(additionalValidationResult.ValidationMessage, additionalValidationResult.NotificationType);
		}
	}
}
