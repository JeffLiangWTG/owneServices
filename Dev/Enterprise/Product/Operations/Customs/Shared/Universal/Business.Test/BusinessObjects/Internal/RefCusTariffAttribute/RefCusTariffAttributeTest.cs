using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Universal.Internal;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.Universal.Testing
{
	[TestedType(typeof(RefCusTariffAttribute))]
	internal class RefCusTariffAttributeTest : EnterpriseBusinessObjectTestCase
	{
		public override void TestCallsBaseSetDefaultValues()
		{
			Assert(true);
		}

		protected override BusinessObject GetNewBusinessObject() => UniversalReferenceTestDataHelper.CreateInternalRefCusTariffAttribute(Factory, "Name", "Value", false, cusTariff.PK);
		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory) => UniversalReferenceTestDataHelper.CreateInternalRefCusTariffAttribute(factory, "NAME2", "VALUE2", false, cusTariff.PK);
		protected override bool CanPersistedObjectBeDeleted => false;
		protected override BusinessObject GetBusinessObjectForFetchForLoad()
		{
			return GetNewBusinessObject();
		}

		protected override void SetUp()
		{
			base.SetUp();
			helper = new UniversalReferenceTestDataHelper(Factory);
			s1p1TariffType = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.SouthAfrica, "1P1");
			cusTariff = UniversalReferenceTestDataHelper.CreateInternalRefCusTariff(Factory, Core.Constants.CountryCodes.SouthAfrica, s1p1TariffType.PK, "DUMMYTRF", new ZDateTime(2010, 12, 10), new ZDateTime(2079, 06, 06), "dummy Description 0");
			Factory.Save();
		}

		RefCusTariffType s1p1TariffType;
		RefCusTariff cusTariff;
		UniversalReferenceTestDataHelper helper;
	}
}
