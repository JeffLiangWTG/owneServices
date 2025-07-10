using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.ComponentModel;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.MasterFiles.Business
{
	[ModuleID(ModuleId.OrgContacts)]
	public class OrgContactDependentCollection : DependentBusinessObjectCollection<OrgContact, BusinessObject>
	{
		public OrgContactDependentCollection(OrgHeader parent, ZQuery filter) : base(parent, filter)
		{
			this.ParentOrg = parent;
			AddFilterBusinessObjectDefaults(parent);
		}

		public OrgContactDependentCollection(OrgHeader parent, BusinessObjectFactory factory, bool allowInDifferentFactories) : base(parent, factory, allowInDifferentFactories)
		{
			this.ParentOrg = parent;
			AddFilterBusinessObjectDefaults(parent);
		}

		public OrgContactDependentCollection(OrgHeader parent, BusinessObjectFactory factory) : this(parent, factory, false)
		{
		}

		public OrgContactDependentCollection(OrgHeader parent) : this(parent, parent.Factory)
		{
		}

		public OrgContactDependentCollection(BusinessObjectFactory factory) : base(factory)
		{
		}

		readonly OrgHeader ParentOrg;

		#region Format All Contacts' Phone Numbers

		public void FormatAllContactPhoneNumbers()
		{
			foreach (var contactItem in this)
			{
				foreach (var phoneItem in (contactItem as OrgContact).PhoneContactItems.Items)
				{
					phoneItem.FormatPhoneNumber();
				}
			}
		}

		#endregion

		#region Get Contact for a Specific Allocation

		public OrgContact GetContactForAllocation(string type)
		{
			foreach (OrgContact contact in this)
			{
				if (contact.Allocations.OfType<OrgContactAllocation>().FirstOrDefault(x => x.PC_Type == type) != null)
				{
					return contact;
				}
			}
			return null;
		}

		#endregion

		#region Implementation

		protected override bool AllowNewCore
		{
			get { return ParentOrg == null || (ParentOrg != null && ParentOrg.SecurityProvider.HasModifyContactContactDetailsSecurity); }
		}

		protected override bool AllowRemoveCore
		{
			get { return ParentOrg == null || (ParentOrg != null && ParentOrg.SecurityProvider.HasModifyContactContactDetailsSecurity); }
		}

		public ZBool SetDocGroupAsDefault(ZString docGroup)
		{
			int count = 0;
			foreach (OrgContact contact in this)
			{
				if (contact.Documents.ContainsDefault(docGroup))
				{
					count++;
				}
			}
			return count < 1;
		}

		protected override void SetDefaultsForNewChild(BusinessObject child)
		{
			base.SetDefaultsForNewChild(child);

			OrgContact newContact = (OrgContact)child;
			newContact.OC_OA_OrgAddress_ZAddress.OrgPK = Master.PK;

			if (ParentOrg != null)
			{
				ParentOrg.RefreshSecurityRights();
			}
		}

		void AddFilterBusinessObjectDefaults(OrgHeader org)
		{
			if (org != null)
			{
				FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault("Organisation", "Property", org.PK));
			}
		}

		#endregion

		protected override IFindBoxListProvider FindBoxListProvider
			=> new ContactNameFindListBoxProvider(this);
	}

	class ContactNameFindListBoxProvider : FindBoxListProvider
	{
		public ContactNameFindListBoxProvider(IBusinessObjectCollection collection)
			: base(collection)
		{
		}

		protected override string GetCodePropertyName(string code) =>
			OrgContact.Schema.OC_ContactName;
	}
}
