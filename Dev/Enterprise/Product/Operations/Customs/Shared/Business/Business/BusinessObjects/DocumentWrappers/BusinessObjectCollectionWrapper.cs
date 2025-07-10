using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.Business.DocumentWrappers
{
	public sealed class BusinessObjectCollectionWrapper<T> : NonPersistentBusinessObjectCollection<T> where T : NonPersistentBusinessObject
	{
		public BusinessObjectCollectionWrapper(IEnumerable<T> collection)
		{
			if (collection != null)
			{
				foreach (var obj in collection)
				{
					Add(obj);
				}
			}
		}

		public BusinessObjectCollectionWrapper()
		{
		}

		#region Overrides of NonPersistentBusinessObjectCollection

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			throw new NotSupportedException();
		}

		protected override bool AllowNewCore
		{
			get { return false; }
		}

		#endregion
	}
}
