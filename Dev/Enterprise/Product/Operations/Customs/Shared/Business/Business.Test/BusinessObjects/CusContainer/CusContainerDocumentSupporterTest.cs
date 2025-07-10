using CargoWise.Definitions;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineCore.DocumentSupport.Testing;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.Business.Testing
{
	[TestedType(typeof(CusContainerDocumentSupporter))]
	sealed class CusContainerDocumentSupporterTest : DocumentSupporterTest
	{
		public void TestSupportedDataContexts()
		{
			var container = Factory.New<BaseCusContainer>();
			AssertEquals("Core.Constants.DataContext.Service is Supported", true, container.DocumentSupporter.IsDataContextSupported(new DataContextValueForTesting(Core.Constants.DataContext.Service)));
		}

		public void TestBusinessContext()
		{
			var container = Factory.New<BaseCusContainer>();
			AssertEquals("var has a business context of CusContainer", BusinessContext.CusContainer, container.DocumentSupporter.BusinessContext);
		}

		public void TestGetDocBusinessObjects()
		{
			GlbCompany.CurrentCompany.SetCountry("AU");
			var container = Factory.New<BaseCusContainer>();
			DocumentWrapper[] result = container.DocumentSupporter.GetDocumentWrappers(Core.Constants.DataContext.Service, null);
			AssertEquals("Document wrapper for data context of Service is of type DocCusContainer", "DocCusContainer", result[0].GetType().Name);
		}

		public void TestContainerNumberSwapping()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_TransportMode = declaration.TransportModeSeaCodeForTesting;

			var consol = Factory.New<ForwardingConsol>();
			var shipment = consol.Shipments.AddNew();
			declaration.JE_JS = shipment.PK;
			TestHelper.MakeConsolRelevantToDeclaration(consol, declaration);
			shipment.JS_RL_NKDestination = consol.JK_RL_NKDischargePort;
			shipment.JS_RL_NKOrigin = consol.JK_RL_NKLoadPort;

			var forwardingContainer123 = consol.Containers.AddNew();
			forwardingContainer123.JC_ContainerNum = "123";

			var forwardingContainer456 = consol.Containers.AddNew();
			forwardingContainer456.JC_ContainerNum = "456";

			var packLine1 = shipment.OuterPackLines.AddNew();
			packLine1.Containers.Add(forwardingContainer123);
			var packLine2 = shipment.OuterPackLines.AddNew();
			packLine2.Containers.Add(forwardingContainer456);
			Factory.Save();
			AssertEquals("Precondition", 2, shipment.ArrivalContainers.Count);

			var customsContainer = declaration.CusContainers.AddNew();
			customsContainer.CO_ContainerNumber = "123";
			AssertEquals(forwardingContainer123, customsContainer.JobContainer);

			customsContainer.CO_ContainerNumber = "456";
			AssertEquals("Cus Container should point to second consol container 'forwardingContainer456'",
						forwardingContainer456, customsContainer.JobContainer);
			AssertEquals("Should be 2 Containers for Consol", 2, consol.Containers.Count);
		}

		public void TestChangingJobContainerSwapsServices()
		{
			var freightContainer = Factory.New<CommonContainer>();
			JobService service = freightContainer.Services.AddNew();

			var container = Factory.New<BaseCusContainer>();
			BaseCusContainer.JobServiceCollectionWrapper originalServices = container.Services;
			AssertEquals(0, originalServices.Count);

			container.CO_JC = freightContainer.PK;
			AssertEquals(1, container.Services.Count);
			AssertEquals(originalServices, container.Services);
		}

		public void TestPenalties()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_TransportMode = declaration.TransportModeSeaCodeForTesting;
			declaration.JE_RL_NKOrigin = "USNYC";
			declaration.JE_RL_NKFinalDestination = "USCHI";
			var freightContainer = declaration.CusContainers.AddNew();

			AssertEquals(false, freightContainer.ImportPenalties.ReadOnly);
			AssertEquals(false, freightContainer.ExportPenalties.ReadOnly);

			var importPenalty = freightContainer.ImportPenalties.AddNew();
			var exportPenalty = freightContainer.ExportPenalties.AddNew();

			AssertEquals(1, freightContainer.ImportPenalties.Count);
			AssertEquals(importPenalty, freightContainer.ImportPenalties[0]);

			AssertEquals(1, freightContainer.ExportPenalties.Count);
			AssertEquals(exportPenalty, freightContainer.ExportPenalties[0]);
		}

		#region Implementation

		protected override IDocumentSupportable GetDocumentSupportableBusinessObject()
		{
			var container = Factory.New<BaseCusContainer>();
			return container;
		}
		#endregion
	}
}
