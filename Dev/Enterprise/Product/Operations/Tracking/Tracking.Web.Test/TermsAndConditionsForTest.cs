using System;
using Enterprise.ZArchitecture.Web.GUI;
using Enterprise.ZArchitecture.Web.GUI.WebControls;

namespace Enterprise.Tracking.Web.Testing
{
	sealed class TermsAndConditionsForTest : TermsAndConditions
	{
		#region Overrides

		protected override ZGlobal GetNewTestGlobal()
		{
			return new TestGlobal();
		}

		protected override void OnInit(EventArgs e)
		{
			base.TermsAndConditionsText = new ZTextLabelNoEncode();
		}

		public new ZTextLabel TermsAndConditionsText
		{
			get { return base.TermsAndConditionsText; }
		}

		public string RedirectUrlForTest => RedirectUrl;

		#endregion

		public void OnLoadForTest()
		{
			OnInit(EventArgs.Empty);
			OnLoad(EventArgs.Empty);
		}

		public void IAgreeButtonCLickForTest()
		{
			IAgreeButton_Click(IAgreeButton, EventArgs.Empty);
		}

		public void IDisagreeButtonClickForTest()
		{
			IDisagreeButton_Click(IDisagreeButton, EventArgs.Empty);
		}
	}
}
