// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using CargoWise.Blazor.Common;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using Yarp.ReverseProxy;
using Yarp.ReverseProxy.Configuration;
using Yarp.ReverseProxy.Forwarder;
using Yarp.ReverseProxy.Model;
using Yarp.ReverseProxy.SessionAffinity;

namespace CargoWise.Blazor.SessionBroker.Test
{
	// adapted from https://github.com/microsoft/reverse-proxy/blob/v1.0.0-preview11/test/ReverseProxy.Tests/Middleware/SessionAffinityMiddlewareTests.cs
	// Changes: * Namespace, class name, WTG analyser conformance
	//			* translated from Xunit to NUnit
	//			* Modified to test the changed behaviour from upstream (moved the AffinityKeyNotSet case from testing success to failure)
	public class CustomSessionAffinityMiddlewareTests
	{
		const string AffinitizedDestinationName = "dest-B";

		[Test]
		public async Task Invoke_SuccessfulFlow_CallNext()
		{
			var foundDestinationId = AffinitizedDestinationName;
			var builder = new CustomSessionAffinityMiddlewareBuilder();
			var sharedSecret = new SecureSecretGenerator().Generate();
			var foundDestination = builder.GetDestinationStateAndAddToStore(foundDestinationId, sharedSecret);
			var invokedMode = string.Empty;
			const string expectedMode = "Mode-B";
			var providers = builder.RegisterAffinityProviders(
				("Mode-A", AffinityStatus.DestinationNotFound, null, p => throw new InvalidOperationException($"Provider {p.Name} call is not expected.")),
				(expectedMode, AffinityStatus.OK, foundDestination, p => invokedMode = p.Name));
			var nextInvoked = false;

			var middleware = builder.GetCustomMiddleware(
				_ =>
				{
					nextInvoked = true;
					return Task.CompletedTask;
				},
				providers.Select(p => p.Object),
				Array.Empty<IAffinityFailurePolicy>(),
				new Mock<ILogger<CustomSessionAffinityMiddleware>>().Object);

			var context = builder.GetDefaultHttpContext(out var destinationFeature);

			await middleware.InvokeAsync(context);

			Assert.That(invokedMode, Is.EqualTo(expectedMode));
			Assert.That(nextInvoked, Is.True);
			providers[0].VerifyGet(p => p.Name, Times.Once);
			providers[0].VerifyNoOtherCalls();
			providers[1].VerifyAll();

			Assert.That(destinationFeature.AvailableDestinations, Has.Exactly(1).Items);
			Assert.That(destinationFeature.AvailableDestinations[0].DestinationId, Is.EqualTo(foundDestinationId));
		}

		[Test]
		public async Task Invoke_SuccessfulFlow_AuthHeaderApplied()
		{
			var builder = new CustomSessionAffinityMiddlewareBuilder();
			var foundDestinationId = AffinitizedDestinationName;
			var sharedSecret = new SecureSecretGenerator().Generate();
			var foundDestination = builder.GetDestinationStateAndAddToStore(foundDestinationId, sharedSecret);
			const string expectedMode = "Mode-B";
			var providers = builder.RegisterAffinityProviders(
				("Mode-A", AffinityStatus.DestinationNotFound, null, p => throw new InvalidOperationException($"Provider {p.Name} call is not expected.")),
				(expectedMode, AffinityStatus.OK, foundDestination, _ => { }
			));

			var middleware = builder.GetCustomMiddleware(
				_ => Task.CompletedTask,
				providers.Select(p => p.Object),
				Array.Empty<IAffinityFailurePolicy>(),
				new Mock<ILogger<CustomSessionAffinityMiddleware>>().Object);

			var context = builder.GetDefaultHttpContext(out var destinationFeature);

			await middleware.InvokeAsync(context);

			Assert.That(context.Request.Headers[CustomHeaders.CWSessionToken].First(), Is.EqualTo(sharedSecret));
		}

		[Test]
		public async Task Invoke_ErrorFlow_CallFailurePolicy(
			[Values(AffinityStatus.DestinationNotFound, AffinityStatus.AffinityKeyExtractionFailed, AffinityStatus.AffinityKeyNotSet)] AffinityStatus affinityStatus,
			[Values(true, false)] bool keepProcessing)
		{
			var builder = new CustomSessionAffinityMiddlewareBuilder();
			var providers = builder.RegisterAffinityProviders(("Mode-B", affinityStatus, null, _ => { }
			));
			var invokedPolicy = string.Empty;
			const string expectedPolicy = "Policy-1";
			var failurePolicies = builder.RegisterFailurePolicies(
				affinityStatus,
				("Policy-0", false, p => throw new InvalidOperationException($"Policy {p.Name} call is not expected.")),
				(expectedPolicy, keepProcessing, p => invokedPolicy = p.Name));
			var nextInvoked = false;
			var logger = AffinityTestHelper.GetLogger<CustomSessionAffinityMiddleware>();
			var middleware = builder.GetCustomMiddleware(
				_ =>
				{
					nextInvoked = true;
					return Task.CompletedTask;
				},
				providers.Select(p => p.Object),
				failurePolicies.Select(p => p.Object),
				logger.Object);
			var context = builder.GetDefaultHttpContext(out var destinationFeature);

			await middleware.InvokeAsync(context);

			Assert.That(invokedPolicy, Is.EqualTo(expectedPolicy));
			Assert.That(nextInvoked, Is.EqualTo(keepProcessing));
			failurePolicies[0].VerifyGet(p => p.Name, Times.Once);
			failurePolicies[0].VerifyNoOtherCalls();
			failurePolicies[1].VerifyAll();
			if (!keepProcessing)
			{
				logger.Verify(
					l => l.Log(LogLevel.Warning, EventIds.AffinityResolutionFailedForCluster, It.IsAny<It.IsAnyType>(), null, (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()),
					Times.Once);
			}
		}

		internal class CustomSessionAffinityMiddlewareBuilder
		{
			ClusterState cluster;
			readonly Endpoint endpoint;
			readonly SessionSecretStore secretStore;
			readonly ClusterModel clusterModel = new (
				new ClusterConfig
				{
					ClusterId = "cluster-1",
					SessionAffinity = new SessionAffinityConfig
					{
						Enabled = true,
						Policy = "Mode-B",
						FailurePolicy = "Policy-1",
					},
				},
				new HttpMessageInvoker(new Mock<HttpMessageHandler>().Object)
				);

			public CustomSessionAffinityMiddlewareBuilder()
			{
				cluster = GetCluster();
				typeof(ClusterState).GetProperty("Model")!.SetValue(cluster, clusterModel); // HACK: It has an internal setter for some reason ¯\_(ツ)_/¯
				endpoint = GetEndpoint(cluster);
				secretStore = new SessionSecretStore();
			}

			public IReadOnlyList<Mock<ISessionAffinityPolicy>> RegisterAffinityProviders(params (string Mode, AffinityStatus? Status, DestinationState Destinations, Action<ISessionAffinityPolicy> Callback)[] prototypes)
			{
				var expectedDestinations = cluster.Destinations.Values.ToList();
				var result = new List<Mock<ISessionAffinityPolicy>>();
				foreach (var (mode, status, destinations, callback) in prototypes)
				{
					var provider = new Mock<ISessionAffinityPolicy>(MockBehavior.Strict);
					provider.SetupGet(p => p.Name).Returns(mode);
					provider.Setup(p => p.FindAffinitizedDestinations(
						It.IsAny<HttpContext>(),
						cluster,
						clusterModel.Config.SessionAffinity,
						expectedDestinations))
					.Returns(new AffinityResult(destinations, status.Value))
					.Callback(() => callback(provider.Object));
					result.Add(provider);
				}
				return result.AsReadOnly();
			}

			public DestinationState GetDestinationStateAndAddToStore(string foundDestinationId, string sharedSecret)
			{
				cluster.Destinations.TryGetValue(foundDestinationId, out var foundDestination);
				secretStore.TryAdd(foundDestination!.DestinationId, sharedSecret);
				return foundDestination;
			}

			public CustomSessionAffinityMiddleware GetCustomMiddleware(
				RequestDelegate next,
				IEnumerable<ISessionAffinityPolicy> affinityProviders,
				IEnumerable<IAffinityFailurePolicy> failurePolicies,
				ILogger<CustomSessionAffinityMiddleware> logger)
			{
				var mockProxyStateLookup = new Mock<IProxyStateLookup>();
				mockProxyStateLookup.Setup(p => p.TryGetCluster(cluster.ClusterId, out cluster)).Returns(true);
				return new CustomSessionAffinityMiddleware(
					next,
					affinityProviders,
					failurePolicies,
					logger,
					mockProxyStateLookup.Object,
					secretStore);
			}

			public DefaultHttpContext GetDefaultHttpContext(out IReverseProxyFeature destinationFeature)
			{
				var context = new DefaultHttpContext();
				context.SetEndpoint(endpoint);
				destinationFeature = new ReverseProxyFeature()
				{
					AvailableDestinations = cluster.Destinations.Values.ToList(),
					Cluster = cluster.Model,
				};
				context.Features.Set(destinationFeature);
				return context;
			}

			public IReadOnlyList<Mock<IAffinityFailurePolicy>> RegisterFailurePolicies(AffinityStatus expectedStatus, params (string Name, bool Handled, Action<IAffinityFailurePolicy> Callback)[] prototypes)
			{
				var result = new List<Mock<IAffinityFailurePolicy>>();
				foreach (var (name, handled, callback) in prototypes)
				{
					var policy = new Mock<IAffinityFailurePolicy>(MockBehavior.Strict);
					policy.SetupGet(p => p.Name).Returns(name);
					policy.Setup(p => p.Handle(It.IsAny<HttpContext>(), cluster, expectedStatus))
						.ReturnsAsync(handled)
						.Callback(() => callback(policy.Object));
					result.Add(policy);
				}
				return result.AsReadOnly();
			}

			Endpoint GetEndpoint(ClusterState cluster)
			{
				var proxyRoute = new RouteConfig();
				var routeConfig = new RouteModel(proxyRoute, cluster, HttpTransformer.Default);
				var endpoint = new Endpoint(default, new EndpointMetadataCollection(routeConfig), string.Empty);
				return endpoint;
			}

			static ClusterState GetCluster()
			{
				var cluster = new ClusterState("cluster-1");
				var destinationManager = cluster.Destinations;
				destinationManager.GetOrAdd("dest-A", id => new DestinationState(id));
				destinationManager.GetOrAdd(AffinitizedDestinationName, id => new DestinationState(id));
				destinationManager.GetOrAdd("dest-C", id => new DestinationState(id));
				return cluster;
			}
		}
	}

	public static class AffinityTestHelper
	{
		public static Mock<ILogger<T>> GetLogger<T>()
		{
			var result = new Mock<ILogger<T>>();
			result.Setup(l => l.IsEnabled(It.IsAny<LogLevel>())).Returns(true);
			return result;
		}
	}
}
