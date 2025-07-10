using System;
using Enterprise.DocumentVisualizer.DocDataObjects;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Freight.Forwarding.Documents.DocDataObjects.CarrierMessageValidation
{
	static class CarrierMessageDataValidationExtensions
	{
		#region StandardAddressValidation

		public static Address AddStandardAddressValidation(this Address address, Func<bool> precondition = null, bool addContactDetailsValidation = true)
		{
			if (addContactDetailsValidation)
			{
				address.AddContactDetailsValidation(precondition);
			}

			address.AddContactDetailsEmailValidation(precondition)
				.AddCarrierStateLengthValidation(precondition)
				.AddAsciiCharactersValidation(precondition)
				.AddCompanyNameLengthValidation(precondition)
				.AddContactNameLengthValidation(precondition)
				.AddEmptyCountryCodeValidation(precondition);

			return address;
		}

		#endregion

		#region AddCarrierStateLengthValidation

		public static Address AddCarrierStateLengthValidation(this Address address, Func<bool> precondition = null)
		{
			if (address == null)
			{
				return null;
			}

			address.StateInfo.AddWarning(precondition.Then(() => address.State.Length > 9), (NoResString)"Carrier message accepts only 9 characters. Extra characters will be truncated when sending message to carrier."); // Non-Translatable validation message

			return address;
		}

		#endregion

		#region AddContactNameLengthValidation

		public static Address AddContactNameLengthValidation(this Address address, Func<bool> precondition = null)
		{
			if (address == null)
			{
				return null;
			}

			address.ContactInfo.AddWarning(precondition.Then(() => address.Contact.Length > 35), (NoResString)"Contact Name must not exceed 35 characters."); // non-translatable validation message

			return address;
		}

		public static Address AddCityLengthValidation(this Address address)
		{
			if (address == null)
			{
				return null;
			}

			address.CityInfo.AddWarning(() => address.City.Length > 50, (NoResString)"City Name must not exceed 50 characters."); // non-translatable validation message

			return address;
		}

		#endregion

		#region AddEmptyCountryCodeValidation

		public static Address AddEmptyCountryCodeValidation(this Address address, Func<bool> precondition = null)
		{
			if (address == null)
			{
				return null;
			}

			address.Country.NameInfo.AddMessageError(precondition.Then(() => !address.Country.Name.IsEmpty && address.Country.Code.IsEmpty), (NoResString)"Country code is required, if Country is entered."); // Non-Translatable validation message

			return address;
		}

		#endregion
	}
}
