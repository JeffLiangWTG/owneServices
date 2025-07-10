using System;
using System.Web.UI.HtmlControls;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Freight.QuotedBookings.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Rating.Business;
using Enterprise.Rating.Integration;
using Enterprise.Registry.Business;
using Enterprise.Tracking.Business;
using Enterprise.Tracking.Business.Testing;
using Enterprise.ZArchitecture.Web.Business.Testing;
using Enterprise.ZArchitecture.Web.GUI.WebControls;

namespace Enterprise.Tracking.Web.Testing
{
	[HttpContextEnabledTest]
	sealed class QuotationDataTest : TestCaseWithFactory
	{
		public void TestSetupLooseCargoDataGrid()
		{
			QuotationTestClass testQuotationPage = new QuotationTestClass();
			testQuotationPage.LooseCargoDataGridInternal = new ZDataGrid();
			testQuotationPage.SetupLooseCargoDataGrid();

			AssertEquals("Columns", 10, testQuotationPage.LooseCargoDataGridInternal.Columns.Count);

			var countColumn = testQuotationPage.LooseCargoDataGridInternal.Columns[0] as ZCalcEditColumn;
			AssertEquals("Count", countColumn.HeaderText);
			AssertEquals(0, countColumn.Decimals);
			AssertEquals("Pack Type", testQuotationPage.LooseCargoDataGridInternal.Columns[1].HeaderText);
			AssertEquals("Length", testQuotationPage.LooseCargoDataGridInternal.Columns[2].HeaderText);
			AssertEquals("Width", testQuotationPage.LooseCargoDataGridInternal.Columns[3].HeaderText);
			AssertEquals("Height", testQuotationPage.LooseCargoDataGridInternal.Columns[4].HeaderText);
			AssertEquals("UD", testQuotationPage.LooseCargoDataGridInternal.Columns[5].HeaderText);
			AssertEquals("Weight", testQuotationPage.LooseCargoDataGridInternal.Columns[6].HeaderText);
			AssertEquals("UW", testQuotationPage.LooseCargoDataGridInternal.Columns[7].HeaderText);
			AssertEquals("Volume", testQuotationPage.LooseCargoDataGridInternal.Columns[8].HeaderText);
			AssertEquals("UV", testQuotationPage.LooseCargoDataGridInternal.Columns[9].HeaderText);
		}

		public void TestShowImperialUnits()
		{
			QuotationTestPage testQuotationPage = new QuotationTestPage();
			testQuotationPage.VolumeDropDownInternal = null;
			AssertEquals("ShowImperialUnits", false, testQuotationPage.ShowImperialUnits);

			ZDropDownListTestClass volumeDropDownForTest = new ZDropDownListTestClass();
			testQuotationPage.VolumeDropDownInternal = volumeDropDownForTest;

			AssertEquals("Precondition: CachedSelectedValue", null,
				volumeDropDownForTest.CachedSelectedValueExposedForTest);
			AssertEquals("ShowImperialUnits", false, testQuotationPage.ShowImperialUnits);

			volumeDropDownForTest.CachedSelectedValueExposedForTest = Core.Constants.Volume.CubicFeet;
			AssertEquals("Precondition: CachedSelectedValue", Core.Constants.Volume.CubicFeet,
				volumeDropDownForTest.CachedSelectedValueExposedForTest);
			AssertEquals("ShowImperialUnits", true, testQuotationPage.ShowImperialUnits);

			volumeDropDownForTest.CachedSelectedValueExposedForTest = Core.Constants.Volume.CubicDecimetres;
			AssertEquals("Precondition: CachedSelectedValue", Core.Constants.Volume.CubicDecimetres,
				volumeDropDownForTest.CachedSelectedValueExposedForTest);
			AssertEquals("ShowImperialUnits", false, testQuotationPage.ShowImperialUnits);
		}

		public void TestPreviewConcurrencyException()
		{
			using (SetupSaveQuoteTest())
			using (var page = new QuotationTestClass())
			{
				page.SiteUser.Login(Helper.TestOrg.OH_Code, "user@test.com", "secretpassword");
				page.LoadOrCreateDataSourceInternal();

				var spotQuote = page.SpotQuote;
				spotQuote.Factory.Saving += factory => SimulateConcurrencyException(factory, spotQuote);

				page.PreviewQuote_Click(page.PreviewQuoteInternal, EventArgs.Empty);
				AssertContains("Error message contains concurrency exception text", "While you have been working with this form, another user has made changes", spotQuote.Quote.Notifications.GetFirst().Message);
			}
		}

		public void TestNoFreightChargeForSelectedIncoTerm()
		{
			using (SetupSaveQuoteTest(false))
			using (var page = new QuotationTestClass())
			{
				page.SiteUser.Login(Helper.TestOrg.OH_Code, "user@test.com", "secretpassword");
				page.LoadOrCreateDataSourceInternal();

				TrackingQuotedBooking spotQuote = page.DataSource as TrackingQuotedBooking;
				Helper.TestOrg.OH_IsConsignor = true;
				Helper.TestOrg.OH_IsDebtor = true;
				spotQuote.ClientPK = Helper.TestOrg.PK;

				ClientRate clientRate = Factory.New<ClientRate>();
				clientRate.TH_OH = Helper.TestOrg.PK;
				spotQuote.Origin = "HKHKG";
				spotQuote.Destination = "AUSYD";

				RateEntry entry1 = clientRate.AddRateEntry("AIR", "LSE", "HKHKG", "AUSYD");
				entry1.TI_OH_TransportProvider = Factory.NewWithValidTestData<OrgHeader>().PK;
				entry1.RateLines[0].TL_RateCalculator = UnitCalculator.Code;
				entry1.RateLines[0].Calculator[Calculator.Items.Operator.UNT] = (ZDecimal)10m;
				entry1.RateLines[0].TL_WeightVolume = Core.Constants.Volume.Litre;

				RateEntry entry2 = clientRate.AddRateEntry("AIR", "LSE", "HKHKG", "AUSYD");
				entry2.TI_OH_TransportProvider = Factory.NewWithValidTestData<OrgHeader>().PK;
				entry2.RateLines[0].TL_RateCalculator = UnitCalculator.Code;
				entry2.RateLines[0].Calculator[Calculator.Items.Operator.UNT] = (ZDecimal)5m;
				entry2.RateLines[0].TL_WeightVolume = Core.Constants.Volume.Litre;
				Factory.Save();

				page.PreviewQuote_Click(page.PreviewQuoteInternal, EventArgs.Empty);
				AssertEmailNotice(page.EmailNoticeInternal, "Unable to calculate a Freight charge for the selected Incoterm.");
			}
		}

		public void TestFinaliseConcurrencyException()
		{
			using (SetupSaveQuoteTest())
			using (var page = new QuotationTestClass())
			{
				page.SiteUser.Login(Helper.TestOrg.OH_Code, "user@test.com", "secretpassword");
				page.LoadOrCreateDataSourceInternal();

				var spotQuote = page.SpotQuote;
				spotQuote.Factory.Saving += factory => SimulateConcurrencyException(factory, spotQuote);

				page.SaveQuote_Click(page.SaveQuoteInternal, EventArgs.Empty);
				AssertContains("Error message contains concurrency exception text", "While you have been working with this form, another user has made changes", spotQuote.Quote.Notifications.GetFirst().Message);
			}
		}

		public void TestPreviewAutoRaterException_EmailNotice()
		{
			using (SetupSaveQuoteTest())
			using (var page = new QuotationTestClass())
			{
				page.SiteUser.Login(Helper.TestOrg.OH_Code, "user@test.com", "secretpassword");
				page.LoadOrCreateDataSourceInternal();

				var spotQuote = page.SpotQuote;
				spotQuote.Factory.Saving += factory => throw new AutoRaterException("AutoRaterException message"); // Simulated throw from AutoRatingCalculatorParameters.GetChargeableAmount

				page.PreviewQuote_Click(page.PreviewQuoteInternal, EventArgs.Empty);

				AssertEmailNotice(page.EmailNoticeInternal, "AutoRaterException message");
			}
		}

		public void TestFinaliseAutoRaterException_EmailNotice()
		{
			using (SetupSaveQuoteTest())
			using (var page = new QuotationTestClass())
			{
				page.SiteUser.Login(Helper.TestOrg.OH_Code, "user@test.com", "secretpassword");
				page.LoadOrCreateDataSourceInternal();

				var spotQuote = page.SpotQuote;
				spotQuote.Factory.Saving += factory => throw new AutoRaterException("AutoRaterException message"); // Simulated throw from AutoRatingCalculatorParameters.GetChargeableAmount

				page.SaveQuote_Click(page.SaveQuoteInternal, EventArgs.Empty);

				AssertEmailNotice(page.EmailNoticeInternal, "AutoRaterException message");
			}
		}

		void AssertEmailNotice(HtmlGenericControl emailNotice, string message = "")
		{
			var additionalInfo = string.IsNullOrEmpty(message) ? "The system was unable to generate a quotation for the provided details." : message;

			AssertEquals("Unable to generate quotation", ((ZTextLabel)emailNotice.Controls[0]).Text);
			AssertEquals(additionalInfo + @"<br>Please contact <a href=""mailto:SalesRep@QuotationCo"">a sales representative</a> directly to request a quotation.", ((HtmlGenericControl)emailNotice.Controls[1]).InnerHtml);
		}

		IDisposable SetupSaveQuoteTest(bool saveQuoteWithoutRates = true)
		{
			var quotationRep = Factory.NewWithValidTestData<GlbStaff>();
			quotationRep.GS_GB_HomeBranch = GlbBranch.CurrentBranch.PK;
			quotationRep.GS_EmailAddress = "SalesRep@QuotationCo";
			Factory.Save();

			var previousSaveQuotesWithoutRates = WebDataRegistry.Instance.SaveQuotesWithoutRates.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty);
			var previousDefaultSalesRepresentative = RatingDataRegistry.Instance.DefaultSalesRepresentative.GetValueWithoutFallback(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty);

			WebDataRegistry.Instance.SaveQuotesWithoutRates.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, saveQuoteWithoutRates);
			RatingDataRegistry.Instance.DefaultSalesRepresentative.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, quotationRep.PK.ToGuid());

			return new DisposableAction(() =>
			{
				WebDataRegistry.Instance.SaveQuotesWithoutRates.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, previousSaveQuotesWithoutRates);
				RatingDataRegistry.Instance.DefaultSalesRepresentative.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, previousDefaultSalesRepresentative);
			});
		}

		void SimulateConcurrencyException(BusinessObjectFactory factory, QuotedBooking spotQuote)
		{
			var factory2 = new BusinessObjectFactory() { RefreshEnabled = false };
			var quoteInFactory2 = factory2.Load<Quote>(spotQuote.Quote.PK);
			quoteInFactory2.TH_SystemCreateUser = "~BP";
			factory2.Save();

			spotQuote.Quote.TH_IsLocked = true;

			throw new ZSaveConcurrencyException(new ZDataConcurrencyException(new InvalidOperationException("Simulated concurrency exception"), ((IBusinessObjectInternals)spotQuote.Quote).Row, ((IDbConnected)factory).Connection), factory);
		}

		public void TestGetNewDataSource()
		{
			BusinessObject oneOffQuote = QuotationPage.GetNewDataSourceInternal();
			AssertNotNull(oneOffQuote);
		}

		QuotationTestPage QuotationPage
		{
			get
			{
				if (fQuotationPage == null)
				{
					fQuotationPage = new QuotationTestPage();
				}
				return fQuotationPage;
			}
		}
		QuotationTestPage fQuotationPage;

		protected override void SetUp()
		{
			base.SetUp();

			Helper = new TestHelper(Factory);
			Factory.Save();

			Helper.TestContact.OC_ContactName = "Test User";
			Helper.TestContact.OC_Email = "user@test.com";
			Helper.TestContact.SetHashedPassword("secretpassword");
			Helper.TestContact.OC_WebAccessEnabled = true;

			Factory.Save();

			QuotationPage.SiteUser.Login(Helper.TestOrg.OH_Code, "user@test.com", "secretpassword");
			Assert(QuotationPage.SiteUser.IsLoggedIn);
		}

		TestHelper Helper;
	}
}
