using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CargoWise.Blazor.Common;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using WTG.Logging.Extensions;
using Yarp.ReverseProxy;
using Yarp.ReverseProxy.Configuration;
using Yarp.ReverseProxy.Model;
using Yarp.ReverseProxy.SessionAffinity;

namespace CargoWise.Blazor.SessionBroker
{
	// adapted from https://github.com/microsoft/reverse-proxy/blob/v1.0.0-preview11/src/ReverseProxy/Middleware/SessionAffinityMiddleware.cs
	// Modified to treat the absence of an affinity key as a failure (that should be handled by launching a new CargoWise.Winzor.AppServer process) instead of randomly assigning
	// an existing node in the cluster
	public class CustomSessionAffinityMiddleware
	{
		readonly RequestDelegate next;
		readonly IDictionary<string, ISessionAffinityPolicy> sessionAffinityProviders;
		readonly IDictionary<string, IAffinityFailurePolicy> affinityFailurePolicies;
		readonly ILogger logger;
		readonly IProxyStateLookup proxyStateLookup;
		readonly SessionSecretStore sessionSecretStore;

		public CustomSessionAffinityMiddleware(
			RequestDelegate next,
			IEnumerable<ISessionAffinityPolicy> affinityProviders,
			IEnumerable<IAffinityFailurePolicy> failurePolicies,
			ILogger<CustomSessionAffinityMiddleware> logger,
			IProxyStateLookup proxyStateLookup,
			SessionSecretStore sessionSecretStore)
		{
			this.next = next;
			this.sessionAffinityProviders = affinityProviders?.ToDictionaryByUniqueId(p => p.Name) ?? throw new ArgumentNullException(nameof(affinityProviders));
			this.affinityFailurePolicies = failurePolicies?.ToDictionaryByUniqueId(p => p.Name) ?? throw new ArgumentNullException(nameof(failurePolicies));
			this.logger = logger;
			this.proxyStateLookup = proxyStateLookup;
			this.sessionSecretStore = sessionSecretStore;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Literal string is safe to use in this context.")]
		public Task InvokeAsync(HttpContext context)
		{
			try
			{
				var proxyFeature = context.GetReverseProxyFeature();
				var cluster = proxyFeature.Cluster.Config;
				var options = cluster.SessionAffinity;

				if (!(options?.Enabled).GetValueOrDefault())
				{
					return next(context);
				}

				return InvokeInternalAsync(context, proxyFeature, options, cluster.ClusterId);
			}
			catch (Exception exception)
			{
				logger.LogError(exception, "Unhandled exception.");
				throw;
			}
		}

		async Task InvokeInternalAsync(HttpContext context, IReverseProxyFeature proxyFeature, SessionAffinityConfig options, string id)
		{
			if (!proxyStateLookup.TryGetCluster(id, out var cluster))
			{
				Log.AffinityResolutionFailedForCluster(logger, id);
				return;
			}

			var destinations = proxyFeature.AvailableDestinations;
			var currentProvider = sessionAffinityProviders.GetRequiredServiceById(options.Policy, SessionAffinityConstants.Policies.Cookie);
			var affinityResult = currentProvider.FindAffinitizedDestinations(context, cluster, options, destinations);

			switch (affinityResult.Status)
			{
				case AffinityStatus.OK:
					proxyFeature.AvailableDestinations = affinityResult.Destinations;
					// There should only ever be one App Server per affinitised request
					var destinationId = affinityResult.Destinations.Single().DestinationId;
					context.Request.Headers[CustomHeaders.CWSessionToken] = sessionSecretStore[destinationId];

					break;
				case AffinityStatus.AffinityKeyNotSet:
				case AffinityStatus.AffinityKeyExtractionFailed:
				case AffinityStatus.DestinationNotFound:
					var failurePolicy = affinityFailurePolicies.GetRequiredServiceById(options.FailurePolicy, SessionAffinityConstants.FailurePolicies.Redistribute);
					var keepProcessing = await failurePolicy.Handle(context, cluster, affinityResult.Status);

					if (!keepProcessing)
					{
						Log.AffinityResolutionFailedForCluster(logger, id);
						return;
					}

					Log.AffinityResolutionFailureWasHandledProcessingWillBeContinued(logger, id, options.FailurePolicy);

					break;
				default:
					throw new NotSupportedException($"Affinity status '{affinityResult.Status}' is not supported.");
			}

			await next(context);
		}

		static class Log
		{
			[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Literal string is safe to use in this context.")]
			static readonly Action<ILogger, string, Exception> _affinityResolutionFailedForCluster = LoggerMessage.Define<string>(
				LogLevel.Warning,
				EventIds.AffinityResolutionFailedForCluster,
				"Affinity resolution failed for cluster '{clusterId}'.");

			[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Literal string is safe to use in this context.")]
			static readonly Action<ILogger, string, string, Exception> _affinityResolutionFailureWasHandledProcessingWillBeContinued = LoggerMessage.Define<string, string>(
				LogLevel.Debug,
				EventIds.AffinityResolutionFailureWasHandledProcessingWillBeContinued,
				"Affinity resolution failure for cluster '{clusterId}' was handled successfully by the policy '{policyName}'. Request processing will be continued.");

			public static void AffinityResolutionFailedForCluster(ILogger logger, string clusterId)
			{
				_affinityResolutionFailedForCluster(logger.Enrich().WithAlert(LogEventCategory.Session, LogEventOutcome.Failure), clusterId, null);
			}

			public static void AffinityResolutionFailureWasHandledProcessingWillBeContinued(ILogger logger, string clusterId, string policyName)
			{
				_affinityResolutionFailureWasHandledProcessingWillBeContinued(logger.Enrich().WithEvent(LogEventCategory.Session, LogEventOutcome.Failure), clusterId, policyName, null);
			}
		}
	}

	public static class EventIds
	{
		public static readonly EventId AffinityResolutionFailedForCluster = new EventId(34, "AffinityResolutionFailedForCluster");
		public static readonly EventId AffinityResolutionFailureWasHandledProcessingWillBeContinued = new EventId(41, "AffinityResolutionFailureWasHandledProcessingWillBeContinued");
	}

	static class ServiceHelpers
	{
		public static IDictionary<string, T> ToDictionaryByUniqueId<T>(this IEnumerable<T> services, Func<T, string> idSelector)
		{
			if (services == null)
			{
				throw new ArgumentNullException(nameof(services));
			}

			var result = new Dictionary<string, T>(StringComparer.OrdinalIgnoreCase);

			foreach (var service in services)
			{
				if (!result.TryAdd(idSelector(service), service))
				{
					throw new ArgumentException($"More than one {typeof(T)} found with the same identifier.", nameof(services));
				}
			}

			return result;
		}

		public static T GetRequiredServiceById<T>(this IDictionary<string, T> services, string id, string defaultId)
		{
			var lookup = id;
			if (string.IsNullOrEmpty(lookup))
			{
				lookup = defaultId;
			}

			if (!services.TryGetValue(lookup, out var result))
			{
				throw new ArgumentException($"No {typeof(T)} was found for the id '{lookup}'.", nameof(id));
			}

			return result;
		}
	}
}
