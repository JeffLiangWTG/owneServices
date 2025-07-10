using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using Serilog;
using Yarp.ReverseProxy.Configuration;
using Yarp.ReverseProxy.Health;
using Yarp.ReverseProxy.Model;

namespace CargoWise.Blazor.SessionBroker
{
	public class ValidateProcessIdHealthCheckPolicy : IActiveHealthCheckPolicy
	{
		readonly IDestinationHealthUpdater destinationHealthUpdater;
		readonly IProxyConfigProvider proxyConfigProvider;
		readonly ILogger logger;

		public ValidateProcessIdHealthCheckPolicy(IDestinationHealthUpdater destinationHealthUpdater, IProxyConfigProvider proxyConfigProvider, ILogger logger)
		{
			this.destinationHealthUpdater = destinationHealthUpdater;
			this.proxyConfigProvider = proxyConfigProvider;
			this.logger = logger;
		}

		public string Name => "ValidateProcessId";

		public void ProbingCompleted(ClusterState cluster, IReadOnlyList<DestinationProbingResult> probingResults)
		{
			if (probingResults.Count == 0)
			{
				return;
			}

			var destinations = proxyConfigProvider.GetConfig().Clusters.Single().Destinations;

			var updatedHealthStates = new NewActiveDestinationHealth[probingResults.Count];
			for (var i = 0; i < probingResults.Count; i++)
			{
				var destinationConfig = destinations[probingResults[i].Destination.DestinationId];
				var expectedProcessId = int.Parse(destinationConfig.Metadata["ProcessId"], CultureInfo.InvariantCulture);
				var health = EvaluateHealth(probingResults[i].Response, expectedProcessId);
				updatedHealthStates[i] = new NewActiveDestinationHealth(probingResults[i].Destination, health);
			}
			destinationHealthUpdater.SetActive(cluster, updatedHealthStates);
		}

		DestinationHealth EvaluateHealth(HttpResponseMessage response, int expectedProcessId)
		{
			var result = DestinationHealth.Unhealthy;
			try
			{
				if (response != null && response.IsSuccessStatusCode)
				{
#pragma warning disable VSTHRD002 // Avoid problematic synchronous waits (review: it this safe? (to read the response, and to synchronously block whilst reading a response)))
					var json = JsonDocument.Parse(response.Content.ReadAsStringAsync().GetAwaiter().GetResult()).RootElement;
#pragma warning restore VSTHRD002 // Avoid problematic synchronous waits
					var health = json.GetProperty("status").GetString();
					var processId = json.GetProperty("data").GetProperty("ProcessId").GetInt32();
					if (health == "Healthy" && processId == expectedProcessId)
					{
						result = DestinationHealth.Healthy;
					}
				}
			}
			catch (Exception ex)
			{
				logger.Error(ex, ex.Message);
			}

			return result;
		}
	}
}
