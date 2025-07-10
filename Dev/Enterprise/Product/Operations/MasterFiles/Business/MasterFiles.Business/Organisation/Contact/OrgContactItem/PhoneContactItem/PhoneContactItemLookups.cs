using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public class PhoneContactItemLookups : ContactItemProxyLookups
	{
		public PhoneContactItemLookups(PhoneContactItem phoneContactItem)
			: base(phoneContactItem)
		{
		}

		new PhoneContactItem Parent
		{
			get { return (PhoneContactItem)base.Parent; }
		}

		public override CodeDescriptionPairList DescriptionList
		{
			get { return new PhoneContactItemDescriptionList(); }
		}

		public override CodeDescriptionPairList SelectableDescriptionList
		{
			get
			{
				var result = new PhoneContactItemDescriptionList();
				var contact = Parent.Contact;
				if (contact != null)
				{
					foreach (var description in contact.PhoneContactItems.Items.Select(item => item.OI_Description))
					{
						if (!description.EqualsIgnoringCase(Parent.OI_Description))
						{
							result.RemoveCode(description);
						}
					}
				}

				return result;
			}
		}

		public Dictionary<string, SchemaStringColumn> OrgContactColumnsByDescription
		{
			get { return GetOrgContactColumnsByDescription(Factory); }
		}

		public Dictionary<string, string> OrgContactPhoneIsManuallyVerifiedColumnsByDescription
		{
			get { return GetOrgContactPhoneIsManuallyVerifiedColumnsByDescription(Factory); }
		}

		public static Dictionary<string, SchemaStringColumn> GetOrgContactColumnsByDescription(BusinessObjectFactory factory)
		{
			return factory.GetCachedValue("PhoneContactItemLookups.OrgContactColumnsByDescription", () =>
			{
				var result = new Dictionary<string, SchemaStringColumn>();
				result.Add(PhoneContactItemDescriptionList.Codes.Extension, OrgContactSchema.OC_PhoneExtension);
				result.Add(PhoneContactItemDescriptionList.Codes.Fax, OrgContactSchema.OC_Fax);
				result.Add(PhoneContactItemDescriptionList.Codes.Home, OrgContactSchema.OC_HomePhone);
				result.Add(PhoneContactItemDescriptionList.Codes.Mobile, OrgContactSchema.OC_Mobile);
				result.Add(PhoneContactItemDescriptionList.Codes.Other, OrgContactSchema.OC_OtherPhone);
				result.Add(PhoneContactItemDescriptionList.Codes.Pager, OrgContactSchema.OC_Pager);
				result.Add(PhoneContactItemDescriptionList.Codes.Work, OrgContactSchema.OC_Phone);

				return result;
			});
		}

		public static Dictionary<string, string> GetOrgContactPhoneIsManuallyVerifiedColumnsByDescription(BusinessObjectFactory factory)
		{
			return factory.GetCachedValue("PhoneContactItemLookups.OrgContactPhoneIsManuallyVerifiedColumnsByDescription", () =>
			{
				var result = new Dictionary<string, string>();
				result.Add(PhoneContactItemDescriptionList.Codes.Fax, OrgContact.Schema.OC_Fax_IsManuallyVerified);
				result.Add(PhoneContactItemDescriptionList.Codes.Home, OrgContact.Schema.OC_HomePhone_IsManuallyVerified);
				result.Add(PhoneContactItemDescriptionList.Codes.Mobile, OrgContact.Schema.OC_Mobile_IsManuallyVerified);
				result.Add(PhoneContactItemDescriptionList.Codes.Other, OrgContact.Schema.OC_OtherPhone_IsManuallyVerified);
				result.Add(PhoneContactItemDescriptionList.Codes.Pager, OrgContact.Schema.OC_Pager_IsManuallyVerified);
				result.Add(PhoneContactItemDescriptionList.Codes.Work, OrgContact.Schema.OC_Phone_IsManuallyVerified);

				return result;
			});
		}
	}
}
