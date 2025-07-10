using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.MarketingManager.GUI.Testing
{
	class GenerateSpotQuoteFromTradeDetailControllerTest : TestCaseWithFactory
	{
		public void TestShowNewForm()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var opportunity = org.SalesOpportunities.AddNew();
			Factory.Save();

			var controller = new GenerateSpotQuoteFromTradeDetailControllerForTest();
			try
			{
				controller.ShowNewForm(Factory.New<OrgTradeDetail>(), opportunity);

				AssertNotNull("Form was created", controller.LastFormShown);
				AssertEquals("Correct Form", "QuotedBookingForm", controller.LastFormShown.GetType().Name);

				var spotQuote = controller.LastFormShown.BusinessEntity as ISalesRelationActivity;

				AssertNotNull(spotQuote);
				AssertContainsExactElementsInAnyOrder(BusinessObjectEqualityComparer<BusinessObject>.PKOnlyComparer, new[] { opportunity }, spotQuote.RelatedParentActivityPivotCollection.Activities.Cast<BusinessObject>());
				AssertEquals(org.PK, spotQuote.Client.Identifier);
			}
			finally
			{
				controller.LastFormShown.Dispose();
			}
		}
	}
}
