using CargoWise.RefDbRepo.PLReferenceData.Business.Message.PuescService;
using Moq;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.PLReferenceData.Tests.Message;

[TestFixture]
sealed class PuescServiceFactoryTest
{
	[Test]
	public void TestCreate() => Assert.Multiple(() =>
	{
		var configurationMock = Mock.Of<IPuescServiceConfiguration>(p =>
		p.Url == "https://example.com/seap_wsChannel/DocumentHandlingPort" &&
		p.Login == "TestLogin" &&
		p.Password == "TestPassword");

		var service = new PuescServiceFactory(configurationMock).Create();
		Assert.IsNotNull(service, "Service created");
		Assert.IsInstanceOf<DocumentHandlingPortClient>(service, "Service is of correct type.");
		var client = (DocumentHandlingPortClient)service;
		Assert.That(client.Endpoint.Address.Uri.ToString(), Is.EqualTo("https://example.com/seap_wsChannel/DocumentHandlingPort"), "Endpoint address is correct.");
		Assert.IsNotNull(client.ClientCredentials.UserName, "Client credentials are not null.");
		Assert.That(client.ClientCredentials.UserName.UserName, Is.EqualTo("TestLogin"), "User name is correct.");
		Assert.That(client.ClientCredentials.UserName.Password, Is.EqualTo("TestPassword"), "Password is correct.");
		Assert.That(client.Endpoint.Binding, Is.InstanceOf<System.ServiceModel.Channels.CustomBinding>(), "Endpoint is configured with a CustomBinding as expected.");
	});
}
