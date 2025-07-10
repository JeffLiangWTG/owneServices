using System;
using System.Web.UI.HtmlControls;
using Enterprise.ZArchitecture.Web.GUI;
using Enterprise.ZArchitecture.Web.GUI.WebControls;

namespace Enterprise.Tracking.Web.Testing
{
	sealed class GlowRedirectPageForTest : GlowRedirect
	{
		protected override ZGlobal GetNewTestGlobal()
		{
			return new TestGlobal();
		}

		public GlowRedirectPageForTest()
		{
			UnauthorisedDiv = new HtmlGenericControl();
			UnauthorisedLabel = new ZTextLabel();
			AuthorisedContent = new HtmlGenericControl();
		}

		public void OnLoad()
		{
			OnLoad(EventArgs.Empty);
		}
	}
}
