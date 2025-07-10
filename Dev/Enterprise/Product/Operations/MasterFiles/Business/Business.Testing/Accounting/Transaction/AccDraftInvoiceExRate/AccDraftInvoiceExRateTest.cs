using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(AccDraftInvoiceExRate))]
	public class AccDraftInvoiceExRateTest : EnterpriseBusinessObjectTestCase
	{
		public void TestSetConcurrencyPolicy()
		{
			var exRate = (AccDraftInvoiceExRate)GetNewBusinessObject();

			AssertConcurrencyPolicy(exRate.AIE_AIH_HeaderInfo, ConcurrencyPolicy.Strict);
			AssertConcurrencyPolicy(exRate.AIE_ExchangeRateInfo, ConcurrencyPolicy.Strict);
			AssertConcurrencyPolicy(exRate.AIE_IsReciprocalInfo, ConcurrencyPolicy.Strict);
			AssertConcurrencyPolicy(exRate.AIE_RX_NKRateCurrencyInfo, ConcurrencyPolicy.Strict);

			void AssertConcurrencyPolicy(ZPropertyInfo propertyInfo, ConcurrencyPolicy policy)
			{
				AssertEquals(propertyInfo.Name, policy, propertyInfo.ConcurrencyPolicy);
			}
		}

		public void TestSetDefaultValues()
		{
			var exRate = (AccDraftInvoiceExRate)GetNewBusinessObject();

			AssertEquals(ZGuid.Empty, exRate.AIE_AIH_Header);
			AssertEquals(1m, exRate.AIE_ExchangeRate);
			AssertEquals(false, exRate.AIE_IsReciprocal);
			AssertEquals(ZString.Empty, exRate.AIE_RX_NKRateCurrency);
		}

		protected override BusinessObject GetNewBusinessObject() => Factory.New<AccDraftInvoiceExRate>();
	}
}
