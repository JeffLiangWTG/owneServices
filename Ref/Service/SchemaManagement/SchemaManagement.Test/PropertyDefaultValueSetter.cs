using System.Reflection;

namespace CargoWise.RefDbRepo.Service.SchemaManagement.Test
{
	static class PropertyDefaultValueSetter
	{
		public static void SetDefaultValues<T>(this T value)
		{
			foreach (var prop in typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance))
			{
				if (prop.CanWrite && prop.PropertyType == typeof(string))
				{
					prop.SetValue(value, string.Empty);
				}
			}
		}
	}
}
