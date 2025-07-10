using System;
using System.Collections.Specialized;
using Enterprise.Environment;
using Enterprise.Tracking.Business;
using Enterprise.ZArchitecture.Web.Business;
using Enterprise.ZArchitecture.Web.GUI;
using Enterprise.ZArchitecture.Web.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Tracking.Web.Testing
{
	public class TestGlobal : Global
	{
		public void ApplicationPreSendRequestHeadersForTesting() => Application_PreSendRequestHeaders();

		protected override NameValueCollection Headers => HeadersForTesting;

		public NameValueCollection HeadersForTesting => headers ?? (headers = new NameValueCollection());
		NameValueCollection headers;

		public void ApplicationStartForTesting(object sender, EventArgs e)
		{
			Application_Start(sender, e);
		}

		public EnvProvider WebEnvProviderForTesting => WebEnvProvider;

		public void ApplicationErrorForTesting(object sender, EventArgs e)
		{
			Application_Error(sender, e);
		}

		protected override bool ExceptionShouldBeHandled(Exception unhandledException)
		{
			if (ExceptionShouldHandledOverrideForTesting != null)
			{
				return ExceptionShouldHandledOverrideForTesting();
			}

			return base.ExceptionShouldBeHandled(unhandledException);
		}

		public Func<bool> ExceptionShouldHandledOverrideForTesting;

		public override string DefaultPage
		{
			get { return "/Default.aspx"; }
		}

		public override string ApplicationRoot
		{
			get { return "/"; }
		}

		public override string UrlForRegistryItems
		{
			get { return "webtracker.com/"; }
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1051:Do not use BaseSourcePath", Justification = "Baseline")]
		public override string MapPath(string path)
		{
			return TestCase.BaseSourcePath + @"Enterprise\Product\Operations\Tracking\Tracking.Web" + path;
		}

		public override WebUser SiteUser
		{
			get
			{
				if (fSiteUser == null)
				{
					fSiteUser = new TrackingSiteUser();
				}
				return fSiteUser;
			}
		}
		TrackingSiteUser fSiteUser;

		public void SetSiteUser(TrackingSiteUser user) => fSiteUser = user;

		public override string HomePage
		{
			get { return @"http://www.test.cargowise.com/"; }
		}

		protected override ZGlobalConfig GetNewGlobalConfig()
		{
			return new ZTestGlobalConfig();
		}
	}
}
