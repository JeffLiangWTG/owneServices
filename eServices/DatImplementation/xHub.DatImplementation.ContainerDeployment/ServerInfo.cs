
namespace xHub.DatImplementation.ContainerDeployment
{
	public class ServerInfo
	{
		public string ServerAddress { get; set; } = string.Empty;
		public string ContainerId { get; set; } = string.Empty;
		public string ContainerName { get; set; } = string.Empty;
		public string ContainerIP { get; set; } = string.Empty;
		public int DatabasePort { get; set; }
		public int GRPCPort { get; set; }
		public int RestApiPort { get; set; }
		public int GateWayPort { get; set; }
        public Dictionary<string, int> DynamicPorts { get; set; } = new();

    }
}
