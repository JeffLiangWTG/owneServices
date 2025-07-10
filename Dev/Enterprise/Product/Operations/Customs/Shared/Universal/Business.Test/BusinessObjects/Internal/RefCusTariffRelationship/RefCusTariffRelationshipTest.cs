using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Universal.Internal;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.Universal.Testing
{
	[TestedType(typeof(RefCusTariffRelationship))]
	class RefCusTariffRelationshipTest : EnterpriseBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			var rel = Factory.New<RefCusTariffRelationship>();
			rel.ZZH_ZZ1_Tariff = cusTariff.PK;
			rel.ZZH_ZZI_TariffType = tariffType.PK;
			return rel;
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var rel = factory.New<RefCusTariffRelationship>();
			rel.ZZH_ZZ1_Tariff = cusTariff.PK;
			rel.ZZH_ZZI_TariffType = tariffType.PK;
			return rel;
		}

		protected override void SetUp()
		{
			base.SetUp();
			helper = new UniversalReferenceTestDataHelper(Factory);
			tariffType = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.Eritrea, "1P1");
			cusTariff = UniversalReferenceTestDataHelper.CreateInternalRefCusTariff(Factory, Core.Constants.CountryCodes.Eritrea, tariffType.PK, "DUMMYTRF", new ZDateTime(2010, 12, 10), new ZDateTime(2079, 06, 06), "dummy Description 0");
			Factory.Save();
		}

		RefCusTariff cusTariff;
		RefCusTariffType tariffType;
		UniversalReferenceTestDataHelper helper;
	}
}
