using System;
using System.Globalization;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.MasterFiles.Integration;
using Enterprise.Security;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public static class CreditOnHoldChecker
	{
		public static IGlbStaff GetLastUserSettingLocalCreditOnHold(Logs eventLogs)
		{
			return GetLastUserSettingCreditOnHold(eventLogs);
		}

		public static IGlbStaff GetLastUserSettingGlobalCreditHold(Logs eventLogs)
		{
			return GetLastUserSettingCreditOnHold(eventLogs, true);
		}

		public static void ValidateGlobalOnCreditHold(OrgMiscServ orgMiscServ)
		{
			if (!orgMiscServ.Header.OH_IsDebtor)
			{
				return;
			}

			if (orgMiscServ.OM_ARGlobalOnCreditHold || orgMiscServ.OM_ARGlobalOnCreditHoldInfo.HasChanges)
			{
				var orgHeader = orgMiscServ.Header;
				var companyData = orgHeader.CompanyData;
				var lastUserWhoTicked = GetLastUserSettingGlobalCreditHold(orgMiscServ.Logs);
				int currentUserLevel = companyData.GetCreditOnHoldMaxLevel(Env.Security);
				string untickWarningMessageSetUserWithoutApprovalRight = string.Empty;
				string untickWarningMessageSetUserWithHigherRight = string.Empty;

				string currentUserWithoutApprovalRightTickedWarningMessage =
					Res.GetString("4abd0ced-3289-4d84-95a4-612db5eae4ec",
@"You ticked 'Global AR on Credit Hold' check box without any approval right.
Only user with approval level 3 will be able to approve credit controlled documents for this organization. Please setup proper Credit On Hold approval rights for users who usually modifying this field.");

				string noncurrentUserWithoutApprovalRightTickedWarningMessage = string.Empty;

				if (lastUserWhoTicked != null)
				{
					untickWarningMessageSetUserWithoutApprovalRight =
						Res.GetString("b5b34e06-a85b-438f-9dca-5db48431b6c8", "You do not have the appropriate security rights to un-tick the 'Global AR on Credit Hold' check box. This is because the credit on hold has been set by {0} ({1}) without any approval right and so only the same user or user with approval level 3 can un-tick it.", lastUserWhoTicked.GS_FullName, lastUserWhoTicked.GS_Code);

					untickWarningMessageSetUserWithHigherRight =
						Res.GetString("395368d1-bf60-4244-9331-d8381fbdd9a5", "You do not have the appropriate security rights to un-tick the 'Global AR on Credit Hold' check box. This is because the credit on hold has been approved by {0} ({1}) with higher approval right.", lastUserWhoTicked.GS_FullName, lastUserWhoTicked.GS_Code);

					noncurrentUserWithoutApprovalRightTickedWarningMessage =
						Res.GetString("9c522a35-04fd-4950-b4e2-b955ab017454",
@"User {0} ({1}) without any approval right ticked 'Global AR on Credit Hold' check box.
Only user with approval level 3 will be able to approve credit controlled documents for this organization. Please setup proper Credit On Hold approval rights for users who usually modifying this field.", lastUserWhoTicked.GS_FullName, lastUserWhoTicked.GS_Code);
				}

				ValidateOnCreditHold(companyData, lastUserWhoTicked, currentUserLevel,
					orgMiscServ.OM_ARGlobalOnCreditHold, orgMiscServ.OM_ARGlobalOnCreditHoldInfo,
					currentUserWithoutApprovalRightTickedWarningMessage,
					noncurrentUserWithoutApprovalRightTickedWarningMessage,
					untickWarningMessageSetUserWithoutApprovalRight,
					untickWarningMessageSetUserWithHigherRight);
			}
		}

		public static void ValidateLocalOnCreditHold(OrgCompanyData orgCompanyData)
		{
			if (!orgCompanyData.OB_IsDebtor)
			{
				return;
			}

			if (orgCompanyData.OB_AROnCreditHold || orgCompanyData.OB_AROnCreditHoldInfo.HasChanges)
			{
				var lastUserWhoTicked = GetLastUserSettingLocalCreditOnHold(orgCompanyData.Logs);
				int currentUserLevel = orgCompanyData.GetCreditOnHoldMaxLevel(Env.Security);
				string currentUserWithoutApprovalRightTickedWarningMessage =
					Res.GetString("9ABE8B42-2C68-4E89-BFAA-CCE71375204D",
@"You ticked 'AR on Credit Hold' check box without any approval right.
Only user with approval level 3 will be able to approve credit controlled documents for this organization. Please setup proper Credit On Hold approval rights for users who usually modifying this field.");

				string noncurrentUserWithoutApprovalRightTickedWarningMessage = string.Empty;
				string untickWarningMessageSetUserWithoutApprovalRight = string.Empty;
				string untickWarningMessageSetUserWithHigherRight = string.Empty;

				if (lastUserWhoTicked != null)
				{
					untickWarningMessageSetUserWithoutApprovalRight =
						Res.GetString("29671B66-5EF1-440B-982E-21C7E311C825", "You do not have the appropriate security rights to un-tick the 'AR on Credit Hold' check box. This is because the credit on hold has been set by {0} ({1}) without any approval right and so only the same user or user with approval level 3 can un-tick it.", lastUserWhoTicked.GS_FullName, lastUserWhoTicked.GS_Code);
					untickWarningMessageSetUserWithHigherRight =
						Res.GetString("4877051b-0e53-49ab-98ca-3b54ef7d1ab0", "You do not have the appropriate security rights to un-tick the 'AR on Credit Hold' check box. This is because the credit on hold has been approved by {0} ({1}) with higher approval right.", lastUserWhoTicked.GS_FullName, lastUserWhoTicked.GS_Code);
					noncurrentUserWithoutApprovalRightTickedWarningMessage = Res.GetString("64EB9761-6218-4706-B7BA-793DCBCE540D",
@"User {0} ({1}) without any approval right ticked 'AR on Credit Hold' check box.
Only user with approval level 3 will be able to approve credit controlled documents for this organization. Please setup proper Credit On Hold approval rights for users who usually modifying this field.", lastUserWhoTicked.GS_FullName, lastUserWhoTicked.GS_Code);
				}

				ValidateOnCreditHold(orgCompanyData, lastUserWhoTicked, currentUserLevel, orgCompanyData.OB_AROnCreditHold,
					orgCompanyData.OB_AROnCreditHoldInfo,
					currentUserWithoutApprovalRightTickedWarningMessage,
					noncurrentUserWithoutApprovalRightTickedWarningMessage,
					untickWarningMessageSetUserWithoutApprovalRight,
					untickWarningMessageSetUserWithHigherRight);
			}
		}

		static void ValidateOnCreditHold(OrgCompanyData orgCompanyData, IGlbStaff lastUserWhoTicked,
			int currentUserLevel, bool onCreditHold, ZPropertyInfo onCreditHoldInfo,
			string currentUserWithoutApprovalRightTickedWarningMessage,
			string nonCurrentUserWithoutApprovalRightTickedWarningMessage,
			string untickWarningMessageSetUserWithoutApprovalRight,
			string untickWarningMessageSetUserWithHigherRight)
		{
			int levelOfLastUserWhoTicked = -1;
			if (lastUserWhoTicked != null)
			{
				levelOfLastUserWhoTicked = orgCompanyData.GetCreditOnHoldMaxLevel(new SecurityCore(null, lastUserWhoTicked.PK.ToGuid(), Env.CurrentBranch.PK, Env.CurrentDepartment.PK, Env.CurrentCompany.PK));
			}
			if (onCreditHold)
			{
				var isCurrentUserChangedValue = onCreditHoldInfo.HasChanges;
				var isUserWithoutRights = (isCurrentUserChangedValue ? currentUserLevel : levelOfLastUserWhoTicked) == 0;
				if (isUserWithoutRights)
				{
					if (isCurrentUserChangedValue || lastUserWhoTicked.PK == GlbStaff.CurrentUser.PK)
					{
						onCreditHoldInfo.AddWarning(currentUserWithoutApprovalRightTickedWarningMessage);
					}
					else
					{
						onCreditHoldInfo.AddWarning(nonCurrentUserWithoutApprovalRightTickedWarningMessage);
					}
				}
			}
			else if (lastUserWhoTicked != null && lastUserWhoTicked.PK != GlbStaff.CurrentUser.PK)
			{
				var maxLevelRequiredToOverrideUnauthorizedUser = levelOfLastUserWhoTicked == 0;
				if (currentUserLevel < (maxLevelRequiredToOverrideUnauthorizedUser ? 3 : levelOfLastUserWhoTicked))
				{
					if (maxLevelRequiredToOverrideUnauthorizedUser)
					{
						onCreditHoldInfo.AddError(untickWarningMessageSetUserWithoutApprovalRight);
					}
					else
					{
						onCreditHoldInfo.AddError(untickWarningMessageSetUserWithHigherRight);
					}
				}
			}
		}

		static IGlbStaff GetLastUserSettingCreditOnHold(Logs eventLogs, bool isGlobal = false)
		{
			String referenceToCheck = String.Format(CultureInfo.InvariantCulture, "%|{0}={1}", OrgCompanyData.LogParameterKeys.Type, OrgCompanyData.LogTypes.CreditOnHold);
			if (isGlobal)
			{
				referenceToCheck = String.Format(CultureInfo.InvariantCulture, "%{0}%", OrgCompanyData.LogReferenceKeys.GlobalCreditOnHold);
			}

			var filter = new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.CreditControlsModified.Code);
			filter.AddToFilter(StmALogSchema.SL_Reference, SQLComparisonOperator.Like, referenceToCheck);
			filter.OrderBy = StmALogSchema.SL_PostedTimeUtc.Name + " DESC";

			var log = eventLogs.Find(filter).FirstOrDefault();

			IGlbStaff staff = null;
			if (log != null)
			{
				staff = log.User;
			}

			return staff;
		}

		#region Test
	}
	#endregion
}
