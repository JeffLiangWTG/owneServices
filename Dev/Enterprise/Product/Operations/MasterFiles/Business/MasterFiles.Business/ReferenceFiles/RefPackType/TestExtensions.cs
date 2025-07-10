#region Test
#if DEBUG

using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Enterprise.MasterFiles.Business.Testing
{
	public static class TestExtensions
	{
		public static List<T> ToList<T>(this IEnumerable collection)
		{
			return collection.OfType<T>().ToList();
		}
	}
}

#endif
#endregion
