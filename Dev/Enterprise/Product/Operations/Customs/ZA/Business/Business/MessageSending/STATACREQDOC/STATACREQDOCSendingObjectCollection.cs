using System;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.ZA.Business
{
	public class STATACREQDOCSendingObjectCollection : NonPersistentBusinessObjectCollection<STATACREQDOCSendingObject>
	{
		public STATACREQDOCSendingObjectCollection(BusinessObjectFactory factory) : base(factory)
		{
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			throw new NotImplementedException();
		}
		protected override bool AllowNewCore => false;
	}
}
