using System.Text.Json.Serialization;

namespace CargoWise.RefDbRepo.CarrierMessagingBuss.Shared.Model
{
	public class ResponseResult<T> where T : class
	{
		[JsonPropertyName("value")]
		public T Value { get; set; }

		[JsonPropertyName("isSuccess")]
		public bool IsSuccess { get; set; }

		[JsonPropertyName("message")]
		public string Message { get; set; } = string.Empty;
	}
}
