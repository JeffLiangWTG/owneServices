using System;
using eServices.Configuration.Schemas;
using NUnit.Framework;
using eServices.Configuration.Framework;

namespace eServices.Configuration.Tests.Framework
{
	public class ConfigurationHandlerFactoryTests
	{
		[Test]
		public void Framework_ConfigurationHandlerFactory_Unknown()
		{
			try
			{
				var message = new ConfigurationMessage {Name = "Unknown"};
				ConfigurationHandlerFactory.GetConfigurationHandler(message);
				Assert.Fail("Expected exception not thrown.");
			}
			catch (Exception ex)
			{
				Assert.That(ex, Is.TypeOf<NotImplementedException>(), "Expected exception not thrown.");
			}
		}
	}
}
