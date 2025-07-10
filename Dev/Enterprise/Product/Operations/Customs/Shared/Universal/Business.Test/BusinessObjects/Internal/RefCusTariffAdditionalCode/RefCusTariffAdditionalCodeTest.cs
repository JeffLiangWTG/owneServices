using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Universal.Internal;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.Universal.Testing
{
	[TestedType(typeof(RefCusTariffAdditionalCode))]
	class RefCusTariffAdditionalCodeTest : EnterpriseBusinessObjectTestCase
	{
		public void TestShouldNotProvideDateRange()
		{
			var tariffAdditionalCode = GetNewBusinessObject();
			Assert(tariffAdditionalCode is ITariffDataGroupingRelatedBusinessObject);
			Assert(!(tariffAdditionalCode is ITariffEffectiveDatesRelatedBusinessObject));
		}

		protected override BusinessObject GetNewBusinessObject() => GetNewBusinessObjectForDeleteTest(Factory);
		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory) => UniversalReferenceTestDataHelper.CreateInternalRefCusTariffAdditionalCode(factory, false, cusTariff.PK, Core.Constants.CountryCodes.Eritrea, "T1T", "C2T");
		protected override bool CanPersistedObjectBeDeleted => false;
		protected override BusinessObject GetBusinessObjectForFetchForLoad()
		{
			return GetNewBusinessObject();
		}

		protected override void SetUp()
		{
			base.SetUp();
			helper = new UniversalReferenceTestDataHelper(Factory);
			tariffType = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.Eritrea, "T1T");
			Factory.Save();
			cusTariff = UniversalReferenceTestDataHelper.CreateInternalRefCusTariff(Factory, Core.Constants.CountryCodes.Eritrea, tariffType.PK, "DUMMYTRF", new ZDateTime(2010, 12, 10), new ZDateTime(2079, 06, 06), "dummy Description 0");
			Factory.Save();
		}

		RefCusTariffType tariffType;
		RefCusTariff cusTariff;
		UniversalReferenceTestDataHelper helper;
	}
}
