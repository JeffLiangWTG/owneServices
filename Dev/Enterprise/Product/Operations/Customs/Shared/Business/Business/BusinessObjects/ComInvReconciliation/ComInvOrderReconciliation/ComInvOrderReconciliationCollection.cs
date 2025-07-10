using CargoWise.EntityFramework;

namespace Enterprise.Customs.Business
{
	public class ComInvOrderReconciliationCollection : BusinessObjectCollection<ComInvOrderReconciliation>
	{
		public ComInvOrderReconciliationCollection(BusinessObjectFactory factory) : base(factory)
		{
		}

		public ComInvOrderReconciliationCollection(BusinessObjectFactory factory, ZQuery filter) : base(factory, filter)
		{
		}
	}
}
