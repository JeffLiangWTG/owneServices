using System;
using System.Linq;
using System.Reflection;

namespace CargoWise.eHub.Products.AirMessaging.PipelineComponents
{
	static class EnvelopContextFactory
	{
		public static EnvelopContext Create(ServiceProvider serviceProvider)
		{
			var envelopContextType = GetEnvelopContextType(serviceProvider);
			return envelopContextType == null ? null : (EnvelopContext)Activator.CreateInstance(envelopContextType);
		}

		static Type GetEnvelopContextType(ServiceProvider serviceProvider)
		{
			var assembly = Assembly.GetExecutingAssembly();
			return (from type in assembly.GetTypes() 
					let attributes = type.GetCustomAttributes(typeof (SupportedServiceProviderAttribute), false) 
					where attributes.Cast<SupportedServiceProviderAttribute>().Any(a => a.ServiceProvider == serviceProvider) 
					select type).FirstOrDefault();
		}
	}

	public enum ServiceProvider
	{
		Traxon,
		CCSJ,
		Delta,
		BT,
		CCN,
		GLSHK,
		Descartes,
		CargoStart,
		ARINC,
        Qatar,
        Cargonaut
	}
}
