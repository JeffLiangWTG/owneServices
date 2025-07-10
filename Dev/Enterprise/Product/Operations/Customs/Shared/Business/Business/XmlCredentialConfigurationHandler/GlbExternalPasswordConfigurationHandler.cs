using System.Linq;
using System.Text;
using CargoWise.EntityFramework;
using CargoWise.eServices.Encryption.Client.Decryptor;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Customs.XmlCredential;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.Business.XmlCredential
{
	public class GlbExternalPasswordConfigurationHandler : XmlCredentialConfigurationHandler
	{
		protected GlbExternalPasswordConfigurationHandler(LoggingInformation logger)
			: base(logger)
		{
		}

		protected override bool ProcessBranchLevel(Configuration configuration, Group systemGroup, ItemData systemItems, Group companyGroup, ItemData companyItems, Group branchGroup, ItemData branchItems)
		{
			logger.LogError(Res.GetString("{0741BD35-FB73-43DE-B9F9-2094547F5D1A}", "Branch Level is not supported."));
			return false;
		}

		protected override bool ProcessCompanyOtherLevel(Configuration configuration, Group systemGroup, ItemData systemItems, Group companyGroup, ItemData companyItems, Group otherGroup, ItemData otherItems)
		{
			var company = GetCompany(companyGroup.Reference);
			return company != null && ProcessCredential(configuration, otherGroup, otherItems, GetStatusReason(systemGroup, companyGroup, null, null, null), company, null, null);
		}

		protected override bool ProcessGroupOtherLevel(Configuration configuration, Group systemGroup, ItemData systemItems, Group companyGroup, ItemData companyItems, Group groupGroup, ItemData groupItems, Group otherGroup, ItemData otherItems)
		{
			var result = false;
			var group = GetGroup(groupGroup.Reference);
			if (group != null)
			{
				var company = companyGroup == null ? null : GetCompany(companyGroup.Reference);
				return (companyGroup == null || company != null) && ProcessCredential(configuration, otherGroup, otherItems, GetStatusReason(systemGroup, companyGroup, groupGroup, null, null), company, group, null);
			}
			return result;
		}

		protected override bool ProcessStaffOtherLevel(Configuration configuration, Group systemGroup, ItemData systemItems, Group companyGroup, ItemData companyItems, Group groupGroup, ItemData groupItems, Group staffGroup, ItemData staffItems, Group otherGroup, ItemData otherItems)
		{
			var result = false;
			var staff = GetStaff(staffGroup.Reference);
			if (staff != null)
			{
				var group = groupGroup == null ? null : GetGroup(groupGroup.Reference);
				var company = companyGroup == null ? null : GetCompany(companyGroup.Reference);
				return (groupGroup == null || group != null) && (companyGroup == null || company != null) && ProcessCredential(configuration, otherGroup, otherItems, GetStatusReason(systemGroup, companyGroup, groupGroup, staffGroup, otherGroup), company, group, staff);
			}
			return result;
		}

		protected override bool ProcessSystemOtherLevel(Configuration configuration, Group systemGroup, ItemData items, Group otherGroup, ItemData otherItems)
		{
			return ProcessCredential(configuration, otherGroup, otherItems, GetStatusReason(systemGroup, null, null, null, null), null, null, null);
		}

		protected bool ProcessCredential(Configuration configuration, Group groupData, ItemData items, ZString statusReason, GlbCompany company, GlbGroup group, GlbStaff staff)
		{
			var result = true;
			var credential = GetCredential(items, Constants.CredentialDetails.Current);
			if (credential != null)
			{
				var externalPassword = GetExternalPassword(groupData, credential, company, group, staff);
				if (externalPassword == null)
				{
					logger.LogError(Res.GetString("{752483C4-605B-4330-B4EB-16CC8D28F763}", "No password found matching (Type='{0}', Company='{1}', Group='{2}', Staff='{3}').", groupData.Type, company?.GC_Code ?? "NONE", group?.GG_Code ?? "NONE", staff?.GS_Code ?? "NONE"));
					result = false;
				}
				else if (configuration.Timestamp < externalPassword.GP_SystemLastEditTimeUtc)
				{
					logger.LogWarning(Res.GetString("{01280B5F-C79E-4B18-B08B-231DC216D916}", "Password (Type='{0}', Company='{1}', Group='{2}', Staff='{3}') has newer changes than XML data.", groupData.Type, company?.GC_Code ?? "NONE", group?.GG_Code ?? "NONE", staff?.GS_Code ?? "NONE"));
				}
				else
				{
					ProcessCredential(externalPassword, groupData, credential, statusReason);
				}
			}
			return result;
		}

		protected virtual void ProcessCredential(GlbExternalPassword externalPassword, Group group, Credential credential, ZString statusReason)
		{
			if (group.StatusSpecified)
			{
				externalPassword.GP_PasswordStatus = GetPasswordStatus(group);
				if (group.Status == PasswordStatusList.Codes.Valid)
				{
					externalPassword.GP_StatusReason = ZString.Empty;
				}
				else
				{
					externalPassword.GP_StatusReason = statusReason.Left(externalPassword.GP_StatusReasonInfo.MaxLength);
					NotifyPasswordStatusUpdate(externalPassword, statusReason);
				}

				if (SupportsPasswordChanging && credential.Password != null && credential.Password.Any())
				{
					var newPassword = EhubClientDecryptor.Decrypt(Encoding.UTF8.GetString(credential.Password));
					if (newPassword != externalPassword.CurrentDecryptedPassword)
					{
						externalPassword.CurrentDecryptedPassword = newPassword;
						NotifyPasswordChanging(externalPassword, newPassword);
					}
				}
			}
		}

		protected virtual bool SupportsPasswordChanging => false;

		protected virtual ZString GetPasswordStatus(Group group)
		{
			return group.Status == PasswordStatusList.Codes.Valid ? (ZString)Core.Constants.PasswordOK : group.Status;
		}

		#region eMail Notification sendings

		protected virtual string GetStatusChangedEmailBody(GlbExternalPassword externalPassword, ZString statusReason)
		{
			return Res.GetString("{A5A279EA-6EA5-4D9C-A65F-1C0A10ACCBB9}", @"Password Type: '{0} ({1})'
Owner: {2}
Password Status: '{3}'
Status Reason:
{4}", externalPassword.GP_PasswordTypeDescription, externalPassword.GP_PasswordType, GetOwnerInfomations(externalPassword), externalPassword.GP_PasswordStatusDescription, statusReason);
		}

		protected virtual string GetStatusChangedEmailSubject(GlbExternalPassword externalPassword)
		{
			return Res.GetString("9C2E5F72-3D6E-42F7-9A66-9C8DE779E418", "Password Status Has Been Updated to '{0}'", externalPassword.GP_PasswordStatusDescription);
		}

		protected virtual string GetPasswordChangedEmailSubject()
		{
			return Res.GetString("E36D05AD-B136-4D9C-947F-5B01B6669638", "Password Has Been Updated");
		}

		protected virtual string GetPasswordChangedEmailBody(GlbExternalPassword externalPassword, ZString newPassword)
		{
			return Res.GetString(
				"57E71433-119E-45D3-B7F9-18523DE1B7C1",
				@"Password Type: '{0} ({1})'
Owner: {2}
New Password: {3}",
				externalPassword.GP_PasswordTypeDescription,
				externalPassword.GP_PasswordType,
				GetOwnerInfomations(externalPassword),
				newPassword
			);
		}

		ZString GetOwnerInfomations(GlbExternalPassword externalPassword)
		{
			var result = new ZStringBuilder();
			var company = externalPassword.Company;
			if (company != null)
			{
				result.Append(Res.GetString("{1E246617-859A-4B68-B4B5-75918357CC55}", "Company = '{0}'", company.GC_Code));
			}
			var group = externalPassword.Group;
			if (group != null)
			{
				result.Append(Res.GetString("{1F27308A-55DF-4343-B240-7E19F495FF27}", "Group = '{0}'", group.GG_Code));
			}
			var staff = externalPassword.Staff;
			if (staff != null)
			{
				result.Append(Res.GetString("{A6891EF9-B39C-4042-A7FD-DE2EFD373661}", "Staff = '{0}'", staff.GS_Code));
			}
			if (result.IsEmpty)
			{
				result.Append(Res.GetString("{E855680E-C4D8-42D5-9F76-62C3F8CAA34B}", "System"));
			}
			return result.ToStringWithDelimiterBetweenAppends(", ");
		}

		void SendEmailNotification(GlbExternalPassword externalPassword, EmailDef emailDef)
		{
			var staff = externalPassword.Staff;
			if (staff == null)
			{
				var group = externalPassword.Group;
				if (group != null)
				{
					var staffEmails = group.Staff.OfType<GlbStaff>().Select(x => x.GS_EmailAddress.ToString()).Where(x => !string.IsNullOrEmpty(x)).Distinct().ToArray();
					if (staffEmails.Length > 0)
					{
						emailDef.AddRecipientForUserCommunication(staffEmails);
					}
				}
			}
			else if (!staff.GS_EmailAddress.IsEmpty)
			{
				emailDef.AddRecipientForUserCommunication(staff.GS_EmailAddress);
			}
			if (emailDef.Recipients.Count == 0)
			{
				Env.OutgoingMailManager.Create(Factory, emailDef, Core.Constants.Groups.PostMastersGroupPK, GroupSourceLocator.GetFromGroup(Factory.Load<GlbGroup>(Core.Constants.Groups.PostMastersGroupPK)));
			}
			else
			{
				Env.OutgoingMailManager.Create(Factory, emailDef);
			}
		}

		protected void NotifyPasswordStatusUpdate(GlbExternalPassword externalPassword, ZString statusReason)
		{
			var subject = GetStatusChangedEmailSubject(externalPassword);
			var body = GetStatusChangedEmailBody(externalPassword, statusReason);
			var email = CreateEmailNotification(subject, body);
			SendEmailNotification(externalPassword, email);
		}

		protected void NotifyPasswordChanging(GlbExternalPassword externalPassword, ZString newPassword)
		{
			var subject = GetPasswordChangedEmailSubject();
			var body = GetPasswordChangedEmailBody(externalPassword, newPassword);
			var email = CreateEmailNotification(subject, body);
			SendEmailNotification(externalPassword, email);
		}

		#endregion

		protected ZString GetStatusReason(Group systemGroup, Group companyGroup, Group groupGroup, Group staffGroup, Group otherGroup)
		{
			var result = new ZStringBuilder();
			AddStatusReason(result, systemGroup);
			AddStatusReason(result, companyGroup);
			AddStatusReason(result, groupGroup);
			AddStatusReason(result, staffGroup);
			AddStatusReason(result, otherGroup);
			return result.ToStringWithNewLineBetweenAppends();
		}

		protected void AddStatusReason(ZStringBuilder result, Group group)
		{
			if (group != null && group.Annotations.IsSpecified)
			{
				var annotationBuilder = new ZStringBuilder();
				foreach (Item annotation in group.Annotations)
				{
					if (annotation.IsSpecified)
					{
						annotationBuilder.Append(annotation.Value.Trim());
					}
				}
				if (!annotationBuilder.IsEmpty)
				{
					result.AppendFormat("{0}: {1}", group.Type, annotationBuilder.ToStringWithDelimiterBetweenAppends(" "));
				}
			}
		}

		protected virtual GlbExternalPassword GetExternalPassword(Group groupData, Credential credential, GlbCompany company, GlbGroup group, GlbStaff staff)
		{
			GlbExternalPassword result = null;
			var passwordType = GetGP_PasswordType(groupData);
			if (!passwordType.IsEmpty)
			{
				var query = new ZQuery(GlbExternalPasswordSchema.GP_PasswordType, passwordType);
				query.AddToFilter(GlbExternalPasswordSchema.GP_UserID, credential?.UserName ?? ZString.Empty);
				query.AddToFilter(GlbExternalPasswordSchema.GP_GC, company?.PK);
				query.AddToFilter(GlbExternalPasswordSchema.GP_GG, group?.PK);
				query.AddToFilter(GlbExternalPasswordSchema.GP_GS, staff?.PK);
				result = Factory.LoadTop1<GlbExternalPassword>(query);
				if (result != null)
				{
					result.DisableConfigurationToSender = true;
				}
			}

			return result;
		}

		protected virtual ZString GetGP_PasswordType(Group groupData)
		{
			return groupData.Type;
		}

		protected override bool ProcessBranchOtherLevel(Configuration configuration, Group systemGroup, ItemData systemItems, Group companyGroup, ItemData companyItems, Group branchGroup, ItemData branchItems, Group group, ItemData itemData)
		{
			logger.LogError(Res.GetString("{EEEF075D-C461-4947-BB88-A43223853C9F}", "Branch Type is not supported"));
			return false;
		}
	}
}
