using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.US.eManifest.Business
{
	static class CusCodeDataExtensions
	{
		internal static ZString DataAsString<T>(this CusCodeDataCollection<T> collection, Func<T, ZString> selector = null) where T : CusCodeData
		{
			if (selector == null)
			{
				selector = c => c.CY_Data;
			}

			return new ZStringBuilder(collection.Cast<T>().Select(selector)).ToStringWithDelimiterBetweenAppends(", ");
		}

		internal static void PopulateDataFromString<T>(this CusCodeDataCollection<T> collection, ZString data, Action<T, ZString> setter = null) where T : CusCodeData
		{
			if (setter == null)
			{
				setter = (c, v) => c.CY_Data = v;
			}

			var values = data.Split(',').Where(v => !v.IsEmpty).ToArray();
			while (values.Length < collection.Count)
			{
				collection[0].Delete();
			}

			for (var i = 0; i < values.Length; i++)
			{
				var value = values[i].Trim().Left(CusCodeData.Schema.CY_DataMaxLength);
				setter(i < collection.Count ? collection[i] : collection.AddNew(), value);
			}
		}
	}
}
