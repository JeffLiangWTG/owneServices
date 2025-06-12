using System;
using System.IO;
using System.Reflection;
using CargoWise.eHub.Core.Tests.PipelineComponents;

namespace CargoWise.eHub.Products.JPCustoms.Tests
{
	public class PipelineComponentBaseTest : BaseComponentTest
	{
		protected new Stream GetEmbeddedResource(string resourceName)
		{
			string fullResourceName = Assembly.GetExecutingAssembly().GetName().Name + '.' + resourceName;
			var resource = Assembly.GetExecutingAssembly().GetManifestResourceStream(fullResourceName);
			if (resource == null)
			{
				throw new Exception(String.Format("Could not locate embedded resource '{0}'", fullResourceName));
			}
			return resource;
		}

		protected string GetEmbeddedResourceAsString(string resourceName)
		{
			using (var stream = GetEmbeddedResource(resourceName))
			{
				return new StreamReader(stream).ReadToEnd();
			}
		}
	}
}
