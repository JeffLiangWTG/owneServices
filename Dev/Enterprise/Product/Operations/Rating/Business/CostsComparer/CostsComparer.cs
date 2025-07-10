using System;
using System.Collections.Generic;
using System.Text;
using CargoWise.ComponentModel;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.DocumentEngineIntegration;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Core.Constants;

namespace Enterprise.Rating.Business
{
	public class CostsComparer : NonPersistentBusinessObject,
		IObsoleteValidation,
		IDocumentSupportable,
		IVisualizerNoteSupporter
	{
		public CostsComparer()
			: base(new BusinessObjectFactory())
		{
		}

		#region Default Values

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			ValidFromDate = ZDateTime.Today;
			ShowOriginDestination = ZBool.True;
			Currency = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;
		}

		#endregion

		#region Has Changes

		public override bool HasChanges
		{
			get { return false; }   //Should not have any changes to affect document printing
			set { base.HasChanges = value; }
		}

		#endregion

		#region Properties

		#region Mode

		[List("TransportModes")]
		[MaxLength(AutoRateEntry.Schema.TI_ModeMaxLength)]
		public ZString Mode
		{
			get { return fMode; }
			set
			{
				if (fMode != value)
				{
					CheckMaximumLength(ModeInfo, value);
					fMode = value;
				}
				if (!IsValidationSuspended)
				{
					ValidateMode();
				}
				ModeInfo.RefreshBinding();
			}
		}

		ZString fMode;

		public ZPropertyInfo ModeInfo
		{
			get { return GetZPropertyInfo(nameof(Mode)); }
		}

		public void ValidateMode()
		{
			ModeInfo.ClearAllNotifications();
			if (Mode.IsEmpty)
			{
				MandatoryValidation.CheckEntered(ModeInfo);
			}
			else
			{
				ListValidation.ErrorIfInvalidCode(ModeInfo);
			}
		}

		#endregion

		#region Origin

		[List("Lookups.Locations")]
		[MaxLength(AutoRateEntry.Schema.TI_OriginLRCMaxLength)]
		public ZString Origin
		{
			get { return fOrigin; }
			set
			{
				if (fOrigin != value)
				{
					CheckMaximumLength(OriginInfo, value);
					fOrigin = value;
				}
				if (!IsValidationSuspended)
				{
					ValidateOrigin();
				}
				OriginInfo.RefreshBinding();
			}
		}

		ZString fOrigin;

		public ZPropertyInfo OriginInfo
		{
			get { return GetZPropertyInfo(nameof(Origin)); }
		}

		public void ValidateOrigin()
		{
			OriginInfo.ClearAllNotifications();
			if (ShowOriginChargesOnly)
			{
				MandatoryValidation.CheckEntered(OriginInfo);
			}
			if (!Origin.IsEmpty)
			{
				ListValidation.ErrorIfInvalidCode(OriginInfo, Lookups.Locations);
			}
		}

		#endregion

		#region Destination

		[List("Lookups.Locations")]
		[MaxLength(AutoRateEntry.Schema.TI_DestinationLRCMaxLength)]
		public ZString Destination
		{
			get { return fDestination; }
			set
			{
				if (fDestination != value)
				{
					CheckMaximumLength(DestinationInfo, value);
					fDestination = value;
				}
				if (!IsValidationSuspended)
				{
					ValidateDestination();
				}
				DestinationInfo.RefreshBinding();
			}
		}

		ZString fDestination;

		public ZPropertyInfo DestinationInfo
		{
			get { return GetZPropertyInfo(nameof(Destination)); }
		}

		public void ValidateDestination()
		{
			DestinationInfo.ClearAllNotifications();
			if (ShowDestinationChargesOnly)
			{
				MandatoryValidation.CheckEntered(DestinationInfo);
			}
			if (!Destination.IsEmpty)
			{
				ListValidation.ErrorIfInvalidCode(DestinationInfo, Lookups.Locations);
			}
		}

		#endregion

		#region Commodity Code

		[List("Lookups.CommodityCodes")]
		[MaxLength(AutoRateEntry.Schema.TI_RH_NKCommodityCodeMaxLength)]
		public ZString CommodityCode
		{
			get { return fCommodityCode; }
			set
			{
				if (fCommodityCode != value)
				{
					CheckMaximumLength(CommodityCodeInfo, value);
					fCommodityCode = value;
				}
				if (!IsValidationSuspended)
				{
					ValidateCommodityCode();
				}
				CommodityCodeInfo.RefreshBinding();
			}
		}

		ZString fCommodityCode;

		public ZPropertyInfo CommodityCodeInfo
		{
			get { return GetZPropertyInfo(nameof(CommodityCode)); }
		}

		public void ValidateCommodityCode()
		{
			CommodityCodeInfo.ClearAllNotifications();
			ListValidation.ErrorIfInvalidCode(CommodityCodeInfo, Lookups.CommodityCodes);
		}

		#endregion

		#region Container Type

		[List("Lookups.Containers")]
		public ZGuid ContainerType
		{
			get { return fContainerType; }
			set
			{
				fContainerType = value;
				if (!IsValidationSuspended)
				{
					ValidateContainerType();
				}
				ContainerTypeInfo.RefreshBinding();
				ContainerFreightRatingClassInfo.RefreshBinding();
				ContainerHandlingRatingClassInfo.RefreshBinding();
			}
		}

		ZGuid fContainerType;

		public ZPropertyInfo ContainerTypeInfo
		{
			get { return GetZPropertyInfo(nameof(ContainerType)); }
		}

		protected bool ContainerType_ReadOnly
		{
			get
			{
				DummyEntry.TI_Mode = Mode;

				return !DummyEntry.IsFCL() && !DummyEntry.IsULD();
			}
		}

		public void ValidateContainerType()
		{
			ContainerTypeInfo.ClearAllNotifications();
			TypeValidation.CheckValidGuid(ContainerTypeInfo);
		}

		public RefContainer Container
		{
			get
			{
				if (ContainerType.IsEmpty || ContainerTypeInfo.HasErrors())
				{
					refContainer = null;
				}
				else if (refContainer == null || refContainer.PK != ContainerType)
				{
					refContainer = Lookups.Containers.FindByPK(fContainerType) as RefContainer;
				}

				return refContainer;
			}
		}

		RefContainer refContainer;

		#endregion

		#region Container classes

		const string HYPHEN = " - ";

		public ZString ContainerFreightRatingClass
		{
			get
			{
				if (Container == null)
				{
					return ZString.Empty;
				}

				var rateClass = Container.RC_FreightRateClass;
				var stringBuilder = new ZStringBuilder(rateClass);
				var freightRateClass = Container.Lookups.FreightRateClassList[rateClass, StringComparison.OrdinalIgnoreCase];
				if (freightRateClass != null && !string.IsNullOrWhiteSpace(freightRateClass.Description))
				{
					stringBuilder.Append(HYPHEN);
					stringBuilder.Append(freightRateClass.Description);
				}
				return stringBuilder.ToString();
			}
		}

		public ZPropertyInfo ContainerFreightRatingClassInfo => GetZPropertyInfo(nameof(ContainerFreightRatingClass));

		protected bool ContainerFreightRatingClass_ReadOnly => true;

		public ZString ContainerHandlingRatingClass
		{
			get
			{
				if (Container == null)
				{
					return ZString.Empty;
				}

				var rateClass = Container.RC_HandlingRateClass;
				var stringBuilder = new StringBuilder(rateClass);
				var handlingRateClass = refContainer.Lookups.HandlingRateClassList[rateClass, StringComparison.OrdinalIgnoreCase];
				if (handlingRateClass != null && !string.IsNullOrWhiteSpace(handlingRateClass.Description))
				{
					stringBuilder.Append(HYPHEN);
					stringBuilder.Append(handlingRateClass.Description);
				}
				return stringBuilder.ToString();
			}
		}

		public ZPropertyInfo ContainerHandlingRatingClassInfo => GetZPropertyInfo(nameof(ContainerHandlingRatingClass));

		protected bool ContainerHandlingRatingClass_ReadOnly => true;

		#endregion

		#region Date

		#region Valid From Date

		public ZDateTime ValidFromDate
		{
			get { return validFromDate; }
			set
			{
				SetNonPersistentPropertyValue(ValidFromDateInfo, ref validFromDate, value);
				ValidateValidFromDate();
			}
		}
		ZDateTime validFromDate;

		public ZPropertyInfo ValidFromDateInfo
		{
			get { return GetZPropertyInfo(nameof(ValidFromDate)); }
		}

		public void ValidateValidFromDate()
		{
			ValidFromDateInfo.ClearAllNotifications();
			TypeValidation.CheckValidZDateTimeAndRange(ValidFromDateInfo);
		}

		#endregion

		#region Valid To Date

		public ZDateTime ValidToDate
		{
			get { return validToDate; }
			set
			{
				SetNonPersistentPropertyValue(ValidToDateInfo, ref validToDate, value);
				ValidateValidToDate();
			}
		}
		ZDateTime validToDate;

		public ZPropertyInfo ValidToDateInfo
		{
			get { return GetZPropertyInfo(nameof(ValidToDate)); }
		}

		public void ValidateValidToDate()
		{
			ValidToDateInfo.ClearAllNotifications();
			TypeValidation.CheckValidZDateTimeAndRange(ValidToDateInfo);
		}

		#endregion

		#endregion

		#region ShowOriginDestination

		public ZBool ShowOriginDestination
		{
			get { return fShowOriginDestination; }
			set
			{
				fShowOriginDestination = value;
				if (!IsValidationSuspended)
				{
					ValidateShowOriginDestination();
				}
				ShowOriginDestinationInfo.RefreshBinding();
			}
		}

		ZBool fShowOriginDestination;

		public ZPropertyInfo ShowOriginDestinationInfo
		{
			get { return GetZPropertyInfo(nameof(ShowOriginDestination)); }
		}

		protected bool ShowOriginDestination_ReadOnly
		{
			get { return !ShowAllCharges; }
		}

		public void ValidateShowOriginDestination()
		{
			ShowOriginDestinationInfo.ClearAllNotifications();
		}

		#endregion

		#region Currency

		[List("Lookups.Currencies")]
		[MaxLength(AutoRateEntry.Schema.TI_RX_NKCurrencyMaxLength)]
		public ZString Currency
		{
			get { return fCurrency; }
			set
			{
				if (fCurrency != value)
				{
					CheckMaximumLength(CurrencyInfo, value);
					fCurrency = value;
				}
				if (!IsValidationSuspended)
				{
					ValidateCurrency();
				}
				CurrencyInfo.RefreshBinding();
			}
		}

		ZString fCurrency;

		public ZPropertyInfo CurrencyInfo
		{
			get { return GetZPropertyInfo(nameof(Currency)); }
		}

		public void ValidateCurrency()
		{
			CurrencyInfo.ClearAllNotifications();
			if (Currency.IsEmpty)
			{
				MandatoryValidation.CheckEntered(CurrencyInfo);
			}
			else
			{
				ListValidation.ErrorIfInvalidCode(CurrencyInfo, Lookups.Currencies);
			}

			if (CurrencyObj != null)
			{
				var exchangeRate = CurrencyConverter.GetExchangeRate(CurrencyObj);
				if (exchangeRate.IsEmpty)
				{
					CurrencyInfo.AddError(Res.GetString("547eb3ab-10ae-4d6f-ac74-7b26d36697cd", "{0} has no current Buy exchange rate to local currency.", Currency));
				}
			}
		}

		public RefCurrency CurrencyObj
		{
			get { return Factory.LoadFromNaturalKey<RefCurrency>(RefCurrencySchema.RX_Code, Currency); }
		}

		#endregion

		#region ShowAllCharges

		public ZBool ShowAllCharges
		{
			get { return showAllCharges; }
			set
			{
				showAllCharges = value;
				ShowAllChargesInfo.RefreshBinding();
			}
		}

		ZBool showAllCharges = ZBool.True;

		public ZPropertyInfo ShowAllChargesInfo
		{
			get { return GetZPropertyInfo(nameof(ShowAllCharges)); }
		}

		#endregion

		#region ShowOriginChargesOnly

		public ZBool ShowOriginChargesOnly
		{
			get { return showOriginChargesOnly; }
			set
			{
				showOriginChargesOnly = value;
				ShowOriginChargesOnlyInfo.RefreshBinding();
			}
		}

		ZBool showOriginChargesOnly;

		public ZPropertyInfo ShowOriginChargesOnlyInfo
		{
			get { return GetZPropertyInfo(nameof(ShowOriginChargesOnly)); }
		}

		#endregion

		#region ShowDestinationChargesOnly

		public ZBool ShowDestinationChargesOnly
		{
			get { return showDestinationChargesOnly; }
			set
			{
				showDestinationChargesOnly = value;
				ShowDestinationChargesOnlyInfo.RefreshBinding();
			}
		}

		ZBool showDestinationChargesOnly;

		public ZPropertyInfo ShowDestinationChargesOnlyInfo
		{
			get { return GetZPropertyInfo(nameof(ShowDestinationChargesOnly)); }
		}

		#endregion

		#region SingleChargeCodeComparisonOnly

		public ZBool SingleChargeCodeComparisonOnly
		{
			get { return fSingleChargeCodeComparisonOnly; }
			set
			{
				fSingleChargeCodeComparisonOnly = value;
				SingleChargeCodeComparisonOnlyInfo.RefreshBinding();
			}
		}

		ZBool fSingleChargeCodeComparisonOnly;

		public ZPropertyInfo SingleChargeCodeComparisonOnlyInfo
		{
			get { return GetZPropertyInfo(nameof(SingleChargeCodeComparisonOnly)); }
		}

		#endregion

		#region ChargeCodePK

		[List("ChargeCodes")]
		public ZGuid ChargeCodePK
		{
			get { return fChargeCodePK; }
			set
			{
				fChargeCodePK = value;
				if (!IsValidationSuspended)
				{
					ValidateChargeCodePK();
				}
				ChargeCodePKInfo.RefreshBinding();
			}
		}

		ZGuid fChargeCodePK;

		public ZPropertyInfo ChargeCodePKInfo
		{
			get { return GetZPropertyInfo(nameof(ChargeCodePK)); }
		}

		protected bool ChargeCodePK_ReadOnly
		{
			get { return !SingleChargeCodeComparisonOnly; }
		}

		public void ValidateChargeCodePK()
		{
			ChargeCodePKInfo.ClearAllNotifications();

			if (SingleChargeCodeComparisonOnly)
			{
				MandatoryValidation.CheckEntered(ChargeCodePKInfo);
				if (!ChargeCodePK.IsEmpty)
				{
					ListValidation.ErrorIfInvalidPK(ChargeCodePKInfo, ChargeCodes);
				}
			}
		}

		#endregion

		#region Contract Number

		[MaxLength(AutoRateEntry.Schema.TI_ContractNumberMaxLength)]
		public ZString ContractNumber
		{
			get { return contractNumber; }
			set { SetNonPersistentPropertyValue(ContractNumberInfo, ref contractNumber, value); }
		}
		ZString contractNumber;

		public ZPropertyInfo ContractNumberInfo
		{
			get { return GetZPropertyInfo(nameof(ContractNumber)); }
		}

		#endregion

		#endregion

		#region Lookups

		public RateEntryLookups Lookups
		{
			get { return fLookups ?? (fLookups = new RateEntryLookups(DummyEntry)); }
		}

		RateEntryLookups fLookups;

		public CodeDescriptionPairList TransportModes
		{
			get
			{
				if (fTransportModes == null)
				{
					fTransportModes = new CodeDescriptionPairList();
					fTransportModes.AddRange(Lookups.TransportModes);
					fTransportModes.RemoveCode(RateMode.ALL);
					fTransportModes.RemoveCode(RateMode.AIR);
					fTransportModes.RemoveCode(RateMode.SEA);
					fTransportModes.RemoveCode(RateMode.ROA);
					fTransportModes.RemoveCode(RateMode.RAI);
					fTransportModes.RemoveCode(RateMode.MAI);
				}

				return fTransportModes;
			}
		}

		CodeDescriptionPairList fTransportModes;

		public AccChargeCodeCollection ChargeCodes
		{
			get
			{
				if (fChargeCodes == null)
				{
					fChargeCodes = new AccChargeCodeCollection(Factory);
				}

				return fChargeCodes;
			}
		}

		AccChargeCodeCollection fChargeCodes;

		#endregion

		#region Validation

		protected override void RunPreSaveValidationCore()
		{
			base.RunPreSaveValidationCore();
			ValidateMode();
			ValidateOrigin();
			ValidateDestination();
			ValidateCommodityCode();
			ValidateContainerType();
			ValidateCurrency();
			ValidateChargeCodePK();
			ValidateValidFromDate();
			ValidateValidToDate();
		}

		#endregion

		#region Costs

		public virtual CostsComparerEntryCollection Costs
		{
			get
			{
				if (fCosts == null)
				{
					fCosts = new CostsComparerEntryCollection(this, Factory);
				}

				return fCosts;
			}
		}

		CostsComparerEntryCollection fCosts;

		public virtual void LoadCosts()
		{
			Costs.RemoveAll();
			Costs.Load(Query);
			ResetSummaryItems();
			ResetSummaryColumns?.Invoke(SummaryItems);
		}

		#region Query

		ZDBOnlyQuery Query
		{
			get
			{
				var entryQuery = new ZDBOnlyQuery(typeof(RateEntry));
				entryQuery.AddToFilter(ModeFilter);

				if (!Origin.IsEmpty)
				{
					entryQuery.AddToFilter(GetLocationQuery(Origin, RateEntrySchema.TI_OriginLRC), JoinCondition.And);
				}

				if (!Destination.IsEmpty)
				{
					entryQuery.AddToFilter(GetLocationQuery(Destination, RateEntrySchema.TI_DestinationLRC), JoinCondition.And);
				}

				if (!CommodityCode.IsEmpty)
				{
					entryQuery.AddToFilter(RateEntrySchema.TI_RH_NKCommodityCode, CommodityCode);
				}

				if (!ContainerType.IsEmpty)
				{
					entryQuery.AddToFilter(ContainerQuery);
				}

				if (!ContractNumber.IsEmpty)
				{
					entryQuery.AddToFilter(RateEntrySchema.TI_ContractNumber, SQLComparisonOperator.StartsWith, ContractNumber);
				}

				entryQuery.AddToFilter(DateFilter);

				var headerQuery = new ZDBOnlySubQuery(typeof(RatingHeader), RateEntrySchema.TI_TH);
				headerQuery.AddToFilter(RatingHeaderSchema.TH_GC, GlbCompany.CurrentCompany.PK);
				headerQuery.AddToFilter(RatingHeaderSchema.TH_RateType, SQLComparisonOperator.Equal, RatingConstants.RatingHeaderTypes.Costing);
				entryQuery.AddSubQuery(headerQuery, JoinCondition.And);

				if (ShowAllCharges || SingleChargeCodeComparisonOnly)
				{
					var rateLineQuery = new ZDBOnlySubQuery(typeof(RateLine), RateEntrySchema.PK);
					rateLineQuery.AddToFilter(RateLinesSchema.TL_AC, ShowAllCharges ? Env.Registry.FreightChargeCode : ChargeCodePK);

					if (ShowAllCharges)
					{
						var chargeCodeQuery = new ZDBOnlySubQuery(typeof(AccChargeCode), RateLinesSchema.TL_AC);
						chargeCodeQuery.AddToFilter(AccChargeCodeSchema.AC_ChargeGroup, ChargeCodeGroupList.Codes.Freight);
						rateLineQuery.AddSubQuery(chargeCodeQuery, JoinCondition.Or);
					}

					entryQuery.AddSubQuery(RateEntrySchema.PK, RateLinesSchema.TL_TI, rateLineQuery, JoinCondition.And);
				}

				entryQuery.MaximumRows = RatingDataRegistry.Instance.CostComparisonLinesNumber.Value;

				return entryQuery;
			}
		}

		ZQuery ContainerQuery
		{
			get
			{
				var result = new ZQuery();

				var filterContainer = Factory.Load<RefContainer>(ContainerType);
				var filterChargeCode = Factory.Load<AccChargeCode>(ChargeCodePK);

				if (filterContainer != null)
				{
					var entryQuery = new ZDBOnlyQuery(typeof(RateEntry));
					var containerSubQuery = new ZDBOnlySubQuery(typeof(RefContainer), RateEntrySchema.TI_RC);
					if (ShowAllCharges || (SingleChargeCodeComparisonOnly && filterChargeCode != null && filterChargeCode.AC_ChargeGroup == ChargeCodeGroupList.Codes.Freight))
					{
						containerSubQuery.AddToFilter(RefContainerSchema.RC_FreightRateClass, filterContainer.RC_FreightRateClass);
					}
					else if (ShowOriginChargesOnly || ShowDestinationChargesOnly || (SingleChargeCodeComparisonOnly && filterChargeCode != null && filterChargeCode.AC_ChargeGroup != ChargeCodeGroupList.Codes.Freight))
					{
						containerSubQuery.AddToFilter(RefContainerSchema.RC_HandlingRateClass, filterContainer.RC_HandlingRateClass);
					}
					entryQuery.AddSubQuery(containerSubQuery, JoinCondition.And);
					entryQuery.AddToFilter(RateEntrySchema.TI_MatchContainerRateClass, true);

					result.AddToFilter(JoinCondition.Or, RateEntrySchema.TI_RC, filterContainer.PK);
					result.AddToFilter(entryQuery, JoinCondition.Or);
					return result;
				}

				return result;
			}
		}

		ZQuery ModeFilter
		{
			get
			{
				var filter = new ZQuery();
				if (ShowOriginChargesOnly || ShowDestinationChargesOnly)
				{
					filter.AddToFilter(RateEntrySchema.TI_RateCategory, ShowOriginChargesOnly ? RatingConstants.RateCategory.ORG : RatingConstants.RateCategory.DST);
					filter.AddToFilter(RateEntrySchema.TI_Mode, Mode);
				}
				else
				{
					try
					{
						var freightMode = (FreightMode)Enum.Parse(typeof(FreightMode), Mode);
						var fclLCLExclusiveFilter = RatingHelper.GetFreightModeAndFCL_LCLExclusiveFilter(freightMode);
						filter.AddToFilter(fclLCLExclusiveFilter, JoinCondition.Or);
					}
					catch (ArgumentException)
					{
						//This is all very sad. Had to extract this sad logics into this not properly working comparer.
					}

					if (SingleChargeCodeComparisonOnly)
					{
						var oDFilter = new ZQuery();
						oDFilter.AddToFilter(JoinCondition.Or, RateEntrySchema.TI_RateCategory, SQLComparisonOperator.Equal, RatingConstants.RateCategory.ORG);
						oDFilter.AddToFilter(JoinCondition.Or, RateEntrySchema.TI_RateCategory, SQLComparisonOperator.Equal, RatingConstants.RateCategory.DST);

						var supplementaryFilter = RatingHelper.GetOriginDestinationModeExclusiveFilter(Mode);
						supplementaryFilter.AddToFilter(oDFilter);

						filter.AddToFilter(supplementaryFilter, JoinCondition.Or);
					}
				}

				return filter;
			}
		}

		ZQuery GetLocationQuery(ZString location, SchemaColumn column)
		{
			return LocationHelper.GetLocationFilter(Factory, location, column, false, typeof(RateEntry));
		}

		ZQuery DateFilter
		{
			get
			{
				var result = new ZQuery();

				if (!ValidFromDate.IsEmpty)
				{
					var endDateFilter = new ZQuery();
					endDateFilter.AddToFilter(RateEntrySchema.TI_RateEndDate, SQLComparisonOperator.GreaterThanOrEqualToDatePartOnly, ValidFromDate);
					endDateFilter.AddToFilter(JoinCondition.Or, RateEntrySchema.TI_RateEndDate, SQLComparisonOperator.Equal, null);
					result.AddToFilter(endDateFilter);
				}

				if (!ValidToDate.IsEmpty)
				{
					var startDateFilter = new ZQuery();
					startDateFilter.AddToFilter(RateEntrySchema.TI_RateStartDate, SQLComparisonOperator.LessThanOrEqualToDatePartOnly, ValidToDate);
					result.AddToFilter(startDateFilter);
				}

				return result;
			}
		}

		#endregion

		#region SummaryColumns

		public delegate void ResetSummaryColumnsEventHandler(SortedList<ChargesSummaryItem, ChargesSummaryItem> summaryItems);
		public event ResetSummaryColumnsEventHandler ResetSummaryColumns;

		#endregion

		#region SummaryItems

		public SortedList<ChargesSummaryItem, ChargesSummaryItem> SummaryItems
		{
			get
			{
				if (fSummaryItems == null)
				{
					fSummaryItems = new SortedList<ChargesSummaryItem, ChargesSummaryItem>();

					foreach (CostsComparerEntry comparerEntry in Costs)
					{
						foreach (var item in comparerEntry.SummaryItems.Keys)
						{
							if (!fSummaryItems.ContainsKey(item))
							{
								fSummaryItems.Add(item, null);
							}
						}
					}
				}

				return fSummaryItems;
			}
		}

		SortedList<ChargesSummaryItem, ChargesSummaryItem> fSummaryItems;

		void ResetSummaryItems()
		{
			fSummaryItems = null;
		}

		#endregion

		#endregion

		#region CurrencyConverter CurrencyConverter

		public CurrencyConverter CurrencyConverter
		{
			get
			{
				if (fCurrencyConverter == null)
				{
					fCurrencyConverter = CurrencyConverter.New(Factory, ZDateTime.Today, Enterprise.ZArchitecture.Core.ExchangeRateType.Buy, 7);
				}

				return fCurrencyConverter;
			}
		}

		CurrencyConverter fCurrencyConverter;

		#endregion

		#region Implementation

		#region DummyEntry

		RateEntry DummyEntry
		{
			get
			{
				if (fDummyEntry == null)
				{
					var dummyRate = (new BusinessObjectFactory()).New<ClientRate>();
					fDummyEntry = dummyRate.EntryCollections[RatingConstants.RateCategory.ORG].LazyLoadingCollection.AddNew();
				}

				return fDummyEntry;
			}
		}

		RateEntry fDummyEntry;

		#endregion

		#endregion

		#region IDocumentSupportable Members

		DocumentSupporter IDocumentSupportable.DocumentSupporter
		{
			get { return new CostsComparerDocumentSupporter(this); }
		}

		class CostsComparerDocumentSupporter : DocumentSupporter
		{
			public CostsComparerDocumentSupporter(CostsComparer parentBusinessObject) : base(parentBusinessObject) { }

			CostsComparer Comparer
			{
				get { return (CostsComparer)BusinessObject; }
			}

			protected override DocumentWrapper[] GetDocumentWrappersInternal(DataContext dataContext, IStmMenuItem commandBeingRun)
			{
				return new[] { DocumentWrapperFactory.CreateWrapper(DataContext.CostsComparer, Comparer) };
			}

			protected override DataContext[] GetSupportedDataContexts()
			{
				return new[] { DataContext.CostsComparer };
			}

			public override BusinessContext BusinessContext
			{
				get { return BusinessContext.CostsComparer; }
			}

			public override ISecurityCheckpoint CustomisationSecurityCheckpoint
			{
				get { return Env.Security.None; }
			}
		}

		#endregion
		ZGuid IVisualizerNoteSupporter.PK => PK;
		ZGuid IVisualizerNoteSupporter.ChildBusinessObjectPK => ZGuid.Empty;
		string IVisualizerNoteSupporter.TableCode => RateEntrySchema.Constants.Prefix;
	}
}

