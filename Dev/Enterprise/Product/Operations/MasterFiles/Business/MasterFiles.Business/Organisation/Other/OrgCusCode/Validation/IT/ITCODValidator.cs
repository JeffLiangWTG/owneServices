using System;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.MasterFiles.Business
{
	sealed class ITCODValidator
	{
		public void Validate(ZString cod, ZPropertyInfo targetPropertyInfo, string messageErrorSuffix = null, INotificationType notificationType = null)
		{
			Argument.NotNull(targetPropertyInfo, nameof(targetPropertyInfo));
			var cusCodeValidator = ITCusCodeValidationSelector.GetCODCodeValidator(cod);
			var validationResult = cusCodeValidator.Validate(cod);

			if (validationResult == ITCusCodeValidationResult.InvalidLength)
			{
				AddMessage(targetPropertyInfo, AppendSuffixIfNotEmpty(InvalidLength, messageErrorSuffix), notificationType);
			}
			else if (validationResult == ITCusCodeValidationResult.InvalidPattern)
			{
				AddMessage(targetPropertyInfo, AppendSuffixIfNotEmpty(InvalidPattern, messageErrorSuffix), notificationType);
			}
			else if (validationResult == ITCusCodeValidationResult.InvalidCheckDigit)
			{
				AddMessage(targetPropertyInfo, AppendSuffixIfNotEmpty(InvalidCheckDigit, messageErrorSuffix), notificationType);
			}

			string AppendSuffixIfNotEmpty(string messageError, string suffix)
			{
				if (!string.IsNullOrEmpty(suffix))
				{
					return FormattableString.Invariant($"{messageError} {suffix}");
				}
				return messageError;
			}
		}

		public void Validate(ZPropertyInfo codeInfo)
		{
			var code = ((ZString)codeInfo.Value);
			Validate(code, codeInfo);
		}

		void AddMessage(ZPropertyInfo codeInfo, string message, INotificationType notificationType)
		{
			if (notificationType == null)
			{
				codeInfo.AddErrorIfEnforced(message, OrganisationRegistry.RegistrationNumberFormatFields.ITCOD);
			}
			else
			{
				codeInfo.AddNotification(notificationType, message);
			}
		}

		string InvalidLength => Res.GetString("f5540a4a-c507-4200-85df-c7133aa46a61", "The COD registration code needs to be 16 or 11 in length.");

		string InvalidPattern => Res.GetString("c7114ed8-dc93-48c3-96c6-cc5c0a5e821e", @"The COD registration code pattern is invalid.
Valid patterns are:
	{0}", validPatterns);

		string InvalidCheckDigit => Res.GetString("4016d65f-f17e-47c0-a82b-230b9c2fbc7b", "The check digit in the COD registration code is incorrect.");

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Specific Technical Formatting")]
		const string validPatterns = @"AAAAAAnnAnnAnnnA
	nnnnnnnnnnn";
	}
}
