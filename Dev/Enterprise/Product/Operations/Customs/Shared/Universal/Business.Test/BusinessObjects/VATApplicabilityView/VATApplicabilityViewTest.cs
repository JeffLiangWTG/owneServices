using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.Universal.Testing
{
	[TestedType(typeof(VATApplicabilityView))]
	class VATApplicabilityViewTest : EnterpriseBusinessObjectTestCase
	{
		public void TestITariffEffectiveDatesRelatedBusinessObjectMembers()
		{
			ITariffEffectiveDatesRelatedBusinessObject bizObj = Helper.CreateNewOrGetExistingVATApplicability(CusTariff, Core.Constants.CountryCodes.SouthAfrica, "VAT", startDate: new ZDateTime(2010, 12, 10), endDate: new ZDateTime(2079, 06, 06));
			AssertEquals("DataGrouping", Core.Constants.CountryCodes.SouthAfrica, bizObj.DataGrouping);
			AssertEquals("StartDate", new ZDateTime(2010, 12, 10), bizObj.StartDate);
			AssertEquals("EndDate", new ZDateTime(2079, 06, 06), bizObj.EndDate);
		}

		public override void TestCallsBaseSetDefaultValues()
		{
			Assert(true);
		}

		protected override BusinessObject GetNewBusinessObject() => VATApplicability;
		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory) => VATApplicability;
		protected override BusinessObject GetBusinessObjectForFetchForLoad() => VATApplicability;
		protected override bool CanPersistedObjectBeDeleted => false;
		protected override void SetUp()
		{
			base.SetUp();
			Helper.CreateTaxOrFee("VAT", 0.01m, Core.Constants.CountryCodes.SouthAfrica);
			s1p1TariffType = Helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.SouthAfrica, "1P1");
			Factory.Save();
		}

		RefCusTariffType s1p1TariffType;
		VATApplicabilityView VATApplicability => vatApplicability ?? (vatApplicability = Helper.CreateNewOrGetExistingVATApplicability(CusTariff, Core.Constants.CountryCodes.SouthAfrica, "VAT"));
		VATApplicabilityView vatApplicability;
		TariffView CusTariff => cusTariff ?? (cusTariff = Helper.CreateTariff(Core.Constants.CountryCodes.SouthAfrica, s1p1TariffType.PK, "DUMMYTRF", new ZDateTime(2010, 12, 10), new ZDateTime(2079, 06, 06), "dummy Description 0"));
		TariffView cusTariff;
		UniversalReferenceTestDataHelper Helper => helper ?? (helper = new UniversalReferenceTestDataHelper(Factory));
		UniversalReferenceTestDataHelper helper;
	}
}
