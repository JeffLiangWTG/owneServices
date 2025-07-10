using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;
using Enterprise.Customs.ZA.Business;
using Enterprise.ZArchitecture.Business;
using NUnit.Framework;

namespace Enterprise.Customs.ZA.Module.Testing.RefCusTariff
{
	[TestedType(typeof(RefCusTariffFilterStripBusinessObject))]
	public class RefCusTariffFilterStripBusinessObjectTests : Universal.Module.Testing.RefCusTariffFilterStripBusinessObjectTests
	{
		public void TestTariffCodePlusCheckDigitQuery()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.SouthAfrica);
			var tariffType = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.SouthAfrica, "3P1");
			Factory.Save();
			var tariff1 = helper.CreateTariff(Core.Constants.CountryCodes.SouthAfrica, tariffType.PK, "123456789", ZDateTime.Today.AddYears(-1), ZDateTime.Today.AddYears(1));
			helper.CreateTariffAttribute(UniversalReferenceConstants.TariffAttributes.CheckDigit, "01", tariff1);

			var tariff2 = helper.CreateTariff(Core.Constants.CountryCodes.SouthAfrica, tariffType.PK, "123456789", ZDateTime.Today.AddYears(-2), ZDateTime.Today.AddYears(2));
			helper.CreateTariffAttribute(UniversalReferenceConstants.TariffAttributes.CheckDigit, "23", tariff2);
			Factory.Save();

			var checkDigitAttrName = helper.CreateNewOrGetExistingRefCusTariffAttributeName(Factory, "CheckDigit", "Check Digit", "Check Digit", Core.Constants.CountryCodes.SouthAfrica, tariffType.ZZI_TariffType);
			Factory.Save();

			var collection = ChildTariffViewCollection.GetNewCollection(Factory, Core.Constants.CountryCodes.SouthAfrica, "3P1", ZDateTime.Today, new[] { new KeyValuePair<ZString, ZString>() });
			var bizObj = new RefCusTariffFilterStripBusinessObject(collection);

			var tariffFilter = (ModuleTextFilter)bizObj.ModuleFilters[RefCusTariffFilterConstants.Tariff];
			tariffFilter.Property = "123456789";
			tariffFilter.IsActive = true;

			var checkDigitFilter = (ModuleTextFilter)bizObj.ModuleFilters[RefCusTariffFilterConstants.CheckDigit];
			checkDigitFilter.Property = "01";
			checkDigitFilter.IsActive = true;

			var filter = bizObj.Filter;
			AssertEquals(true, tariff1.MatchesFilter(filter));
			AssertEquals(false, tariff2.MatchesFilter(filter));

			checkDigitFilter.Property = "23";
			filter = bizObj.Filter;
			AssertEquals(false, tariff1.MatchesFilter(filter));
			AssertEquals(true, tariff2.MatchesFilter(filter));

			checkDigitFilter.Property = "";
			filter = bizObj.Filter;
			AssertEquals(true, tariff1.MatchesFilter(filter));
			AssertEquals(true, tariff2.MatchesFilter(filter));
		}

		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject()
		{
			return new RefCusTariffFilterStripBusinessObject();
		}
	}
}
