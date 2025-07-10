using CargoWise.EntityFramework;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(RefExchangeRate.Loader))]
	sealed class RefExchangeRateLoaderTest : LoaderTestCase
	{
		public void TestGetCurrentCompanyEffectiveRateOn()
		{
			var rate = new RefExchangeRate.Loader(Factory).GetEffectiveRateOn(ZDateTime.Today, Core.Constants.CurrencyCodes.UnitedStates, Core.Constants.ExchangeRateTypes.Code.CustomsRate, GlbCompany.CurrentCompany.PK);
			if (rate == null)
			{
				rate = Factory.New<RefExchangeRate>();
				rate.RE_ExRateType = Core.Constants.ExchangeRateTypes.Code.CustomsRate;
				rate.RE_RX_NKExCurrency = Core.Constants.CurrencyCodes.UnitedStates;
				rate.RE_GC = GlbCompany.CurrentCompany.PK;
				rate.RE_StartDate = ZDate.Today;
				rate.RE_ExpiryDate = ZDate.Today;
			}
			rate = new RefExchangeRate.Loader(Factory).GetEffectiveRateOn(ZDateTime.Today, Core.Constants.CurrencyCodes.UnitedStates, Core.Constants.ExchangeRateTypes.Code.CustomsRate, GlbCompany.CurrentCompany.PK);
			AssertEquals(Core.Constants.ExchangeRateTypes.Code.CustomsRate, rate.RE_ExRateType);
			AssertEquals(Core.Constants.CurrencyCodes.UnitedStates, rate.RE_RX_NKExCurrency);
			AssertEquals(GlbCompany.CurrentCompany.PK, rate.RE_GC);
			Assert(rate.RE_StartDate.Date <= ZDate.Today);
			Assert(rate.RE_ExpiryDate.Date >= ZDate.Today);
		}

		public void TestReadOnly()
		{
			var rate = Factory.New<RefExchangeRate>();
			rate.RE_IsSystem = true;
			Assert("System Exchange Rate should be readonly", rate.ReadOnly);
			rate.RE_IsSystem = false;
			Assert(!rate.ReadOnly);
		}

		public void TestGetFilterByPK()
		{
			var guid = ZGuid.NewZGuid();
			var query = RefExchangeRate.Loader.GetFilterByPK(guid);
			var expectedSql = $@"RE_GC = '{GlbCompany.CurrentCompany.PK}' 
AND
RE_PK = '{guid}'
";
			AssertEquals(expectedSql, query.LiteralTextSqlFormatted);
		}

		protected override BusinessObject.Loader GetNewLoaderToTest() => new RefExchangeRate.Loader(Factory);
	}
}
