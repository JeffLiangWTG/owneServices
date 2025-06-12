namespace eServices.Dms.Core.Tests;

public class WebTests
{
	[Explicit("Doesn't work in DAT")]
	[Test]
	public async Task GetWebResourceRootReturnsOkStatusCode()
	{
		// Arrange
		var appHost = await DistributedApplicationTestingBuilder.CreateAsync<Projects.eServices_Dms_Core_AppHost>();
		appHost.Services.ConfigureHttpClientDefaults(clientBuilder =>
		{
			clientBuilder.AddStandardResilienceHandler();
		});

		await using var app = await appHost.BuildAsync();
		var resourceNotificationService = app.Services.GetRequiredService<ResourceNotificationService>();
		await app.StartAsync();

		// Act
		var httpClient = app.CreateHttpClient("configapi");
		await resourceNotificationService.WaitForResourceAsync("configapi", KnownResourceStates.Running).WaitAsync(TimeSpan.FromSeconds(30));
		var response = await httpClient.GetAsync("/wtg/alive");

		// Assert
		Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
	}
}
