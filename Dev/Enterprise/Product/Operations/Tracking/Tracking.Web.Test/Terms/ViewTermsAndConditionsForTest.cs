using System;
using Enterprise.ZArchitecture.Web.GUI;
using Enterprise.ZArchitecture.Web.GUI.WebControls;

namespace Enterprise.Tracking.Web.Testing
{
	sealed class ViewTermsAndConditionsForTest : ViewTermsAndConditions
	{
		protected override ZGlobal GetNewTestGlobal()
		{
			return new TestGlobal();
		}

		public ViewTermsAndConditionsForTest()
		{
			TermsAndConditionsText = new ZTextLabel();
		}

		public ZTextLabel TermsAndConditionsText_ForTest => TermsAndConditionsText;

		public void OnLoad_ForTest()
		{
			OnLoad(EventArgs.Empty);
		}
	}
}
