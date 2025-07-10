using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.US.Business
{
	public static class FDAOrganisationValidator
	{
		public static void Validate(OrgHeader organisation, ZPropertyInfo info)
		{
			ZString infoName = info.HumanReadableName;
			if (organisation != null)
			{
				OrgHeaderWrapper wrapped = OrgHeaderWrapper.New(organisation);
				var pgaContactDetails = (IPGAContactDetails)wrapped;
				if (organisation.MainAddress.OA_City.IsEmpty)
				{
					info.AddMessageError(string.Format(ValidationConstants.PriorNotice.OrganisationCity, infoName));
				}

				if (organisation.CountryCode.IsEmpty)
				{
					info.AddMessageError(string.Format(ValidationConstants.PriorNotice.OrganisationCountry, infoName));
				}

				if (organisation.MainAddress.OA_State.IsEmpty)
				{
					if (organisation.CountryCode == Core.Constants.CountryCodes.UnitedStates || organisation.CountryCode == Core.Constants.CountryCodes.Canada)
					{
						info.AddMessageError(string.Format(ValidationConstants.PriorNotice.OrganisationStateProvince, infoName));
					}
				}

				if (organisation.OH_FullName.IsEmpty)
				{
					info.AddMessageError(string.Format(ValidationConstants.PriorNotice.OrganisationFirmName, infoName));
				}

				if (organisation.MainAddress.OA_PostCode.IsEmpty)
				{
					info.AddMessageError(string.Format(ValidationConstants.PriorNotice.OrganisationPCode, infoName));
				}

				if (pgaContactDetails.Name.GetFirstName().IsEmpty)
				{
					info.AddMessageError(string.Format(ValidationConstants.PriorNotice.OrganisationFDAContactFirstName, infoName));
				}

				if (pgaContactDetails.Name.GetLastName().IsEmpty)
				{
					info.AddMessageError(string.Format(ValidationConstants.PriorNotice.OrganisationFDAContactLastName, infoName));
				}

				if (organisation.MainAddress.OA_Address1.IsEmpty)
				{
					info.AddMessageError(string.Format(ValidationConstants.PriorNotice.OrganisationAddress, infoName));
				}

				ZString fDAContactPhoneNumberToCheck = pgaContactDetails.PhoneNumber.IsEmpty ? organisation.MainAddress.OA_Phone : pgaContactDetails.PhoneNumber;

				if (fDAContactPhoneNumberToCheck.IsEmpty)
				{
					info.AddMessageError(string.Format(ValidationConstants.PriorNotice.OrganisationFDAContactPhone, infoName));
				}
				else
				{
					string phoneNumber = fDAContactPhoneNumberToCheck.KeepNumericCharacters();
					if (phoneNumber.Length < 10 || phoneNumber.Length > 14)
					{
						info.AddMessageError(ValidationConstants.PriorNotice.OrganisationFDAContactPhoneFormat);
					}
				}

				ZString fDAContactFAXToCheck = pgaContactDetails.Fax.IsEmpty ? organisation.MainAddress.OA_Fax : pgaContactDetails.Fax;

				if (!fDAContactFAXToCheck.IsEmpty)
				{
					string faxNumber = fDAContactFAXToCheck.KeepNumericCharacters();
					if (faxNumber.Length < 10 || faxNumber.Length > 45)
					{
						info.AddMessageError(ValidationConstants.PriorNotice.OrganisationFAXFormat);
					}
				}
			}
			else
			{
				info.AddMessageError(string.Format(ValidationConstants.PriorNotice.Organisation, infoName));
			}
		}
	}
}
