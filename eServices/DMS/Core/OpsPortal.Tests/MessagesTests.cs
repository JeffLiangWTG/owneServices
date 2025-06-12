using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using eServices.Dms.Core.OpsPortal.Services;
using eServices.Dms.Core.ServiceDefaults;
using eServices.eHubDataModel.eHubTransactionsCore;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Moq;
using Moq.Protected;
using Messages = eServices.Dms.Core.OpsPortal.Components.Pages.Messages;

namespace eServices.Dms.Core.OpsPortal.Tests;

[FixtureLifeCycle(LifeCycle.InstancePerTestCase)]
public class MessagesTests : BunitContext
{
	[Test]
	public void MessagesIndexRedirectsToElastic()
	{
		Services.AddDbContextFactory<eHubTransactionsContext>(options => options.UseInMemoryDatabase("DmsMessages"));

		this.SetupQuickGrid();

		var cut = Render<Messages.Index>();

		cut.Find("p").MarkupMatches("""
			<p>
			  To search for messages, please use the
			  <a href="https://eye.wtg.ws/s/eservices/app/r/s/oZg6X" target="_blank">eHub Messages Dashboard</a>.
			</p>
			""");
	}

	[Test]
	public void MessagesDetailsReturnsMessageInfo()
	{
		var httpResponse = new HttpResponseMessage(System.Net.HttpStatusCode.OK);
		httpResponse.Headers.Add(DmsHeaders.MessageTimeReceived, DateTimeOffset.MinValue.ToString());
		httpResponse.Headers.Add(DmsHeaders.MessageSenderId, "SENDER");
		httpResponse.Headers.Add(DmsHeaders.MessageRecipientId, "RECIPIENT");
		httpResponse.Headers.Add(DmsHeaders.MessageStatus, "Delivered");

		var httpClientFactory = new Mock<IHttpClientFactory>();
		var httpMessageHandler = new Mock<HttpMessageHandler>();
		httpMessageHandler.Protected()
			.Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
			.ReturnsAsync(httpResponse);

		var httpClient = new HttpClient(httpMessageHandler.Object)
		{
			BaseAddress = new Uri("http://localhost:5000/")
		};
		httpClientFactory.Setup(x => x.CreateClient("DmsMessagesApi")).Returns(httpClient);
		Services.AddSingleton(httpClientFactory.Object);

		var configuration = new ConfigurationBuilder().Build();
		Services.AddSingleton<IConfiguration>(configuration);

		Services.AddDbContextFactory<eHubTransactionsContext>(options => options.UseInMemoryDatabase("DmsMessages"));

		var id = Guid.NewGuid();
		var navigationManager = Services.GetRequiredService<NavigationManager>();
		var uri = navigationManager.GetUriWithQueryParameter("id", id);
		navigationManager.NavigateTo(uri);

		var cut = Render<Messages.Details>();

		cut.FindAll("dd").MarkupMatches($"""
			<dd class="col-sm-10">0001-01-01T00:00:00</dd>
			<dd class="col-sm-10">SENDER</dd>
			<dd class="col-sm-10">RECIPIENT</dd>
			<dd class="col-sm-10">Delivered</dd>
			<dd class="col-sm-10">
			  <a href="/messages/content?id={id}&amp;format=text" target="_blank">View</a>,
			  <a href="/messages/content?id={id}">Download</a>,
			</dd>
			""");
	}

	[Test]
	public void MessagesDetailsRedirectsIfNotFound()
	{
		var httpResponse = new HttpResponseMessage(System.Net.HttpStatusCode.NotFound);

		var httpClientFactory = new Mock<IHttpClientFactory>();
		var httpMessageHandler = new Mock<HttpMessageHandler>();
		httpMessageHandler.Protected()
			.Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
			.ReturnsAsync(httpResponse);

		var httpClient = new HttpClient(httpMessageHandler.Object)
		{
			BaseAddress = new Uri("http://localhost:5000/")
		};
		httpClientFactory.Setup(x => x.CreateClient("DmsMessagesApi")).Returns(httpClient);
		Services.AddSingleton(httpClientFactory.Object);

		var config = new Dictionary<string, string?>() { ["eHubAdminMessageDetailsAddress"] = "http://localhost:5001/eHubAdmin" };
		var configuration = new ConfigurationBuilder().AddInMemoryCollection(config).Build();
		Services.AddSingleton<IConfiguration>(configuration);

		Services.AddDbContextFactory<eHubTransactionsContext>(options => options.UseInMemoryDatabase("DmsMessages"));

		var id = Guid.NewGuid();
		var navigationManager = Services.GetRequiredService<NavigationManager>();
		var uri = navigationManager.GetUriWithQueryParameter("id", id);
		navigationManager.NavigateTo(uri);

		var cut = Render<Messages.Details>();

		Assert.That(navigationManager.Uri, Is.EqualTo($"http://localhost:5001/eHubAdmin?EI_PK={id}"));
	}

	[TestCase(null, typeof(FileStreamHttpResult))]
	[TestCase("text", typeof(ContentHttpResult))]
	public async Task MessagesContentReturnsMessageContent(string? format, Type responseType)
	{
		var httpResponse = new HttpResponseMessage(System.Net.HttpStatusCode.OK);
		httpResponse.Content = new StringContent("Hello World!");

		var httpClientFactory = new Mock<IHttpClientFactory>();
		var httpMessageHandler = new Mock<HttpMessageHandler>();
		httpMessageHandler.Protected()
			.Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
			.ReturnsAsync(httpResponse);

		var httpClient = new HttpClient(httpMessageHandler.Object) { BaseAddress = new Uri("http://localhost:5000/") };
		httpClientFactory.Setup(x => x.CreateClient("DmsMessagesApi")).Returns(httpClient);

		var result = await MessagesEndpoints.GetContent(httpClientFactory.Object, null!, Guid.Empty, format);

		Assert.That(result, Is.TypeOf(responseType));
		string? content = result switch
		{
			ContentHttpResult contentResult => contentResult.ResponseContent,
			FileStreamHttpResult fileStreamResult => new StreamReader(fileStreamResult.FileStream).ReadToEnd(),
			_ => result?.ToString()
		};
		Assert.That(content, Is.EqualTo("Hello World!"));
	}
}
