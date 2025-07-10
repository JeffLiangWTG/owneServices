using Enterprise.Packing.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Packing.Module.Testing
{
	[TestedType(typeof(PalletTransactionFilterBusinessObject))]
	internal class PalletTransactionFilterBusinessObjectTest : FilterStripBusinessObjectTestCase
	{
		#region Test Filter Max Length

		public void TestFilterMaxLength()
		{
			CombineAssertions(() =>
			{
				var filterBizO = GetNewFilterStripBusinessObject();
				AssertEquals("MaxLength of ConnoteNo should be set correctly.", DtbBookingSchema.KM_TransportReference.MaxLength, filterBizO["ConnoteNo"].MaxLength);
				AssertEquals("MaxLength of ConsignmentID should be set correctly.", DtbBookingSchema.KM_JobID.MaxLength, filterBizO["ConsignmentID"].MaxLength);
				AssertEquals("MaxLength of Docket ID should be set correctly.", PkgPalletTransactionSchema.KTR_PaperDocketID.MaxLength, filterBizO["Docket ID"].MaxLength);
				AssertEquals("MaxLength of Equipment Code should be set correctly.", PkgPalletTransactionSchema.KTR_EquipmentCode.MaxLength, filterBizO["Equipment Code"].MaxLength);
				AssertEquals("MaxLength of Job Reference Number should be set correctly.", CusEntryNumSchema.CE_EntryNum.MaxLength, filterBizO["Job Reference Number"].MaxLength);
			});
		}

		#endregion

		public void TestJobReferenceFilter()
		{
			var pallet1 = Factory.NewWithValidTestData<PkgPalletTransaction>();
			pallet1.AdditionalReferenceNumbers.AddNew().CE_EntryNum = "ABC";

			var pallet2 = Factory.NewWithValidTestData<PkgPalletTransaction>();
			pallet2.AdditionalReferenceNumbers.AddNew().CE_EntryNum = "DEF";

			var pallet3 = Factory.NewWithValidTestData<PkgPalletTransaction>();
			pallet3.AdditionalReferenceNumbers.AddNew().CE_EntryNum = "ABC";

			Factory.Save();

			var filter = new PalletTransactionFilterBusinessObject();
			filter["Job Reference Number"].IsActive = true;

			((ModuleTextFilter)filter["Job Reference Number"]).Property = "ABC";
			var pallets = Factory.Load<PkgPalletTransaction>(filter.Filter);
			AssertCollectionContains(pallet1, pallets);
			AssertCollectionContains(pallet3, pallets);
			AssertCollectionNotContains(pallet2, pallets);

			((ModuleTextFilter)filter["Job Reference Number"]).Property = "DEF";
			pallets = Factory.Load<PkgPalletTransaction>(filter.Filter);
			AssertCollectionNotContains(pallet1, pallets);
			AssertCollectionNotContains(pallet3, pallets);
			AssertCollectionContains(pallet2, pallets);
		}

		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject()
		{
			return new PalletTransactionFilterBusinessObject();
		}
	}
}
