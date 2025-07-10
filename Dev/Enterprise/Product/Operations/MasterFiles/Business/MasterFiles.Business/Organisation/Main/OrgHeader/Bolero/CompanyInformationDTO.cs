using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Newtonsoft.Json;

namespace Enterprise.MasterFiles.Business
{
	public class CompanyInformationDTO
	{
		public CompanyInformationDTO(BoleroInvitationDetails invitationDetails)
		{
			const string defaultPostCode = "OOO";
			var org = invitationDetails.Org;
			var selectedContact = org.Factory.Load<OrgContact>(invitationDetails.SelectedContactPK);
			var entityTypes = (org.OH_IsConsignee || org.OH_IsConsignor) ? new List<string>() : null;

			if (org.OH_IsConsignee)
			{
				entityTypes?.Add(OrgConstants.OrganisationTypes.Consignee);
			}

			if (org.OH_IsConsignor)
			{
				entityTypes?.Add(OrgConstants.OrganisationTypes.Consignor);
			}

			var names = selectedContact.Name.ToString().Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
			var lastName = names.Length > 1 ? names.Last() : string.Empty;
			var firstName = names.Length > 1 ? string.Join(" ", names.Take(names.Length - 1).ToArray()) : selectedContact.Name.ToString();

			FirstName = firstName;
			LastName = lastName;
			Email = selectedContact.Email;
			LegalCompanyName = org.OH_FullName;
			AddressLine1 = org.MainAddress.Address1;
			AddressLine2 = org.MainAddress.Address2;
			City = org.MainAddress.City;
			PostCode = string.IsNullOrEmpty(org.MainAddress.Postcode) ? defaultPostCode : org.MainAddress.Postcode;
			State = org.MainAddress.State;
			CountryCode = org.MainAddress.OA_RN_NKCountryCode;
			CompanyOrgCode = org.PK;
			EntityTypes = entityTypes;
			CompanyIdentifications = GetCompanyIdentifications(org);
		}

		[JsonProperty("firstName")]
		[System.ComponentModel.DataAnnotations.Required(ErrorMessage = "Missing Field")]
		public string FirstName { get; set; }

		[JsonProperty("lastName")]
		public string LastName { get; set; }

		[JsonProperty("email")]
		[System.ComponentModel.DataAnnotations.Required(ErrorMessage = "Missing Field")]
		public string Email { get; set; }

		[JsonProperty("legalCompanyName")]
		[System.ComponentModel.DataAnnotations.Required(ErrorMessage = "Missing Field")]
		public string LegalCompanyName { get; set; }

		[JsonProperty("addressLine1")]
		[System.ComponentModel.DataAnnotations.Required(ErrorMessage = "Missing Field")]
		public string AddressLine1 { get; set; }

		[JsonProperty("addressLine2")]
		public string AddressLine2 { get; set; }

		[JsonProperty("city")]
		[System.ComponentModel.DataAnnotations.Required(ErrorMessage = "Missing Field")]
		public string City { get; set; }

		[JsonProperty("postCode")]
		public string PostCode { get; set; }

		[JsonProperty("state")]
		public string State { get; set; }

		[JsonProperty("countryCode")]
		[System.ComponentModel.DataAnnotations.Required(ErrorMessage = "Missing Field")]
		public string CountryCode { get; set; }

		[JsonProperty("companyIdentifications")]
		[System.ComponentModel.DataAnnotations.Required(ErrorMessage = "Missing Field")]
		public Dictionary<string, string> CompanyIdentifications { get; set; }

		[JsonProperty("entityTypes")]
		[System.ComponentModel.DataAnnotations.Required(ErrorMessage = "Please ensure the organization type is either consignee or consignor.")]
		public List<string> EntityTypes { get; set; }

		[JsonProperty("companyOrgCode")]
		[System.ComponentModel.DataAnnotations.Required(ErrorMessage = "Missing Field")]
		public ZGuid CompanyOrgCode { get; set; }

		Dictionary<string, string> GetCompanyIdentifications(OrgHeader org)
		{
			var companyIdentifications = new Dictionary<string, string>();
			var cuscodeTypes = OrgCusCodeInfo.GetMainOrganizationNumberTypes(org.MainAddress.OA_RN_NKCountryCode);

			foreach (var codeType in cuscodeTypes)
			{
				var regValue = org.CustomsCodes.GetCustomsRegNo(codeType, org.CountryCode);
				if (!string.IsNullOrEmpty(regValue))
				{
					companyIdentifications.Add(codeType, regValue);
				}
			}

			return companyIdentifications.Count > 0 ? companyIdentifications : null;
		}
	}
}
