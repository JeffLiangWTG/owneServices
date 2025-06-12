using System;
using System.Configuration;
using System.IO;
using System.Xml.Linq;
using NUnit.Framework;
using NUnit.Framework.Interfaces;
using WTG.DevTools.TestFramework;

namespace CargoWise.eServices.Authentication.IntegrationTests
{
	public class WithAuthenticationServiceAttribute : WithWebApplicationAttribute
	{
		const string applicationName = "AuthenticationWebService";

		public static WithAuthenticationServiceAttribute Current => 
			(WithAuthenticationServiceAttribute)TestContext.CurrentContext.Test.Properties.Get(applicationName);

		protected override string RelativeBuildPath => "WebService";

		protected override string ApplicationName => applicationName;

		protected override bool EnableWindowsAuthentication => true;

		public string BaseUrl => GetUrl("Authentication/");
		public string HealthCheckUrl => GetUrl("wtg/status");

		protected override void InitializeWebConfig(XDocument doc)
		{
		}

		public override void BeforeTest(ITest test)
		{
			base.BeforeTest(test);

			ConfigurationManager.AppSettings["CargoWise.eServices.Authentication.ServiceClient.Endpoint"] =
				WithAuthenticationServiceAttribute.Current.BaseUrl;
			ConfigurationManager.AppSettings["CargoWise.eServices.Authentication.HealthCheck.Http.Endpoint"] =
				WithAuthenticationServiceAttribute.Current.HealthCheckUrl;

			File.Copy(
				Path.Combine(TestContext.CurrentContext.TestDirectory, "IntegrationTests.log4net.config"), 
				Path.Combine(DeploymentPath, "log4net.config"),
				overwrite: true);
		}
	}
}
