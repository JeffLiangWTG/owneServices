using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	[TestedType(typeof(ReconRefundedChargeCollection))]
	sealed class ReconRefundedChargeCollectionTest : CusCodeDataCollectionTest<ReconRefundedCharge>
	{
		public void TestAddNewIfNotExists()
		{
			var coll = new ReconRefundedChargeCollection(ReconDec);
			coll.AddNewIfNotExists(Core.Constants.USCustoms.FeeCodes.MerchandiseProcessing);
			AssertEquals(1, coll.Count);
			AssertNotNull(coll.GetFirstElementHaving(Core.Constants.USCustoms.FeeCodes.MerchandiseProcessing));
			coll.AddNewIfNotExists(Core.Constants.USCustoms.FeeCodes.MerchandiseProcessing);
			AssertEquals(1, coll.Count);
			coll.AddNewIfNotExists(Core.Constants.USCustoms.FeeCodes.Mushroom);
			AssertEquals(2, coll.Count);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			var result = Factory.New<ReconRefundedCharge>();
			result.Parent = ReconDec;
			return result;
		}

		protected override CusCodeDataCollection<ReconRefundedCharge> GetCusCodeDataCollection() => new ReconRefundedChargeCollection(ReconDec.ReconWrappedJobDeclaration);

		ReconDeclaration reconDec;
		ReconDeclaration ReconDec => reconDec ?? (reconDec = new ReconDeclaration(Factory.New<JobDeclaration>()));
	}
}
