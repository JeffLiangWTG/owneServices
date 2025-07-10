using CargoWise.Definitions;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineCore.DocumentSupport.Testing;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.Freight.ContainerYard.Business;
using NUnit.Framework;

namespace Enterprise.Freight.ContainerYard.Testing
{
	[TestedType(typeof(GateTransportCFSDetailDocumentSupporter))]
	sealed class GateTransportCFSDetailDocumentSupporterTest : DocumentSupporterTest
	{
		public void TestSupportedDataContext()
		{
			var gateTransportCFSDetail = Factory.New<GateTransportCFSDetail>();
			AssertEquals("DataContext.GenericFreightJob is Supported", true, gateTransportCFSDetail.DocumentSupporter.IsDataContextSupported(new DataContextValueForTesting(Core.Constants.DataContext.GenericFreightJob)));
		}

		public void TestBusinessContext()
		{
			var gateTransportCFSDetail = Factory.New<GateTransportCFSDetail>();
			AssertEquals("Business context should be 'GateTransportCFSDet'", BusinessContext.GateTransportCFSDet, gateTransportCFSDetail.DocumentSupporter.BusinessContext);
		}

		public void TestGetDocumentWrappers()
		{
			var docSupporter = GetDocumentSupportableBusinessObject().DocumentSupporter;

			DocumentWrapper[] wrappers = docSupporter.GetDocumentWrappers(Core.Constants.DataContext.GenericFreightJob, null);
			AssertNotNull(wrappers);
			AssertEquals("FreightWrapperFromGateTransport", wrappers[0].GetType().Name);
		}

		#region Implementation

		protected override IDocumentSupportable GetDocumentSupportableBusinessObject()
		{
			return Factory.New<GateTransportCFSDetail>();
		}

		#endregion
	}
}
