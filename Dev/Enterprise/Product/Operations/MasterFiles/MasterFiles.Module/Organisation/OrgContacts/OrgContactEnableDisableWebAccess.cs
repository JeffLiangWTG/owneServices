using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.GUI;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Module
{
	public class OrgContactEnableDisableWebAccess
	{
		public OrgContactEnableDisableWebAccess()
		{
		}

		protected BusinessObjectFactory Factory { get; set; }

		ContactsLists GenerateContactsLists(BusinessObject[] selectedObjects, bool enable)
		{
			var contactsToChange = new List<OrgContact>();
			var emailHasErrorContacts = new List<OrgContact>();
			var constraintErrorContacts = new List<OrgContact>();

			foreach (OrgContact contact in selectedObjects.Cast<OrgContact>())
			{
				if (contact.OC_WebAccessEnabled == enable)
				{
					continue;
				}
				if (enable)
				{
					if (!contact.CheckContactHasValidEmail())
					{
						emailHasErrorContacts.Add(contact);
						continue;
					}
					if (OC_EmailConstraintViolated(contact))
					{
						constraintErrorContacts.Add(contact);
						continue;
					}
				}
				contactsToChange.Add(contact);
			}
			return new ContactsLists(contactsToChange, emailHasErrorContacts, constraintErrorContacts);
		}

		bool OC_EmailConstraintViolated(OrgContact orgContact)
		{
			var constraintQuery = new ZDBOnlyQuery(typeof(OrgContact));
			var emailQuery = new ZQuery(OrgContactSchema.OC_Email, SQLComparisonOperator.Equal, orgContact.OC_Email);
			var webAccessQuery = new ZQuery(OrgContactSchema.OC_WebAccessEnabled, SQLComparisonOperator.Equal, true);
			constraintQuery.AddToFilter(emailQuery, JoinCondition.And);
			constraintQuery.AddToFilter(webAccessQuery, JoinCondition.And);
			constraintQuery.AddToFilter(OrgContactSchema.PK, SQLComparisonOperator.NotEqual, orgContact.PK);
			if (Factory.ExistsInDatabase(OrgContactSchema.Constants.TableName, constraintQuery))
			{
				return true;
			}
			return false;
		}

		public void EnableDisableWebAccess(bool enable, BusinessObject[] selectedObjects, ISecurityCheckpoint checkpoint)
		{
			Argument.NotNull(checkpoint, "checkpoint");
			Argument.NotNull<Array>(selectedObjects, "selectedObjects");
			var errors = string.Empty;

			if (selectedObjects.Length > 0)
			{
				Factory = selectedObjects[0].Factory;
				if (checkpoint.IsAllowed)
				{
					var question = Res.GetString("ed8d535e-405d-48e7-8080-1c2de152c491", "You are about to {0} Web Access for {1} contacts.",
						(enable ? Res.GetString("fd472e94-89e3-4223-90c1-c9673bb8ee95", "enable") : Res.GetString("28d849f4-749f-4879-8b3f-0154dd94c19d", "disable")),
						selectedObjects.Length.ToString(CultureInfo.CurrentCulture));
					var caption = Res.GetString("bb3ceeb5-001f-458f-8ab0-4366fcb1cafe", "Enable / Disable Web Access");
					var confirmation = ConfirmationMessage;

					if (Globals.Message.ShowConfirmation(question, caption, confirmation, MessageBoxIcon.Question, MessageBoxButtons.OKCancel) == DialogResult.OK)
					{
						var contactsLists = GenerateContactsLists(selectedObjects, enable);

						if (enable)
						{
							var inactiveUsers = contactsLists.ContactsToChange.Where(c => !c.OC_IsActive).ToList();
							if (inactiveUsers.Any())
							{
								var userConfirmation = Globals.Message.Show(
									Res.GetString("3ea9fc6d-ce6a-41ad-b9d2-dc21dcf1f700", "There are some contacts marked as inactive. Would you like to continue to activate and enable web access?"),
									Res.GetString("bb3ceeb5-001f-458f-8ab0-4366fcb1cafe", "Enable / Disable Web Access"),
									MessageBoxButtons.YesNo, MessageBoxIcon.Question);

								if (userConfirmation == DialogResult.Yes)
								{
									inactiveUsers.ForEach((contact) =>
									{
										contact.OC_IsActive = true;
									});
									errors = SaveIfApplicable();

									if (!errors.IsNullOrEmpty())
									{
										DisplayMsgIfNotEmpty(errors);
										return;
									}
								}
								else
								{
									return;
								}
							}

							if (GetSendEmailConfirmation())
							{
								EnableDisableCore(enable, contactsLists);
								errors = SaveIfApplicable();
							}
						}
						else
						{
							EnableDisableCore(enable, contactsLists);
							errors = SaveIfApplicable();
						}
					}
				}
				else
				{
					errors = Res.GetString("a2a38b24-89d5-4533-94b2-17850e3c4ef4", "You are not allowed to Enable/Disable Web Access in this module. Please contact your system administrator.");
				}
			}
			else
			{
				errors = Res.GetString("77597159-eef7-4424-8cdc-5f7d1F437c93", "Please select at least one contact");
			}

			DisplayMsgIfNotEmpty(errors);
		}

		bool GetSendEmailConfirmation()
		{
			var question = Res.GetString("07ef6e27-4d0f-4161-a356-90884ef14fcf", "You are about to send emails containing instructions to set/reset passwords to the selected contacts.");
			var caption = Res.GetString("887d990c-8cae-41fe-a066-96f8aff5a186", "Send Email Set/Reset Password");
			var confirmation = ConfirmationMessage;
			if (Globals.Message.ShowConfirmation(question, caption, confirmation, MessageBoxIcon.Question, MessageBoxButtons.OKCancel) == DialogResult.OK)
			{
				return true;
			}
			return false;
		}

		string ConfirmationMessage => Res.GetString("ac44ab69-5b52-4668-931f-427e2b4bc449", "Yes");

		string SaveIfApplicable()
		{
			var result = string.Empty;
			try
			{
				Factory.Save();
			}
			catch (ZSaveException ex)
			{
				result = Res.GetString("026ae15a-5f52-40c1-89f2-4cdf64930365", "Could not save changes. Error: ") + ex.Message;
				ZExceptionReporting.HandleSaveException(ex);
			}
			return result;
		}

		void DisplayMsgIfNotEmpty(string msgToDisplay)
		{
			if (!string.IsNullOrEmpty(msgToDisplay))
			{
				Globals.Message.Show(msgToDisplay);
			}
		}

		void EnableDisableCore(bool enable, ContactsLists contactsLists)
		{
			var processed = new List<BusinessObject>();
			foreach (var orgContact in contactsLists.ContactsToChange)
			{
				orgContact.OC_WebAccessEnabled = enable;
				if (enable)
				{
					if (!ContactSendEmailSetResetPassword.SendPasswordInstructions(orgContact, false))
					{
						orgContact.OC_WebAccessEnabled = !enable;
						contactsLists.EmailSendingErrorContacts.Add(orgContact);
						continue;
					}
				}
				processed.Add(orgContact);
			}

			DisplayMsgIfNotEmpty(GetMsgToDisplay(enable, processed, contactsLists));
		}

		string GetMsgToDisplay(bool enable, List<BusinessObject> processed, ContactsLists contactsLists)
		{
			var enabledOrDisabled = enable ? Res.GetString("1127a415-480a-41a1-b209-6cafd9603967", "enabled.") : Res.GetString("4e7876be-9c84-4fb4-b025-98186e30e828", "disabled.");
			var msgToDisplay = string.Empty;

			var errorMsg = Res.GetString("4c6b81ee-8cb1-480e-9944-ab3eb3dda58f", "The following contacts could not have Web Access enabled as the email containing set/reset password instructions could not be sent. An error was encountered while sending the password instruction email");
			msgToDisplay = PopulateErrorMsgFromList(contactsLists.EmailSendingErrorContacts, errorMsg, msgToDisplay);

			errorMsg = Res.GetString("2b4a067d-cd52-44e6-b1a6-91713597f88d", "The following contacts could not be enabled as they have an invalid email address:");
			msgToDisplay = PopulateErrorMsgFromList(contactsLists.EmailHasErrorContacts, errorMsg, msgToDisplay);

			errorMsg = Res.GetString("baf5654b-20ae-45df-9360-f855fd6fd6bf", "The following contacts could not be enabled as Web Access can only be enabled once per email:");
			msgToDisplay = PopulateErrorMsgFromList(contactsLists.ConstraintErrorContacts, errorMsg, msgToDisplay);

			if (processed.Any())
			{
				msgToDisplay += System.Environment.NewLine;
				msgToDisplay += Res.GetString("1fbdb30f-caf1-4698-9757-19dedca73b53", "{0} contacts have had Web Access ", processed.Count.ToString(CultureInfo.CurrentCulture)) + enabledOrDisabled;
			}
			else if (processed.Count == 0 && string.IsNullOrEmpty(msgToDisplay))
			{
				msgToDisplay += System.Environment.NewLine;
				msgToDisplay += Res.GetString("c59926dc-c57d-4ba4-ae71-24a088177276", "The selected contacts are already {0} No changes were made.", enabledOrDisabled);
			}

			return msgToDisplay;
		}

		string PopulateErrorMsgFromList(List<OrgContact> contactList, string errorMsg, string overallMsg)
		{
			if (contactList.Any())
			{
				overallMsg += System.Environment.NewLine;
				overallMsg += errorMsg;
				overallMsg += System.Environment.NewLine;
				foreach (var contact in contactList)
				{
					overallMsg += contact.OC_ContactName + ", ";
				}
				overallMsg = overallMsg.Remove(overallMsg.Length - 2, 2);
				overallMsg += System.Environment.NewLine;
			}
			return overallMsg;
		}

		protected class ContactsLists
		{
			public ContactsLists(List<OrgContact> contactsToChange, List<OrgContact> emailHasErrorContacts, List<OrgContact> constraintErrorContacts)
			{
				ContactsToChange = contactsToChange;
				EmailHasErrorContacts = emailHasErrorContacts;
				ConstraintErrorContacts = constraintErrorContacts;
				EmailSendingErrorContacts = new List<OrgContact>();
			}

			public List<OrgContact> ContactsToChange { get; set; }
			public List<OrgContact> EmailHasErrorContacts { get; set; }
			public List<OrgContact> ConstraintErrorContacts { get; set; }
			public List<OrgContact> EmailSendingErrorContacts { get; set; }
		}

		ContactSendEmailSetResetPassword contactSendEmailSetResetPassword;
		ContactSendEmailSetResetPassword ContactSendEmailSetResetPassword => contactSendEmailSetResetPassword ?? (contactSendEmailSetResetPassword = new ContactSendEmailSetResetPassword());
	}
}
