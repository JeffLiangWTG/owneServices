using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.MarketingManager.Business;
using Enterprise.MarketingManager.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.MarketingManager.GUI.Testing
{
	class SalesBreakdownControlTest : TestCaseWithFactory
	{
		#region TradedSalesAnalysisTabPage

		public void TestTradedSalesAnalysisTabPage_OnlyVisibleForSystemDefinedProducts()
		{
			var systemDefinedProduct = Factory.New<OrgSalesProduct>();
			systemDefinedProduct.MP_IsSystemDefined = true;
			systemDefinedProduct.MP_Name = "AAA";
			var nonSystemDefinedProduct = Factory.New<OrgSalesProduct>();
			nonSystemDefinedProduct.MP_IsSystemDefined = false;
			nonSystemDefinedProduct.MP_Name = "BBB";

			var org = Factory.New<OrgHeader>();
			var salesHeaderCollection = (SalesHeaderCollection)org.ActualAndProspectiveSalesHeaderCollection;
			var systemDefinedSalesHeader = salesHeaderCollection.AddNew(systemDefinedProduct);
			var salesForSystemDefinedProduct = systemDefinedSalesHeader.EntitySalesCollectionProductView.AddNew();
			var nonSystemDefinedSalesHeader = salesHeaderCollection.AddNew(nonSystemDefinedProduct);
			var salesForNonSystemDefinedProduct = nonSystemDefinedSalesHeader.EntitySalesCollectionProductView.AddNew();
			var salesBreakdown = new SalesBreakdown(org);

			using (var form = new SalesBreakdownFormForTest(salesBreakdown))
			{
				form.Show();

				var control = form.SalesBreakdownControl;
				control.SalesHeaderControl_Exposed.Grid.SelectSingleElement(nonSystemDefinedSalesHeader);
				control.SalesHeaderControl_Exposed.Grid.SelectSingleElement(systemDefinedSalesHeader);
				AssertEquals(true, control.TradedSalesAnalysisTabPage_Exposed.TabVisible);

				control.SalesHeaderControl_Exposed.Grid.SelectSingleElement(nonSystemDefinedSalesHeader);
				AssertEquals(false, control.TradedSalesAnalysisTabPage_Exposed.TabVisible);

				control.SalesHeaderControl_Exposed.Grid.SelectSingleElement(systemDefinedSalesHeader);
				AssertEquals(true, control.TradedSalesAnalysisTabPage_Exposed.TabVisible);
			}
		}

		public void TestTradedSalesAnalysisTabPage_ShouldRefreshVisibilityOnSalesHeaderCollectionRefresh()
		{
			var org = Factory.New<OrgHeader>();
			var salesBreakdown = new SalesBreakdown(org);

			using (var form = new SalesBreakdownFormForTest(salesBreakdown))
			{
				form.Show();

				var control = form.SalesBreakdownControl;
				AssertEquals("Precondition", 0, control.SalesHeaderControl_Exposed.Grid.ListManager.Count);
				AssertEquals(false, control.TradedSalesAnalysisTabPage_Exposed.TabVisible);

				var shpProduct = Factory.LoadFromNaturalKey<OrgSalesProduct>(OrgSalesProductSchema.MP_Code, SystemDefinedSalesProductList.Codes.ForwardingShipment);
				var salesHeaderCollection = (SalesHeaderCollection)org.ActualAndProspectiveSalesHeaderCollection;
				var systemDefinedSalesHeader = salesHeaderCollection.AddNew(shpProduct);

				AssertEquals(1, control.SalesHeaderControl_Exposed.Grid.ListManager.Count);
				AssertEquals(true, control.TradedSalesAnalysisTabPage_Exposed.TabVisible);
			}
		}

		#endregion

		public void TestReadOnlyButtons()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var product = Factory.New<OrgSalesProduct>();
			product.MP_Code = "AAA";
			product.MP_Name = "AAA";
			var sales = org.SalesCollection.AddNew();
			sales.OW_MP_Product = product.PK;
			Factory.Save();

			using (var form = new ZForm(org))
			using (var control = new SalesBreakdownControlForTest())
			{
				var salesBreakdown = new SalesBreakdown(org);
				control.SetDataBinding(salesBreakdown, null);
				form.Controls.Add(control);
				form.Show();

				AssertEquals(true, control.ViewProspectValuesButton_Exposed.Enabled);
			}

			org.SetReadOnlyIncludingChildren(true);
			using (var form = new ZForm(org))
			using (var control = new SalesBreakdownControlForTest())
			{
				var salesBreakdown = new SalesBreakdown(org);
				control.SetDataBinding(salesBreakdown, null);
				form.Controls.Add(control);
				form.Show();

				AssertEquals(false, control.ViewProspectValuesButton_Exposed.Enabled);
			}
		}

		#region EditProspectValuesButton

		public void TestEditProspectValuesButton()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var product = Factory.New<OrgSalesProduct>();
			product.MP_Code = "AAA";
			product.MP_Name = "AAA";
			var sales = org.SalesCollection.AddNew();
			sales.OW_MP_Product = product.PK;
			Factory.Save();

			using (var form = new ZForm(org))
			using (var control = new SalesBreakdownControlForTest())
			{
				var salesBreakdown = new SalesBreakdown(org);
				control.SetDataBinding(salesBreakdown, null);
				form.Controls.Add(control);
				form.Show();

				control.ViewProspectValuesButton_Exposed.PerformClick();

				var lastShownForm = ZFormModaliser.LastFormShownForTest;
				AssertType(typeof(ProspectiveTradeProfileForm), lastShownForm);
				lastShownForm.Close();
				ZFormModaliser.LastFormShownForTest = null;
			}
		}

		public void TestEditProspectValuesButton_ShowErrorPopupWhenNotSaved()
		{
			var org = Factory.New<OrgHeader>();
			var product = Factory.New<OrgSalesProduct>();
			product.MP_Code = "AAA";
			product.MP_Name = "AAA";
			var salesHeaderCollection = (SalesHeaderCollection)org.ActualAndProspectiveSalesHeaderCollection;
			var salesHeader = salesHeaderCollection.AddNew(product);
			var sales = salesHeader.EntitySalesCollectionProductView.AddNew();
			sales.OW_MP_Product = product.PK;

			using (var form = new ZForm(org))
			using (var control = new SalesBreakdownControlForTest())
			{
				var salesBreakdown = new SalesBreakdown(org);
				control.SetDataBinding(salesBreakdown, null);
				form.Controls.Add(control);
				form.Show();

				AssertEquals("Precondition", true, control.ViewProspectValuesButton_Exposed.Enabled);
				control.ViewProspectValuesButton_Exposed.PerformClick();

				AssertEquals("Cannot Add / Edit Estimate Values", UnitTestUserNotification.Instance.LastMessage.Caption);
				AssertEquals("Cannot add or edit estimate values until Organization is saved.", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertNull(ZFormModaliser.LastFormShownForTest);
			}
		}

		public void TestEditProspectValuesButton_ShowErrorPopupWhenHasBeenDeleted()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_Code = "ORGXXX";
			var product = Factory.New<OrgSalesProduct>();
			product.MP_Code = "AAA";
			product.MP_Name = "AAA";
			var sales = org.SalesCollection.AddNew();
			sales.OW_MP_Product = product.PK;
			Factory.Save();

			using (var form = new ZForm(org))
			using (var control = new SalesBreakdownControlForTest())
			{
				var salesBreakdown = new SalesBreakdown(org);
				control.SetDataBinding(salesBreakdown, null);
				form.Controls.Add(control);
				form.Show();

				var anotherFactory = new BusinessObjectFactory();
				using (GetFactoryIsolater(anotherFactory))
				{
					var orgInOtherFactory = anotherFactory.Load<OrgHeader>(org.PK);
					orgInOtherFactory.Delete();
					anotherFactory.Save();
				}

				control.ViewProspectValuesButton_Exposed.PerformClick();

				AssertEquals("Cannot Add / Edit Estimate Values", UnitTestUserNotification.Instance.LastMessage.Caption);
				AssertEquals("Organization (ORGXXX) has been deleted.", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertNull(ZFormModaliser.LastFormShownForTest);
			}
		}

		#endregion

		public void TestTradedSalesAnalysisPeriod_ShouldChangeToCurrentFinancialYearOnShowFinancialYearToDateChecked()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var salesBreakdown = new SalesBreakdown(org);

			using (var form = new SalesBreakdownFormForTest(salesBreakdown))
			{
				form.Show();

				var control = form.SalesBreakdownControl;
				var analysisControl = control.DynamicTradedSalesAnalysisControl_Exposed;

				var shpProduct = Factory.LoadFromNaturalKey<OrgSalesProduct>(OrgSalesProductSchema.MP_Code, SystemDefinedSalesProductList.Codes.ForwardingShipment);
				var salesHeaderCollection = (SalesHeaderCollection)org.ActualAndProspectiveSalesHeaderCollection;
				var systemDefinedSalesHeader = salesHeaderCollection.AddNew(shpProduct);

				var analysis = analysisControl.InnerControl.SalesAnalysis;

				control.ShowPerAnnumRevenueRadioButton_Exposed.PerformClick();
				analysisControl.InnerControl.SetAnalysisPeriod(SalesAnalysisPeriodList.Codes.Trailing12Months);
				control.ShowFinancialYearToDateRevenueRadioButton_Exposed.PerformClick();

				AssertEquals(SalesAnalysisPeriodList.Codes.CurrentFinancialYear, analysis.Period);
			}
		}

		public void TestTradedSalesAnalysisPeriod_ShouldChangeToTrailing12MonthsOnShowPerAnnumChecked()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var salesBreakdown = new SalesBreakdown(org);

			using (var form = new SalesBreakdownFormForTest(salesBreakdown))
			{
				form.Show();

				var control = form.SalesBreakdownControl;
				var analysisControl = control.DynamicTradedSalesAnalysisControl_Exposed;

				var shpProduct = Factory.LoadFromNaturalKey<OrgSalesProduct>(OrgSalesProductSchema.MP_Code, SystemDefinedSalesProductList.Codes.ForwardingShipment);
				var salesHeaderCollection = (SalesHeaderCollection)org.ActualAndProspectiveSalesHeaderCollection;
				var systemDefinedSalesHeader = salesHeaderCollection.AddNew(shpProduct);

				var analysis = analysisControl.InnerControl.SalesAnalysis;

				control.ShowFinancialYearToDateRevenueRadioButton_Exposed.PerformClick();
				analysisControl.InnerControl.SetAnalysisPeriod(SalesAnalysisPeriodList.Codes.CurrentFinancialYear);
				control.ShowPerAnnumRevenueRadioButton_Exposed.PerformClick();

				AssertEquals(SalesAnalysisPeriodList.Codes.Trailing12Months, analysis.Period);
			}
		}

		[ExpectNoExceptions]
		public void TestDispose_ChartControlHostChildNull()
		{
			var org = Factory.New<OrgHeader>();
			var salesBreakdown = new SalesBreakdown(org);
			using (var form = new SalesBreakdownFormForTest(salesBreakdown))
			{
				var control = form.SalesBreakdownControl;
				control.SetDataBinding(salesBreakdown, null);
#if !WINZOR
				control.ChartPlotView_Exposed = null;
#endif
			}
		}

		#region Implementation

		class SalesBreakdownFormForTest : ZForm
		{
			public SalesBreakdownFormForTest(SalesBreakdown salesBreakdown)
			: base(salesBreakdown)
			{
				salesBreakdownControl = new SalesBreakdownControlForTest();
				this.BindingSource.SetBindingMember(salesBreakdownControl, ".");

				Controls.Add(salesBreakdownControl);
			}

			public SalesBreakdownControlForTest SalesBreakdownControl
			{
				get { return salesBreakdownControl; }
			}
			readonly SalesBreakdownControlForTest salesBreakdownControl;
		}

		class SalesBreakdownControlForTest : SalesBreakdownControl
		{
			public SalesHeaderControl SalesHeaderControl_Exposed
			{
				get { return base.salesHeaderControl; }
			}

			public ZTabPage TradedSalesAnalysisTabPage_Exposed
			{
				get { return base.TradedSalesAnalysisTabPage; }
			}

			public ZTabPage ProspectiveAnalysisTabPage_Exposed
			{
				get { return base.ProspectiveAnalysisTabPage; }
			}

			public ZToolStripButton ViewProspectValuesButton_Exposed
			{
				get { return base.viewProspectValuesButton; }
			}

			public ZRadioButton ShowPerAnnumRevenueRadioButton_Exposed
			{
				get { return base.ShowPerAnnumRevenueRadioButton; }
			}

			public ZRadioButton ShowFinancialYearToDateRevenueRadioButton_Exposed
			{
				get { return base.ShowFinancialYearToDateRevenueRadioButton; }
			}

			public DynamicTradedSalesAnalysisControl DynamicTradedSalesAnalysisControl_Exposed
			{
				get { return base.dynamicTradedSalesAnalysisControl; }
			}

#if !WINZOR
			public OxyplotView ChartPlotView_Exposed
			{
				get { return base.plotView; }
				set { base.plotView = value; }
			}
#endif
		}

		#endregion
	}
}
