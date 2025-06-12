using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CargoWise.eHub.Portal.Helpers
{
	public static class TypeConverterHelper
	{
		public static int? ToNullableInt32(this string s)
		{
			int i;
			if (Int32.TryParse(s, out i)) return i;
			return null;
		}

		public static Guid? ToNullableGuid(this string s)
		{
			Guid i;
			if (Guid.TryParse(s, out i)) return i;
			return null;
		}
	}
}