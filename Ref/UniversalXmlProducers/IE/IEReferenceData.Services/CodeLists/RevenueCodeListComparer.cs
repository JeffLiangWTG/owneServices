using System;
using System.Collections.Generic;

namespace CargoWise.RefDbRepo.IEReferenceData.CodeLists.Services
{
	class RevenueCodeListComparer : IEqualityComparer<IRevenueCodeDescriptionPair>
	{
		public bool Equals(IRevenueCodeDescriptionPair x, IRevenueCodeDescriptionPair y)
		{
			if (Object.ReferenceEquals(x, y))
			{
				return true;
			}
			if (x is null || y is null)
			{
				return false;
			}
			return x.Code == y.Code; // only compare code as there can be some slight variation in descriptions
		}

		public int GetHashCode(IRevenueCodeDescriptionPair x)
		{
			if (x is null)
			{
				return 0;
			}
			return x.Code.GetHashCode();
		}
	}
}
