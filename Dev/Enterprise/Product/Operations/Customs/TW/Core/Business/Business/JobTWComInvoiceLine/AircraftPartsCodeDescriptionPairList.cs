using System;
using System.Collections.Generic;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.TW.Business
{
	public class AircraftPartsCodeDescriptionPairList : CodeDescriptionPairList
	{
		public AircraftPartsCodeDescriptionPairList()
		{
		}

		public new void Sort()
		{
			Elements.Sort(new AircraftPartsCodeComparer());
		}
	}

	class AircraftPartsCodeComparer : IComparer<ICodeDescription>
	{
		public int Compare(ICodeDescription x, ICodeDescription y)
		{
			if (ZInt.TryParse(x.Code, out ZInt xCode) && ZInt.TryParse(y.Code, out ZInt yCode))
			{
				return xCode.CompareTo(yCode);
			}
			return string.Compare(x.Code, y.Code, StringComparison.OrdinalIgnoreCase);
		}
	}
}
