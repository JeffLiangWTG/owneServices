using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CargoWise.EntityFramework;
using CargoWise.Glow.Model.Interfaces;
using CargoWise.Tools.DuplicateDetector;
using CargoWise.Tools.DuplicateDetector.Common;
using Enterprise.MasterData.Common;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterData.Business
{
	public class DeduplicationGlbPerson : IGlbPerson, IDeduplicationMaster, IDeduplicationGlowObject, IPerson, IRawNameProvider
	{
		[Newtonsoft.Json.JsonConstructor]
		DeduplicationGlbPerson() { }

		public DeduplicationGlbPerson(GlbPerson person, bool isDummy = false) : this(person,
				person.PER_BirthDateInternal.IsValid ? person.PER_BirthDateInternal.ToDateTime() : default(DateTime?),
				person.PER_HomeAddress1Internal,
				person.PER_HomeAddress2Internal,
				person.PER_CityInternal,
				person.PER_PostcodeInternal,
				person.PER_StateInternal,
				person.PER_EmailAddressInternal,
				person.PER_EmailAddress2Internal,
				person.PER_MobilePhoneInternal,
				person.PER_MobilePhone2Internal,
				person.PER_FaxNumberInternal,
				person.PER_HomePhoneInternal,
				person.PER_RN_NKCountryInternal,
				isDummy)
		{
		}

		public DeduplicationGlbPerson(GlbPerson person, DateTime? birthDate, string homeAddress1, string homeAddress2, string city, string postcode, string state, string emailAddress, string emailAddress2, string mobilePhone, string mobilePhone2, string faxNumber, string homePhone, string countryCode, bool isDummy)
		{
			IsDummy = isDummy;
			Master = person;
			IsInDatabase = person.IsInDatabase;
			RawName = person.PER_FullName;

			PER_PK = person.PK.ToGuid();
			PER_FullName = person.PER_FullName;
			PER_IsActive = person.PER_IsActive;
			PER_FriendlyName = person.PER_FriendlyName;

			PER_BirthDate = birthDate;
			PER_HomeAddress1 = homeAddress1;
			PER_HomeAddress2 = homeAddress2;
			PER_City = city;
			PER_Postcode = postcode;
			PER_State = state;
			PER_EmailAddress = emailAddress;
			PER_EmailAddress2 = emailAddress2;
			PER_MobilePhone = mobilePhone;
			PER_MobilePhone2 = mobilePhone2;
			PER_FaxNumber = faxNumber;
			PER_HomePhone = homePhone;
			PER_RN_NKCountry = countryCode;

			LoadDedupOrgContacts(person);

			GlbStaffs = person.StaffCollection
			.Cast<GlbStaff>()
			.Where(staff => staff != null)
			.Select(staff => new DeduplicationGlbStaff(staff, this))
			.ToArray();

			HRJobApplicants = person.ApplicantCollection
			.Cast<Integration.Recruiter.IHRJobApplicant>()
			.Where(applicant => applicant != null)
			.Select(applicant => new DeduplicationHRJobApplicant(applicant, this))
			.ToArray();

			CertificateOrAccreditations = person.Certificates
			.Cast<GenRegCertAccredMaintList>()
			.Where(certificate => certificate != null)
			.Select(certificate => new DeduplicationCertifcateOrAccreditation<IGlbPerson>(certificate))
			.ToArray();
		}

		void LoadDedupOrgContacts(GlbPerson person)
		{
			var dedupOrgHeaderMap = new Dictionary<Guid, DeduplicationOrgHeader>();
			var dedupBrandMap = new Dictionary<Guid, DeduplicationOrgBrandOrRelatedName>();
			var dedupOrgContactList = new List<DeduplicationOrgContact>();

			foreach (OrgContact orgContact in person.ContactCollection)
			{
				var orgPk = orgContact.OC_OH.ToGuid();
				var dedupOrgContact = new DeduplicationOrgContact(orgContact, this, true);

				if (!dedupOrgHeaderMap.TryGetValue(orgPk, out var deduplicationOrgHeader))
				{
					var orgHeader = person.Factory.Load<OrgHeader>(orgContact.OC_OH);
					if (orgHeader != null)
					{
						deduplicationOrgHeader = DeduplicationOrgHeader.CreateBridgeDedupOrgHeader(orgHeader);
						var dedupBrandList = new List<DeduplicationOrgBrandOrRelatedName>();

						foreach (OrgBrandOrRelatedName brand in orgHeader.BrandsOrRelatedNames)
						{
							var brandPk = brand.PK.ToGuid();
							if (!dedupBrandMap.TryGetValue(brandPk, out var dedupOrgBrandOrRelatedName))
							{
								dedupOrgBrandOrRelatedName = new DeduplicationOrgBrandOrRelatedName(brand, deduplicationOrgHeader);
								dedupBrandMap.Add(brandPk, dedupOrgBrandOrRelatedName);
							}
							dedupBrandList.Add(dedupOrgBrandOrRelatedName);
						}

						deduplicationOrgHeader.OrgBrandOrRelatedNames = dedupBrandList.ToArray();
						dedupOrgHeaderMap.Add(orgPk, deduplicationOrgHeader);
					}
				}
				dedupOrgContact.OrgHeader = deduplicationOrgHeader;
				dedupOrgContactList.Add(dedupOrgContact);
			}

			OrgContacts = dedupOrgContactList.ToArray();
		}

		public string GetRelatedTo(BusinessObjectFactory factory)
		{
			const int maxRelations = 2;
			const string seperator = ", ";
			const string elipsis = ", ... ";

			var distinctContactOrgs = OrgContacts.Select(contact => contact.OrgHeader).Distinct();
			var distinctStaffBranchPKs = GlbStaffs.Where(staff => staff.GS_GB_HomeBranch.HasValue).Select(staff => staff.GS_GB_HomeBranch).Distinct();
			var distinctStaffBranches = factory.Load<GlbBranch>(new ZQuery(GlbBranchSchema.PK, distinctStaffBranchPKs));
			var distinctStaffOrgPks = distinctStaffBranches.Select(branch => branch.GB_OH_OrgProxy).Distinct();
			var distinctStaffOrgs = factory.Load<OrgHeader>(new ZQuery(OrgHeaderSchema.PK, distinctStaffOrgPks));

			var distinctContactOrgCodes = distinctContactOrgs.Select(org => org.OH_Code);
			var distinctStaffOrgCodes = distinctStaffOrgs.Select(org => org.OH_Code.ToString());
			var allRelatedOrgs = distinctContactOrgCodes.Union(distinctStaffOrgCodes);

			var hasMoreThanMax = allRelatedOrgs.Count() > maxRelations;
			var relatedOrgsToDisplay = allRelatedOrgs.OrderBy(orgCode => orgCode).Take(maxRelations);
			var code = new StringBuilder();
			code.Append(string.Join(seperator, relatedOrgsToDisplay));
			if (hasMoreThanMax)
			{
				code.Append(elipsis);
				code.Append(Res.GetString("7e683908-8ee5-4d31-adf4-79627900a2a8", "({0} more)", allRelatedOrgs.Count() - maxRelations));
			}
			return code.ToString().TrimEnd(new[] { ',', ' ' });
		}

		public Type BizoType => typeof(GlbPerson);
		public bool IsInDatabase { get; }
		public Guid PK => PER_PK;
		public string CountryCode => PER_RN_NKCountry;
		public string TablePrefix => GlbPersonSchema.Constants.Prefix;
		public Guid PER_PK { get; set; }
		public DateTime? PER_BirthDate { get; set; }
		public string PER_City { get; set; }
		public string PER_DriversLicenseNumber { get; set; }
		public string PER_EmailAddress { get; set; }
		public string PER_EmailAddress2 { get; set; }
		public string PER_FaxNumber { get; set; }
		public string PER_FriendlyName { get; set; }
		public string PER_FriendlyNameAI { get; set; }
		public Guid? PER_IDPUserId { get; set; }
		public string PER_FullName { get; set; }
		public string PER_FullNameAI { get; set; }
		public string PER_Gender { get; set; }
		public string PER_HomeAddress1 { get; set; }
		public string PER_HomeAddress2 { get; set; }
		public string PER_HomePhone { get; set; }
		public bool PER_IsActive { get; set; }
		public bool PER_IsValid { get; set; }
		public string PER_LegalName { get; set; }
		public string PER_LegalNameAI { get; set; }
		public string PER_MobilePhone { get; set; }
		public string PER_MobilePhone2 { get; set; }
		public string PER_NameSuffix { get; set; }
		public string PER_Passport { get; set; }
		public DateTime? PER_PassportExpiryDate { get; set; }
		public string PER_PassportPlaceOfIssue { get; set; }
		public string PER_Postcode { get; set; }
		public string PER_PreferredLanguage { get; set; }
		public string PER_RN_NKCountry { get; set; }
		public string PER_RN_NKNationalityCodeISO { get; set; }
		public string PER_State { get; set; }
		public DateTime? PER_SystemCreateTimeUtc { get; set; }
		public string PER_SystemCreateUser { get; set; }
		public string PER_SystemCreateBranch { get; set; }
		public string PER_SystemCreateDepartment { get; set; }
		public DateTime? PER_SystemLastEditTimeUtc { get; set; }
		public string PER_SystemLastEditUser { get; set; }
		public string PER_NameTitle { get; set; }
		public string PER_ValidationStatus { get; set; }
		public bool PER_WebAccessEnabled { get; set; }
		public DateTime? PER_LoginDisabledUntilUtc { get; set; }

		public IRefCountryInfo Country { get; set; }
		public IRefCountryInfo NationalityCodeISO { get; set; }
		public IGlbStaffInfo CreatedByStaff { get; set; }
		public IGlbStaffInfo LastEditedByStaff { get; set; }
		public IDeduplicatable Master { get; }
		public bool ShouldRunDeduplication => Master?.ShouldRunDeduplication ?? false;
		public string PersonsTypeInfo => Master?.Info;
		public bool IsDummy { get; set; }

		public ICollection<IOrgContact> OrgContacts { get; private set; }

		public ICollection<IGlbPersonLanguage> GlbPersonLanguages { get; }

		public ICollection<IGlbStaff> GlbStaffs { get; }

		public ICollection<IHRJobApplicant> HRJobApplicants { get; }

		public ICollection<ICertificateOrAccreditation<IGlbPerson>> CertificateOrAccreditations { get; }
		public ICollection<IAcknowledgement<IGlbPerson>> Acknowledgements { get; }
		public ICollection<ILog<IGlbPerson>> Logs { get; }

		public (string ColName, string Value) AddressFull
		{
			get
			{
				var result = new StringBuilder();

				if (!string.IsNullOrEmpty(PER_HomeAddress1))
				{
					result.Append(PER_HomeAddress1);
				}

				if (!string.IsNullOrEmpty(PER_HomeAddress2))
				{
					result.Append(PER_HomeAddress2);
				}

				if (!string.IsNullOrEmpty(PER_City))
				{
					result.Append(" " + PER_City);
				}

				if (!string.IsNullOrEmpty(PER_State))
				{
					result.Append(" " + PER_State);
				}

				if (!string.IsNullOrEmpty(PER_Postcode))
				{
					result.Append(" " + PER_Postcode);
				}

				if (!string.IsNullOrEmpty(CountryCode))
				{
					result.Append(" " + CountryCode);
				}

				return ("AddressFull", result.ToString().Trim());
			}
		}

		public IList<(string ColName, string Value)> Address => new List<(string ColName, string Value)> {
			(nameof(PER_HomeAddress1), PER_HomeAddress1),
			(nameof(PER_HomeAddress2), PER_HomeAddress2)
		};

		public (string ColName, DateTime? Value) BirthDate => (nameof(PER_BirthDate), PER_BirthDate);

		public IList<(string ColName, string Value)> EmailAddresses => new List<(string ColName, string Value)>
		{
			(nameof(PER_EmailAddress), PER_EmailAddress),
			(nameof(PER_EmailAddress2), PER_EmailAddress2)
		};

		public (string ColName, string Value) FullName => (nameof(PER_FullName), PER_FullName);

		public IList<(string ColName, string Value)> PhoneNumbers => new List<(string ColName, string Value)>
		{
			(nameof(PER_FaxNumber), PER_FaxNumber),
			(nameof(PER_HomePhone), PER_HomePhone),
			(nameof(PER_MobilePhone), PER_MobilePhone),
			(nameof(PER_MobilePhone2), PER_MobilePhone2)
		};

		public Type GlowType => typeof(IGlbPerson);

		public string RawName { get; }

		public ICollection<IGlbPasswordHistory<IGlbPerson>> GlbPasswordHistories { get; }
	}
}
