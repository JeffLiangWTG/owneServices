using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Customs.Universal.Testing
{
	[TestedType(typeof(RefCusConditionCollection))]
	public class RefCusConditionCollectionTests : ActiveBusinessObjectCollectionTestCase<RefCusConditionCollection>
	{
		protected override RefCusConditionCollection GetCollectionToTest()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var testTariffType = helper.CreateNewOrGetExistingTariffType("TSG", "TTT", nomenclatureGroupType: "TNG");
			Factory.Save();
			var testTariff = helper.CreateTariff("TSG", testTariffType.PK, "TariffCOde", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime, compositeKey: "10.10..10");
			return new RefCusConditionCollection(testTariff);
		}
	}
}
