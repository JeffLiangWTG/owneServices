using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.ServiceModel.Dispatcher;

namespace CargoWise.eServices.Billing.WcfService
{
	public class UnhandledExceptionBehaviorAttribute : Attribute, IServiceBehavior
	{
		public UnhandledExceptionBehaviorAttribute(Type errorHandlerType)
		{
			this.errorHandlerType = errorHandlerType;
		}

		public void Validate(ServiceDescription serviceDescription, ServiceHostBase serviceHostBase)
		{
		}

		public void AddBindingParameters(ServiceDescription serviceDescription, ServiceHostBase serviceHostBase, Collection<ServiceEndpoint> endpoints, BindingParameterCollection bindingParameters)
		{
		}

		public void ApplyDispatchBehavior(ServiceDescription serviceDescription, ServiceHostBase serviceHostBase)
		{
			IErrorHandler errorHandler;

			try
			{
				errorHandler = Activator.CreateInstance(errorHandlerType) as IErrorHandler;
			}
			catch (MissingMethodException e)
			{
				throw new ArgumentException("The errorHandlerType specified in the ErrorBehaviorAttribute constructor must have a public empty constructor.", e);
			}

			if (errorHandler == null)
			{
				throw new ArgumentException("The errorHandlerType specified in the ErrorBehaviorAttribute constructor must implement System.ServiceModel.Dispatcher.IErrorHandler.");
			}

			foreach (var channelDispatcher in GetChannelDispatchers(serviceHostBase))
			{
				channelDispatcher.ErrorHandlers.Add(errorHandler);
			}
		}

		internal virtual IEnumerable<ChannelDispatcher> GetChannelDispatchers(ServiceHostBase serviceHostBase)
		{
			return serviceHostBase.ChannelDispatchers.OfType<ChannelDispatcher>();
		}

		readonly Type errorHandlerType;
	}
}