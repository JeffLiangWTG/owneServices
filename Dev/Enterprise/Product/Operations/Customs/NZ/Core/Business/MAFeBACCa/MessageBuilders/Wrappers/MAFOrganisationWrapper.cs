namespace Enterprise.Customs.NZ.Business.MAFeBACCa.MessageBuilders
{
	using CargoWise.Types;
	using Enterprise.Core;
	using Enterprise.Customs.NZ.Business.MAFeBACCa.Interfaces;
	using Enterprise.MasterFiles.Business;

	class MAFOrganisationWrapper : IMAFOrganisation
	{
		MAFOrganisationWrapper() { }

		#region GetMAFOrganisation

		public static IMAFOrganisation GetMAFOrganisation(OrgHeader organisation, string cusCodeType, ContactType contactType = null, bool orgCodeMandatory = false)
		{
			return organisation != null ? GetMAFOrganisation(organisation.MainAddress, cusCodeType, contactType, orgCodeMandatory) : null;
		}

		public static IMAFOrganisation GetMAFOrganisation(OrgAddress orgAddress, string cusCodeType, ContactType contactType = null, bool orgCodeMandatory = false)
		{
			if (orgAddress != null)
			{
				var organisation = orgAddress.Header;
				if (organisation != null)
				{
					var orgCode = GetOrgCode(organisation, orgAddress.PK, cusCodeType);
					if (!orgCodeMandatory || !orgCode.IsEmpty)
					{
						var result = new MAFOrganisationWrapper();
						result.OrganisationCode = orgCode;
						result.OrganisationName = organisation.OH_FullNameTruncated;
						result.AddressLine1 = orgAddress.OA_Address1;
						result.AddressLine2 = orgAddress.OA_Address2;
						result.City = orgAddress.OA_City;
						result.PostalCode = orgAddress.OA_PostCode;

						if (orgAddress.RelatedCountry != null)
						{
							result.Country = orgAddress.RelatedCountry.Code;
						}

						if (contactType != null)
						{
							result.ContactPhone = result.Phone = orgAddress.OA_Phone;
							result.ContactFax = result.Fax = orgAddress.OA_Fax;
							result.ContactEmail = result.Email = orgAddress.OA_Email;

							var contact = new DefaultContactFinder(organisation, false).DefaultContact(contactType);
							if (contact != null)
							{
								result.ContactName = contact.OC_ContactName;
								result.ContactPhone = contact.OC_Phone.IsEmpty ? orgAddress.OA_Phone : contact.OC_Phone;
								result.ContactFax = contact.OC_Fax.IsEmpty ? orgAddress.OA_Fax : contact.OC_Fax;
								result.ContactEmail = contact.OC_Email.IsEmpty ? orgAddress.OA_Email : contact.OC_Email;
							}
						}
						return result;
					}
				}
			}
			return null;
		}

		public static IMAFOrganisation GetMAFOrganisation(JobDocAddress docAddress, string cusCodeType = "", bool includeContactDetails = false)
		{
			if (docAddress != null)
			{
				var orgCode = GetOrgCode(docAddress, cusCodeType);
				if ((string.IsNullOrEmpty(cusCodeType) || !orgCode.IsEmpty)
					&& (docAddress.E2_AddressOverride || docAddress.HasRealAddress))
				{
					var result = new MAFOrganisationWrapper { OrganisationCode = orgCode };
					result.OrganisationName = docAddress.E2_CompanyNameTruncated;
					result.AddressLine1 = docAddress.E2_Address1;
					result.AddressLine2 = docAddress.E2_Address2;
					result.City = docAddress.E2_City;
					result.PostalCode = docAddress.E2_Postcode;
					result.Country = docAddress.E2_RN_NKCountryCode;

					if (includeContactDetails)
					{
						result.ContactName = docAddress.E2_Contact;
						result.ContactPhone = docAddress.E2_Phone;
						result.ContactFax = docAddress.E2_Fax;
						result.ContactEmail = docAddress.E2_Email;
					}
					return result;
				}
			}
			return null;
		}

		public static IMAFOrganisation GetMAFOrganisation(string orgCode, GlbCompany contactCompany, GlbBranch contactBranch, GlbStaff contact)
		{
			var result = new MAFOrganisationWrapper { OrganisationCode = orgCode };

			if (contactCompany != null)
			{
				result.OrganisationName = contactCompany.GC_Name;
				result.Country = contactCompany.GC_RN_NKCountryCode;
			}

			if (contactBranch != null)
			{
				result.AddressLine1 = contactBranch.GB_Address1;
				result.AddressLine2 = contactBranch.GB_Address2;
				result.City = contactBranch.GB_City;
				result.PostalCode = contactBranch.GB_PostCode;

				if (contact != null)
				{
					result.ContactName = contact.GS_FullName;
					result.ContactPhone = contact.GS_WorkPhone.IsEmpty ? contactBranch.GB_Phone : contact.GS_WorkPhone;
					result.ContactFax = contact.GS_FaxNum.IsEmpty ? contactBranch.GB_Fax : contact.GS_FaxNum;
					result.ContactEmail = contact.GS_EmailAddress.IsEmpty ? contactBranch.GB_Email : contact.GS_EmailAddress;
				}
			}
			return result;
		}

		public static IMAFOrganisation GetMAFOrganisation(IMAFOrganisation organisation, ZString contactName, ZString contactPhone, ZString contactFax, ZString contactEmail)
		{
			MAFOrganisationWrapper result = null;
			if (organisation != null)
			{
				result = organisation as MAFOrganisationWrapper;
				if (result == null)
				{
					result = new MAFOrganisationWrapper();
					result.OrganisationCode = organisation.OrganisationCode;
					result.OrganisationName = organisation.OrganisationName;

					result.AddressLine1 = organisation.AddressLine1;
					result.AddressLine2 = organisation.AddressLine2;
					result.City = organisation.City;
					result.PostalCode = organisation.PostalCode;
					result.Country = organisation.Country;
				}

				result.ContactName = contactName;
				result.ContactPhone = contactPhone;
				result.ContactFax = contactFax;
				result.ContactEmail = contactEmail;
			}
			return result;
		}

		#endregion

		#region GetOrgCode

		static ZString GetOrgCode(JobDocAddress docAddress, ZString cusCodeType)
		{
			var result = ZString.Empty;
			if (docAddress.HasRealOrganisation)
			{
				var organisation = docAddress.Organisation;
				result = GetOrgCode(organisation, docAddress.E2_OA_Address, cusCodeType);
			}
			return result;
		}

		static ZString GetOrgCode(OrgHeader organisation, ZGuid orgAddressPk, ZString cusCodeType)
		{
			var result = ZString.Empty;
			if (!cusCodeType.IsEmpty)
			{
				var cusCode = OrgCusCode.Load(organisation.Factory, cusCodeType, Constants.CountryCodes.NewZealand, organisation.PK, orgAddressPk);
				if (cusCode == null && organisation.Addresses.Count == 1)
				{
					cusCode = OrgCusCode.Load(organisation.Factory, cusCodeType, Constants.CountryCodes.NewZealand, organisation.PK, ZGuid.Empty);
				}
				if (cusCode != null)
				{
					result = cusCode.OK_CustomsRegNo;
				}
			}
			return result;
		}

		#endregion

		#region Implementation of IMAFOrganisation

		public ZString OrganisationCode { get; private set; }
		public ZString OrganisationName { get; private set; }
		public ZString AddressLine1 { get; private set; }
		public ZString AddressLine2 { get; private set; }
		public ZString City { get; private set; }
		public ZString PostalCode { get; private set; }
		public ZString Country { get; private set; }
		public ZString ContactName { get; private set; }
		public ZString ContactPhone { get; private set; }
		public ZString ContactFax { get; private set; }
		public ZString ContactEmail { get; private set; }
		public ZString Phone { get; private set; }
		public ZString Fax { get; private set; }
		public ZString Email { get; private set; }

		#endregion
	}
}
