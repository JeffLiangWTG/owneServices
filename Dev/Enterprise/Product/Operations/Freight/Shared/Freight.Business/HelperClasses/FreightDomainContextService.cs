using CargoWise.EntityFramework;

namespace Enterprise.Freight.Business
{
	public static class FreightDomainContextService
	{
		public static bool SetFreightDomainContext(this BusinessObjectFactory factory, FreightDomainContext domainContext)
		{
			if (factory == null
				|| domainContext == FreightDomainContext.Unspecified)
			{
				return false;
			}

			var currentContext = factory.GetFreightDomainContext();

			if (currentContext == FreightDomainContext.Unspecified)
			{
				factory.GetCachedValue<FreightDomainContextValueHolder>().Context = domainContext;
				return true;
			}
			// TODO add ErrorReporter.ReportOnce for when the context is changing e.g.
			// else if (currentContext != domainContext)
			// ErrorReporter.ReportOnce("SwitchingFreightDomainContext", $"Switching freight domain context: {currentContext}->{domainContext}");
			// currently we have failing unit tests which will need to be fixed together with adding the ErrorReporter

			return false;
		}

		public static FreightDomainContext GetFreightDomainContext(this BusinessObjectFactory factory) =>
			factory?.GetCachedValue<FreightDomainContextValueHolder>().Context
				?? FreightDomainContext.Unspecified;

		sealed class FreightDomainContextValueHolder
		{
			public FreightDomainContext Context { get; set; }
		}
	}
}
