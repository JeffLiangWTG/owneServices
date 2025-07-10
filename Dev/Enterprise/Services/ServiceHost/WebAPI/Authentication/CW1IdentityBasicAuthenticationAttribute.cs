using System;
using CargoWise.Data;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Security;
using Enterprise.ZArchitecture.Environment;
using WTG.Foundation.FrameworkExtensions;

namespace Enterprise.Services.ServiceHost
{
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
	public sealed class CW1IdentityBasicAuthenticationAttribute : IdentityBasicAuthenticationAttribute
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1115:DoNotUseSetUserContext", Justification = "User has been validated")]
		protected override bool IsOK(string userName, string password, out string message)
		{
			using (Db.DisposableActionForDbConnection())
			{
				var loginController = new UserLoginController();
				var authenticatedUser = loginController.ValidateUserLoginAndPassword(userName, password);
				message = authenticatedUser.FailureMessage;
				if (authenticatedUser.IsOK)
				{
					var staff = (GlbStaff)authenticatedUser.User;
					message = ValidateStaffBranchAndDepartment(staff);
					if (message.IsNullOrEmpty())
					{
						using (Env.Instance.SuppressSwitchContextCheck(ensureContextIsRestoredAfterSuppression: false))
						{
							var userContext = EnvProxy.Instance.NewUserContext(staff.GS_LoginName, staff.GS_GB_HomeBranch.ToGuid(), staff.GS_GE_HomeDepartment.ToGuid());
							EnvProxy.Instance.SetUserContext(userContext); // User has been validated
						}
					}
				}

				return authenticatedUser.IsOK && message.IsNullOrEmpty();
			}
		}

		string ValidateStaffBranchAndDepartment(GlbStaff staff)
		{
			if (staff.GS_GB_HomeBranch.IsEmpty)
			{
				return Res.GetString("A0524E2F-98A6-4892-86F0-22F32538F65F", "Staff branch is empty.");
			}
			if (!staff.GS_GB_HomeBranch.IsValid)
			{
				return Res.GetString("643053A8-8C06-454B-A8D1-39472619E4B8", "Staff branch is invalid.");
			}

			if (staff.GS_GE_HomeDepartment.IsEmpty)
			{
				return Res.GetString("CA586CCC-609C-4BF5-8ABA-0AF26E3692E1", "Staff department is empty.");
			}
			if (!staff.GS_GE_HomeDepartment.IsValid)
			{
				return Res.GetString("22912BCC-1C0A-45C9-8041-8940D4B592B7", "Staff department is invalid.");
			}

			return string.Empty;
		}
	}
}
