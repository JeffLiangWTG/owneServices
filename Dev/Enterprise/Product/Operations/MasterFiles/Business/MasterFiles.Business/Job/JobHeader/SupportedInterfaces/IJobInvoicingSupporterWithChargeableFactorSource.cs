using Enterprise.Registry.Business;

namespace Enterprise.MasterFiles.Business
{
	public interface IJobInvoicingSupporterWithChargeableFactorSource : IJobInvoicingSupporter
	{
		ChargeableFactorSource ChargeableFactorSource { get; }
	}
}
