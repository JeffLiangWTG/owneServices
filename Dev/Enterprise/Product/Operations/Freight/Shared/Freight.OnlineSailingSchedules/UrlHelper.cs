using System.Net;
using CargoWise.Common;
using CargoWise.Types;

namespace Enterprise.Freight.OnlineSailingSchedules
{
	public static class UrlHelper
	{
		public static string ConvertToParams<T>(T entity)
			where T : class
		{
			Argument.NotNull(entity, "entity");
			var result = new ZStringBuilder();
			var properties = typeof(T).GetProperties();

			foreach (var propertyInfo in properties)
			{
				var propertyValue = (string)propertyInfo.GetValue(entity);

				if (!string.IsNullOrEmpty(propertyValue))
				{
					result.AppendFormat("{0}={1}&", WebUtility.UrlEncode(propertyInfo.Name), WebUtility.UrlEncode(propertyValue));
				}
			}

			return result.ToString().TrimEnd('&');
		}
	}
}
