using System.Collections;
using System.Collections.Generic;

namespace Enterprise.Rating.Business
{
	/// <summary>
	/// Two rates are considered "equal" if they should be grouped togeather on the same
	/// pricing page.
	/// </summary>
	internal abstract class PageEqualityComparer : IEqualityComparer<RateEntry>, IEqualityComparer
	{
		public abstract bool Equals(RateEntry x, RateEntry y);
		public abstract int GetHashCode(RateEntry obj);

		#region IEqualityComparer Members

		bool IEqualityComparer.Equals(object x, object y)
		{
			return Equals((RateEntry)x, (RateEntry)y);
		}

		int IEqualityComparer.GetHashCode(object obj)
		{
			return GetHashCode((RateEntry)obj);
		}

		#endregion
	}
}

