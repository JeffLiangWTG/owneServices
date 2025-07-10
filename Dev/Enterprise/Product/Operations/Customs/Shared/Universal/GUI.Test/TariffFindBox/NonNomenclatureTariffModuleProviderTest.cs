using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Universal.Testing;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI.Internal;
using NUnit.Framework;

namespace Enterprise.Customs.Universal.GUI.Testing
{
	class NonNomenclatureTariffModuleProviderTest : TestCaseWithFactory
	{
		[RequiresSTA]
		public void TestShow()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var dataGroupingSKK = helper.CreateNewOrGetExistingDataGrouping("SKK");
			var dataGroupingER = helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Eritrea);
			Factory.Save();
			dataGroupingER.ZZZ_ZZZ_Grouping = dataGroupingSKK.PK;
			var tariffType = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.Eritrea, "TST");
			Factory.Save();
			var tariff1 = helper.CreateTariff(Core.Constants.CountryCodes.Eritrea, tariffType.PK, "1020304050", ZDateTime.Today.AddYears(-1), ZDateTime.Today.AddYears(1));
			var tariff2 = helper.CreateTariff("SKK", tariffType.PK, "2030405060", ZDateTime.Today.AddYears(-1), ZDateTime.Today.AddYears(1));
			var tariff3 = helper.CreateTariff(Core.Constants.CountryCodes.Eritrea, tariffType.PK, "2030405060", ZDateTime.Today.AddYears(-1), ZDateTime.Today.AddYears(1));
			Factory.Save();
			using (var findbox = new TariffFindBox())
			{
				using (var form = (EmbeddedModulePopup)NonNomenclatureTariffModuleProvider.Show(findbox, Core.Constants.CountryCodes.Eritrea, "TST"))
				{
					var filter = form.Module_ForTest.FilterBusinessObject.Filter;
					AssertEquals(true, tariff1.MatchesFilter(filter));
					AssertEquals(false, tariff2.MatchesFilter(filter));
					AssertEquals(true, tariff3.MatchesFilter(filter));
					AssertEquals(FilterVisibility.AlwaysAppliedAndHidden, form.Module_ForTest.FilterBusinessObject.ModuleFilters[Constants.RefCusTariffFilters.CountryOrGrouping].Visibility);
					var tariffTypeFilter = (DataGroupingRelatedFilter)form.Module_ForTest.FilterBusinessObject.ModuleFilters[Constants.RefCusTariffFilters.TariffType];
					AssertEquals(FilterVisibility.AlwaysVisible, tariffTypeFilter.Visibility);
					AssertEquals("tariffTypeFilter.Property1", Core.Constants.CountryCodes.Eritrea, tariffTypeFilter.Property1);
					AssertEquals("tariffTypeFilter.Property2", "TST", tariffTypeFilter.Property2);
					var effectiveDateFilter = (ModuleSingleDateFilter)form.Module_ForTest.FilterBusinessObject.ModuleFilters[Constants.RefCusTariffFilters.EffectiveDate];
					AssertEquals("effectiveDateFilter.Property1", ZDateTime.Today, effectiveDateFilter.Property1);
					var descriptionFilter = form.Module_ForTest.FilterBusinessObject[Constants.RefCusTariffFilters.DefaultLanguageDescription];
					AssertEquals("Show Description Filter as Default", FilterVisibility.Visible, descriptionFilter.Visibility);
				}

				findbox.CodeBox.Text = "10203";
				findbox.GetEffectiveDate = () => ZDateTime.Today.AddDays(1);
				findbox.ShowDescriptionFilterOnNonNomenclatureTariffModule = true;
				using (var form = (EmbeddedModulePopup)NonNomenclatureTariffModuleProvider.Show(findbox, Core.Constants.CountryCodes.Eritrea, "TST"))
				{
					form.Module_ForTest.FilterBusinessObject.ModuleFilters[Constants.RefCusTariffFilters.TariffCode].IsActive = true;
					var filter = form.Module_ForTest.FilterBusinessObject.Filter;
					AssertEquals(true, tariff1.MatchesFilter(filter));
					AssertEquals(false, tariff2.MatchesFilter(filter));
					AssertEquals(false, tariff3.MatchesFilter(filter));
					var effectiveDateFilter = (ModuleSingleDateFilter)form.Module_ForTest.FilterBusinessObject.ModuleFilters[Constants.RefCusTariffFilters.EffectiveDate];
					AssertEquals("effectiveDateFilter.Property1", ZDateTime.Today.AddDays(1), effectiveDateFilter.Property1);
					var descriptionFilter = form.Module_ForTest.FilterBusinessObject[Constants.RefCusTariffFilters.DefaultLanguageDescription];
					AssertEquals("Show Description Filter as Default", FilterVisibility.AlwaysVisible, descriptionFilter.Visibility);
				}
			}
		}
	}
}
