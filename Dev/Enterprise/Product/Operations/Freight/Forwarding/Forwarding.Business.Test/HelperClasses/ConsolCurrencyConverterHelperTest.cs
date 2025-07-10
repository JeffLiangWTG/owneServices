using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business.AWB;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Forwarding.Business.Testing
{
	sealed class ConsolCurrencyConverterHelperTest : TestCaseWithFactory
	{
		public void TestTryConvertAmountUsingExchangeRateFromFreightCostOrSchedule()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Australia))
			{
				var voyage = Factory.New<JobVoyage>();
				voyage.JV_AirSeaRoad = Constants.TransportModes.Sea;

				voyage.Origins.AddNew().JA_RL_NKPortOfLoading = "AUSYD";
				voyage.Destinations.AddNew().JB_RL_NKPortOfDischarge = "USLAX";

				var voyageExRate1 = voyage.ExRates.AddNew();
				voyageExRate1.E8_RX_NKExCurrency = "USD";
				voyageExRate1.E8_VoyageExchangeRate = 1.024m;

				var voyageExRate2 = voyage.ExRates.AddNew();
				voyageExRate2.E8_RX_NKExCurrency = "EUR";
				voyageExRate2.E8_VoyageExchangeRate = 2.222m;

				var voyageExRate3 = voyage.ExRates.AddNew();
				voyageExRate3.E8_RX_NKExCurrency = "NZD";
				voyageExRate3.E8_VoyageExchangeRate = 2.048m;

				var consol = Factory.New<ForwardingConsol>();
				consol.JK_TransportMode = Constants.TransportModes.Sea;
				consol.JK_RL_NKLoadPort = "AUSYD";
				consol.JK_RL_NKDischargePort = "USLAX";
				consol.Transports[0].JW_JX = voyage.Sailings[0].PK;

				var header = Factory.New<ConsolExportAWBHeader>();
				header.EH_ParentID = consol.PK;
				header.EH_Currency = "USD";

				var chargeCode = Factory.NewWithValidTestData<AccChargeCode>();
				CreateConsolCost(consol, chargeCode.PK, "USD", 1.111m);

				var amount = consol.TryConvertAmountUsingExchangeRateFromFreightCostOrSchedule(100, "USD", header.EH_Currency);
				AssertEquals("Precondition: Charge: USD, Header: USD, should not convert", 100m, amount);

				amount = consol.TryConvertAmountUsingExchangeRateFromFreightCostOrSchedule(100, "AUD", header.EH_Currency);
				AssertEquals("Charge: AUD, Header: USD, convert amount from Local to Foreign", 102.40m, amount);

				header.EH_Currency = "AUD";
				amount = consol.TryConvertAmountUsingExchangeRateFromFreightCostOrSchedule(100, "AUD", header.EH_Currency);
				AssertEquals("Charge: AUD, Header: AUD, should not convert", 100m, amount);

				amount = consol.TryConvertAmountUsingExchangeRateFromFreightCostOrSchedule(100, "USD", header.EH_Currency);
				AssertEquals("Charge: USD, Header: AUD, convert amount from Foreign to Local", 97.66m, amount);

				header.EH_Currency = "NZD";
				amount = consol.TryConvertAmountUsingExchangeRateFromFreightCostOrSchedule(100, "EUR", header.EH_Currency);
				AssertEquals("Charge: EUR, Header: NZD, convert amount from Foreign to Foreign", 92.17m, amount);
			}
		}

		BusinessObject CreateConsolCost(ForwardingConsol consol, ZGuid chargeCodePK, ZString currencyCode, ZDecimal exchangeRate)
		{
			var result = (BusinessObject)Factory.New<IJobConsolCost>();
			result[JobConsolCostSchema.E6_AC_ChargeCode] = chargeCodePK;
			result[JobConsolCostSchema.E6_GC] = GlbCompany.CurrentCompany.PK;

			result.SetContext(BusinessContext.EnableDirectSettingConsolCostParent);
			try
			{
				result[JobConsolCostSchema.E6_ParentID] = consol.PK;
				result[JobConsolCostSchema.E6_ParentTableCode] = JobConsolSchema.Constants.Prefix;
			}
			finally
			{
				result.RemoveContext(BusinessContext.EnableDirectSettingConsolCostParent);
			}

			if (!currencyCode.IsEmpty)
			{
				result[JobConsolCostSchema.E6_RX_NKCurrency] = currencyCode;
				result[JobConsolCostSchema.E6_ExchangeRate] = exchangeRate;
			}

			return result;
		}
	}
}
