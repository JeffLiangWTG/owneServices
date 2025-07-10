using System;
using Enterprise.Environment;
using Enterprise.Registry.Business;
using Enterprise.Security;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class AgencyShipmentDetentionInvoiceConsumerTypeTest : AgencyShipmentConsumerTypeTest
	{
		public void TestShouldCreateWIPsOrAccruals()
		{
			LinerAgencyDataRegistry.Instance.CreateWIPAccrualsForDetentionCharges.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			AssertEquals(true, JobInvoicingConsumerTypes.AgencyDetentionInvoice.ShouldCreateWIPs(null, ""));
			AssertEquals(true, JobInvoicingConsumerTypes.AgencyDetentionInvoice.ShouldCreateAccruals(null, ""));

			LinerAgencyDataRegistry.Instance.CreateWIPAccrualsForDetentionCharges.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			AssertEquals(false, JobInvoicingConsumerTypes.AgencyDetentionInvoice.ShouldCreateWIPs(null, ""));
			AssertEquals(false, JobInvoicingConsumerTypes.AgencyDetentionInvoice.ShouldCreateAccruals(null, ""));
		}

		protected override JobInvoicingConsumerType GetJobInvoicingConsumerType()
		{
			return JobInvoicingConsumerTypes.AgencyDetentionInvoice;
		}

		protected override SecurityCheckpoint ExpectedDistanceCalculationCheckpoint
		{
			get { return Env.Security.RoadDistanceCalculationServiceShipping; }
		}
	}
}
