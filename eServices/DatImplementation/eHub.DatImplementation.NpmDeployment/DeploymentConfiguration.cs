namespace eHub.DatImplementation.NpmDeployment
{
    public class DeploymentConfiguration
    {
        public string Directory { get; }
        public string Command { get; }

        public DeploymentConfiguration(string deploymentConfiguration)
        {
            var parts = deploymentConfiguration.Split(';');

            foreach (var part in parts)
            {
                var keyValue = part.Split('=');

                if (keyValue.Length == 2)
                {
                    var key = keyValue[0];
                    var value = keyValue[1];

                    switch (key)
                    {
                        case "Dir":
                            Directory = value;
                            break;
                        case "Command":
                            Command = value;
                            break;
                    }
                }
            }
        }
    }
}
