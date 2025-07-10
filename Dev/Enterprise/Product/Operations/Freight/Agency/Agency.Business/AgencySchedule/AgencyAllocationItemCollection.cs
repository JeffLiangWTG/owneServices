using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Freight.Agency.Business
{
	public abstract partial class AgencyAllocationItemCollection<T, U> : NonPersistentBusinessObjectCollection<T>
		where T : AgencyAllocationItem<U>
		where U : BusinessObject
	{
		protected AgencyAllocationItemCollection(BusinessObjectCollection collection)
			: base(collection.Factory)
		{
			this.collection = collection;
			collection.CountChanged += new CollectionCountChangedEventHandler(Collection_CountChanged);
		}

		public sealed override void Load()
		{
			foreach (T allocation in this)
			{
				UnHookKeyChangeEvents(allocation.BizObj);
			}

			RemoveAllButLeaveRelationshipsIntact();

			foreach (U bizObj in collection)
			{
				if (ShouldBeInThisCollection(bizObj))
				{
					Add(CachedWrapElement(bizObj));
				}

				HookKeyChangeEvents(bizObj);
			}
		}

		#region Implementation

		protected virtual void HookKeyChangeEvents(U bizObj)
		{
		}

		protected virtual void UnHookKeyChangeEvents(U bizObj)
		{
		}

		protected void KeyValueChangedHandler(object sender, EventArgs e)
		{
			U bizObj = (U)sender;

			if (ShouldBeInThisCollection(bizObj))
			{
				if (!cache.ContainsKey(bizObj.PK))
				{
					Add(CachedWrapElement(bizObj));
				}
			}
			else
			{
				if (cache.ContainsKey(bizObj.PK))
				{
					Remove(cache[bizObj.PK]);
				}
			}
		}

		protected override void OnAdded(BusinessObject bizObj)
		{
			T item = (T)bizObj;
			base.OnAdded(item);
			BusinessObject innerBizObj = item.BizObj;

			if (innerBizObj != null && !cache.ContainsKey(innerBizObj.PK))
			{
				cache.Add(innerBizObj.PK, item);
			}
		}

		protected override void OnRemoved(BusinessObject bizO)
		{
			T item = (T)bizO;
			base.OnRemoved(item);

			BusinessObject innerBizObj = item.BizObj;
			if (innerBizObj != null)
			{
				cache.Remove(innerBizObj.PK);
			}
		}

		protected virtual bool ShouldBeInThisCollection(U bizObj)
		{
			return true;
		}

		protected sealed override BusinessObject CreateNonPersistentBusinessObject()
		{
			throw new NotSupportedException();
		}

		protected sealed override bool AllowNewCore
		{
			get { return false; }
		}

		protected sealed override bool AllowRemoveCore
		{
			get { return false; }
		}

		protected abstract T WrapElement(U bizObj);
		protected readonly BusinessObjectCollection collection;

		void Collection_CountChanged(object sender, CollectionCountChangedEventArgs e)
		{
			U bizObj = (U)e.BizObject;

			if (e.ItemAdded)
			{
				if (ShouldBeInThisCollection(bizObj))
				{
					Add(CachedWrapElement(bizObj));
				}
				HookKeyChangeEvents(bizObj);
			}
			else if (e.ItemRemoved)
			{
				if (cache.ContainsKey(bizObj.PK))
				{
					Remove(cache[bizObj.PK]);
				}
				UnHookKeyChangeEvents(bizObj);
			}
		}

		T CachedWrapElement(U bizObj)
		{
			T result;

			if (!cache.TryGetValue(bizObj.PK, out result))
			{
				result = WrapElement(bizObj);
				cache.Add(bizObj.PK, result);
			}

			return result;
		}

		readonly Dictionary<ZGuid, T> cache = new Dictionary<ZGuid, T>();

		#endregion
	}
}

#region Test
#if DEBUG

#region Test Methods

namespace Enterprise.Freight.Agency.Business
{
	partial class AgencyAllocationItemCollection<T, U>
	{
		public T GetWrapperForTesting(U bizObj)
		{
			return cache[bizObj.PK];
		}
	}
}

#endregion
#endif
#endregion
