using System.Text;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.MasterFiles.Business
{
	public class ZAddressWithContact : ZAddress
	{
		#region Schema

		public abstract new class Schema : ZAddress.Schema
		{
			public const string ContactFK = "ContactFK";
			public const string ContactDetails = "ContactDetails";
		}

		#endregion

		public DefaultContactHandler GetDefaultContact;

		public ZAddressWithContact(ZPropertyInfo contactFKInfo, ZPropertyInfo addressFKInfo)
			: base(addressFKInfo)
		{
			Argument.NotNull(addressFKInfo, "addressFKInfo");
			Argument.NotNull(contactFKInfo, "contactFKInfo");
			fContactFKInfo = GetWrappedZPropertyInfo(Schema.ContactFK, x => contactFKInfo);
			SetContactDetails();
			AddressFKInfo.ValueChanged += (s, e) =>
			{
				SetDefaultContact();
				SetContactDetails();
			};
			OrgPKInfo.ValueChanged += (s, e) =>
			{
				ContactFK = ZGuid.Empty;
				SetDefaultContact();
			};
		}

		public ZAddressWithContact(ZPropertyInfo contactFKInfo, ZPropertyInfo addressFKInfo, DefaultContactHandler defaultContactHandler) : this(contactFKInfo, addressFKInfo)
		{
			GetDefaultContact = defaultContactHandler;
			if (ContactFK == ZGuid.Empty)
			{
				SetDefaultContact();
			}
		}

		void SetDefaultContact()
		{
			if (GetDefaultContact != null)
			{
				ContactFK = GetDefaultContact(OrgHeaderAsIOrgHeader, OrgAddressAsIOrgAddress);
			}
		}

		#region Overrides

		new OrgHeader OrgHeader
		{
			get { return (OrgHeader)base.OrgHeader; }
		}

		new OrgAddress OrgAddress
		{
			get { return (OrgAddress)base.OrgAddress; }
		}

		#endregion

		#region ContactFKInfo

		readonly ZWrappedPropertyInfo fContactFKInfo;
		public ZWrappedPropertyInfo ContactFKInfo
		{
			get
			{
				return fContactFKInfo;
			}
		}

		[List("ContactsActive")]
		public ZGuid ContactFK
		{
			get { return (ZGuid)ContactFKInfo.InnerInfo.Value; }
			set
			{
				ContactFKInfo.InnerInfo.Value = value;
				SetContactDetails();
			}
		}

		[ChildEditable(false)]
		public OrgContactDependentCollection ContactsActive
		{
			get
			{
				return OrgHeader == null ? null : OrgHeader.ContactsActive;
			}
		}

		public bool ContactFK_ReadOnly
		{
			get
			{
				return OrgHeader == null || ContactFKInfo.InnerInfo.ReadOnly;
			}
		}

		#endregion

		OrgContact fOrgContact;
		public OrgContact OrgContact
		{
			get
			{
				ZGuid contactPK = (ZGuid)ContactFKInfo.Value;

				if (fOrgContact == null || contactPK != fOrgContact.PK)
				{
					fOrgContact = Factory.Load<OrgContact>(contactPK);
				}

				return fOrgContact;
			}
		}

		#region Contact Details

		public void SetContactDetails()
		{
			var contactDetails = new ContactDetailsGUIFormatter(OrgContact, OrgAddress, OrgHeader).GetContactDetailsForGUI();
			ContactDetails_Phone = contactDetails.Phone;
			ContactDetails_Fax = contactDetails.Fax;
			ContactDetails_Email = contactDetails.Email;
			ContactDetails_Web = contactDetails.Web;
			ContactDetails_WebLink = contactDetails.WebLink;
			ContactDetails_PhoneEnabled = contactDetails.PhoneEnabled;
			ContactDetails_FaxVisible = contactDetails.FaxVisible;
			ContactDetails_EmailVisible = contactDetails.EmailVisible;
			ContactDetails_WebVisible = contactDetails.WebVisible;
			ContactDetails_WebLinkVisible = contactDetails.WebLinkVisible;
			ContactDetails_PhoneCore = contactDetails.PhoneCore;
			ContactDetails_FaxCore = contactDetails.FaxCore;
			RefreshBinding();
		}

		public ZString ContactDetails_Phone { get; private set; }
		public ZPropertyInfo ContactDetails_PhoneInfo { get { return GetZPropertyInfo(nameof(ContactDetails_Phone)); } }
		public ZString ContactDetails_Fax { get; private set; }
		public ZPropertyInfo ContactDetails_FaxInfo { get { return GetZPropertyInfo(nameof(ContactDetails_Fax)); } }
		public ZString ContactDetails_Email { get; private set; }
		public ZPropertyInfo ContactDetails_EmailInfo { get { return GetZPropertyInfo(nameof(ContactDetails_Email)); } }
		public ZString ContactDetails_Web { get; private set; }
		public ZPropertyInfo ContactDetails_WebInfo { get { return GetZPropertyInfo(nameof(ContactDetails_Web)); } }
		public ZString ContactDetails_WebLink { get; private set; }
		public ZPropertyInfo ContactDetails_WebLinkInfo { get { return GetZPropertyInfo(nameof(ContactDetails_WebLink)); } }
		public ZBool ContactDetails_PhoneEnabled { get; private set; }
		public ZPropertyInfo ContactDetails_PhoneEnabledInfo { get { return GetZPropertyInfo(nameof(ContactDetails_PhoneEnabled)); } }
		public ZBool ContactDetails_FaxVisible { get; private set; }
		public ZPropertyInfo ContactDetails_FaxVisibleInfo { get { return GetZPropertyInfo(nameof(ContactDetails_FaxVisible)); } }
		public ZBool ContactDetails_EmailVisible { get; private set; }
		public ZPropertyInfo ContactDetails_EmailVisibleInfo { get { return GetZPropertyInfo(nameof(ContactDetails_EmailVisible)); } }
		public ZBool ContactDetails_WebVisible { get; private set; }
		public ZPropertyInfo ContactDetails_WebVisibleInfo { get { return GetZPropertyInfo(nameof(ContactDetails_WebVisible)); } }
		public ZBool ContactDetails_WebLinkVisible { get; private set; }
		public ZPropertyInfo ContactDetails_WebLinkVisibleInfo { get { return GetZPropertyInfo(nameof(ContactDetails_WebLinkVisible)); } }
		public ZString ContactDetails_PhoneCore { get; private set; }
		public ZString ContactDetails_FaxCore { get; private set; }

		#endregion

		public delegate ZGuid DefaultContactHandler(IOrgHeader orgHeader, IOrgAddress orgAddress);
	}

	public class ContactDetailsForGUI
	{
		public ZString Phone { get; set; }
		public ZString Fax { get; set; }
		public ZString Email { get; set; }
		public ZString Web { get; set; }
		public ZString WebLink { get; set; }

		public ZBool PhoneEnabled { get; set; }
		public ZBool FaxVisible { get; set; }
		public ZBool EmailVisible { get; set; }
		public ZBool WebVisible { get; set; }
		public ZBool WebLinkVisible { get; set; }

		public ZGuid OrgPK { get; set; }
		public ZGuid ContactPK { get; set; }

		public ZString PhoneCore { get; set; }
		public ZString FaxCore { get; set; }
	}

	public class ContactDetailsGUIFormatter
	{
		public static class OrganisationMessages
		{
			public static string NoPhoneFoundOnFile { get { return Res.GetString("OrganisationMessages.NoPhoneFoundOnFile", "Ph: *NOT FOUND*"); } }
			public static string NoFaxFoundOnFile { get { return Res.GetString("OrganisationMessages.NoFaxFoundOnFile", "Fax: *NOT FOUND*"); } }
			public static string NoEmailFoundOnFile { get { return Res.GetString("OrganisationMessages.NoEmailFoundOnFile", "Em: *NOT FOUND*"); } }
			public static string NoWebFoundOnFile { get { return Res.GetString("OrganisationMessages.NoWebFoundOnFile", "Web: *NOT FOUND*"); } }
		}

		public ContactDetailsGUIFormatter(OrgContact orgContact, OrgAddress orgAddress, OrgHeader orgHeader)
		{
			this.orgContact = orgContact;
			this.orgAddress = orgAddress;
			this.orgHeader = orgHeader;
		}

		readonly OrgContact orgContact;
		readonly OrgAddress orgAddress;
		readonly OrgHeader orgHeader;

		public ContactDetailsForGUI GetContactDetailsForGUI()
		{
			var details = new ContactDetailsForGUI
			{
				OrgPK = orgHeader == null ? ZGuid.Empty : orgHeader.PK,
				ContactPK = orgContact == null ? ZGuid.Empty : orgContact.PK,
				Phone = OrgPhone,
				PhoneCore = OrgPhoneCore,
				PhoneEnabled = true,
				Fax = OrgFax,
				FaxCore = OrgFaxCore,
				FaxVisible = true
			};

			bool contactSelected = orgHeader != null && orgContact != null;

			if (contactSelected)
			{
				details.Email = OrgEmail;

				string webAddress = OrgWeb.Trim();
				bool isValidWeb = (webAddress != OrganisationMessages.NoWebFoundOnFile);

				if (isValidWeb)
				{
					details.Web = Res.GetString("ZAddressWithContact|d412a9bb-d9c4-4099-937b-436a0627c8f9", "Web:") + " ";
					details.WebLink = webAddress;
					details.WebLinkVisible = true;
				}
				else
				{
					details.Web = webAddress;
					details.WebLinkVisible = false;
				}
			}
			else
			{
				details.WebLinkVisible = false;
			}

			details.EmailVisible = details.WebVisible = contactSelected;
			return details;
		}

		string OrgPhone
		{
			get
			{
				StringBuilder result = new StringBuilder();

				if (!OrgPhoneCore.IsEmpty)
				{
					result.Append(Res.GetString("ZAddressWithContact|8d952c7e-1994-44a8-91a2-2d3908cf57a7", "Ph: {0}", OrgPhoneCore));
				}
				if (!OrgMobileCore.IsEmpty)
				{
					if (result.Length > 0)
					{
						result.Append(Res.GetString("ZAddressWithContact|29eb2120-3184-4e8e-94a1-6b1b50602c63", ", (M) {0}", OrgMobileCore));
					}
					else
					{
						result.Append(Res.GetString("ZAddressWithContact|8d952c7e-1994-44a8-91a2-2d3908cf57a7", "Ph: {0}", OrgMobileCore));
					}
				}

				return (result.Length == 0) ? OrganisationMessages.NoPhoneFoundOnFile : result.ToString();
			}
		}

		string OrgFax
		{
			get { return OrgFaxCore.IsEmpty ? OrganisationMessages.NoFaxFoundOnFile : Res.GetString("ZAddressWithContact|6623c9d9-2b8d-4720-bfbe-4263a1a75df7", "Fax: {0}", OrgFaxCore); }
		}

		string OrgEmail
		{
			get { return OrgEmailCore.IsEmpty ? OrganisationMessages.NoEmailFoundOnFile : Res.GetString("ZAddressWithContact|d589fabe-cc50-4809-932b-b8bb6308d94f", "Em: {0}", OrgEmailCore); }
		}

		string OrgWeb
		{
			get { return OrgWebCore.IsEmpty ? OrganisationMessages.NoWebFoundOnFile : (string)OrgWebCore; }
		}

		protected virtual ZString OrgPhoneCore
		{
			get
			{
				return TryGetPhoneFromContact(out var contactPhone) ? contactPhone :
					TryGetPhoneFromAddress(out var addressPhone) ? addressPhone :
					TryGetPhoneFromOrg(out var orgPhone) ? orgPhone :
					ZString.Empty;
			}
		}

		protected virtual ZString OrgMobileCore
		{
			get { return orgContact == null ? ZString.Empty : orgContact.OC_Mobile; }
		}

		protected virtual ZString OrgFaxCore
		{
			get
			{
				return TryGetFaxFromContact(out var contactFax) ? contactFax :
					TryGetFaxFromAddress(out var addressFax) ? addressFax :
					TryGetFaxFromOrg(out var orgFax) ? orgFax :
					ZString.Empty;
			}
		}

		protected virtual ZString OrgEmailCore
		{
			get { return orgContact == null ? ZString.Empty : orgContact.EmailFallbackToOrganisation; }
		}

		protected virtual ZString OrgWebCore
		{
			get { return orgHeader == null ? ZString.Empty : orgHeader.MainWebURL.PU_URL; }
		}

		bool TryGetPhoneFromContact(out ZString contactPhone)
		{
			if (orgContact == null || orgContact.OC_Phone.IsEmpty)
			{
				contactPhone = default;
				return false;
			}

			contactPhone = orgContact.OC_Phone;
			return true;
		}

		bool TryGetPhoneFromAddress(out ZString addressPhone)
		{
			if (orgAddress == null || orgAddress.OA_Phone.IsEmpty || (orgContact != null && orgAddress.PK != orgContact.WorkingAddressPK))
			{
				addressPhone = default;
				return false;
			}

			addressPhone = orgAddress.OA_Phone;
			return true;
		}

		bool TryGetPhoneFromOrg(out ZString orgPhone)
		{
			if (orgHeader == null || (orgHeader.MainAddress?.OA_Phone.IsEmpty ?? true))
			{
				orgPhone = default;
				return false;
			}

			orgPhone = orgHeader.MainAddress.OA_Phone;
			return true;
		}

		bool TryGetFaxFromContact(out ZString contactFax)
		{
			if (orgContact == null || orgContact.OC_Fax.IsEmpty)
			{
				contactFax = default;
				return false;
			}

			contactFax = orgContact.OC_Fax;
			return true;
		}

		bool TryGetFaxFromAddress(out ZString addressFax)
		{
			if (orgAddress == null || orgAddress.OA_Fax.IsEmpty || (orgContact != null && orgAddress.PK != orgContact.WorkingAddressPK))
			{
				addressFax = default;
				return false;
			}

			addressFax = orgAddress.OA_Fax;
			return true;
		}

		bool TryGetFaxFromOrg(out ZString orgFax)
		{
			if (orgHeader == null || (orgHeader.MainAddress?.OA_Fax.IsEmpty ?? true))
			{
				orgFax = default;
				return false;
			}

			orgFax = orgHeader.MainAddress.OA_Fax;
			return true;
		}
	}
}
