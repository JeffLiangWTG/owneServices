namespace xHub.DatImplementation.ContainerDeployment
{
	public class IntegrationTestEnvironments
	{
		public List<IntegrationTestEnvironment> Environments { get; set; } = new();
	}

	public class IntegrationTestEnvironment
	{
		public string Name { get; set; } = string.Empty;
		public List<Deployment> Deployments { get; set; } = new();
		public ServerInfo ServerInfo { get; set; } = new();
	}

	public class Deployment
	{
		public DeploymentType DeploymentType { get; set; }
		public string FileName { get; set; } = string.Empty;
		public string Name { get; set; } = string.Empty;
	}

	public enum DeploymentType
	{
		Interface,
		Web
	}

}
