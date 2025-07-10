using Enterprise.BatchProcessor;
using Enterprise.Customs.SG.MHUB;

namespace Enterprise.Customs.SG.V4.MHUB
{
	public abstract class CheckLoggedInCommand : MHUBWebCommand
	{
		public CheckLoggedInCommand(LoginCommand login, IMHUBSettings settingsProvider,LoggingInformation logger, bool verboseLogging)
			: base(login.loginDetails.EdiServlet, settingsProvider, logger, verboseLogging)
		{
			this.login = login;
		}

		public override bool Execute()
		{
			bool result = false;

			if (login.LoginState == LoginCommand.LoginStateType.NotLoggedIn)
			{
				login.Execute();
			}

			if (login.LoginState == LoginCommand.LoginStateType.LoggedIn)
			{
				sessionCookie = login.SessionCookie;
				result = base.Execute();
			}

			return result;
		}

		protected LoginCommand login;
	}
}
