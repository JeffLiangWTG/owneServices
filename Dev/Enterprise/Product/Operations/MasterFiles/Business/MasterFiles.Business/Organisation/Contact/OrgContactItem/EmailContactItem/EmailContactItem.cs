using System;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.MasterFiles.Business
{
	public class EmailContactItem : ContactItemProxy
	{
		public EmailContactItem(ZString description, ZPropertyInfo addressInfoOnOrgContact)
			: base(OrgContactItemTypes.Codes.Email, description, addressInfoOnOrgContact)
		{
		}

		public EmailContactItem(OrgContactItem orgContactItem)
			: base(orgContactItem)
		{
			if (orgContactItem.OI_ContactItemType != OrgContactItemTypes.Codes.Email)
			{
				throw new ArgumentException("orgContactItem.OI_ContactItemType must be EML, but was " + orgContactItem.OI_ContactItemType);
			}
		}

		public EmailContactItem(OrgContact contact)
			: base(contact, OrgContactItemTypes.Codes.Email)
		{
		}

		#region DefaultValues

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			OI_ContactItemType = OrgContactItemTypes.Codes.Email;
		}

		#endregion

		#region Properties

		[EmailAddress]
		public override ZString OI_Address
		{
			get
			{
				return base.OI_Address;
			}

			set
			{
				base.OI_Address = value;

				Contact?.Person?.FindDuplicates(Contact);
				Contact?.Header?.FindDuplicates();
			}
		}

		#endregion

		#region AddressInfoOnOrgContact

		protected override ZPropertyInfo GetAddressInfoOnOrgContact(OrgContact contact, string description)
		{
			if (contact != null && description == EmailContactItemDescriptionList.Codes.Main)
			{
				return contact.OC_EmailInfo;
			}

			return null;
		}

		#endregion

		#region Lookups

		protected override ContactItemProxyLookups GetNewLookups()
		{
			return new EmailContactItemLookups(this);
		}

		#endregion

		#region Validation

		protected override ContactItemProxyValidation GetNewValidation()
		{
			return new EmailContactItemValidation(this);
		}

		#endregion
	}
}
