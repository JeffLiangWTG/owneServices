using System;
namespace Enterprise.Services.ServiceHost.ApplicationContext
{
	public class ObjectFactoryServiceHost : System.ServiceModel.ServiceHost
	{
		public ObjectFactoryServiceHost()
		{
		}

		public ObjectFactoryServiceHost(Type serviceType, params Uri[] baseAddresses)
			: base(serviceType, baseAddresses)
		{
		}

		protected override void OnOpening()
		{
			Description.Behaviors.Add(new ObjectFactoryServiceBehaviour());
			base.OnOpening();
		}
	}
}
