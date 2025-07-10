using CargoWise.Types;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.Customs.Universal.GUI.Testing
{
	[TestedType(typeof(RefCusTariffFilterStripBusinessObject))]
	class RefCusTariffFilterStripBusinessObjectTests : ZArchitecture.Modules.Testing.FilterStripBusinessObjectTestCase
	{
		public void TestTariffCodeFilter()
		{
			var helper = new TariffSearchHelper(Enterprise.Core.Constants.CountryCodes.SouthAfrica, "", null, null);
			var filterBO = new RefCusTariffFilterStripBusinessObject(helper);
			var filterStrip = filterBO.FilterStrips.AddNew();
			filterStrip.FilterDescription = Constants.RefCusTariffFilters.TariffCode;
			var tariffFilter = (ModuleTextFilter)filterStrip.CurrentModuleFilter;
			tariffFilter.IsActive = true;
			AssertEquals(2, tariffFilter.ComparisonOperator_List.Count);
			var list = tariffFilter.ComparisonOperator_List;
			Assert(ModuleFilterWithListAndComparisonOperators<ZString>.ComparisonConstants.Exact, list.ContainsCode(ModuleFilterWithListAndComparisonOperators<ZString>.ComparisonConstants.Exact));
			Assert(ModuleFilterWithListAndComparisonOperators<ZString>.ComparisonConstants.StartsWith, list.ContainsCode(ModuleFilterWithListAndComparisonOperators<ZString>.ComparisonConstants.StartsWith));
			filterStrip = filterBO.FilterStrips.AddNew();
			filterStrip.FilterDescription = Constants.RefCusTariffFilters.TariffCode;
			var filter2 = (ModuleTextFilter)filterStrip.CurrentModuleFilter;
			filter2.IsActive = true;
			tariffFilter.Property = "1";
			AssertHasError("Tariff Code must be at least 2 characters long.", tariffFilter.PropertyInfo, "Tariff Code must be at least 2 characters long.");
			tariffFilter.Property = "1010";
			AssertHasError("Multiple Tariff Code Filter.", tariffFilter.PropertyInfo, "Only one Tariff Code filter is allowed.");
			filter2.IsActive = false;
			tariffFilter.Property = "";
			AssertHasError("Please enter a Tariff Code", tariffFilter.PropertyInfo, "Please enter a Tariff Code.");
			tariffFilter.Property = "1010";
			AssertNoErrors(tariffFilter.PropertyInfo);
			filterStrip = filterBO.FilterStrips.AddNew();
			filterStrip.FilterDescription = Constants.RefCusTariffFilters.DefaultLanguageDescription;
			var descrFilter2 = (ModuleTextFilter)filterStrip.CurrentModuleFilter;
			descrFilter2.IsActive = true;
			descrFilter2.Property = "ADDD";
			tariffFilter.Property = "";
			AssertNoErrors(tariffFilter.PropertyInfo);
		}

		public void TestDescriptionFilter()
		{
			var helper = new TariffSearchHelper(Enterprise.Core.Constants.CountryCodes.SouthAfrica, "", null, null);
			var filterBO = new RefCusTariffFilterStripBusinessObject(helper);
			var filterStrip = filterBO.FilterStrips.AddNew();
			filterStrip.FilterDescription = Constants.RefCusTariffFilters.DefaultLanguageDescription;
			var descriptionFilter = (ModuleTextFilter)filterStrip.CurrentModuleFilter;
			descriptionFilter.IsActive = true;
			AssertEquals(3, descriptionFilter.ComparisonOperator_List.Count);
			var list = descriptionFilter.ComparisonOperator_List;
			Assert(ModuleFilterWithListAndComparisonOperators<ZString>.ComparisonConstants.Exact, list.ContainsCode(ModuleFilterWithListAndComparisonOperators<ZString>.ComparisonConstants.Exact));
			Assert(ModuleFilterWithListAndComparisonOperators<ZString>.ComparisonConstants.StartsWith, list.ContainsCode(ModuleFilterWithListAndComparisonOperators<ZString>.ComparisonConstants.StartsWith));
			Assert(ModuleFilterWithListAndComparisonOperators<ZString>.ComparisonConstants.Contains, list.ContainsCode(ModuleFilterWithListAndComparisonOperators<ZString>.ComparisonConstants.Contains));
			filterStrip = filterBO.FilterStrips.AddNew();
			filterStrip.FilterDescription = Constants.RefCusTariffFilters.DefaultLanguageDescription;
			var filter2 = (ModuleTextFilter)filterStrip.CurrentModuleFilter;
			filter2.IsActive = true;
			descriptionFilter.Property = "1";
			AssertHasError("Description must be at least 3 characters long.", descriptionFilter.PropertyInfo, "Description must be at least 3 characters long.");
			descriptionFilter.Property = "OTH";
			AssertHasError("Multiple Description Filter.", descriptionFilter.PropertyInfo, "Only one Description filter is allowed.");
			filter2.IsActive = false;
			descriptionFilter.Property = "";
			AssertHasError("Please enter a Description", descriptionFilter.PropertyInfo, "Please enter a Description (Default Language).");
			descriptionFilter.Property = "OTH";
			AssertHasError(descriptionFilter.PropertyInfo, "Searching for 'OTH' in Description should only be done when Tariff Code is specified.");
			descriptionFilter.Property = "OTHe";
			AssertHasError(descriptionFilter.PropertyInfo, "Searching for 'OTHe' in Description should only be done when Tariff Code is specified.");
			descriptionFilter.Property = "OTHeR";
			AssertHasError(descriptionFilter.PropertyInfo, "Searching for 'OTHeR' in Description should only be done when Tariff Code is specified.");
			descriptionFilter.Property = "OTHeRs";
			AssertNoErrors(descriptionFilter.PropertyInfo);
			filterStrip = filterBO.FilterStrips.AddNew();
			filterStrip.FilterDescription = Constants.RefCusTariffFilters.TariffCode;
			var tariffFilter2 = (ModuleTextFilter)filterStrip.CurrentModuleFilter;
			tariffFilter2.IsActive = true;
			tariffFilter2.Property = "102030";
			descriptionFilter.Property = "";
			AssertNoErrors(descriptionFilter.PropertyInfo);
			descriptionFilter.Property = "OTH";
			AssertNoErrors(descriptionFilter.PropertyInfo);
			descriptionFilter.Property = "OTHeR";
			AssertNoErrors(descriptionFilter.PropertyInfo);
			tariffFilter2.Property = "";
			descriptionFilter.Property = "OTH";
			AssertHasError(descriptionFilter.PropertyInfo, "Searching for 'OTH' in Description should only be done when Tariff Code is specified.");
		}

		public void TestEffectiveDateFilter()
		{
			var helper = new TariffSearchHelper(Enterprise.Core.Constants.CountryCodes.SouthAfrica, "", null, null);
			var filterBO = new RefCusTariffFilterStripBusinessObject(helper);
			var filterStrip = filterBO.FilterStrips.AddNew();
			filterStrip.FilterDescription = Constants.RefCusTariffFilters.EffectiveDate;
			var effectiveDateFilter = (ModuleSingleDateFilter)filterStrip.CurrentModuleFilter;
			effectiveDateFilter.IsActive = true;
			filterStrip = filterBO.FilterStrips.AddNew();
			filterStrip.FilterDescription = Constants.RefCusTariffFilters.EffectiveDate;
			var filter2 = (ModuleSingleDateFilter)filterStrip.CurrentModuleFilter;
			filter2.IsActive = true;
			effectiveDateFilter.Property1 = ZDateTime.Today;
			AssertHasError("Multiple Effective Date Filter.", effectiveDateFilter.Property1Info, "Only one Effective Date filter is allowed.");
			filter2.IsActive = false;
			effectiveDateFilter.Validation.ValidateProperty1();
			AssertNoErrors(effectiveDateFilter.Property1Info);
			effectiveDateFilter.Property1 = ZDateTime.Empty;
			AssertHasError("Effective Date must be specified.", effectiveDateFilter.Property1Info, "Effective Date must be specified.");
		}

		public void TestShowExpandedResultsFilter()
		{
			var helper = new TariffSearchHelper(Enterprise.Core.Constants.CountryCodes.SouthAfrica, "", null, null);
			var filterBO = new RefCusTariffFilterStripBusinessObject(helper);
			var filterStrip = filterBO.FilterStrips.AddNew();
			filterStrip.FilterDescription = Constants.RefCusTariffFilters.ShowExpandedResults;
			var showExpandedResultsFilter = (ModuleFlagsFilter)filterStrip.CurrentModuleFilter;
			showExpandedResultsFilter.IsActive = true;
			filterStrip = filterBO.FilterStrips.AddNew();
			filterStrip.FilterDescription = Constants.RefCusTariffFilters.ShowExpandedResults;
			var filter2 = (ModuleFlagsFilter)filterStrip.CurrentModuleFilter;
			filter2.IsActive = true;
			showExpandedResultsFilter.Validation.ValidateProperty0();
			AssertHasError("Multiple Show Expanded Results Filter.", showExpandedResultsFilter.Property0Info, "Only one Show Expanded Results filter is allowed.");
			filter2.IsActive = false;
			showExpandedResultsFilter.Validation.ValidateProperty0();
			AssertNoErrors(showExpandedResultsFilter.Property0Info);
		}

		public void TestFiltersValidation()
		{
			var helper = new TariffSearchHelper(Enterprise.Core.Constants.CountryCodes.SouthAfrica, "", null, null);
			var filterBO = new RefCusTariffFilterStripBusinessObject(helper);
			var tariffFilter = ((ModuleTextFilter)filterBO[Constants.RefCusTariffFilters.TariffCode]);
			tariffFilter.IsActive = true;
			tariffFilter.Property = "12";
			tariffFilter.Property = "";
			var validate = tariffFilter.PropertyValidation;
			AssertEquals(true, tariffFilter.HasErrors);
			AssertHasError("Please enter a Tariff Code", tariffFilter.PropertyInfo, "Please enter a Tariff Code.");
			tariffFilter.Property = "1";
			validate = tariffFilter.PropertyValidation;
			AssertHasError("Tariff Code must be at least 2 characters long.", tariffFilter.PropertyInfo, "Tariff Code must be at least 2 characters long.");
			var descrFilter = ((ModuleTextFilter)filterBO[Constants.RefCusTariffFilters.DefaultLanguageDescription]);
			descrFilter.IsActive = true;
			descrFilter.Property = "a";
			tariffFilter.Property = "";
			descrFilter.Property = "";
			validate = descrFilter.PropertyValidation;
			AssertEquals(true, descrFilter.HasErrors);
			AssertHasError("Please enter a Description", descrFilter.PropertyInfo, "Please enter a Description (Default Language).");
			tariffFilter.Property = "";
			descrFilter.Property = "C";
			validate = descrFilter.PropertyValidation;
			AssertEquals(true, descrFilter.HasErrors);
			AssertHasError("Description must be at least 3 characters long.", descrFilter.PropertyInfo, "Description must be at least 3 characters long.");
			helper = new TariffSearchHelper(Enterprise.Core.Constants.CountryCodes.China, "", null, null);
			helper.PartialDescriptionMinLength = 2;
			validate = descrFilter.PropertyValidation;
			AssertEquals(true, descrFilter.HasErrors);
			AssertHasError("Description must be at least 2 characters long.", descrFilter.PropertyInfo, "Description must be at least 3 characters long.");
		}

		public void TestEffectiveDateFormatIsLong()
		{
			var filter = GetNewFilterStripBusinessObject();
			var effectiveDateFilter = (ModuleSingleDateFilter)filter[Constants.RefCusTariffFilters.EffectiveDate];
			AssertEquals("Has DateTimeFormat Long", ZDateTimePickerFormat.Long, effectiveDateFilter.DateTimeFormat);
		}

		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject()
		{
			var helper = new TariffSearchHelper(Core.Constants.CountryCodes.SouthAfrica, "", null, null);
			return new RefCusTariffFilterStripBusinessObject(helper);
		}
	}
}
