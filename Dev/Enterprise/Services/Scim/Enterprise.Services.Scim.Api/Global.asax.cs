using System;
using System.Diagnostics.CodeAnalysis;
using CargoWise.Data;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Web.GUI;
using WTG.StaticAnalysis.Annotation;

namespace Enterprise.Services.Scim.Api
{
	public class Global : ZGlobal
	{
		protected override bool IsApplicationUserInteractive => false;

		protected override void Application_Start(object sender, EventArgs e)
		{
			base.Application_Start(sender, e);
			DbConnection.ApplicationName = "ScimService";
		}

		protected sealed override void Application_BeginRequest(object sender, EventArgs e)
		{
			base.Application_BeginRequest(sender, e);
			SetupUserContext();
		}

		[SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Page path")]
		public override string DefaultPage
		{
			get { return ApplicationRoot + "default.htm"; }
		}

		[SuppressMessage("CargoWiseOne", "CW1115:DoNotUseSetUserContext", Justification = "Web Application. Should Set up User Context rather than temporary user context")]
		static void SetupUserContext()
		{
			if (!(Env.CurrentUserContext?.User?.IsWebUser ?? false))
			{
				lock (setupUserContextLock)
				{
					if (!(Env.CurrentUserContext?.User?.IsWebUser ?? false))
					{
						using (Db.DisposableActionForDbConnection())
						{
							var userContext = new UserContext(ZArchitecture.Environment.User.WebUserName, EnvProxy.Instance.Registry.WebBranch, EnvProxy.Instance.Registry.WebDepartment);
							Env.SetUserContext(userContext); // Web Application. Should Set up User Context rather than temporary user context
						}
					}
				}
			}
		}

		[ThreadSafe]
		static readonly object setupUserContextLock = new object();
	}
}
