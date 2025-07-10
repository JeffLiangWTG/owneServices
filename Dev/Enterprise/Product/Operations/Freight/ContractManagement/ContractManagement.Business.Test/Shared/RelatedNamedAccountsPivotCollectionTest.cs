using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.ContractManagement.Business.Testing
{
	[TestedType(typeof(RelatedNamedAccountsPivotCollection))]
	public class RelatedNamedAccountsPivotCollectionTest : ActiveBusinessObjectCollectionTestCase<RelatedNamedAccountsPivotCollection>
	{
		public void TestNewElementsHaveTablePrefixSet()
		{
			var contract = Factory.NewWithValidTestData<RatingContract>();
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var newContractPivot = contract.NamedAccountPivots.AddRelatedIfNotExist(org) as RatingContractNamedAccountPivot;
			AssertEquals("When RatingContract is master, table prefix should be RCT.", contract.TablePrefix, newContractPivot.RNP_ParentTableCode);

			var allocationLineMaster = contract.Allocations.AddNew();
			var org2 = Factory.NewWithValidTestData<OrgHeader>();
			var newAllocationPivot = allocationLineMaster.NamedAccountPivots.AddRelatedIfNotExist(org2) as RatingContractNamedAccountPivot;
			AssertEquals("When RatingContractAllocationLine is master, table prefix should be RCA.", allocationLineMaster.TablePrefix, newAllocationPivot.RNP_ParentTableCode);
		}

		protected override RelatedNamedAccountsPivotCollection GetCollectionToTest()
		{
			return new RelatedNamedAccountsPivotCollection(ContractMaster);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			var result = Factory.New<RatingContractNamedAccountPivot>();
			result.RNP_ParentID = ContractMaster.PK;
			result.RNP_ParentTableCode = ContractMaster.TablePrefix;
			result.RNP_OH_NamedAccount = Factory.New<OrgHeader>().PK;

			return result;
		}

		RatingContract ContractMaster => contractMaster ?? (contractMaster = Factory.NewWithValidTestData<RatingContract>());
		RatingContract contractMaster;
	}
}
