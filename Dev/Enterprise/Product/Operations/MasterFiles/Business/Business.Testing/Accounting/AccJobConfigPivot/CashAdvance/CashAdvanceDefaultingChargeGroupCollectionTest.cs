using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(CashAdvanceDefaultingChargeCodeCollection))]
	sealed class CashAdvanceDefaultingChargeCodeCollectionTest : BusinessObjectCollectionTestCase
	{
		AccCashAdvanceDefaultingConfiguration Master;

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			Master = Factory.New<AccCashAdvanceDefaultingConfiguration>();
			return new CashAdvanceDefaultingChargeCodeCollection(Master);
		}

		public void TestSetDefaultsForNewChild()
		{
			var collection = (CashAdvanceDefaultingChargeCodeCollection)GetCollectionToTest();
			var cashAdvanceDefaultingChargeCode = collection.AddNew();

			AssertEquals("JCT_JCF_JobConfig is set to AccCashAdvanceDefaultingConfiguration PK", Master.PK, cashAdvanceDefaultingChargeCode.JCT_JCF_JobConfig);
		}

		public void TestRelationshipFilter()
		{
			var chargeCode = Factory.NewWithValidTestData<AccChargeCode>();
			var config = Factory.New<AccCashAdvanceDefaultingConfiguration>();
			var chargeCodePivot = Factory.New<CashAdvanceDefaultingChargeCode>();
			chargeCodePivot.JCT_ParentId = chargeCode.PK;
			chargeCodePivot.JCT_JCF_JobConfig = config.PK;
			var chargeGroupPivot = Factory.New<CashAdvanceDefaultingChargeGroup>();
			chargeGroupPivot.JCT_Code = "FRT";
			chargeGroupPivot.JCT_JCF_JobConfig = config.PK;
			Factory.Save();

			var collection = new CashAdvanceDefaultingChargeGroupCollection(config);
			collection.Load();
			AssertContainsExactElementsInAnyOrder(chargeGroupPivot, collection);
		}
	}
}
