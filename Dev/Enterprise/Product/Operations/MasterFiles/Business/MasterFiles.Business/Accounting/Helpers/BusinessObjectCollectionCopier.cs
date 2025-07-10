using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Schema;

namespace Enterprise.MasterFiles.Business.Accounting.Helpers
{
	public class BusinessObjectCollectionCopier
	{
		readonly SchemaColumn[] keyColumns;
		readonly SchemaColumn[] allColumns;

		public BusinessObjectCollectionCopier(SchemaColumn[] keyFields, SchemaColumn[] valueFields)
		{
			this.keyColumns = keyFields;
			this.allColumns = keyFields.Union(valueFields).ToArray();
		}

		public IComparable[][] ConvertToNestedList(IBusinessObjectCollection collection)
		{
			var comparer = new ArrayComparer();
			var collectionSorted =
				(
					from item
					in collection.Cast<BusinessObject>().OrderBy(item => (from c in keyColumns select (IComparable)item[c]).ToArray(), comparer)
					where (!item.IsDeleted)
					select BusinessObjectToArray(item)
				)
				.ToArray();

			return collectionSorted;
		}

		public static bool AreEqual(IComparable[][] first, IComparable[][] second)
		{
			if (first.Length != second.Length)
			{
				return false;
			}

			for (int i = 0; i < first.Length; i++)
			{
				if (!first[i].SequenceEqual(second[i]))
				{
					return false;
				}
			}

			return true;
		}

		/// <summary>
		/// Copy Nested List Representation to a Business Object Collection
		/// </summary>
		public void CopyTo(IComparable[][] source, IBusinessObjectCollection destination)
		{
			using (destination.SuspendListChanged())
			{
				foreach (BusinessObject item in destination.ToArray())
				{
					item.Delete();
				}

				foreach (var sourceItem in source)
				{
					var newobject = destination.AddNew();

					using (newobject.GetValidationSuspender())
					{
						for (int i = 0; i < allColumns.Length; i++)
						{
							((IBusinessObjectInternals)newobject).Row[allColumns[i].Name] = (object)sourceItem[i] ?? DBNull.Value;
						}
					}
				}
			}
		}

		#region Helper Methods and Classes

		IComparable[] BusinessObjectToArray(BusinessObject bo)
		{
			return
				(
					from c
					in allColumns
					select GetBusinessObjectField(bo, c)
				).ToArray();
		}

		IComparable GetBusinessObjectField(BusinessObject bo, SchemaColumn column)
		{
			var boInternals = ((IBusinessObjectInternals)bo);
			boInternals.EnsureBlobField(column);

			if (boInternals.Row[column.Name] == DBNull.Value)
			{
				return null;
			}
			else
			{
				return (IComparable)boInternals.Row[column.Name];
			}
		}

		public class ArrayComparer : IComparer<IComparable[]>
		{
			public int Compare(IComparable[] x, IComparable[] y)
			{
				for (int i = 0; i < x.Length; i++)
				{
					if (i >= y.Length)
					{
						return 1;
					}

					var compareElement = x[i].CompareTo(y[i]);

					if (compareElement != 0)
					{
						return compareElement;
					}
				}

				if (x.Length < y.Length)
				{
					return -1;
				}

				return 0;
			}
		}

		#endregion
	}
}
