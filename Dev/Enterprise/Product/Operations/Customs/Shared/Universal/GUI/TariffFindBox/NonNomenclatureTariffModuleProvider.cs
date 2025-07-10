using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Internal;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.Universal.GUI
{
	public static class NonNomenclatureTariffModuleProvider
	{
		public static IFindBoxPopup Show(ITariffFindBox findBox, ZString country, ZString tariffType)
		{
			var module = (ZFilterModule)ZModuleFactory.Instance.Create(ModuleIDs.Customs.Universal.RefCusTariff);
			module.OverrideModuleDecisionProvider(new ZArchitecture.Modules.Internal.DirectToFormModuleDecisionProvider(findBox));
			var defaults = new FilterBusinessObjectDefaults();
			defaults.Add(new FilterBusinessObjectDefault(Constants.RefCusTariffFilters.CountryOrGrouping, "Property", country, false));
			defaults.Add(new FilterBusinessObjectDefault(Constants.RefCusTariffFilters.TariffType, "Property1", country, false));
			defaults.Add(new FilterBusinessObjectDefault(Constants.RefCusTariffFilters.TariffType, "Property2", tariffType, false));
			defaults.Add(new FilterBusinessObjectDefault(Constants.RefCusTariffFilters.TariffCode, "Property", (ZString)findBox.Code));

			if (findBox.GetEffectiveDate != null)
			{
				var effectiveDate = findBox.GetEffectiveDate?.Invoke() ?? ZDateTime.Today;
				defaults.Add(new FilterBusinessObjectDefault(Constants.RefCusTariffFilters.EffectiveDate, "Property1", effectiveDate, false));
			}
			else
			{
				defaults.Add(new FilterBusinessObjectDefault(Constants.RefCusTariffFilters.EffectiveDate, "Property1", ZDateTime.Today, true));
			}

			if (findBox.ShowDescriptionFilterOnNonNomenclatureTariffModule)
			{
				defaults.Add(new FilterBusinessObjectDefault(Constants.RefCusTariffFilters.DefaultLanguageDescription, "Property", ZString.Empty));
			}

			module.FilterBusinessObject.SetExternalDefaults(defaults);
			module.FilterBusinessObject.ModuleFilters[Constants.RefCusTariffFilters.CountryOrGrouping].Visibility = FilterVisibility.AlwaysAppliedAndHidden;
			((INomenclatureEnabler)module.FilterBusinessObject).Enable = false;
			return new EmbeddedModulePopup(module);
		}
	}

	public interface INomenclatureEnabler
	{
		bool Enable { get; set; }
	}
}
