using System;
using System.Web;
using System.Web.Http;
using CargoWise.Application;
using Enterprise.Environment;
using Enterprise.Services.ServiceHost.Common;
using Enterprise.ZArchitecture.Web.Business;
using Enterprise.ZArchitecture.Web.GlobalBase;

namespace Enterprise.Services.ServiceHost
{
	public class Global : ZEnterpriseGlobal
	{
		protected override void Application_Start(object sender, EventArgs e)
		{
			base.Application_Start(sender, e);

			GlobalConfiguration.Configure(WebApiServiceConfig.Register);
			ObjectFactory.Configure("assembly://Enterprise.Services.ServiceHost/Enterprise.Services.ServiceHost.ApplicationContext/Configuration.xml");
		}

		protected override void Session_Start(object sender, EventArgs e)
		{
			// Ensures a SessionID to avoid System.Web.HttpException when the application attempts to sets the Session ID on a flushed response.
			string sessionId = Session.SessionID;

			base.Session_Start(sender, e);
		}

		protected override void Application_PreSendRequestHeaders(object sender, EventArgs e)
		{
			if (HttpRuntime.UsingIntegratedPipeline)
			{
				SecurityHelper.SecureResponse(Response);
				SecurityHelper.SetCacheHeader(Response);
			}
			base.Application_PreSendRequestHeaders(sender, e);
		}

		public override WebUser GetNewSiteUser() => new OrgContactWebUser();

		readonly Lazy<EnvProvider> lazyWebServicesEnvProvider = new Lazy<EnvProvider>(() => new WebServicesEnvProvider());
		protected override EnvProvider WebEnvProvider => lazyWebServicesEnvProvider.Value;
	}
}
