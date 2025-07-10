using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.MasterFiles.Business.Testing
{
	public abstract class CurrencyConverterTestCase : TestCaseWithFactory
	{
		#region Implementation

		protected override void SetUp()
		{
			base.SetUp();
			// Initialise
			RefCurrency currency = DecoratedCurrency;
			RefExchangeRate exchangeRate = DecoratedExchangeRate;
		}

		protected RefCurrency LocalCurrency
		{
			get { return GlbCompany.CurrentCompany.LocalCurrency; }
		}

		protected RefCurrency DecoratedCurrency
		{
			get
			{
				if (fDecoratedCurrency == null)
				{
					fDecoratedCurrency = Factory.New<RefCurrency>();
					fDecoratedCurrency.RX_Code = "XXX";
					fDecoratedCurrency.RX_Desc = "Description";
					fDecoratedCurrency.RX_IsSystem = true;
					fDecoratedCurrency.RX_SubUnitName = "SubUnit";
					fDecoratedCurrency.RX_SubUnitRatio = 100;
					fDecoratedCurrency.RX_Symbol = "X";
					fDecoratedCurrency.RX_UnitName = "XXX";
				}
				return fDecoratedCurrency;
			}
		}

		RefCurrency fDecoratedCurrency;

		protected RefExchangeRate DecoratedExchangeRate
		{
			get
			{
				if (fDecoratedExchangeRate == null)
				{
					fDecoratedExchangeRate = SetExchangeRate(GlbCompany.CurrentCompany, "CUS", new ZDateTime(2003, 11, 14), new ZDateTime(2003, 11, 15), 0.7m, DecoratedCurrency);
				}

				return fDecoratedExchangeRate;
			}
		}

		RefExchangeRate fDecoratedExchangeRate;

		protected RefExchangeRate SetExchangeRate(GlbCompany company, ZString rateType, ZDateTime startDate, ZDateTime endDate, ZDecimal rate, RefCurrency foreignCurrency)
		{
			RefExchangeRate result = Factory.New<RefExchangeRate>();

			result.RE_GC = company.PK;
			result.RE_RX_NKExCurrency = foreignCurrency.RX_Code;
			result.RE_StartDate = startDate;
			result.RE_ExpiryDate = endDate;
			result.RE_SellRate = rate;
			result.RE_ExRateType = rateType;

			return result;
		}

		#endregion
	}
}
