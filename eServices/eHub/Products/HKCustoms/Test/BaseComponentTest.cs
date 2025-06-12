using Microsoft.Test.BizTalk.PipelineObjects;
using Rhino.Mocks;
using System;
using System.IO;
using System.Reflection;

namespace CargoWise.eHub.Products.HKCustoms.Test.PipelineComponents
{
	public abstract class BaseComponentTest
	{
		#region Implementation

		protected Stream GetEmbeddedResource(string resourceName)
		{
			return GetEmbeddedResource(resourceName, Assembly.GetExecutingAssembly());
		}

		protected Stream GetEmbeddedResource(string resourceName, Assembly executingAssembly)
		{
			string fullResourceName = executingAssembly.GetName().Name + '.' + resourceName;
			Stream resource = executingAssembly.GetManifestResourceStream(fullResourceName);
			if (resource == null)
			{
				throw new Exception(String.Format("Could not locate embedded resource '{0}'", fullResourceName));
			}
			return resource;
		}

		#endregion

		#region Properties

		protected MessageFactory MessageFactory
		{
			get { return messageFactory ?? (messageFactory = new MessageFactory()); }
		}
		MessageFactory messageFactory;

		protected MockRepository MockRepository
		{
			get { return mockRepository ?? (mockRepository = new MockRepository()); }
		}
		MockRepository mockRepository;

		#endregion
	}
}
