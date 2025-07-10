using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.ContractManagement.Business;
using Enterprise.Freight.Forwarding.Business.HelperClasses.FilteredRatingContractAllocationLine;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;
using static Enterprise.Core.Constants;

namespace Enterprise.Freight.Forwarding.Business.Testing
{
	[TestedType(typeof(FilteredRatingContractAllocationLineCollection))]
	internal class FilteredRatingContractAllocationLineCollectionTest : ActiveBusinessObjectCollectionTestCase<FilteredRatingContractAllocationLineCollection>
	{
		public void TestFilterSetsCorrectAllocationLineIdFirstContract()
		{
			var collection = GetCollectionToTest();
			var contract1 = Factory.New<RatingContract>();
			var contract2 = Factory.New<RatingContract>();

			var org1 = Factory.New<OrgHeader>();

			var allocationLine1 = contract1.Allocations.AddNew();
			allocationLine1.RCA_AllocationLineID = "002";
			var allocationLine2 = contract2.Allocations.AddNew();
			allocationLine2.RCA_AllocationLineID = "002";

			contract1.RCT_ContractNumber = "CONTRACT1";
			contract1.RCT_OH = org1.PK;
			contract1.RCT_ContractType = RatingContractTypes.Provider;

			carrierContractPK = contract1.PK;

			contract2.RCT_ContractNumber = "CONTRACT2";

			var pk = (collection as IFindBoxListProvider).PrimaryKeyFromCode("002");

			AssertEquals(allocationLine1.PK, pk);
		}

		public void TestFilterSetsCorrectAllocationLineIdSecondContract()
		{
			var collection = GetCollectionToTest();
			var contract1 = Factory.New<RatingContract>();
			var contract2 = Factory.New<RatingContract>();

			var allocationLine1 = contract1.Allocations.AddNew();
			allocationLine1.RCA_AllocationLineID = "002";
			var allocationLine2 = contract2.Allocations.AddNew();
			allocationLine2.RCA_AllocationLineID = "002";

			contract1.RCT_ContractNumber = "CONTRACT1";

			var org1 = Factory.New<OrgHeader>();

			contract2.RCT_ContractNumber = "CONTRACT2";
			contract2.RCT_OH = org1.PK;
			contract2.RCT_ContractType = RatingContractTypes.Provider;

			carrierContractPK = contract2.PK;

			var pk = (collection as IFindBoxListProvider).PrimaryKeyFromCode("002");

			AssertEquals(allocationLine2.PK, pk);
		}

		public void TestCompleteFilterIsCleared()
		{
			var collection = GetCollectionToTest();
			var contract1 = Factory.New<RatingContract>();
			var contract2 = Factory.New<RatingContract>();

			var org1 = Factory.New<OrgHeader>();

			var allocationLine1 = contract1.Allocations.AddNew();
			allocationLine1.RCA_AllocationLineID = "002";
			var allocationLine2 = contract2.Allocations.AddNew();
			allocationLine2.RCA_AllocationLineID = "002";

			contract1.RCT_OH = org1.PK;
			contract1.RCT_ContractNumber = "CONTRACT1";
			contract1.RCT_ContractType = RatingContractTypes.Provider;

			contract2.RCT_OH = org1.PK;
			contract2.RCT_ContractNumber = "CONTRACT2";
			contract2.RCT_ContractType = RatingContractTypes.Provider;

			carrierContractPK = contract2.PK;

			var pk = (collection as IFindBoxListProvider).PrimaryKeyFromCode("002");

			AssertEquals(allocationLine2.PK, pk);

			carrierContractPK = contract1.PK;

			collection = GetCollectionToTest();
			pk = (collection as IFindBoxListProvider).PrimaryKeyFromCode("002");
			AssertEquals(allocationLine1.PK, pk);
		}

		ZGuid carrierContractPK;

		protected override FilteredRatingContractAllocationLineCollection GetCollectionToTest() => new FilteredRatingContractAllocationLineCollection(Factory, () => carrierContractPK);
	}
}
