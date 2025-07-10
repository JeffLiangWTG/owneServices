using System.Collections.Generic;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.LandedCosting.Business;
using Enterprise.LandedCosting.Business.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.LandedCosting.GUI.Testing
{
	[TestedType(typeof(LandCostInputFakeForm))]
	sealed class LandCostInputUserControlFakeFormTest : ZFormBasherTest
	{
		[TestDate(2017, 03, 30)]
		public void TestNoteLabel_NonReciprocalExRate_DisplayExpectedExampleChangeRate()
		{
			const string testExCurrency = Core.Constants.CurrencyCodes.Algeria;
			var startDate = new ZDate(2017, 03, 30);
			var expectedRate = 12345.0M;

			var exRates = new List<KeyValuePair<ZDateTime, decimal>>
				{
					new KeyValuePair<ZDateTime, decimal>(startDate, expectedRate),
					new KeyValuePair<ZDateTime, decimal>(startDate.AddDays(7), 0.123M),
					new KeyValuePair<ZDateTime, decimal>(startDate.AddDays(30), 0.234M),
					new KeyValuePair<ZDateTime, decimal>(startDate.AddDays(90), 1.345M),
				};

			SetupCurrencyExchangeRate(exRates, testExCurrency);

			var lcHeader = CreateLandedCostHeaderForNoteLabelTests(testExCurrency);
			Assert("Cost header exchange rate is not reciprocal", !lcHeader.IsReciprocalExRate);

			foreach (var lcHeaderCostInput in lcHeader.CostInputs)
			{
				var costInput = lcHeaderCostInput as LandCostInput;
				if (costInput != null)
				{
					costInput.LI_RX_NKCostCurrency = testExCurrency;
				}
			}

			using (var form = new LandCostInputFakeForm(lcHeader))
			{
				form.Show();
				form.landCostInputUserControl.MainTabControl.SelectedTab = form.landCostInputUserControl.LinesTabPage;
				form.landCostInputUserControl.MainTabControl.SelectedTab = form.landCostInputUserControl.CostTabPage;
				Assert("Exchange rate note label is visible when selected tab is Transport Logistics Costs tab and the exchange rate is not reciprocal", form.landCostInputUserControl.NotePanel.Visible);

				var noteText = form.landCostInputUserControl.NoteLabel.Text;
				Assert("Expected note message", noteText.StartsWith("Note that manually entered foreign exchange rates need to be entered as a direct quote"));
				Assert("Expected note message contains local currency code", noteText.Contains(lcHeader.Company.GC_RX_NKLocalCurrency.ToString()));
				Assert("Expected note message contains foreign currency code", noteText.Contains(testExCurrency));
				Assert("Expected note message contains expected foreign exchanged rate", noteText.Contains(expectedRate.ToString("G29")));
			}
		}

		[TestDate(2017, 03, 30)]
		public void TestNoteLabel_NonReciprocalExRate_RefExRateNotFound_DisplayDefaultExampleChangeRate()
		{
			TestConnection.ExecuteNonQuery(@"DELETE FROM dbo.ZZRefExchangeRate");

			const string testExCurrency = Core.Constants.CurrencyCodes.Japan;
			const string defaultExCurrency = "USD";
			var expectedRate = LandCostInputUserControl.DefaultExchangeRate;

			var lcHeader = CreateLandedCostHeaderForNoteLabelTests(testExCurrency);
			Assert("Cost header exchange rate is not reciprocal", !lcHeader.IsReciprocalExRate);

			foreach (var lcHeaderCostInput in lcHeader.CostInputs)
			{
				var costInput = lcHeaderCostInput as LandCostInput;
				if (costInput != null)
				{
					costInput.LI_RX_NKCostCurrency = testExCurrency;
				}
			}

			using (var form = new LandCostInputFakeForm(lcHeader))
			{
				form.Show();
				form.landCostInputUserControl.MainTabControl.SelectedTab = form.landCostInputUserControl.LinesTabPage;
				form.landCostInputUserControl.MainTabControl.SelectedTab = form.landCostInputUserControl.CostTabPage;
				Assert("Exchange rate note label is visible when selected tab is Transport Logistics Costs tab and the exchange rate is not reciprocal", form.landCostInputUserControl.NotePanel.Visible);

				var noteText = form.landCostInputUserControl.NoteLabel.Text;
				Assert("Expected note message", noteText.StartsWith("Note that manually entered foreign exchange rates need to be entered as a direct quote"));
				Assert("Expected note message contains local currency code", noteText.Contains(lcHeader.Company.GC_RX_NKLocalCurrency.ToString()));
				Assert("Expected note message contains default currency code", noteText.Contains(defaultExCurrency));
				Assert("Expected note message contains default exchanged rate", noteText.Contains(expectedRate.ToString("G29")));
			}
		}

		[TestDate(2017, 03, 30)]
		public void TestNoteLabel_NonReciprocalExRate_SameCurrency_DisplayNoExampleChangeRate()
		{
			var testExCurrency = Core.Constants.CurrencyCodes.SouthAfrica;
			var lcHeader = CreateLandedCostHeaderForNoteLabelTests(testExCurrency);
			Assert("Cost header exchange rate is not reciprocal", !lcHeader.IsReciprocalExRate);

			foreach (var lcHeaderCostInput in lcHeader.CostInputs)
			{
				var costInput = lcHeaderCostInput as LandCostInput;
				if (costInput != null)
				{
					costInput.LI_RX_NKCostCurrency = lcHeader.Company.GC_RX_NKLocalCurrency;
				}
			}

			using (var form = new LandCostInputFakeForm(lcHeader))
			{
				form.Show();
				form.landCostInputUserControl.MainTabControl.SelectedTab = form.landCostInputUserControl.LinesTabPage;
				form.landCostInputUserControl.MainTabControl.SelectedTab = form.landCostInputUserControl.CostTabPage;
				Assert("Exchange rate note label is visible when selected tab is Transport Logistics Costs tab and the exchange rate is not reciprocal", form.landCostInputUserControl.NotePanel.Visible);

				var noteText = form.landCostInputUserControl.NoteLabel.Text;
				Assert("Expected note message", noteText.Equals("Note that manually entered foreign exchange rates need to be entered as a direct quote"));
			}
		}

		public void TestNoteLabel_ReciprocalExRate_CostTabPage_Invisible()
		{
			var testDec = (BusinessObject)Factory.New<Integration.Customs.IBaseJobDeclaration>();
			GlbCompany.CurrentCompany.GC_IsReciprocal = true;

			var zaCompany = Factory.New<GlbCompany>();
			zaCompany.GC_RN_NKCountryCode = Core.Constants.CountryCodes.SouthAfrica;

			var lcHeader = Factory.New<LandedCostHeader>();
			lcHeader.LT_ParentID = testDec.PK;
			lcHeader.LT_ParentTableCode = "JE";
			lcHeader.LT_GC = zaCompany.PK;

			Assert("Cost header exchange rate is reciprocal", lcHeader.IsReciprocalExRate);

			foreach (var lcHeaderCostInput in lcHeader.CostInputs)
			{
				var costInput = lcHeaderCostInput as LandCostInput;
				if (costInput != null)
				{
					costInput.LI_RX_NKCostCurrency = "USD";
				}
			}

			using (var form = new LandCostInputFakeForm(lcHeader))
			{
				form.Show();

				form.landCostInputUserControl.MainTabControl.SelectedTab = form.landCostInputUserControl.LinesTabPage;
				form.landCostInputUserControl.NotePanel.Visible = true;
				Assert("Exchange rate note label is set visible", form.landCostInputUserControl.NotePanel.Visible);

				form.landCostInputUserControl.MainTabControl.SelectedTab = form.landCostInputUserControl.CostTabPage;
				Assert("Exchange rate note label is invisible when rate calculation is reciprocal", !form.landCostInputUserControl.NotePanel.Visible);
			}
		}

		public void TestNoteLabel_LinesTabPage_Invisible()
		{
			var helper = new TestHelper(Factory);
			var lcHeader = helper.GetLCHeaderWithDistributionObjectsPluggedIn();

			using (var form = new LandCostInputFakeForm(lcHeader))
			{
				form.Show();
				form.landCostInputUserControl.MainTabControl.SelectedTab = form.landCostInputUserControl.LinesTabPage;
				Assert("Exchange rate note label is invisible when selected tab is Lines tab", !form.landCostInputUserControl.NotePanel.Visible);
			}
		}

		public void TestLandCostInputUserControl()
		{
			TestHelper helper = new TestHelper(Factory);
			LandedCostHeader lCHeader = helper.GetLCHeaderWithDistributionObjectsPluggedIn();

			using (LandCostInputFakeForm form = new LandCostInputFakeForm(lCHeader))
			{
				form.Show();
				form.landCostInputUserControl.MainTabControl.SelectedTab = form.landCostInputUserControl.LinesTabPage;
				AssertEquals("BindTo of History user control is set", "Histories", form.landCostInputUserControl.historiesUserControl.LCHistoryGrid.BindTo);
				AssertEquals("LCHistoryMaster is set", lCHeader, form.landCostInputUserControl.historiesUserControl.LCHistoryMaster);
			}
		}

		public void TestEstimatedDutyPercentCalcEditVisibleForEstimatedLC()
		{
			TestHelper helper = new TestHelper(Factory);
			LandedCostHeader lCHeader = helper.GetLCHeaderWithDistributionObjectsPluggedIn();
			lCHeader.LT_LandedCostType = LandedCostType.Estimated;

			using (LandCostInputFakeForm form = new LandCostInputFakeForm(lCHeader))
			{
				form.Show();
				form.landCostInputUserControl.MainTabControl.SelectedTab = form.landCostInputUserControl.LinesTabPage;
				AssertEquals(true, form.landCostInputUserControl.EstimatedDutyPercentCalcEdit.Visible);
			}

			lCHeader.LT_LandedCostType = LandedCostType.Actual;
			using (LandCostInputFakeForm form = new LandCostInputFakeForm(lCHeader))
			{
				form.Show();
				form.landCostInputUserControl.MainTabControl.SelectedTab = form.landCostInputUserControl.LinesTabPage;
				AssertEquals(false, form.landCostInputUserControl.EstimatedDutyPercentCalcEdit.Visible);
			}
		}

		protected override Form GetFormToBashCore()
		{
			var helper = new TestHelper(Factory);
			var lCHeader = helper.GetLCHeaderWithDistributionObjectsPluggedIn();
			_ = helper.GetHistory(lCHeader);
			Factory.Save();

			return new LandCostInputFakeForm(lCHeader);
		}

		public override bool AllowUntranslatableFormTitle() => true;

		LandedCostHeader CreateLandedCostHeaderForNoteLabelTests(ZString exCurrencyCode)
		{
			var testDec = (BusinessObject)Factory.New<Integration.Customs.IBaseJobDeclaration>();
			GlbCompany.CurrentCompany.GC_IsReciprocal = false;

			var zaCompany = Factory.New<GlbCompany>();
			zaCompany.GC_RN_NKCountryCode = Core.Constants.CountryCodes.SouthAfrica;

			var helper = new TestHelper(Factory);
			var lcHeader = helper.GetLCHeaderWithDistributionObjectsPluggedIn();
			lcHeader.LT_GC = zaCompany.PK;
			lcHeader.LT_ParentID = testDec.PK;
			lcHeader.LT_ParentTableCode = "JE";

			Assert("Cost header exchange rate is not reciprocal", !lcHeader.IsReciprocalExRate);

			foreach (var lcHeaderCostInput in lcHeader.CostInputs)
			{
				var costInput = lcHeaderCostInput as LandCostInput;
				if (costInput != null)
				{
					costInput.LI_RX_NKCostCurrency = exCurrencyCode;
				}
			}

			return lcHeader;
		}

		void SetupCurrencyExchangeRate(IList<KeyValuePair<ZDateTime, decimal>> cusExchangeRates, ZString exCurrencyCode)
		{
			foreach (var cusExchangeRate in cusExchangeRates)
			{
				var query = new ZQuery(RefExchangeRateSchema.RE_GC, Env.CurrentCompany.PK);
				query.AddToFilter(RefExchangeRateSchema.RE_RX_NKExCurrency, exCurrencyCode);
				query.AddToFilter(RefExchangeRateSchema.RE_StartDate, cusExchangeRate.Key);
				query.AddToFilter(RefExchangeRateSchema.RE_SellRate, cusExchangeRate.Value);

				var exchangeRate = Factory.LoadTop1<RefExchangeRate>(query) ?? Factory.New<RefExchangeRate>();

				exchangeRate.RE_ExRateType = Core.Constants.ExchangeRateTypes.Code.CustomsRate;
				exchangeRate.RE_GC = Env.CurrentCompany.PK;

				exchangeRate.RE_StartDate = cusExchangeRate.Key;
				exchangeRate.RE_ExpiryDate = cusExchangeRate.Key.AddDays(365);
				exchangeRate.RE_SellRate = cusExchangeRate.Value;
				exchangeRate.RE_RX_NKExCurrency = exCurrencyCode;
			}

			Factory.Save();
		}

		sealed class LandCostInputFakeForm : ZForm
		{
			public LandCostInputFakeForm(LandedCostHeader lCHeaderWithHost)
				: base(lCHeaderWithHost)
			{
				CaptionRenderingEnabled = true;
			}

			public override string FormCaption => "This is the test";

			protected override void InitialiseForm()
			{
				base.InitialiseForm();
				InitializeComponent();
			}

			public LandCostInputUserControl landCostInputUserControl;

			new void InitializeComponent()
			{
				landCostInputUserControl = new LandCostInputUserControl();
				Controls.Add(landCostInputUserControl);
				landCostInputUserControl.Dock = DockStyle.Fill;
				Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1024, 680, true);
				DataSourceAssemblyName = "Enterprise.LandedCosting.Business";
				DataSourceTypeName = "Enterprise.LandedCosting.Business.LandedCostHeader";
				Name = "LandCostInputUserControl";
			}
		}
	}
}
