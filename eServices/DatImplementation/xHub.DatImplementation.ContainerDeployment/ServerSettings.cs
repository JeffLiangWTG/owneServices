using Newtonsoft.Json;
using WTG.DeploymentUtils.SecureStore;


namespace xHub.DatImplementation.ContainerDeployment
{
	public class ConnectionInfo
	{
		public string UserName { get; set; } = string.Empty;
		public string EncryptedPassword { get; set; } = string.Empty;

		[JsonIgnore]
		public string DecriptPassword => PasswordDecriptor();

		string PasswordDecriptor()
		{
			var secureStore = new SecureStore();
			return secureStore.Decrypt(EncryptedPassword);
		}
	}

	public class Server
	{
		public string Name { get; set; } = string.Empty;
		public ConnectionInfo ConnectionInfo { get; set; } = new();
		public string RemotePath { get; set; } = string.Empty;
		public string WindowsServiceApiUrl { get; set; } = string.Empty;
	}

	public class ServerSettings
	{
		public List<Server> Servers { get; set; } = new();
	}
}
