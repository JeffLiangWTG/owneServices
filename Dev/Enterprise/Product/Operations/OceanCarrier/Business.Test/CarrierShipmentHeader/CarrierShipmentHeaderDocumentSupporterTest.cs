using CargoWise.Definitions;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineCore.DocumentSupport.Testing;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.Environment;
using NUnit.Framework;
using Constants = Enterprise.Core.Constants;

namespace Enterprise.OceanCarrier.Business.Testing
{
	[TestedType(typeof(CarrierShipmentHeaderDocumentSupporter))]
	class CarrierShipmentHeaderDocumentSupporterTest : DocumentSupporterTest
	{
		public void TestCustomisationSecurityCheckPoint()
		{
			AssertEquals(Env.Security.None, DocumentSupporter.CustomisationSecurityCheckpoint);
		}

		public void TestSupportedDataContext()
		{
			AssertEquals(expected: true, DocumentSupporter.IsDataContextSupported(new DataContextValueForTesting(Constants.DataContext.CarrierShipmentHeader)));
		}

		public void TestGetDocBusinessObject()
		{
			DocumentWrapper[] wrappers = DocumentSupporter.GetDocumentWrappers(Constants.DataContext.CarrierShipmentHeader, null);
			AssertNotNull(wrappers);
			AssertEquals(1, wrappers.Length);
		}

		public void TestBusinessContext()
		{
			AssertEquals(BusinessContext.OceanCarrierShipment, DocumentSupporter.BusinessContext);
		}

		#region Implementation

		DocumentSupporter DocumentSupporter
		{
			get { return new CarrierShipmentHeaderDocumentSupporter((CarrierShipmentHeader)GetDocumentSupportableBusinessObject()); }
		}

		protected override IDocumentSupportable GetDocumentSupportableBusinessObject()
		{
			return Factory.NewWithValidTestData<CarrierShipmentHeader>();
		}

		#endregion
	}
}
