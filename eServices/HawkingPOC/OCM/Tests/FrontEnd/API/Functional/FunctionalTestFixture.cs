using System.Net.Http;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using OcmPoc.Core.Services;
using OcmPoc.Infrastructure.MessageInterfaces.Queueing;
using OcmPoc.Infrastructure.MessageInterfaces.Repositories;
using OcmPoc.Infrastructure.MessageInterfaces.Tracking;
using Xunit;

namespace OcmPoc.FrontEnd.Api.Tests.Functional
{
	public abstract class FunctionalTestFixture : IClassFixture<WebApplicationFactory<Startup>>
	{
		protected static HttpClient Client { get; private set; }

		protected Mock<IMessageRepository> MockMessageRepository { get; }
		protected Mock<IMessageFlowRepository> MockMessageFlowRepository { get; }
		protected Mock<IMessageFlowService> MockMessageFlowService { get; }
		protected Mock<IQueueWriter> MockMessageQueueWriter { get; }
		protected Mock<IQueueClient> MockMessageQueueClient { get; }
		protected Mock<IEventLogger> MockEventLogger { get; }

		public FunctionalTestFixture(WebApplicationFactory<Startup> webAppFactory)
		{
			MockMessageRepository = new Mock<IMessageRepository>();
			MockMessageFlowRepository = new Mock<IMessageFlowRepository>();
			MockMessageFlowService = new Mock<IMessageFlowService>();
			MockMessageQueueWriter = new Mock<IQueueWriter>();
			MockMessageQueueClient = new Mock<IQueueClient>();
			MockEventLogger = new Mock<IEventLogger>();

			MockMessageQueueClient
				.Setup(m => m.GetQueueWriter(It.IsAny<string>()))
				.Returns(MockMessageQueueWriter.Object);

			if (Client == null)
			{
				Client = webAppFactory.WithWebHostBuilder(ConfigureBuilder)
									  .CreateClient();
			}
		}

		void ConfigureBuilder(IWebHostBuilder builder)
		{
			builder.ConfigureServices(InitialiseServices)
					.UseSetting("ConfiguredForTest", bool.TrueString);
		}

		protected abstract void InitialiseServices(IServiceCollection services);
	}
}
