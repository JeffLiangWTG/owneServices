using System;
using System.Reflection;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Dispatcher;
using CargoWise.Application;
namespace Enterprise.Services.ServiceHost.ApplicationContext
{
	public class ObjectFactoryInstanceProvider : IInstanceProvider
	{
		readonly Type serviceType;
		readonly Lazy<MethodInfo> instanceFactory;

		public ObjectFactoryInstanceProvider(Type serviceType)
		{
			this.serviceType = serviceType;
			this.instanceFactory = new Lazy<MethodInfo>(GetInstanceFactory);
		}

		public object GetInstance(InstanceContext instanceContext, Message message)
		{
			return instanceFactory.Value.Invoke(null, null);
		}

		public object GetInstance(InstanceContext instanceContext)
		{
			return GetInstance(instanceContext, null);
		}

		public void ReleaseInstance(InstanceContext instanceContext, object instance)
		{
		}

		MethodInfo GetInstanceFactory()
		{
			return typeof(ObjectFactory).GetMethod(nameof(ObjectFactory.Get), Array.Empty<Type>()).MakeGenericMethod(serviceType);
		}
	}
}
