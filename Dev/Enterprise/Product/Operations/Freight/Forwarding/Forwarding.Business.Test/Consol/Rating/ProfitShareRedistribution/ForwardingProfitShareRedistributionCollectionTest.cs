using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Rating.Business;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.Business.Testing
{
	[TestedType(typeof(ForwardingProfitShareRedistributionCollection))]
	sealed class ForwardingProfitShareRedistributionCollectionTest : BusinessObjectCollectionTestCase
	{
		public void TestCompanyFilter()
		{
			var branchA = Factory.NewWithValidTestData<GlbBranch>();
			var branchB = Factory.NewWithValidTestData<GlbBranch>();
			Factory.Save();

			using (Env.SetTemporaryUserContext(Env.CurrentUser.LoginName, branchA.PK.ToGuid(), Env.CurrentDepartmentPK))
			{
				CreateProfitShare("A00001");
				CreateProfitShare("A00002");
				Factory.Save();
			}

			using (Env.SetTemporaryUserContext(Env.CurrentUser.LoginName, branchB.PK.ToGuid(), Env.CurrentDepartmentPK))
			{
				CreateProfitShare("B00001");
				CreateProfitShare("B00002");
				Factory.Save();
			}

			CreateProfitShare("N00001", true);
			CreateProfitShare("N00002", true);
			CreateProfitShare("N00003", true);
			Factory.Save();

			AssertElementsInCollection(Env.CurrentBranchPK, new[] { "N00001", "N00002", "N00003" });
			AssertElementsInCollection(branchA.PK, new[] { "A00001", "A00002", "N00001", "N00002", "N00003" });
			AssertElementsInCollection(branchB.PK, new[] { "B00001", "B00002", "N00001", "N00002", "N00003" });

			void AssertElementsInCollection(ZGuid branchPK, string[] expectedBatchNumbers)
			{
				using (Env.SetTemporaryUserContext(Env.CurrentUser.LoginName, branchPK.ToGuid(), Env.CurrentDepartmentPK))
				{
					base.Collection.Load();
					AssertContainsExactElementsInAnyOrder(expectedBatchNumbers, base.Collection.Cast<ProfitShareRedistribution>().Select(x => x.PSR_BatchNumber));
				}
			}

			void CreateProfitShare(string batchNumber, bool overrideDefaultCompanyWithBlankPK = false)
			{
				var profitShareRedistribution = Factory.NewWithValidTestData<ProfitShareRedistribution>();
				profitShareRedistribution.PSR_BatchNumber = batchNumber;

				if (overrideDefaultCompanyWithBlankPK)
				{
					//To test that old records which are saved without company PK are visible to all companies
					profitShareRedistribution.PSR_GC_Company = ZGuid.Empty;
				}
			}
		}

		protected override BusinessObjectCollection GetCollectionToTest() => new ForwardingProfitShareRedistributionCollection(Factory);
	}
}
