using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;

namespace CargoWise.eHub.DataModel.Common
{
	public class TestDbSet<T> : DbSet<T>, IQueryable, IEnumerable<T>
		where T : class
	{
		ObservableCollection<T> _data;
		IQueryable _query;

		public TestDbSet()
		{
			_data = new ObservableCollection<T>();
			_query = _data.AsQueryable();
		}

		public override T Add(T item)
		{
			_data.Add(item);
			return item;
		}

		public override IEnumerable<T> AddRange(IEnumerable<T> entities)
		{
			foreach (var item in entities)
				_data.Add(item);
			return entities;
		}

		public override T Remove(T item)
		{
			_data.Remove(item);
			return item;
		}

		public override IEnumerable<T> RemoveRange(IEnumerable<T> entities)
		{
			foreach (var item in entities)
			{
				Remove(item);
			}
			return entities;
		}

		public override T Attach(T item)
		{
			_data.Add(item);
			return item;
		}

		public override T Create()
		{
			return Activator.CreateInstance<T>();
		}

		public override TDerivedEntity Create<TDerivedEntity>()
		{
			return Activator.CreateInstance<TDerivedEntity>();
		}

		public override ObservableCollection<T> Local
		{
			get { return new ObservableCollection<T>(_data); }
		}

		Type IQueryable.ElementType
		{
			get { return _query.ElementType; }
		}

		Expression IQueryable.Expression
		{
			get { return _query.Expression; }
		}

		IQueryProvider IQueryable.Provider
		{
			get { return _query.Provider; }
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return _data.GetEnumerator();
		}

		IEnumerator<T> IEnumerable<T>.GetEnumerator()
		{
			return _data.GetEnumerator();
		}
	}
}