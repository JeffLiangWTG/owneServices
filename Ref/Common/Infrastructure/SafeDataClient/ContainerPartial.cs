using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.OData.Client;

namespace CargoWise.RefDbRepo.Common.SafeDataClient.Default
{
	[SuppressMessage("Microsoft.Maintainability", "CA1506")]
	public partial class Container : IContainer
	{
		public IEnumerable<Tuple<object, EntityStates>> EntityStates
		{
			get { return Entities.Select(x => Tuple.Create(x.Entity, x.State)); }
		}

		public void DeleteOjbect<T>(T data)
		{
			DeleteObject(data);
		}

		IQueryable<T> IContainer.CreateQuery<T>(string entitySetName)
		{
			MergeOption = MergeOption.AppendOnly;
			return CreateQuery<T>(entitySetName);
		}

		public IQueryable<T> CreateQueryWithNoTracking<T>(string entitySetName)
		{
			MergeOption = MergeOption.NoTracking;
			return CreateQuery<T>(entitySetName);
		}

		async Task<bool> IContainer.SaveChangesAsync(SaveChangesOptions options)
		{
			return (await SaveChangesAsync(options)).BatchStatusCode == (int)HttpStatusCode.OK;
		}
	}
}
