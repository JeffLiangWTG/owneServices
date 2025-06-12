using System.Net;

namespace CargoWise.eServices.Billing.WcfService
{
	public interface IDnsResolver
	{
		IPAddress[] GetHostAddresses(string hostName);
	}

	public class DnsResolver : IDnsResolver
	{
		public IPAddress[] GetHostAddresses(string hostName)
		{
			return Dns.GetHostAddresses(hostName);
		}
	}
}
