using System.Collections.ObjectModel;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.ServiceModel.Dispatcher;
namespace Enterprise.Services.ServiceHost.ApplicationContext
{
	public class ObjectFactoryServiceBehaviour : IServiceBehavior
	{
		public ObjectFactoryServiceBehaviour()
		{
		}

		public void AddBindingParameters(ServiceDescription serviceDescription, ServiceHostBase serviceHostBase, Collection<ServiceEndpoint> endpoints, BindingParameterCollection bindingParameters)
		{
		}

		public void ApplyDispatchBehavior(ServiceDescription serviceDescription, ServiceHostBase serviceHostBase)
		{
			var instanceProvider = new ObjectFactoryInstanceProvider(serviceDescription.ServiceType);
			foreach (var dispatcherBase in serviceHostBase.ChannelDispatchers)
			{
				var dispatcher = dispatcherBase as ChannelDispatcher;
				if (dispatcher != null)
				{
					foreach (var endpoint in dispatcher.Endpoints)
					{
						endpoint.DispatchRuntime.InstanceProvider = instanceProvider;
					}
				}
			}
		}

		public void Validate(ServiceDescription serviceDescription, ServiceHostBase serviceHostBase)
		{
		}
	}
}
