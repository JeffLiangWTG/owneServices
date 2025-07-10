using Enterprise.Freight.Forwarding.Orders.Business;

namespace Enterprise.Customs.Business
{
	public class ComInvOrderLineReconciliationCollection : OrderLineCollection
	{
		public ComInvOrderLineReconciliationCollection(ComInvOrderReconciliation parentOrder) : base(parentOrder)
		{
		}

		public new ComInvOrderLineReconciliation this[int index]
		{
			get { return (ComInvOrderLineReconciliation)base[index]; }
		}

		public new ComInvOrderLineReconciliation AddNew()
		{
			return (ComInvOrderLineReconciliation)base.AddNew();
		}
	}
}
