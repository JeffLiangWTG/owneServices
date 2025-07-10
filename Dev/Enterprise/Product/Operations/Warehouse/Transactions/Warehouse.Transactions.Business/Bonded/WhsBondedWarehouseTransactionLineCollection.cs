using System;
using System.Collections;
using Enterprise.Warehouse.Integration.BondedWarehouse;

namespace Enterprise.Warehouse.Transactions.Business.Bonded
{
	public class WhsBondedWarehouseTransactionLineCollection : WhsWarehouseTransactionLineCollection, IWhsBondedWarehouseTransactionLineCollection
	{
		public WhsBondedWarehouseTransactionLineCollection()
		{
		}

		public static WhsBondedWarehouseTransactionLineCollection GetNew(params IWhsBondedWarehouseTransactionLine[] lines)
		{
			var result = new WhsBondedWarehouseTransactionLineCollection();
			result.AddRange(lines);
			return result;
		}

		public void AddRange(IWhsBondedWarehouseTransactionLine[] lines)
		{
			foreach (var line in lines)
			{
				Add(line);
			}
		}

		public new IWhsBondedWarehouseTransactionLine this[int i]
		{
			get { return (IWhsBondedWarehouseTransactionLine)base[i]; }
			set { base[i] = value; }
		}

		public void Add(IWhsBondedWarehouseTransactionLine line)
		{
			base.Add(line);
		}

		public new WhsBondedWarehouseTransactionLineCollection Clone()
		{
			return (WhsBondedWarehouseTransactionLineCollection)CloneCore();
		}

		protected override WhsWarehouseTransactionLineCollection CloneCore()
		{
			return GetNew((IWhsBondedWarehouseTransactionLine[])Lines.ToArray(typeof(IWhsBondedWarehouseTransactionLine)));
		}

		protected override void CheckType(object element)
		{
			if (!(element is IWhsBondedWarehouseTransactionLine))
			{
				throw new NotSupportedException("This collection requires IWhsBondedWarehouseTransactionLine elements.");
			}
		}

		#region IWhsBondedWarehouseTransactionLineCollection Members

		IWhsBondedWarehouseTransactionLineCollection IWhsBondedWarehouseTransactionLineCollection.Clone()
		{
			return Clone();
		}

		void IWhsBondedWarehouseTransactionLineCollection.Add(IWhsBondedWarehouseTransactionLine line)
		{
			Add(line);
		}

		IWhsBondedWarehouseTransactionLine IWhsBondedWarehouseTransactionLineCollection.this[int i]
		{
			get { return this[i]; }
			set { this[i] = (WhsBondedWarehouseTransactionLine)value; }
		}

		#region IEnumerable Members

		IEnumerator IEnumerable.GetEnumerator()
		{
			return Lines.GetEnumerator();
		}

		#endregion

		#endregion
	}
}
