using System;
using System.IO;
using System.Reflection;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CargoWise.eHub.Products.SGCustoms.MHAccess.BizTalk.Tests
{
	public class TestBase
	{
		protected Stream GetEmbeddedResource(string resourceName)
		{
			var fullResourceName = Assembly.GetExecutingAssembly().GetName().Name + '.' + resourceName;
			var resource = Assembly.GetExecutingAssembly().GetManifestResourceStream(fullResourceName);
			if (resource == null)
				throw new Exception(string.Format("Could not locate embedded resource '{0}'", fullResourceName));
			return resource;
		}

		protected void AssertException(Action action, Type exceptionType, string exceptionMessage)
		{
			try
			{
				action();
			}
			catch (Exception ex)
			{
				Assert.AreEqual(exceptionType, ex.GetType());
				Assert.IsTrue(ex.Message.Contains(exceptionMessage),
					$"Expected \"{exceptionMessage}\" is contained by \"{ex.Message}\"");
				return;
			}

			throw new AssertFailedException($"Expected exception with type {exceptionType}, but no exception occured");
		}
	}
}