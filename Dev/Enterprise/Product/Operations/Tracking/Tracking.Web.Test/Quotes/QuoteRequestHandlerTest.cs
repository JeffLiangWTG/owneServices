using System;
using CargoWise.Types;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Environment;
using Enterprise.Freight.QuotedBookings.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Rating.Business;
using Enterprise.Registry.Business;
using Enterprise.Tracking.Business;
using Enterprise.Tracking.Business.Testing;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Web.Business.Utilities;
using NUnit.Framework;

namespace Enterprise.Tracking.Web.Testing
{
	[TestedType(typeof(QuoteRequestHandler))]
	sealed class QuoteRequestHandlerTest : DataRequestHandlerTestCase<QuoteRequestHelper>
	{
		#region TestQuoteWithoutRates

		public void TestQuoteWithoutRates()
		{
			var oneOffQuoteA = CreateSpotQuote();

			var helper = new TrackingQuotationHelper(Factory, WebTestHelper.TestSiteUser.LoggedInOrgContact, oneOffQuoteA);

			WebDataRegistry.Instance.SaveQuotesWithoutRates.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			AssertEquals("Precondition: Preview should be true", true, helper.Preview());
			Factory.Save();

			var quoteRequestHandler = new QuoteRequestHandler();
			quoteRequestHandler.QueryString.Add(DataRequestHelper.DataKey, oneOffQuoteA.Quote.PK.ToString());

			WebDataRegistry.Instance.SaveQuotesWithoutRates.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			Assert("Document for a quote without rates is not generated", quoteRequestHandler.GetBinaryData().Length == 0);

			WebDataRegistry.Instance.SaveQuotesWithoutRates.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			Assert("Document for a quote without rates is generated if SaveQuotesWithoutRates=true", quoteRequestHandler.GetBinaryData().Length > 0);
		}

		#endregion

		public void TestQuoteWithoutCompany()
		{
			var oneOffQuoteA = CreateSpotQuote();
			oneOffQuoteA.Quote.TH_GC = ZGuid.Empty;
			var helper = new TrackingQuotationHelper(Factory, WebTestHelper.TestSiteUser.LoggedInOrgContact, oneOffQuoteA);
			Factory.Save();

			var quoteRequestHandler = new QuoteRequestHandler();
			quoteRequestHandler.QueryString.Add(DataRequestHelper.DataKey, oneOffQuoteA.Quote.PK.ToString());

			Assert("Document for a quote without a company is not generated", quoteRequestHandler.GetBinaryData().Length == 0);
		}

		public void TestQuoteWithNegativeSellAmt()
		{
			WebDataRegistry.Instance.SaveQuotesWithoutRates.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			var spotQuote = CreateSpotQuote();
			var job = new JobHeader.Loader(spotQuote.Quote).TryCreate();
			var charge = Factory.NewWithValidTestData<JobCharge>();
			charge.JR_JH = job.PK;
			charge.JR_AC = Env.Registry.FreightChargeCode;
			charge.JR_OSSellAmt = -100m;
			Factory.Save();

			var helper = new TrackingQuotationHelper(Factory, WebTestHelper.TestSiteUser.LoggedInOrgContact, spotQuote);
			AssertEquals("Precondition: Preview should be true", true, helper.Preview());
			Factory.Save();

			var quoteRequestHandler = new QuoteRequestHandler();
			quoteRequestHandler.QueryString.Add(DataRequestHelper.DataKey, spotQuote.Quote.PK.ToString());

			Assert("Document for a quote without negative charges is generated", quoteRequestHandler.GetBinaryData().Length > 0);
		}

		protected override DataRequestHandler<QuoteRequestHelper> GetNewRequestHandler()
		{
			CreateClientRate(5);
			var spotQuote = CreateSpotQuote();

			var helper = new TrackingQuotationHelper(Factory, WebTestHelper.TestSiteUser.LoggedInOrgContact, spotQuote);
			AssertEquals("Precondition: Preview should be true", true, helper.Preview());
			Factory.Save();

			var quoteRequestHandler = new QuoteRequestHandler();
			quoteRequestHandler.QueryString.Add(DataRequestHelper.DataKey, spotQuote.Quote.PK.ToString());
			return quoteRequestHandler;
		}

		ClientRate CreateClientRate(ZDecimal calculator)
		{
			ClientRate clientRate = Factory.New<ClientRate>();
			WebTestHelper.TestOrg.OH_IsConsignor = true;
			clientRate.TH_OH = WebTestHelper.TestOrg.PK;

			RateEntry entry = clientRate.AddRateEntry(TransportMode, ContainerMode, Origin, Destination);
			RateLine line = entry.RateLines[0];
			line.TL_RateCalculator = UnitCalculator.Code;
			line.Calculator[Calculator.Items.Operator.UNT] = calculator;
			line.TL_WeightVolume = Core.Constants.Volume.Litre;
			Factory.Save();

			return clientRate;
		}

		RefExchangeRate CreateExchangeRate(RefCurrency currency, ZDecimal rate, ZString type)
		{
			RefExchangeRate exchangeRate = Factory.New<RefExchangeRate>();
			exchangeRate.RE_StartDate = ZDateTime.Today.AddDays(-5);
			exchangeRate.RE_ExpiryDate = ZDateTime.Today.AddDays(5);
			exchangeRate.RE_ExRateType = type;
			exchangeRate.RE_SellRate = rate;
			exchangeRate.RE_RX_NKExCurrency = currency.RX_Code;
			exchangeRate.RE_GC = GlbCompany.CurrentCompany.PK;
			return exchangeRate;
		}

		QuotedBooking CreateSpotQuote()
		{
			var quotationRep = Factory.NewWithValidTestData<GlbStaff>();
			quotationRep.GS_GB_HomeBranch = GlbBranch.CurrentBranch.PK;
			quotationRep.GS_EmailAddress = "SalesRep@QuotationCo";
			RatingDataRegistry.Instance.DefaultSalesRepresentative.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, quotationRep.PK.ToGuid());

			Factory.Save();

			var fSpotQuote = TrackingQuotedBooking.GetNewQuotation(Factory, WebTestHelper.TestSiteUser);
			fSpotQuote.ClientPK = WebTestHelper.TestOrg.PK;
			fSpotQuote.Mode = ContainerMode;
			fSpotQuote.Origin = Origin;
			fSpotQuote.Destination = Destination;
			fSpotQuote.VolumeUnit = Core.Constants.Volume.Litre;
			fSpotQuote.Volume = 20m;

			var job = new Job.Loader(fSpotQuote).Load();
			AssertNull("Precondition: Job should not be created by the test. This should be created automatically when previewing or saving the quote.", job);

			return fSpotQuote;
		}

		const string TransportMode = Core.Constants.TransportModes.Air;
		const string ContainerMode = Core.Constants.ContainerModes.Loose;
		const string Origin = "HKHKG";
		const string Destination = "AUSYD";
		TestHelper WebTestHelper;

		protected override void SetUp()
		{
			base.SetUp();

			RefCurrency hKD = RefCurrency.LoadFromCurrencyCode(Factory, Core.Constants.CurrencyCodes.HongKong);
			CreateExchangeRate(hKD, 0.8m, Core.Constants.ExchangeRateTypes.Code.BuyRate);
			CreateExchangeRate(hKD, 0.8m, Core.Constants.ExchangeRateTypes.Code.SellRate);

			RefCurrency uSD = RefCurrency.LoadFromCurrencyCode(Factory, Core.Constants.CurrencyCodes.UnitedStates);
			CreateExchangeRate(uSD, 0.7m, Core.Constants.ExchangeRateTypes.Code.BuyRate);
			CreateExchangeRate(uSD, 0.7m, Core.Constants.ExchangeRateTypes.Code.SellRate);

			Factory.Save();
			ExchangeRateReader.GetReaderInstance().ClearCache();

			AssertEquals("Precondition: HKD Exchanges Rates were set up correctly", 0.8m,
				Env.CurrentCompany.ExchangeRate.TodaysRate(hKD.RX_Code, ExchangeRateType.Sell));

			AssertEquals("Precondition: USD Exchanges Rates were set up correctly", 0.7m,
				Env.CurrentCompany.ExchangeRate.TodaysRate(uSD.RX_Code, ExchangeRateType.Sell));

			WebTestHelper = new TestHelper(Factory);

			WebTestHelper.TestOrg.FillWithValidTestData();
			WebTestHelper.TestOrg.OH_IsDebtor = true;
			WebTestHelper.TestOrg.OH_IsConsignor = true;
			WebTestHelper.TestOrg.CompanyData.OB_RX_NKARDDefltCurrency = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;

			WebTestHelper.TestSiteUser.Login(WebTestHelper.TestOrg.OH_Code, WebTestHelper.TestContact.OC_Email, WebTestHelper.TestContact.PasswordForTesting);
		}
	}
}
