using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Universal.Internal;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.Universal.Testing
{
	[TestedType(typeof(RefCusTariff))]
	internal class RefCusTariffTest : EnterpriseBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject() => UniversalReferenceTestDataHelper.CreateInternalRefCusTariff(Factory, Core.Constants.CountryCodes.SouthAfrica, s1p1TariffType.PK, "DUMMYTRF", new ZDateTime(2010, 12, 10), new ZDateTime(2079, 06, 06), "dummy Description 0");
		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory) => UniversalReferenceTestDataHelper.CreateInternalRefCusTariff(factory, Core.Constants.CountryCodes.SouthAfrica, s1p1TariffType.PK, "DUMMYTRF", new ZDateTime(2010, 12, 10), new ZDateTime(2079, 06, 06), "dummy Description 0");
		protected override bool CanPersistedObjectBeDeleted => false;
		protected override void SetUp()
		{
			base.SetUp();
			helper = new UniversalReferenceTestDataHelper(Factory);
			s1p1TariffType = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.SouthAfrica, "1P1", nomenclatureGroupType: "ZA");
			Factory.Save();
		}

		RefCusTariffType s1p1TariffType;
		UniversalReferenceTestDataHelper helper;
	}
}
