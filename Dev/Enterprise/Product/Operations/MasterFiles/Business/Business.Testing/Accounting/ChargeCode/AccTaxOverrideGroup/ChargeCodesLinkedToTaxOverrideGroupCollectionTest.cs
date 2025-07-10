using System.ComponentModel;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(ChargeCodesLinkedToTaxOverrideGroupCollection))]
	sealed class ChargeCodesLinkedToTaxOverrideGroupCollectionTest : ActiveBusinessObjectCollectionTestCase<ChargeCodesLinkedToTaxOverrideGroupCollection>
	{
		protected override ChargeCodesLinkedToTaxOverrideGroupCollection GetCollectionToTest()
		{
			return new ChargeCodesLinkedToTaxOverrideGroupCollection(Factory.New<AccTaxOverrideGroup>());
		}

		public void TestCollectionContainsCorrectObjects()
		{
			AccTaxOverrideGroup overrideGroup = Factory.New<AccTaxOverrideGroup>();
			ChargeCodesLinkedToTaxOverrideGroupCollection collection = new ChargeCodesLinkedToTaxOverrideGroupCollection(overrideGroup);
			AssertEquals(0, collection.Count);

			Factory.New<AccChargeCode>().AC_AX_TaxOverrideGroup = overrideGroup.PK;
			Factory.New<AccChargeCode>().AC_AX_TaxOverrideGroup = Factory.New<AccTaxOverrideGroup>().PK;
			Factory.New<AccChargeCode>().AC_AX_TaxOverrideGroup = overrideGroup.PK;

			AssertEquals(2, collection.Count);
		}

		public void TestCollectionShouldOnlyContainChargeCodesForLoginCompany()
		{
			AccTaxOverrideGroup overrideGroup = Factory.New<AccTaxOverrideGroup>();
			ChargeCodesLinkedToTaxOverrideGroupCollection collection = new ChargeCodesLinkedToTaxOverrideGroupCollection(overrideGroup);

			GlbCompany aUCompany1 = Factory.Load<GlbCompany>(GlbCompany.CurrentCompany.PK);
			aUCompany1.GC_RN_NKCountryCode = Enterprise.Core.Constants.CountryCodes.Australia;
			GlbCompany aUCompany2 = Factory.NewWithValidTestData<GlbCompany>();
			aUCompany2.GC_RN_NKCountryCode = Enterprise.Core.Constants.CountryCodes.Australia;

			AccChargeCode chargeCode1 = Factory.NewWithValidTestData<AccChargeCode>();
			chargeCode1.AC_GC = aUCompany1.PK;
			chargeCode1.AC_AX_TaxOverrideGroup = overrideGroup.PK;

			AccChargeCode chargeCode2 = Factory.NewWithValidTestData<AccChargeCode>();
			chargeCode2.AC_GC = aUCompany2.PK;
			chargeCode2.AC_AX_TaxOverrideGroup = overrideGroup.PK;

			collection.RefreshBinding();

			AssertEquals("Should only have one charge code in the collection", 1, collection.Count);
			AssertEquals("Charge Code should be related to current company", chargeCode1.PK, collection[0].PK);
		}

		public void TestAllowNew()
		{
			AssertEquals("AllowNew default value.", false, ((IBindingList)Collection).AllowNew);

			Collection.SetAllowNew(true);
			AssertEquals("AllowNew set value.", true, ((IBindingList)Collection).AllowNew);
		}
	}
}
