namespace xHub.DatImplementation.ContainerDeployment
{
	public class DeploymentRequest
	{
		public string ContainerName { get; set; }
		public List<InterfaceParameter> InterfaceParameters { get; set; } = new();
		public List<WebsiteParameter> WebsiteParameters { get; set; } = new();
        public Dictionary<string, int> DynamicPorts { get; set; } = new();

        public DeploymentRequest(IEnumerable<Deployment> deployments,Dictionary<string,int> dynamicports ,string containername)
		{
			ContainerName = containername;
			foreach (var deployment in deployments)
			{
				if (deployment.DeploymentType == DeploymentType.Interface)
				{
					InterfaceParameters.Add(new()
					{
						InterfaceFileName = deployment.FileName,
						InterfaceName = deployment.Name
					});
				}
				else
				{
					WebsiteParameters.Add(new()
					{
						WebSiteFileName = deployment.FileName,
						WebSiteName = deployment.Name
					});
				}
			}

            DynamicPorts = dynamicports;
        }
	}

	public struct InterfaceParameter
	{
		public string InterfaceFileName { get; set; }
		public string InterfaceName { get; set; }
	}

	public struct WebsiteParameter
	{
		public string WebSiteFileName { get; set; }
		public string WebSiteName { get; set; }
	}
}


