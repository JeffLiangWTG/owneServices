using System.Threading.Tasks;
using System.Windows.Forms;
using CargoWise.Blazor.Client.Integration.Messaging;
using Enterprise.Winzor.Architecture.Test;
using Moq;
using NUnit.Framework;

namespace WinzorFramework.RemoteClientServices.Test
{
	public class SystemToSystemTrustHandlerTest
	{
		[Test]
		public async Task TestSendMessageAsync()
		{
			var accessToken = "mockAccessToken";
			var postUrl = "https://example.com/api/post";
			var systemToSystemTrustHandler = new WinzorSystemToSystemTrustHandler();
			var trustMessage = new SystemToSystemTrustMessage() { AccessToken = accessToken, PostUrl = postUrl };
			var form = default(Form);
			using var ctx = new EnterpriseTestContext();
			await ctx.RenderEntryPointComponent(
				() =>
				{
					form = new Form();
					return form;
				},
				ctx.DefaultClientServices.WindowService);

			await form.InvokeWinzorDispatcherAsync(() =>
			{
				systemToSystemTrustHandler.SendMessage(accessToken, postUrl);
			});

			ctx.MockCargoWiseClientServices.WindowService.Verify(x => x.OpenUrlAsync(It.Is<SystemToSystemTrustMessage>(a => a.AccessToken == accessToken && a.PostUrl == postUrl)), Times.Once);
		}
	}
}
