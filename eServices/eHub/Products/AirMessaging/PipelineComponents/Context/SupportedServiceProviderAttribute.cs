using System;

namespace CargoWise.eHub.Products.AirMessaging.PipelineComponents
{
	[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
	sealed class SupportedServiceProviderAttribute : Attribute
	{
		public SupportedServiceProviderAttribute(ServiceProvider serviceProvider)
		{
			ServiceProvider = serviceProvider;
		}

		public readonly ServiceProvider ServiceProvider;
	}
}
