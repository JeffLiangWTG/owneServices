using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Yarp.ReverseProxy;
using Yarp.ReverseProxy.Configuration;
using Yarp.ReverseProxy.Model;
using Yarp.ReverseProxy.SessionAffinity;

namespace CargoWise.Blazor.SessionBroker
{
	public interface IAffinityCookieValidator
	{
		bool Validate(HttpContext context);
	}

	public class AffinityCookieValidator : IAffinityCookieValidator
	{
		readonly ClusterState clusterState;
		readonly ClusterConfig clusterConfig;
		readonly ISessionAffinityPolicy cookieSessionAffinityPolicy;

		public AffinityCookieValidator(IProxyStateLookup proxyStateLookup, IProxyConfigProvider proxyConfigProvider, IEnumerable<ISessionAffinityPolicy> sessionAffinityPolicys)
		{
			clusterState = proxyStateLookup.GetClusters().Single();
			clusterConfig = proxyConfigProvider.GetConfig().Clusters.Single();
			cookieSessionAffinityPolicy = sessionAffinityPolicys.First(p => p.Name == SessionAffinityConstants.Policies.Cookie);
		}

		public bool Validate(HttpContext context)
		{
			var destinations = clusterState.Destinations.Values
														.Where(d => d.Health.Active == DestinationHealth.Healthy)
														.ToList();

			var affinityResult = cookieSessionAffinityPolicy.FindAffinitizedDestinations(context, clusterState, clusterConfig.SessionAffinity, destinations);
			return affinityResult.Status == AffinityStatus.OK;
		}
	}
}
