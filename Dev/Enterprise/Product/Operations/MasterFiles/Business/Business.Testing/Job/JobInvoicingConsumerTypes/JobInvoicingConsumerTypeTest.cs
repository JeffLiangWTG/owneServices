using CargoWise.EntityFramework.Testing;
using Enterprise.Environment;
using Enterprise.Security;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.MasterFiles.Business.Testing
{
	public abstract class JobInvoicingConsumerTypeTest : TestCaseWithFactory
	{
		public virtual void TestControllerID()
		{
			var expectedID = ExpectedControllerID;
			if (expectedID != null)
			{
				AssertEquals(expectedID, ConsumerType.ControllerID);
			}
			else
			{
				Assert("The unit test in question has not implemented ExpectedControllerID", true);
			}
		}

		public virtual void TestIsTransportModeSupported()
		{
			AssertEquals(false, ConsumerType.IsTransportModeSupported);
		}

		public virtual void TestIsDirectionSupported()
		{
			AssertEquals(false, ConsumerType.IsDirectionSupported);
		}

		protected virtual int CheckChargeBranchDefaultingRuleListElements(int intexToStartFrom)
		{
			return intexToStartFrom;
		}

		public void TestDistanceCalculationCheckpoint()
		{
			AssertEquals(ExpectedDistanceCalculationCheckpoint, ConsumerType.DistanceCalculationCheckpoint);
		}

		public virtual void TestInvoicingPrintingApplicable()
		{
			AssertEquals(true, ConsumerType.InvoicingPrintingApplicable(null));
		}

		public virtual void TestCreditStatusApplicable()
		{
			AssertEquals(true, ConsumerType.CreditStatusApplicable(null));
		}

		public void TestShouldExcludeFromPeriodicBillingByDefault() => TestShouldExcludeFromPeriodicBillingByDefaultCore();

		protected virtual void TestShouldExcludeFromPeriodicBillingByDefaultCore() => AssertEquals(false, ConsumerType.ShouldExcludeFromPeriodicBillingByDefault);

		public virtual void TestProfitLossApplicable()
		{
			AssertEquals(true, ConsumerType.ProfitLossApplicable(null));
		}

		public virtual void TestAllowRevenuePosting()
		{
			AssertEquals(true, ConsumerType.AllowRevenuePosting(null));
		}

		public virtual void TestAllowCostPosting()
		{
			AssertEquals(true, ConsumerType.AllowCostPosting(null));
		}

		public virtual void TestMenuName()
		{
			AssertEquals(null, ConsumerType.MenuName(null));
		}

		public virtual void TestDisplayName()
		{
			AssertEquals("Billing", ConsumerType.DisplayName(null));
		}

		public virtual void TestJobInvoicingCheckPoint()
		{
			AssertEquals(Env.Security.None, ConsumerType.JobInvoicingCheckPoint);
		}

		public virtual void TestRevenueChargeDescription()
		{
			AssertEquals("Revenue", ConsumerType.RevenueChargeDescription(null));
		}

		public virtual void TestExcludeFromClientVisibleOption()
		{
			AssertEquals("ExcludeFromClientVisibleOption should be false", false, ConsumerType.ExcludeFromClientVisibleOption);
		}

		public virtual void TestSupportsWiseRates()
		{
			AssertEquals("SupportsWiseRates should be false", false, ConsumerType.SupportsWiseRates);
		}

		protected virtual ControllerID ExpectedControllerID => null;
		protected abstract SecurityCheckpoint ExpectedDistanceCalculationCheckpoint { get; }
		protected abstract JobInvoicingConsumerType GetJobInvoicingConsumerType();

		protected JobInvoicingConsumerType ConsumerType
		{
			get { return consumerType ?? (consumerType = GetJobInvoicingConsumerType()); }
		}
		protected JobInvoicingConsumerType consumerType;
	}
}
