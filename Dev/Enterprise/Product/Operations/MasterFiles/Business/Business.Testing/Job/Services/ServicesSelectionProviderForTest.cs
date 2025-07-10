using System;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class ServicesSelectionProviderForTest : IServicesSelectionProvider
	{
		public Func<IHaveServices, JobService[]> GetServicesToPrintImplementation { get; set; }
		public JobService[] GetServicesToPrint(IHaveServices parent)
		{
			return GetServicesToPrintImplementation != null ? GetServicesToPrintImplementation(parent) : null;
		}
	}
}
