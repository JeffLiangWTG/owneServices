using System;
using System.ComponentModel;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;

namespace Enterprise.MasterFiles.Business
{
	[ProvideMetaDataProperty("ReadOnlySecurity", MetaDataTypes.ReadOnly)]
	public abstract class ContactItemProxy : AutoContactItemProxy
	{
		protected ContactItemProxy(ZString contactItemType, ZString description, ZPropertyInfo addressInfoOnOrgContact)
			: base(addressInfoOnOrgContact.BizObj.Factory)
		{
			using (GetValidationSuspender())
			using (SuspendSettingHasChanges())
			{
				this.OrgContactItem = null;
				this.AddressInfoOnOrgContact = addressInfoOnOrgContact;

				base.OI_Address = (ZString)addressInfoOnOrgContact.Value;
				base.OI_ContactItemType = contactItemType;
				base.OI_Description = description;
				base.OI_IsPrimary = ZBool.True;
				base.OI_OC = addressInfoOnOrgContact.BizObj.PK;
			}
		}

		protected ContactItemProxy(OrgContactItem orgContactItem)
			: base(orgContactItem.Factory)
		{
			using (GetValidationSuspender())
			using (SuspendSettingHasChanges())
			{
				this.AddressInfoOnOrgContact = null;
				this.OrgContactItem = orgContactItem;

				base.OI_Address = orgContactItem.OI_Address;
				base.OI_Address_IsManuallyVerified = orgContactItem.OI_Address_IsManuallyVerified;
				base.OI_ContactItemType = orgContactItem.OI_ContactItemType;
				base.OI_Description = orgContactItem.OI_Description;
				base.OI_IsPrimary = orgContactItem.OI_IsPrimary;
				base.OI_OC = orgContactItem.OI_OC;
			}
		}

		protected ContactItemProxy(OrgContact contact, ZString contactItemType)
			: base(contact.Factory)
		{
			using (GetValidationSuspender())
			using (SuspendSettingHasChanges())
			{
				this.OrgContactItem = null;
				this.AddressInfoOnOrgContact = null;

				base.OI_Address = ZString.Empty;
				base.OI_ContactItemType = contactItemType;
				base.OI_Description = ZString.Empty;
				base.OI_IsPrimary = ZBool.True;
				base.OI_OC = contact.PK;
			}
		}

		#region AddressInfoOnOrgContact

		public ZPropertyInfo AddressInfoOnOrgContact
		{
			get { return addressInfoOnOrgContact; }
			private set
			{
				if (addressInfoOnOrgContact != value)
				{
					UnhookOrgContactValueChangedEvents();
					addressInfoOnOrgContact = value;
					HookOrgContactValueChangedEvents();
				}
			}
		}
		ZPropertyInfo addressInfoOnOrgContact;

		void UnhookOrgContactValueChangedEvents()
		{
			if (addressInfoOnOrgContact != null)
			{
				addressInfoOnOrgContact.ValueChanged -= addressInfoOnOrgContact_ValueChanged;
			}
		}

		void HookOrgContactValueChangedEvents()
		{
			if (addressInfoOnOrgContact != null)
			{
				addressInfoOnOrgContact.ValueChanged += addressInfoOnOrgContact_ValueChanged;
			}
		}

		void addressInfoOnOrgContact_ValueChanged(object sender, EventArgs e)
		{
			if (!isSettingProperty)
			{
				var value = (ZString)addressInfoOnOrgContact.Value;
				base.OI_Address = value;
			}
		}

		protected virtual ZPropertyInfo GetAddressInfoOnOrgContact(OrgContact contact, string description)
		{
			return null;
		}

		protected virtual ZPropertyInfo GetAddressIsManuallyVerifiedInfoOnOrgContact(OrgContact contact, string description)
		{
			return null;
		}

		#endregion

		#region OrgContactItem

		OrgContactItem CreateNewOrgContactItem()
		{
			var result = Factory.New<OrgContactItem>();

			using (result.SuspendSettingHasChanges())
			using (result.GetValidationSuspender())
			{
				result.OI_Address = base.OI_Address;
				result.OI_Address_IsManuallyVerified = base.OI_Address_IsManuallyVerified;
				result.OI_ContactItemType = base.OI_ContactItemType;
				result.OI_Description = base.OI_Description;
				result.OI_IsPrimary = base.OI_IsPrimary;
				result.OI_OC = base.OI_OC;
			}

			return result;
		}

		public OrgContactItem OrgContactItem
		{
			get { return orgContactItem; }
			private set
			{
				if (orgContactItem != value)
				{
					UnhookOrgContactItemValueChangedEvents();
					orgContactItem = value;
					HookOrgContactItemValueChangedEvents();
				}
			}
		}
		OrgContactItem orgContactItem;

		void UnhookOrgContactItemValueChangedEvents()
		{
			if (orgContactItem != null)
			{
				orgContactItem.OI_AddressInfo.ValueChanged -= OrgContactItem_AddressValueChanged;
				orgContactItem.OI_Address_IsManuallyVerifiedInfo.ValueChanged -= OrgContactItem_Address_IsManuallyVerifiedValueChanged;
				orgContactItem.OI_ContactItemTypeInfo.ValueChanged -= OrgContactItem_ContactItemTypeValueChanged;
				orgContactItem.OI_DescriptionInfo.ValueChanged -= OrgContactItem_DescriptionValueChanged;
				orgContactItem.OI_IsPrimaryInfo.ValueChanged -= OrgContactItem_IsPrimaryValueChanged;
				orgContactItem.OI_OCInfo.ValueChanged -= OrgContactItem_OI_OCValueChanged;
			}
		}

		void HookOrgContactItemValueChangedEvents()
		{
			if (orgContactItem != null)
			{
				orgContactItem.OI_AddressInfo.ValueChanged += OrgContactItem_AddressValueChanged;
				orgContactItem.OI_Address_IsManuallyVerifiedInfo.ValueChanged += OrgContactItem_Address_IsManuallyVerifiedValueChanged;
				orgContactItem.OI_ContactItemTypeInfo.ValueChanged += OrgContactItem_ContactItemTypeValueChanged;
				orgContactItem.OI_DescriptionInfo.ValueChanged += OrgContactItem_DescriptionValueChanged;
				orgContactItem.OI_IsPrimaryInfo.ValueChanged += OrgContactItem_IsPrimaryValueChanged;
				orgContactItem.OI_OCInfo.ValueChanged += OrgContactItem_OI_OCValueChanged;
			}
		}

		void OrgContactItem_AddressValueChanged(object sender, EventArgs e)
		{
			OrgContactItem_ValueChanged(OI_AddressInfo, orgContactItem.OI_AddressInfo);
		}

		void OrgContactItem_Address_IsManuallyVerifiedValueChanged(object sender, EventArgs e)
		{
			OrgContactItem_ValueChanged(OI_Address_IsManuallyVerifiedInfo, orgContactItem.OI_Address_IsManuallyVerifiedInfo);
		}

		void OrgContactItem_ContactItemTypeValueChanged(object sender, EventArgs e)
		{
			OrgContactItem_ValueChanged(OI_ContactItemTypeInfo, orgContactItem.OI_ContactItemTypeInfo);
		}

		void OrgContactItem_DescriptionValueChanged(object sender, EventArgs e)
		{
			OrgContactItem_ValueChanged(OI_DescriptionInfo, orgContactItem.OI_DescriptionInfo);
		}

		void OrgContactItem_IsPrimaryValueChanged(object sender, EventArgs e)
		{
			OrgContactItem_ValueChanged(OI_IsPrimaryInfo, orgContactItem.OI_IsPrimaryInfo);
		}

		void OrgContactItem_OI_OCValueChanged(object sender, EventArgs e)
		{
			OrgContactItem_ValueChanged(OI_OCInfo, orgContactItem.OI_OCInfo);
		}

		void OrgContactItem_ValueChanged(ZPropertyInfo proxyPropertyInfo, ZPropertyInfo orgContactItemPropertyInfo)
		{
			if (!isSettingProperty && orgContactItem != null)
			{
				proxyPropertyInfo.Value = orgContactItemPropertyInfo.Value;
			}
		}

		#endregion

		#region Properties

		#region OI_IsPrimary

		public override ZBool OI_IsPrimary
		{
			get { return base.OI_IsPrimary; }
			set
			{
				base.OI_IsPrimary = value;
				if (OrgContactItem != null)
				{
					OrgContactItem.OI_IsPrimary = value;
				}
			}
		}

		#endregion

		#region OI_ContactItemType

		public override ZString OI_ContactItemType
		{
			get
			{
				return base.OI_ContactItemType;
			}
			set
			{
				base.OI_ContactItemType = value;
				if (OrgContactItem != null)
				{
					OrgContactItem.OI_ContactItemType = value;
				}
			}
		}

		#endregion

		#region OI_OC

		public OrgContact Contact
		{
			get { return Factory.Load<OrgContact>(OI_OC); }
		}

		public override ZGuid OI_OC
		{
			get { return base.OI_OC; }
			set
			{
				base.OI_OC = value;

				if (AddressInfoOnOrgContact != null)
				{
					throw new NotSupportedException("Cannot change OI_OC for contact items stored on the OrgContact table");
				}
				else if (OrgContactItem != null)
				{
					OrgContactItem.OI_OC = value;
				}
			}
		}

		#endregion

		#region OI_Address

		public override ZString OI_Address
		{
			get { return base.OI_Address; }
			set
			{
				isSettingProperty = true;
				try
				{
					if (OI_Address != value)
					{
						var contact = Contact;
						var previousAddressInfoOnOrgContact = AddressInfoOnOrgContact;
						var previouslyOnOrgContact = AddressInfoOnOrgContact != null;
						var newAddressInfoOnOrgContact = GetAddressInfoOnOrgContact(contact, OI_Description);
						var shouldBeOnOrgContact = newAddressInfoOnOrgContact != null && contact != null && !value.IsEmpty && (previousAddressInfoOnOrgContact == newAddressInfoOnOrgContact || ParentCollections.All(col => !col.Cast<ContactItemProxy>().Any(item => item.AddressInfoOnOrgContact == newAddressInfoOnOrgContact)));
						var shouldHaveOrgContactItem = !shouldBeOnOrgContact && !value.IsEmpty;

						base.OI_Address = value;

						if (previouslyOnOrgContact && !shouldBeOnOrgContact)
						{
							SetAddressOnOrgContact(previousAddressInfoOnOrgContact, ZString.Empty);
							SetAddressIsManuallyVerifiedOnOrgContact(contact, OI_Description, ZBool.False);
							AddressInfoOnOrgContact = null;
						}

						if (OrgContactItem != null && !shouldHaveOrgContactItem)
						{
							OrgContactItem.Delete();
							OrgContactItem = null;
						}

						if (shouldBeOnOrgContact)
						{
							AddressInfoOnOrgContact = newAddressInfoOnOrgContact;
						}
						else if (shouldHaveOrgContactItem)
						{
							if (OrgContactItem == null)
							{
								OrgContactItem = CreateNewOrgContactItem();
							}
						}

						if (AddressInfoOnOrgContact != null)
						{
							SetAddressOnOrgContact(AddressInfoOnOrgContact, value);
						}
						else if (OrgContactItem != null)
						{
							OrgContactItem.OI_Address = value;
						}
					}
				}
				finally
				{
					isSettingProperty = false;
				}
			}
		}

		protected virtual void SetAddressOnOrgContact(ZPropertyInfo addressInfo, ZString value)
		{
			addressInfo.Value = value;
		}

		protected virtual void SetAddressIsManuallyVerifiedOnOrgContact(OrgContact contact, ZString oI_Description, ZBool value)
		{
		}

		#endregion

		#region OI_Description

		[List("Lookups.SelectableDescriptionList")]
		public override ZString OI_Description
		{
			get { return base.OI_Description; }
			set
			{
				isSettingProperty = true;
				try
				{
					if (OI_Description != value)
					{
						var previousDescription = OI_Description;
						var previousAddressIsManuallyVerified = OI_Address_IsManuallyVerified;
						var contact = Contact;
						var previousAddressInfoOnOrgContact = AddressInfoOnOrgContact;
						var previouslyOnOrgContact = previousAddressInfoOnOrgContact != null;
						var newAddressInfoOnOrgContact = GetAddressInfoOnOrgContact(contact, value);
						var shouldBeOnOrgContact = newAddressInfoOnOrgContact != null && contact != null && !OI_Address.IsEmpty && (previousAddressInfoOnOrgContact == newAddressInfoOnOrgContact || ParentCollections.All(col => !col.Cast<ContactItemProxy>().Any(item => item.AddressInfoOnOrgContact == newAddressInfoOnOrgContact)));
						var shouldHaveOrgContactItem = !shouldBeOnOrgContact && !OI_Address.IsEmpty;

						base.OI_Description = value;

						if (previouslyOnOrgContact && previousAddressInfoOnOrgContact != newAddressInfoOnOrgContact)
						{
							SetAddressOnOrgContact(previousAddressInfoOnOrgContact, ZString.Empty);
							SetAddressIsManuallyVerifiedOnOrgContact(contact, previousDescription, ZBool.False);
						}

						if (!shouldBeOnOrgContact)
						{
							AddressInfoOnOrgContact = null;
						}

						if (OrgContactItem != null && !shouldHaveOrgContactItem)
						{
							OrgContactItem.Delete();
							OrgContactItem = null;
						}

						if (shouldBeOnOrgContact)
						{
							AddressInfoOnOrgContact = newAddressInfoOnOrgContact;
						}
						else if (shouldHaveOrgContactItem)
						{
							if (OrgContactItem == null)
							{
								OrgContactItem = CreateNewOrgContactItem();
							}
						}

						if (AddressInfoOnOrgContact != null)
						{
							SetAddressOnOrgContact(AddressInfoOnOrgContact, OI_Address);
							SetAddressIsManuallyVerifiedOnOrgContact(contact, value, previousAddressIsManuallyVerified);
						}
						else if (OrgContactItem != null)
						{
							OrgContactItem.OI_Description = value;
						}
					}
				}
				finally
				{
					isSettingProperty = false;
				}
			}
		}

		[List("Lookups.InverseDescriptionList")]
		public ZString DisplayDescription
		{
			get
			{
				var result = Lookups.DescriptionList.GetDescriptionFromCode(OI_Description);
				return !string.IsNullOrEmpty(result) ? (ZString)result : OI_Description;
			}
			set
			{
				var description = Lookups.InverseDescriptionList.GetDescriptionFromCode(value);
				OI_Description = !string.IsNullOrEmpty(description) ? (ZString)description : value;

				if (!IsValidationSuspended)
				{
					Validation.ValidateOI_Description();
				}
			}
		}

		public ZWrappedPropertyInfo DisplayDescriptionInfo
		{
			get { return GetWrappedZPropertyInfo(nameof(DisplayDescription), x => OI_DescriptionInfo); }
		}

		#endregion

		bool isSettingProperty;

		#endregion

		#region Save / Delete

		public override void Delete()
		{
			UnhookOrgContactValueChangedEvents();
			UnhookOrgContactItemValueChangedEvents();
			if (AddressInfoOnOrgContact != null)
			{
				SetAddressOnOrgContact(AddressInfoOnOrgContact, ZString.Empty);
			}
			if (OrgContactItem != null)
			{
				OrgContactItem.Delete();
			}

			base.Delete();
		}

		#endregion

		#region IReadOnlySecurity Members

		protected bool GetReadOnlySecurity(PropertyDescriptor property)
		{
			return !HasModifyContactItemSecurity || MetaData.GetReadOnlyExcludingMethodProvider(this, property);
		}

		bool HasModifyContactItemSecurity
		{
			get
			{
				var contact = Contact;
				if (contact != null)
				{
					var header = contact.Header;
					if (header != null)
					{
						return header.SecurityProvider.HasModifyContactContactDetailsSecurity;
					}
				}

				return Env.Security.OrgContactModifyContactDetails.IsAllowed;
			}
		}

		#endregion

		#region Lookups

		public ContactItemProxyLookups Lookups
		{
			get
			{
				if (fLookups == null || !IsLookupsCachedInBase)
				{
					fLookups = GetNewLookups();
				}

				return fLookups;
			}
		}

		protected virtual ContactItemProxyLookups GetNewLookups()
		{
			return new ContactItemProxyLookups(this);
		}

		ContactItemProxyLookups fLookups;

		#endregion
	}
}
