using System;
using CargoWise.EntityFramework;

namespace Enterprise.Freight.Business
{
	public class ContainerToSelectFromForPrintingCollection : NonPersistentBusinessObjectCollection<ContainerToSelectFromForPrinting>
	{
		public ContainerToSelectFromForPrintingCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		protected override bool AllowNewCore
		{
			get { return false; }
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			throw new NotSupportedException();
		}
	}
}
