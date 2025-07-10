using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business.Testing
{
	public sealed class OrgSalesTestHelper
	{
		public OrgSalesTestHelper(BusinessObjectFactory factory)
		{
			this.Factory = factory;
		}

		readonly BusinessObjectFactory Factory;

		#region Organisations

		public OrgHeader NewOrgHeader()
		{
			OrgHeader result = Factory.New<OrgHeader>();
			result.OH_FullName = String.Format("Test Client #{0}", ++OrgIndex);
			result.MainAddress.OA_Address1 = String.Format("{0} Fake Street", 100 + OrgIndex);
			result.MainAddress.OA_City = "Sydney";
			result.MainAddress.OA_State = "NSW";
			result.MainAddress.OA_PostCode = "2000";
			result.OH_RL_NKClosestPort = "AUSYD";
			result.OH_Code = String.Format("TESTORG{0}", OrgIndex);

			return result;
		}

		int OrgIndex;

		#endregion

		#region Currencies

		public RefExchangeRate NewExchangeRate(RefCurrency currency, ZString rateType, ZDecimal rate)
		{
			RefExchangeRate exchangeRate = currency.ExchangeRates.AddNew();
			exchangeRate.RE_ExRateType = rateType;
			exchangeRate.RE_SellRate = rate;
			exchangeRate.RE_StartDate = ZDateTime.Today.AddMonths(-1);
			exchangeRate.RE_ExpiryDate = ZDateTime.Today.AddMonths(1);
			exchangeRate.RE_GC = GlbCompany.CurrentCompany.PK;

			return exchangeRate;
		}

		public RefExchangeRate NewExchangeRate(ZString currency, ZString rateType, ZDecimal rate)
		{
			return NewExchangeRate(Currencies[currency], rateType, rate);
		}

		public CurrencyFactory Currencies
		{
			get
			{
				if (fCurrencies == null)
				{
					fCurrencies = new CurrencyFactory(Factory);
				}

				return fCurrencies;
			}
		}

		CurrencyFactory fCurrencies;

		public class CurrencyFactory
		{
			public CurrencyFactory(BusinessObjectFactory factory)
			{
				this.Factory = factory;
			}

			public RefCurrency this[string currency]
			{
				get { return Factory.LoadFromNaturalKey<RefCurrency>(RefCurrencySchema.RX_Code, currency); }
			}

			readonly BusinessObjectFactory Factory;
		}

		#endregion

		public OrgSales NewOrgSales(OrgHeader primary, OrgHeader supplier, OrgHeader buyer)
		{
			OrgSales result = Factory.New<OrgSales>();

			if (primary != null)
			{
				result.OW_OH_Primary = primary.PK;
			}

			if (supplier != null)
			{
				result.OW_OH_Supplier = supplier.PK;
			}

			if (buyer != null)
			{
				result.OW_OH_Buyer = buyer.PK;
			}

			return result;
		}

		public OrgTradeDetail NewOrgTradeDetail(OrgSales sale, string recurrenceType, short jobCount, decimal revenue, string currency)
		{
			OrgTradeDetail result = sale.TradeDetails.AddNew();
			result.ProspectDetail.PAP_RecurrenceType = recurrenceType;
			result.CurrentProspectPeriod.PAS_RepeatsMnth = jobCount;
			result.CurrentProspectPeriod.PAS_EstimatedProfit = revenue;
			result.CurrentProspectPeriod.PAS_RX_NKCurrency = currency;

			return result;
		}
	}
}
