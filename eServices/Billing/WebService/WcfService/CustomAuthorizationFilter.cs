using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using Common.Logging;
using Hangfire.Dashboard;

namespace CargoWise.eServices.Billing.WcfService
{
	public class CustomAuthorizationFilter : IDashboardAuthorizationFilter
	{
		public bool Authorize(DashboardContext context)
		{
			var hostedServerNames = Configuration.HostedServerNames;
			var hostIPs = hostedServerNames.SelectMany(DnsResolver.GetHostAddresses).ToList();
			var environment = context.GetOwinEnvironment();
			var headers = environment["owin.RequestHeaders"] as IDictionary<string, string[]>;
			if (environment.TryGetValue("server.IsLocal", out var isLocalObj) && isLocalObj is bool isLocal && isLocal)
			{
				return true;
			}
			if (headers != null)
			{
				var currentIp = headers.TryGetValue("X-FORWARDED-FOR", out var forwardedFor) ? forwardedFor : Array.Empty<string>();
				Logger.InfoFormat($"requestHeaders for X-FORWARDED-FOR: {string.Join(",", currentIp)}");
				return IpMatch(hostIPs, currentIp);
			}
			return false;
		}

		private bool IpMatch(List<IPAddress> ipAddresses, string[] currentIp)
		{
			return currentIp.Any(ip => ipAddresses.Any(addr => addr.ToString() == ip));
		}

		static readonly ILog Logger = LogManager.GetLogger(typeof (CustomAuthorizationFilter));
		protected IConfigurationProvider Configuration { get; } = Global.WindsorContainer.Resolve<IConfigurationProvider>();
		protected IDnsResolver DnsResolver { get; } = Global.WindsorContainer.Resolve<IDnsResolver>();
	}
}
