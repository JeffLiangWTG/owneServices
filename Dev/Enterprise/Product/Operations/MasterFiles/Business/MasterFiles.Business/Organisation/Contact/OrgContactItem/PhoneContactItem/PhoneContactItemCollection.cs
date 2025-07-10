using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.MasterFiles.Business
{
	public class PhoneContactItemCollection : ContactItemProxyCollection<PhoneContactItem>
	{
		public PhoneContactItemCollection(OrgContact contact)
			: base(contact, OrgContactItemTypes.Codes.Phone)
		{
			contact.OC_FaxInfo.ValueChanged += (sender, e) => CheckAddNewItemForAddressOnOrgContact(e as ValueChangedEventArgs, contact.OC_FaxInfo, PhoneContactItemDescriptionList.Codes.Fax);
			contact.OC_HomePhoneInfo.ValueChanged += (sender, e) => CheckAddNewItemForAddressOnOrgContact(e as ValueChangedEventArgs, contact.OC_HomePhoneInfo, PhoneContactItemDescriptionList.Codes.Home);
			contact.OC_MobileInfo.ValueChanged += (sender, e) => CheckAddNewItemForAddressOnOrgContact(e as ValueChangedEventArgs, contact.OC_MobileInfo, PhoneContactItemDescriptionList.Codes.Mobile);
			contact.OC_OtherPhoneInfo.ValueChanged += (sender, e) => CheckAddNewItemForAddressOnOrgContact(e as ValueChangedEventArgs, contact.OC_OtherPhoneInfo, PhoneContactItemDescriptionList.Codes.Other);
			contact.OC_PagerInfo.ValueChanged += (sender, e) => CheckAddNewItemForAddressOnOrgContact(e as ValueChangedEventArgs, contact.OC_PagerInfo, PhoneContactItemDescriptionList.Codes.Pager);
			contact.OC_PhoneInfo.ValueChanged += (sender, e) => CheckAddNewItemForAddressOnOrgContact(e as ValueChangedEventArgs, contact.OC_PhoneInfo, PhoneContactItemDescriptionList.Codes.Work);
			contact.OC_PhoneExtensionInfo.ValueChanged += (sender, e) => CheckAddNewItemForAddressOnOrgContact(e as ValueChangedEventArgs, contact.OC_PhoneExtensionInfo, PhoneContactItemDescriptionList.Codes.Extension);
		}

		#region Add

		protected override PhoneContactItem GetNewContactItemProxy(OrgContactItem orgContactItem)
		{
			return new PhoneContactItem(orgContactItem);
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new PhoneContactItem(Contact);
		}

		#endregion

		#region AddItemsForOrgContactColumns

		void CheckAddNewItemForAddressOnOrgContact(ValueChangedEventArgs e, ZPropertyInfo addressInfo, ZString description)
		{
			if (e != null)
			{
				if (e.OldValue.IsEmpty)
				{
					var address = (ZString)addressInfo.Value;
					if (!address.IsEmpty)
					{
						if (!Items.Any(item => item.AddressInfoOnOrgContact == addressInfo))
						{
							AddNewItemForAddressOnOrgContact(addressInfo, description);
						}
					}
				}
				else if (e.OldValue != e.NewValue)
				{
					var contactItem = Items.SingleOrDefault(item => item.OI_Description.Equals(description));
					if (contactItem != null)
					{
						contactItem.HasChanges = true;
					}
				}
			}
		}

		protected override void AddItemsForOrgContactColumnsCore()
		{
			base.AddItemsForOrgContactColumnsCore();

			var addressInfoOnOrgContactAlreadyInCollection = new HashSet<ZPropertyInfo>(Items.Select(item => item.AddressInfoOnOrgContact).Where(info => info != null));
			foreach (var descriptionOrgContactColumnPair in PhoneContactItemLookups.GetOrgContactColumnsByDescription(Factory))
			{
				var addressInfo = Contact.ZPropertyInfoHash[descriptionOrgContactColumnPair.Value.Name];
				if (!addressInfoOnOrgContactAlreadyInCollection.Contains(addressInfo))
				{
					var address = (ZString)addressInfo.Value;
					if (!address.IsEmpty)
					{
						AddNewItemForAddressOnOrgContact(addressInfo, descriptionOrgContactColumnPair.Key);
					}
				}
			}
		}

		void AddNewItemForAddressOnOrgContact(ZPropertyInfo addressInfo, ZString description)
		{
			var phoneItem = new PhoneContactItem(description, addressInfo);
			phoneItem.HasChanges = false;
			var address = Items.SingleOrDefault(item => item.OI_Description.Equals(description));
			if (address != null)
			{
				Remove(address);
			}
			Add(phoneItem);
		}

		#endregion
	}
}
