using System;
using System.Web;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using Enterprise.ZArchitecture.Web.Business.Testing;
using Enterprise.ZArchitecture.Web.GUI.WebControls;
using Enterprise.ZArchitecture.Web.GUI.WebControls.Testing;

namespace Enterprise.MarketingManager.WebVoting
{
	[HttpContextEnabledTest]
	class Submission_Test : ZPageTestCase
	{
		public void TestPageContainsExamJs()
		{
			AssertEquals(false, Page.ZClientScript.IsClientScriptBlockRegistered(Page.GetType(), "Exam"));
			Page.Page_LoadInternal(Page, EventArgs.Empty);

			AssertEquals(true, Page.ZClientScript.IsClientScriptBlockRegistered(Page.GetType(), "Exam"));
		}

		public void TestAbandonSessionOnPageLoad()
		{
			Assert("Pre-condition", !SessionStateContainer.IsAbandoned);

			Page.Page_LoadInternal(Page, EventArgs.Empty);
			Assert("Should be abandoned", SessionStateContainer.IsAbandoned);
		}

		public void TestDataSource_NotTransferredFromDefault()
		{
			Page.ContextInternal.Handler = new Login();
			Page.OnLoadInternal(EventArgs.Empty);
			AssertNull(Page.DataSource);
			AssertEquals("", Page.SubmissionMessageLabelInternal.InnerHtml);
		}

		public void TestShowLogOffLinkButton()
		{
			Assert("Should not show LogOff button", !Page.ShowLogOffLinkButtonInternal);
		}

		#region SubmissionPageForTest

		class SubmissionPageForTest : Submission
		{
			protected override HtmlForm GetForm(Control parent)
			{
				return new HtmlForm();
			}
			internal void OnLoadInternal(EventArgs e) => OnLoad(e);
			internal HtmlGenericControl SubmissionMessageLabelInternal => SubmissionMessageLabel;
			internal void SetSubmissionMessageLabelInternal(HtmlGenericControl control) => SubmissionMessageLabel = control;
			internal bool ShowLogOffLinkButtonInternal => ShowLogOffLinkButton;
			internal HttpContext ContextInternal => Context;
			internal void Page_LoadInternal(object sender, EventArgs e) => Page_Load(sender, e);
		}

		#endregion

		new SubmissionPageForTest Page
		{
			get { return (SubmissionPageForTest)base.Page; }
		}

		protected override ZPage GetNewZPage()
		{
			SubmissionPageForTest result = new SubmissionPageForTest();
			result.SetSubmissionMessageLabelInternal(new HtmlGenericControl());
			result.Controls.Add(result.SubmissionMessageLabelInternal);
			return result;
		}
	}
}
