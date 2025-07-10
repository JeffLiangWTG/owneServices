using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.DocumentVisualizer.DocDataObjects;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Business.Macros
{
	public sealed class Address : IAddress
	{
		public Address(OrgAddress address)
		{
			this.address = address;
		}

		readonly OrgAddress address;

		public ZGuid AddressIdentifier
		{
			get
			{
				if (!addressIdentifier.HasValue)
				{
					addressIdentifier = address?.PK ?? ZGuid.Empty;
				}
				return addressIdentifier.Value;
			}
			set
			{
				addressIdentifier = value;
			}
		}

		ZGuid? addressIdentifier;

		public ZGuid HeaderIdentifier
		{
			get
			{
				if (!headerIdentifier.HasValue)
				{
					headerIdentifier = address?.OA_OH ?? ZGuid.Empty;
				}
				return headerIdentifier.Value;
			}
			set
			{
				headerIdentifier = value;
			}
		}

		ZGuid? headerIdentifier;

		public ZString CompanyName
		{
			get
			{
				if (!companyName.HasValue)
				{
					companyName = address?.CompanyName ?? ZString.Empty;
				}
				return companyName.Value;
			}
			set
			{
				companyName = value;
				addressFormatted = null;
			}
		}

		ZString? companyName;

		public ZString AdditionalAddressInformation
		{
			get
			{
				if (!additionalAddressInformation.HasValue)
				{
					additionalAddressInformation = address?.PrimaryOrgAddressAdditionalInfoDetail;
				}
				return additionalAddressInformation.Value;
			}
			set => additionalAddressInformation = value;
		}

		ZString? additionalAddressInformation;

		public ZString AddressLine1
		{
			get
			{
				if (!addressLine1.HasValue)
				{
					addressLine1 = address?.OA_Address1 ?? ZString.Empty;
				}
				return addressLine1.Value;
			}
			set
			{
				addressLine1 = value;
				addressFormatted = null;
			}
		}

		ZString? addressLine1;

		public ZString AddressLine2
		{
			get
			{
				if (!addressLine2.HasValue)
				{
					addressLine2 = address?.OA_Address2 ?? ZString.Empty;
				}
				return addressLine2.Value;
			}
			set
			{
				addressLine2 = value;
				addressFormatted = null;
			}
		}

		ZString? addressLine2;

		public ZString City
		{
			get
			{
				if (!city.HasValue)
				{
					city = address?.OA_City ?? ZString.Empty;
				}
				return city.Value;
			}
			set
			{
				city = value;
				addressFormatted = null;
			}
		}

		ZString? city;

		public ZString State
		{
			get
			{
				if (!state.HasValue)
				{
					state = address?.OA_State ?? ZString.Empty;
				}
				return state.Value;
			}
			set
			{
				state = value;
				addressFormatted = null;
			}
		}

		ZString? state;

		public ZString Postcode
		{
			get
			{
				if (!postcode.HasValue)
				{
					postcode = address?.OA_PostCode ?? ZString.Empty;
				}
				return postcode.Value;
			}
			set
			{
				postcode = value;
				addressFormatted = null;
			}
		}

		ZString? postcode;

		public ZString Phone
		{
			get
			{
				if (!phone.HasValue)
				{
					phone = address?.PhoneNumber.ToString() ?? ZString.Empty;
				}
				return phone.Value;
			}
			set => phone = value;
		}

		ZString? phone;

		public ZString Fax
		{
			get
			{
				if (!fax.HasValue)
				{
					fax = address?.FaxNumber.ToString() ?? ZString.Empty;
				}
				return fax.Value;
			}
			set => fax = value;
		}

		ZString? fax;

		public ZString Email
		{
			get
			{
				if (!email.HasValue)
				{
					email = address?.OA_Email ?? ZString.Empty;
				}
				return email.Value;
			}
			set => email = value;
		}

		ZString? email;

		public ZString Contact { get; set; }

		public ZString TaxNumber { get; set; }

		public ICodeDescription TaxNumberType { get; set; }

		public ZString AddressFormatted
		{
			get
			{
				if (!addressFormatted.HasValue)
				{
					addressFormatted = GetFormattedAddress();
				}

				return addressFormatted.Value;
			}
			set => addressFormatted = value;
		}

		ZString? addressFormatted;

		ZString GetFormattedAddress()
		{
			if (address != null)
			{
				var formatter = new AddressFormatter(address.Factory, CompanyName, AddressLine1, AddressLine2, City, State, Postcode, (NoResString)Country?.Name, true);
				return formatter.PostalAddress().Replace("\n", System.Environment.NewLine);
			}

			return ZString.Empty;
		}

		public ICountry Country => country ?? (country = new Country(address?.RelatedCountry));
		Country country;

		public IUnloco Unloco => unloco ?? (unloco = new Unloco(address?.RelatedPortCode));
		Unloco unloco;

		public IReadOnlyCollection<IRegistrationNumber> RegistrationNumbers => registrationNumbers ?? (registrationNumbers = CreateRegistrationNumbers(address?.Header));

		IReadOnlyCollection<IRegistrationNumber> registrationNumbers;

		IReadOnlyCollection<IRegistrationNumber> CreateRegistrationNumbers(OrgHeader header)
		{
			var result = header?
				.CustomsCodes
				.OfType<OrgCusCode>()
				.Select(code => new RegistrationNumber(code))
				.ToArray();

			return result ?? System.Array.Empty<IRegistrationNumber>();
		}
	}
}
