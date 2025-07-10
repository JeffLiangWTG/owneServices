using System;
using CargoWise.EntityFramework;

namespace Enterprise.Rating.Business
{
	public class ChooserRateEntryCollection : NonPersistentBusinessObjectCollection<ChooserRateEntry>
	{
		public ChooserRateEntryCollection(BusinessObjectFactory factory) : base(factory)
		{
		}

		protected override bool AllowNewCore => false;
		protected override bool AllowRemoveCore => false;

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			throw new InvalidOperationException();
		}
	}
}
