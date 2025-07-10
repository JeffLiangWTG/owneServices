using CargoWise.Definitions;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineCore.DocumentSupport.Testing;
using Enterprise.Environment;
using NUnit.Framework;
using Constants = Enterprise.Core.Constants;

namespace Enterprise.ProcessManagement.Business.Test
{
	[TestedType(typeof(WorkRequestDocumentSupporter))]
	class WorkRequestDocumentSupporterTest : DocumentSupporterTest
	{
		public void TestCustomisationSecurityCheckPoint()
		{
			var workRequest = Factory.New<WorkRequest>();
			AssertEquals(Env.Security.CustomerServiceTicketCustomiseDocuments, workRequest.DocumentSupporter.CustomisationSecurityCheckpoint);
		}

		public void TestSupportedDataContext()
		{
			var workRequest = Factory.New<WorkRequest>();
			AssertEquals(true, workRequest.DocumentSupporter.IsDataContextSupported(new DataContextValueForTesting(Constants.DataContext.GenericFreightJob)));
		}

		public void TestBusinessContext()
		{
			var workRequest = Factory.New<WorkRequest>();
			AssertEquals(BusinessContext.WorkRequest, workRequest.DocumentSupporter.BusinessContext);
		}

		protected override IDocumentSupportable GetDocumentSupportableBusinessObject()
		{
			return Factory.New<WorkRequest>();
		}
	}
}
