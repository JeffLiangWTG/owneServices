using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using CargoWise.RefDbRepo.Service.Schema_0_9_New;

namespace CargoWise.RefDbRepo.NewService.Test
{
	public class ObjectReferenceDataRepository : IReferenceDataRepository, IReadOnlyReferenceDataRepository
	{
		public ObjectReferenceDataRepository()
		{
			this.storage = new Dictionary<Type, List<object>>();
		}
		readonly Dictionary<Type, List<object>> storage;

		public bool IsDbProvider
		{
			get { return false; }
		}

		public IQueryable<T> Get<T>() where T : class
		{
			return storage.ContainsKey(typeof(T)) ? storage[typeof(T)].Cast<T>().AsQueryable() : Enumerable.Empty<T>().AsQueryable();
		}

		public T Create<T>(Expression<Func<T>> initializer = null) where T : new()
		{
			var result = initializer == null ? new T() : initializer.Compile().Invoke();
			List<object> objs = null;
			if (!storage.TryGetValue(typeof(T), out objs))
			{
				objs = new List<object>();
				storage.Add(typeof(T), objs);
			}
			objs.Add(result);
			return result;
		}

		public void Dispose()
		{
		}

		public void Add<T>(T data) where T : class
		{
			throw new NotImplementedException();
		}

		public void Update<T>(T data) where T : class
		{
			throw new NotImplementedException();
		}

		public void UpdateRefDbVersionControl(RefDbVersionControl refDbVersionControl)
		{
			var obj = storage[typeof(RefDbVersionControl)].Cast<RefDbVersionControl>().FirstOrDefault(o => o.RVC_ParentCode == refDbVersionControl.RVC_ParentCode && o.RVC_ParentPK == refDbVersionControl.RVC_ParentPK);
			if (obj != null)
			{
				obj.RVC_LastUpdatedUTC = refDbVersionControl.RVC_LastUpdatedUTC;
				obj.RVC_Deleted = refDbVersionControl.RVC_Deleted;
				obj.RVC_CreatedTimeUTC = refDbVersionControl.RVC_CreatedTimeUTC;
			}
		}

		public Task<int> SaveChangesAsync()
		{
			throw new NotImplementedException();
		}
	}
}
