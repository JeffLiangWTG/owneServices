using System;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;

namespace Enterprise.Rating.Business
{
	public class BulkRateUpdaterValidation : ZValidation
	{
		public BulkRateUpdaterValidation(BulkRateUpdater parent)
				: base(parent)
		{
			if (ReferenceEquals(parent, null))
			{
				throw new ArgumentNullException(nameof(parent));
			}

			this.Parent = parent;
			this.ParentListInternals = parent;
		}

		#region Overrides

		public override Type AutoValidationType => typeof(BulkRateUpdaterValidation);

		public override void ValidateAll()
		{
			using (ParentListInternals.SuspendListChanged())
			{
				ValidateModule();
				ValidateType();
				ValidateMode(); 
				ValidateOrigin();
				ValidateDestination();
				ValidateServiceLevel();
				ValidateCommodityCode();
				ValidateNewEntryStartDate(); 
				ValidateNewEntryEndDate(); 
				ValidateSupplier();
				ValidateCarrier();
				ValidateCarrierServiceLevel();
				ValidateStartDate();
				ValidateEndDate();
				ValidateSelectedClient();
				ValidateGatewayAgentType();
				ValidateShowActiveQuotes();
				ValidateShowClientRates();
				ValidateShowCompanyTariffs();
				ValidateShowCostings();
				ValidateShowIntercompanyTariffs();
				ValidateShipmentConsolidationStatus();
				ValidateContainerTypes();
			}
		}

		#endregion

		public void ValidateModule()
		{
			Parent.ModuleInfo.ClearAllNotifications();
			if (Parent.Module.IsEmpty)
			{
				MandatoryValidation.CheckEntered(Parent.ModuleInfo);
			}
			else
			{
				ListValidation.ErrorIfInvalidCode(Parent.ModuleInfo, Parent.Modules);
			}

			ValidateType();
		}

		public void ValidateType()
		{
			Parent.TypeInfo.ClearAllNotifications();
			if (Parent.Type.IsEmpty)
			{
				MandatoryValidation.CheckEntered(Parent.TypeInfo);
			}
			else
			{
				ListValidation.ErrorIfInvalidCode(Parent.TypeInfo, Parent.Types);
			}

			ValidateMode();
		}

		public void ValidateMode()
		{
			Parent.ModeInfo.ClearAllNotifications();
			if (Parent.Mode.IsEmpty)
			{
				MandatoryValidation.CheckEntered(Parent.ModeInfo);
			}
			else
			{
				ListValidation.ErrorIfInvalidCode(Parent.ModeInfo, Parent.Modes);
			}
		}

		public void ValidateOrigin()
		{
			Parent.OriginInfo.ClearAllNotifications();
			if (!Parent.Origin.IsEmpty)
			{
				ListValidation.ErrorIfInvalidCode(Parent.OriginInfo, Parent.Lookups.Locations);
			}
		}

		public void ValidateDestination()
		{
			Parent.DestinationInfo.ClearAllNotifications();
			if (!Parent.Destination.IsEmpty)
			{
				ListValidation.ErrorIfInvalidCode(Parent.DestinationInfo, Parent.Lookups.Locations);
			}
		}

		public void ValidateServiceLevel()
		{
			Parent.ServiceLevelInfo.ClearAllNotifications();
			ListValidation.ErrorIfInvalidCode(Parent.ServiceLevelInfo, Parent.Lookups.ServiceLevel_NIs);
		}

		public void ValidateCarrierServiceLevel()
		{
			Parent.CarrierServiceLevelInfo.ClearAllNotifications();
			ListValidation.ErrorIfInvalidCode(Parent.CarrierServiceLevelInfo, Parent.Lookups.CarrierServiceLevels);
		}

		public void ValidateGatewayServiceLevel()
		{
			Parent.GatewayServiceLevelInfo.ClearAllNotifications();
			ListValidation.ErrorIfInvalidCode(Parent.GatewayServiceLevelInfo, Parent.Lookups.GatewayServiceLevels);
		}

		public void ValidateShipmentGatewayServiceLevel()
		{
			Parent.ShipmentGatewayServiceLevelInfo.ClearAllNotifications();
			ListValidation.ErrorIfInvalidCode(Parent.ShipmentGatewayServiceLevelInfo, Parent.Lookups.ShipmentGatewayServiceLevels);
		}

		public void ValidateCommodityCode()
		{
			Parent.CommodityCodeInfo.ClearAllNotifications();
			ListValidation.ErrorIfInvalidCode(Parent.CommodityCodeInfo, Parent.Lookups.CommodityCodes);
		}

		public void ValidateSupplier()
		{
			Parent.SupplierInfo.ClearAllNotifications();
			TypeValidation.CheckValidGuid(Parent.SupplierInfo);
		}

		public void ValidateCarrier()
		{
			Parent.CarrierInfo.ClearAllNotifications();
			TypeValidation.CheckValidGuid(Parent.CarrierInfo);
		}

		public void ValidateStartDate()
		{
			Parent.StartDateInfo.ClearAllNotifications();
			TypeValidation.CheckValidZDateAndRange(Parent.StartDateInfo);
		}

		public void ValidateEndDate()
		{
			Parent.EndDateInfo.ClearAllNotifications();
			TypeValidation.CheckValidZDateAndRange(Parent.EndDateInfo);

			if (!Parent.StartDate.IsEmpty && !Parent.EndDate.IsEmpty && Parent.StartDate.IsValid && Parent.EndDate.IsValid && Parent.StartDate > Parent.EndDate)
			{
				Parent.EndDateInfo.AddError(Res.GetString("af6bfe5a-e1de-4e46-9e94-543a2ff1d9af", "End Date cannot be earlier than the the Start Date."));
			}
		}

		public void ValidateContractNumberLinked()
		{
			Parent.ContractNumberLinkedInfo.ClearAllNotifications();
			ListValidation.ErrorIfInvalidCode(Parent.ContractNumberLinkedInfo, Parent.ContractNumberLinkedValues);
		}

		public void ValidateGatewayAgentType()
		{
			Parent.GatewayAgentTypeInfo.ClearAllNotifications();
			if (!string.IsNullOrEmpty(Parent.GatewayAgentType) && !Parent.ShowIntercompanyTariffs)
			{
				Parent.GatewayAgentTypeInfo.AddWarning(Res.GetString("fb6271f1-dc09-401d-9c0a-61b1bdef9c09", "Gateway Agent Type filter is applicable only for Intercompany Tariffs."));
			}
			if (Parent.GatewayAgentType == Core.Constants.GatewayAgentType.Codes.SucceedingSendingAgent || Parent.GatewayAgentType == Core.Constants.GatewayAgentType.Codes.FirstSendingAgent)
			{
				ListValidation.ErrorIfInvalidCode(Parent.GatewayAgentTypeInfo, Parent.Lookups.NotObsoleteGatewayAgentTypes, ErrorMessages.ObsoleteGatewayAgentTypesErrorMessage);
			}
			else
			{
				ListValidation.ErrorIfInvalidCode(Parent.GatewayAgentTypeInfo, Parent.Lookups.GatewayAgentTypes);
			}
		}

		public void ValidateShipmentConsolidationStatus()
		{
			Parent.ShipmentConsolidationStatusInfo.ClearAllNotifications();
			if (!Parent.ShipmentConsolidationStatus.IsEmpty && !Parent.ShowCostings)
			{
				Parent.ShipmentConsolidationStatusInfo.AddWarning(Res.GetString("8A66AED6-7B0D-4CA4-AE96-4A9C33E5D324", "Shipment Consolidation Status filter is only applicable to Costing Tariffs."));
			}

			ListValidation.ErrorIfInvalidCode(Parent.ShipmentConsolidationStatusInfo, Parent.Lookups.ShipmentConsolidationStatusList);
		}

		public void ValidateSelectedClient()
		{
			Parent.SelectedClientInfo.ClearAllNotifications();
			ListValidation.ErrorIfInvalidPK(Parent.SelectedClientInfo, Parent.Organisations);
		}

		public void ValidateShowClientRates()
		{
			Parent.ShowClientRatesInfo.ClearAllNotifications();
			if (Parent.ShowClientRates && !Parent.ClientRatesBulkUpdateAllowed)
			{
				Parent.ShowClientRates = false;
				Parent.ShowClientRatesInfo.AddWarning(Res.GetString("8c7e4702-50ca-4bf9-bd8e-8c65e6a8dfc4", "You do not have security rights to update Client Rates."));
			}

			ValidateModuleSelection(Parent.ShowClientRatesInfo, Parent.ShowClientRates);
		}

		public void ValidateContainerTypes()
		{
			foreach (var containerType in Parent.ContainerTypes.Cast<BulkRateUpdaterContainerType>())
			{
				containerType.Validation.ValidateRC_Code();
			}
		}

		public void ValidateShowIntercompanyTariffs()
		{
			Parent.ShowIntercompanyTariffsInfo.ClearAllNotifications();
			if (Parent.ShowIntercompanyTariffs && !Parent.IntercompanyTariffsBulkUpdateAllowed)
			{
				Parent.ShowIntercompanyTariffs = false;
				Parent.ShowIntercompanyTariffsInfo.AddWarning(Res.GetString("c69c6771-1914-4a4d-8ee2-62ed1818295c", "You do not have security rights to update Intercompany Tariffs."));
			}
			ValidateModuleSelection(Parent.ShowIntercompanyTariffsInfo, Parent.ShowIntercompanyTariffs);
		}

		public void ValidateShowActiveQuotes()
		{
			Parent.ShowActiveQuotesInfo.ClearAllNotifications();
			if (Parent.ShowActiveQuotes && !Parent.QuotationBulkUpdateAllowed)
			{
				Parent.ShowActiveQuotes = false;
				Parent.ShowActiveQuotesInfo.AddWarning(Res.GetString("4968972f-17a4-4697-8af9-7f558cfd0825", "You do not have security rights to update Quotes."));
			}
			ValidateModuleSelection(Parent.ShowActiveQuotesInfo, Parent.ShowActiveQuotes);
		}

		public void ValidateShowCostings()
		{
			Parent.ShowCostingsInfo.ClearAllNotifications();
			if (Parent.ShowCostings && !Parent.CostingRatesBulkUpdateAllowed)
			{
				Parent.ShowCostings = false;
				Parent.ShowCostingsInfo.AddWarning(Res.GetString("28f40d32-42ae-4e00-8354-4b969daf4f45", "You do not have security rights to update Costings."));
			}

			ValidateModuleSelection(Parent.ShowCostingsInfo, Parent.ShowCostings);
		}

		public void ValidateShowCompanyTariffs()
		{
			Parent.ShowCompanyTariffsInfo.ClearAllNotifications();
			if (Parent.ShowCompanyTariffs && !Parent.CompanyTariffBulkUpdateAllowed)
			{
				Parent.ShowCompanyTariffs = false;
				Parent.ShowCompanyTariffsInfo.AddWarning(Res.GetString("c414b0d7-9927-4718-a6d7-d232012aeba6", "You do not have security rights to update Company Tariffs."));
			}
			ValidateModuleSelection(Parent.ShowCompanyTariffsInfo, Parent.ShowCompanyTariffs);
		}

		public void ValidateCreateNewEntry()
		{
			Parent.CreateNewEntryInfo.ClearAllNotifications();
			if (Parent.CreateNewEntry && Parent.ShowActiveQuotes)
			{
				Parent.CreateNewEntryInfo.AddWarning(Res.GetString("713ffe42-6007-4828-99aa-615186cc6c14", "You cannot choose to create new entries (trade lanes) on Quotations."));
			}
		}

		public void ValidateNewEntryStartDate()
		{
			Parent.NewEntryStartDateInfo.ClearAllNotifications();
			if (Parent.CreateNewEntry)
			{
				MandatoryValidation.CheckEntered(Parent.NewEntryStartDateInfo);

				if (!Parent.NewEntryStartDateInfo.HasErrors())
				{
					TypeValidation.CheckValidZDateTimeAndRange(Parent.NewEntryStartDateInfo);
				}
			}
		}

		public void ValidateNewEntryEndDate()
		{
			Parent.NewEntryEndDateInfo.ClearAllNotifications();
			if (Parent.CreateNewEntry)
			{
				TypeValidation.CheckValidZDateTimeAndRange(Parent.NewEntryEndDateInfo);
			}
		}

		#region ValidateModuleSelection

		void ValidateModuleSelection(ZPropertyInfo zPropertyInfo, bool isChecked)
		{
			if (Parent.IsNonIntercompanyTariffsModuleSelected && Parent.ShowIntercompanyTariffs)
			{
				if (isChecked)
				{
					zPropertyInfo.AddError(ErrorMessages.IntercompanyTariffRatesBulkUpdateCheck);
				}
			}
			else
			{
				ClearNotification(Parent.ShowClientRatesInfo, Parent.ShowClientRates);
				ClearNotification(Parent.ShowActiveQuotesInfo, Parent.ShowActiveQuotes);
				ClearNotification(Parent.ShowCostingsInfo, Parent.ShowCostings);
				ClearNotification(Parent.ShowCompanyTariffsInfo, Parent.ShowCompanyTariffs);
				ClearNotification(Parent.ShowIntercompanyTariffsInfo, Parent.ShowIntercompanyTariffs);

				Parent.UpdateDummyEntry();
			}

			ValidateGatewayAgentType();
		}

		void ClearNotification(ZPropertyInfo zPropertyInfo, bool isChecked)
		{
			if (isChecked && zPropertyInfo.HasError(ErrorMessages.IntercompanyTariffRatesBulkUpdateCheck))
			{
				zPropertyInfo.ClearAllNotifications();
			}
		}

		#endregion

		#region Implementation

		protected readonly BulkRateUpdater Parent;
		readonly ISingleElementListInternal ParentListInternals;

		#endregion
	}
}
