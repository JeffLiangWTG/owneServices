using System;
using System.Collections.Generic;

namespace Enterprise.MasterFiles.Business
{
	public static class ArrayExtension
	{
		public static T[] Add<T>(this T[] array, T newRecord, IEqualityComparer<T> comparer)
		{
			var index = Array.FindIndex(array, element => comparer.Equals(element, newRecord));

			if (index == -1)
			{
				return new List<T>(array) { newRecord }.ToArray();
			}

			array[index] = newRecord;
			return array;
		}
	}
}
