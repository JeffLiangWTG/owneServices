using System;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.AspNetCore.Mvc.Controllers;

namespace CargoWise.RefDbRepo.NewService.Test
{
	class ExternalControllersFeatureProvider : IApplicationFeatureProvider<ControllerFeature>
	{
		private readonly Type[] _controllers;

		public ExternalControllersFeatureProvider(params Type[] controllers)
		{
			_controllers = controllers ?? Array.Empty<Type>();
		}

		public void PopulateFeature(IEnumerable<ApplicationPart> parts, ControllerFeature feature)
		{
			foreach (var controller in _controllers)
			{
				feature.Controllers.Add(controller.GetTypeInfo());
			}
		}
	}
}
