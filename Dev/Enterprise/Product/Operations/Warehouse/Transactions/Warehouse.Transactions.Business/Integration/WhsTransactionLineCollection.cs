using System;
using System.Collections;
using Enterprise.Warehouse.Integration;

namespace Enterprise.Warehouse.Transactions.Business
{
	public class WhsWarehouseTransactionLineCollection : IWhsWarehouseTransactionLineCollection
	{
		public WhsWarehouseTransactionLineCollection()
		{
		}

		public static WhsWarehouseTransactionLineCollection GetNew(params IWhsWarehouseTransactionLine[] lines)
		{
			var result = new WhsWarehouseTransactionLineCollection();
			foreach (var line in lines)
			{
				result.Add(line);
			}
			return result;
		}

		public IWhsWarehouseTransactionLine this[int i]
		{
			get
			{
				return (IWhsWarehouseTransactionLine)Lines[i];
			}
			set
			{
				CheckType(value);
				Lines[i] = value;
			}
		}

		public void Add(IWhsWarehouseTransactionLine line)
		{
			CheckType(line);
			Lines.Add(line);
		}

		public bool Contains(WhsWarehouseTransactionLine line)
		{
			return Lines.Contains(line);
		}

		public void Sort(IComparer comparer)
		{
			Lines.Sort(comparer);
		}

		public WhsWarehouseTransactionLineCollection Clone()
		{
			return CloneCore();
		}

		protected virtual WhsWarehouseTransactionLineCollection CloneCore()
		{
			return WhsWarehouseTransactionLineCollection.GetNew((WhsWarehouseTransactionLine[])Lines.ToArray(typeof(WhsWarehouseTransactionLine)));
		}

		public int Count
		{
			get { return Lines.Count; }
		}

		protected virtual void CheckType(object element)
		{
			if (!(element is IWhsWarehouseTransactionLine))
			{
				throw new NotSupportedException("This collection requires IWarehouseTransactionLine elements.");
			}
		}

		protected ArrayList Lines = new ArrayList();

		public IEnumerator GetEnumerator()
		{
			return Lines.GetEnumerator();
		}

		#region IWhsWarehouseTransactionLineCollection Members

		void IWhsWarehouseTransactionLineCollection.Add(IWhsWarehouseTransactionLine line)
		{
			Add(line);
		}

		IWhsWarehouseTransactionLineCollection IWhsWarehouseTransactionLineCollection.Clone()
		{
			return Clone();
		}

		int IWhsWarehouseTransactionLineCollection.Count
		{
			get { return Count; }
		}

		IWhsWarehouseTransactionLine IWhsWarehouseTransactionLineCollection.this[int i]
		{
			get { return this[i]; }
			set { this[i] = value; }
		}

		#region IEnumerable Members

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		#endregion

		#endregion
	}
}
