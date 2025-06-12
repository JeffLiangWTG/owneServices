using System;
using System.Configuration;
using System.Xml.Linq;
using System.Xml.XPath;
using NUnit.Framework;
using NUnit.Framework.Interfaces;
using WTG.DevTools.TestFramework;

namespace CargoWise.eHub.Portal.IntegrationTests.Attributes
{
	public class WithPortalServiceAttribute : WithWebApplicationAttribute
	{
		const string applicationName = "Portal";

		public WithPortalServiceAttribute()
			: base()
		{
		}

		protected override string RelativeBuildPath => applicationName;
		public static WithPortalServiceAttribute Current => (WithPortalServiceAttribute)TestContext.CurrentContext.Test.Properties.Get(applicationName);

		protected override string ApplicationName => applicationName;

		bool IsLocalDeployment => ServerName == "localhost";

		Exception SetupException { get; set; }

		protected override void InitializeWebConfig(XDocument doc)
		{
			doc.XPathSelectElement("//common/logging//arg[@key='configFile']")
				.SetAttributeValue("value", @"~\nologging.log4net.config");
		}

		public override void BeforeTest(ITest test)
		{
			try
			{
				// If we're testing against a remote deployment then we don't want to run the base method
				// which deploys the service locally
				if (IsLocalDeployment)
				{
					base.BeforeTest(test);
				}

				test.Properties.Set(ApplicationName, this);
				SetupException = null;
			}
			catch (Exception ex)
			{
				SetupException = ex;
			}
		}

		public override void AfterTest(ITest test)
		{
			try
			{
				if (IsLocalDeployment)
				{
					base.AfterTest(test);
				}
			}
			catch when (SetupException != null)
			{
			}
		}

		public Uri GetHttpEndPoint(string route)
		{
			string EndpointPath = IsLocalDeployment ? $"/{route}" : $"/{ApplicationName}/{route}";
			return GetHttpUri(EndpointPath);
		}
	}
}
