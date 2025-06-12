using System.Net.Http;
using System.Text;
using Newtonsoft.Json;
using Serilog;



namespace xHub.DatImplementation.ContainerDeployment
{
	public class DeploymentServiceAPICall
	{
		private readonly ILogger logger;

		public DeploymentServiceAPICall(ILogger logger)
		{
			this.logger = logger;
		}
		public async Task<Tuple<bool, ServerInfo>> SendDeploymentRequestAsync(DeploymentRequest deploymentRequest, Server selectedServer)
		{
			try
			{
				using var _httpClient = new HttpClient();
				_httpClient.Timeout = TimeSpan.FromMinutes(25);
				var apiUrl = selectedServer.WindowsServiceApiUrl;
				var jsonString = JsonConvert.SerializeObject(deploymentRequest);
				var content = new StringContent(jsonString, Encoding.UTF8, "application/json");
				var response = await _httpClient.PostAsync(apiUrl, content);
				if (!response.IsSuccessStatusCode)
				{
					logger.Information($"Failed to call deployment API. Status: {response.StatusCode}, Reason: {response.ReasonPhrase}");
					return new Tuple<bool, ServerInfo>(false, new());
				}
				var responseContent = await response.Content.ReadAsStringAsync();
				if (string.IsNullOrEmpty(responseContent))
				{
					logger.Information("Deserialization of API response returned null.");
					return new Tuple<bool, ServerInfo>(false, new());
				}
				var executionResult = !string.IsNullOrEmpty(responseContent) ? JsonConvert.DeserializeObject<ExecutionResult>(responseContent) : new();
				if (response.IsSuccessStatusCode && executionResult != null && executionResult.Success)
				{
					executionResult.Content.ServerAddress = selectedServer.Name;
					return new Tuple<bool, ServerInfo>(true, executionResult.Content);
				}
				else
				{
					return new Tuple<bool, ServerInfo>(false, new());
				}
			}
			catch
			{
				logger.Information("An error occurred while sending the deployment request.");
				return new Tuple<bool, ServerInfo>(false, new());
			}
		}
	}
}
