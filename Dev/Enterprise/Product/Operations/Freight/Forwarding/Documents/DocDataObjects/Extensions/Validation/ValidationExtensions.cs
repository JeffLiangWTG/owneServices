using System;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using CargoWise.Application;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.DocumentVisualizer.DocDataObjects;
using Enterprise.ZArchitecture.Core;
using Res = Enterprise.Freight.Forwarding.Documents.DataObjects.Res;

namespace Enterprise.Freight.Forwarding.Documents.DocDataObjects
{
	public static class ValidationExtensions
	{
		#region AddPartyNameAndAddressValidation

		public static Address AddPartyNameAndAddressValidation(this Address address, string partyName, string messageValidation = "")
		{
			return AddPartyNameAndAddressValidation(address, partyName, null, messageValidation: messageValidation);
		}

		public static Address AddPartyNameAndAddressValidation(this Address address, string partyName, Func<bool> additionalCondition, bool useMessageError = true, string messageValidation = "")
		{
			if (address == null)
			{
				return null;
			}

			messageValidation = string.IsNullOrEmpty(messageValidation) ? Res.GetString("A67134C5-60F2-42DF-8636-E97E6658A755", "party name and address information is required.") : messageValidation;
			if (useMessageError)
			{
				address.CompanyNameInfo.AddMessageError(() => address.IsPartyNameAndAddressEmpty() && (additionalCondition == null || additionalCondition()), string.Format(CultureInfo.InvariantCulture, "{0} {1}", partyName, messageValidation));
			}
			else
			{
				address.CompanyNameInfo.AddWarning(() => address.IsPartyNameAndAddressEmpty() && (additionalCondition == null || additionalCondition()), string.Format(CultureInfo.InvariantCulture, "{0} {1}", partyName, messageValidation));
			}

			address.AddValidationDependencies(address.CompanyNameInfo, address.AddressLine1Info, address.AddressLine2Info);

			if (address.Country is Country country)
			{
				address.AddValidationDependencies(address.CompanyNameInfo, country.NameInfo);
			}

			address.AddCompanyNameLengthValidation();

			return address;
		}

		public static bool IsPartyNameAndAddressEmpty(this Address address)
		{
			if (address?.Country == null)
			{
				return true;
			}

			return address.CompanyName.IsEmpty
				|| address.AddressLine1.IsEmpty
				|| address.Country.Name.IsEmpty;
		}

		#endregion

		#region AddContactDetailsValidation

		public static Address AddContactDetailsValidation(this Address address, Func<bool> precondition = null)
		{
			if (address == null)
			{
				return null;
			}

			var errorMessage = Res.GetString("30F667A8-153B-456E-A2F8-17CFFAB874AD", "Please enter both contact name and at least one communication: phone, email or fax.");

			address.ContactInfo.AddMessageError(() => address.AreContactDetailsInvalid(precondition), errorMessage);

			address.OnValueChanged(nameof(address.Contact)).Validate(nameof(address.Phone), nameof(address.Email), nameof(address.Fax));
			address.OnValueChanged(nameof(address.Phone)).Validate(nameof(address.Contact), nameof(address.Email), nameof(address.Fax));
			address.OnValueChanged(nameof(address.Email)).Validate(nameof(address.Contact), nameof(address.Phone), nameof(address.Fax));
			address.OnValueChanged(nameof(address.Fax)).Validate(nameof(address.Contact), nameof(address.Phone), nameof(address.Email));

			return address;
		}

		public static bool AreContactDetailsInvalid(this Address address, Func<bool> precondition = null)
		{
			var areContactDetailsEmpty = address.Phone.IsEmpty
										&& address.Email.IsEmpty
										&& address.Fax.IsEmpty;

			return (precondition == null || precondition()) &&
					(!address.Contact.IsEmpty && areContactDetailsEmpty
					|| address.Contact.IsEmpty && !areContactDetailsEmpty
					|| Regex.IsMatch(address.Contact, @"^(.)\1*$"));
		}

		#endregion

		#region AddEmailValidation

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "programmatic constant")]
		public static Address AddContactDetailsEmailValidation(this Address address, Func<bool> precondition = null)
		{
			if (address == null)
			{
				return null;
			}

			bool IsValidEmail()
			{
				// TODO: WI00749813 - Investigate usages of Email Validation RegEx
				const string pattern = @"[a-z0-9!#$%&'*+/=?^_`{|}~-]+(?:\.[a-z0-9!#$%&'*+/=?^_`{|}~-]+)*@(?:[a-z0-9](?:[a-z0-9-]*[a-z0-9])?\.)+[a-z0-9](?:[a-z0-9-]*[a-z0-9])?";
				return address.Email.Length > 6
					&& new Regex(pattern, RegexOptions.IgnoreCase).Match(address.Email).Success;
			}

			address.EmailInfo.AddMessageError(precondition.Then(() => !address.Email.IsEmpty && !IsValidEmail()), Res.GetString("22DA8362-4840-4379-AA01-22E98F59F84D", "Please enter a valid email. Email must contain at least 6 characters, at least one dot '.' after '@' with at least one character in between and at least 2 characters after the dot. Email can only contain alphanumeric characters and '_', '-', '@', '.'."));

			return address;
		}

		#endregion

		#region AddCompanyNameValidation

		public static Address AddCompanyNameLengthValidation(this Address address, Func<bool> precondition = null)
		{
			if (address == null)
			{
				return null;
			}

			address.CompanyNameInfo.AddWarning(precondition.Then(() => address.CompanyName.Length > 70), Res.GetString("78F2C557-71F1-49E0-AB64-AD37787E8C0C", "Party name should not exceed 70 characters. Please note that any excess characters might be truncated by the message recipient."));

			return address;
		}

		#endregion

		#region AddPostcodeValidationForUSImports

		public static Address AddPostcodeValidationForUSImports(this Address address, string partyName)
		{
			if (address == null)
			{
				return null;
			}

			bool IsPostcodeInvalid()
			{
				return address.Postcode.IsEmpty
					&& string.Compare(address.Country?.Code, Core.Constants.CountryCodes.UnitedStates, StringComparison.CurrentCultureIgnoreCase) == 0;
			}

			address.PostcodeInfo.AddMessageError(IsPostcodeInvalid, string.Format(CultureInfo.InvariantCulture, (NoResString)"{0} postcode is required for US imports.", partyName)); // non-translatable validation message

			if (address.Country is Country country)
			{
				address.AddValidationDependencies(address.PostcodeInfo, country.CodeInfo);
			}

			return address;
		}

		#endregion

		#region AddCurrentUserValidation

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "non-translatable validation message")]
		public static SignatureDetails AddCurrentUserValidation(this SignatureDetails signature, Address address)
		{
			if (address == null || signature == null)
			{
				return null;
			}

			bool AreStaffDetailsInvalid()
			{
				return address.Email.IsEmpty
					&& address.Phone.IsEmpty;
			}

			const string errorMessage = "Your staff login profile does not specify contact details. At least one communication number (email or phone) is required.";

			signature.NameInfo.AddMessageError(AreStaffDetailsInvalid, errorMessage);

			signature.AddValidationDependencies(signature.NameInfo, address.EmailInfo, address.PhoneInfo);

			return signature;
		}

		#endregion

		#region AddRequiredValidation

		public static Unloco AddRequiredValidation(this Unloco unloco, string prefix = "")
		{
			if (unloco == null)
			{
				return null;
			}

			var spacer = string.IsNullOrWhiteSpace(prefix)
				? ""
				: " ";

			unloco.CodeInfo.AddMessageError(() => unloco.Code.IsEmpty, FormattableString.Invariant($"{prefix}{spacer}Code is required.")); // non-translatable validation message
			unloco.NameInfo.AddMessageError(() => unloco.Name.IsEmpty, FormattableString.Invariant($"{prefix}{spacer}Name is required.")); // non-translatable validation message

			return unloco;
		}

		#endregion

		#region AddAsciiCharactersValidation

		public static Address AddAsciiCharactersValidation(this Address address, Func<bool> precondition = null)
		{
			if (address == null)
			{
				return null;
			}

			address.CompanyNameInfo.AddAsciiCharactersValidation(precondition);
			address.ContactInfo.AddAsciiCharactersValidation(precondition);
			address.AddressLine1Info.AddAsciiCharactersValidation(precondition);
			address.AddressLine2Info.AddAsciiCharactersValidation(precondition);
			address.CityInfo.AddAsciiCharactersValidation(precondition);
			address.StateInfo.AddAsciiCharactersValidation(precondition);
			address.PostcodeInfo.AddAsciiCharactersValidation(precondition);
			address.PhoneInfo.AddAsciiCharactersValidation(precondition);
			address.FaxInfo.AddAsciiCharactersValidation(precondition);
			address.EmailInfo.AddAsciiCharactersValidation(precondition);

			return address;
		}

		public static Unloco AddAsciiCharactersValidation(this Unloco unloco)
		{
			if (unloco == null)
			{
				return null;
			}

			unloco.CodeInfo.AddAsciiCharactersValidation();
			unloco.NameInfo.AddAsciiCharactersValidation();

			return unloco;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "non-translatable validation message")]
		public static void AddAsciiCharactersValidation(this ZPropertyInfo info, Func<bool> precondition = null)
		{
			const string errorMessage = "Most messaging providers do not support non ASCII characters.";

			info.AddMessageError(precondition.Then(() => HasNonAsciiCharacters(new ZString(info.Value)) || HasSpecialCharacter(new ZString(info.Value))), errorMessage);
		}

		public static bool HasNonAsciiCharacters(ZString value)
		{
			var encoding = Encoding.GetEncoding("ISO-8859-1");
			var bytes = encoding.GetBytes(value);
			var result = encoding.GetString(bytes);
			return !string.Equals(value, result);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "non-translatable validation message")]
		public static void AddSpecialCharactersValidation(this ZPropertyInfo info)
		{
			const string errorMessage = "Most messaging providers do not support special characters.";

			info.AddMessageError(() => HasSpecialCharacter(new ZString(info.Value)), errorMessage);
		}

		static bool HasSpecialCharacter(ZString value)
		{
			return value.ContainsAnyChar((NoResString)"²") || value.ContainsAnyChar((NoResString)"³"); // non-translatable special character
		}

		#endregion

		#region AddInvalidCodeValidation

		public static void AddInvalidCodeValidation(this ZPropertyInfo info, string messageError = null)
		{
			info.AddMessageError(() => CheckIfInvalidCode(info), messageError ?? DefaultInvalidCodeMessageError);
		}

		static bool CheckIfInvalidCode(ZPropertyInfo info)
		{
			var result = false;
			if (!info.Value.IsEmpty)
			{
				var lookups = MetaData.GetListDataSource(info.BizObj, info.PropertyDescriptor);

				if (lookups is ICodeDescriptionPairList codeDescriptionPairList)
				{
					return !codeDescriptionPairList.ContainsCode(info.Value);
				}
				else if (lookups is IBusinessObjectCollection boCollection)
				{
					var elementType = boCollection.TypeOfElements;

					var propertyName = CodePropertyAttribute.CodePropertyNameFromType(elementType);
					var tableName = BusinessObjectFactory.GetTableNameFromType(elementType);
					var schemaResolver = ObjectFactory.Get<IApplicationSchemaResolver>();
					var schemaColumn = schemaResolver.GetSchemaColumn(propertyName, tableName);

					var query = new ZQuery();

					if (lookups is ICodeMapper codeMapper && info.PropertyType == typeof(ZString))
					{
						query.AddToFilter(schemaColumn, codeMapper.GetLocalCode((ZString)info.Value));
					}
					else
					{
						query.AddToFilter(schemaColumn, info.Value);
					}

					query.AddToFilter(boCollection.CompleteFilter, JoinCondition.And);

					return boCollection.Factory.LoadTop1(elementType, query) == null;
				}
			}

			return result;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "non-translatable message")]
		const string DefaultInvalidCodeMessageError = "You have not entered a valid code.";

		#endregion

		#region AddSupportedCharactersValidationForUSCustoms

		public static Address AddSupportedCharactersValidationForUSCustoms(this Address address)
		{
			if (address == null)
			{
				return null;
			}

			address.CompanyNameInfo.AddSupportedCharactersValidationForUSCustoms();
			address.ContactInfo.AddSupportedCharactersValidationForUSCustoms();
			address.AddressLine1Info.AddSupportedCharactersValidationForUSCustoms();
			address.AddressLine2Info.AddSupportedCharactersValidationForUSCustoms();
			address.CityInfo.AddSupportedCharactersValidationForUSCustoms();
			address.StateInfo.AddSupportedCharactersValidationForUSCustoms();
			address.PostcodeInfo.AddSupportedCharactersValidationForUSCustoms();
			address.PhoneInfo.AddSupportedCharactersValidationForUSCustoms();
			address.FaxInfo.AddSupportedCharactersValidationForUSCustoms();
			address.EmailInfo.AddSupportedCharactersValidationForUSCustoms();

			return address;
		}

		public static Unloco AddSupportedCharactersValidationForUSCustoms(this Unloco unloco)
		{
			if (unloco == null)
			{
				return null;
			}

			unloco.CodeInfo.AddSupportedCharactersValidationForUSCustoms();
			unloco.NameInfo.AddSupportedCharactersValidationForUSCustoms();

			return unloco;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "non-translatable validation message")]
		public static void AddSupportedCharactersValidationForUSCustoms(this ZPropertyInfo info)
		{
			const string errorMessage = "This text contains characters not supported by the United States Customs (CBP).\r\n"
				+ "Only characters shown directly on a keyboard with US layout are acceptable for this message, not typed or special characters.";
			info.AddMessageError(() => HasUnsupportedBasicAsciiCharacter(new ZString(info.Value)), errorMessage);
		}

		public static bool HasUnsupportedBasicAsciiCharacter(ZString value)
		{
			foreach (char c in value)
			{
				if (!IsSupportedCharacterByBasicAscii(c))
				{
					return true;
				}
			}

			return false;
		}

		static bool IsSupportedCharacterByBasicAscii(char c)
		{
			int code = c;
			return (code >= 9 && code <= 13) || (code >= 32 && code <= 126);
		}

		#endregion

		#region AddEmptyCodeDescriptionValidation

		public static CodeDescription AddEmptyCodeDescriptionValidation(this CodeDescription codeDescription, string name, Func<bool> precondition = null)
		{
			if (codeDescription == null)
			{
				return null;
			}

			codeDescription.CodeInfo.AddMessageError(() => (precondition == null || precondition()) && codeDescription.Code.IsEmpty, FormattableString.Invariant($"A {name} is required.")); // non-translatable validation message

			return codeDescription;
		}

		#endregion

		#region AddValidationDependencies

		public static void AddValidationDependencies(this DocDataObject docDataObject, ZPropertyInfo infoToValidate, params ZPropertyInfo[] infosOnWhichValidationDepends)
		{
			foreach (var info in infosOnWhichValidationDepends)
			{
				if (info.BizObj is DocDataObject valueChangedDataObject)
				{
					valueChangedDataObject.OnValueChanged(info.Name).Do(() => docDataObject.Validate(infoToValidate.Name));
				}
			}
		}

		#endregion

		#region AddValidationDependenciesToValidateAll

		public static void AddValidationDependenciesToValidateAll(this DocDataObject docDataObjectToValidate, params ZPropertyInfo[] infosOnWhichValidationDepends)
		{
			foreach (var info in infosOnWhichValidationDepends)
			{
				if (info.BizObj is DocDataObject valueChangedDataObject)
				{
					valueChangedDataObject.OnValueChanged(info.Name).Do(() => docDataObjectToValidate.ValidateAll());
				}
			}
		}

		#endregion

		#region Then

		public static Func<bool> Then(this Func<bool> precondition, Func<bool> nextcondition)
		{
			return () => (precondition == null || precondition()) && nextcondition();
		}

		#endregion

	}
}
