using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.US.Business
{
	static class CusCodeDataExtensionMethods
	{
		public static List<T> GetElementsInOrder<T>(this Customs.Business.CusCodeDataCollection<T> cusCodeDataCollection) where T : Customs.Business.CusCodeData
		{
			List<T> list = new List<T>(new TypedEnumerable<T>(cusCodeDataCollection));
			list.Sort(new System.Comparison<T>((T code1, T code2) =>
			{
				int result = code1.CY_Code.CompareTo(code2.CY_Code);

				if (result == 0)
				{
					result = code1.CY_Data.CompareTo(code2.CY_Data);
				}

				return result;
			}));

			return list;
		}

		public static ZString GetStringRepresentationOfElementsInOrder<T>(this Customs.Business.CusCodeDataCollection<T> cusCodeDataCollection) where T : Customs.Business.CusCodeData
		{
			List<T> list = GetElementsInOrder(cusCodeDataCollection);

			ZStringBuilder finalResult = new ZStringBuilder();

			foreach (T element in list)
			{
				finalResult.Append(element.CY_Code + ":" + element.CY_Data);
			}

			return finalResult.ToString();
		}
	}
}
