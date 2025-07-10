using CargoWise.Definitions;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineCore.DocumentSupport.Testing;
using Enterprise.Environment;
using NUnit.Framework;
using Constants = Enterprise.Core.Constants;

namespace Enterprise.OceanCarrier.Business.Testing
{
	[TestedType(typeof(CarrierVoyagePortCallDocumentSupporter))]
	class CarrierVoyagePortCallDocumentSupporterTest : DocumentSupporterTest
	{
		public void TestCustomisationSecurityCheckPoint()
		{
			AssertEquals(Env.Security.None, DocumentSupporter.CustomisationSecurityCheckpoint);
		}

		public void TestSupportedDataContext()
		{
			AssertEquals(expected: true, DocumentSupporter.IsDataContextSupported(new DataContextValueForTesting(Constants.DataContext.CarrierVoyagePortCall)));
		}

		public void TestGetDocBusinessObject()
		{
			var wrappers = DocumentSupporter.GetDocumentWrappers(Constants.DataContext.CarrierVoyagePortCall, null);
			AssertNotNull(wrappers);
			AssertEquals(1, wrappers.Length);
		}

		public void TestBusinessContext()
		{
			AssertEquals(BusinessContext.OCSVoyagePortCall, DocumentSupporter.BusinessContext);
		}

		#region Implementation

		DocumentSupporter DocumentSupporter
		{
			get { return new CarrierVoyagePortCallDocumentSupporter((CarrierVoyagePortCall)GetDocumentSupportableBusinessObject()); }
		}

		protected override IDocumentSupportable GetDocumentSupportableBusinessObject()
		{
			return Factory.NewWithValidTestData<CarrierVoyagePortCall>();
		}

		#endregion
	}
}
