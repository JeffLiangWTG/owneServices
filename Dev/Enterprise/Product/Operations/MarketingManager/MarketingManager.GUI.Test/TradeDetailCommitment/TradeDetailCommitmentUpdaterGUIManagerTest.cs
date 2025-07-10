using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.MarketingManager.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MarketingManager.GUI.Testing
{
	public class TradeDetailCommitmentUpdaterGUIManagerTest : TestCaseWithFactory
	{
		public void TestShowTradeStatusConversionForm()
		{
			var forwardingProduct = Factory.LoadTop1<OrgSalesProduct>(new ZQuery(OrgSalesProductSchema.MP_Code, "SHP"));

			var oppStatusCollection = new OpportunityStatusCollection
			{
				{ "AAA", (NoResString)"Desc A", false, true, true, OpportunityTradeStatus.Codes.Active },
				{ "BBB", (NoResString)"Desc B", false, true, true, OpportunityTradeStatus.Codes.Unsuccessful },
				{ "CCC", (NoResString)"Desc C", false, true, true, OpportunityTradeStatus.Codes.Successful }
			};
			OrganisationsDataRegistry.Instance.OpportunityStatus.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, oppStatusCollection);

			var org = Factory.NewWithValidTestData<OrgHeader>();
			var opportunity = Factory.NewWithValidTestData<OrgOpportunity>();
			opportunity.P8_OH = org.PK;
			opportunity.P8_Status = "AAA";

			var tradeLane1 = org.SalesCollection.AddNew();
			tradeLane1.OW_MP_Product = forwardingProduct.PK;
			var detail1 = tradeLane1.TradeDetails.AddNew();

			var tradeLane2 = org.SalesCollection.AddNew();
			tradeLane2.OW_MP_Product = forwardingProduct.PK;
			var detail2 = tradeLane2.TradeDetails.AddNew();

			opportunity.AssociatedTradeLanesPivots.AddPivotFor(tradeLane1);
			opportunity.AssociatedTradeLanesPivots.AddPivotFor(tradeLane2);
			opportunity.AssociatedTradeLanesPivots.AddPivotFor(detail1);
			opportunity.AssociatedTradeLanesPivots.AddPivotFor(detail2);

			Factory.Save();

			var manager = new TradeDetailCommitmentUpdaterGUIManager();
			manager.ShowForm(opportunity, new EventArgs());
			using (var lastShownForm = ZFormModaliser.LastFormShownDialogForTest)
			{
				AssertEquals("", lastShownForm.Text);
				lastShownForm.Close();
			}
			ZFormModaliser.LastFormShownDialogForTest = null;
		}
	}
}
