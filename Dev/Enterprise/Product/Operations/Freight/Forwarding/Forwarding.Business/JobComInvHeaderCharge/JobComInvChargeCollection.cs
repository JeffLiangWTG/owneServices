namespace Enterprise.Freight.Forwarding.Orders.Business
{
	public class JobComInvChargeCollection : Customs.Common.JobComInvChargeCollection<JobComInvCharge>
	{
		public JobComInvChargeCollection(Customs.Common.ICommonInvoice parent)
			: base(parent)
		{
		}
	}
}
