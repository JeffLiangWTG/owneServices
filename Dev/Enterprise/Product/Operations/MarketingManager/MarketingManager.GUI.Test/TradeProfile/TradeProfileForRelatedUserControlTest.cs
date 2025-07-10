using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Windows.UI.Testing;
using Enterprise.Environment;
using Enterprise.MarketingManager.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.GUI;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MarketingManager.GUI.Testing
{
	class TradeProfileForRelatedUserControlTest : TestCaseWithFactory
	{
		#region NewProsectValueButton

		public void TestNewProspectValueButton()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var opportunity = org.SalesOpportunities.AddNew();
			opportunity.FillWithValidTestData();
			Factory.Save();

			var salesProducts = Factory.Load<OrgSalesProduct>(new ZQuery());

			using (var form = new ZForm(opportunity))
			using (var control = new TradeProfileForRelatedUserControlForTest())
			{
				form.Controls.Add(control);
				form.Show();
				AssertEquals(salesProducts.Length, control.NewProspectValueButton_Exposed.DropDown.Items.Count);

				control.NewProspectValueButton_Exposed.DropDown.Items[0].PerformClick();

				using (var lastShownForm = ZFormModaliser.LastFormShownForTest)
				{
					AssertType(typeof(OpportunityProspectiveTradeProfileForm), lastShownForm);
				}
			}

			var salesHeader = ((SalesHeaderCollection)opportunity.ActualAndProspectiveSalesHeaderCollection).AddNew(salesProducts[0]);
			salesHeader.EntitySalesCollectionProductView.AddNew();
			Factory.Save();

			using (var form = new ZForm(opportunity))
			using (var control = new TradeProfileForRelatedUserControlForTest())
			{
				form.Controls.Add(control);
				form.Show();
				AssertEquals(salesProducts.Length, control.NewProspectValueButton_Exposed.DropDown.Items.Count);
			}
		}

		#endregion

		#region EditProspectValuesButton

		public void TestEditProspectValuesButton()
		{
			var product1 = Factory.NewWithValidTestData<OrgSalesProduct>();
			product1.MP_Code = "AAA";
			var product2 = Factory.NewWithValidTestData<OrgSalesProduct>();
			product2.MP_Code = "BBB";
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var opportunity = org.SalesOpportunities.AddNew();
			opportunity.FillWithValidTestData();
			var salesHeader = ((SalesHeaderCollection)opportunity.ProspectiveSalesHeaderCollection).AddNew(product2);
			salesHeader.EntitySalesCollectionProductView.AddNew();
			Factory.Save();

			using (var form = new ZForm(opportunity))
			using (var control = new TradeProfileForRelatedUserControlForTest())
			{
				form.Controls.Add(control);
				form.Show();

				try
				{
					ZFormModaliser.ShowDialogsInTest = true;

					AssertEquals("Precondition", true, control.EditProspectValuesButton_Exposed.Enabled);
					control.EditProspectValuesButton_Exposed.PerformClick();

					var lastShownForm = (ZForm)ZFormModaliser.LastFormShownForTest;
					AssertType(typeof(OpportunityProspectiveTradeProfileForm), lastShownForm);

					var factoryOfNewForm = lastShownForm.BusinessEntity.Factory;
					var opportunityInNewForm = factoryOfNewForm.Load<OrgOpportunity>(opportunity.PK);
					var product1InNewForm = factoryOfNewForm.Load<OrgSalesProduct>(product1.PK);
					salesHeader = ((SalesHeaderCollection)opportunityInNewForm.ProspectiveSalesHeaderCollection).AddNew(product1InNewForm);
					salesHeader.EntitySalesCollectionProductView.AddNew();
					factoryOfNewForm.Save();

					lastShownForm.Close();

					ZFormModaliser.LastFormShownForTest = null;
				}
				finally
				{
					ZFormModaliser.ShowDialogsInTest = false;
				}

				AssertEquals("Should have refreshed sales headers", 2, control.SalesValueAnalysisCollection.Count);
				var newSalesHeader = control.SalesValueAnalysisCollection.Cast<OpportunitySalesValueAnalysis>().First(x => x.SalesHeader.SalesProduct.PK == product1.PK).SalesHeader;
				AssertEquals("Should have refreshed sales", 1, newSalesHeader.EntitySales.Count());
			}
		}

		public void TestEditProspectValuesButton_ShowErrorPopupWhenNotSaved()
		{
			var org = Factory.New<OrgHeader>();
			var opportunity = org.SalesOpportunities.AddNew();
			opportunity.ValueItems.AddNew();

			using (var form = new ZForm(opportunity))
			using (var control = new TradeProfileForRelatedUserControlForTest())
			{
				form.Controls.Add(control);
				form.Show();

				AssertEquals("Precondition", true, control.EditProspectValuesButton_Exposed.Enabled);
				control.EditProspectValuesButton_Exposed.PerformClick();

				AssertEquals("Cannot Add / Edit Estimate Values", UnitTestUserNotification.Instance.LastMessage.Caption);
				AssertEquals("Cannot add or edit estimate values until Opportunity is saved.", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertNull(ZFormModaliser.LastFormShownForTest);
			}
		}

		public void TestEditProspectValuesButton_ShowErrorPopupWhenHasBeenDeleted()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var opportunity = org.SalesOpportunities.AddNew();
			opportunity.FillWithValidTestData();
			opportunity.P8_OpportunityID = "O00002022";
			var product = Factory.NewWithValidTestData<OrgSalesProduct>();
			product.MP_Code = "AAA";
			product.MP_Name = "AAA";
			var salesHeader = ((SalesHeaderCollection)opportunity.ProspectiveSalesHeaderCollection).AddNew(product);
			salesHeader.EntitySalesCollectionProductView.AddNew();
			Factory.Save();

			using (var form = new ZForm(opportunity))
			using (var control = new TradeProfileForRelatedUserControlForTest())
			{
				form.Controls.Add(control);
				form.Show();

				var anotherFactory = new BusinessObjectFactory();
				using (GetFactoryIsolater(anotherFactory))
				{
					var opportunityInOtherFactory = anotherFactory.Load<OrgOpportunity>(opportunity.PK);
					opportunityInOtherFactory.Delete();
					anotherFactory.Save();
				}

				AssertEquals("Precondition", true, control.EditProspectValuesButton_Exposed.Enabled);
				control.EditProspectValuesButton_Exposed.PerformClick();

				AssertEquals("Cannot Add / Edit Estimate Values", UnitTestUserNotification.Instance.LastMessage.Caption);
				AssertEquals("Opportunity (O00002022) has been deleted.", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertNull(ZFormModaliser.LastFormShownForTest);
			}
		}

		public void TestReadOnlyButtons()
		{
			var org = Factory.New<OrgHeader>();
			var opportunity = org.SalesOpportunities.AddNew();
			opportunity.ValueItems.AddNew();

			using (var form = new ZForm(opportunity))
			using (var control = new TradeProfileForRelatedUserControlForTest())
			{
				form.Controls.Add(control);
				form.SetDataBinding(opportunity, "");
				control.SetDataBinding(opportunity, "");
				form.Show();
				Application.DoEvents();
				AssertEquals(true, control.EditProspectValuesButton_Exposed.Enabled);
				AssertEquals(true, control.NewProspectValueButton_Exposed.Enabled);
			}

			org.SetReadOnlyIncludingChildren(true);
			using (var form = new ZForm(opportunity))
			using (var control = new TradeProfileForRelatedUserControlForTest())
			{
				form.Controls.Add(control);
				form.SetDataBinding(opportunity, "");
				control.SetDataBinding(opportunity, "");
				form.Show();
				Application.DoEvents();
				AssertEquals(false, control.EditProspectValuesButton_Exposed.Enabled);
				AssertEquals(false, control.NewProspectValueButton_Exposed.Enabled);
			}
		}

		public void TestRefreshSalesValueAnalysisCollection()
		{
			var product = Factory.NewWithValidTestData<OrgSalesProduct>();
			var org = Factory.NewWithValidTestData<OrgHeader>();

			var opportunity = org.SalesOpportunities.AddNew();
			opportunity.FillWithValidTestData();

			var salesHeader = ((SalesHeaderCollection)opportunity.ProspectiveSalesHeaderCollection).AddNew(product);
			salesHeader.EntitySalesCollectionProductView.AddNew();

			AssertEquals("Pre-condition", 1, opportunity.ProspectiveSalesHeaderCollection.Count);

			using (var form = new ZForm(opportunity))
			using (var control = new TradeProfileForRelatedUserControlForTest())
			{
				form.Controls.Add(control);
				form.Show();

				control.RefreshSalesValueAnalysisCollection();
				AssertEquals("Collection should still contain 1 element", 1, opportunity.ProspectiveSalesHeaderCollection.Count);
			}
		}

		public void TestRefreshCalculatedEstimatedValues()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var opportunity = org.SalesOpportunities.AddNew();
			var product = Factory.LoadFromNaturalKey<OrgSalesProduct>(OrgSalesProductSchema.MP_Code, "SHP");

			var sales = org.SalesCollection.AddNew();
			sales.OW_MP_Product = product.PK;
			sales.OW_IsTraded = false;
			sales.OW_OH_Primary = org.PK;

			var tradeDetail = sales.TradeDetails.AddNew();
			tradeDetail.PA_Status = OpportunityTradeStatus.Codes.Active;
			tradeDetail.ProspectDetail.PAP_RecurrenceType = OrgTradeProspectRecurrenceTypeList.Codes.Yearly;
			tradeDetail.CurrentProspectPeriod.PAS_RX_NKCurrency = "AUD";
			tradeDetail.CurrentProspectPeriod.PAS_RateOffered = 10m;
			tradeDetail.CurrentProspectPeriod.PAS_EstimatedProfit = 1000m;

			opportunity.AssociatedTradeLanesPivots.AddPivotFor(sales);
			opportunity.AssociatedTradeLanesPivots.AddPivotFor(tradeDetail);
			opportunity.P8_Status = "WON";

			Factory.Save();

			using (var form = new OpportunityForm(opportunity))
			using (var control = new TradeProfileForRelatedUserControlForTest())
			{
				form.Controls.Add(control);
				form.Show();

				var opportunityManagementDetailsControl = ControlTestHelper.FindControls<OpportunityManagementDetailsControl>(form).Single();
				var opportunityTabControl = ControlTestHelper.FindControls<ZTemplateTabControl>(opportunityManagementDetailsControl).Single(x => x.Name == "OpportunityTabControl");
				var valueAnalysisTabPage = ControlTestHelper.FindControls<ZTabPagePlugIn>(opportunityTabControl).Single(x => x.Name == "ValueAnalysisTabPage");

				opportunityTabControl.SelectedIndex = opportunityTabControl.Controls.IndexOf(valueAnalysisTabPage);
				var tradeProfileForRelatedUserControl = valueAnalysisTabPage.PlugIn.UserControl as TradeProfileForRelatedUserControl;

				AssertEquals("Pre-condition - PipelineValue", "1,000.00", opportunityManagementDetailsControl.Controls.Find("PipelineValueCalcEdit", true).Single().Text);
				AssertEquals("Pre-condition - CommittedAnnualValue", "0.00", opportunityManagementDetailsControl.Controls.Find("CommittedValueCalcEdit", true).Single().Text);
				AssertEquals("Pre-condition - UnsuccessfulValue", "0.00", opportunityManagementDetailsControl.Controls.Find("UnsuccessfulValueCalcEdit", true).Single().Text);

				tradeDetail.PA_Status = OpportunityTradeStatus.Codes.Unsuccessful;
				tradeProfileForRelatedUserControl.UpdateProspectStatusButton.PerformClick();

				AssertEquals("PipelineValue", "0.00", opportunityManagementDetailsControl.Controls.Find("PipelineValueCalcEdit", true).Single().Text);
				AssertEquals("CommittedAnnualValue", "0.00", opportunityManagementDetailsControl.Controls.Find("CommittedValueCalcEdit", true).Single().Text);
				AssertEquals("UnsuccessfulValue", "1,000.00", opportunityManagementDetailsControl.Controls.Find("UnsuccessfulValueCalcEdit", true).Single().Text);

				opportunity.P8_Status = "CRT";
				tradeDetail.PA_Status = OpportunityTradeStatus.Codes.Active;
				opportunityManagementDetailsControl.TradeProfilePlugin.RefreshData();

				AssertEquals("PipelineValue", "1,000.00", opportunityManagementDetailsControl.Controls.Find("PipelineValueCalcEdit", true).Single().Text);
				AssertEquals("CommittedAnnualValue", "0.00", opportunityManagementDetailsControl.Controls.Find("CommittedValueCalcEdit", true).Single().Text);
				AssertEquals("UnsuccessfulValue", "0.00", opportunityManagementDetailsControl.Controls.Find("UnsuccessfulValueCalcEdit", true).Single().Text);

				opportunity.P8_Status = "WON";
				tradeDetail.PA_Status = OpportunityTradeStatus.Codes.Unsuccessful;
				opportunityManagementDetailsControl.TradeProfilePlugin.RefreshData();

				AssertEquals("PipelineValue", "0.00", opportunityManagementDetailsControl.Controls.Find("PipelineValueCalcEdit", true).Single().Text);
				AssertEquals("CommittedAnnualValue", "0.00", opportunityManagementDetailsControl.Controls.Find("CommittedValueCalcEdit", true).Single().Text);
				AssertEquals("UnsuccessfulValue", "1,000.00", opportunityManagementDetailsControl.Controls.Find("UnsuccessfulValueCalcEdit", true).Single().Text);
			}
		}

		public void TestButtonsDisabledOnSecurityNotAllowed()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var opportunity = org.SalesOpportunities.AddNew();
			var product = Factory.LoadFromNaturalKey<OrgSalesProduct>(OrgSalesProductSchema.MP_Code, "SHP");

			var sales = org.SalesCollection.AddNew();
			sales.OW_MP_Product = product.PK;
			sales.OW_IsTraded = false;
			sales.OW_OH_Primary = org.PK;

			var tradeDetail = sales.TradeDetails.AddNew();
			tradeDetail.PA_Status = OpportunityTradeStatus.Codes.Active;
			tradeDetail.ProspectDetail.PAP_RecurrenceType = OrgTradeProspectRecurrenceTypeList.Codes.Yearly;
			tradeDetail.CurrentProspectPeriod.PAS_RX_NKCurrency = "AUD";
			tradeDetail.CurrentProspectPeriod.PAS_RateOffered = 10m;
			tradeDetail.CurrentProspectPeriod.PAS_EstimatedProfit = 1000m;

			opportunity.AssociatedTradeLanesPivots.AddPivotFor(sales);
			opportunity.AssociatedTradeLanesPivots.AddPivotFor(tradeDetail);
			opportunity.P8_Status = "WON";

			Factory.Save();

			Env.Security.ClientIntelligenceModifyTradeProfile.IsAllowed = true;

			using (var form = new OpportunityForm(opportunity))
			{
				form.Show();

				var opportunityManagementDetailsControl = ControlTestHelper.FindControls<OpportunityManagementDetailsControl>(form).Single();
				var opportunityTabControl = ControlTestHelper.FindControls<ZTemplateTabControl>(opportunityManagementDetailsControl).Single(x => x.Name == "OpportunityTabControl");
				var valueAnalysisTabPage = ControlTestHelper.FindControls<ZTabPagePlugIn>(opportunityTabControl).Single(x => x.Name == "ValueAnalysisTabPage");
				opportunityTabControl.SelectedTab = valueAnalysisTabPage;
				
				AssertEquals("Edit Estimates", true, form.GetEditProspectValuesButton().Enabled);
				AssertEquals("Add Estimates", true, form.GetNewProspectValueButton().Enabled);
				AssertEquals("Update Status", true, form.GetUpdateProspectStatusButton().Enabled);

				form.GetSalesValueAnalysisControl().Grid.PerformMouseDoubleClickForTest(0);

				AssertNotNull(ZFormModaliser.LastFormShownForTest);
			}

			ZFormModaliser.LastFormShownForTest = null;

			Env.Security.ClientIntelligenceModifyTradeProfile.IsAllowed = false;

			using (var form = new OpportunityForm(opportunity))
			{
				form.Show();

				var opportunityManagementDetailsControl = ControlTestHelper.FindControls<OpportunityManagementDetailsControl>(form).Single();
				var opportunityTabControl = ControlTestHelper.FindControls<ZTemplateTabControl>(opportunityManagementDetailsControl).Single(x => x.Name == "OpportunityTabControl");
				var valueAnalysisTabPage = ControlTestHelper.FindControls<ZTabPagePlugIn>(opportunityTabControl).Single(x => x.Name == "ValueAnalysisTabPage");
				opportunityTabControl.SelectedTab = valueAnalysisTabPage;

				AssertEquals("Edit Estimates", false, form.GetEditProspectValuesButton().Enabled);
				AssertEquals("Add Estimates", false, form.GetNewProspectValueButton().Enabled);
				AssertEquals("Update Status", false, form.GetUpdateProspectStatusButton().Enabled);

				form.GetSalesValueAnalysisControl().Grid.PerformMouseDoubleClickForTest(0);

				AssertNull(ZFormModaliser.LastFormShownForTest);
			}
		}

		#endregion

		#region Implementation

		class TradeProfileForRelatedUserControlForTest : TradeProfileForRelatedUserControl
		{
			public ZToolStripButton EditProspectValuesButton_Exposed
			{
				get { return base.editProspectValuesButton; }
			}

			public ZToolStripDropDownButton NewProspectValueButton_Exposed
			{
				get { return base.newProspectValueButton; }
			}
		}

		#endregion
	}
}
