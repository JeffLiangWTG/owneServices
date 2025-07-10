using System;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.US.Business
{
	public class ReconEDIMessageViewCollection : BusinessObjectCollection<EDIMessage>
	{
		public ReconEDIMessageViewCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		protected override bool AllowNewCore => false;

		protected override bool AllowRemoveCore => false;

		protected override BusinessObject AddNewCore()
		{
			throw new NotSupportedException("You cannot add a new message directly into a view collection.");
		}
	}
}
