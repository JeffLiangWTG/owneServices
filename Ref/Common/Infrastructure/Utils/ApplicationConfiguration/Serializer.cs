using System.Text.Json;

namespace CargoWise.RefDbRepo.Common.Utils
{
	public static class Serializer
	{
		public static byte[] Serialize<T>(T obj) where T : class
		{
			Argument.Argument.NotNull(obj, nameof(obj));
			return JsonSerializer.SerializeToUtf8Bytes(obj);
		}

		public static T Deserialize<T>(byte[] data) where T : class
		{
			Argument.Argument.NotNull(data, nameof(data));
			return JsonSerializer.Deserialize<T>(data);
		}
	}
}
