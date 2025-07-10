using CargoWise.Definitions;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineCore.DocumentSupport.Testing;
using NUnit.Framework;
using Constants = Enterprise.Core.Constants;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(JobServiceDocumentSupporter))]
	sealed class JobServiceDocumentSupporterTest : DocumentSupporterTest
	{
		public void TestDocumentSupporter()
		{
			var service = Factory.New<DummyWithServices>().Services.AddNew();
			AssertEquals("Document Supporter should be of type", typeof(JobServiceDocumentSupporter), service.DocumentSupporter.GetType());
		}

		public void TestService()
		{
			var service = Factory.New<DummyWithServices>().Services.AddNew();
			JobServiceDocumentSupporter docSupporter = (JobServiceDocumentSupporter)service.DocumentSupporter;
			AssertEquals("Document Supporter Service should be TestService", service.PK, docSupporter.Service.PK);
		}

		public void TestSupportedDataContext()
		{
			var service = Factory.New<DummyWithServices>().Services.AddNew();
			AssertEquals("Core.Constants.DataContext.RequestForService is Supported", true, service.DocumentSupporter.IsDataContextSupported(new DataContextValueForTesting(Core.Constants.DataContext.RequestForService)));
			AssertEquals("Core.Constants.DataContext.GenericFreightJob is Supported", true, service.DocumentSupporter.IsDataContextSupported(new DataContextValueForTesting(Core.Constants.DataContext.GenericFreightJob)));
		}

		public void TestBusinessContext()
		{
			var service = Factory.New<DummyWithServices>().Services.AddNew();
			AssertEquals(BusinessContext.JobService, service.DocumentSupporter.BusinessContext);
		}

		public void TestShowDocumentsInDynamicMenu()
		{
			var service = Factory.New<DummyWithServices>().Services.AddNew();
			AssertEquals(false, service.DocumentSupporter.ShowDocumentsInDynamicMenu);

			var documentSupportableService = GetDocumentSupportableBusinessObject();
			AssertEquals(true, documentSupportableService.DocumentSupporter.ShowDocumentsInDynamicMenu);
		}

		public void TestGenericFreightJobDocumentWrapper()
		{
			AssertEquals(1, GetDocumentSupportableBusinessObject().DocumentSupporter.GetDocumentWrappers(Constants.DataContext.GenericFreightJob, null).Length);
		}

		#region Implementation

		protected override IDocumentSupportable GetDocumentSupportableBusinessObject()
		{
			var shipment = Factory.New<Enterprise.Integration.Forwarding.IForwardingShipment>();
			var propertyInfo = shipment.GetType().GetProperty("DocsAndCartage", System.Reflection.BindingFlags.GetProperty | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.DeclaredOnly | System.Reflection.BindingFlags.Public);
			var parent = propertyInfo.GetValue(shipment, null) as IHaveServices;
			var service = parent.Services.AddNew();
			return service;
		}

		#endregion
	}
}
