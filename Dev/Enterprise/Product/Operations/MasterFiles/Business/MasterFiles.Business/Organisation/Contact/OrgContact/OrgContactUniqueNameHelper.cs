using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public static class OrgContactUniqueNameHelper
	{
		public static ZString GenerateUniqueContactName(Dictionary<string, ZGuid> existingNames, string proposedName, OrgContact excludingContact)
		{
			string result = proposedName;

			var excludingContactPk = excludingContact?.PK ?? ZGuid.Empty;
			bool hasDuplicateName = (existingNames.TryGetValue(proposedName, out ZGuid foundContactPK) && foundContactPK != excludingContactPk);

			if (hasDuplicateName)
			{
				int counter = 1;
				do
				{
					result = GenerateUniqueContactNameCore(proposedName, counter);
					hasDuplicateName = (existingNames.TryGetValue(result, out foundContactPK) && foundContactPK != excludingContactPk);
					counter++;
				}
				while (hasDuplicateName);
			}

			return result;
		}

		public static ZString GenerateUniqueContactName(OrgContact contact, string proposedName)
		{
			var result = proposedName;

			var org = contact.Header;
			var foundContacts = FindByContactName(org, null, result);
			if (foundContacts.Any(x => x.PK != contact.PK))
			{
				int counter = 1;
				do
				{
					result = GenerateUniqueContactNameCore(proposedName, counter);
					foundContacts = FindByContactName(org, null, result);
					counter++;
				}
				while (foundContacts.Any(x => x.PK != contact.PK));
			}

			return result;
		}

		public static ZString GenerateUniqueContactName(OrgHeader organisation, OrgContactDependentCollection contacts, string proposedName)
		{
			string result = proposedName;

			var foundContacts = FindByContactName(organisation, contacts, result);
			if (foundContacts.Length > 0)
			{
				int counter = 1;
				do
				{
					result = GenerateUniqueContactNameCore(proposedName, counter);
					foundContacts = FindByContactName(organisation, contacts, result);
					counter++;
				}
				while (foundContacts.Length > 0);
			}

			return result;
		}

		public static ZString GetNameWithoutNumberSuffix(string name)
		{
			return Regex.Replace(name, @"\([0-9]{1,3}\)", "").Trim();
		}

		static OrgContact[] FindByContactName(OrgHeader organisation, OrgContactDependentCollection contacts, string contactName)
		{
			var query = new ZQuery(OrgContactSchema.OC_ContactName, contactName);
			if (contacts != null)
			{
				return (OrgContact[])contacts.Find(query);
			}
			else
			{
				query.AddToFilter(OrgContactSchema.OC_OH, organisation.PK);
				return organisation.Factory.Load<OrgContact>(query);
			}
		}

		static string GenerateUniqueContactNameCore(string proposedName, int counter)
		{
			string result;
			var suffix = FormattableString.Invariant($" ({counter})");
			if (proposedName.Length + suffix.Length > OrgContactSchema.OC_ContactName.MaxLength)
			{
				result = proposedName.Substring(0, OrgContactSchema.OC_ContactName.MaxLength - suffix.Length) + suffix;
			}
			else
			{
				result = proposedName + suffix;
			}

			return result;
		}
	}
}
