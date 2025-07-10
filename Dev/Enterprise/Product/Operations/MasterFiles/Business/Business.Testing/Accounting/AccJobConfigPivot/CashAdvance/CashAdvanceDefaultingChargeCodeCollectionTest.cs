using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(CashAdvanceDefaultingChargeGroupCollection))]
	sealed class CashAdvanceDefaultingChargeGroupCollectionTest : BusinessObjectCollectionTestCase
	{
		AccCashAdvanceDefaultingConfiguration Master;

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			Master = Factory.New<AccCashAdvanceDefaultingConfiguration>();
			return new CashAdvanceDefaultingChargeGroupCollection(Master);
		}

		public void TestSetDefaultsForNewChild()
		{
			var collection = (CashAdvanceDefaultingChargeGroupCollection)GetCollectionToTest();
			var cashAdvanceDefaultingChargeGroup = collection.AddNew();

			AssertEquals("JCT_JCF_JobConfig is set to AccCashAdvanceDefaultingConfiguration PK", Master.PK, cashAdvanceDefaultingChargeGroup.JCT_JCF_JobConfig);
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

			var collection = new CashAdvanceDefaultingChargeCodeCollection(config);
			collection.Load();
			AssertContainsExactElementsInAnyOrder(chargeCodePivot, collection);
		}
	}
}
