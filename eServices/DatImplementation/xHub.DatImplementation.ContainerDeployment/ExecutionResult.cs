
namespace xHub.DatImplementation.ContainerDeployment
{
	public class ExecutionResult
	{
		public bool Success { get; set; }
		public ServerInfo Content { get; set; } = new();
		public string Message { get; set; } = string.Empty;
		public string ErrorCode { get; set; } = string.Empty;
		public int Status { get; set; }
		public string RequestId { get; set; } = string.Empty;
	}
}
