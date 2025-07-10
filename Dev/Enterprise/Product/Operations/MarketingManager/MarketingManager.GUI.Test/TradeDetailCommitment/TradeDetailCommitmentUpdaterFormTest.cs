using System;
using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MarketingManager.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.MarketingManager.GUI.Testing
{
	[TestedType(typeof(TradeDetailCommitmentUpdaterForm))]
	class TradeDetailCommitmentUpdaterFormTest : ZFormBasherTest
	{
		protected override Form GetFormToBashCore()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var opportunity = org.SalesOpportunities.AddNew();
			var convertor = new TradeDetailCommitmentUpdater(opportunity);
			return new TradeDetailCommitmentUpdaterForm(convertor);
		}

		public void TestShowFormCancel()
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
			detail1.ProspectDetail.PAP_ExpectedTradeStartDate = ZDate.Today;

			var tradeLane2 = org.SalesCollection.AddNew();
			tradeLane2.OW_MP_Product = forwardingProduct.PK;
			var detail2 = tradeLane2.TradeDetails.AddNew();
			detail2.ProspectDetail.PAP_ExpectedTradeStartDate = ZDate.Today;

			opportunity.AssociatedTradeLanesPivots.AddPivotFor(tradeLane1);
			opportunity.AssociatedTradeLanesPivots.AddPivotFor(tradeLane2);
			opportunity.AssociatedTradeLanesPivots.AddPivotFor(detail1);
			opportunity.AssociatedTradeLanesPivots.AddPivotFor(detail2);

			Factory.Save();

			var convertor = new TradeDetailCommitmentUpdater(opportunity);
			using (var form = new TradeDetailCommitmentUpdaterForm(convertor))
			{
				form.Show();
				var cancelButton = (ZButton)form.Controls.Find("cancelButton", searchAllChildren: true).FirstOrDefault();
				cancelButton.PerformClick();
				AssertEquals(DialogResult.None, form.DialogResult);
				AssertEquals(OpportunityTradeStatus.Codes.Active, detail1.PA_Status);
				AssertEquals(OpportunityTradeStatus.Codes.Active, detail2.PA_Status);
			}
		}

		public void TestShowFormConfirm()
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
			detail1.ProspectDetail.PAP_ExpectedTradeStartDate = ZDate.Today;

			var tradeLane2 = org.SalesCollection.AddNew();
			tradeLane2.OW_MP_Product = forwardingProduct.PK;
			var detail2 = tradeLane2.TradeDetails.AddNew();
			detail2.ProspectDetail.PAP_ExpectedTradeStartDate = ZDate.Today;

			opportunity.AssociatedTradeLanesPivots.AddPivotFor(tradeLane1);
			opportunity.AssociatedTradeLanesPivots.AddPivotFor(tradeLane2);
			opportunity.AssociatedTradeLanesPivots.AddPivotFor(detail1);
			opportunity.AssociatedTradeLanesPivots.AddPivotFor(detail2);

			Factory.Save();

			opportunity.P8_Status = "CCC";

			var convertor = new TradeDetailCommitmentUpdater(opportunity);
			using (var form = new TradeDetailCommitmentUpdaterForm(convertor))
			{
				form.Show();
				var confirmButton = (ZButton)form.Controls.Find("confirmButton", searchAllChildren: true).FirstOrDefault();
				confirmButton.PerformClick();
				AssertEquals(DialogResult.OK, form.DialogResult);
				AssertEquals(OpportunityTradeStatus.Codes.Successful, detail1.PA_Status);
				AssertEquals(OpportunityTradeStatus.Codes.Successful, detail2.PA_Status);
			}
		}
	}
}
