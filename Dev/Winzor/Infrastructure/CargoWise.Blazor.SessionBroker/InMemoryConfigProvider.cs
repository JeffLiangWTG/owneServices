// adapted from https://microsoft.github.io/reverse-proxy/articles/configproviders.html
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Primitives;
using Yarp.ReverseProxy.Configuration;

namespace CargoWise.Blazor.SessionBroker
{
	public class InMemoryConfigProvider : IProxyConfigProvider
	{
		volatile InMemoryConfig _config;
		readonly object configUpdateLocker = new ();

		public InMemoryConfigProvider(IReadOnlyList<RouteConfig> routes, IReadOnlyList<ClusterConfig> clusters)
		{
			_config = new InMemoryConfig(routes, clusters);
		}

		public IProxyConfig GetConfig() => _config;

		public void AddNodeToCluster(string clusterId, string identifier, string address, int pid)
		{
			lock (configUpdateLocker)
			{
				var oldConfig = _config;
				var oldCluster = oldConfig.Clusters.Single(x => x.ClusterId == clusterId);
				var newDestinations = new Dictionary<string, DestinationConfig>(oldCluster.Destinations);
				newDestinations.Add(identifier, new DestinationConfig
				{
					Address = address,
					Metadata = new Dictionary<string, string>()
					{
						["ProcessId"] = pid.ToString(CultureInfo.InvariantCulture),
					},
				});

				var newCluster = oldCluster with { Destinations = newDestinations };
				_config = new InMemoryConfig(oldConfig.Routes, oldConfig.Clusters.Where(x => x.ClusterId != clusterId).Append(newCluster).ToList().AsReadOnly());
				oldConfig.SignalChange();
			}
		}

		public void RemoveNodeFromCluster(string clusterId, string identifier)
		{
			lock (configUpdateLocker)
			{
				var oldConfig = _config;
				var oldCluster = oldConfig.Clusters.Single(x => x.ClusterId == clusterId);
				var newDestinations = new Dictionary<string, DestinationConfig>(oldCluster.Destinations);
				newDestinations.Remove(identifier);
				var newCluster = oldCluster with { Destinations = newDestinations };
				_config = new InMemoryConfig(oldConfig.Routes, oldConfig.Clusters.Where(x => x.ClusterId != clusterId).Append(newCluster).ToList().AsReadOnly());
				oldConfig.SignalChange();
			}
		}

		class InMemoryConfig : IProxyConfig
		{
			readonly CancellationTokenSource _cts = new CancellationTokenSource();

			public InMemoryConfig(IReadOnlyList<RouteConfig> routes, IReadOnlyList<ClusterConfig> clusters)
			{
				Routes = routes;
				Clusters = clusters;
				ChangeToken = new CancellationChangeToken(_cts.Token);
			}

			public IReadOnlyList<RouteConfig> Routes { get; private set; }

			public IReadOnlyList<ClusterConfig> Clusters { get; private set; }

			public IChangeToken ChangeToken { get; private set; }

			internal void SignalChange() => _cts.Cancel();
		}
	}

	public static class InMemoryConfigProviderExtensions
	{
		public static IReverseProxyBuilder LoadFromMemory(this IReverseProxyBuilder builder, IReadOnlyList<RouteConfig> routes, IReadOnlyList<ClusterConfig> clusters)
		{
			builder.Services.AddSingleton<IProxyConfigProvider>(new InMemoryConfigProvider(routes, clusters));
			return builder;
		}
	}
}
