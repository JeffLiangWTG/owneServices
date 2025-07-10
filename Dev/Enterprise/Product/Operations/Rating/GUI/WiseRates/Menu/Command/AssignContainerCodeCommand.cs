using System.Linq;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Module;
using Enterprise.Rating.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Internal;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Rating.GUI
{
	public class AssignContainerCodeCommand : IWiseRatesCommand
	{
		bool IWiseRatesCommand.Assign(WiseEntryView wiseEntryView, object sender)
		{
			var (isSeaMode, containerCodeOrISOType) = GetIsSeaModeAndContainerCodeFromEntryView(wiseEntryView);
			if (string.IsNullOrWhiteSpace(containerCodeOrISOType))
			{
				return false;
			}

			return WiseRatesGUIHelper.EnhanceUserPermissionToRunAction(
				permissionName: Res.GetString("33ccd122-db2b-4537-9922-85f897e3f16a", "Container"),
				securityCheckpointSelector: securityCore => securityCore.ContainersModify,
				needsConfirmation: true,
				confirmationCaption: string.Empty,
				action: () => isSeaMode
					? AssignContainerCodeForSea(containerCodeOrISOType)
					: AssignContainerCodeForAir(containerCodeOrISOType));
		}

		#region SEA

		public static bool AssignContainerCodeForSea(string isoType)
		{
			var selectedContainer = SelectContainerForSea(isoType);
			if (selectedContainer == null)
			{
				return false;
			}

			if (!selectedContainer.RC_ISOType.EqualsIgnoringCase(isoType))
			{
				var factory = new BusinessObjectFactory();
				var containerToEdit = (RefContainer)factory.ImportFromAnotherFactory(selectedContainer);
				containerToEdit.RC_ISOType = isoType;
				factory.Save();
				selectedContainer.Reload();
			}

			return true;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Constant strings")]
		static RefContainer SelectContainerForSea(string isoType)
		{
			using (var module = (ZFilterGridModule)ZModuleFactory.Instance.Create(ModuleIDs.RefContainer))
			{
				var provider = new UnmappedContainerSingleSelect();
				module.OverrideModuleDecisionProvider(provider);

				const string TransportMode = "Transport Mode";
				const string ISOCode = "ISO Code";

				#region Default and readonly filters

				// Find SEA containers having no ISO Type
				// or ones with exact input ISO Type (to list the newly and correctly created containers)
				var filterDefaults = ((IFilterBusinessObjectDefaultsProvider)module.GridCollection).FilterBusinessObjectDefaults;
				var filterDefaultTransportMode = new FilterBusinessObjectDefault(TransportMode, "Property", new ZString(RefContainerLookups.ShippingModes.Sea));
				filterDefaults.Add(filterDefaultTransportMode);
				var filterDefaultISOTypeBlank = new FilterBusinessObjectDefault(ISOCode, "ComparisonOperator",
					new ZString(ModuleFilterWithListAndComparisonOperators<ZString>.ComparisonConstants.IsBlank), FilterOrCategory.Red, 1);
				filterDefaults.Add(filterDefaultISOTypeBlank);
				var filterDefaultISOType = new FilterBusinessObjectDefault(ISOCode, "Property", new ZString(isoType), FilterOrCategory.Red);
				filterDefaults.Add(filterDefaultISOType);
				module.FilterBusinessObject.SetExternalDefaults(filterDefaults);

				var filterBizO = (RefContainerFilterBusinessObject)module.FilterBusinessObject;
				void SetFilterGroupSingleAndReadOnly(string filterName)
				{
					filterBizO.ModuleFilters
						.Where(filter => filter.Code.StartsWith(filterName))
						.ToList()
						.ForEach(filter =>
						{
							filter.Visibility = FilterVisibility.AlwaysVisible;
							filter.IsGroupOrCategoryReadOnly = true;
							filter.IsSingleInstanceOnly = true;
							filter.ReadOnly = true;
						});
				}
				SetFilterGroupSingleAndReadOnly(TransportMode);
				SetFilterGroupSingleAndReadOnly(ISOCode);

				#endregion

				var popup = new EmbeddedModulePopup(module, null, true);
				popup.RequireAtLeastOneItemToBeSelected = true;
				provider.Popup = popup;

				ZFormModaliser.ShowDialogAndDispose(popup);

				return provider.SelectedContainer;
			}
		}

		#endregion

		#region AIR

		public static bool AssignContainerCodeForAir(string containerCode)
		{
			var selected = SelectContainerForAir(containerCode);
			return selected != null;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Constant strings")]
		static RefContainer SelectContainerForAir(string containerCode)
		{
			// We can always directly show the "Create New RefContainer" form.
			// However, we let the module framework do all the jobs like security points checking and new/edit/delete containers.
			// Users will have more steps to finish but it saves us work and potential defects.
			using (var module = (ZFilterGridModule)ZModuleFactory.Instance.Create(ModuleIDs.RefContainer))
			{
				var provider = new UnmappedContainerSingleSelect();
				module.OverrideModuleDecisionProvider(provider);

				const string TransportMode = "Transport Mode";
				const string ContainerCode = "Container Code";

				// Find AIR containers with exact input container code (to list the newly and correctly created containers)
				var filterDefaults = ((IFilterBusinessObjectDefaultsProvider)module.GridCollection).FilterBusinessObjectDefaults;
				var filterDefaultTransportMode = new FilterBusinessObjectDefault(TransportMode, "Property", new ZString(RefContainerLookups.ShippingModes.Air));
				filterDefaults.Add(filterDefaultTransportMode);
				var filterDefaultContainerCode = new FilterBusinessObjectDefault(ContainerCode, "Property", new ZString(containerCode));
				filterDefaults.Add(filterDefaultContainerCode);
				module.FilterBusinessObject.SetExternalDefaults(filterDefaults);

				var filterBizO = (RefContainerFilterBusinessObject)module.FilterBusinessObject;
				void SetFilterSingleAndReadOnly(string filterName)
				{
					filterBizO.ModuleFilters
						.Where(filter => filter.Code.EqualsIgnoringCase(filterName))
						.ToList()
						.ForEach(filter =>
						{
							filter.Visibility = FilterVisibility.AlwaysVisible;
							filter.IsGroupOrCategoryReadOnly = true;
							filter.IsSingleInstanceOnly = true;
							filter.ReadOnly = true;
						});
				}
				SetFilterSingleAndReadOnly(TransportMode);
				SetFilterSingleAndReadOnly(ContainerCode);

				var popup = new EmbeddedModulePopup(module, null, true);
				popup.RequireAtLeastOneItemToBeSelected = true;
				provider.Popup = popup;

				ZFormModaliser.ShowDialogAndDispose(popup);

				return provider.SelectedContainer;
			}
		}

		#endregion

		bool IWiseRatesCommand.IsEnabled(WiseEntryView wiseEntryView)
		{
			return wiseEntryView?.TI_RCInfo?.HasErrors() ?? false;
		}

		class UnmappedContainerSingleSelect : SingleSelectModuleDecisionProvider
		{
			public RefContainer SelectedContainer => (RefContainer)SelectedBusinessObject;
		}

		#region Utils

		public static (bool IsSeaMode, string ContainerCode) GetIsSeaModeAndContainerCodeFromEntryView(WiseEntryView wiseEntryView)
		{
			var wiseEntry = wiseEntryView?.UnderlyingWiseEntry;
			if (wiseEntry == null)
			{
				return (false, ""); // nothing to assign
			}

			var wiseRate = wiseEntry?.WiseRate;
			var containerCode = wiseRate?.Container?.Code ?? string.Empty;

			var transportMode = wiseRate?.TransportMode;
			var acceptedModes = new[] { RatingConstants.TransportMode.SEA, RatingConstants.TransportMode.AIR };
			if (!acceptedModes.Contains(transportMode))
			{
				ErrorReporter.ReportOnce(
					"AssignContainerCodeCommand|GetIsSeaModeAndContainerCodeFromEntryView",
					$"Unexpected Transport Mode: '{transportMode}' while only AIR and SEA are accepted");
				return (false, ""); // nothing to assign
			}

			return (transportMode == RatingConstants.TransportMode.SEA, containerCode);
		}

		#endregion
	}
}
