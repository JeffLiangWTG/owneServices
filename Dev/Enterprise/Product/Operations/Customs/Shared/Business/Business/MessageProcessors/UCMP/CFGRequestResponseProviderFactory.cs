using System.Collections.Generic;
using Enterprise.MasterFiles.Business.Customs.XmlCredential;
using Enterprise.MasterFiles.Business.Customs.XtCredential;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Business.MessageProcessor;

namespace Enterprise.Customs.Business.MessageProcessors.UCMP
{
	public interface IRequestResponseProvider
	{
	}

	public interface IRequestResponseProvider<TRequest, TResponse> : IRequestResponseProvider
		 where TRequest : ICFGMessageRequest
		 where TResponse : ICFGMessageResponse
	{
		TRequest GetRequestMessage(EDIInterchange outgoingInterchange);
		TResponse ParseResponseMessage(EDIMessage message);
		object GetLinkedObjectFromRequest(EDIInterchange outgoingInterchange);
	}

	public class CFGRequestResponseProviderFactory
	{
		readonly Dictionary<string, IRequestResponseProvider> providers;

		public CFGRequestResponseProviderFactory()
		{
			providers = new Dictionary<string, IRequestResponseProvider>
			{
				{ Constants.Configuration.XHRecipient, new ConfigurationRequestResponseProvider() },
				{ XtCredentialConstants.CustomsCredentialChange, new CredentialChangeRequestResponseProvider() }
			};
		}

		public IRequestResponseProvider<TRequest, TResponse> GetProvider<TRequest, TResponse>(string configurationType)
			where TRequest : ICFGMessageRequest
			where TResponse : ICFGMessageResponse
		{
			return providers.TryGetValue(configurationType, out var provider) ? provider as IRequestResponseProvider<TRequest, TResponse> : null;
		}
	}
}
