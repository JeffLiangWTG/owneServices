using System;
using System.ServiceModel.Activation;
namespace Enterprise.Services.ServiceHost.ApplicationContext
{
	public class ObjectFactoryServiceHostFactory : ServiceHostFactory
	{
		public ObjectFactoryServiceHostFactory()
		{
		}

		protected override System.ServiceModel.ServiceHost CreateServiceHost(Type serviceType, Uri[] baseAddresses)
		{
			return new ObjectFactoryServiceHost(serviceType, baseAddresses);
		}
	}
}
