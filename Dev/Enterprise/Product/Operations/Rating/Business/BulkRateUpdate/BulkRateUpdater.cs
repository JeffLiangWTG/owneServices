using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Rating.Business
{
	public class BulkRateUpdater : NonPersistentBusinessObject, IObsoleteValidation
	{
		public BulkRateUpdater()
			: base(new BusinessObjectFactory())
		{
			ShowClientRates = true;
			UpdateStandardRates = true;
		}

		#region Entries

		public SimpleRateEntryCollection Entries => entries ?? (entries = new SimpleRateEntryCollection(Factory));

		SimpleRateEntryCollection entries;

		public void LoadEntries()
		{
			Entries.RemoveAll();
			LoadEntries(Entries, null);
		}

		internal void LoadEntries(SimpleRateEntryCollection entryCollection, ZDBOnlySubQuery subQuery)
		{
			if (IsNonIntercompanyTariffsModuleSelected ^ ShowIntercompanyTariffs)
			{
				var query = Query;
				if (subQuery != null)
				{
					query.AddSubQuery(subQuery, JoinCondition.And);
				}

				entryCollection.Load(query);
				SetUpdateFlag(entryCollection);
			}
		}

		void SetUpdateFlag(SimpleRateEntryCollection entryCollection)
		{
			foreach (RateEntry entry in entryCollection)
			{
				if (!includeInAutoUpdates.TryGetValue(entry.PK, out var includeInUpdates))
				{
					var header = entry.Parent?.Header;
					if (header != null)
					{
						includeInUpdates = header.GetCompanyDataForGlbCompany(GlbCompany.CurrentCompany).OB_ARAutoUpdateRates;
					}

					includeInAutoUpdates.Add(entry.PK, includeInUpdates);
				}

				entry.IncludeInUpdate = includeInUpdates;
			}
		}

		readonly Dictionary<ZGuid, bool> includeInAutoUpdates = new Dictionary<ZGuid, bool>();

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Enterprise.Globalization", "EDI007", Justification = "Translation is not required")]
		public void ResetActionsLineChargeDescription(RateEntry entry)
		{
			if (entry.IncludeInUpdate && actionsLine?.ChargeCode != null)
			{
				var shouldResetChargeDescription =
					(entry.IsQuote() && !Env.Security.QuotationChargeDescriptionOverride.IsAllowed) ||
					(entry.IsClientRate() && !Env.Security.ClientRatesChargeDescriptionOverride.IsAllowed) ||
					(entry.IsCosting() && !Env.Security.CostingRatesChargeDescriptionOverride.IsAllowed) ||
					(entry.IsCompanyTariff() && !Env.Security.CompanyTariffRatesChargeDescriptionOverride.IsAllowed) ||
					(entry.IsIntercompanyTariff() && !Env.Security.IntercompanyTariffsChargeDescriptionOverride.IsAllowed);

				if (shouldResetChargeDescription)
				{
					actionsLine.OverrideChargeDescription = false;
					actionsLine.TL_RateDesc = actionsLine.ChargeCode.AC_Desc;
				}
			}
		}

		#region Query

		ZDBOnlyQuery Query
		{
			get
			{
				var result = new ZDBOnlyQuery(typeof(RateEntry));

				if (ShowIntercompanyTariffs)
				{
					var entryQuery = new ZDBOnlyQuery(typeof(RateEntry));
					entryQuery.AddToFilter(GetEntriesFilter(false));

					entryQuery.AddSubQuery(GetHeaderSubQuery(IntercompanyTariffsFilter, null), JoinCondition.And);

					result.AddToFilter(entryQuery, JoinCondition.Or);

					return result;
				}

				if (ShowClientRates || ShowActiveQuotes)
				{
					var entryQuery = new ZDBOnlyQuery(typeof(RateEntry));
					entryQuery.AddToFilter(GetEntriesFilter(true));

					var headerFilter = new ZQuery();

					if (ShowClientRates)
					{
						headerFilter.AddToFilter(ClientRatesFilter, JoinCondition.Or);
					}

					if (ShowActiveQuotes)
					{
						headerFilter.AddToFilter(QuotesFilter, JoinCondition.Or);
					}

					entryQuery.AddSubQuery(GetHeaderSubQuery(headerFilter), JoinCondition.And);

					result.AddToFilter(entryQuery, JoinCondition.Or);
				}

				if (ShowCostings)
				{
					var entryQuery = new ZDBOnlyQuery(typeof(RateEntry));
					entryQuery.AddToFilter(GetEntriesFilter(false));

					entryQuery.AddSubQuery(GetHeaderSubQuery(CostingsFilter), JoinCondition.And);

					result.AddToFilter(entryQuery, JoinCondition.Or);
				}

				if (ShowCompanyTariffs)
				{
					var entryQuery = new ZDBOnlyQuery(typeof(RateEntry));
					entryQuery.AddToFilter(GetEntriesFilter(false));

					entryQuery.AddSubQuery(GetHeaderSubQuery(CompanyTariffsFilter), JoinCondition.And);

					result.AddToFilter(entryQuery, JoinCondition.Or);
				}

				return result;
			}
		}

		#region HeaderSubQuery

		ZDBOnlySubQuery GetHeaderSubQuery(ZQuery headerFilter) => GetHeaderSubQuery(headerFilter, GlbCompany.CurrentCompany.PK);

		ZDBOnlySubQuery GetHeaderSubQuery(ZQuery headerFilter, ZGuid? globalCompanyPK)
		{
			var result = new ZDBOnlySubQuery(typeof(RatingHeader), RateEntrySchema.TI_TH);

			if (globalCompanyPK.HasValue)
			{
				result.AddToFilter(RatingHeaderSchema.TH_GC, globalCompanyPK.Value);
			}
			else
			{
				result.AddToFilter(RatingHeaderSchema.TH_GC, null);
			}

			result.AddToFilter(headerFilter);

			return result;
		}

		ZQuery ClientRatesFilter
		{
			get
			{
				var query = new ZQuery();
				query.AddToFilter(RatingHeaderSchema.TH_RateType, RatingConstants.RatingHeaderTypes.ClientRate);

				if (!SelectedClient.IsEmpty)
				{
					query.AddToFilter(RatingHeaderSchema.TH_OH, SelectedClient);
				}

				return query;
			}
		}

		ZQuery QuotesFilter
		{
			get
			{
				var result = new ZQuery(RatingHeaderSchema.TH_RateType, RatingConstants.RatingHeaderTypes.Quote);
				result.AddToFilter(RatingHeaderSchema.TH_Accepted, null);
				result.AddToFilter(RatingHeaderSchema.TH_IsCancelled, ZBool.False);

				if (!StartDate.IsEmpty)
				{
					result.AddToFilter(GetDateFilter(StartDate, RatingHeaderSchema.TH_QuoteDate, RatingHeaderSchema.TH_QuoteEndDate));
				}

				result.AddToFilter(GetDateFilter(!EndDate.IsEmpty && EndDate > ZDateTime.Today ? EndDate : ZDateTime.Today, RatingHeaderSchema.TH_QuoteDate, RatingHeaderSchema.TH_QuoteEndDate));

				if (!SelectedClient.IsEmpty)
				{
					var rateQueryHelper = new RateQueryHelper(Factory);
					result.AddToFilter(rateQueryHelper.GetOrganisationQueryForQuotation(SelectedClient));
				}

				return result;
			}
		}

		ZQuery CostingsFilter
		{
			get
			{
				var result = new ZQuery(RatingHeaderSchema.TH_RateType, RatingConstants.RatingHeaderTypes.Costing);
				if (!Supplier.IsEmpty)
				{
					result.AddToFilter(RatingHeaderSchema.TH_OH, Supplier);
				}

				return result;
			}
		}

		ZQuery CompanyTariffsFilter
		{
			get
			{
				const byte companyTariffsLevel = 1;

				var result = new ZQuery(RatingHeaderSchema.TH_RateType, RatingConstants.RatingHeaderTypes.Tariff);
				result.AddToFilter(RatingHeaderSchema.TH_GlobalRateLevel, companyTariffsLevel);

				return result;
			}
		}

		ZQuery IntercompanyTariffsFilter
		{
			get
			{
				var result = new ZQuery(RatingHeaderSchema.TH_RateType, RatingConstants.RatingHeaderTypes.IntercompanyTariff);

				if (!Supplier.IsEmpty)
				{
					result.AddToFilter(RatingHeaderSchema.TH_OH, Supplier);
				}

				return result;
			}
		}

		#endregion

		#region EntriesFilter

		ZQuery GetEntriesFilter(bool includeSupplier)
		{
			var filter = new ZQuery();
			filter.AddToFilter(TypeModeFilter);
			if (!Origin.IsEmpty)
			{
				filter.AddToFilter(GetLocationQuery(Origin, RateEntrySchema.TI_OriginLRC), JoinCondition.And);
			}
			if (!Destination.IsEmpty)
			{
				filter.AddToFilter(GetLocationQuery(Destination, RateEntrySchema.TI_DestinationLRC), JoinCondition.And);
			}
			if (!ServiceLevel.IsEmpty)
			{
				filter.AddToFilter(RateEntrySchema.TI_RS_NKServiceLevel_NI, ServiceLevel);
			}
			if (!CarrierServiceLevel.IsEmpty)
			{
				filter.AddToFilter(RateEntrySchema.TI_PL_NKCarrierServiceLevel, CarrierServiceLevel);
			}
			if (!GatewayServiceLevel.IsEmpty)
			{
				filter.AddToFilter(RateEntrySchema.TI_RS_NKGatewayServiceLevel, GatewayServiceLevel);
			}
			if (!ShipmentGatewayServiceLevel.IsEmpty)
			{
				filter.AddToFilter(RateEntrySchema.TI_RS_NKShipmentGatewayServiceLevel, ShipmentGatewayServiceLevel);
			}
			if (!CommodityCode.IsEmpty)
			{
				filter.AddToFilter(RateEntrySchema.TI_RH_NKCommodityCode, CommodityCode);
			}
			if (includeSupplier && !Supplier.IsEmpty)
			{
				filter.AddToFilter(RateEntrySchema.TI_OH_Supplier, Supplier);
			}
			if (!Carrier.IsEmpty)
			{
				filter.AddToFilter(RateEntrySchema.TI_OH_TransportProvider, Carrier);
			}
			if (!ContractNumber.IsEmpty)
			{
				filter.AddToFilter(RateEntrySchema.TI_ContractNumber, SQLComparisonOperator.StartsWith, ContractNumber);
			}
			if (!GatewayAgentType.IsEmpty && ShowIntercompanyTariffs)
			{
				filter.AddToFilter(RateEntrySchema.TI_GatewayAgentType, SQLComparisonOperator.Equal, GatewayAgentType);
			}
			if (!ShipmentConsolidationStatus.IsEmpty && ShowCostings)
			{
				filter.AddToFilter(RateEntrySchema.TI_ShipmentConsolidationStatus, SQLComparisonOperator.Equal, ShipmentConsolidationStatus);
			}
			if (ContractNumberLinked != YesNoEmptyList.Codes.Empty)
			{
				var linked = ContractNumberLinked == YesNoEmptyList.Codes.Yes;

				filter.AddToFilter(RateEntrySchema.TI_ContractNumberLinked, SQLComparisonOperator.Equal, linked);
			}
			if (ContainerTypes.Count > 0)
			{
				filter.AddToFilter(RateEntrySchema.TI_RC, ContainerTypes.Select(t => t.Container.PK));
			}
			filter.AddToFilter(DatesFilter);

			return filter;
		}

		ZQuery TypeModeFilter
		{
			get
			{
				var filter = new ZQuery(RateEntrySchema.TI_RateCategory, Type);
				if (Mode != Core.Constants.RateMode.ALL)
				{
					filter.AddToFilter(RateEntrySchema.TI_Mode, Mode);
				}

				return filter;
			}
		}

		ZQuery GetLocationQuery(ZString location, SchemaColumn column)
			 => LocationHelper.GetLocationType(location) == LocationHelper.LocationType.Port
				? new ZQuery(column, location)
				: LocationHelper.GetLocationFilter(Factory, location, column, false, typeof(RateEntry));

		ZQuery DatesFilter
		{
			get
			{
				var filter = new ZQuery();

				if (!StartDate.IsEmpty)
				{
					filter.AddToFilter(GetDateFilter(StartDate, RateEntrySchema.TI_RateStartDate, RateEntrySchema.TI_RateEndDate));
				}

				if (!EndDate.IsEmpty)
				{
					filter.AddToFilter(GetDateFilter(EndDate, RateEntrySchema.TI_RateStartDate, RateEntrySchema.TI_RateEndDate));
				}

				if (NoExpireDate)
				{
					filter.AddToFilter(RateEntrySchema.TI_RateEndDate, SQLComparisonOperator.Equal, null);
				}

				if (!ShowExpired)
				{
					var expiredFilter = new ZQuery(RateEntrySchema.TI_RateEndDate, SQLComparisonOperator.GreaterThanOrEqualToDatePartOnly, ZDate.Today);
					expiredFilter.AddToFilter(JoinCondition.Or, RateEntrySchema.TI_RateEndDate, SQLComparisonOperator.Equal, null);
					filter.AddToFilter(expiredFilter);
				}

				return filter;
			}
		}

		ZQuery GetDateFilter(ZDateTime date, SchemaColumn startColumn, SchemaColumn endColumn)
		{
			var filter = new ZQuery();

			var dateFilterPart1 = new ZQuery(startColumn, SQLComparisonOperator.LessThanOrEqualToDatePartOnly, date);
			filter.AddToFilter(dateFilterPart1, JoinCondition.And);

			var dateFilterPart2 = new ZQuery(endColumn, SQLComparisonOperator.GreaterThanOrEqualToDatePartOnly, date);
			dateFilterPart2.AddToFilter(JoinCondition.Or, endColumn, SQLComparisonOperator.Equal, null);
			filter.AddToFilter(dateFilterPart2, JoinCondition.And);

			return filter;
		}

		#endregion

		#endregion

		#endregion

		#region Preview and Save

		public SimpleRateEntryCollection PreviewEntries
		{
			get
			{
				if (previewEntries == null)
				{
					previewEntries = new SimpleRateEntryCollection(Factory);
					RegisterEditableChildObject(previewEntries);
				}

				return previewEntries;
			}
		}

		SimpleRateEntryCollection previewEntries;

		SimpleRateEntryCollection remainingEntriesToUpdate;

		public void UpdateFirstRateEntriesBatch()
		{
			PreviewEntries.RemoveAll();

			var action = GetUpdateAction();
			var factory = new BusinessObjectFactory
			{
				NameForDebugging = "BulkUpdateAction.UpdateRateEntriesBatch"
			};

			remainingEntriesToUpdate = action.GetRateEntriesToUpdate(factory);
			var updatedEntries = action.Update(remainingEntriesToUpdate, factory, DefaultBatchSize);
			AllEntriesToUpdateCount = remainingEntriesToUpdate.Count + updatedEntries.Count;
			HasAdditionalBatchesToProcess = remainingEntriesToUpdate.Any();

			PreviewEntries.AddRange(updatedEntries);
		}

		BulkUpdateAction GetUpdateAction()
		{
			switch (Action)
			{
				case Actions.AddOrReplaceCharge:
					return new BulkUpdateActionAddOrReplaceCharge(this);

				case Actions.ReplaceCharge:
					return new BulkUpdateActionReplaceCharge(this);

				case Actions.IncreaseDecreaseCharge:
					return new BulkUpdateActionIncreaseDecreaseCharge(this);

				case Actions.DeleteCharge:
					return new BulkUpdateActionDeleteCharge(this);

				default:
					return null;
			}
		}

		public int AllEntriesToUpdateCount { get; private set; }

		public bool HasAdditionalBatchesToProcess { get; private set; }

		public event EventHandler<ProcessingProgressedEventArgs> ProcessingProgressed;

		string HandleSaveException(Exception ex)
		{
			var exceptionMessage = RateEntryCollectionValidator.HandleOverlapExceptionForBulkRateUpdater(ex, PreviewEntries);
			return string.IsNullOrEmpty(exceptionMessage)
				? Res.GetString("9a7e1e8c-eef9-4d74-b2e5-2473d18c25de", "An error occurred saving updated rates.")
				: exceptionMessage;
		}

		public string TrySaveRateEntryInBatches()
		{
			string errorMessage = null;

			var updatedEntriesCount = PreviewEntries.Count;
			if (updatedEntriesCount > 0)
			{
				var factoryProvider = new BusinessObjectFactoryProvider(PreviewEntries[0].Factory);
				savedSuccessfully = true;

				var hasEntriesToSave = true;
				var updateAction = GetUpdateAction();
				while (hasEntriesToSave)
				{
					factoryProvider.Current.Saved += new BusinessObjectFactory.SavedEventHandler(Factory_Saved);
					try
					{
						factoryProvider.Current.Save();
					}
					catch (ZSaveException ex)
					{
						return HandleSaveException(ex);
					}
					finally
					{
						factoryProvider.Current.Saved -= new BusinessObjectFactory.SavedEventHandler(Factory_Saved);
					}

					//updatedEntriesCount can be slightly inaccurate due to CreateNewEntry creating additional rates once updated. This is intended functionality.
					FireProcessingProgressed(updatedEntriesCount * 100 / AllEntriesToUpdateCount);

					hasEntriesToSave = remainingEntriesToUpdate.Any();
					if (hasEntriesToSave)
					{
						factoryProvider.CreateNewWithoutSave();
						var updatedEntries = updateAction.Update(remainingEntriesToUpdate, factoryProvider.Current, DefaultBatchSize);
						updatedEntriesCount += updatedEntries.Count;
					}
				}

				if (!savedSuccessfully)
				{
					errorMessage = Res.GetString("9a7e1e8c-eef9-4d74-b2e5-2473d18c25de", "An error occurred saving updated rates.");
				}
			}

			return errorMessage;
		}

		void FireProcessingProgressed(int percentage)
		{
			ProcessingProgressed?.Invoke(this, new ProcessingProgressedEventArgs(percentage));
		}

		bool savedSuccessfully;

		void Factory_Saved(BusinessObjectFactory factory, bool isSavedSuccessfully)
		{
			if (!isSavedSuccessfully)
			{
				savedSuccessfully = false;
			}
		}

		public void SelectAllEntries(bool shouldSelect)
		{
			foreach (RateEntry entry in Entries)
			{
				entry.IncludeInUpdate = shouldSelect;
			}
		}

		#endregion

		#region Properties

		#region Batch Size for Saving Entries

		protected virtual int DefaultBatchSize => 50;

		#endregion

		#region Module

		[List(nameof(Modules))]
		[MaxLength(3)]
		public ZString Module
		{
			get { return module; }
			set
			{
				SetNonPersistentPropertyValue(ModuleInfo, ref module, value);
				if (!IsValidationSuspended)
				{
					Validation.ValidateModule();
				}
			}
		}

		ZString module;

		public ZPropertyInfo ModuleInfo => GetZPropertyInfo(nameof(Module));

		#endregion

		#region Type
		[List(nameof(Types))]
		[MaxLength(3)]
		public ZString Type
		{
			get { return type; }
			set
			{
				if (type != value)
				{
					CheckMaximumLength(TypeInfo, value);
					type = value;
					DummyEntry.TI_RateCategory = value;

					if (!IsContainerised)
					{
						ContainerTypes.RemoveAll();
					}
					ContainerTypes.SetReadOnlyIncludingChildren(!IsContainerised);
				}
				if (!IsValidationSuspended)
				{
					Validation.ValidateType();
				}
				if (Modes.Count == 1)
				{
					Mode = Modes[0].Code;
				}

				TypeInfo.RefreshBinding();
			}
		}

		ZString type;

		public ZPropertyInfo TypeInfo => GetZPropertyInfo(nameof(Type));

		#endregion

		#region Mode

		[List(nameof(Modes))]
		[MaxLength(AutoRateEntry.Schema.TI_ModeMaxLength)]
		public ZString Mode
		{
			get { return mode; }
			set
			{
				if (mode != value)
				{
					CheckMaximumLength(ModeInfo, value);
					transportModeFilter.Clear();
					transportModeFilter.AddToFilter(RefContainerSchema.RC_ShippingMode, RatingConstants.GetTransportModeFromMode(value));
					mode = value;
					if (!IsContainerised)
					{
						ContainerTypes.RemoveAll();
					}
					ContainerTypes.SetReadOnlyIncludingChildren(!IsContainerised);
				}
				if (!IsValidationSuspended)
				{
					Validation.ValidateMode();
				}
				ModeInfo.RefreshBinding();
			}
		}

		ZString mode;

		public ZPropertyInfo ModeInfo => GetZPropertyInfo(nameof(Mode));

		#endregion

		#region Origin

		[List(nameof(Lookups) + "." + nameof(RateEntryLookups.Locations))]
		[MaxLength(AutoRateEntry.Schema.TI_OriginLRCMaxLength)]
		public ZString Origin
		{
			get { return origin; }
			set
			{
				if (origin != value)
				{
					CheckMaximumLength(OriginInfo, value);
					origin = value;
				}
				if (!IsValidationSuspended)
				{
					Validation.ValidateOrigin();
				}
				OriginInfo.RefreshBinding();
			}
		}

		ZString origin;

		public ZPropertyInfo OriginInfo => GetZPropertyInfo(nameof(Origin));

		#endregion

		#region Destination

		[List(nameof(Lookups) + "." + nameof(RateEntryLookups.Locations))]
		[MaxLength(AutoRateEntry.Schema.TI_DestinationLRCMaxLength)]
		public ZString Destination
		{
			get { return destination; }
			set
			{
				if (destination != value)
				{
					CheckMaximumLength(DestinationInfo, value);
					destination = value;
				}
				if (!IsValidationSuspended)
				{
					Validation.ValidateDestination();
				}
				DestinationInfo.RefreshBinding();
			}
		}

		ZString destination;

		public ZPropertyInfo DestinationInfo => GetZPropertyInfo(nameof(Destination));

		#endregion

		#region Service Level

		[List(nameof(Lookups) + "." + nameof(RateEntryLookups.ServiceLevel_NIs))]
		[MaxLength(AutoRateEntry.Schema.TI_RS_NKServiceLevel_NIMaxLength)]
		public ZString ServiceLevel
		{
			get { return serviceLevel; }
			set
			{
				if (serviceLevel != value)
				{
					CheckMaximumLength(ServiceLevelInfo, value);
					serviceLevel = value;
				}
				if (!IsValidationSuspended)
				{
					Validation.ValidateServiceLevel();
				}
				ServiceLevelInfo.RefreshBinding();
			}
		}

		ZString serviceLevel;

		public ZPropertyInfo ServiceLevelInfo => GetZPropertyInfo(nameof(ServiceLevel));

		#endregion

		#region Carrier Service Level

		public OrgCarrierServiceLevelCollection CarrierServiceLevels
		{
			get
			{
				OrgCarrierServiceLevelCollection result = null;
				var carrierOrg = Factory.Load<OrgHeader>(Carrier);
				if (carrierOrg != null)
				{
					result = new OrgCarrierServiceLevelCollection(carrierOrg.MiscServ);
				}
				else
				{
					result = new OrgCarrierServiceLevelCollection(Factory);
				}
				result.Load();
				return result;
			}
		}

		[List(nameof(CarrierServiceLevels))]
		[MaxLength(AutoRateEntry.Schema.TI_PL_NKCarrierServiceLevelMaxLength)]
		public ZString CarrierServiceLevel
		{
			get { return carrierServiceLevel; }
			set
			{
				if (carrierServiceLevel != value)
				{
					CheckMaximumLength(CarrierServiceLevelInfo, value);
					carrierServiceLevel = value;
				}
				if (!IsValidationSuspended)
				{
					Validation.ValidateCarrierServiceLevel();
				}
				CarrierServiceLevelInfo.RefreshBinding();
			}
		}

		ZString carrierServiceLevel;

		public ZPropertyInfo CarrierServiceLevelInfo => GetZPropertyInfo(nameof(CarrierServiceLevel));

		#endregion

		#region Gateway Service Level

		[List(nameof(Lookups) + "." + nameof(RateEntryLookups.GatewayServiceLevels))]
		[MaxLength(AutoRateEntry.Schema.TI_RS_NKGatewayServiceLevelMaxLength)]
		public ZString GatewayServiceLevel
		{
			get { return gatewayServiceLevel; }
			set
			{
				if (gatewayServiceLevel != value)
				{
					CheckMaximumLength(GatewayServiceLevelInfo, value);
					gatewayServiceLevel = value;
				}
				if (!IsValidationSuspended)
				{
					Validation.ValidateGatewayServiceLevel();
				}
				GatewayServiceLevelInfo.RefreshBinding();
			}
		}

		ZString gatewayServiceLevel;

		public ZPropertyInfo GatewayServiceLevelInfo => GetZPropertyInfo(nameof(GatewayServiceLevel));

		#endregion

		#region Shipment Gateway Service Level

		[List(nameof(Lookups) + "." + nameof(RateEntryLookups.ShipmentGatewayServiceLevels))]
		[MaxLength(AutoRateEntry.Schema.TI_RS_NKShipmentGatewayServiceLevelMaxLength)]
		public ZString ShipmentGatewayServiceLevel
		{
			get { return shipmentGatewayServiceLevel; }
			set
			{
				if (shipmentGatewayServiceLevel != value)
				{
					CheckMaximumLength(ShipmentGatewayServiceLevelInfo, value);
					shipmentGatewayServiceLevel = value;
				}
				if (!IsValidationSuspended)
				{
					Validation.ValidateShipmentGatewayServiceLevel();
				}
				ShipmentGatewayServiceLevelInfo.RefreshBinding();
			}
		}

		ZString shipmentGatewayServiceLevel;

		public ZPropertyInfo ShipmentGatewayServiceLevelInfo => GetZPropertyInfo(nameof(ShipmentGatewayServiceLevel));

		#endregion

		#region Commodity Code

		[List(nameof(Lookups) + "." + nameof(RateEntryLookups.CommodityCodes))]
		[MaxLength(AutoRateEntry.Schema.TI_RH_NKCommodityCodeMaxLength)]
		public ZString CommodityCode
		{
			get { return commodityCode; }
			set
			{
				if (commodityCode != value)
				{
					CheckMaximumLength(CommodityCodeInfo, value);
					commodityCode = value;
				}
				if (!IsValidationSuspended)
				{
					Validation.ValidateCommodityCode();
				}
				CommodityCodeInfo.RefreshBinding();
			}
		}

		ZString commodityCode;

		public ZPropertyInfo CommodityCodeInfo => GetZPropertyInfo(nameof(CommodityCode));

		#endregion

		#region Container Types

		readonly ZQuery transportModeFilter = new ();

		/// <summary>
		/// Collection of containers that are applicable to the current transport mode.
		/// Lookups.Containers does not enforce TransportMode when applying validation, so we need a new collection that
		/// is restricted to the TransportMode to validate whether any selected containers are invalid for this mode.
		/// </summary>
		RefContainerCollection ModeRestrictedContainersForValidation
		{
			get { return new RefContainerCollection(Factory, transportModeFilter); }
		}

		public BulkRateUpdaterContainerTypeCollection ContainerTypes
		{
			get
			{
				if (containerTypes == null)
				{
					containerTypes = new BulkRateUpdaterContainerTypeCollection(Factory, Lookups, ModeRestrictedContainersForValidation);
					RegisterEditableChildObject(ContainerTypes);
				}
				return containerTypes;
			}
		}
		BulkRateUpdaterContainerTypeCollection containerTypes;

		#endregion

		#region Supplier

		[List(nameof(Lookups) + "." + nameof(RateEntryLookups.Suppliers))]
		public ZGuid Supplier
		{
			get { return supplier; }
			set
			{
				supplier = value;
				if (!IsValidationSuspended)
				{
					Validation.ValidateSupplier();
				}
				SupplierInfo.RefreshBinding();
			}
		}

		ZGuid supplier;

		public ZPropertyInfo SupplierInfo => GetZPropertyInfo(nameof(Supplier));

		#endregion

		#region Carrier

		[List(nameof(Lookups) + "." + nameof(RateEntryLookups.ShippingProviders))]
		public ZGuid Carrier
		{
			get { return carrier; }
			set
			{
				carrier = value;
				DummyEntry.TI_OH_Supplier = value;
				if (!IsValidationSuspended)
				{
					Validation.ValidateCarrier();
				}
				CarrierInfo.RefreshBinding();
			}
		}

		ZGuid carrier;

		public ZPropertyInfo CarrierInfo => GetZPropertyInfo(nameof(Carrier));

		#endregion

		#region StartDate

		public ZDate StartDate
		{
			get { return startDate; }
			set
			{
				startDate = value;
				if (!IsValidationSuspended)
				{
					Validation.ValidateStartDate();
				}
				StartDateInfo.RefreshBinding();
			}
		}

		ZDate startDate;

		public ZPropertyInfo StartDateInfo => GetZPropertyInfo(nameof(StartDate));

		#endregion

		#region EndDate

		public ZDate EndDate
		{
			get { return endDate; }
			set
			{
				endDate = value;
				if (!IsValidationSuspended)
				{
					Validation.ValidateEndDate();
				}
				EndDateInfo.RefreshBinding();
			}
		}

		ZDate endDate;

		public ZPropertyInfo EndDateInfo => GetZPropertyInfo(nameof(EndDate));

		#endregion

		#region No Expire Date

		public ZBool NoExpireDate
		{
			get { return noExpireDate; }
			set { SetNonPersistentPropertyValue(NoExpireDateInfo, ref noExpireDate, value); }
		}
		ZBool noExpireDate;

		public ZPropertyInfo NoExpireDateInfo => GetZPropertyInfo(nameof(NoExpireDate));

		#endregion

		#region Show Expired

		public ZBool ShowExpired
		{
			get { return showExpired; }
			set { SetNonPersistentPropertyValue(ShowExpiredInfo, ref showExpired, value); }
		}
		ZBool showExpired = true;

		public ZPropertyInfo ShowExpiredInfo => GetZPropertyInfo(nameof(ShowExpired));

		#endregion

		#region Contract Number

		[MaxLength(AutoRateEntry.Schema.TI_ContractNumberMaxLength)]
		public ZString ContractNumber
		{
			get { return contractNumber; }
			set { SetNonPersistentPropertyValue(ContractNumberInfo, ref contractNumber, value); }
		}
		ZString contractNumber;

		public ZPropertyInfo ContractNumberInfo => GetZPropertyInfo(nameof(ContractNumber));

		#endregion

		#region Contract Number Linked

		[List(nameof(ContractNumberLinkedValues), AllowOnlyTheseValues = true)]
		[MaxLength(3)]
		public ZString ContractNumberLinked
		{
			get { return contractNumberLinked; }
			set
			{
				if (contractNumberLinked != value)
				{
					CheckMaximumLength(ContractNumberLinkedInfo, value);
					contractNumberLinked = value;
				}
				if (!IsValidationSuspended)
				{
					Validation.ValidateContractNumberLinked();
				}
				ContractNumberLinkedInfo.RefreshBinding();
			}
		}
		ZString contractNumberLinked = YesNoEmptyList.Codes.Empty;

		public ZPropertyInfo ContractNumberLinkedInfo => GetZPropertyInfo(nameof(ContractNumberLinked));

		#endregion

		#region Gateway Agent Type

		[List(nameof(Lookups) + "." + nameof(RateEntryLookups.GatewayAgentTypes))]
		[MaxLength(AutoRateEntry.Schema.TI_GatewayAgentTypeMaxLength)]
		public ZString GatewayAgentType
		{
			get { return gatewayAgentType; }
			set
			{
				if (gatewayAgentType != value)
				{
					CheckMaximumLength(GatewayAgentTypeInfo, value);
					gatewayAgentType = value;
				}
				if (!IsValidationSuspended)
				{
					Validation.ValidateGatewayAgentType();
				}
				GatewayAgentTypeInfo.RefreshBinding();
			}
		}
		ZString gatewayAgentType;

		public ZPropertyInfo GatewayAgentTypeInfo => GetZPropertyInfo(nameof(GatewayAgentType));

		#endregion

		#region Shipment Consolidation Status

		[List(nameof(Lookups) + "." + nameof(RateEntryLookups.ShipmentConsolidationStatusList))]
		[MaxLength(AutoRateEntry.Schema.TI_ShipmentConsolidationStatus)]
		public ZString ShipmentConsolidationStatus
		{
			get { return shipmentConsolidationStatus; }
			set
			{
				if (shipmentConsolidationStatus != value)
				{
					CheckMaximumLength(ShipmentConsolidationStatusInfo, value);
					shipmentConsolidationStatus = value;
				}
				if (!IsValidationSuspended)
				{
					Validation.ValidateShipmentConsolidationStatus();
				}
				ShipmentConsolidationStatusInfo.RefreshBinding();
			}
		}
		ZString shipmentConsolidationStatus;

		public ZPropertyInfo ShipmentConsolidationStatusInfo => GetZPropertyInfo(nameof(ShipmentConsolidationStatus));

		#endregion

		#region Selected Client

		[List(nameof(Organisations))]
		public ZGuid SelectedClient
		{
			get { return selectedClient; }
			set
			{
				selectedClient = value;
				if (!IsValidationSuspended)
				{
					Validation.ValidateSelectedClient();
				}
				SelectedClientInfo.RefreshBinding();
			}
		}
		ZGuid selectedClient;

		public ZPropertyInfo SelectedClientInfo => GetZPropertyInfo(nameof(SelectedClient));

		#endregion

		#region Update Standard / Agent Rates Selection

		public RateLineItem.RateTypeToUpdate SelectedRatesTypeToUpdate { get; set; }

		public ZBool UpdateStandardRates
		{
			get { return updateStandardRates; }
			set
			{
				updateStandardRates = value;
				SelectedRatesTypeToUpdate = RateLineItem.RateTypeToUpdate.Standard;
			}
		}
		ZBool updateStandardRates;

		public ZBool UpdateAgentRates
		{
			get { return updateAgentRates; }
			set
			{
				updateAgentRates = value;
				SelectedRatesTypeToUpdate = RateLineItem.RateTypeToUpdate.Agent;
			}
		}
		ZBool updateAgentRates;

		public ZBool UpdateBothRates
		{
			get { return updateBothRates; }
			set
			{
				updateBothRates = value;
				SelectedRatesTypeToUpdate = RateLineItem.RateTypeToUpdate.StandardAndAgent;
			}
		}
		ZBool updateBothRates;

		#endregion

		#region ShowClientRates

		public ZBool ShowClientRates
		{
			get { return showClientRates; }
			set
			{
				showClientRates = value;
				if (!IsValidationSuspended)
				{
					Validation.ValidateShowClientRates();
				}
				ShowClientRatesInfo.RefreshBinding();
			}
		}

		ZBool showClientRates;

		public ZPropertyInfo ShowClientRatesInfo => GetZPropertyInfo(nameof(ShowClientRates));

		#endregion

		#region ShowIntercompanyTariffs

		public ZBool ShowIntercompanyTariffs
		{
			get { return showIntercompanyTariffs; }
			set
			{
				showIntercompanyTariffs = value;
				if (!IsValidationSuspended)
				{
					Validation.ValidateShowIntercompanyTariffs();
				}
				ShowIntercompanyTariffsInfo.RefreshBinding();
			}
		}
		ZBool showIntercompanyTariffs;

		public ZPropertyInfo ShowIntercompanyTariffsInfo => GetZPropertyInfo(nameof(ShowIntercompanyTariffs));

		#endregion

		#region ShowActiveQuotes

		public ZBool ShowActiveQuotes
		{
			get { return showActiveQuotes; }
			set
			{
				showActiveQuotes = value;
				if (!IsValidationSuspended)
				{
					Validation.ValidateShowActiveQuotes();
				}
				ShowActiveQuotesInfo.RefreshBinding();
			}
		}

		ZBool showActiveQuotes;

		public ZPropertyInfo ShowActiveQuotesInfo => GetZPropertyInfo(nameof(ShowActiveQuotes));

		#endregion

		#region ShowCostings

		public ZBool ShowCostings
		{
			get { return showCostings; }
			set
			{
				showCostings = value;
				if (!IsValidationSuspended)
				{
					Validation.ValidateShowCostings();
				}
				ShowCostingsInfo.RefreshBinding();
			}
		}

		ZBool showCostings;

		public ZPropertyInfo ShowCostingsInfo => GetZPropertyInfo(nameof(ShowCostings));

		#endregion

		#region ShowCompanyTariffs

		public ZBool ShowCompanyTariffs
		{
			get { return showCompanyTariffs; }
			set
			{
				showCompanyTariffs = value;
				if (!IsValidationSuspended)
				{
					Validation.ValidateShowCompanyTariffs();
				}
				ShowCompanyTariffsInfo.RefreshBinding();
			}
		}

		ZBool showCompanyTariffs;

		public ZPropertyInfo ShowCompanyTariffsInfo => GetZPropertyInfo(nameof(ShowCompanyTariffs));

		#endregion

		#region IsNonIntercompanyTariffsModuleSelected

		internal bool IsNonIntercompanyTariffsModuleSelected => (ShowClientRates || ShowActiveQuotes || ShowCostings || ShowCompanyTariffs);

		#endregion

		#region ActionsLine

		public RateLine ActionsLine
		{
			get
			{
				if (actionsLine == null)
				{
					actionsLine = DummyEntry.RateLines.AddNew();
					actionsLine.IsBulkRateUpdateActionLine = true;
				}

				return actionsLine;
			}
		}

		RateLine actionsLine;

		#endregion

		#region Action

		public enum Actions
		{
			AddOrReplaceCharge,
			ReplaceCharge,
			IncreaseDecreaseCharge,
			DeleteCharge
		}

		public Actions Action
		{
			get { return action; }
			set
			{
				if (action != value)
				{
					action = value;
					ActionsLine.SetReadOnlyExcludingChargeCode(action == Actions.DeleteCharge || action == Actions.IncreaseDecreaseCharge);
					ActionsLine.LockCalculator = action == Actions.DeleteCharge || action == Actions.IncreaseDecreaseCharge;
					if (action == Actions.IncreaseDecreaseCharge)
					{
						ActionsLine.TL_RateCalculator = CompanyTariffOrCostBasedCalculator.CostBasedCode;
						ActionsLine.ViewAgentRates = false;
					}
					else if (action == Actions.DeleteCharge)
					{
						using (ActionsLine.GetValidationSuspender())
						{
							ActionsLine.TL_RateCalculator = ZString.Empty;
						}
					}
					else if (ActionsLine.ChargeCode != null && ActionsLine.ChargeCode.AC_RateCalculator != ActionsLine.TL_RateCalculator)
					{
						ActionsLine.RateLineItems.SetReadOnlyIncludingChildren(false);
						ActionsLine.TL_RateCalculator = ActionsLine.ChargeCode.AC_RateCalculator;
					}

					ActionsLine.InitializeCalculator();
					ActionsLine.RefreshBinding();
				}
			}
		}

		Actions action;

		#region AddOrReplaceCharge

		[BusinessObjectTestExclude]
		public ZBool AddOrReplaceCharge
		{
			get { return Action == Actions.AddOrReplaceCharge; }
			set
			{
				if (value)
				{
					Action = Actions.AddOrReplaceCharge;
					RefreshBindingForAction();
				}
				AddOrReplaceChargeInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo AddOrReplaceChargeInfo => GetZPropertyInfo(nameof(AddOrReplaceCharge));

		#endregion

		#region ReplaceCharge

		[BusinessObjectTestExclude]
		public ZBool ReplaceCharge
		{
			get { return Action == Actions.ReplaceCharge; }
			set
			{
				if (value)
				{
					Action = Actions.ReplaceCharge;
					RefreshBindingForAction();
				}
			}
		}

		public ZPropertyInfo ReplaceChargeInfo => GetZPropertyInfo(nameof(ReplaceCharge));

		#endregion

		#region IncreaseDecreaseCharge

		[BusinessObjectTestExclude]
		public ZBool IncreaseDecreaseCharge
		{
			get { return Action == Actions.IncreaseDecreaseCharge; }
			set
			{
				if (value)
				{
					Action = Actions.IncreaseDecreaseCharge;
					RefreshBindingForAction();
				}
			}
		}

		public ZPropertyInfo IncreaseDecreaseChargeInfo => GetZPropertyInfo(nameof(IncreaseDecreaseCharge));

		#endregion

		#region DeleteCharge

		[BusinessObjectTestExclude]
		public ZBool DeleteCharge
		{
			get { return Action == Actions.DeleteCharge; }
			set
			{
				if (value)
				{
					Action = Actions.DeleteCharge;
					RefreshBindingForAction();
				}
			}
		}

		public ZPropertyInfo DeleteChargeInfo => GetZPropertyInfo(nameof(DeleteCharge));

		#endregion

		void RefreshBindingForAction()
		{
			AddOrReplaceChargeInfo.RefreshBinding();
			ReplaceChargeInfo.RefreshBinding();
			IncreaseDecreaseChargeInfo.RefreshBinding();
			DeleteChargeInfo.RefreshBinding();
		}

		#endregion

		#region CreateNewEntry

		public ZBool CreateNewEntry
		{
			get { return createNewEntry; }
			set
			{
				createNewEntry = value;
				if (!IsValidationSuspended)
				{
					Validation.ValidateCreateNewEntry();
				}
				CreateNewEntryInfo.RefreshBinding();
			}
		}

		ZBool createNewEntry;

		public ZPropertyInfo CreateNewEntryInfo => GetZPropertyInfo(nameof(CreateNewEntry));

		#endregion

		#region NewEntryStartDate

		public ZDate NewEntryStartDate
		{
			get { return newEntryStartDate; }
			set
			{
				newEntryStartDate = value;
				if (!IsValidationSuspended)
				{
					Validation.ValidateNewEntryStartDate();
				}
				NewEntryStartDateInfo.RefreshBinding();
			}
		}

		ZDate newEntryStartDate;

		public ZPropertyInfo NewEntryStartDateInfo => GetZPropertyInfo(nameof(NewEntryStartDate));

		#endregion

		#region NewEntryEndDate

		public ZDate NewEntryEndDate
		{
			get { return newEntryEndDate; }
			set
			{
				newEntryEndDate = value;
				if (!IsValidationSuspended)
				{
					Validation.ValidateNewEntryEndDate();
				}
				NewEntryEndDateInfo.RefreshBinding();
			}
		}

		ZDate newEntryEndDate;

		public ZPropertyInfo NewEntryEndDateInfo => GetZPropertyInfo(nameof(NewEntryEndDate));

		#endregion

		#endregion

		#region Lookups

		public RateEntryLookups Lookups => lookups ?? (lookups = new RateEntryLookups(DummyEntry));
		RateEntryLookups lookups;

		public CodeDescriptionPairList Types
		{
			get
			{
				if (fTypes == null)
				{
					fTypes = new Dictionary<ZString, CodeDescriptionPairList>();
				}

				CodeDescriptionPairList list = null;
				if (fTypes.ContainsKey(Module))
				{
					list = fTypes[Module];
				}
				else
				{
					list = new CodeDescriptionPairList(OLookUpEditType.CustomType);
					switch (Module)
					{
						case "FWD":
							AddRateCategory(list, RatingConstants.RateCategory.AIR);
							AddRateCategory(list, RatingConstants.RateCategory.FCL);
							AddRateCategory(list, RatingConstants.RateCategory.LCL, Res.GetString("c197b770-16d1-4e80-b0da-8e1bc309d46d", "LCL/FTL/LTL/COU Freight"));
							AddRateCategory(list, RatingConstants.RateCategory.ORG);
							AddRateCategory(list, RatingConstants.RateCategory.DST);
							break;
						case "SHP":
							AddRateCategory(list, RatingConstants.RateCategory.SOR);
							AddRateCategory(list, RatingConstants.RateCategory.SDE);
							AddRateCategory(list, RatingConstants.RateCategory.SCO);
							AddRateCategory(list, RatingConstants.RateCategory.SNC);
							AddRateCategory(list, RatingConstants.RateCategory.SED);
							AddRateCategory(list, RatingConstants.RateCategory.SID);
							break;
						case "CFS":
							AddRateCategory(list, RatingConstants.RateCategory.PAC);
							AddRateCategory(list, RatingConstants.RateCategory.UNP);
							AddRateCategory(list, RatingConstants.RateCategory.CST);
							break;
						case "TRT":
							AddRateCategory(list, RatingConstants.RateCategory.TRN);
							AddRateCategory(list, RatingConstants.RateCategory.TBC);
							break;
						case "WRH":
							AddRateCategory(list, RatingConstants.RateCategory.WHS);
							AddRateCategory(list, RatingConstants.RateCategory.TRW);
							AddRateCategory(list, RatingConstants.RateCategory.TWU);
							AddRateCategory(list, RatingConstants.RateCategory.CYD);
							AddRateCategory(list, RatingConstants.RateCategory.CYU);
							AddRateCategory(list, RatingConstants.RateCategory.CYM);
							break;
						case "CUS":
							AddRateCategory(list, RatingConstants.RateCategory.CAI);
							AddRateCategory(list, RatingConstants.RateCategory.CFC);
							AddRateCategory(list, RatingConstants.RateCategory.CLC, Res.GetString("F42C2D0A-0D0F-4C95-AA36-86BD9CD03E91", "Customs LCL/FTL/LTL Freight"));
							AddRateCategory(list, RatingConstants.RateCategory.COR);
							AddRateCategory(list, RatingConstants.RateCategory.CDS);
							break;
					}

					if (list.Count > 0)
					{
						fTypes.Add(Module, list);
					}
				}

				return list;
			}
		}

		void AddRateCategory(CodeDescriptionPairList list, ZString code, ZString description)
			=> list.AddPair(code, description);

		void AddRateCategory(CodeDescriptionPairList list, ZString code)
			=> AddRateCategory(list, code, CompanyTariffCodes.GetCompanyTariffDiscountDescription(code));

		CompanyTariffCodes CompanyTariffCodes => companyTariffCodes ?? (companyTariffCodes = new CompanyTariffCodes());
		CompanyTariffCodes companyTariffCodes;

		Dictionary<ZString, CodeDescriptionPairList> fTypes;

		public CodeDescriptionPairList Modules
		{
			get
			{
				if (modules == null)
				{
					modules = new CodeDescriptionPairList();
					modules.AddPair("FWD", Res.GetString("bf8fd3bc-6f80-4731-a6f0-d7c35268db91", "Forwarding"));
					modules.AddPair("SHP", Res.GetString("253779f7-a2c7-45e5-a1f2-54e8f74fd6ca", "Liner & Agency"));
					modules.AddPair("CFS", Res.GetString("c97f4180-267c-4ad0-9208-40e8934fc32f", "CFS"));
					modules.AddPair("TRT", Res.GetString("4c7e055a-092e-40f1-8796-27ff3587817f", "Transport Charges"));
					modules.AddPair("WRH", Res.GetString("5c772199-009b-43f0-b7ec-8408a77c42b3", "Warehouse"));
					modules.AddPair("CUS", Res.GetString("009C6167-42BE-454A-A312-988303B2CC93", "Customs"));
				}

				return modules;
			}
		}

		CodeDescriptionPairList modules;

		public CodeDescriptionPairList Modes
		{
			get
			{
				if (modes == null)
				{
					modes = new Dictionary<ZString, CodeDescriptionPairList>();
				}

				CodeDescriptionPairList list;
				if (!modes.ContainsKey(Type))
				{
					list = Type.IsEmpty
						? new CodeDescriptionPairList(OLookUpEditType.CustomType)
						: RateEntryLookups.GetTransportModesByRateCategory(Type);

					if (list.Count > 0)
					{
						modes.Add(Type, list);
					}
				}
				else
				{
					list = modes[Type];
				}

				return list;
			}
		}
		Dictionary<ZString, CodeDescriptionPairList> modes;

		public CodeDescriptionPairList ContractNumberLinkedValues => new YesNoEmptyList();

		public OrgHeaderCollection Organisations => organisations ?? (organisations = new OrgHeaderCollection(Factory));
		OrgHeaderCollection organisations;

		#endregion

		// called by BulkUpdateActionsPage.ValidatePage
		protected override void RunPreSaveValidationCore()
		{
			base.RunPreSaveValidationCore();
			Validation.ValidateAll();
		}

		#region Security

		public bool ClientRatesBulkUpdateAllowed
		{
			get
			{
				if (!ClientRatesBulkUpdateRetrieved)
				{
					ClientRatesBulkUpdateRetrieved = true;
					clientRatesBulkUpdateAllowed = Env.Security.ClientRatesBulkUpdate.IsAllowed;
				}

				return clientRatesBulkUpdateAllowed;
			}
		}

		bool clientRatesBulkUpdateAllowed;
		bool ClientRatesBulkUpdateRetrieved;

		public bool IntercompanyTariffsBulkUpdateAllowed
		{
			get
			{
				if (!IntercompanyTariffsBulkUpdateRetrieved)
				{
					IntercompanyTariffsBulkUpdateRetrieved = true;
					intercompanyTariffsBulkUpdateAllowed = Env.Security.IntercompanyTariffsBulkUpdate.IsAllowed;
				}

				return intercompanyTariffsBulkUpdateAllowed;
			}
		}
		bool intercompanyTariffsBulkUpdateAllowed;
		bool IntercompanyTariffsBulkUpdateRetrieved;

		public bool CostingRatesBulkUpdateAllowed
		{
			get
			{
				if (!CostingRatesBulkUpdateRetrieved)
				{
					CostingRatesBulkUpdateRetrieved = true;
					costingRatesBulkUpdateAllowed = Env.Security.CostingRatesBulkUpdate.IsAllowed;
				}

				return costingRatesBulkUpdateAllowed;
			}
		}

		bool costingRatesBulkUpdateAllowed;
		bool CostingRatesBulkUpdateRetrieved;

		public bool QuotationBulkUpdateAllowed
		{
			get
			{
				if (!QuotationBulkUpdateRetrieved)
				{
					QuotationBulkUpdateRetrieved = true;
					quotationBulkUpdateAllowed = Env.Security.QuotationBulkUpdate.IsAllowed;
				}

				return quotationBulkUpdateAllowed;
			}
		}

		bool quotationBulkUpdateAllowed;
		bool QuotationBulkUpdateRetrieved;

		public bool CompanyTariffBulkUpdateAllowed
		{
			get
			{
				if (!CompanyTariffBulkUpdateRetrieved)
				{
					CompanyTariffBulkUpdateRetrieved = true;
					companyTariffBulkUpdateAllowed = Env.Security.CompanyTariffRatesBulkUpdate.IsAllowed;
				}

				return companyTariffBulkUpdateAllowed;
			}
		}

		bool companyTariffBulkUpdateAllowed;
		bool CompanyTariffBulkUpdateRetrieved;

		#endregion

		#region Implementation

		#region DummyEntry

		RateEntry DummyEntry
		{
			get
			{
				if (dummyEntry == null)
				{
					RatingHeader dummyRate;

					if (ShowIntercompanyTariffs)
					{
						dummyRate = new BusinessObjectFactory().New<IntercompanyTariff>();
					}
					else
					{
						dummyRate = new BusinessObjectFactory().New<ClientRate>();
					}

					dummyEntry = dummyRate.EntryCollections[RatingConstants.RateCategory.ORG].LazyLoadingCollection.AddNew();
				}

				return dummyEntry;
			}
		}

		RateEntry dummyEntry;

		internal void UpdateDummyEntry()
		{
			if (dummyEntry != null && (ShowIntercompanyTariffs ^ DummyEntry.IsIntercompanyTariff()))
			{
				dummyEntry = null;
				actionsLine = null;
			}
		}

		bool IsContainerised
		{
			get
			{
				DummyEntry.TI_Mode = Mode;

				return DummyEntry.IsFCL() || DummyEntry.IsULD();
			}
		}

		#endregion

		#region Internal Classes

		public class ProcessingProgressedEventArgs : EventArgs
		{
			public ProcessingProgressedEventArgs(int percentComplete)
			{
				PercentComplete = percentComplete;
			}

			public int PercentComplete { get; }
		}

		#endregion

		#endregion

		public BulkRateUpdaterValidation Validation => new BulkRateUpdaterValidation(this);
	}
}
