using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.ComponentModel;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	[ModuleID(ModuleId.OrgContacts)]
	public class OrgContactCollection : BusinessObjectCollection<OrgContact>, IOrgContactCollection
	{
		public OrgContactCollection(BusinessObjectFactory factory) : base(factory)
		{
		}

		public OrgContactCollection(BusinessObjectFactory factory, ZQuery filter) : base(factory, filter)
		{
		}

		/// <summary>
		/// For all Contacts with email addresses, removes any duplicate contacts based on email address.
		/// Ie if 2 contacts exist with the same email, the 1st contact is removed from this collection and the last remains.
		/// </summary>
		public void RemoveDuplicatesBasedOnEmailAddress()
		{
			var emails = new HashSet<string>();
			for (int i = Count - 1; i >= 0; i--)
			{
				var contact = this[i];
				if (!contact.OC_Email.IsEmpty)
				{
					if (!emails.Add(contact.OC_Email))
					{
						Remove(contact);
					}
				}
			}
		}

		public bool UseOrgCodeFilter { get; set; }

		protected override IFindBoxListProvider FindBoxListProvider
		{
			get
			{
				if (UseOrgCodeFilter)
				{
					return findBoxListProvider ?? (findBoxListProvider = new OrgContactCodeListProvider(this));
				}
				return base.FindBoxListProvider;
			}
		}
		IFindBoxListProvider findBoxListProvider;
	}

	class OrgContactCodeListProvider : FindBoxListProvider
	{
		public OrgContactCodeListProvider(IBusinessObjectCollection collection) : base(collection)
		{
		}

		protected override void AddCodeEqualsFilter(ZQuery query, string code)
		{
			query.AddToFilter(GetQueryWithOrg(code));
		}

		ZQuery GetQueryWithOrg(string code)
		{
			var result = Regex.Match(code, @"^(.*?)(?: \((\w+)\))?$");
			if (result.Groups[2].Success)
			{
				var parent = List.Factory.LoadFromNaturalKey<OrgHeader>(OrgHeaderSchema.OH_Code, result.Groups[2].Value);
				if (parent != null)
				{
					return new ZQuery(OrgContactSchema.OC_OH, parent.PK)
						.AddToFilter(OrgContactSchema.OC_ContactName, result.Groups[1].Value);
				}
			}

			return new ZQuery(OrgContactSchema.OC_ContactName, code);
		}

		protected override string GetCodePropertyName(Type t)
			=> nameof(OrgContact.UniqueCode);
	}
}
