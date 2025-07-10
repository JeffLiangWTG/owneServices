using System;
using CargoWise.EntityFramework;

namespace Enterprise.MasterFiles.Business
{
	public class LinkedDGSubstanceInfoCollection : NonPersistentBusinessObjectCollection<LinkedDGSubstanceInfo>
	{
		public LinkedDGSubstanceInfoCollection(BusinessObjectFactory factory) : base(factory)
		{
		}

		protected override bool AllowNewCore => false;
		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			throw new NotSupportedException();
		}
	}
}
