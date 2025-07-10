using System;
using System.Linq;
using System.Text.RegularExpressions;
using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.ComponentModel;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.MasterFiles.Business
{
	public class FilteredContactsCollection : BusinessObjectCollectionView<OrgContact>
	{
		public FilteredContactsCollection(OrgContactDependentCollection collectionToFilter) : base(collectionToFilter)
		{
			Rebuild();
		}

		public FilteredContactsCollection(OrgContactCollection collectionToFilter) : base(collectionToFilter)
		{
			Rebuild();
		}

		protected override bool AllowNewCore
		{
			get
			{
				return base.AllowNewCore && Env.Security.OrgContactNew.IsAllowed;
			}
		}

		protected override bool IsThisPartOfTheCollection(BusinessObject element)
		{
			var contact = (OrgContact)element;
			var includeByActiveFlag = IncludeInactiveContacts || contact.OC_IsActive || (!contact.OC_IsActive && (contact.HasErrors || contact.HasChanges));
			var includedByWebAccessEnabledFlag = !OnlyShowWebAccessEnabledContacts || contact.OC_WebAccessEnabled;

			return includeByActiveFlag && includedByWebAccessEnabledFlag && IncludeByFilterString(contact);
		}

		bool IncludeByFilterString(OrgContact contact)
		{
			var result = false;
			if (FilterOption == OrgHeaderLookups.LookupConstants.Contacts)
			{
				result = string.IsNullOrWhiteSpace(filterString) ||
				contact.OC_ContactName.Contains(filterString, StringComparison.CurrentCultureIgnoreCase) ||
				contact.OC_Title.Contains(filterString, StringComparison.CurrentCultureIgnoreCase) ||
				contact.OC_Email.Contains(filterString, StringComparison.CurrentCultureIgnoreCase) ||
				contact.OC_Phone.Contains(filterString, StringComparison.CurrentCultureIgnoreCase) ||
				contact.OC_Mobile.Contains(filterString, StringComparison.CurrentCultureIgnoreCase) ||
				contact.OC_HomePhone.Contains(filterString, StringComparison.CurrentCultureIgnoreCase);

				if (!result && !string.IsNullOrEmpty(unFormattedFilterString))
				{
					result = contact.OC_Phone.Contains(unFormattedFilterString, StringComparison.CurrentCultureIgnoreCase) ||
							 contact.OC_Mobile.Contains(unFormattedFilterString, StringComparison.CurrentCultureIgnoreCase) ||
							 contact.OC_HomePhone.Contains(unFormattedFilterString, StringComparison.CurrentCultureIgnoreCase);
				}
			}
			else if (FilterOption == OrgHeaderLookups.LookupConstants.AllocatedContact)
			{
				result = string.IsNullOrWhiteSpace(filterString) ||
				contact.Allocations.OfType<OrgContactAllocation>().FirstOrDefault(x => x.PC_Type.Contains(filterString, StringComparison.CurrentCultureIgnoreCase)) != null ||
				contact.Allocations.OfType<OrgContactAllocation>().FirstOrDefault(x => x.AllocationDescription.Contains(filterString, StringComparison.CurrentCultureIgnoreCase)) != null;
			}
			return result;
		}

		#region IncludeInactiveContacts

		public bool IncludeInactiveContacts
		{
			get { return includeInactiveContacts; }
			set
			{
				if (value != includeInactiveContacts)
				{
					includeInactiveContacts = value;
					Rebuild();
				}
			}
		}

		bool includeInactiveContacts;

		#endregion

		#region Web Access Enabled

		public bool OnlyShowWebAccessEnabledContacts
		{
			get => onlyShowWebAccessEnabledContacts;
			set
			{
				if (value != onlyShowWebAccessEnabledContacts)
				{
					onlyShowWebAccessEnabledContacts = value;
					Rebuild();
				}
			}
		}

		bool onlyShowWebAccessEnabledContacts;

		#endregion

		#region FilterString

		public string FilterString
		{
			get { return filterString; }
			set
			{
				if (value != filterString)
				{
					filterString = value;
					SetUnFormattedFilterString();
					Rebuild();
					ReSort();
				}
			}
		}

		string filterString;
		internal string unFormattedFilterString;

		void SetUnFormattedFilterString()
		{
			unFormattedFilterString = string.Empty;
			if (!string.IsNullOrEmpty(filterString))
			{
				unFormattedFilterString = Regex.Replace(filterString, "[^a-zA-Z0-9]", string.Empty);
				if (filterString.Substring(0, 1) == plus_sign)
				{
					unFormattedFilterString = plus_sign + unFormattedFilterString;
				}
			}
		}
		const string plus_sign = "+";

		#endregion

		#region FilterOption

		public string FilterOption
		{
			get => filterOption ?? OrgHeaderLookups.LookupConstants.Contacts;
			set
			{
				if (value != filterOption)
				{
					filterOption = value;
					Rebuild();
				}
			}
		}
		string filterOption;

		#endregion
	}

	public class FilteredContactsCollectionWrapper : NonPersistentBusinessObject
	{
		public FilteredContactsCollection Collection => ParentCollection;

		readonly ContactsCollectionWithModuleID ParentCollection;

		public FilteredContactsCollectionWrapper(OrgContactCollection collection, Type gridContextParentType = null)
		{
			ParentCollection = new ContactsCollectionWithModuleID(collection, gridContextParentType);
		}

		public bool IncludeInactiveContacts
		{
			get { return ParentCollection.IncludeInactiveContacts; }
			set { ParentCollection.IncludeInactiveContacts = value; }
		}

		[ModuleID(ModuleId.OrgContacts)]
		internal class ContactsCollectionWithModuleID : FilteredContactsCollection, IUseParentGridContext
		{
			public ContactsCollectionWithModuleID(OrgContactCollection collectionToFilter, Type gridContextParentType) : base(collectionToFilter)
			{
				GridContextParentType = gridContextParentType;
				Rebuild();
			}

			readonly Type GridContextParentType;
			Type IUseParentGridContext.ParentType => GridContextParentType;
		}
	}
}
