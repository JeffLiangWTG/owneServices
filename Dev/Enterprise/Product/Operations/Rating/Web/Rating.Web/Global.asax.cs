using System;
using System.Web.Http;
using CargoWise.Application;
using Enterprise.Environment;
using Enterprise.Rating.Web.Configuration;
using Enterprise.ZArchitecture.Web.GlobalBase;

namespace Enterprise.Rating.Web
{
	public class Global : ZEnterpriseGlobal
	{
		protected override void Application_Start(object sender, EventArgs e)
		{
			base.Application_Start(sender, e);

			SharedStartup.ConfigureAtStartup();

			GlobalConfiguration.Configure(config =>
			{
				var configurations = ObjectFactory.New<IRatesAPIsAppSettings>();

				SharedStartup.ConfigureWebApi(config, configurations);
			});
		}

		protected override EnvProvider WebEnvProvider { get; } = new WebServicesEnvProvider();
	}
}
