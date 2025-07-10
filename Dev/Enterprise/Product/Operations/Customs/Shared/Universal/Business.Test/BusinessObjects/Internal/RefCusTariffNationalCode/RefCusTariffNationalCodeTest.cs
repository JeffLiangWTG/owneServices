using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Universal.Internal;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.Universal.Testing
{
	[TestedType(typeof(RefCusTariffNationalCode))]
	public class RefCusTariffNationalCodeTest : EnterpriseBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			return GetNewBusinessObjectForDeleteTest(Factory);
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var result = factory.New<RefCusTariffNationalCode>();
			result.ZZW_ZZ1_Tariff = cusTariff.PK;
			result.ZZW_StartDate = ZDate.Today.AddMonths(-1);
			result.ZZW_EndDate = ZDate.Today.AddMonths(1);
			result.ZZW_ZZZ_NKDataGrouping = Core.Constants.CountryCodes.Eritrea;
			result.ZZW_NationalCode = Core.Constants.CountryCodes.Eritrea;
			return result;
		}

		protected override bool CanPersistedObjectBeDeleted => false;
		protected override void SetUp()
		{
			base.SetUp();
			helper = new UniversalReferenceTestDataHelper(Factory);
			s1p1TariffType = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.Eritrea, "1P1");
			Factory.Save();
			cusTariff = helper.CreateTariff(Core.Constants.CountryCodes.Eritrea, s1p1TariffType.PK, "DUMMYTRF", new ZDateTime(2010, 12, 10), new ZDateTime(2079, 06, 06), "dummy Description 0");
			Factory.Save();
		}

		RefCusTariffType s1p1TariffType;
		TariffView cusTariff;
		UniversalReferenceTestDataHelper helper;
	}
}
