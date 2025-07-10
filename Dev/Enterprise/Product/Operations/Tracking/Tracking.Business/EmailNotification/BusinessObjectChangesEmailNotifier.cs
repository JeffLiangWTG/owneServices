using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.MessageProcessors;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Tracking.Business
{
	/// <summary>
	/// Class that sends an email to an Enterprise users whenever 
	/// object have been saved via web site
	/// </summary>
	public class BusinessObjectChangesEmailNotifier
	{
		public BusinessObjectChangesEmailNotifier(IBizOChangesEmailNotification bizO)
		{
			this.BizO = bizO;
			bizO.AddPropertiesForEmailReporting(DataState.Original);
			bizO.Factory.Saving += BizO_Saving;
			bizO.Factory.Saved += BizO_Saved;
		}

		internal object GetRegistryItemValueByLocation(IRegistryItem registryItem)
		{
			Guid branchPK = BizO.EventBranch != null ? BizO.EventBranch.PK.ToGuid() : Guid.Empty;
			Guid companyPK = RelatedCompany != null ? RelatedCompany.PK.ToGuid() : Guid.Empty;
			return registryItem.GetFallBackValueAtAllLevels(companyPK, branchPK, Guid.Empty);
		}

		internal ZString GetStaffEmailAndLanguage(BusinessObjectFactory factory, ZGuid staffGuid, out ZString language)
		{
			var email = ZString.Empty;
			language = ZString.Empty;
			if (!staffGuid.IsEmpty)
			{
				var staff = factory.Load<GlbStaff>(staffGuid);
				if (staff != null)
				{
					email = staff.GS_EmailAddress;
					language = staff.GS_WorkingLanguage;
				}
			}
			return email;
		}

		internal MultilingualString GetRegistryItemAndBranchLocation(IMultilingualRegistryItem registryItem)
		{
			if (registryItem != null)
			{
				MultilingualString result = ResString.GetMultilingualString("90e057cc-ade3-458e-95eb-65fccfa520d9", "System Registry: {0}", registryItem.LocationMultilingual);
				if (BizO.EventBranch == null)
				{
					result = MultilingualString.Join(" -> ", result, ResString.GetMultilingualString("71969fa3-3f72-453a-b591-23c224aacbe0", "System"));
				}
				else
				{
					result = MultilingualString.Join(" -> ", result,
						ResString.GetMultilingualString("ba1c822f-419f-43e4-b373-52c65e9824de", "Companies"), (NoResString)BizO.EventBranch.Company.GC_Name,
						ResString.GetMultilingualString("a492deab-59dd-412b-a58f-bd7d6543fe50", "Branches"), (NoResString)BizO.EventBranch.GB_BranchName);
				}
				return result;
			}
			return (NoResString)"";
		}

		[Flags]
		internal enum BizOStatus
		{
			NotChanged = 0,
			Created = 1,
			Modified = 2,
			Cancelled = 4,
			UserNoteAdded = 8,
			UserNoteChanged = 16
		}

		readonly IBizOChangesEmailNotification BizO;

		internal BizOStatus EmailStatus;

		void BizO_Saving(BusinessObjectFactory factory)
		{
			if (!BizO.IsDeleted)
			{
				BizO.AddPropertiesForEmailReporting(DataState.Updated);
				EmailStatus = GetEmailStatus();
			}
			BizO.Factory.Saving -= BizO_Saving;
		}

		internal BizOStatus GetEmailStatus()
		{
			if (BizO.HasChanges)
			{
				BizOStatus status;
				if (BizO.IsInDatabase)
				{
					status = BizO.IsCancelled ? BizOStatus.Cancelled : BizOStatus.Modified;
				}
				else
				{
					status = BizOStatus.Created;
				}

				IWebUserEditableNoteSupport bizOWithNote = BizO as IWebUserEditableNoteSupport;
				if (bizOWithNote != null)
				{
					if (bizOWithNote.UserEditableNoteHelper.IsNoteAdded)
					{
						status |= BizOStatus.UserNoteAdded;
					}
					else if (bizOWithNote.UserEditableNoteHelper.IsNoteChanged)
					{
						status |= BizOStatus.UserNoteChanged;
					}
				}
				return status;
			}

			return BizOStatus.NotChanged;
		}

		void BizO_Saved(BusinessObjectFactory factory, bool savedSuccessfully)
		{
			if (!emailHasBeenSent && savedSuccessfully && !BizO.IsDeleted && (EmailStatus != BizOStatus.NotChanged))
			{
				try
				{
					EmailDef email = PrepareEmailMessage();
					if (email.Recipients.Count > 0)
					{
						Env.OutgoingMailManager.CreateAndSave(email);
					}
				}
				catch (Exception ex) when (!ex.IsCriticalException())
				{
					//if sending of email is failed, do nothing and continue operation
				}
				finally
				{
					emailHasBeenSent = true;
				}
			}
			BizO.Factory.Saved -= BizO_Saved;
		}

		bool emailHasBeenSent;

		OrgHeader RelatedOrg
		{
			get
			{
				return BizO.RelatedOrg ?? (BizO.LoggedInContact != null ? BizO.LoggedInContact.ParentOrg : null);
			}
		}

		GlbCompany RelatedCompany
		{
			get
			{
				return BizO.EventBranch != null ? BizO.EventBranch.Company : null;
			}
		}

		OrgStaffAssignmentsCollection StaffAssignments
		{
			get
			{
				if (RelatedOrg != null)
				{
					return RelatedCompany != null ? RelatedOrg.GetStaffAssignmentsForGlbCompany(RelatedCompany) : RelatedOrg.StaffAssignments;
				}
				return null;
			}
		}

		#region Prepare Email Message

		internal EmailDef PrepareEmailMessage()
		{
			EmailDef email = new EmailDef { ContentType = EmailContentTypes.HTML, FromDisplayName = Core.Constants.ProductName };

			if (EmailStatus != BizOStatus.NotChanged && AddEmailRecipientsAndFooterText(email) > 0)
			{
				using (Res.TemporarilySwitchLanguage(language))
				{
					email.AttachHeaderImage(SystemDataRegistry.Instance.HtmlEmailBannerImage);
					email.AttachFooterImage(SystemDataRegistry.Instance.HtmlEmailFooterImage);
					email.Subject = GetHumanReadableName() + GetStatusDescription();

					ZString emaiBody = ExtractEmailTemplateFromResource();
					emaiBody = emaiBody.Replace("(*Title*)", email.Subject);
					emaiBody = emaiBody.Replace("(*HtmlStyleSheet*)", SystemDataRegistry.Instance.HtmlEmailStyleSheet.Value);
					emaiBody = emaiBody.Replace("(*HeadingWithUrl*)", GetHeadingWithURL());
					emaiBody = emaiBody.Replace("(*Message*)", GetEmailMessage());
					email.Body = emaiBody;
				}
			}

			return email;
		}

		#region AddEmailRecipientsAndFooterText

		int AddEmailRecipientsAndFooterText(EmailDef email)
		{
			string sendingRule = BizO.NotificationSendingRule.Value;
			int rolesRecipients = 0, groupRecipients = 0;

			switch (sendingRule)
			{
				case EmailNotificationSendingRules.ALL:
					rolesRecipients = AddEmailRecipientsByStaffRoles(email);
					groupRecipients = AddEmailRecipientsByGroupRegistryItem(email);
					break;
				case EmailNotificationSendingRules.GRP:
					groupRecipients = AddEmailRecipientsByGroupRegistryItem(email);
					if (email.Recipients.Count == 0)
					{
						rolesRecipients = AddEmailRecipientsByStaffRoles(email);
					}
					break;
				case EmailNotificationSendingRules.ROL:
					rolesRecipients = AddEmailRecipientsByStaffRoles(email);
					if (email.Recipients.Count == 0)
					{
						groupRecipients = AddEmailRecipientsByGroupRegistryItem(email);
					}
					break;
			}

			if (email.Recipients.Count > 0)
			{
				using (Res.TemporarilySwitchLanguage(language))
				{
					AddEmailFooter(rolesRecipients, groupRecipients, email);
				}
			}

			return email.Recipients.Count;
		}

		internal void AddEmailFooter(int rolesRecipients, int groupRecipients, EmailDef email)
		{
			string result = string.Empty;

			string groupRegistryItemAndBranchLocation = GetRegistryItemAndBranchLocation(BizO.EmailGroupRegistryItem);

			string wasSentToAllUsers = Res.GetString("3e61ee07-2d3c-426e-994a-fa9a4aad832f", "Since there are no valid email addresses set up in this group this email has been sent to all users.");
			wasSentToAllUsers = email.FooterText.Contains(wasSentToAllUsers)
														? System.Environment.NewLine + wasSentToAllUsers
														: string.Empty;

			if (rolesRecipients > 0)
			{
				string rolesRegistryItemAndBranchLocation = GetRegistryItemAndBranchLocation(BizO.StaffRolesToNotify);
				string orgFullNameAndCode = String.Format("{0} {1}", RelatedOrg.OH_Code, RelatedOrg.OH_FullNameTruncated);

				result = groupRecipients > 0

									? Res.GetString("bfc03543-3e74-40bc-929b-162857e1fecc", @"This email was sent to:

 - an assigned staff for {0} to those who have next roles:
{1}.
You can change this in the setting located at
{2}.

 - the email group defined at System Registry:
{3}.{4}",
											orgFullNameAndCode,
								MultilingualString.Join(", ", notifyedRoles.ToArray()),
											rolesRegistryItemAndBranchLocation,
											groupRegistryItemAndBranchLocation,
											wasSentToAllUsers)

									: Res.GetString("f5a48a04-41bf-4380-85e9-e3a7619f0a4b", @"This email was sent to an assigned staff for {0} to those who have next roles:
{1}.
You can change this in the setting located at
{2}.",
								orgFullNameAndCode,
								MultilingualString.Join(", ", notifyedRoles.ToArray()),
											rolesRegistryItemAndBranchLocation);
			}
			else if (groupRecipients > 0)
			{
				result = Res.GetString("09f0c83a-4605-45c3-9b39-c1caea1067ca", @"This email was sent to the email group defined at
{0}.{1}",
					groupRegistryItemAndBranchLocation,
					wasSentToAllUsers);
			}

			email.FooterText = result;
		}

		internal int AddEmailRecipientsByStaffRoles(EmailDef email)
		{
			if (RelatedOrg != null && BizO.StaffRolesToNotify != null)
			{
				var staffRoles = (ICodeDescriptionBoolList)GetRegistryItemValueByLocation(BizO.StaffRolesToNotify);

				foreach (CodeDescriptionBool role in staffRoles)
				{
					if (role.Bool)
					{
						ZGuid staffGuid = BizO.GetStaffGuid(StaffAssignments, role);
						ZString staffLanguage;
						ZString emailAddress = GetStaffEmailAndLanguage(BizO.Factory, staffGuid, out staffLanguage);

						if (emailAddress.IsEmpty)
						{
							ZString staffNK = StaffAssignments.GetStaffAssignment(role.Code, OrgStaffAssignmentsLookups.AllServices);
							GlbStaff staff = StaffAssignments.Factory.LoadFromNaturalKey<GlbStaff>(GlbStaffSchema.GS_Code, staffNK);
							if (staff != null)
							{
								staffGuid = staff.PK;
							}
							else
							{
								staffGuid = ZGuid.Empty;
							}
							emailAddress = GetStaffEmailAndLanguage(BizO.Factory, staffGuid, out staffLanguage);
						}

						if (!emailAddress.IsEmpty && !email.Recipients.Contains(emailAddress))
						{
							email.AddRecipientForUserCommunication(emailAddress);
							notifyedRoles.Add(role.Description);
							AddRecipientLanguage(staffLanguage);
						}
					}
				}
			}

			return email.Recipients.Count;
		}

		readonly List<MultilingualString> notifyedRoles = new List<MultilingualString>();
		ZString language = ZString.Empty;

		int AddEmailRecipientsByGroupRegistryItem(EmailDef email)
		{
			if (BizO.EmailGroupRegistryItem != null)
			{
				Guid groupPK = (Guid)GetRegistryItemValueByLocation(BizO.EmailGroupRegistryItem);
				var groupLocation = GetRegistryItemAndBranchLocation(BizO.EmailGroupRegistryItem);

				string groupLanguage;
				email.AddGroupOfRecipientsOrAllUsersIfGroupIsEmpty(groupPK, groupLocation, out groupLanguage);
				AddRecipientLanguage(groupLanguage);
			}

			return email.Recipients.Count;
		}

		void AddRecipientLanguage(ZString recipientLanguage)
		{
			if (language.IsEmpty)
			{
				language = recipientLanguage;
			}
			else if (recipientLanguage != language)
			{
				language = Res.DefaultLanguage;
			}
		}

		#endregion

		ZString GetHumanReadableName()
		{
			ZString humanReadableName = BizO.HumanReadableName;

			if (!humanReadableName.Contains(BizO.Number))
			{
				humanReadableName += " " + BizO.Number;
			}

			return humanReadableName;
		}

		ZString GetStatusDescription()
		{
			ZString description = ZString.Empty;

			if ((EmailStatus & BizOStatus.Cancelled) == BizOStatus.Cancelled)
			{
				description = " " + Res.GetString("e3747013-2ba2-4d22-80d7-a83b7585d0e2", "has been canceled");
			}
			else if ((EmailStatus & BizOStatus.Created) == BizOStatus.Created)
			{
				description = " " + Res.GetString("7fcb9c0a-865e-4682-b1a6-e18333b79290", "has been placed");
			}
			else if ((EmailStatus & BizOStatus.Modified) == BizOStatus.Modified)
			{
				description = " " + Res.GetString("4a3cd53d-a2a2-47d6-b8bf-d012402caab6", "has been modified");
			}

			IWebUserEditableNoteSupport bizOWithNote = BizO as IWebUserEditableNoteSupport;
			if (bizOWithNote != null)
			{
				if ((EmailStatus & BizOStatus.UserNoteAdded) == BizOStatus.UserNoteAdded)
				{
					description += " " + Res.GetString("e3ef5723-e947-46a0-9f95-63ff09736a67", "and has new {0}", bizOWithNote.UserEditableNoteHelper.EditableNoteType.Description);
				}
				else if ((EmailStatus & BizOStatus.UserNoteChanged) == BizOStatus.UserNoteChanged)
				{
					description += " " + Res.GetString("e1b6cf0a-4cf9-4539-b2cb-398cdb647287", "and has modified {0}", bizOWithNote.UserEditableNoteHelper.EditableNoteType.Description);
				}
			}

			return description;
		}

		ZString ExtractEmailTemplateFromResource()
		{
			Stream stream = typeof(BusinessObjectChangesEmailNotifier).Assembly.GetManifestResourceStream(PathOfBizObjChangeEmailTemplate);
			if (stream != null)
			{
				return new StreamReader(stream).ReadToEnd();
			}
			throw new ZException("Email template was not loaded!");
		}

		const string PathOfBizObjChangeEmailTemplate = "Enterprise.Tracking.Business.EmailNotification.BizObjChangeEmailTemplate.htm";

		ZString GetHeadingWithURL()
		{
			ZString editFormUrl = ZString.Empty;
			if (BizO.ControllerForEnterpriseUrl != null)
			{
				editFormUrl = ShowEditFormUrlHandler.Instance.Create(BizO.ControllerForEnterpriseUrl, BizO.PK);
			}

			ZString headingWithURL;

			if (!editFormUrl.IsEmpty)
			{
				headingWithURL = String.Format("<a href=\"{0}\">{1}</a>", editFormUrl, GetHumanReadableName());
			}
			else
			{
				headingWithURL = GetHumanReadableName();
			}

			return headingWithURL;
		}

		#region GetEmailMessage

		ZString GetEmailMessage()
		{
			const string htmlBRElement = "<br />";

			ZStringBuilder stringbuilder = new ZStringBuilder();
			stringbuilder.AppendIfNotEmpty(GetChangedByDescription());
			stringbuilder.Append(Res.GetString("72a89269-47d7-4e1f-b8d2-213619123928", "When: {0}", ZDateTime.Now.ToLongTimeString()));
			stringbuilder.AppendIfNotEmpty(GetHtmlTableForShortFormatProperties());
			stringbuilder.AppendIfNotEmpty(GetHtmlTableForWebEditableNote());

			return stringbuilder.ToStringWithDelimiterBetweenAppends(htmlBRElement + System.Environment.NewLine + htmlBRElement);
		}

		ZString GetChangedByDescription()
		{
			ZString description = ZString.Empty;

			if (BizO.LoggedInContact != null)
			{
				if ((EmailStatus & BizOStatus.Cancelled) == BizOStatus.Cancelled)
				{
					description = Res.GetString("fbc980c7-5753-4d43-b04d-4c39293e2fbd", "Canceled By: {0} ({1})",
												BizO.LoggedInContact.ParentOrg.OH_FullNameTruncated,
						BizO.LoggedInContact.OC_ContactName);
				}
				else if ((EmailStatus & BizOStatus.Created) == BizOStatus.Created)
				{
					description = Res.GetString("86f88f8a-dc3a-4610-b856-5d345b63ee3f", "Added By: {0} ({1})",
												BizO.LoggedInContact.ParentOrg.OH_FullNameTruncated,
						BizO.LoggedInContact.OC_ContactName);
				}
				else if ((EmailStatus & BizOStatus.Modified) == BizOStatus.Modified)
				{
					description = Res.GetString("335176d5-2cad-400a-8d56-7330330037ae", "Changed By: {0} ({1})",
												BizO.LoggedInContact.ParentOrg.OH_FullNameTruncated,
						BizO.LoggedInContact.OC_ContactName);
				}
			}

			return description;
		}

		internal ZString GetHtmlTableForShortFormatProperties()
		{
			ZString result = ZString.Empty;
			bool hasOriginalValue = !((EmailStatus & BizOStatus.Created) == BizOStatus.Created);
			bool showTable = false;

			PropertyChangeInfo[] infos = BizO.GetPropertiesForEmailReporting();

			if (infos != null && infos.Length > 0)
			{
				HtmlTableCreator table;

				if (hasOriginalValue)
				{
					table = new HtmlTableCreator(new[] { Res.GetString("de596a41-6b2e-4b3a-ace2-ea3e413298d4", "Field"), Res.GetString("8ffc865d-f40f-4de7-8242-7000aceda13e", "Original Value"), Res.GetString("2089b5e1-45be-4514-9cab-ae4222603aa4", "Updated Value") }) { EnableHTMLEncoding = false };

					foreach (PropertyChangeInfo info in infos)
					{
						if (info.HasChanges)
						{
							table.WriteRow(new string[] { info.HumanReadableName, WebUtility.HtmlEncode(info.OriginalValue), WebUtility.HtmlEncode(info.UpdatedValue) });
							showTable = true;
						}
					}
				}
				else
				{
					table = new HtmlTableCreator(new[] { Res.GetString("de596a41-6b2e-4b3a-ace2-ea3e413298d4", "Field"), Res.GetString("2089b5e1-45be-4514-9cab-ae4222603aa4", "Updated Value") }) { EnableHTMLEncoding = false };

					foreach (PropertyChangeInfo info in infos)
					{
						if (info.UpdatedValue != PropertyChangeInfo.Empty)
						{
							table.WriteRow(new string[] { info.HumanReadableName, WebUtility.HtmlEncode(info.UpdatedValue) });
						}
						showTable = true;
					}
				}

				if (showTable)
				{
					result = table.ToHtml();
				}
			}

			return result;
		}

		internal ZString GetHtmlTableForWebEditableNote()
		{
			ZString result = ZString.Empty;

			IWebUserEditableNoteSupport bizObjWithNote = BizO as IWebUserEditableNoteSupport;

			if (bizObjWithNote != null)
			{
				PropertyChangeInfo changeInfo = new PropertyChangeInfo(
					(NoResString)bizObjWithNote.UserEditableNoteHelper.EditableNoteType.Description,
					bizObjWithNote.UserEditableNoteHelper.OriginalNoteText,
					bizObjWithNote.UserEditableNoteHelper.FullNoteText);

				if (changeInfo.HasChanges)
				{
					HtmlTableCreator table = new HtmlTableCreator(new string[] { changeInfo.HumanReadableName, Res.GetString("6a6230b3-99d3-4080-9377-246bdfe1893f", "Value") });
					table.EnableHTMLEncoding = false;
					table.WriteRow(Res.GetString("2089b5e1-45be-4514-9cab-ae4222603aa4", "Updated Value"), WebUtility.HtmlEncode(changeInfo.UpdatedValue));
					if (!bizObjWithNote.UserEditableNoteHelper.EditableNoteType.IsReadOnlyAfterAdd)
					{
						table.WriteRow(Res.GetString("8ffc865d-f40f-4de7-8242-7000aceda13e", "Original Value"), WebUtility.HtmlEncode(changeInfo.OriginalValue));
					}
					result = table.ToHtml();
				}
			}

			return result;
		}

		#endregion

		#endregion
	}
}
