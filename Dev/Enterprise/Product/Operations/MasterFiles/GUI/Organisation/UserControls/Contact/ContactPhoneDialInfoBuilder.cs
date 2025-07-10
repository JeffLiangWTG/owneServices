using System.Collections.Generic;
using System.Linq;
using Enterprise.MasterFiles.Business;

namespace Enterprise.MasterFiles.GUI
{
	public class ContactPhoneDialInfoBuilder
	{
		public PhoneDialInfo GetDefaultDialInfo(OrgContact contact, OrgHeader org)
		{
			return GetContactWorkPhoneDialInfo(contact) ?? GetOfficePhoneDialInfo(contact, org);
		}

		public IEnumerable<PhoneDialInfo> GetAlternativePhoneDialInfos(OrgContact contact, OrgHeader org)
		{
			if (contact != null)
			{
				var extension = contact.PhoneContactItems.GetMostImportantItem(PhoneContactItemDescriptionList.Codes.Extension);
				foreach (var item in contact.PhoneContactItems.Items.Where(item => item.IsCallable))
				{
					if (extension != null && item.OI_Description == PhoneContactItemDescriptionList.Codes.Work)
					{
						yield return new PhoneDialInfo(item, extension);
					}
					else
					{
						yield return new PhoneDialInfo(item);
					}
				}
			}

			var officePhoneDialInfo = GetOfficePhoneDialInfo(contact, org);
			if (officePhoneDialInfo != null)
			{
				yield return officePhoneDialInfo;
			}
		}

		protected PhoneDialInfo GetContactWorkPhoneDialInfo(OrgContact contact)
		{
			if (contact != null)
			{
				var contactWorkPhone = contact.PhoneContactItems.GetMostImportantItem(PhoneContactItemDescriptionList.Codes.Work);
				if (contactWorkPhone != null)
				{
					var contactExtension = contact.PhoneContactItems.GetMostImportantItem(PhoneContactItemDescriptionList.Codes.Extension);
					return contactExtension == null ? new PhoneDialInfo(contactWorkPhone) : new PhoneDialInfo(contactWorkPhone, contactExtension);
				}
			}
			return null;
		}

		protected virtual PhoneDialInfo GetOfficePhoneDialInfo(OrgContact contact, OrgHeader org)
		{
			return GetContactOrgPhoneDialInfo(org);
		}

		protected PhoneDialInfo GetContactOrgPhoneDialInfo(OrgHeader org)
		{
			if (org != null)
			{
				var contactOrgPhone = org.MainAddress.OA_Phone;
				if (!contactOrgPhone.IsEmpty)
				{
					return new PhoneDialInfo(contactOrgPhone, OfficeDescription);
				}
			}

			return null;
		}

		protected string OfficeDescription
		{
			get { return ResString.GetMultilingualString("9ddf594d-10f0-4f45-bfbe-e85822a10305", "Office"); }
		}
	}
}
