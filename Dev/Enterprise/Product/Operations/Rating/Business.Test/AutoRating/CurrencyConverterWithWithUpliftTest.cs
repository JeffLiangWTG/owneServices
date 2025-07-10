using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Integration;
using Enterprise.DocumentEngineCore.Registry;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Rating.Business.Testing
{
	public class CurrencyConverterWithWithUpliftTest : TestCaseWithFactory
	{
		public void TestGetExchangeRateWithoutIncludeCFXInRatingCalculation()
		{
			GlbCompany.CurrentCompany.SetCountry("AU");
			DocumentsDataRegistry.Instance.IncludeCFXinExchangeRateOnQuotationPrinting.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false);

			Helper.NewExchangeRate("EUR", Core.Constants.ExchangeRateTypes.Code.BuyRate, 0.62m);
			Helper.NewExchangeRate("EUR", Core.Constants.ExchangeRateTypes.Code.SellRate, 0.61m);
			Helper.NewExchangeRate("USD", Core.Constants.ExchangeRateTypes.Code.BuyRate, 0.77m);
			Helper.NewExchangeRate("USD", Core.Constants.ExchangeRateTypes.Code.SellRate, 0.76m);

			var converter = new CurrencyConverterWithWithUplift(Factory, null, Helper.CurrencyConverter);
			AssertEquals(0.6100m, Utilities.Round(converter.GetExchangeRate("EUR", "AUD", DocumentsDataRegistry.Instance.IncludeCFXinExchangeRateOnQuotationPrinting.Value), 4));
			AssertEquals(1.6129m, Utilities.Round(converter.GetExchangeRate("AUD", "EUR", DocumentsDataRegistry.Instance.IncludeCFXinExchangeRateOnQuotationPrinting.Value), 4));
			AssertEquals(0.7600m, Utilities.Round(converter.GetExchangeRate("USD", "AUD", DocumentsDataRegistry.Instance.IncludeCFXinExchangeRateOnQuotationPrinting.Value), 4));
			AssertEquals(1.2987m, Utilities.Round(converter.GetExchangeRate("AUD", "USD", DocumentsDataRegistry.Instance.IncludeCFXinExchangeRateOnQuotationPrinting.Value), 4));
			AssertEquals(0.7922m, Utilities.Round(converter.GetExchangeRate("EUR", "USD", DocumentsDataRegistry.Instance.IncludeCFXinExchangeRateOnQuotationPrinting.Value), 4));
			AssertEquals(1.2258m, Utilities.Round(converter.GetExchangeRate("USD", "EUR", DocumentsDataRegistry.Instance.IncludeCFXinExchangeRateOnQuotationPrinting.Value), 4));
			AssertEquals(1m, converter.GetExchangeRate("EUR", "EUR", DocumentsDataRegistry.Instance.IncludeCFXinExchangeRateOnQuotationPrinting.Value));
			AssertEquals(1m, converter.GetExchangeRate("USD", "USD", DocumentsDataRegistry.Instance.IncludeCFXinExchangeRateOnQuotationPrinting.Value));
			AssertEquals(1m, converter.GetExchangeRate("AUD", "AUD", DocumentsDataRegistry.Instance.IncludeCFXinExchangeRateOnQuotationPrinting.Value));
			AssertEquals(0m, converter.GetExchangeRate("NZD", "AUD", DocumentsDataRegistry.Instance.IncludeCFXinExchangeRateOnQuotationPrinting.Value));
			AssertEquals(0m, converter.GetExchangeRate("AUD", "NZD", DocumentsDataRegistry.Instance.IncludeCFXinExchangeRateOnQuotationPrinting.Value));
			AssertEquals(1m, converter.GetExchangeRate("NZD", "NZD", DocumentsDataRegistry.Instance.IncludeCFXinExchangeRateOnQuotationPrinting.Value));
			AssertEquals(0m, converter.GetExchangeRate("AUD", "###", DocumentsDataRegistry.Instance.IncludeCFXinExchangeRateOnQuotationPrinting.Value));
			AssertEquals(0m, converter.GetExchangeRate("###", "AUD", DocumentsDataRegistry.Instance.IncludeCFXinExchangeRateOnQuotationPrinting.Value));

			var client = Helper.NewOrgHeader();
			var testQuote = Helper.NewQuote(client);
			testQuote.AddRateEntry("ORG", "ALL", "AUSYD", "").AddRateLine("ODOC").Calculator[Calculator.Items.Operator.BAS] = (ZDecimal)10m;
			testQuote.AddRateEntry("ORG", "ALL", "USLAX", "").AddRateLine("ODOC").Calculator[Calculator.Items.Operator.BAS] = (ZDecimal)10m;
			testQuote.TH_AirCFX = 1m;
			testQuote.TH_ExportAirCFX = 2m;
			testQuote.TH_SeaCFX = 3m;
			testQuote.TH_ExportSeaCFX = 4m;
			Factory.Save();
			testQuote.TH_OneTimeQuote = true;
			var oneOffQuote = testQuote.CurrentOneOffQuote;
			oneOffQuote.TT_TransportMode = Core.Constants.TransportModes.Air;
			oneOffQuote.TT_ContainerMode = Core.Constants.ContainerModes.Loose;
			oneOffQuote.TT_RL_NKReceivalLocation = "USLAX";
			oneOffQuote.TT_RL_NKDeliveryLocation = "AUSYD";
			oneOffQuote.TT_ActualWeight = 100m;
			oneOffQuote.TT_UnitOfWeight = RatingConstants.Units.KG;
			oneOffQuote.TT_ActualVolume = 0.5m;
			oneOffQuote.TT_UnitOfVolume = RatingConstants.Units.M3;
			oneOffQuote.PickUpDocAddress.OrganisationPK = client.PK;
			Factory.Save();

			var proxy = new AutoRatingProxy(oneOffQuote.RatingAdapter);
			proxy.ValuesCanBeSet = true;
			proxy.QuoteNumber = testQuote.TH_QuoteNumber;

			var charges = new FreightAutoRater(new RatingContext()).AutoRate(proxy, CostSell.Revenue).RateInfoCollection;
			AssertEquals("Charges exist", 1, charges.Count);

			converter = new CurrencyConverterWithWithUplift(Factory, charges[0], Helper.CurrencyConverter);
			AssertEquals(0.76m, Utilities.Round(converter.GetExchangeRate("USD", "AUD", DocumentsDataRegistry.Instance.IncludeCFXinExchangeRateOnQuotationPrinting.Value), 4));
			AssertEquals(1.2987m, Utilities.Round(converter.GetExchangeRate("AUD", "USD", DocumentsDataRegistry.Instance.IncludeCFXinExchangeRateOnQuotationPrinting.Value), 4));
			AssertEquals(0.7922m, Utilities.Round(converter.GetExchangeRate("EUR", "USD", DocumentsDataRegistry.Instance.IncludeCFXinExchangeRateOnQuotationPrinting.Value), 4));
			AssertEquals(1.2258m, Utilities.Round(converter.GetExchangeRate("USD", "EUR", DocumentsDataRegistry.Instance.IncludeCFXinExchangeRateOnQuotationPrinting.Value), 4));
			AssertEquals(1m, converter.GetExchangeRate("USD", "USD", DocumentsDataRegistry.Instance.IncludeCFXinExchangeRateOnQuotationPrinting.Value));
			AssertEquals(1m, converter.GetExchangeRate("AUD", "AUD", DocumentsDataRegistry.Instance.IncludeCFXinExchangeRateOnQuotationPrinting.Value));
			AssertEquals(0m, converter.GetExchangeRate("NZD", "AUD", DocumentsDataRegistry.Instance.IncludeCFXinExchangeRateOnQuotationPrinting.Value));

			oneOffQuote.TT_RL_NKReceivalLocation = "AUSYD";
			oneOffQuote.TT_RL_NKDeliveryLocation = "USLAX";
			proxy = new AutoRatingProxy(oneOffQuote.RatingAdapter);
			proxy.ValuesCanBeSet = true;
			proxy.QuoteNumber = testQuote.TH_QuoteNumber;
			charges = new FreightAutoRater(new RatingContext()).AutoRate(proxy, CostSell.Revenue).RateInfoCollection;

			converter = new CurrencyConverterWithWithUplift(Factory, charges[0], Helper.CurrencyConverter);
			AssertEquals(0.76m, Utilities.Round(converter.GetExchangeRate("USD", "AUD", DocumentsDataRegistry.Instance.IncludeCFXinExchangeRateOnQuotationPrinting.Value), 4));
			AssertEquals(1.2987m, Utilities.Round(converter.GetExchangeRate("AUD", "USD", DocumentsDataRegistry.Instance.IncludeCFXinExchangeRateOnQuotationPrinting.Value), 4));
			AssertEquals(0.7922m, Utilities.Round(converter.GetExchangeRate("EUR", "USD", DocumentsDataRegistry.Instance.IncludeCFXinExchangeRateOnQuotationPrinting.Value), 4));
			AssertEquals(1.2258m, Utilities.Round(converter.GetExchangeRate("USD", "EUR", DocumentsDataRegistry.Instance.IncludeCFXinExchangeRateOnQuotationPrinting.Value), 4));

			oneOffQuote.TT_TransportMode = Core.Constants.TransportModes.Sea;
			oneOffQuote.TT_ContainerMode = Core.Constants.ContainerModes.LCL;
			oneOffQuote.TT_RL_NKReceivalLocation = "USLAX";
			oneOffQuote.TT_RL_NKDeliveryLocation = "AUSYD";
			proxy = new AutoRatingProxy(oneOffQuote.RatingAdapter);
			proxy.ValuesCanBeSet = true;
			proxy.QuoteNumber = testQuote.TH_QuoteNumber;
			charges = new FreightAutoRater(new RatingContext()).AutoRate(proxy, CostSell.Revenue).RateInfoCollection;

			converter = new CurrencyConverterWithWithUplift(Factory, charges[0], Helper.CurrencyConverter);
			AssertEquals(0.76m, Utilities.Round(converter.GetExchangeRate("USD", "AUD", DocumentsDataRegistry.Instance.IncludeCFXinExchangeRateOnQuotationPrinting.Value), 4));
			AssertEquals(1.2987m, Utilities.Round(converter.GetExchangeRate("AUD", "USD", DocumentsDataRegistry.Instance.IncludeCFXinExchangeRateOnQuotationPrinting.Value), 4));
			AssertEquals(0.7922m, Utilities.Round(converter.GetExchangeRate("EUR", "USD", DocumentsDataRegistry.Instance.IncludeCFXinExchangeRateOnQuotationPrinting.Value), 4));
			AssertEquals(1.2258m, Utilities.Round(converter.GetExchangeRate("USD", "EUR", DocumentsDataRegistry.Instance.IncludeCFXinExchangeRateOnQuotationPrinting.Value), 4));

			oneOffQuote.TT_RL_NKReceivalLocation = "AUSYD";
			oneOffQuote.TT_RL_NKDeliveryLocation = "USLAX";
			proxy = new AutoRatingProxy(oneOffQuote.RatingAdapter);
			proxy.ValuesCanBeSet = true;
			proxy.QuoteNumber = testQuote.TH_QuoteNumber;
			charges = new FreightAutoRater(new RatingContext()).AutoRate(proxy, CostSell.Revenue).RateInfoCollection;

			converter = new CurrencyConverterWithWithUplift(Factory, charges[0], Helper.CurrencyConverter);
			AssertEquals(0.76m, Utilities.Round(converter.GetExchangeRate("USD", "AUD", DocumentsDataRegistry.Instance.IncludeCFXinExchangeRateOnQuotationPrinting.Value), 4));
			AssertEquals(1.2987m, Utilities.Round(converter.GetExchangeRate("AUD", "USD", DocumentsDataRegistry.Instance.IncludeCFXinExchangeRateOnQuotationPrinting.Value), 4));
			AssertEquals(0.7922m, Utilities.Round(converter.GetExchangeRate("EUR", "USD", DocumentsDataRegistry.Instance.IncludeCFXinExchangeRateOnQuotationPrinting.Value), 4));
			AssertEquals(1.2258m, Utilities.Round(converter.GetExchangeRate("USD", "EUR", DocumentsDataRegistry.Instance.IncludeCFXinExchangeRateOnQuotationPrinting.Value), 4));
		}

		public void TestGetExchangeRate()
		{
			GlbCompany.CurrentCompany.SetCountry("AU");

			Helper.NewExchangeRate("EUR", Core.Constants.ExchangeRateTypes.Code.BuyRate, 0.62m);
			Helper.NewExchangeRate("EUR", Core.Constants.ExchangeRateTypes.Code.SellRate, 0.61m);
			Helper.NewExchangeRate("USD", Core.Constants.ExchangeRateTypes.Code.BuyRate, 0.77m);
			Helper.NewExchangeRate("USD", Core.Constants.ExchangeRateTypes.Code.SellRate, 0.76m);

			var converter = new CurrencyConverterWithWithUplift(Factory, null, Helper.CurrencyConverter);
			AssertEquals(0.6100m, Utilities.Round(converter.GetExchangeRate("EUR", "AUD"), 4));
			AssertEquals(1.6129m, Utilities.Round(converter.GetExchangeRate("AUD", "EUR"), 4));
			AssertEquals(0.7600m, Utilities.Round(converter.GetExchangeRate("USD", "AUD"), 4));
			AssertEquals(1.2987m, Utilities.Round(converter.GetExchangeRate("AUD", "USD"), 4));
			AssertEquals(0.7922m, Utilities.Round(converter.GetExchangeRate("EUR", "USD"), 4));
			AssertEquals(1.2258m, Utilities.Round(converter.GetExchangeRate("USD", "EUR"), 4));
			AssertEquals(1m, converter.GetExchangeRate("EUR", "EUR"));
			AssertEquals(1m, converter.GetExchangeRate("USD", "USD"));
			AssertEquals(1m, converter.GetExchangeRate("AUD", "AUD"));
			AssertEquals(0m, converter.GetExchangeRate("NZD", "AUD"));
			AssertEquals(0m, converter.GetExchangeRate("AUD", "NZD"));
			AssertEquals(1m, converter.GetExchangeRate("NZD", "NZD"));
			AssertEquals(0m, converter.GetExchangeRate("AUD", "###"));
			AssertEquals(0m, converter.GetExchangeRate("###", "AUD"));

			var client = Helper.NewOrgHeader();
			var testQuote = Helper.NewQuote(client);
			testQuote.AddRateEntry("ORG", "ALL", "AUSYD", "").AddRateLine("ODOC").Calculator[Calculator.Items.Operator.BAS] = (ZDecimal)10m;
			testQuote.AddRateEntry("ORG", "ALL", "USLAX", "").AddRateLine("ODOC").Calculator[Calculator.Items.Operator.BAS] = (ZDecimal)10m;
			testQuote.TH_AirCFX = 1m;
			testQuote.TH_ExportAirCFX = 2m;
			testQuote.TH_SeaCFX = 3m;
			testQuote.TH_ExportSeaCFX = 4m;
			Factory.Save();
			testQuote.TH_OneTimeQuote = true;
			var oneOffQuote = testQuote.CurrentOneOffQuote;
			oneOffQuote.TT_TransportMode = Core.Constants.TransportModes.Air;
			oneOffQuote.TT_ContainerMode = Core.Constants.ContainerModes.Loose;
			oneOffQuote.TT_RL_NKReceivalLocation = "USLAX";
			oneOffQuote.TT_RL_NKDeliveryLocation = "AUSYD";
			oneOffQuote.TT_ActualWeight = 100m;
			oneOffQuote.TT_UnitOfWeight = RatingConstants.Units.KG;
			oneOffQuote.TT_ActualVolume = 0.5m;
			oneOffQuote.TT_UnitOfVolume = RatingConstants.Units.M3;
			oneOffQuote.PickUpDocAddress.OrganisationPK = client.PK;

			Factory.Save();

			var proxy = new AutoRatingProxy(oneOffQuote.RatingAdapter);
			proxy.ValuesCanBeSet = true;
			proxy.QuoteNumber = testQuote.TH_QuoteNumber;

			var charges = new FreightAutoRater(new RatingContext()).AutoRate(proxy, CostSell.Revenue).RateInfoCollection;
			AssertEquals("Charges exist", 1, charges.Count);

			converter = new CurrencyConverterWithWithUplift(Factory, charges[0], Helper.CurrencyConverter);
			AssertEquals(0.7524m, Utilities.Round(converter.GetExchangeRate("USD", "AUD"), 4));
			AssertEquals(1.2857m, Utilities.Round(converter.GetExchangeRate("AUD", "USD"), 4));
			AssertEquals(0.7843m, Utilities.Round(converter.GetExchangeRate("EUR", "USD"), 4));
			AssertEquals(1.2135m, Utilities.Round(converter.GetExchangeRate("USD", "EUR"), 4));
			AssertEquals(1m, converter.GetExchangeRate("USD", "USD"));
			AssertEquals(1m, converter.GetExchangeRate("AUD", "AUD"));
			AssertEquals(0m, converter.GetExchangeRate("NZD", "AUD"));

			oneOffQuote.TT_RL_NKReceivalLocation = "AUSYD";
			oneOffQuote.TT_RL_NKDeliveryLocation = "USLAX";
			proxy = new AutoRatingProxy(oneOffQuote.RatingAdapter);
			proxy.ValuesCanBeSet = true;
			proxy.QuoteNumber = testQuote.TH_QuoteNumber;
			charges = new FreightAutoRater(new RatingContext()).AutoRate(proxy, CostSell.Revenue).RateInfoCollection;

			converter = new CurrencyConverterWithWithUplift(Factory, charges[0], Helper.CurrencyConverter);
			AssertEquals(0.7448m, Utilities.Round(converter.GetExchangeRate("USD", "AUD"), 4));
			AssertEquals(1.2727m, Utilities.Round(converter.GetExchangeRate("AUD", "USD"), 4));
			AssertEquals(0.7764m, Utilities.Round(converter.GetExchangeRate("EUR", "USD"), 4));
			AssertEquals(1.2013m, Utilities.Round(converter.GetExchangeRate("USD", "EUR"), 4));

			oneOffQuote.TT_TransportMode = Core.Constants.TransportModes.Sea;
			oneOffQuote.TT_ContainerMode = Core.Constants.ContainerModes.LCL;
			oneOffQuote.TT_RL_NKReceivalLocation = "USLAX";
			oneOffQuote.TT_RL_NKDeliveryLocation = "AUSYD";
			proxy = new AutoRatingProxy(oneOffQuote.RatingAdapter);
			proxy.ValuesCanBeSet = true;
			proxy.QuoteNumber = testQuote.TH_QuoteNumber;
			charges = new FreightAutoRater(new RatingContext()).AutoRate(proxy, CostSell.Revenue).RateInfoCollection;

			converter = new CurrencyConverterWithWithUplift(Factory, charges[0], Helper.CurrencyConverter);
			AssertEquals(0.7372m, Utilities.Round(converter.GetExchangeRate("USD", "AUD"), 4));
			AssertEquals(1.2597m, Utilities.Round(converter.GetExchangeRate("AUD", "USD"), 4));
			AssertEquals(0.7684m, Utilities.Round(converter.GetExchangeRate("EUR", "USD"), 4));
			AssertEquals(1.1890m, Utilities.Round(converter.GetExchangeRate("USD", "EUR"), 4));

			oneOffQuote.TT_RL_NKReceivalLocation = "AUSYD";
			oneOffQuote.TT_RL_NKDeliveryLocation = "USLAX";
			proxy = new AutoRatingProxy(oneOffQuote.RatingAdapter);
			proxy.ValuesCanBeSet = true;
			proxy.QuoteNumber = testQuote.TH_QuoteNumber;
			charges = new FreightAutoRater(new RatingContext()).AutoRate(proxy, CostSell.Revenue).RateInfoCollection;

			converter = new CurrencyConverterWithWithUplift(Factory, charges[0], Helper.CurrencyConverter);
			AssertEquals(0.7296m, Utilities.Round(converter.GetExchangeRate("USD", "AUD"), 4));
			AssertEquals(1.2468m, Utilities.Round(converter.GetExchangeRate("AUD", "USD"), 4));
			AssertEquals(0.7605m, Utilities.Round(converter.GetExchangeRate("EUR", "USD"), 4));
			AssertEquals(1.1768m, Utilities.Round(converter.GetExchangeRate("USD", "EUR"), 4));
		}

		public void TestConvert()
		{
			Helper.NewExchangeRate("EUR", Core.Constants.ExchangeRateTypes.Code.BuyRate, 0.62m);
			Helper.NewExchangeRate("EUR", Core.Constants.ExchangeRateTypes.Code.SellRate, 0.61m);
			Helper.NewExchangeRate("USD", Core.Constants.ExchangeRateTypes.Code.BuyRate, 0.77m);
			Helper.NewExchangeRate("USD", Core.Constants.ExchangeRateTypes.Code.SellRate, 0.76m);

			var converter = new CurrencyConverterWithWithUplift(Factory, null, Helper.CurrencyConverter);

			AssertEquals(1315.79m, converter.Convert(1000m, "USD", "AUD"));
			AssertEquals(770.00m, converter.Convert(1000m, "AUD", "USD"));
			AssertEquals(1262.30m, converter.Convert(1000m, "EUR", "USD"));
			AssertEquals(815.79m, converter.Convert(1000m, "USD", "EUR"));
			AssertEquals(1000.00m, converter.Convert(1000m, "AUD", "AUD"));
			AssertEquals(1000.00m, converter.Convert(1000m, "USD", "USD"));
			AssertEquals(1000.00m, converter.Convert(1000m, "NZD", "NZD"));
			AssertEquals(0m, converter.Convert(1000m, "USD", "NZD"));
			AssertEquals(0m, converter.Convert(1000m, "NZD", "USD"));
		}

		public void TestConvertReciprocal()
		{
			GlbCompany.CurrentCompany.GC_IsReciprocal = true;
			try
			{
				Helper.NewExchangeRate("EUR", Core.Constants.ExchangeRateTypes.Code.BuyRate, 1m / 0.62m);
				Helper.NewExchangeRate("EUR", Core.Constants.ExchangeRateTypes.Code.SellRate, 1m / 0.61m);
				Helper.NewExchangeRate("USD", Core.Constants.ExchangeRateTypes.Code.BuyRate, 1m / 0.77m);
				Helper.NewExchangeRate("USD", Core.Constants.ExchangeRateTypes.Code.SellRate, 1m / 0.76m);

				var converter = new CurrencyConverterWithWithUplift(Factory, null, Helper.CurrencyConverter);

				AssertEquals(1315.79m, converter.Convert(1000m, "USD", "AUD"));
				AssertEquals(770.00m, converter.Convert(1000m, "AUD", "USD"));
				AssertEquals(1262.30m, converter.Convert(1000m, "EUR", "USD"));
				AssertEquals(815.79m, converter.Convert(1000m, "USD", "EUR"));
				AssertEquals(1000.00m, converter.Convert(1000m, "AUD", "AUD"));
				AssertEquals(1000.00m, converter.Convert(1000m, "USD", "USD"));
				AssertEquals(1000.00m, converter.Convert(1000m, "NZD", "NZD"));
				AssertEquals(0m, converter.Convert(1000m, "USD", "NZD"));
				AssertEquals(0m, converter.Convert(1000m, "NZD", "USD"));
			}
			finally
			{
				GlbCompany.CurrentCompany.GC_IsReciprocal = false;
			}
		}

		#region Implementation

		protected TestHelper Helper
		{
			get
			{
				if (fHelper == null)
				{
					fHelper = new TestHelper(Factory);
				}

				return fHelper;
			}
		}

		TestHelper fHelper;

		#endregion
	}
}
