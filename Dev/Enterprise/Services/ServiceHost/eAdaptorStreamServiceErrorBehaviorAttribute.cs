using System;
using System.Collections.ObjectModel;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.ServiceModel.Dispatcher;

namespace Enterprise.Services.ServiceHost
{
	[AttributeUsage(AttributeTargets.Class)]
	public sealed class eAdaptorStreamServiceErrorBehaviorAttribute : Attribute, IServiceBehavior
	{
		public void AddBindingParameters(ServiceDescription serviceDescription, ServiceHostBase serviceHostBase, Collection<ServiceEndpoint> endpoints, BindingParameterCollection bindingParameters) { }

		public void ApplyDispatchBehavior(ServiceDescription serviceDescription, ServiceHostBase serviceHostBase)
		{
			foreach (ChannelDispatcher dispatcher in serviceHostBase.ChannelDispatchers)
			{
				dispatcher.ErrorHandlers.Add(new eAdaptorStreamServiceErrorHandler());
			}
		}

		public void Validate(ServiceDescription serviceDescription, ServiceHostBase serviceHostBase) { }
	}
}
