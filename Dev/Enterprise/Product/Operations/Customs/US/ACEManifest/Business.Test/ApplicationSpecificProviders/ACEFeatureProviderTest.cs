using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Customs.US.ACEManifest.Business.Testing
{
	[TestedType(typeof(FeatureProvider))]
	sealed class ACEFeatureProviderTest : ASYCUDA.Business.Testing.FeatureProviderAbstractTest<FeatureProvider>
	{
		public void TestSupportArrivalInformation()
		{
			var featureProvider = new FeatureProvider();
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			AssertEquals("US Air AMS should have Arrival Information enabled. ", true, featureProvider.SupportArrivalInformation(header));
		}

		public void TestSupportArrivalTransfers()
		{
			var featureProvider = new FeatureProvider();
			AssertEquals("US Air AMS should have Arrival Transfers enabled. ", true, featureProvider.SupportArrivalTransfers);
		}

		public void TestExpectedQtyOnACEManifest()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			Assert(header.FeatureProvider.SupportArrivalInformation(header));
			header.AMA_ManifestType = ACEManifestTypes.Codes.IAM;
			Assert(header.FeatureProvider.SupportArrivalInformation(header));
			var p = header.ArrivalHeaders.AddNew();
			p.ATH_AMA_ManifestHeader = header.PK;
			Factory.Save();
			var bill = header.Bills.AddNew();
			bill.ABL_ManifestQty = 300;
			var arrHeader1 = header.ArrivalHeaders.AddNew();
			arrHeader1.ATH_VoyageFlightNo = "FLT1";
			arrHeader1.ATH_ETAAtDischargePort = ZDateTime.Today.AddDays(-2);
			var arrLine11 = arrHeader1.ArrivalDetails.AddNew();
			AssertEquals(0, arrLine11.ATL_ExpectedQty);
			arrLine11.ATL_ABL_AsycudaBill = bill.PK;
			AssertEquals(300, arrLine11.ATL_ExpectedQty);
			bill.ABL_ManifestQty = 400;
			AssertEquals(400, arrLine11.ATL_ExpectedQty);
			arrLine11.ATL_Quantity = 10;
			AssertEquals(10, arrLine11.ATL_Quantity);
			var arrHeader2 = header.ArrivalHeaders.AddNew();
			arrHeader2.ATH_VoyageFlightNo = "FLT2";
			arrHeader2.ATH_ETAAtDischargePort = ZDateTime.Today.AddDays(-1);
			var arrLine21 = arrHeader2.ArrivalDetails.AddNew();
			AssertEquals(0, arrLine21.ATL_ExpectedQty);
			arrLine21.ATL_ABL_AsycudaBill = bill.PK;
			AssertEquals(390, arrLine21.ATL_ExpectedQty);
			bill.ABL_ManifestQty = 500;
			AssertEquals(490, arrLine21.ATL_ExpectedQty);
			arrLine11.ATL_Quantity = 20;
			AssertEquals(20, arrLine11.ATL_Quantity);
			arrLine21.OnLoaded();
			AssertEquals(480, arrLine21.ATL_ExpectedQty);
		}

		public void TestSupportsAsycudaPacks()
		{
			var featureProvider = new FeatureProvider();
			AssertEquals("US Air AMS SupportsAsycudaPacks should be true.", true, featureProvider.SupportsAsycudaPacks);
		}
	}
}
