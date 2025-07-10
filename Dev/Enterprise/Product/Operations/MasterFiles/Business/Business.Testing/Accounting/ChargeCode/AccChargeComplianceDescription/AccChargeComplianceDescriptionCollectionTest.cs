using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(AccChargeComplianceDescriptionCollection))]
	sealed class AccChargeComplianceDescriptionCollectionTest : ActiveBusinessObjectCollectionTestCase<AccChargeComplianceDescriptionCollection>
	{
		protected override AccChargeComplianceDescriptionCollection GetCollectionToTest()
		{
			return new AccChargeComplianceDescriptionCollection(Factory.New<AccChargeCode>());
		}

		public void TestIsUsedBy()
		{
			AssertType<AccChargeComplianceDescriptionCollection>(Factory.New<AccChargeCode>().ChargeComplianceDescriptions);
		}

		public void TestCollectionContainsCorrectObjects()
		{
			var charge1 = Factory.NewWithValidTestData<AccChargeCode>();
			var charge2 = Factory.NewWithValidTestData<AccChargeCode>();
			var charge1_Collection = new AccChargeComplianceDescriptionCollection(charge1);
			AssertEquals(0, charge1_Collection.Count);

			var accChargeComplianceDescription1 = Factory.New<AccChargeComplianceDescription>();
			var accChargeComplianceDescription2 = Factory.New<AccChargeComplianceDescription>();
			var accChargeComplianceDescription3 = Factory.New<AccChargeComplianceDescription>();
			accChargeComplianceDescription1.ADE_AC = charge1.PK;
			accChargeComplianceDescription2.ADE_AC = charge2.PK;
			AssertEquals(1, charge1_Collection.Count);
			AssertCollectionContains(accChargeComplianceDescription1, charge1_Collection);

			accChargeComplianceDescription3.ADE_AC = charge1.PK;
			AssertEquals(2, charge1_Collection.Count);
			AssertCollectionContains(accChargeComplianceDescription1, charge1_Collection);
			AssertCollectionContains(accChargeComplianceDescription3, charge1_Collection);

			var charge2_Collection = new AccChargeComplianceDescriptionCollection(charge2);
			AssertEquals(1, charge2_Collection.Count);
			AssertCollectionContains(accChargeComplianceDescription2, charge2_Collection);
		}
	}
}
