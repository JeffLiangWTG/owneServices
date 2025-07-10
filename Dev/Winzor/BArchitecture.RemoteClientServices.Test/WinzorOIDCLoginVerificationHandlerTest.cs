using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using CargoWise.Blazor.Client.Integration.Messaging;
using CargoWise.Blazor.Client.Integration.OIDCVerify;
using Enterprise.Winzor.Architecture.Test;
using Enterprise.ZArchitecture.Core;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using NUnit.Framework;
using WTG.OpenIDConnect.Login;

namespace WinzorFramework.RemoteClientServices.Test
{
	public class WinzorOIDCLoginVerificationHandlerTest
	{
		[Test]
		public async Task CancelLoginGetCorrectMessageAsync()
		{
			using var ctx = new EnterpriseTestContext();
			var oidcSettingsVerificationMock = new Mock<IOIDCSettingsVerification>();
			await ctx.RenderEntryPointComponent(() => new Form(), oidcSettingsVerificationMock.Object);

			var request = new OIDCLoginRequestMessageBuilderStub().Build();
			var loginServer = new WinzorOIDCLoginVerificationHandler();

			using var cts = new CancellationTokenSource();
			await cts.CancelAsync();

			var response = loginServer.LoginLocal(request, Mock.Of<OIDCWebLauncher>(), cts.Token, Mock.Of<OIDCLoginFactory>());
			Assert.That(response.Error, Is.EqualTo(OIDCLoginResponseMessage.ErrorType.OperationCanceled));
		}

		[TestCase(OIDCLoginResponseMessage.ErrorType.Timeout, "Login timed out", "Login timed out. Response took longer than 10 seconds.")]
		[TestCase(OIDCLoginResponseMessage.ErrorType.LoginFailed, "Failed Login", "TEST: whatever error description thrown by the client side")]
		[TestCase(OIDCLoginResponseMessage.ErrorType.OperationCanceled, "Login canceled", "Login request was canceled")]
		[TestCase(OIDCLoginResponseMessage.ErrorType.Exception, "Exception during login processing", "TEST: whatever error description thrown by the client side")]
		public async Task DifferentErrorTypeGetsCorrectMessageAsync(OIDCLoginResponseMessage.ErrorType errorType, string errorString, string errorDescription)
		{
			using var ctx = new EnterpriseTestContext();
			var oidcSettingsVerificationMock = new Mock<IOIDCSettingsVerification>();
			oidcSettingsVerificationMock
				.Setup(v => v.VerifyLoginAsync(It.IsAny<OIDCVerifySettingsRequestMessage>()))
				.ReturnsAsync(OIDCLoginResponseMessage.CreateFailedResponse(errorType, errorString, errorDescription));

			await ctx.RenderEntryPointComponent(() => new Form(), oidcSettingsVerificationMock.Object);

			var request = new OIDCLoginRequestMessageBuilderStub().Build();
			var loginServer = new WinzorOIDCLoginVerificationHandler();

			var response = loginServer.LoginLocal(request, Mock.Of<OIDCWebLauncher>(), new CancellationToken(), Mock.Of<OIDCLoginFactory>());
			Assert.That(response.Error, Is.EqualTo(errorType));
			Assert.That(response.ErrorString, Is.EqualTo(errorString));
			var expectedErrorDescription = errorType == OIDCLoginResponseMessage.ErrorType.Exception ? $"{errorString}\r\n{errorDescription}" : errorDescription;
			Assert.That(response.ErrorDescription, Is.EqualTo(expectedErrorDescription));
		}

		[Test]
		public async Task VerifyLoginAsyncShouldCallCancelLoginAsyncWhenThrowsOperationCancelledExceptionAsync()
		{
			using var ctx = new EnterpriseTestContext();
			var oidcSettingsVerificationMock = new Mock<IOIDCSettingsVerification>();
			oidcSettingsVerificationMock.Setup(asp => asp.VerifyLoginAsync(It.IsAny<OIDCVerifySettingsRequestMessage>())).ThrowsAsync(new OperationCanceledException());
			await ctx.RenderEntryPointComponent(() => new Form(), oidcSettingsVerificationMock.Object);

			var request = new OIDCLoginRequestMessageBuilderStub().Build();
			var loginServer = new WinzorOIDCLoginVerificationHandler();

			using var cts = new CancellationTokenSource();

			var response = loginServer.LoginLocal(request, Mock.Of<OIDCWebLauncher>(), cts.Token, Mock.Of<OIDCLoginFactory>());
			oidcSettingsVerificationMock.Verify(asp => asp.CancelLoginAsync(It.IsAny<String>()), Times.Once);
			Assert.That(response.Error, Is.EqualTo(OIDCLoginResponseMessage.ErrorType.OperationCanceled));
		}

		class OIDCLoginRequestMessageBuilderStub
		{
			readonly OIDCLoginRequestMessage _instance;

			public OIDCLoginRequestMessageBuilderStub()
			{
				_instance = new OIDCLoginRequestMessage(
					new Uri($"https://{GetHostName()}:1000"),
					"interactive.public",
					"",
					new[] { "openid", "profile", "offline_access" },
					OIDCLoginRequestMessage.LoginPrompt.Login,
					OIDCLoginRequestMessage.OIDCServer.Generic,
					"Succeed :)",
					"Failed :(",
					10,
					"EDI",
					Guid.NewGuid());
			}

			public OIDCLoginRequestMessage Build()
			{
				return _instance;
			}

			static string GetHostName()
			{
				var serverName = Environment.MachineName;
				var commonName = Dns.GetHostEntry(serverName).HostName;
				return commonName.ToLowerInvariant();
			}
		}
	}
}
