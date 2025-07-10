using System;
using CargoWise.Common;
using Enterprise.ZArchitecture.Web.GUI;

namespace Enterprise.Tracking.Web.Testing
{
	sealed class LoginCompleteForTest : LoginComplete
	{
		public void DoPageLoad()
		{
			try
			{
				Page_Load(null, EventArgs.Empty);
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				if (ex is QueryStringException || ex is NullReferenceException)
				{
					throw;
				}
			}
		}

		protected override ZGlobal GetNewTestGlobal()
		{
			var result = new GlobalForLoginCompleteTest();
			result.OnCustomSessionStart();
			return result;
		}
	}
}
