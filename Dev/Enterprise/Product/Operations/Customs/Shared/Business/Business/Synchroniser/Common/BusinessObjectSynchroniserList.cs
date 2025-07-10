using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.Business
{
	public class BusinessObjectSynchroniserList : List<BusinessObjectSynchroniser>
	{
		#region Add

		public void Add(BusinessObjectSynchroniser newSync, bool enabled, bool detectEnabled)
		{
			newSync.SetEnabled(enabled, detectEnabled);
			Add(newSync);
		}

		public void AddIfNotExistsOtherwiseSetEnabled(BusinessObjectSynchroniser newSync, bool enabled, bool detectEnabled)
		{
			var existing = Find(newSync.Destination, newSync.Source);
			if (existing != null)
			{
				existing.SetEnabled(enabled, detectEnabled);
			}
			else
			{
				newSync.SetEnabled(enabled, detectEnabled);
				Add(newSync);
			}
		}

		#endregion

		#region Remove

		public void Remove(BusinessObject destination, BusinessObject source)
		{
			Remove(Find(destination, source));
		}

		public void Remove(BusinessObject source)
		{
			Find(source).ToList().ForEach(Remove);
		}

		new void Remove(BusinessObjectSynchroniser target)
		{
			if (target != null)
			{
				target.SetEnabled(false, target.DetectEnabled);
				target.Dispose();
				base.Remove(target);
			}
		}

		#endregion

		#region Find

		public BusinessObjectSynchroniser Find(BusinessObject destination, BusinessObject source)
		{
			return this.FirstOrDefault(sync => sync.Destination.PK == destination.PK && sync.Source.PK == source.PK);
		}

		public T Find<T>(BusinessObject destination, BusinessObject source)
			where T : BusinessObjectSynchroniser
		{
			var synchroniserType = typeof(T);
			return (T)this.FirstOrDefault(sync => sync.GetType() == synchroniserType && sync.Destination.PK == destination.PK && sync.Source.PK == source.PK);
		}

		public T FindMatchingDestination<T>(BusinessObject destination, bool allowMatchOnInheritiedTypeOrImplementations = false)
			where T : BusinessObjectSynchroniser
		{
			var synchroniserType = typeof(T);
			return (T)this.FirstOrDefault(sync => (sync.GetType() == synchroniserType || (allowMatchOnInheritiedTypeOrImplementations && sync is T)) && sync.Destination.PK == destination.PK);
		}

		public T FindMatchingSource<T>(BusinessObject source, bool allowMatchOnInheritiedTypeOrImplementations = false)
			where T : BusinessObjectSynchroniser
		{
			var synchroniserType = typeof(T);
			return (T)this.FirstOrDefault(sync => (sync.GetType() == synchroniserType || (allowMatchOnInheritiedTypeOrImplementations && sync is T)) && sync.Source.PK == source.PK);
		}

		public IEnumerable<BusinessObjectSynchroniser> Find(BusinessObject source)
		{
			return this.Where(sync => sync.Source.PK == source.PK);
		}

		#endregion
	}
}
