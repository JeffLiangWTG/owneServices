using CargoWise.EntityFramework;

namespace Enterprise.Tracking.Business
{
	public class ContainerSummaryRowCollection : NonPersistentBusinessObjectCollection<ContainerSummaryRow>
	{
		public ContainerSummaryRowCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new ContainerSummaryRow();
		}

		protected override bool AllowNewCore
		{
			get { return false; }
		}
	}
}
