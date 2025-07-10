using System;
using System.Web;
using System.Web.UI.WebControls;
using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.Web.Business.Testing;
using Enterprise.ZArchitecture.Web.GUI;
using Enterprise.ZArchitecture.Web.GUI.WebControls;

namespace Enterprise.Tracking.Web.Testing
{
	sealed class ErrorTest : TestCaseWithFactory
	{
		[HttpContextEnabledTest]
		public void TestInvalidQuery()
		{
			HttpContext.Current.Request.QueryString["invalidQuery"] = "true";

			var page = new ErrorPageForTest();
			page.OnLoad();

			AssertEquals("If you directly typed this address please check for typos.\r\nIf you copied & pasted this address please check you included the entire address.", page.MessageDescriptionForTest.Text);
		}

		class ErrorPageForTest : Error
		{
			protected override ZGlobal GetNewTestGlobal()
			{
				return new TestGlobal();
			}

			public ErrorPageForTest()
			{
				HomeLink = new HyperLink();
				PageTitle = new ZTextLabel();
				MessageDescription = new Literal();
			}

			public Literal MessageDescriptionForTest => MessageDescription;

			public void OnLoad()
			{
				OnLoad(EventArgs.Empty);
			}
		}
	}
}
