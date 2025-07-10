using System;
using System.Web.UI;
using Enterprise.Registry.Business;
using Enterprise.Tracking.Business;

namespace Enterprise.Tracking.Web.Testing
{
	sealed class ViewTermsAndConditionsTest : BaseTrackingPageTest
	{
		public void TestSiteTermsAndConditionsSetFromRegistry_NotEncoded()
		{
			var terms = $"Terms and conditions{System.Environment.NewLine}Hello";

			using (WebDataRegistry.Instance.WebTrackerSiteTermsAndConditions.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, terms))
			{
				var page = new ViewTermsAndConditionsForTest();
				page.OnLoad_ForTest();

				AssertEquals("Terms and conditions<br />Hello", page.TermsAndConditionsText_ForTest.Text);
			}
		}

		protected override string GetExpectedPageName() => WebTracker.Pages.ViewTermsAndConditions;

		protected override Control GetNewControl() => new ViewTermsAndConditionsForTest();
	}
}
