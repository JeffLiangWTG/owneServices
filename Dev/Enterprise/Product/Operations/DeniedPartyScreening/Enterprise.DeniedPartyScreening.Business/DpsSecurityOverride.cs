using System;
using System.Collections.Generic;
using CargoWise.Application;
using CargoWise.Common;
using Enterprise.Environment;
using Enterprise.Security;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.DeniedPartyScreening
{
	public class DpsSecurityOverride
	{
		public DpsSecurityOverride(Func<SecurityCore, SecurityCheckpoint>[] securityCheckFunctions)
		{
			this.securityCheckFunctions = Argument.NotNull(securityCheckFunctions, "securityCheckFunctions");
		}

		public bool IsAllowed { get { return isAllowed; } }
		public Func<SecurityCore, SecurityCheckpoint>[] NotAllowedSecurityCheckFunctions { get { return notAllowedSecurityCheckFunctions.ToArray(); } }

		public string CheckUserPrivilege(string userName, string password)
		{
			IUserLoginController loginController;
			if (!TryLoginUser(userName, password, out loginController))
			{
				return Enterprise.DeniedPartyScreening.Business.Res.GetString("7260c0f8-ec7f-41c2-9c0d-0ff278ba2425", "Username/password is invalid");
			}

			var userSecurity = (SecurityCore)loginController.GetSecurityForUser(userName, Env.CurrentBranch.PK, Env.CurrentDepartment.PK);

			foreach (var securityCheckFunction in securityCheckFunctions)
			{
				if (!securityCheckFunction(userSecurity).IsAllowed)
				{
					notAllowedSecurityCheckFunctions.Add(securityCheckFunction);
				}
			}

			if (notAllowedSecurityCheckFunctions.Count == 0)
			{
				isAllowed = true;
			}

			return string.Empty;
		}

		#region Implementation

		bool TryLoginUser(string userName, string password, out IUserLoginController loginController)
		{
			loginController = (IUserLoginController)Activator.CreateInstance(ObjectFactory.GetType<IUserLoginController>());
			return loginController.ValidateUserLoginAndPassword(userName, password).LoginValidated;
		}

		readonly Func<SecurityCore, SecurityCheckpoint>[] securityCheckFunctions;
		readonly List<Func<SecurityCore, SecurityCheckpoint>> notAllowedSecurityCheckFunctions = new List<Func<SecurityCore, SecurityCheckpoint>>();
		bool isAllowed;

		#endregion
	}
}
