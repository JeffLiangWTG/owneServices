using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Customs.Universal.Testing
{
	[TestedType(typeof(TariffDataObjectCollection))]
	class TariffDataObjectCollectionTest : NonPersistentBusinessObjectCollectionTestCase<TariffDataObjectCollection>
	{
		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new TariffDataObject(Helper, CusTariff);
		}

		protected override TariffDataObjectCollection GetCollectionToTest()
		{
			return new TariffDataObjectCollection();
		}

		protected override void SetUp()
		{
			base.SetUp();
			tariffTypeTST = DataHelper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.Eritrea, "TST");
			Factory.Save();
		}

		RefCusTariffType tariffTypeTST;
		TariffView CusTariff => cusTariff ?? (cusTariff = DataHelper.CreateTariff(Core.Constants.CountryCodes.Eritrea, tariffTypeTST.PK, "1020304050", ZDateTime.BrettsBirthday, ZDateTime.Today.AddYears(1), "DESC TARIFF", compositeKey: "10.20.30.40.50"));
		TariffView cusTariff;
		TariffSearchHelper Helper => helper ?? (helper = new TariffSearchHelper(Core.Constants.CountryCodes.Eritrea, "", null, null));
		TariffSearchHelper helper;
		UniversalReferenceTestDataHelper DataHelper => dataHelper ?? (dataHelper = new UniversalReferenceTestDataHelper(Factory));
		UniversalReferenceTestDataHelper dataHelper;
	}
}
