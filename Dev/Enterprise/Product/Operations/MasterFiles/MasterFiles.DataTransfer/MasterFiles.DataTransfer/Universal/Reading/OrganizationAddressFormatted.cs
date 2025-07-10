using System.Collections.Generic;
using CargoWise.Tools.Telephony;
using CargoWise.Types;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.MasterFiles.DataTransfer.Universal
{
	public class OrganizationAddressFormatted : IDataObject
	{
		readonly OrganizationAddress source;

		public OrganizationAddressFormatted(OrganizationAddress addressData)
		{
			source = addressData;
		}

		public ZString? Phone
		{
			get
			{
				if (source.Phone.HasValue && (!phone.HasValue || previousSourcePhone != source.Phone.Value))
				{
					phone = source.Phone;
					if (!string.IsNullOrEmpty(phone.GetValueOrDefault()))
					{
						var countryCode = Country.GetCodeAsUpperCase();
						if (countryCode.IsEmpty && Port != null)
						{
							countryCode = Port.Code.GetValueOrDefault();
						}
						ZString formattedPhone = PhoneNumberFormatter.Instance.FormatE164(phone.ToString(), countryCode);
						if (!formattedPhone.IsEmpty)
						{
							phone = formattedPhone;
						}
					}

					previousSourcePhone = source.Phone.Value;
				}

				return phone;
			}
		}
		ZString? phone;
		ZString previousSourcePhone;

		public ZString? AddressType => source.AddressType;
		public ZString? AddressShortCode => source.AddressShortCode;
		public ZBool? AddressOverride => source.AddressOverride;
		public ZString? Address1 => source.Address1;
		public ZString? Address2 => source.Address2;
		public ZString? AdditionalAddressInformation => source.AdditionalAddressInformation;
		public ZString? City => source.City;
		public ZString? CompanyName => source.CompanyName;
		public ZString? OrganizationCode => source.OrganizationCode;
		public ZString? Email => source.Email;
		public ZString? Fax => source.Fax;
		public ZString? GovRegNum => source.GovRegNum;
		public ZString? Mobile => source.Mobile;
		public ZString? Postcode => source.Postcode;
		public ZString? State => source.State;
		public Country Country => source.Country;
		public UNLOCO Port => source.Port;
		public ZString? Contact => source.Contact;
		public RegistrationNumberType GovRegNumType => source.GovRegNumType;
		public ZBool? SuppressAddressValidationError => source.SuppressAddressValidationError;
		public ZBool? IsResidential => source.IsResidential;
		public List<RegistrationNumber> RegistrationNumberCollection => source.RegistrationNumberCollection;
		public ZString? UniversalNettingCode => source.UniversalNettingCode;
		public ZString? UniversalOfficeCode => source.UniversalOfficeCode;

		public bool HasTypeAndCodeOnly()
		{
			return source.HasTypeAndCodeOnly();
		}

		public bool HasTypeOnly()
		{
			return source.HasTypeOnly();
		}
	}
}
