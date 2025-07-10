using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using Enterprise.MarketingManager.Business;
using Enterprise.MarketingManager.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MarketingManager.GUI
{
	public class ImportRelatedActivityPromptUserTradeDetailDeciderTest : TestCaseWithFactory
	{
		public void TestGetDecision()
		{
			var shpProduct = Factory.LoadFromNaturalKey<OrgSalesProduct>(OrgSalesProductSchema.MP_Code, SystemDefinedSalesProductList.Codes.ForwardingShipment);
			var brkProduct = Factory.LoadFromNaturalKey<OrgSalesProduct>(OrgSalesProductSchema.MP_Code, SystemDefinedSalesProductList.Codes.CustomsBrokerage);
			var whsProduct = Factory.LoadFromNaturalKey<OrgSalesProduct>(OrgSalesProductSchema.MP_Code, SystemDefinedSalesProductList.Codes.Warehouse);
			AssertEquals("Precondition", true, shpProduct.IsAutoGenerateSpotQuoteSupported);
			AssertEquals("Precondition", false, brkProduct.IsAutoGenerateSpotQuoteSupported);
			AssertEquals("Precondition", false, whsProduct.IsAutoGenerateSpotQuoteSupported);

			var org = Factory.NewWithValidTestData<OrgHeader>();
			var opportunity = org.SalesOpportunities.AddNew();
			var salesHeaderCollection = (SalesHeaderCollection)opportunity.ActualAndProspectiveSalesHeaderCollection;
			var shpSalesHeader = salesHeaderCollection.AddNew(shpProduct);
			var shpSales1 = shpSalesHeader.EntitySalesCollectionProductView.AddNew();
			var shpTradeDetail1A = shpSales1.EntityTradeDetailsCollection.AddNew();
			var shpTradeDetail1B = shpSales1.EntityTradeDetailsCollection.AddNew();
			var shpSales2 = shpSalesHeader.EntitySalesCollectionProductView.AddNew();
			var shpTradeDetail2A = shpSales2.EntityTradeDetailsCollection.AddNew();
			var shpTradeDetail2B = shpSales2.EntityTradeDetailsCollection.AddNew();

			foreach (var nonSpotQuoteSupportedProduct in new[] { brkProduct, whsProduct })
			{
				var salesHeader = salesHeaderCollection.AddNew(nonSpotQuoteSupportedProduct);
				var sales1 = salesHeader.EntitySalesCollectionProductView.AddNew();
				sales1.EntityTradeDetailsCollection.AddNew();
				sales1.EntityTradeDetailsCollection.AddNew();
			}

			Factory.Save();

			try
			{
				IEnumerable<EntityTradeDetailWrapper> actualTradeDetails = null;

				ZFormModaliser.SetDelegateToCallBeforeShowingFormsOrDialogs((form) =>
				{
					var tradeDetailSelectionForm = (TradeDetailSelectionForm)form;
					tradeDetailSelectionForm.SelectedTradeDetail = shpTradeDetail2A;
					actualTradeDetails = tradeDetailSelectionForm.BusinessEntity.Cast<EntityTradeDetailWrapper>();
				});

				using (var form = new ZForm())
				{
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);
					var decider = new ImportRelatedActivityPromptUserTradeDetailDecider(form);
					var result = decider.GetDecision(opportunity);
					using (var lastForm = ZFormModaliser.LastFormShownDialogForTest)
					{
						AssertType(typeof(TradeDetailSelectionForm), lastForm);

						AssertEquals(true, ((TradeDetailSelectionForm)lastForm).AllowContinueWithoutSelection);

						AssertContainsExactElementsInAnyOrder(
							new[]
							{
								shpTradeDetail1A,
								shpTradeDetail1B,
								shpTradeDetail2A,
								shpTradeDetail2B
							},
							actualTradeDetails);
					}

					AssertEquals(shpTradeDetail2A, result.SelectedTradeDetail);
					AssertEquals(false, result.Cancelled);
				}
			}
			finally
			{
				ZFormModaliser.ClearDelegateToCallBeforeShowingFormsOrDialogs();
			}
		}

		public void TestGetDecisionWithNoTradeDetails()
		{
			var shpProduct = Factory.LoadFromNaturalKey<OrgSalesProduct>(OrgSalesProductSchema.MP_Code, SystemDefinedSalesProductList.Codes.ForwardingShipment);
			var brkProduct = Factory.LoadFromNaturalKey<OrgSalesProduct>(OrgSalesProductSchema.MP_Code, SystemDefinedSalesProductList.Codes.CustomsBrokerage);
			var whsProduct = Factory.LoadFromNaturalKey<OrgSalesProduct>(OrgSalesProductSchema.MP_Code, SystemDefinedSalesProductList.Codes.Warehouse);
			AssertEquals("Precondition", true, shpProduct.IsAutoGenerateSpotQuoteSupported);
			AssertEquals("Precondition", false, brkProduct.IsAutoGenerateSpotQuoteSupported);
			AssertEquals("Precondition", false, whsProduct.IsAutoGenerateSpotQuoteSupported);

			var org = Factory.NewWithValidTestData<OrgHeader>();
			var opportunity = org.SalesOpportunities.AddNew();
			var salesHeaderCollection = (SalesHeaderCollection)opportunity.ActualAndProspectiveSalesHeaderCollection;
			var shpSalesHeader = salesHeaderCollection.AddNew(shpProduct);
			shpSalesHeader.EntitySalesCollectionProductView.AddNew();
			shpSalesHeader.EntitySalesCollectionProductView.AddNew();

			foreach (var nonSpotQuoteSupportedProduct in new[] { brkProduct, whsProduct })
			{
				var salesHeader = salesHeaderCollection.AddNew(nonSpotQuoteSupportedProduct);
				salesHeader.EntitySalesCollectionProductView.AddNew();
			}

			Factory.Save();

			using (var form = new ZForm())
			{
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);
				var decider = new ImportRelatedActivityPromptUserTradeDetailDecider(form);
				var result = decider.GetDecision(opportunity);
				AssertNull(ZFormModaliser.LastFormShownDialogForTest);
				AssertEquals(false, result.Cancelled);
			}
		}
	}
}
