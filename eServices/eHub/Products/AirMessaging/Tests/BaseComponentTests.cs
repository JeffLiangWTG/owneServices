using System;
using System.IO;
using System.Reflection;
using CargoWise.BizTalk.UnitTestFX;
using Microsoft.Test.BizTalk.PipelineObjects;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rhino.Mocks;

namespace Tests
{
	public abstract class BaseComponentTests
	{
		protected delegate void WritePropertyBagDelegate(string propName, ref object ptrVar);
		protected delegate void ReadPropertyBagDelegate(string propName, out object ptrVar, int errorLog);

		#region Asserts

		protected void AssertXmlStream(Stream expected, Stream actual)
		{
			var compareResult = new XmlDiffTool().Execute(actual, expected);
			try { Assert.IsTrue(compareResult.Success); }
			catch (AssertFailedException) { throw new AssertFailedException(compareResult.OutputUpdateGram); }
		}

		protected void AssertException(Action action, Type exceptionType, string exceptionMessage)
		{
			try
			{
				action();
			}
			catch (Exception ex)
			{
				Assert.AreEqual(exceptionType, ex.GetType(), $"\r\nExpected: [{exceptionType.ToString()}].\r\nActual: [{ex.ToString()}].");
				Assert.AreEqual(exceptionMessage, ex.Message, $"Expected: [{exceptionMessage}]. Actual: [{ex.Message}].");
				return;
			}

			throw new AssertFailedException(string.Format("Expected exception with type {0}, but no exception occured", exceptionType.ToString()));
		}

		#endregion

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

		protected PipelineFactory PipelineFactory
		{
			get { return pipelineFactory ?? (pipelineFactory = new PipelineFactory()); }
		}
		PipelineFactory pipelineFactory;

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
