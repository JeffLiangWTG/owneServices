using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.SG.Access.Business.Testing
{
	[TestedType(typeof(AsycudaPack))]
	sealed class AsycudaPackTest : EnterpriseBusinessObjectTestCase
	{
		public void TestHasValidTradeNetPermit()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			var pack = bill.Packs.AddNew();
			Assert("Default to false.", !pack.HasValidTradeNetPermitNumber);
			var entryNumber = pack.PackedItem.CustomsEntryNumbers.AddNew();
			entryNumber.CE_EntryType = ASYCUDA.Business.Constants.CustomsEntryType.TradeNetPermit;
			entryNumber.CE_EntryNum = "00001";
			Factory.InvalidateCachedProperties();
			Assert("Should be true as the pack has a valid TradeNet Permit number.", pack.HasValidTradeNetPermitNumber);
		}

		public void TestPackedItemRelationship()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			var pack = bill.Packs.AddNew();
			AssertEquals("pack.IsOnePackedItemRelationship", true, pack.IsOnePackedItemRelationship);
		}

		public void TestIAsycudaPack()
		{
			var bizObj = GetNewBusinessObject();
			Factory.Save();
			AssertEquals(bizObj.GetType(), new BusinessObjectFactory().Load<Integration.Customs.ASYCUDA.SGAccess.IAsycudaPack>(bizObj.PK).GetType());
			AssertEquals(bizObj.GetType(), new BusinessObjectFactory().Load<ASYCUDA.Business.AsycudaPack>(bizObj.PK).GetType());
		}

		public void TestLinePriceCurrency_ReadOnly()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			var pack = bill.Packs.AddNew();
			Assert(pack.LinePriceCurrencyInfo.ReadOnly);
		}

		protected override BusinessObject GetBusinessObjectForFetchForLoad() => GetNewBusinessObjectForDeleteTest(Factory);

		protected override BusinessObject GetNewBusinessObject() => GetNewBusinessObjectForDeleteTest(Factory);

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var header = factory.NewWithValidTestData<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			return bill.Packs.AddNew();
		}
	}
}
