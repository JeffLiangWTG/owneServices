using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Enterprise.MasterFiles.Business.Testing
{
	public static class TypeExtensions
	{
		public static List<string> GetAllPublicConstantValues(this Type type)
		{
			return type
				.GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy)
				.Where(fi => fi.IsLiteral && !fi.IsInitOnly && fi.FieldType == typeof(string))
				.Select(x => (string)x.GetRawConstantValue())
				.ToList();
		}
	}
}
