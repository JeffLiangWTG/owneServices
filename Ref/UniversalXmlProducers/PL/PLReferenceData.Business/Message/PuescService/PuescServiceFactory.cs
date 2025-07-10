using System.ServiceModel;
using System.ServiceModel.Channels;
using System.Text;
using CargoWise.RefDbRepo.PLReferenceData.Business.Message.PuescService.Credentials;

namespace CargoWise.RefDbRepo.PLReferenceData.Business.Message.PuescService;

sealed class PuescServiceFactory(IPuescServiceConfiguration puescServiceConfiguration) : IPuescServiceFactory
{
	readonly IPuescServiceConfiguration serviceConfiguration = puescServiceConfiguration;

	public IPuescService Create()
	{
		var customBinding = CreateCustomBinding();
		var endpoint = new EndpointAddress(serviceConfiguration.Url);
		var client = new DocumentHandlingPortClient(customBinding, endpoint);
		client.ChannelFactory.Endpoint.EndpointBehaviors.Remove(typeof(System.ServiceModel.Description.ClientCredentials));
		client.ChannelFactory.Endpoint.EndpointBehaviors.Add(new PuescClientCredentials());
		client.ClientCredentials.UserName.UserName = serviceConfiguration.Login;
		client.ClientCredentials.UserName.Password = serviceConfiguration.Password;

		return client;
	}

	static CustomBinding CreateCustomBinding()
	{
		var securityElement = SecurityBindingElement.CreateUserNameOverTransportBindingElement();
		securityElement.IncludeTimestamp = false;
		var encodingElement = new TextMessageEncodingBindingElement(MessageVersion.Soap11WSAddressingAugust2004, Encoding.UTF8);
		var transportElement = new HttpsTransportBindingElement()
		{
			AuthenticationScheme = System.Net.AuthenticationSchemes.Digest,
			MaxReceivedMessageSize = 100000000
		};

		return new(securityElement, encodingElement, transportElement);
	}
}
