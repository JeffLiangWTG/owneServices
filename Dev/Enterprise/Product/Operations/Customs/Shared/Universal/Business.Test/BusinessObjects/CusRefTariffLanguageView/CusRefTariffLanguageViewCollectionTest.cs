using System.ComponentModel;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Customs.Universal.Testing
{
	[TestedType(typeof(CusRefTariffLanguageViewCollection))]
	public class CusRefTariffLanguageViewCollectionTest : ActiveBusinessObjectCollectionTestCase<CusRefTariffLanguageViewCollection>
	{
		public void TestAllowNew()
		{
			var collection = GetCollectionToTest();
			AssertEquals(true, ((IBindingList)collection).AllowNew);
		}

		public void TestSetDefaultsForNewElement()
		{
			var cusTariff = CreateTariffView();
			var collection = new CusRefTariffLanguageViewCollection(cusTariff);
			var language = collection.AddNew();
			AssertEquals(cusTariff.PK, language.ZX7_ZZ1_Tariff);
		}

		protected override CusRefTariffLanguageViewCollection GetCollectionToTest()
		{
			var cusTariff = CreateTariffView();
			return new CusRefTariffLanguageViewCollection(cusTariff);
		}

		TariffView CreateTariffView()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var hsnTariffType = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.Taiwan, Universal.Constants.TariffTypes.HarmonizedSystem);
			Factory.Save();
			return helper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.Taiwan, hsnTariffType.PK, "87031000002", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
		}
	}
}
