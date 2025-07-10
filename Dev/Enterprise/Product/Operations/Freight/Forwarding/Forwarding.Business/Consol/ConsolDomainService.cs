using System;
using CargoWise.EntityFramework;

namespace Enterprise.Freight.Forwarding.Business
{
	public class ConsolDomainService : IService
	{
		protected ConsolDomainService(BusinessObjectFactory factory)
		{
			this.factory = factory;
		}

		public static ConsolDomainService GetInstance(BusinessObjectFactory factory)
		{
			ConsolDomainService result = factory.ServiceContainer.GetService<ConsolDomainService>();
			if (result == null)
			{
				result = new ConsolDomainService(factory);
				factory.ServiceContainer.AddService(result);
			}
			return result;
		}

		public ForwardingModuleConsolCollection ModuleConsolCollection
		{
			get { return moduleConsolCollection ?? (moduleConsolCollection = new ForwardingModuleConsolCollection(factory)); }
			set
			{
				if (value != null && moduleConsolCollection != null)
				{
					throw new InvalidOperationException("You can only create one ForwardingModuleConsolCollection per factory");
				}
				moduleConsolCollection = value;
			}
		}
		ForwardingModuleConsolCollection moduleConsolCollection;

		readonly BusinessObjectFactory factory;
	}
}
