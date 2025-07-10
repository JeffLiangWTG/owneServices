using System;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Web.GUI;

namespace Enterprise.Tracking.Web.Testing
{
	sealed class TestPageWithNullDataSourceForTest : BasePageWithAuthorisation
	{
		protected override ZGlobal GetNewTestGlobal()
		{
			return new TestGlobal();
		}

		public TestPageWithNullDataSourceForTest()
		{
			AuthorisedContent = new System.Web.UI.HtmlControls.HtmlGenericControl("DIV");
			UnauthorisedDiv = new System.Web.UI.HtmlControls.HtmlGenericControl("DIV");
			UnauthorisedLabel = new ZArchitecture.Web.GUI.WebControls.ZTextLabel();
		}

		protected override BusinessObject GetNewDataSource()
		{
			return null;
		}

		public void OnLoadForTest()
		{
			this.OnLoad(EventArgs.Empty);
		}

		protected override bool CanAccessAuthorisedContent => true;
	}
}
