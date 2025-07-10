using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.MasterFiles.Business
{
	/// <summary>
	/// Contains the helpers to implement phone number properties.
	/// </summary>
	public class PhoneNumberPropertyHelper
	{
		#region Constructors

		public PhoneNumberPropertyHelper(Func<ZString> defaultCountryCodeRetriever)
		{
			this.defaultCountryCodeRetriever = defaultCountryCodeRetriever;
		}

		#endregion

		#region Get/Set Phone Number

		/// <summary>
		/// Gets the phone number to present as formatted.
		/// </summary>
		/// <param name="rawPhoneNumberPropertyInfo">The phone number property that contains the actual data.</param>
		/// <returns>The phone number to present as formatted.</returns>
		public ZString GetPhoneNumber(ZPropertyInfo rawPhoneNumberPropertyInfo)
		{
			ZString result;
			var phoneNumberInput = GetPhoneNumberInput(rawPhoneNumberPropertyInfo.Name);
			var rawPhoneNumber = (ZString)rawPhoneNumberPropertyInfo.Value;

			if (!phoneNumberInput.IsEmpty)
			{
				result = phoneNumberInput;
			}
			else
			{
				var formattedPhoneNumber = FormatPhoneNumberInInternational(rawPhoneNumber);
				result = formattedPhoneNumber.IsEmpty ? rawPhoneNumber : formattedPhoneNumber;
			}

			return result;
		}

		/// <summary>
		/// Sets the phone number.
		/// </summary>
		/// <param name="rawPhoneNumberPropertyInfo">The phone number property that contains the actual data.</param>
		/// <param name="formattedPhoneNumberPropertyInfo">The phone number property that presents the formatted data.</param>
		/// <param name="value">The new phone number.</param>
		/// <param name="customValidation">The custom validation to perform.</param>
		/// <param name="isManuallyVerifiedPropertyInfo">The manually verified property.</param>
		public void SetPhoneNumber(ZPropertyInfo rawPhoneNumberPropertyInfo, ZPropertyInfo formattedPhoneNumberPropertyInfo, ZString value, Action customValidation, ZPropertyInfo isManuallyVerifiedPropertyInfo)
		{
			var rawPhoneNumberHasChanges = false; //Can't use ZPropertyInfo.HasChanges for non persistent property value
			var normalizedPhoneNumber = NormalizePhoneNumber(value);
			var rawPhoneNumberValue = (ZString)rawPhoneNumberPropertyInfo.Value;

			if (normalizedPhoneNumber.IsEmpty)
			{
				SetPhoneNumberInput(rawPhoneNumberPropertyInfo.Name, value);
				if (rawPhoneNumberPropertyInfo.MaxLength < 0 || value.Length <= rawPhoneNumberPropertyInfo.MaxLength)
				{
					if (rawPhoneNumberValue != value)
					{
						rawPhoneNumberHasChanges = true;
					}
					rawPhoneNumberPropertyInfo.Value = value;
				}
			}
			else
			{
				SetPhoneNumberInput(rawPhoneNumberPropertyInfo.Name, ZString.Empty);
				if (rawPhoneNumberValue != normalizedPhoneNumber)
				{
					rawPhoneNumberHasChanges = true;
				}
				rawPhoneNumberPropertyInfo.Value = normalizedPhoneNumber;
			}

			if (isManuallyVerifiedPropertyInfo != null && rawPhoneNumberHasChanges)
			{
				isManuallyVerifiedPropertyInfo.Value = ZBool.False;
			}

			if (customValidation != null)
			{
				customValidation();
			}

			if (rawPhoneNumberHasChanges)
			{
				formattedPhoneNumberPropertyInfo.RefreshBinding();
			}
		}

		#endregion

		#region Tooltip

		public ZString GetPhoneNumberInLocalIfLoggedInSameCountry(ZPropertyInfo rawPhoneNumberPropertyInfo)
		{
			var result = ZString.Empty;

			if (GlbBranch.CurrentBranch.Country != null)
			{
				var currentCountryCode = GlbBranch.CurrentBranch.Country.Code;
				var rawPhoneNumber = (ZString)rawPhoneNumberPropertyInfo.Value;

				if (currentCountryCode == GetCountryCode(rawPhoneNumber))
				{
					result = FormatPhoneNumberInLocal(rawPhoneNumber);
				}
			}

			return result;
		}

		#endregion

		#region Implementations

		ZString DefaultCountryCode
		{
			get { return defaultCountryCodeRetriever(); }
		}

		ZString GetPhoneNumberInput(ZString rawPhoneNumberPropertyName)
		{
			if (!phoneNumberInputs.ContainsKey(rawPhoneNumberPropertyName))
			{
				phoneNumberInputs.Add(rawPhoneNumberPropertyName, ZString.Empty);
			}
			return phoneNumberInputs[rawPhoneNumberPropertyName];
		}

		void SetPhoneNumberInput(ZString rawPhoneNumberPropertyName, ZString value)
		{
			phoneNumberInputs[rawPhoneNumberPropertyName] = value;
		}

		ZString FormatPhoneNumberInInternational(ZString phoneNumber)
		{
			return phoneNumberFormatterAndValidator.FormatInternational(phoneNumber, DefaultCountryCode);
		}

		ZString FormatPhoneNumberInLocal(ZString phoneNumber)
		{
			return phoneNumberFormatterAndValidator.FormatLocal(phoneNumber, DefaultCountryCode);
		}

		ZString GetCountryCode(ZString phoneNumber)
		{
			return phoneNumberFormatterAndValidator.GetCountryCode(phoneNumber, DefaultCountryCode);
		}

		public ZString NormalizePhoneNumber(ZString phoneNumber)
		{
			return phoneNumberFormatterAndValidator.Normalize(phoneNumber, DefaultCountryCode);
		}

		readonly Dictionary<ZString, ZString> phoneNumberInputs = new Dictionary<ZString, ZString>();
		readonly PhoneNumberFormatterAndValidator phoneNumberFormatterAndValidator = new PhoneNumberFormatterAndValidator();
		readonly Func<ZString> defaultCountryCodeRetriever;

		#endregion
	}
}