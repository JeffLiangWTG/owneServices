using System;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using OcmPoc.Infrastructure.MessageInterfaces.Documents;
using OcmPoc.Infrastructure.MessageInterfaces.Entities;
using Xunit;

namespace OcmPoc.FrontEnd.Api.Tests.Functional
{
	public class MessageFlowsApiShould : FunctionalTestFixture
	{
		Guid messageId;
		int messageFlowId;
		string messageContent;

		public MessageFlowsApiShould(WebApplicationFactory<Startup> webAppFactory) 
			: base(webAppFactory)
		{
			messageId = Guid.NewGuid();
			messageFlowId = 42;

			MockMessageFlowService
				.Setup(m => m.CreateNewFlowAsync(It.IsAny<Message>()))
				.Callback<Message>(message => messageContent = Encoding.UTF8.GetString(message.Body))
				.ReturnsAsync(new MessageFlow { InitialMessageId = messageId, Id = messageFlowId });
		}

		protected override void InitialiseServices(IServiceCollection services)
		{
			services.AddScoped(sp => MockMessageFlowService.Object);
		}

		[Fact]
		public async Task ReturnBadRequest_WhenInvalidMessagePosted()
		{
			var (statusCode, responseBody) = await PostAsync(string.Empty);

			statusCode.Should().Be(HttpStatusCode.BadRequest, responseBody);
		}

		[Fact]
		public async Task InitiateNewMessageFlow_WhenValidMessagePosted()
		{
			var message = "\"Hello, World!\"";

			var (statusCode, responseBody) = await PostAsync(message);

			statusCode.Should().Be(HttpStatusCode.OK, responseBody);
			messageContent.Should().Be(message.Trim('"'));
		}

		async Task<(HttpStatusCode, string)> PostAsync(string body)
		{
			using (var response = await Client.PostAsync("/api/message-flows", new StringContent(body, Encoding.UTF8, "application/json")))
			{
				return (response.StatusCode, await response.Content.ReadAsStringAsync());
			}
		}
	}
}
