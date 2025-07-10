using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.Universal.Testing
{
	[TestedType(typeof(TariffRelationshipView))]
	class TariffRelationshipViewTest : EnterpriseBusinessObjectTestCase
	{
		public void TestZZH_ZZI_TariffTypeDesc()
		{
			var tariffType = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.Eritrea, "$@#");
			tariffType.ZZI_Description = "BOB THE BUILDER";
			Factory.Save();
			var relatedTariff = helper.CreateTariffRelationship(cusTariff.PK, tariffType.PK, cusTariff.ZZ1_TariffCode);
			AssertEquals("relatedTariff.ZZH_ZZI_TariffTypeDesc", "BOB THE BUILDER", relatedTariff.ZZH_ZZI_TariffTypeDesc);
		}

		public void TestDefaultDerivedPropertiesForValidTariff()
		{
			var s12BTariffType = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.SouthAfrica, "12B");
			Factory.Save();
			AssertEquals(cusTariff.ZZ1_TariffCode, relationship.ZZH_RelatedTariffCode);
			AssertEquals(cusTariff.ZZ1_ZZI_TariffType, relationship.ZZH_ZZI_RelatedTariffType);
			AssertEquals(cusTariff.ZZ1_ZZZ_NKDataGrouping, relationship.ZZH_ZZZ_RelatedTariffDataGrouping);
			var newTariff = helper.CreateTariff(Core.Constants.CountryCodes.SouthAfrica, s12BTariffType.PK, "DUMMYTRF2", new ZDateTime(2010, 12, 10), new ZDateTime(2079, 06, 06), "dummy Description 0");
			relationship.ZZH_ZZ1_LinkedTariffOrNationalCode = newTariff.PK;
			AssertEquals(newTariff.ZZ1_TariffCode, relationship.ZZH_RelatedTariffCode);
			AssertEquals(newTariff.ZZ1_ZZI_TariffType, relationship.ZZH_ZZI_RelatedTariffType);
			AssertEquals(newTariff.ZZ1_ZZZ_NKDataGrouping, relationship.ZZH_ZZZ_RelatedTariffDataGrouping);
		}

		public void TestDefaultDerivedPropertiesForInvalidTariff()
		{
			relationship.ZZH_ZZ1_LinkedTariffOrNationalCode = ZGuid.NewZGuid();
			AssertEquals(ZString.Empty, relationship.ZZH_RelatedTariffCode);
			AssertEquals(ZGuid.Empty, relationship.ZZH_ZZI_RelatedTariffType);
			AssertEquals(ZString.Empty, relationship.ZZH_ZZZ_RelatedTariffDataGrouping);
		}

		public void TestRelatedTariffType()
		{
			var s12BTariffType = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.SouthAfrica, "12B");
			Factory.Save();
			AssertEquals(s1p1TariffType, relationship.RelatedTariffType);
			var newTariff = helper.CreateTariff(Core.Constants.CountryCodes.Eritrea, s12BTariffType.PK, "DUMMYTRF", new ZDateTime(2010, 12, 10), new ZDateTime(2079, 06, 06), "dummy Description 0");
			relationship.ZZH_ZZ1_LinkedTariffOrNationalCode = newTariff.PK;
			AssertEquals(s12BTariffType, relationship.RelatedTariffType);
		}

		public void TestRelatedTariffFromTypeCode()
		{
			AssertEquals(s1p1TariffType.ZZI_TariffType, relationship.RelatedTariffFromTypeCode);
		}

		public void TestRelatedTariffFromCode()
		{
			AssertEquals(cusTariff.ZZ1_TariffCode, relationship.RelatedTariffFromCode);
		}

		public override void TestSaveAndDeleteBusinessObject()
		{
			Assert("SaveAndDelete not supported", true);
		}

		public override void TestCallsBaseSetDefaultValues()
		{
			Assert(true);
		}

		public void TestITariffEffectiveDatesRelatedBusinessObject()
		{
			var iTariff = relationship as ITariffEffectiveDatesRelatedBusinessObject;

			CombineAssertions(() =>
			{
				AssertEquals("StartDate", new ZDateTime(2010, 12, 10), iTariff.StartDate);
				AssertEquals("EndDate", new ZDateTime(2079, 06, 06), iTariff.EndDate);
				AssertEquals("DataGrouping", "ER", iTariff.DataGrouping);
			});
		}

		protected override BusinessObject GetNewBusinessObject() => relationship;
		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory) => GetNewBusinessObject();
		protected override BusinessObject GetBusinessObjectForFetchForLoad() => GetNewBusinessObject();
		protected override void SetUp()
		{
			base.SetUp();
			helper = new UniversalReferenceTestDataHelper(Factory);
			s1p1TariffType = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.SouthAfrica, "1P1");
			Factory.Save();
			cusTariff = helper.CreateTariff(Core.Constants.CountryCodes.Eritrea, s1p1TariffType.PK, "DUMMYTRF", new ZDateTime(2010, 12, 10), new ZDateTime(2079, 06, 06), "dummy Description 0");
			relationship = helper.CreateTariffRelationship(cusTariff.PK, cusTariff.ZZ1_ZZI_TariffType, cusTariff.ZZ1_TariffCode);
			Factory.Save();
		}

		UniversalReferenceTestDataHelper helper;
		RefCusTariffType s1p1TariffType;
		TariffView cusTariff;
		TariffRelationshipView relationship;
	}
}
