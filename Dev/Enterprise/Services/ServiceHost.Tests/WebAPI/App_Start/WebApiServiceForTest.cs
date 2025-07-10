using System;
using System.Web.Http.SelfHost;

namespace Enterprise.Services.ServiceHost.Tests
{
	public sealed class WebApiServiceForTest : IDisposable
	{
		public WebApiServiceForTest()
		{
			var port = "42800";
			Uri = new Uri("http://localhost:" + port);

			configuration = new HttpSelfHostConfiguration(Uri);
			WebApiServiceConfig.Register(configuration);
			configuration.EnsureInitialized();
		}

		readonly HttpSelfHostConfiguration configuration;
		public Uri Uri { get; }

		public IDisposable Run()
		{
			var server = new HttpSelfHostServer(configuration);
			try
			{
				server.OpenAsync().Wait();
				return new ServerCloseDisposable(server);
			}
			catch
			{
				server.Dispose();
				throw;
			}
		}

		public void Dispose()
		{
			configuration.Dispose();

			GC.SuppressFinalize(this);
		}

		sealed class ServerCloseDisposable : IDisposable
		{
			public ServerCloseDisposable(HttpSelfHostServer server)
			{
				this.server = server;
			}

			HttpSelfHostServer server;

			void IDisposable.Dispose()
			{
				if (server != null)
				{
					server.CloseAsync().Wait();
					server.Dispose();
					server = null;
				}
			}
		}
	}
}
