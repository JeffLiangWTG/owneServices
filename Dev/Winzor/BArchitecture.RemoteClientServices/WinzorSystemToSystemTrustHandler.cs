using CargoWise.Blazor.Client.Integration.Messaging;
using Enterprise.Integration.SystemToSystemTrust;

namespace WinzorFramework.RemoteClientServices
{
	public class WinzorSystemToSystemTrustHandler : ISystemToSystemTrustHandler
	{
		public void SendMessage(string accessToken, string postUrl)
		{
			var trustMessage = new SystemToSystemTrustMessage() { AccessToken = accessToken, PostUrl = postUrl };
			CargoWiseClientInvoker.Invoke(cws => cws.WindowService.OpenUrlAsync(trustMessage));
		}
	}
}
