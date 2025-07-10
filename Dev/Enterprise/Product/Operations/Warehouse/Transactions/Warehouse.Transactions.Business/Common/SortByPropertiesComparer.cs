using System.Collections.Generic;
using CargoWise.Types;

namespace Enterprise.Warehouse.Transactions.Business
{
	#region SortByPropertyComparer class

	public abstract class SortByPropertiesComparer<TObject> : IComparer<TObject>
	{
		IEnumerable<IComparer<TObject>> Comparers
		{
			get { return comparers ?? (comparers = GetElementaryComparers()); }
		}

		IEnumerable<IComparer<TObject>> comparers;

		public int Compare(TObject x, TObject y)
		{
			return CompareCore(x, y);
		}

		protected abstract IEnumerable<IComparer<TObject>> GetElementaryComparers();

		int CompareCore(TObject o1, TObject o2)
		{
			int result = Equal;
			foreach (var comparer in Comparers)
			{
				result = comparer.Compare(o1, o2);
				if (result != Equal)
				{
					break;
				}
			}
			return result;
		}

		protected delegate TProperty GetProperty<TProperty>(TObject line);

		#region ValueComparer class

		protected class ValueComparer : IComparer<TObject>
		{
			readonly GetProperty<IZType> method;

			public ValueComparer(GetProperty<IZType> method)
			{
				this.method = method;
			}

			public int Compare(TObject x, TObject y)
			{
				var property1 = GetCachedPropertyValue(x);
				var firstIsEmpty = property1.IsEmpty;

				var property2 = GetCachedPropertyValue(y);
				var secondIsEmpty = property2.IsEmpty;

				return property1.IsEmpty || secondIsEmpty ? firstIsEmpty.CompareTo(secondIsEmpty) : property1.CompareTo(property2);
			}

			IZType GetCachedPropertyValue(TObject objectWithProperty)
			{
				IZType result;
				if (!PropertyValueCache.TryGetValue(objectWithProperty, out result))
				{
					PropertyValueCache[objectWithProperty] = result = method(objectWithProperty);
				}

				return result;
			}

			readonly Dictionary<TObject, IZType> PropertyValueCache = new Dictionary<TObject, IZType>();
		}

		#endregion

		public override int GetHashCode()
		{
			return this.GetType().GetHashCode();
		}

		public override bool Equals(object obj)
		{
			return obj != null && obj.GetType() == this.GetType();
		}

		protected const int Equal = 0;
		protected const int LeftIsLess = -1;
		protected const int RightIsLess = 1;
	}

	#endregion
}
