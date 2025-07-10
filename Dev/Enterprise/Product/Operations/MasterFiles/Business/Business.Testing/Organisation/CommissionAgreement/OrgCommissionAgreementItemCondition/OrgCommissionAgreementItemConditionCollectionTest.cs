using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(OrgCommissionAgreementItemConditionCollection))]
	sealed class OrgCommissionAgreementItemConditionCollectionTest : ActiveBusinessObjectCollectionTestCase<OrgCommissionAgreementItemConditionCollection>
	{
		#region Default Values

		public void TestDefaultValues()
		{
			var item = Factory.NewWithValidTestData<OrgCommissionAgreementItem>();

			var collection = new OrgCommissionAgreementItemConditionCollection(item);
			var newCondition = collection.AddNew();
			AssertEquals(item.PK, newCondition.CIC_CAI);
		}

		#endregion

	}
}
