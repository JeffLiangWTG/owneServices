using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace WinzorFramework.Extensions;

[System.Diagnostics.CodeAnalysis.SuppressMessage("StyleCop.CSharp.SpacingRules", "SA1010:Opening square brackets should be spaced correctly", Justification = "<Pending>")]
public static class ServiceCollectionExtensions
{
	/// <summary>
	/// Decorates a service of type <typeparamref name="TInterface"/> with a decorator of type <typeparamref name="TDecorator"/>.
	/// This method allows applying the decorator pattern to services registered in the Microsoft Dependency Injection container.
	/// </summary>
	/// <typeparam name="TInterface">The type of the service to be decorated.</typeparam>
	/// <typeparam name="TDecorator">The type of the decorator which implements <typeparamref name="TInterface"/> and accepts <typeparamref name="TInterface"/> as a constructor parameter.</typeparam>
	/// <param name="services">The <see cref="IServiceCollection"/> to add the decorated service to.</param>
	/// <exception cref="InvalidOperationException">Thrown if the service of type <typeparamref name="TInterface"/> is not registered.</exception>
	public static void Decorate<TInterface, TDecorator>(this IServiceCollection services)
	where TInterface : class
	where TDecorator : class, TInterface
	{
		var wrappedDescriptor = services.FirstOrDefault(s => s.ServiceType == typeof(TInterface))
			?? throw new InvalidOperationException($"{typeof(TInterface).Name} is not registered");

		var objectFactory = ActivatorUtilities.CreateFactory(typeof(TDecorator), [typeof(TInterface)]);

		services.Replace(ServiceDescriptor.Describe(
			typeof(TInterface),
			s => (TInterface)objectFactory(s, [s.CreateInstance(wrappedDescriptor)]),
			wrappedDescriptor.Lifetime));
	}

	static object CreateInstance(this IServiceProvider services, ServiceDescriptor descriptor)
	{
		if (descriptor.ImplementationInstance is not null)
		{
			return descriptor.ImplementationInstance;
		}

		if (descriptor.ImplementationFactory is not null)
		{
			return descriptor.ImplementationFactory(services);
		}

		if (descriptor.ImplementationType is not null)
		{
			return ActivatorUtilities.GetServiceOrCreateInstance(services, descriptor.ImplementationType);
		}

		throw new InvalidOperationException("ServiceDescriptor does not have a valid implementation type, instance, or factory.");
	}
}
