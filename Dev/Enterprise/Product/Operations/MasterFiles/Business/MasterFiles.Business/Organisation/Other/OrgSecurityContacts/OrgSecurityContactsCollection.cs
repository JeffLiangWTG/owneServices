using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public class OrgSecurityContactsCollection : DependentBusinessObjectCollection<OrgSecurityContacts, BusinessObject>
	{
		public OrgSecurityContactsCollection(OrgSecurity securityRight)
			: this((BusinessObject)securityRight)
		{
			ParentSecurityRight = securityRight;
		}

		public OrgSecurityContactsCollection(OrgContact contact)
			: this((BusinessObject)contact)
		{
			ParentContact = contact;
		}

		protected OrgSecurityContactsCollection(BusinessObject bizo)
			: base(bizo)
		{
			Factory.Saving += Factory_Saving;
		}

		#region Parents

		public readonly OrgSecurity ParentSecurityRight;

		public readonly OrgContact ParentContact;

		#endregion

		#region Inheriting Parent Rights

		public void InheritParentRights()
		{
			foreach (OrgSecurityContacts securityContact in this)
			{
				securityContact.OZ_Granted = securityContact.Security.OX_Granted;
			}
		}

		public void DenyAll()
		{
			foreach (OrgSecurityContacts securityContact in this)
			{
				securityContact.OZ_Granted = false;
			}
		}

		#endregion

		#region Granted Rights

		public bool IsRightGranted(WebSecurityRight securityRight)
		{
			bool result = false;

			if (ParentContact != null && ParentContact.OC_WebAccessEnabled)
			{
				foreach (OrgSecurityContacts securityContact in this)
				{
					if (securityContact.Security.SecurityKey == securityRight.Code)
					{
						result = securityContact.OZ_Granted;
						break;
					}
				}
			}

			return result;
		}

		#endregion

		#region Rights Management

		public bool DontLoadDummyContactRecords { get; set; }

		public override void Load()
		{
			base.Load();

			if (ParentSecurityRight != null)
			{
				LoadAllSecuritiesOnParentSecurityRight();
			}
			else if (ParentContact != null)
			{
				LoadAllSecuritiesOnParentContact();
			}
		}

		public void Refresh()
		{
			Load();
			FireListResetEvent();
		}

		void LoadAllSecuritiesOnParentSecurityRight()
		{
			var header = ParentSecurityRight.Organisation;
			if (header != null)
			{
				if (ParentSecurityRight.ContactSecurityRightsNoCreate == null)
				{
					var temporaryCollection = new OrgSecurityContactsCollection(ParentSecurityRight);
					Factory.AddFetchHint(OrgSecurityContactsSchema.Instance, temporaryCollection.GetCompleteLoadFilter(temporaryCollection.AdditionalFilter));
				}

				var existingSecurities =
						this.Cast<OrgSecurityContacts>()
						.Select(securityContact => securityContact.Contact)
						.Where(contact => contact != null && !contact.IsDeleted)
						.Select(contact => contact.OC_ContactName)
						.Distinct()
						.ToDictionary(contactName => contactName);

				if (!DontLoadDummyContactRecords)
				{
					foreach (OrgContact contact in header.Contacts)
					{
						AddIfSecurityRightDoesNotExist(contact, ParentSecurityRight, existingSecurities, contact.OC_ContactName);
					}
				}
			}
		}

		void LoadAllSecuritiesOnParentContact()
		{
			var header = ParentContact.Header;
			if (header != null)
			{
				if (ParentContact.SecurityRightsNoCreate == null)
				{
					var temporaryCollection = new OrgSecurityContactsCollection(ParentContact);
					Factory.AddFetchHint(OrgSecurityContactsSchema.Instance, temporaryCollection.GetCompleteLoadFilter(temporaryCollection.AdditionalFilter));
				}

				header.SecurityRights.Load();

				var existingSecurities =
						this.Cast<OrgSecurityContacts>()
						.Select(securityContact => securityContact.Security)
						.Where(security => security != null)
						.Select(security => security.SecurityKey)
						.Distinct()
						.ToDictionary(securityKey => securityKey);

				if (!DontLoadDummyContactRecords)
				{
					foreach (OrgSecurity securityRight in header.SecurityRights)
					{
						AddIfSecurityRightDoesNotExist(ParentContact, securityRight, existingSecurities, securityRight.SecurityKey);
					}
				}
			}
		}

		void AddIfSecurityRightDoesNotExist(OrgContact contact, OrgSecurity security, Dictionary<ZString, ZString> existingSecurities, ZString key)
		{
			if (!existingSecurities.ContainsKey(key))
			{
				OrgSecurityContacts dummySecurity = Factory.New<OrgSecurityContacts>();
				using (dummySecurity.SuspendSettingHasChanges())
				using (dummySecurity.GetValidationSuspender())
				{
					dummySecurity.OZ_OC = contact.PK;
					dummySecurity.OZ_OX = security.PK;
					dummySecurity.OZ_Granted = security.OX_Granted && contact.OC_WebAccessEnabled;
				}
				Add(dummySecurity);
				existingSecurities.Add(key, key);
			}
		}

		void Factory_Saving(BusinessObjectFactory factory)
		{
			var securityReplacementList = new List<OrgSecurityContacts>(1);

			for (int i = Count - 1; i >= 0; i--)
			{
				OrgSecurityContacts securityContacts = this[i];
				if (securityContacts.IsInDatabase && !securityContacts.IsDeleted && !securityContacts.HasDifferentSecurityToParent)
				{
					OrgSecurityContacts securityReplacement = GetReplacementForDeletedSecurityRight(securityContacts);
					securityReplacementList.Add(securityReplacement);

					securityContacts.Delete();
				}
			}

			foreach (var securityReplacement in securityReplacementList)
			{
				Add(securityReplacement);
			}
		}

		OrgSecurityContacts GetReplacementForDeletedSecurityRight(OrgSecurityContacts securityToDelete)
		{
			var newContactSecurity = Factory.New<OrgSecurityContacts>();
			using (newContactSecurity.GetValidationSuspender())
			{
				using (newContactSecurity.SuspendSettingHasChanges())
				{
					newContactSecurity.OZ_OC = securityToDelete.OZ_OC;
					newContactSecurity.OZ_OX = securityToDelete.OZ_OX;
					newContactSecurity.OZ_Granted = securityToDelete.OZ_Granted;
				}

				foreach (var parentCollection in ((IBusinessObjectInternals)securityToDelete).ParentCollections)
				{
					if (parentCollection is OrgSecurityContactsCollection)
					{
						parentCollection.Add(newContactSecurity);
					}
				}
			}

			return newContactSecurity;
		}

		#endregion

		#region Implementation

		protected override bool AllowNewCore
		{
			get { return false; }
		}

		#endregion
	}
}
