using CargoWise.Definitions;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineCore.DocumentSupport.Testing;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.Environment;
using NUnit.Framework;
using Constants = Enterprise.Core.Constants;

namespace Enterprise.OceanCarrier.Business.Testing
{
	[TestedType(typeof(CarrierShipmentCargoDocumentSupporter))]
	class CarrierShipmentCargoDocumentSupporterTest : DocumentSupporterTest
	{
		public void TestCustomisationSecurityCheckPoint()
		{
			AssertEquals(Env.Security.None, DocumentSupporter.CustomisationSecurityCheckpoint);
		}

		public void TestSupportedDataContext()
		{
			AssertEquals(expected: true, DocumentSupporter.IsDataContextSupported(new DataContextValueForTesting(Constants.DataContext.CarrierShipmentCargo)));
		}

		public void TestGetDocBusinessObject()
		{
			DocumentWrapper[] wrappers = DocumentSupporter.GetDocumentWrappers(Constants.DataContext.CarrierShipmentCargo, null);
			AssertNotNull(wrappers);
			AssertEquals(1, wrappers.Length);
		}

		public void TestBusinessContext()
		{
			AssertEquals(BusinessContext.CarrierShipmentCargo, DocumentSupporter.BusinessContext);
		}

		#region Implementation

		DocumentSupporter DocumentSupporter
		{
			get { return new CarrierShipmentCargoDocumentSupporter((CarrierShipmentCargo)GetDocumentSupportableBusinessObject()); }
		}

		protected override IDocumentSupportable GetDocumentSupportableBusinessObject()
		{
			return Factory.NewWithValidTestData<CarrierShipmentCargo>();
		}

		#endregion
	}
}
