using System;
using CargoWise.Types;

namespace Enterprise.Warehouse.Environment.Business
{
	public static class WhsLocationHelper
	{
		public static int GetMaxLocationComponentValueWithFixedWidth(ZByte fixedWidth)
		{
			return (int)(Math.Pow(10, fixedWidth) - 1);
		}

		public static bool LocationStringCompare(WhsLocation location, string locationString)
		{
			var result = false;
			if (location != null)
			{
				result = location.WLV_LocationString.EqualsIgnoringCase(locationString)
					|| location.WLV_LocationString_UserFriendly.EqualsIgnoringCase(locationString);
			}

			return result;
		}
	}
}
