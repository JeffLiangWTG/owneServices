using System;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.US.AMS.Business
{
	public class StowPlanDataBusinessObjectCollection<T> : NonPersistentBusinessObjectCollection<T> where T : NonPersistentBusinessObject
	{
		public StowPlanDataBusinessObjectCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		protected override bool AllowNewCore
		{
			get { return false; }
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			throw new NotImplementedException();
		}
	}
}
