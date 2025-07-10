using CargoWise.EntityFramework;
using Enterprise.Customs.ASYCUDA.Business;
using Enterprise.Customs.ASYCUDA.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.PE.Manifest.Business.Testing
{
	[TestedType(typeof(AsycudaManifestHeader))]
	sealed class AsycudaManifestHeaderTest : AsycudaManifestHeaderAbstractTest
	{
		public void TestIAsycudaManifestHeader()
		{
			var bizObj = GetNewBusinessObject();
			bizObj.FillWithValidTestData();
			Factory.Save();
			AssertEquals(bizObj.GetType(), new BusinessObjectFactory().Load<Integration.Customs.ASYCUDA.PEManifest.IAsycudaManifestHeader>(bizObj.PK).GetType());
			AssertEquals(bizObj.GetType(), new BusinessObjectFactory().Load<ASYCUDA.Business.AsycudaManifestHeader>(bizObj.PK).GetType());
		}

		protected override BusinessObject GetNewBusinessObject() => GetNewBusinessObjectForDeleteTest(Factory);

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var header = factory.NewWithValidTestData<AsycudaManifestHeader>();
			header.SuspendCheckBusinessObjectType();
			header.AMA_JobReference = "C1234";
			return header;
		}

		public void TestBills()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			AssertType<AsycudaBillCollection<AsycudaBill, AsycudaManifestHeader>>(header.Bills);
		}

		public void TestDefaultGetTypes()
		{
			var header = AsycudaManifestHeaderHelper.CreateNew(Factory, Core.Constants.CountryCodes.Peru, PEManifestTypes.Codes.MAN);
			CombineAssertions(() =>
			{
				AssertEquals("Default Bill Type", typeof(AsycudaBill), header.GetBillType());
			});
		}

		public void TestClearBillsCargoNature()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			header.AMA_TransportMode = Core.Constants.TransportModes.Sea;
			var bill1 = header.Bills.AddNew();
			bill1.CargoNature = "1";
			var bill2 = header.Bills.AddNew();
			bill2.CargoNature = "20";

			header.AMA_TransportMode = Core.Constants.TransportModes.Air;
			AssertEquals("First Bill Cargo nature cleared", string.Empty, bill1.CargoNature);
			AssertEquals("Second Bill Cargo nature cleared", string.Empty, bill2.CargoNature);
		}

		public void TestClearBillsCargoCondition()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			header.AMA_TransportMode = Core.Constants.TransportModes.Sea;
			var bill1 = header.Bills.AddNew();
			bill1.CargoCondition = "1";
			var bill2 = header.Bills.AddNew();
			bill2.CargoCondition = "12";

			header.AMA_TransportMode = Core.Constants.TransportModes.Air;
			AssertEquals("First Bill Cargo Condition cleared", string.Empty, bill1.CargoCondition);
			AssertEquals("Second Bill Cargo Condition cleared", string.Empty, bill2.CargoCondition);
		}

		public void TestShowPackedItems()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			Assert("ShowPackedItems", !header.ShowPackedItems);
		}
	}
}
