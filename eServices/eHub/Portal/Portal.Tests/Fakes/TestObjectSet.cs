using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Entity.Core.Objects;
using System.Linq;
using System.Linq.Expressions;

namespace CargoWise.eHub.Portal.Tests.Fakes
{
	class TestObjectSet<T> : IObjectSet<T> where T : class
	{
		public TestObjectSet()
			: this(Enumerable.Empty<T>())
		{
		}
		public TestObjectSet(IEnumerable<T> entities)
		{
			_set = new HashSet<T>();
			foreach (var entity in entities)
			{
				_set.Add(entity);
			}
			_queryableSet = _set.AsQueryable();
		}
		public void AddObject(T entity)
		{
			_set.Add(entity);
		}
		public void Attach(T entity)
		{
			_set.Add(entity);
		}
		public void DeleteObject(T entity)
		{
			_set.Remove(entity);
		}
		public void Detach(T entity)
		{
			_set.Remove(entity);
		}
		public Type ElementType
		{
			get { return _queryableSet.ElementType; }
		}
		public Expression Expression
		{
			get { return _queryableSet.Expression; }
		}
		public IQueryProvider Provider
		{
			get { return _queryableSet.Provider; }
		}
		public IEnumerator<T> GetEnumerator()
		{
			return _set.GetEnumerator();
		}
		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}
		public void Clear()
		{
			_set.Clear();
		}

		readonly HashSet<T> _set;
		readonly IQueryable<T> _queryableSet;
	}
}
