using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.US.Business;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.DocumentWrappers;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.US.DocumentWrappers
{
	public class USContactAddressSource : DocumentWrapper, IContactDetails
	{
		USContactAddressSource(USOrganisation uSOrganisation, BusinessObjectFactory factoryToWrap)
			: base(uSOrganisation, factoryToWrap)
		{
		}

		public static USContactAddressSource New(USOrganisation uSOrganisation, BusinessObjectFactory factoryToWrap)
		{
			return (uSOrganisation != null && uSOrganisation.Organisation != null) ? new USContactAddressSource(uSOrganisation, factoryToWrap) : null;
		}

		public override string ToString()
		{
			return ContactName;
		}

		public DocContacts Contact
		{
			get { return DocContacts.New(OrgContact, Factory); }
		}

		public DocAddress OrgAddress
		{
			get { return DocAddress.New(USOrganisation.Address, Factory); }
		}

		public Enterprise.DocumentWrappers.DocOrganisation Organisation
		{
			get { return Enterprise.DocumentWrappers.DocOrganisation.New(USOrganisation.Organisation, Factory); }
		}

		public Enterprise.DocumentWrappers.DocOrganisation AddressOverride
		{
			get { return Enterprise.DocumentWrappers.DocOrganisation.New(USOrganisation.Organisation, Factory); }
		}

		public ZString PostalAddress
		{
			get
			{
				ZString result = ZString.Empty;
				if (OrgAddress != null)
				{
					result = OrgAddress.PostalAddress;
				}
				else if (Contact != null)
				{
					result = Contact.PostalAddress;
				}
				return result;
			}
		}

		public ZString PostalAddressInEnglish
		{
			get
			{
				ZString result = ZString.Empty;

				if (OrgAddress != null)
				{
					result = OrgAddress.PostalAddressInEnglish;
				}
				else if (Contact != null)
				{
					result = Contact.PostalAddressInEnglish;
				}

				return result;
			}
		}

		public ZString Name
		{
			get
			{
				ZString result = USOrganisation.ZO_Contact + "\n" + USOrganisation.Organisation.OH_FullName;
				return result.Trim();
			}
		}

		public ZString Code
		{
			get { return (Contact != null) ? Contact.Code : ZString.Empty; }
		}

		public ZString AttachmentType
		{
			get { return (Contact != null) ? Contact.AttachmentType : ZString.Empty; }
		}

		public ZDateTime Birthday
		{
			get { return (Contact != null) ? Contact.Birthday : ZDateTime.Empty; }
		}

		public ZString ContactName
		{
			get { return USOrganisation.ZO_Contact; }
		}

		public ZString Email
		{
			get { return (Contact != null) ? Contact.Email : ZString.Empty; }
		}

		public ZString Fax
		{
			get { return (Contact != null) ? Contact.Fax : ZString.Empty; }
		}

		public ZString HomePhone
		{
			get { return (Contact != null) ? Contact.HomePhone : ZString.Empty; }
		}

		public ZString Language
		{
			get { return (Contact != null) ? Contact.Language : ZString.Empty; }
		}

		public ZString Mobile
		{
			get { return (Contact != null) ? Contact.Mobile : ZString.Empty; }
		}

		public ZString NotifyMode
		{
			get { return (Contact != null) ? Contact.NotifyMode : ZString.Empty; }
		}

		public ZString OtherPhone
		{
			get { return (Contact != null) ? Contact.OtherPhone : ZString.Empty; }
		}

		public ZString Pager
		{
			get { return (Contact != null) ? Contact.Pager : ZString.Empty; }
		}

		public ZString Password
		{
			get { return (Contact != null) ? Contact.Password : ZString.Empty; }
		}

		public ZString PersonalInfo
		{
			get { return (Contact != null) ? Contact.PersonalInfo : ZString.Empty; }
		}

		public ZString Phone
		{
			get { return USOrganisation.ZO_Phone_Formatted; }
		}

		public ZString Title
		{
			get { return (Contact != null) ? Contact.Title : ZString.Empty; }
		}

		#region Implementation

		USOrganisation USOrganisation
		{
			get { return (USOrganisation)WrappedObject; }
		}

		OrgContact OrgContact
		{
			get
			{
				if (fOrgContact == null || fOrgContact.OC_ContactName != USOrganisation.ZO_Contact)
				{
					ZQuery query = new ZQuery(OrgContactSchema.OC_ContactName, USOrganisation.ZO_Contact);
					fOrgContact = Factory.LoadTop1<OrgContact>(query);
				}
				return fOrgContact;
			}
		}
		OrgContact fOrgContact;

		#endregion
	}
}
