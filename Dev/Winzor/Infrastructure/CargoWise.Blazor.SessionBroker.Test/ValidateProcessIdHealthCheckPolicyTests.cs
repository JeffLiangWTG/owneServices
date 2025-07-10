using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using Moq;
using NUnit.Framework;
using Yarp.ReverseProxy.Configuration;
using Yarp.ReverseProxy.Health;
using Yarp.ReverseProxy.Model;

namespace CargoWise.Blazor.SessionBroker.Test
{
	public class ValidateProcessIdHealthCheckPolicyTests
	{
		[Test]
		public void PolicyHasCorrectName()
		{
			var policy = new ValidateProcessIdHealthCheckPolicy(null, null, null);
			Assert.That(policy.Name, Is.EqualTo("ValidateProcessId"));
		}

		[Test]
		public void EmptyProbingResultList_DoesNothing()
		{
			var policy = new ValidateProcessIdHealthCheckPolicy(new DestinationHealthUpdaterStub(), GetClusterConfig(), null);
			var clusterInfo = GetClusterInfo("cluster1", 1);

			// initially, health should be unknown
			foreach (var destination in clusterInfo.Destinations.Values)
			{
				Assert.That(destination.Health.Active, Is.EqualTo(DestinationHealth.Unknown), $"Unexpected health for destination {destination.DestinationId}");
			}

			policy.ProbingCompleted(clusterInfo, Array.Empty<DestinationProbingResult>());
			Assert.That(clusterInfo.Destinations.Single().Value.Health.Active, Is.EqualTo(DestinationHealth.Unknown));

			// Passive health check status should not be affected
			foreach (var destination in clusterInfo.Destinations.Values)
			{
				Assert.That(destination.Health.Passive, Is.EqualTo(DestinationHealth.Unknown), $"Unexpected passive health for destination {destination.DestinationId}");
			}
		}

		// NB All remaining tests have a cluster with 2 destinations.  The second destination should always be healthy - this validates
		// that the policy is evaluating each destination separately

		[Test]
		public void SuccessfulResponseWithCorrectProcessId_MarksDestinationHealthy()
		{
			var policy = new ValidateProcessIdHealthCheckPolicy(new DestinationHealthUpdaterStub(), GetClusterConfig(), null);
			var clusterInfo = GetClusterInfo("cluster1", 2);
			using var response1 = GetResponse(HttpStatusCode.OK, 11111);
			using var response2 = GetResponse(HttpStatusCode.OK, 22222);
			var probingResults = new[]
			{
				new DestinationProbingResult(clusterInfo.Destinations["destination0"], response1, null),
				new DestinationProbingResult(clusterInfo.Destinations["destination1"], response2, null),
			};

			// initially, health should be unknown
			foreach (var destination in clusterInfo.Destinations.Values)
			{
				Assert.That(destination.Health.Active, Is.EqualTo(DestinationHealth.Unknown), $"Unexpected health for destination {destination.DestinationId}");
			}

			policy.ProbingCompleted(clusterInfo, probingResults);
			Assert.That(clusterInfo.Destinations["destination0"].Health.Active, Is.EqualTo(DestinationHealth.Healthy));
			Assert.That(clusterInfo.Destinations["destination1"].Health.Active, Is.EqualTo(DestinationHealth.Healthy));

			// Passive health check status should not be affected
			foreach (var destination in clusterInfo.Destinations.Values)
			{
				Assert.That(destination.Health.Passive, Is.EqualTo(DestinationHealth.Unknown), $"Unexpected passive health for destination {destination.DestinationId}");
			}
		}

		[Test]
		public void SuccessfulResponseWithIncorrectProcessId_MarksDestinationUnhealthy()
		{
			var policy = new ValidateProcessIdHealthCheckPolicy(new DestinationHealthUpdaterStub(), GetClusterConfig(), null);
			var clusterInfo = GetClusterInfo("cluster1", 2);
			using var response1 = GetResponse(HttpStatusCode.OK, 12345);
			using var response2 = GetResponse(HttpStatusCode.OK, 22222);
			var probingResults = new[]
			{
				new DestinationProbingResult(clusterInfo.Destinations["destination0"], response1, null),
				new DestinationProbingResult(clusterInfo.Destinations["destination1"], response2, null),
			};

			// initially, health should be unknown
			foreach (var destination in clusterInfo.Destinations.Values)
			{
				Assert.That(destination.Health.Active, Is.EqualTo(DestinationHealth.Unknown), $"Unexpected health for destination {destination.DestinationId}");
			}

			policy.ProbingCompleted(clusterInfo, probingResults);
			Assert.That(clusterInfo.Destinations["destination0"].Health.Active, Is.EqualTo(DestinationHealth.Unhealthy));
			Assert.That(clusterInfo.Destinations["destination1"].Health.Active, Is.EqualTo(DestinationHealth.Healthy));

			// Passive health check status should not be affected
			foreach (var destination in clusterInfo.Destinations.Values)
			{
				Assert.That(destination.Health.Passive, Is.EqualTo(DestinationHealth.Unknown), $"Unexpected passive health for destination {destination.DestinationId}");
			}
		}

		[Test]
		public void SuccessfulResponseWithNoProcessId_MarksDestinationUnhealthy()
		{
			var policy = new ValidateProcessIdHealthCheckPolicy(new DestinationHealthUpdaterStub(), GetClusterConfig(), null);
			var clusterInfo = GetClusterInfo("cluster1", 2);
			using var response1 = GetResponse(HttpStatusCode.OK, 0);
			using var response2 = GetResponse(HttpStatusCode.OK, 22222);
			var probingResults = new[]
			{
				new DestinationProbingResult(clusterInfo.Destinations["destination0"], response1, null),
				new DestinationProbingResult(clusterInfo.Destinations["destination1"], response2, null),
			};

			// initially, health should be unknown
			foreach (var destination in clusterInfo.Destinations.Values)
			{
				Assert.That(destination.Health.Active, Is.EqualTo(DestinationHealth.Unknown), $"Unexpected health for destination {destination.DestinationId}");
			}

			policy.ProbingCompleted(clusterInfo, probingResults);
			Assert.That(clusterInfo.Destinations["destination0"].Health.Active, Is.EqualTo(DestinationHealth.Unhealthy));
			Assert.That(clusterInfo.Destinations["destination1"].Health.Active, Is.EqualTo(DestinationHealth.Healthy));

			// Passive health check status should not be affected
			foreach (var destination in clusterInfo.Destinations.Values)
			{
				Assert.That(destination.Health.Passive, Is.EqualTo(DestinationHealth.Unknown), $"Unexpected passive health for destination {destination.DestinationId}");
			}
		}

		[Test]
		public void UnsuccessfulResponse_MarksDestinationUnhealthy()
		{
			var policy = new ValidateProcessIdHealthCheckPolicy(new DestinationHealthUpdaterStub(), GetClusterConfig(), null);
			var clusterInfo = GetClusterInfo("cluster1", 2);
			using var response1 = GetResponse(HttpStatusCode.InternalServerError, null);
			using var response2 = GetResponse(HttpStatusCode.OK, 22222);
			var probingResults = new[]
			{
				new DestinationProbingResult(clusterInfo.Destinations["destination0"], response1, null),
				new DestinationProbingResult(clusterInfo.Destinations["destination1"], response2, null),
			};

			// initially, health should be unknown
			foreach (var destination in clusterInfo.Destinations.Values)
			{
				Assert.That(destination.Health.Active, Is.EqualTo(DestinationHealth.Unknown), $"Unexpected health for destination {destination.DestinationId}");
			}

			policy.ProbingCompleted(clusterInfo, probingResults);
			Assert.That(clusterInfo.Destinations["destination0"].Health.Active, Is.EqualTo(DestinationHealth.Unhealthy));
			Assert.That(clusterInfo.Destinations["destination1"].Health.Active, Is.EqualTo(DestinationHealth.Healthy));

			// Passive health check status should not be affected
			foreach (var destination in clusterInfo.Destinations.Values)
			{
				Assert.That(destination.Health.Passive, Is.EqualTo(DestinationHealth.Unknown), $"Unexpected passive health for destination {destination.DestinationId}");
			}
		}

		[Test]
		public void ExceptionDuringProbing_MarksDestinationUnhealthy()
		{
			var policy = new ValidateProcessIdHealthCheckPolicy(new DestinationHealthUpdaterStub(), GetClusterConfig(), null);
			var clusterInfo = GetClusterInfo("cluster1", 2);
			using var response2 = GetResponse(HttpStatusCode.OK, 22222);
			var probingResults = new[]
			{
				new DestinationProbingResult(clusterInfo.Destinations["destination0"], null, new InvalidOperationException()),
				new DestinationProbingResult(clusterInfo.Destinations["destination1"], response2, null),
			};

			// initially, health should be unknown
			foreach (var destination in clusterInfo.Destinations.Values)
			{
				Assert.That(destination.Health.Active, Is.EqualTo(DestinationHealth.Unknown), $"Unexpected health for destination {destination.DestinationId}");
			}

			policy.ProbingCompleted(clusterInfo, probingResults);
			Assert.That(clusterInfo.Destinations["destination0"].Health.Active, Is.EqualTo(DestinationHealth.Unhealthy));
			Assert.That(clusterInfo.Destinations["destination1"].Health.Active, Is.EqualTo(DestinationHealth.Healthy));

			// Passive health check status should not be affected
			foreach (var destination in clusterInfo.Destinations.Values)
			{
				Assert.That(destination.Health.Passive, Is.EqualTo(DestinationHealth.Unknown), $"Unexpected passive health for destination {destination.DestinationId}");
			}
		}

		InMemoryConfigProvider GetClusterConfig()
		{
			var cluster = new ClusterConfig
			{
				ClusterId = "cluster1",
				Destinations = new Dictionary<string, DestinationConfig>
				{
					["destination0"] = new DestinationConfig { Address = "http://localhost:10000", Metadata = new Dictionary<string, string> { { "ProcessId", "11111" } } },
					["destination1"] = new DestinationConfig { Address = "http://localhost:10001", Metadata = new Dictionary<string, string> { { "ProcessId", "22222" } } },
				},
			};
			return new InMemoryConfigProvider(Array.Empty<RouteConfig>(), new[] { cluster });
		}

		HttpResponseMessage GetResponse(HttpStatusCode status, int? processId)
		{
			var response = new
			{
				status = "Healthy",
				data = new Dictionary<string, int>(),
			};
			if (processId.HasValue)
			{
				response.data["ProcessId"] = processId.Value;
			}

			return new HttpResponseMessage(status)
			{
				Content = new StringContent(System.Text.Json.JsonSerializer.Serialize(response), System.Text.Encoding.UTF8, "application/json"),
			};
		}

		ClusterState GetClusterInfo(string id, int destinationCount)
		{
			using var httpMessageInvoker = new HttpMessageInvoker(new Mock<HttpMessageHandler>().Object);
			var clusterModel = new ClusterModel(
				new ClusterConfig
				{
					ClusterId = id,
					HealthCheck = new HealthCheckConfig
					{
						Active = new ActiveHealthCheckConfig
						{
							Enabled = true,
							Policy = "policy",
							Path = "/api/health/",
						},
					},
				},
				httpMessageInvoker);
			var clusterState = new ClusterState(id);
			typeof(ClusterState).GetProperty("Model").SetValue(clusterState, clusterModel);
			for (var i = 0; i < destinationCount; i++)
			{
				var destinationConfig = new DestinationModel(new DestinationConfig { Address = $"https://localhost:1000{i}/{id}/", Health = $"https://localhost:2000{i}/{id}/" });
				var destinationId = $"destination{i}";
				clusterState.Destinations.GetOrAdd(destinationId, d =>
				{
					var di = new DestinationState(destinationId);
					typeof(DestinationState).GetProperty("Model").SetValue(di, destinationConfig);
					return di;
				});
			}

			return clusterState;
		}

		class DestinationHealthUpdaterStub : IDestinationHealthUpdater
		{
			public void SetActive(ClusterState cluster, IEnumerable<NewActiveDestinationHealth> newHealthStates)
			{
				foreach (var newHealthState in newHealthStates)
				{
					newHealthState.Destination.Health.Active = newHealthState.NewActiveHealth;
				}
			}

			public void SetPassive(ClusterState cluster, DestinationState destination, DestinationHealth newHealth, TimeSpan reactivationPeriod)
			{
				throw new NotImplementedException();
			}
		}
	}
}
