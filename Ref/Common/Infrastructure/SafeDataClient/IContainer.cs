using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.OData.Client;

namespace CargoWise.RefDbRepo.Common.SafeDataClient
{
	public interface IContainer
	{
		void AddObject(string entitySetName, object entity);
		IQueryable<T> CreateQuery<T>(string entitySetName);
		IQueryable<T> CreateQueryWithNoTracking<T>(string entitySetName);
		Task<bool> SaveChangesAsync(SaveChangesOptions options);
		void UpdateObject(object entity);
		void DeleteObject(object entity);
		IEnumerable<Tuple<object, EntityStates>> EntityStates { get; }
		void ChangeState(object entity, EntityStates state);
	}
}
