using System;
using System.Collections.Generic;
using Enterprise.BatchProcessor;
using Enterprise.Customs.SG.MHUB;

namespace Enterprise.Customs.SG.V4.MHUB
{
	public class LogoutCommand : CheckLoggedInCommand
	{
		public LogoutCommand(LoginCommand login, IMHUBSettings settingsProvider, LoggingInformation logger, bool verboseLogging)
			: base(login, settingsProvider, logger, verboseLogging)
		{
		}

		public override bool Execute()
		{
			bool result = false;
			if (login.LoginState == LoginCommand.LoginStateType.LoggedIn)
			{
				result = base.Execute();

				if (result)
				{
					login.LoginState = LoginCommand.LoginStateType.NotLoggedIn;
				}
			}

			return result;
		}

		protected override MHUBConstants.CommandType CommandToExecute
		{
			get { return MHUBConstants.CommandType.Logout; }
		}

		protected override Dictionary<String, object> InputParameterList
		{
			get { return new Dictionary<String, object>(); }
		}
	}
}
