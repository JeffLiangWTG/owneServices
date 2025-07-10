using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Business.Testing
{
	sealed class BaseJobVoyageExRateValidationTest : BusinessObjectValidationTestCase
	{
		#region TestE8_RX_NKExCurrency

		public void TestE8_RX_NKExCurrency()
		{
			Rate.E8_RX_NKExCurrency = ZString.Empty;
			AssertHasError(Rate.E8_RX_NKExCurrencyInfo, "Please enter a " + Rate.E8_RX_NKExCurrencyInfo.Description + ".");

			Rate.E8_RX_NKExCurrency = OtherCurrency.RX_Code;
			AssertNoErrors(rate.E8_RX_NKExCurrencyInfo);

			Rate.E8_RX_NKExCurrency = LocalCurrency.RX_Code;
			AssertHasError(Rate.E8_RX_NKExCurrencyInfo, "You cant select your local currency here.");

			VoyageExRate rate2 = Voyage.ExRates.AddNew();
			rate2.E8_RX_NKExCurrency = OtherCurrency.RX_Code;
			AssertNoErrors(rate2.E8_RX_NKExCurrencyInfo);

			Rate.E8_RX_NKExCurrency = OtherCurrency.RX_Code;
			AssertHasError(Rate.E8_RX_NKExCurrencyInfo, "Duplicate Currency.");

			Rate.E8_GC = ZGuid.NewZGuid();  // some other company
			Rate.Validation.ValidateE8_RX_NKExCurrency();
			AssertNoErrors("Same currency, but for a different company", Rate.E8_RX_NKExCurrencyInfo);
		}

		public void TestE8_RX_NKExCurrency_DifferentUnlocoSpecified()
		{
			var sailingVoyage = Factory.NewWithValidTestData<JobVoyage>();
			sailingVoyage.Origins.AddNew().JA_RL_NKPortOfLoading = "TWTPE";
			sailingVoyage.Destinations.AddNew().JB_RL_NKPortOfDischarge = "ITNAP";

			var exchangeRate = sailingVoyage.ExRates.AddNew();
			var exchangeRate1 = sailingVoyage.ExRates.AddNew();

			exchangeRate.E8_RX_NKExCurrency = "USD";
			exchangeRate1.E8_RX_NKExCurrency = "USD";
			exchangeRate.Validation.ValidateE8_RX_NKExCurrency();
			exchangeRate1.Validation.ValidateE8_RX_NKExCurrency();
			AssertHasErrors("Same currency, and no Port UNLOCO specified", exchangeRate.E8_RX_NKExCurrencyInfo);
			AssertHasErrors("Same currency, and no Port UNLOCO specified", exchangeRate1.E8_RX_NKExCurrencyInfo);

			exchangeRate.E8_RL_NKPort = "ITNAP";
			exchangeRate.Validation.ValidateE8_RX_NKExCurrency();
			exchangeRate1.Validation.ValidateE8_RX_NKExCurrency();
			AssertNoErrors("Same currency, but one Port UNLOCO specified", exchangeRate.E8_RX_NKExCurrencyInfo);
			AssertNoErrors("Same currency, but one Port UNLOCO specified", exchangeRate1.E8_RX_NKExCurrencyInfo);

			exchangeRate1.E8_RL_NKPort = "TWTPE";
			exchangeRate.Validation.ValidateE8_RX_NKExCurrency();
			exchangeRate1.Validation.ValidateE8_RX_NKExCurrency();
			AssertNoErrors("Same currency, but different Port UNLOCO specified", exchangeRate.E8_RX_NKExCurrencyInfo);
			AssertNoErrors("Same currency, but different Port UNLOCO specified", exchangeRate1.E8_RX_NKExCurrencyInfo);

			exchangeRate.E8_RL_NKPort = "TWTPE";
			exchangeRate.Validation.ValidateE8_RX_NKExCurrency();
			exchangeRate1.Validation.ValidateE8_RX_NKExCurrency();
			AssertHasErrors("Same currency, and same Port UNLOCO specified", exchangeRate.E8_RX_NKExCurrencyInfo);
			AssertHasErrors("Same currency, and same Port UNLOCO specified", exchangeRate1.E8_RX_NKExCurrencyInfo);
		}

		#endregion

		#region TestE8_VoyageExchangeRate

		public void TestE8_VoyageExchangeRate()
		{
			Rate.E8_VoyageExchangeRate = 0m;
			AssertHasError(Rate.E8_VoyageExchangeRateInfo, "You must enter a non-zero exchange rate.");

			Rate.E8_VoyageExchangeRate = 1m;
			AssertNoErrors(Rate.E8_VoyageExchangeRateInfo);

			Rate.E8_VoyageExchangeRate = -1m;
			AssertHasError(Rate.E8_VoyageExchangeRateInfo, "You cant have a negative exchange rate.");
		}

		#endregion

		#region TestE8_RL_NKPort

		public void TestE8_RL_NKPortvalidation()
		{
			var sailingVoyage = Factory.NewWithValidTestData<JobVoyage>();
			sailingVoyage.Origins.AddNew().JA_RL_NKPortOfLoading = "TWTPE";
			sailingVoyage.Origins.AddNew().JA_RL_NKPortOfLoading = "ITVCE";
			sailingVoyage.Destinations.AddNew().JB_RL_NKPortOfDischarge = "ITNAP";
			sailingVoyage.Destinations.AddNew().JB_RL_NKPortOfDischarge = "ITVCE";

			sailingVoyage.ExRates.AddNew().E8_RL_NKPort = "TWTPE";
			AssertEquals("Precondition: Port is TWTPE", "TWTPE", sailingVoyage.ExRates[0].E8_RL_NKPort);
			AssertNoErrors("No error thrown when Port UNLOCO is a Load Port", sailingVoyage.ExRates[0].E8_RL_NKPortInfo);

			sailingVoyage.ExRates.AddNew().E8_RL_NKPort = "ITVCE";
			AssertEquals("Precondition: Port is ITVCE", "ITVCE", sailingVoyage.ExRates[1].E8_RL_NKPort);
			AssertNoErrors("No error thrown when Port UNLOCO is both a Load and Discharge Port", sailingVoyage.ExRates[1].E8_RL_NKPortInfo);

			sailingVoyage.ExRates.AddNew().E8_RL_NKPort = "ITNAP";
			AssertEquals("Precondition: Port is ITNAP", "ITNAP", sailingVoyage.ExRates[2].E8_RL_NKPort);
			AssertNoErrors("No error thrown when Port UNLOCO is a Discharge Port", sailingVoyage.ExRates[2].E8_RL_NKPortInfo);

			sailingVoyage.ExRates.AddNew().E8_RL_NKPort = "AUSYD";
			AssertEquals("Precondition: Port is AUSYD", "AUSYD", sailingVoyage.ExRates[3].E8_RL_NKPort);
			AssertHasErrors("Error thrown when Port UNLOCO isn't a port of call.", sailingVoyage.ExRates[3].E8_RL_NKPortInfo);
		}

		#endregion

		#region Implementation

		#region LocalCurrency

		RefCurrency LocalCurrency
		{
			get { return GlbCompany.CurrentCompany.LocalCurrency; }
		}

		#endregion

		#region OtherCurrency

		RefCurrency OtherCurrency
		{
			get
			{
				if (otherCurrency == null)
				{
					ZQuery query = new ZQuery(RefCurrencySchema.PK, SQLComparisonOperator.NotEqual, LocalCurrency.PK);
					query.AddToFilter(JoinCondition.And, RefCurrencySchema.RX_IsActive, true);
					otherCurrency = Factory.LoadTop1<RefCurrency>(query);
				}

				return otherCurrency;
			}
		}

		RefCurrency otherCurrency;

		#endregion

		#region Rate

		VoyageExRate Rate
		{
			get
			{
				if (rate == null)
				{
					rate = Voyage.ExRates.AddNew();
				}
				return rate;
			}
		}

		VoyageExRate rate;

		#endregion

		#region Voyage

		JobVoyage Voyage
		{
			get
			{
				if (voyage == null)
				{
					voyage = Factory.New<JobVoyage>();
				}

				return voyage;
			}
		}

		JobVoyage voyage;

		#endregion

		#endregion
	}
}
