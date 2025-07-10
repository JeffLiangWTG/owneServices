using System;
using CargoWise.EntityFramework;

namespace Enterprise.TransportBookings.Business
{
	public class InstructionToSelectFromForPrintingCollection : NonPersistentBusinessObjectCollection<InstructionToSelectFromForPrinting>
	{
		public InstructionToSelectFromForPrintingCollection(BusinessObjectFactory factory)
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
