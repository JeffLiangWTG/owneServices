using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.GUI
{
	public static class OrgContactSupersedeHelper
	{
		public static void SupersedeContacts(OrgContact[] contacts)
		{
			if (contacts.Length <= 0 || !Env.Security.OrgContactModify.IsAllowed)
			{
				return;
			}

			if (RequireARContact(contacts.First().ParentOrg, contacts))
			{
				return;
			}

			var question = Res.GetString("3b36ea08-5073-4e28-890d-d430e6aa7a20", "You are about to deactivate {0} contact(s) and supersede their Web Access. Do you want to continue?",
				contacts.Length.ToString(CultureInfo.CurrentCulture));
			var caption = Res.GetString("cfb8edd3-7673-4e4e-b0bf-d720d7768297", "Deactivate and Supersede Web Access");

			if (Globals.Message.Show(question, caption, MessageBoxButtons.YesNo, DialogResult.No) != DialogResult.Yes)
			{
				return;
			}

			var inactiveContacts = new List<OrgContact>();
			var supersedeCandidates = new List<OrgContact>();

			foreach (var contact in contacts)
			{
				if (!contact.OC_IsActive)
				{
					inactiveContacts.Add(contact);
					continue;
				}

				supersedeCandidates.Add(contact);
			}

			if (inactiveContacts.Any())
			{
				var skipContactsMessage = Res.GetString("8a72a06d-5f18-4b44-9e39-01d4499b1a9a", "The following contacts will be skipped because they are already inactive:\r\n{0}",
					string.Join("\r\n", inactiveContacts.Select(x => x.OC_ContactName)));
				var skipCaption = Res.GetString("0dd41c41-01a0-4efe-b363-ae644fb39641", "Contacts already inactive");
				if (Globals.Message.Show(skipContactsMessage, skipCaption, MessageBoxButtons.OKCancel, DialogResult.Cancel) != DialogResult.OK)
				{
					return;
				}
			}

			var supersedeCandidatePKs = supersedeCandidates.Select(x => x.PK).ToArray();
			var contactsWithOtherWebAccessContact = new ZDBOnlyQuery(typeof(OrgContact));
			contactsWithOtherWebAccessContact.AddToFilter(OrgContactSchema.PK, supersedeCandidatePKs);
			var personSubQuery = new ZDBOnlySubQuery(typeof(GlbPerson), GlbPersonSchema.PK);
			var otherContactSubQuery = new ZDBOnlySubQuery(typeof(OrgContact), OrgContactSchema.OC_PER);
			otherContactSubQuery.AddToFilter(OrgContactSchema.PK, SQLComparisonOperator.NotEqual, supersedeCandidatePKs);
			otherContactSubQuery.AddToFilter(OrgContactSchema.OC_IsActive, true);
			otherContactSubQuery.AddToFilter(OrgContactSchema.OC_WebAccessEnabled, true);
			personSubQuery.AddSubQuery(GlbPersonSchema.PK, otherContactSubQuery, JoinCondition.And);
			contactsWithOtherWebAccessContact.AddSubQuery(OrgContactSchema.OC_PER, personSubQuery, JoinCondition.And);

			var factory = contacts[0].Factory;
			var contactsToSupersede = factory.Load<OrgContact>(contactsWithOtherWebAccessContact);

			var contactsToDeactivate = supersedeCandidates.Except(contactsToSupersede).ToArray();
			if (contactsToDeactivate.Any())
			{
				var deactivateQuestion = Res.GetString("8e4360e7-9fc5-4789-8528-8bbcf3810ecb", "The following contacts will be deactivated (without superseding their web access) because there are no other active web access contacts for the Person:\r\n{0}",
					string.Join("\r\n", contactsToDeactivate.Select(x => x.OC_ContactName)));
				var deactivateCaption = Res.GetString("032e5528-6fa0-4c2d-8783-5dd9cfe208f6", "Deactivate");
				if (Globals.Message.Show(deactivateQuestion, deactivateCaption, MessageBoxButtons.OKCancel, DialogResult.Cancel) != DialogResult.OK)
				{
					return;
				}

				Array.ForEach(contactsToDeactivate, x => { x.OC_IsActive = false; });
			}

			var failedSupersedeContacts = new List<OrgContact>();
			foreach (var contact in contactsToSupersede)
			{
				if (!contact.SupersedeWebAccess())
				{
					failedSupersedeContacts.Add(contact);
				}
			}

			try
			{
				factory.Save();
			}
			catch (ZSaveException ex)
			{
				ZExceptionReporting.HandleSaveException(ex);
				Globals.Message.Show(Res.GetString("85a4ca5a-ae41-432f-8353-fb467c3d67df", "Could not save changes. Error: ") + ex.Message);
				return;
			}

			if (failedSupersedeContacts.Any())
			{
				var skippedNotificationMessage = failedSupersedeContacts[0].CannotSupersedeReason + "\r\n" + string.Join("\r\n", failedSupersedeContacts.Select(x => x.OC_ContactName));
				var skippedNotificationCaption = Res.GetString("9d2d5e72-c348-42ce-a94e-c390cbaa06d0", "Skipped contacts");
				Globals.Message.Show(skippedNotificationMessage, skippedNotificationCaption, MessageBoxButtons.OK, DialogResult.OK);
			}
		}

		internal static bool RequireARContact(OrgHeader header, OrgContact[] contacts)
		{
			var requireContact = false;
			if (header != null && !header.IsDeleted && header.OH_IsDebtor && (header.OH_IsTempAccount ? Env.Registry.GetTempOrgDebtorRequiredFields().RequireARContact : Env.Registry.GetOrgDebtorRequiredFields().RequireARContact))
			{
				if (contacts != null)
				{
					requireContact = contacts.Where(c => c.OC_IsActive && c.Documents.Cast<OrgDocument>().Any(d => d.OD_DocumentGroup == ContactType.Receivables.Code)).ContainsSameElementsInAnyOrder(GetAllActiveARContacts(header));
				}
				else
				{
					requireContact = !GetAllActiveARContacts(header).Any();
				}
			}

			if (requireContact)
			{
				Globals.Message.ShowError(Res.GetString("70354E64-5051-4311-A352-35F10CA2A9D2", "There should always be at least one active A/R Contact on Debtor Organizations."));
			}

			return requireContact;
		}

		static IEnumerable<OrgContact> GetAllActiveARContacts(OrgHeader header) => header.GetActiveContacts().Cast<OrgContact>().Where(c => c.Documents.Cast<OrgDocument>().Any(d => d.OD_DocumentGroup == ContactType.Receivables.Code));
	}
}
