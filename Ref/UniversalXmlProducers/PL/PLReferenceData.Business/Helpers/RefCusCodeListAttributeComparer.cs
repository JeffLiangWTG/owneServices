using System.Collections.Generic;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;

namespace CargoWise.RefDbRepo.PLReferenceData.Business.Helpers
{
	public class RefCusCodeListAttributeComparer : IEqualityComparer<RefCusCodeListAttribute>
	{
		public bool Equals(RefCusCodeListAttribute x, RefCusCodeListAttribute y)
		{
			if (x == null || y == null)
			{
				return false;
			}

			return x.ZZE_Value == y.ZZE_Value && x.ZZE_ZXE_NKName == y.ZZE_ZXE_NKName;
		}

		public int GetHashCode(RefCusCodeListAttribute obj)
		{
			return obj.ZZE_Value.GetHashCode();
		}
	}
}
