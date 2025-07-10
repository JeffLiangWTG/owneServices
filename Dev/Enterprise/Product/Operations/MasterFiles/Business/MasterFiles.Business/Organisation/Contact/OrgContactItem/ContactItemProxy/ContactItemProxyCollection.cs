using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public abstract class ContactItemProxyCollection<T> : NonPersistentBusinessObjectCollection<T>
		where T : ContactItemProxy
	{
		protected ContactItemProxyCollection(OrgContact contact, string type)
			: base(contact.Factory)
		{
			Contact = contact;
			ContactItemType = type;
		}

		protected readonly OrgContact Contact;
		protected readonly ZString ContactItemType;

		#region Add

		public T AddNew(ZString description)
		{
			T result = AddNew();
			using (result.SuspendSettingHasChanges())
			using (result.GetValidationSuspender())
			{
				result.OI_Description = description;
			}

			return result;
		}

		protected abstract T GetNewContactItemProxy(OrgContactItem orgContactItem);

		#endregion

		#region GetItemsWithDescription

		public IEnumerable<T> GetItemsWithDescription(ZString description)
		{
			return Items.Where(item => item.OI_Description.EqualsIgnoringCase(description));
		}

		#endregion

		#region GetMostImportantItem

		public T GetMostImportantItem(ZString description)
		{
			var itemsWithDescription = GetItemsWithDescription(description);
			return
				(from item in itemsWithDescription
				 where !item.OI_Address.IsEmpty
				 orderby item.OI_Address
				 select item).FirstOrDefault();
		}

		#endregion

		public void Refresh()
		{
			using (SuspendListChanged())
			using (SuspendSettingHasChanges())
			{
				RemoveAndDeleteAll();

				var orgContactItemsQuery = new ZQuery(OrgContactItemSchema.OI_ContactItemType, ContactItemType);
				orgContactItemsQuery.AddToFilter(OrgContactItemSchema.OI_OC, Contact.PK);
				var orgContactItems = Factory.Load<OrgContactItem>(orgContactItemsQuery);
				foreach (var orgContactItem in orgContactItems)
				{
					Add(GetNewContactItemProxy(orgContactItem));
				}

				AddItemsForOrgContactColumns();
			}
		}

		#region AddItemsForOrgContactColumns

		void AddItemsForOrgContactColumns()
		{
			if (!Contact.IsDeleted)
			{
				using (Contact.SuspendSettingHasChanges())
				using (Contact.GetValidationSuspender())
				{
					AddItemsForOrgContactColumnsCore();
				}
			}
		}

		protected virtual void AddItemsForOrgContactColumnsCore()
		{
		}

		#endregion

		#region IEnumerable Members

		public IEnumerable<T> Items
		{
			get { return this.Cast<T>(); }
		}

		#endregion
	}
}
