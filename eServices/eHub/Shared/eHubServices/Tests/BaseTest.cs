using System;
using System.IO;
using System.Reflection;
using CargoWise.eHub.Share.eHubServices.Tests.eHubSender.Mock;
using Common.Logging;

namespace CargoWise.eHub.Share.eHubServices.Tests
{
	public class BaseTest
	{
		readonly ILog logger;

		public BaseTest()
		{
			logger = new TestLogger();
		}

		protected Stream GetEmbeddedResource(string resourceName)
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


		protected ILog Logger
		{
			get
			{
				return logger;
			}
		}
	}
}
