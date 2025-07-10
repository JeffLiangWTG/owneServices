using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Glow.Model.Interfaces;
using CargoWise.Tools.DuplicateDetector;
using CargoWise.Tools.DuplicateDetector.Common;
using Enterprise.MasterData.Common;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterData.Business
{
	public class DeduplicationOrgContact : IOrgContact, IDeduplicationGlowObject, IPerson, IRawNameProvider
	{
		[Newtonsoft.Json.JsonConstructor]
		public DeduplicationOrgContact() { }

		public DeduplicationOrgContact(string contactName)
		{
			OC_ContactName = contactName;
			RawName = contactName;
		}

		public DeduplicationOrgContact(OrgContact contact, DeduplicationGlbPerson parentGlowGlbPerson, bool shouldIncludeChildren) : this(contact, shouldIncludeChildren)
		{
			GlbPerson = parentGlowGlbPerson;
		}

		public DeduplicationOrgContact(OrgContact contact, DeduplicationOrgHeader parentGlowOrgHeader, bool shouldIncludeChildren) : this(contact, shouldIncludeChildren)
		{
			OrgHeader = parentGlowOrgHeader;
		}

		public DeduplicationOrgContact(OrgContact contact, bool shouldIncludeChildren)
		{
			OC_PK = contact.PK.ToGuid();
			OC_ContactName = contact.OC_ContactName;
			OC_Birthday = contact.InsecureBirthday.IsValid ? contact.InsecureBirthday.ToDateTime() : default(DateTime?);
			OC_Email = contact.OC_Email;
			OC_Fax = contact.OC_Fax;
			OC_HomePhone = contact.OC_HomePhone;
			OC_IsActive = contact.OC_IsActive;
			OC_Language = contact.OC_Language;
			OC_Mobile = contact.OC_Mobile;
			OC_OH = !contact.OC_OH.IsEmpty && contact.OC_OH.IsValid ? contact.OC_OH.ToGuid() : Guid.Empty;
			OC_OtherPhone = contact.OC_OtherPhone;
			OC_Phone = contact.OC_Phone;
			OC_PER = contact.OC_PER.IsValid ? contact.OC_PER.ToGuid() : Guid.Empty;
			IsInDatabase = contact.IsInDatabase;
			RawName = contact.OC_ContactName;

			if (shouldIncludeChildren)
			{
				try
				{
					CertificateOrAccreditations = contact.Certificates
					.Cast<GenRegCertAccredMaintList>()
					.Where(certificate => certificate != null)
					.Select(certificate => new DeduplicationCertifcateOrAccreditation<IOrgContact>(certificate))
					.ToArray();

					OrgContactItems = contact.ContactItems
					.Cast<OrgContactItem>()
					.Where(item => item != null)
					.Select(item => new DeduplicationOrgContactItem(item, this))
					.ToArray();
				}
				catch (InvalidOperationException ex)
				{
					var devMessage = Res.GetString("7fbc8a91-c628-4f3b-994e-a5cde15d5e51", "{0} job applicant: {1}", ex.Message, contact.PK);
					ExceptionReporter.Instance.ReportDeveloperException("634a7059-1302-4068-afad-722522d06078", devMessage, ex);
				}
			}
		}

		public Type BizoType => typeof(OrgContact);
		public bool IsInDatabase { get; }
		public Guid PK => OC_PK;
		public string TablePrefix => OrgContactSchema.Constants.Prefix;
		//TODO: Determine country code
		public string CountryCode => string.Empty;
		public Guid OC_PK { get; set; }
		public string OC_AttachmentType { get; set; }
		public DateTime? OC_Birthday { get; set; }
		public string OC_ContactName { get; set; }
		public string OC_ContactSource { get; set; }
		public DateTime? OC_DetailsVerified { get; set; }
		public string OC_Email { get; set; }
		public string OC_Fax { get; set; }
		public string OC_Gender { get; set; }
		public string OC_HomePhone { get; set; }
		public bool OC_IsActive { get; set; }
		public bool OC_IsValid { get; set; }
		public string OC_JobCategory { get; set; }
		public string OC_Language { get; set; }
		public string OC_Mobile { get; set; }
		public string OC_NotifyMode { get; set; }
		public Guid? OC_OA_OrgAddress { get; set; }
		public string OC_ODP_NKDepartmentCode { get; set; }
		public Guid OC_OH { get; set; }
		public Guid? OC_OH_AddressOverride { get; set; }
		public string OC_OtherPhone { get; set; }
		public string OC_Pager { get; set; }
		public Guid OC_PER { get; set; }
		public string OC_PersonalInfo { get; set; }
		public string OC_Phone { get; set; }
		public string OC_PhoneExtension { get; set; }
		public string OC_RN_NKNationality { get; set; }
		public string OC_Salutation { get; set; }
		public DateTime? OC_SystemCreateTimeUtc { get; set; }
		public string OC_SystemCreateUser { get; set; }
		public string OC_SystemCreateBranch { get; set; }
		public string OC_SystemCreateDepartment { get; set; }
		public DateTime? OC_SystemLastEditTimeUtc { get; set; }
		public string OC_SystemLastEditUser { get; set; }
		public string OC_Title { get; set; }
		public bool OC_WebAccessEnabled { get; set; }
		public DateTime? OC_WebContractSignedDate { get; set; }
		public DateTime? OC_YearJoinedCompany { get; set; }
		public DateTime? OC_YearJoinedIndustry { get; set; }
		public IRefCountryInfo Nationality { get; set; }
		public IOrgAddress OrgAddress { get; set; }
		public IOrgHeader AddressOverride { get; set; }
		public IOrgHeader OrgHeader { get; set; }
		public IGlbPerson GlbPerson { get; set; }
		public IGlbStaffInfo CreatedByStaff { get; set; }
		public IGlbStaffInfo LastEditedByStaff { get; set; }

		public ICollection<IOrgContactAttribute> OrgContactAttributes { get; }

		public ICollection<IOrgContactItem> OrgContactItems { get; }

		public ICollection<IOrgDocument> OrgDocuments { get; }

		public ICollection<IOrgSecurityContact> OrgSecurityContacts { get; }

		public ICollection<IGlbGroupOrgContactLink> GlbGroupOrgContactLinks { get; }

		public ICollection<ICrmOpportunityContact> CrmOpportunityContacts { get; }

		public ICollection<IConversationParticipant<IOrgContact>> ConversationParticipants { get; }

		public ICollection<ILog<IOrgContact>> Logs { get; }

		public ICollection<IAcknowledgement<IOrgContact>> Acknowledgements { get; }

		public ICollection<ICertificateOrAccreditation<IOrgContact>> CertificateOrAccreditations { get; }

		public ICollection<IGlbPasswordHistory<IOrgContact>> GlbPasswordHistories { get; }

		public (string ColName, string Value) AddressFull => ("AddressFull", ((DeduplicationOrgAddress)OrgAddress)?.AddressFull);

		public IList<(string ColName, string Value)> Address => new List<(string ColName, string Value)> {
			("OA_Address1", ((DeduplicationOrgAddress)OrgAddress)?.OA_Address1),
			("OA_Address2", ((DeduplicationOrgAddress)OrgAddress)?.OA_Address2)
		};

		public (string ColName, DateTime? Value) BirthDate => (nameof(OC_Birthday), OC_Birthday);
		public IList<(string ColName, string Value)> EmailAddresses => new List<(string ColName, string Value)>
		{
			(nameof(OC_Email), OC_Email)
		};
		public (string ColName, string Value) FullName => (nameof(OC_ContactName), OC_ContactName);
		public IList<(string ColName, string Value)> PhoneNumbers => new List<(string ColName, string Value)>
		{
			(nameof(OC_Fax), OC_Fax),
			(nameof(OC_HomePhone), OC_HomePhone),
			(nameof(OC_Mobile), OC_Mobile),
			(nameof(OC_OtherPhone), OC_OtherPhone),
			(nameof(OC_Phone), OC_Phone)
		};
		public Type GlowType => typeof(IOrgContact);

		public string RawName { get; }
	}
}
