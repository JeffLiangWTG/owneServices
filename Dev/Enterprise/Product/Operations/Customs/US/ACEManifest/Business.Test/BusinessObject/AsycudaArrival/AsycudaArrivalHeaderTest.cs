using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.US.ACEManifest.Business.Testing
{
	[TestedType(typeof(AsycudaArrivalHeader))]
	class AsycudaArrivalHeaderTest : EnterpriseBusinessObjectTestCase
	{
		public void TestReference()
		{
			var arrHeader = Factory.New<AsycudaArrivalHeader>();
			arrHeader.ATH_Reference = "a";
			AssertEquals("A", arrHeader.ATH_Reference);
			AssertExceptionThrown<MaxLengthExceededException>(() => arrHeader.ATH_Reference = "ab");
			ErrorReporter.Clear();
		}

		public void TestVoyageFlightNo()
		{
			var arrHeader = Factory.New<AsycudaArrivalHeader>();
			arrHeader.ATH_VoyageFlightNo = "Flt01234";
			AssertEquals("FLT01234", arrHeader.ATH_VoyageFlightNo);
			AssertExceptionThrown<MaxLengthExceededException>(() => arrHeader.ATH_VoyageFlightNo = "FLT123456");
			ErrorReporter.Clear();
		}

		public void TestCreateNewAsycudaTransferHeaderCollection()
		{
			var header = Factory.New<AsycudaArrivalHeader>();
			AssertEquals(typeof(ManifestBase.AsycudaTransferHeaderCollection<AsycudaTransferHeader>), header.TransferHeaders.GetType());
			AssertEquals(typeof(ManifestBase.AsycudaTransferHeaderCollection<AsycudaTransferHeader>), ((ASYCUDA.Business.AsycudaArrivalHeader)header).TransferHeaders.GetType());
		}

		public void TestGetNewValidation()
		{
			var header = Factory.New<AsycudaArrivalHeader>();
			AssertEquals(typeof(AsycudaArrivalHeaderValidation), header.Validation.GetType());
			AssertEquals(typeof(AsycudaArrivalHeaderValidation), ((ASYCUDA.Business.AsycudaArrivalHeader)header).Validation.GetType());
		}

		protected override BusinessObject GetNewBusinessObject() => GetNewBusinessObjectForDeleteTest(Factory);
		protected override BusinessObject GetBusinessObjectForFetchForLoad() => GetNewBusinessObjectForDeleteTest(Factory);
		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var header = factory.NewWithValidTestData<AsycudaManifestHeader>();
			header.SuspendCheckBusinessObjectType();
			return header.ArrivalHeaders.AddNew();
		}
	}
}
