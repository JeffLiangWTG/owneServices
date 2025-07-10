using Enterprise.Environment;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.Security;

namespace Enterprise.Warehouse.Transactions.Business
{
	public class WhsReceiveInvoicingSupporter : WhsDocketInvoicingSupporter
	{
		public WhsReceiveInvoicingSupporter(WhsReceive parent)
			: base(parent)
		{
		}

		public override JobInvoicingConsumerType ConsumerType => JobInvoicingConsumerTypes.WarehouseInwards;

		protected override SecurityCheckpoint GetAuditSecurityCore() => Env.Security.WhsReceiveAuditBilling;

		protected override SecurityCheckpoint GetJobInvoicingSecurityCore() => Env.Security.WhsReceiveJobInvoicing;

		protected override bool CreateAccountingJobOnSavingOfOperationsJobCore => !Parent.IsCreatedFromPickByBOM;

		protected override string GetReasonNotToAllowAutoRateCore(AutoRateOptions options = default)
			=> Parent.IsCreatedFromPickByBOM ? Res.GetString("e6d27fa5-a0cf-4f38-9f72-b57c5babee40", "Receive {0} is created from Pick Order and cannot be Auto Rated.", Parent.HumanReadableName) : base.GetReasonNotToAllowAutoRateCore();
	}
}
