using System.Globalization;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.Business
{
	public class SupervisorOverridesValidation : AutoSupervisorOverridesValidation
	{
		public SupervisorOverridesValidation(AutoSupervisorOverrides parent)
			: base(parent)
		{
		}

		protected override void CheckSupervisorName()
		{
			base.CheckSupervisorName();

			MandatoryValidation.CheckEntered(Parent.SupervisorNameInfo, ValidationMessageConstants.UserIsMandatory);
			ListValidation.ErrorIfInvalidCode(Parent.SupervisorNameInfo, Parent.Lookups.Users, (NoResString)ValidationMessageConstants.UserIsNotInList);

			if (!Parent.SupervisorNameInfo.HasNotifications())
			{
				var staff = Parent.Supervisor;
				var companyPK = staff.HomeBranch?.GB_GC ?? ZGuid.Empty;
				if (companyPK != GlbCompany.CurrentCompany.PK)
				{
					Parent.SupervisorNameInfo.AddError(string.Format(CultureInfo.InvariantCulture, ValidationMessageConstants.UserNotInCurrentCompany));
				}
				else if (!staff.GS_IsController)
				{
					var hasTotalSecurityRight = Parent.UnAuthorisedMessagesForLog.Cast<MessageLog>().All(messageLog => Parent.StaffsHaveSecurityRight(messageLog.TargetCode, new[] { staff.PK }));
					if (!hasTotalSecurityRight)
					{
						Parent.SupervisorNameInfo.AddError(string.Format(CultureInfo.InvariantCulture, ValidationMessageConstants.UserIsNotSupervisor));
					}
				}
			}
		}

		protected override void CheckSupervisorPassword()
		{
			base.CheckSupervisorPassword();

			MandatoryValidation.CheckEntered(Parent.SupervisorPasswordInfo, ValidationMessageConstants.PasswordIsMandatory);

			var supervisor = Parent.Supervisor;
			if ((supervisor != null) && !Parent.SupervisorPasswordInfo.HasNotifications())
			{
				var loginResult = Env.LoginController.ValidateUserLoginAndPassword(supervisor.GS_LoginName, Parent.SupervisorPassword);
				if (!loginResult.LoginValidated)
				{
					Parent.SupervisorPasswordInfo.AddError(ValidationMessageConstants.PasswordIsWrong);
				}
			}
		}

		#region Implementation

		public new SupervisorOverrides Parent
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return (SupervisorOverrides)base.Parent; }
		}

		#endregion
	}

	[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Validation message")]
	static class ValidationMessageConstants
	{
		internal const string UserNotInCurrentCompany = "Please select a user code of current company.";
		internal const string UserIsMandatory = "Supervisor user code. Supervisor user code is mandatory";
		internal const string UserIsNotInList = "Please select different user code. The specified user code is not found.";
		internal const string UserIsNotSupervisor = "User does not have authority to override this error. Please select a user who has sufficient authority or check this users security rights in Operate->Customs ->Supervisor Overrides’";
		internal const string PasswordIsMandatory = "Supervisor password. Supervisor password is mandatory";
		internal const string PasswordIsWrong = " Please enter a valid password. The specified password is wrong.";
	}
}
