using System;
using CargoWise.RefDbRepo.Service.Schema_0_9_New;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.Extensions.DependencyInjection;

namespace CargoWise.RefDbRepo.NewSafeDataUpdateService
{
	public class CustomBodyModelValidator(IObjectModelValidator defaultValidator) : IObjectModelValidator
	{
		public void Validate(ActionContext actionContext, ValidationStateDictionary validationState, string prefix, object model)
		{
			if (model != null && model.GetType() == typeof(SerializedGeometry))
			{
				return;
			}

			defaultValidator.Validate(actionContext, validationState, prefix, model);
		}

		public static CustomBodyModelValidator GetCustomModelValidator(IServiceProvider serviceProvider, ServiceDescriptor serviceDescriptor)
		{
			IObjectModelValidator defaultValidator = null;
			if (serviceDescriptor.ImplementationFactory != null)
			{
				defaultValidator = (IObjectModelValidator)serviceDescriptor.ImplementationFactory(serviceProvider);
			}
			else if (serviceDescriptor.ImplementationType != null)
			{
				defaultValidator = (IObjectModelValidator)ActivatorUtilities.CreateInstance(serviceProvider, serviceDescriptor.ImplementationType);
			}
			else
			{
				defaultValidator = ActivatorUtilities.CreateInstance<ObjectModelValidator>(serviceProvider);
			}
			return new CustomBodyModelValidator(defaultValidator);
		}
	}
}
