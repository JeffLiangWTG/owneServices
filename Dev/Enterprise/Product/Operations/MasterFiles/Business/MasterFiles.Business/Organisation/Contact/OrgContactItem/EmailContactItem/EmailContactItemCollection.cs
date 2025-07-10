using System;
using System.Linq;
using CargoWise.EntityFramework;

namespace Enterprise.MasterFiles.Business
{
	public class EmailContactItemCollection : ContactItemProxyCollection<EmailContactItem>
	{
		public EmailContactItemCollection(OrgContact contact)
			: base(contact, OrgContactItemTypes.Codes.Email)
		{
			contact.OC_EmailInfo.ValueChanged += OC_EmailInfo_ValueChanged;
		}

		#region Add

		protected override EmailContactItem GetNewContactItemProxy(OrgContactItem orgContactItem)
		{
			return new EmailContactItem(orgContactItem);
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new EmailContactItem(Contact);
		}

		#endregion

		#region AddItemsForOrgContactColumns

		void OC_EmailInfo_ValueChanged(object sender, EventArgs e)
		{
			var valueChangedEventArgs = e as ValueChangedEventArgs;
			if (valueChangedEventArgs != null && valueChangedEventArgs.OldValue.IsEmpty)
			{
				var address = Contact.OC_Email;
				if (!address.IsEmpty)
				{
					if (!Items.Any(item => item.AddressInfoOnOrgContact == Contact.OC_EmailInfo))
					{
						AddNewItemForAddressOnOrgContact();
					}
				}
			}
		}

		protected override void AddItemsForOrgContactColumnsCore()
		{
			base.AddItemsForOrgContactColumnsCore();

			var primaryEmail = Contact.OC_Email;
			if (!primaryEmail.IsEmpty && !Items.Any(item => item.AddressInfoOnOrgContact == Contact.OC_EmailInfo))
			{
				AddNewItemForAddressOnOrgContact();
			}
		}

		void AddNewItemForAddressOnOrgContact()
		{
			var mailEmailItem = new EmailContactItem(EmailContactItemDescriptionList.Codes.Main, Contact.OC_EmailInfo);
			mailEmailItem.HasChanges = false;
			var address = Items.SingleOrDefault(item => item.OI_Description.Equals(EmailContactItemDescriptionList.Codes.Main));
			if (address != null)
			{
				Remove(address);
			}
			Add(mailEmailItem);
		}

		#endregion
	}
}
