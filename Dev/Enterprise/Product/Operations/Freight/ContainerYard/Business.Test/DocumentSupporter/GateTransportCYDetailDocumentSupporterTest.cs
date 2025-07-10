using CargoWise.Definitions;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineCore.DocumentSupport.Testing;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.Freight.ContainerYard.Business;
using NUnit.Framework;

namespace Enterprise.Freight.ContainerYard.Testing
{
	[TestedType(typeof(GateTransportCYDetailDocumentSupporter))]
	sealed class GateTransportCYDetailDocumentSupporterTest : DocumentSupporterTest
	{
		protected override IDocumentSupportable GetDocumentSupportableBusinessObject()
		{
			return Factory.New<GateTransportCYDetail>();
		}

		public void TestSupportedDataContext()
		{
			var gateTransportCYDetail = Factory.New<GateTransportCYDetail>();
			AssertEquals("DataContext.GenericFreightJob is Supported", true, gateTransportCYDetail.DocumentSupporter.IsDataContextSupported(new DataContextValueForTesting(Core.Constants.DataContext.GenericFreightJob)));
		}

		public void TestBusinessContext()
		{
			var gateTransportCYDetail = Factory.New<GateTransportCYDetail>();
			AssertEquals("Business context should be 'GateTransportCYDet'", BusinessContext.GateTransportCYDet, gateTransportCYDetail.DocumentSupporter.BusinessContext);
		}

		public void TestGetDocumentWrappers()
		{
			var docSupporter = GetDocumentSupportableBusinessObject().DocumentSupporter;

			DocumentWrapper[] wrappers = docSupporter.GetDocumentWrappers(Core.Constants.DataContext.GenericFreightJob, null);
			AssertNotNull(wrappers);
			AssertEquals("FreightWrapperFromGateTransport", wrappers[0].GetType().Name);
		}
	}
}
