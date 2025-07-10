using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using CargoWise.Application;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Customs.Common;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.Freight.Business;
using Enterprise.Freight.CarbonEmissions.Business;
using Enterprise.Freight.CarbonEmissions.Integration;
using Enterprise.Freight.Common.Business;
using Enterprise.Freight.Forwarding.Business.HelperClasses.FilteredRatingContractAllocationLine;
using Enterprise.Freight.Forwarding.Orders.Business;
using Enterprise.Freight.Integration;
using Enterprise.Freight.Integration.QuotedBooking;
using Enterprise.Integration;
using Enterprise.Integration.Freight;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.EventManagement;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Core.Constants;
using EventConstants = CargoWise.EventReference.Constants;

namespace Enterprise.Freight.Forwarding.Business
{
	[CodeProperty(Schema.JC_ContainerCode)]
	public class ForwardingContainer : CommonContainer,
		Enterprise.Integration.Forwarding.IForwardingContainer,
		ICartageContainer,
		IDocManagerSupport,
		IEDocsProvider,
		IBillDetails,
		IPackLineSynchroniseProvider,
		IProcessHandlingInfoProvider,
		IAdditionalReferenceNumberSupporter,
		ITrackableContainer,
		ICargoDimensions,
		ICO2eEmptyContainerProvider
	{
		public ForwardingContainer(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
			forwardingContainerInfoCollector = Factory.GetCachedValue("ForwardingContainerInfoCollector", () => new ForwardingContainerInfoCollector(this), CacheStalenessPolicy.NeverStale);
		}

		readonly ForwardingContainerInfoCollector forwardingContainerInfoCollector;

		#region Business Object Overrides

		protected override JobContainerValidation GetNewValidation()
		{
			return new ForwardingContainerValidation(this);
		}

		public new ForwardingContainerValidation Validation
		{
			get { return (ForwardingContainerValidation)base.Validation; }
		}

		protected override IContainerDefaultingStrategy NewContainerDefaultingStrategyCore()
		{
			return ObjectFactory.Get<IContainerDefaultingStrategy>("ForwardingContainerDefaultingStrategy", this);
		}

		protected override bool ShouldCheckIfCanUpdatePropertyForSpecialCases()
		{
			return true;
		}

		#endregion

		#region Property Overrides

		public override ZShort JC_ContainerCount
		{
			get { return base.JC_ContainerCount; }
			set
			{
				var changed = value != base.JC_ContainerCount;
				base.JC_ContainerCount = value;

				if (!JC_RCA_AllocationLine.IsEmpty)
				{
					Consol?.AllocationConsumptionLogger.TryCaptureInitialPersistedConsumption();
				}

				MarkCusContainerAsNeedingValidation();
				Consol?.MarkAsNeedingValidationIncludingChildren();
				if (changed)
				{
					this.UpdateCO2eStatusToNotCurrent(new CO2eStatusChangedReason(JC_ContainerCountInfo), IsCopying, CO2eTypes.EmptyPickup);
					this.UpdateCO2eStatusToNotCurrent(new CO2eStatusChangedReason(JC_ContainerCountInfo), IsCopying, CO2eTypes.EmptyReturn);
				}
			}
		}

		public override ZGuid JC_OA_DepartureContainerYardAddress
		{
			get { return base.JC_OA_DepartureContainerYardAddress; }
			set
			{
				if (value != base.JC_OA_DepartureContainerYardAddress)
				{
					base.JC_OA_DepartureContainerYardAddress = value;
					this.UpdateCO2eStatusToNotCurrent(new CO2eStatusChangedReason(JC_OA_DepartureContainerYardAddressInfo), IsCopying, CO2eTypes.EmptyPickup);
				}
			}
		}

		public override ZGuid JC_OA_ArrivalContainerYardAddress
		{
			get { return base.JC_OA_ArrivalContainerYardAddress; }
			set
			{
				if (value != base.JC_OA_ArrivalContainerYardAddress)
				{
					base.JC_OA_ArrivalContainerYardAddress = value;
					this.UpdateCO2eStatusToNotCurrent(new CO2eStatusChangedReason(JC_OA_ArrivalContainerYardAddressInfo), IsCopying, CO2eTypes.EmptyReturn);
				}
			}
		}

		public override ZGuid JC_RC
		{
			get { return base.JC_RC; }
			set
			{
				var isValueChanged = base.JC_RC != value;
				if (isValueChanged)
				{
					base.JC_RC = value;

					if (!JC_RCA_AllocationLine.IsEmpty)
					{
						Consol?.AllocationConsumptionLogger.TryCaptureInitialPersistedConsumption();
					}

					Consol?.AllocationRouteContainerWeightLimitHelper.CheckAndPromptOverrideOnContainerWeightLimit(base.JC_RCA_AllocationLine);
					this.UpdateCO2eStatusToNotCurrent(new CO2eStatusChangedReason(JC_RCInfo), IsCopying, CO2eTypes.EmptyPickup);
					this.UpdateCO2eStatusToNotCurrent(new CO2eStatusChangedReason(JC_RCInfo), IsCopying, CO2eTypes.EmptyReturn);
					MarkCusContainerAsNeedingValidation();
					Consol?.MarkAsNeedingValidationIncludingChildren();
					Booking?.MarkAsNeedingValidationIncludingChildren();
				}
			}
		}

		public override ZString JC_SealNum
		{
			get { return base.JC_SealNum; }
			set
			{
				base.JC_SealNum = value;
				MarkCusContainerAsNeedingValidation();
			}
		}

		public override ZGuid JC_JK
		{
			get { return base.JC_JK; }
			set
			{
				base.JC_JK = value;
				MarkCusContainerAsNeedingValidation();
			}
		}

		public override ZString JC_ContainerMode
		{
			get { return base.JC_ContainerMode; }
			set
			{
				base.JC_ContainerMode = value;
				MarkCusContainerAsNeedingValidation();
			}
		}

		public override ZString JC_ContainerNum
		{
			get { return base.JC_ContainerNum; }
			set
			{
				base.JC_ContainerNum = value;
				MarkCusContainerAsNeedingValidation();
			}
		}

		public override ZDecimal JC_SetPointTemp
		{
			get { return base.JC_SetPointTemp; }
			set
			{
				base.JC_SetPointTemp = value;
				MarkCusContainerAsNeedingValidation();
			}
		}

		public override ZDecimal JC_AirVentFlow
		{
			get { return base.JC_AirVentFlow; }
			set
			{
				base.JC_AirVentFlow = value;
				MarkCusContainerAsNeedingValidation();
			}
		}

		public override ZString JC_TempRecorderSerialNo
		{
			get { return base.JC_TempRecorderSerialNo; }
			set
			{
				base.JC_TempRecorderSerialNo = value;
				MarkCusContainerAsNeedingValidation();
			}
		}

		public override ZBool JC_IsNonOperativeReefer
		{
			get { return base.JC_IsNonOperativeReefer; }
			set
			{
				base.JC_IsNonOperativeReefer = value;
				MarkCusContainerAsNeedingValidation();
			}
		}

		public override ZBool JC_IsControlledAtmosphere
		{
			get { return base.JC_IsControlledAtmosphere; }
			set
			{
				base.JC_IsControlledAtmosphere = value;
				MarkCusContainerAsNeedingValidation();
			}
		}

		public override ZByte JC_HumidityPercent
		{
			get { return base.JC_HumidityPercent; }
			set
			{
				base.JC_HumidityPercent = value;
				MarkCusContainerAsNeedingValidation();
			}
		}

		public override ZBool JC_IsEmptyContainer
		{
			get { return base.JC_IsEmptyContainer; }
			set
			{
				base.JC_IsEmptyContainer = value;

				if (!IsValidationSuspended)
				{
					foreach (PackLine packLine in PackLines)
					{
						packLine.Validation.ValidateJL_PackageCount();
					}
				}
			}
		}

		public override ZGuid JC_JX
		{
			get { return base.JC_JX; }
			set
			{
				base.JC_JX = value;
				MarkCusContainerAsNeedingValidation();
			}
		}

		public override ZGuid JC_JS_FCLBookingOnlyLink
		{
			get { return base.JC_JS_FCLBookingOnlyLink; }
			set
			{
				base.JC_JS_FCLBookingOnlyLink = value;
				MarkCusContainerAsNeedingValidation();
			}
		}

		[EventDateProperty(AutoEvents.GateInCode, EstimateActual.Actual, shouldOnlyUpdateEmptyDate: true)]
		public override ZDateTime JC_FCLWharfGateIn
		{
			get { return base.JC_FCLWharfGateIn; }
			set
			{
				if (base.JC_FCLWharfGateIn != value)
				{
					base.JC_FCLWharfGateIn = value;

					LogEvent(AutoEvents.GateIn, value.ToOffset(), EventConstants.Facilities.Code.Terminal);
				}
			}
		}

		[EventDateProperty(AutoEvents.GateOutCode, EstimateActual.Actual, shouldOnlyUpdateEmptyDate: true)]
		public override ZDateTime JC_FCLWharfGateOut
		{
			get { return base.JC_FCLWharfGateOut; }
			set
			{
				if (base.JC_FCLWharfGateOut != value)
				{
					base.JC_FCLWharfGateOut = value;

					LogEvent(AutoEvents.GateOut, value.ToOffset(), EventConstants.Facilities.Code.Terminal);
				}
			}
		}

		[EventDateProperty(AutoEvents.GateInCode, EstimateActual.Actual, shouldOnlyUpdateEmptyDate: true)]
		[EventDateProperty(AutoEvents.DehireCode, EstimateActual.Actual)]
		public override ZDateTime JC_ContainerYardEmptyReturnGateIn
		{
			get { return base.JC_ContainerYardEmptyReturnGateIn; }
			set
			{
				if (base.JC_ContainerYardEmptyReturnGateIn != value)
				{
					base.JC_ContainerYardEmptyReturnGateIn = value;

					LogEvent(AutoEvents.GateIn, value.ToOffset(), EventConstants.Facilities.Code.ContainerYard);
					LogEvent(AutoEvents.Dehire, value.ToOffset(), EventConstants.Facilities.Code.ContainerYard);
				}
			}
		}

		[EventDateProperty(AutoEvents.GateOutCode, EstimateActual.Actual, shouldOnlyUpdateEmptyDate: true)]
		public override ZDateTime JC_ContainerYardEmptyPickupGateOut
		{
			get { return base.JC_ContainerYardEmptyPickupGateOut; }
			set
			{
				if (base.JC_ContainerYardEmptyPickupGateOut != value)
				{
					base.JC_ContainerYardEmptyPickupGateOut = value;

					LogEvent(AutoEvents.GateOut, value.ToOffset(), EventConstants.Facilities.Code.ContainerYard);
				}
			}
		}

		public override ZBool IsGrossWeightOverriddenForBinding
		{
			get => base.IsGrossWeightOverriddenForBinding;
			set
			{
				if (base.IsGrossWeightOverriddenForBinding != value)
				{
					base.IsGrossWeightOverriddenForBinding = value;
					if (!value)
					{
						SetGrossWeightFromCombinedWeights();
					}
				}
			}
		}

		public override bool IsGrossWeightOverrideAvailable
		{
			get
			{
				if (Transport == Constants.TransportModes.Air)
				{
					var consolMode = (string)Consol?.JK_ConsolMode;
					return consolMode == Constants.ContainerModes.BuyersConsol
						|| consolMode == Constants.ContainerModes.ULD;
				}

				return false;
			}
		}

		public override ZString JC_GrossWeightUQ
		{
			get { return base.JC_GrossWeightUQ; }
			set
			{
				if (JC_GrossWeightUQ != value)
				{
					var oldValue = base.JC_GrossWeightUQ;
					if (oldValue != value && Core.Constants.Weight.ContainsCode(oldValue) && Core.Constants.Weight.ContainsCode(value))
					{
						if (JC_TareWeight == JC_Calc_TareWeight)
						{
							JC_TareWeight = Core.Constants.Weight.Convert(JC_TareWeight, oldValue, value);
						}
						else
						{
							GrossWeightUQShowTareWeightWarning = true;
						}

						if (JC_DunnageWeight != 0)
						{
							GrossWeightUQShowDunnageWeightWarning = true;
						}
					}

					var oldUnit = ContainerWeightUnit;
					base.JC_GrossWeightUQ = value;

					if (!IsGrossWeightOverrideActive)
					{
						if (IsGrossWeightVerified)
						{
							JC_GrossWeight = Constants.Weight.Convert(JC_GrossWeight, oldUnit, ContainerWeightUnit, false);
						}
						else
						{
							JC_GrossWeight = JC_TareWeight + JC_DunnageWeight + Constants.Weight.Convert(JC_Calc_TotalWeight, JC_Calc_TotalWeightUnit, ContainerWeightUnit, false);
						}
					}
				}
			}
		}

		public bool GrossWeightUQShowTareWeightWarning
		{
			get => (grossWeightUQShowTareWeightWarning ?? false);
			set
			{
				grossWeightUQShowTareWeightWarning = value;
			}
		}
		bool? grossWeightUQShowTareWeightWarning;

		public bool GrossWeightUQShowDunnageWeightWarning
		{
			get => (grossWeightUQShowDunnageWeightWarning ?? false);
			set
			{
				grossWeightUQShowDunnageWeightWarning = value;
			}
		}
		bool? grossWeightUQShowDunnageWeightWarning;

		public override ZDecimal JC_TareWeight
		{
			get { return base.JC_TareWeight; }
			set
			{
				base.JC_TareWeight = value;
				if (JC_TareWeight == JC_Calc_TareWeight)
				{
					GrossWeightUQShowTareWeightWarning = false;
				}
				Validation.ValidateJC_TareWeight();
			}
		}

		public override ZDecimal JC_DunnageWeight
		{
			get { return base.JC_DunnageWeight; }
			set
			{
				base.JC_DunnageWeight = value;
				if (JC_DunnageWeight == 0)
				{
					GrossWeightUQShowDunnageWeightWarning = false;
				}
				Validation.ValidateJC_DunnageWeight();
			}
		}

		public override ZBool JC_IsShipperOwned
		{
			get { return base.JC_IsShipperOwned; }
			set
			{
				if (base.JC_IsShipperOwned != value)
				{
					base.JC_IsShipperOwned = value;
					Validation.ValidateJC_IsShipperOwned();
				}
			}
		}

		#region JC_DepartureEstimatedPickup

		public override ZDateTime JC_DepartureEstimatedPickup
		{
			get { return base.JC_DepartureEstimatedPickup; }
			set
			{
				if (base.JC_DepartureEstimatedPickup != value)
				{
					base.JC_DepartureEstimatedPickup = value;
					CreateOrUpdateFullContainerEventLog(AutoEvents.PickedUp, EstimateActual.Estimate, value.ToOffset());
				}
			}
		}

		void CreateOrUpdateFullContainerEventLog(Event eventType, EstimateActual estimateActual, ZDateTimeOffset eventTime)
		{
			var stmALog = Logs.MostRecentLogByEventTime(eventType);
			if (stmALog != null && stmALog.SL_EventTimeOffset != eventTime
				&& stmALog.Parameters.ContainsKey(EventConstants.EventReferenceParameters.Codes.Type)
				&& stmALog.Parameters[EventConstants.EventReferenceParameters.Codes.Type] == Constants.EventReferenceParameterTypes.FullContainer)
			{
				Logs.CreateRecreateOrUpdateEventLog(eventType, estimateActual, eventTime, stmALog.SL_Reference);
			}
			else
			{
				Logs.CreateRecreateOrUpdateEventLog(eventType, estimateActual, eventTime, "|TYP=FUL"); // log reference
			}
		}

		#endregion

		#region JC_DepartureCartageComplete

		public override ZDateTime JC_DepartureCartageComplete
		{
			get { return base.JC_DepartureCartageComplete; }
			set
			{
				if (base.JC_DepartureCartageComplete != value)
				{
					base.JC_DepartureCartageComplete = value;
					CreateOrUpdateFullContainerEventLog(AutoEvents.PickedUp, EstimateActual.Actual, value.ToOffset());
				}
			}
		}

		#endregion

		#region JC_ArrivalEstimatedDelivery

		public override ZDateTime JC_ArrivalEstimatedDelivery
		{
			get { return base.JC_ArrivalEstimatedDelivery; }
			set
			{
				if (base.JC_ArrivalEstimatedDelivery != value)
				{
					base.JC_ArrivalEstimatedDelivery = value;
					CreateOrUpdateFullContainerEventLog(AutoEvents.Delivered, EstimateActual.Estimate, value.ToOffset());
				}
			}
		}

		#endregion

		#region JC_ArrivalCartageComplete

		public override ZDateTime JC_ArrivalCartageComplete
		{
			get { return base.JC_ArrivalCartageComplete; }
			set
			{
				if (base.JC_ArrivalCartageComplete != value)
				{
					base.JC_ArrivalCartageComplete = value;
					CreateOrUpdateFullContainerEventLog(AutoEvents.Delivered, EstimateActual.Actual, value.ToOffset());
				}
			}
		}

		#endregion

		#region JC_TotalDimensions

		public override ZDecimal JC_TotalLength
		{
			get { return base.JC_TotalLength; }
			set
			{
				base.JC_TotalLength = value;

				Validation.ValidateJC_TotalWidth();
				Validation.ValidateJC_TotalHeight();
			}
		}

		public override ZDecimal JC_TotalWidth
		{
			get { return base.JC_TotalWidth; }
			set
			{
				base.JC_TotalWidth = value;

				Validation.ValidateJC_TotalLength();
				Validation.ValidateJC_TotalHeight();
			}
		}

		public override ZDecimal JC_TotalHeight
		{
			get { return base.JC_TotalHeight; }
			set
			{
				base.JC_TotalHeight = value;

				Validation.ValidateJC_TotalLength();
				Validation.ValidateJC_TotalWidth();
			}
		}

		#endregion

		[EventDateProperty(AutoEvents.FreightUnloadedCode, EstimateActual.Actual)]
		public override ZDateTime JC_FCLUnloadFromVessel
		{
			get { return base.JC_FCLUnloadFromVessel; }
			set
			{
				if (base.JC_FCLUnloadFromVessel != value)
				{
					base.JC_FCLUnloadFromVessel = value;

					ContainerPenaltyRelatedDateChanging(ContainerPenaltyRelatedDateType.FCLUnloadFromVessel);
				}
			}
		}

		#region JC_RCA_AllocationLine

		public override ZGuid JC_RCA_AllocationLine
		{
			get => base.JC_RCA_AllocationLine;
			set
			{
				if (base.JC_RCA_AllocationLine != value)
				{
					base.JC_RCA_AllocationLine = value;
					Consol?.AllocationConsumptionLogger.TryCaptureInitialPersistedConsumption();

					if (Consol?.AllocationLine == null)
					{
						Consol?.AllocationRouteContainerWeightLimitHelper.CheckAndPromptOverrideOnContainerWeightLimit(value);
					}

					if (!IsValidationSuspended)
					{
						Validation.ValidateJC_RCA_AllocationLine();
						Consol?.Validation.ValidateJK_CarrierContractNumber();
					}

					Consol?.AllocationRouteContainerWeightLimitHelper.SetRequireApprovalEventCancellationCheck();
				}
			}
		}

		public bool JC_RCA_AllocationLine_ReadOnly
		{
			get
			{
				if (JC_RCA_AllocationLine.IsEmpty)
				{
					return false;
				}

				return IsRouteConsistentWithParent();
			}
		}

		public bool IsRouteConsistentWithParent()
		{
			var parentAllocationRoute = Consol?.AllocationLine ?? QuotedBooking?.AllocationRoute;
			return parentAllocationRoute?.PK == JC_RCA_AllocationLine;
		}

		public IQuotedBooking QuotedBooking => ObjectFactory.Get<IQuotedBookingBuilder>().Load(Factory, JC_JS_FCLBookingOnlyLink) as IQuotedBooking;

		public IRatingContractAllocationLine AllocationLine => Factory.Load<IRatingContractAllocationLine>(JC_RCA_AllocationLine);

		public override IRatingContractAllocationLineCollection AllocationLineCollection
		{
			get
			{
				if (allocationLineCollection == null)
				{
					allocationLineCollection = new FilteredRatingContractAllocationLineCollection(Factory, () => Consol?.CarrierContract?.PK ?? QuotedBooking?.CarrierContract?.PK);
				}

				return allocationLineCollection;
			}
		}

		#endregion

		#region Container Load List Lines

		ContainerLoadListLineCollection containerLoadListLines;
		public ContainerLoadListLineCollection ContainerLoadListLines
		{
			get => containerLoadListLines ?? (containerLoadListLines = new ContainerLoadListLineCollection(this.Factory, new ZQuery(ContainerLoadListLineSchema.CLL_JC_Container, PK)));
		}

		public override bool IsAttachedToContainerLoadList() => ContainerLoadListLines.Count > 0;

		#endregion

		#region JC_RH_NKContainerCommodityCode

		public override ZString JC_RH_NKContainerCommodityCode
		{
			get => base.JC_RH_NKContainerCommodityCode;
			set
			{
				base.JC_RH_NKContainerCommodityCode = value;

				Consol?.MarkAsNeedingValidation();
			}
		}

		#endregion

		#region SailingCTOAvailableDate

		public ZDateTime SailingCTOAvailableDate
		{
			get
			{
				return Sailing?.Destination?.JB_AvailabilityDate ?? ZDateTime.Empty;
			}
		}

		public ZPropertyInfo SailingCTOAvailableDateInfo
		{
			get
			{
				return GetWrappedZPropertyInfo(nameof(SailingCTOAvailableDate), x => Sailing?.Destination?.JB_AvailabilityDateInfo);
			}
		}

		#endregion

		#region JC_EmptyReturnedBy

		public override bool RequireCalculateJC_EmptyReturnedBy
		{
			get
			{
				return JC_OverriddenFCLAvailableHasChanges ||
					(!JC_OverrideFCLAvailableStorage && ArrivalTransport != null && (ArrivalTransport?.JW_TerminalAvailabilityDateInfo?.HasChanges ?? false)) ||
					(!JC_OverrideFCLAvailableStorage && ArrivalTransport == null && (StandaloneSailing?.Destination?.JB_AvailabilityDateInfo.HasChanges ?? false)) ||
					JC_FCLWharfGateOutInfo.HasChanges ||
					JC_FCLUnloadFromVesselInfo.HasChanges ||
					JC_FCLWharfGateInInfo.HasChanges ||
					SailingCTOAvailableDateInfo.HasChanges ||
					JC_FCLOnBoardVesselInfo.HasChanges ||
					(Consol != null && (Consol.JK_ATAForLastTransportInfo.HasChanges || Consol.JK_DepartureForFirstTransportInfo.HasChanges));
			}
		}

		#endregion

		#endregion

		#region Related Business Objects

		public new ForwardingConsol Consol
		{
			get { return (ForwardingConsol)base.Consol; }
		}

		protected override CommonConsol LoadParentConsol()
		{
			return Factory.Load<ForwardingConsol>(JC_JK);
		}

		protected override PackLineManyToManyCollection GetNewPackLineCollection()
		{
			return new ForwardingPackLineManyToManyCollection(this);
		}

		protected override void OnPackLineCountChanged(CollectionCountChangedEventArgs e)
		{
			base.OnPackLineCountChanged(e);

			if (!IsValidationSuspended)
			{
				Validation.ValidateJC_IsEmptyContainer();
			}
		}

		void MarkCusContainerAsNeedingValidation()
		{
			ZQuery query = new ZQuery();
			query.AddToFilter(CusContainerSchema.CO_JC, PK);
			if (!IsInDatabase)
			{
				query.FetchOnlyFromLocalCache = true;
			}

			BusinessObject customsContainer = (BusinessObject)Factory.LoadTop1<Enterprise.Integration.Customs.Shared.IBaseCusContainer>(query);
			if (customsContainer != null)
			{
				customsContainer.MarkAsNeedingValidation();
			}
		}

		protected override RefCommodityCodeCollection GetNewContainerCommodityCodeListCore()
		{
			return new ForwardingContainerCommodityCodeCollection(Factory);
		}

		#endregion

		#region Fetch Strategy

		protected override EnterpriseBusinessObjectFetchStrategy GetFetchStrategyCore()
		{
			return new ForwardingContainerFetchStrategy(this);
		}

		protected class ForwardingContainerFetchStrategy : ContainerFetchStrategy
		{
			public ForwardingContainerFetchStrategy(ForwardingContainer container)
				: base(container)
			{
			}

			protected override void FetchForLoadChildEditableObjectsCore()
			{
				base.FetchForLoadChildEditableObjectsCore();
				Factory.AddFetchHint(CusEntryNumSchema.CE_ParentID, Container.PK);
			}

			protected override void FetchForViewCore(TableColumn[] columns)
			{
				var propertiesUsingEntryNumbers = new[]
				{
					nameof(AMSNumber),
					nameof(ITReferenceNumber)
				};

				if (columns.Any(c => propertiesUsingEntryNumbers.Contains(c.ColumnName)))
				{
					Factory.AddFetchHint(CusEntryNumSchema.CE_ParentID, Container.PK);
				}

				base.FetchForViewCore(columns);
			}
		}

		#endregion

		#region Factory

		public override void OnSaving()
		{
			base.OnSaving();

			if (this.ContainerParent is IContainerTrackingProvider containerParent)
			{
				new ContainerTrackingSubscriptionRequestedManager(this, containerParent).UpdateIfNecessary();
			}
		}

		public override void Delete()
		{
			deletedStack = System.Environment.StackTrace;

			base.Delete();
		}

		string deletedStack;

		#endregion

		#region ICartageContainer Members

		ZString ICartageContainer.ContainerMode
		{
			get { return JC_ContainerMode; }
		}

		ZString ICartageContainer.ContainerNumber
		{
			get { return JC_ContainerNum; }
		}

		ZGuid ICartageContainer.ContainerRC
		{
			get { return JC_RC; }
		}

		ZGuid ICartageContainer.JobContainerPK
		{
			get { return PK; }
		}

		ZDecimal ICartageContainer.NetWeight
		{
			get { return JC_Calc_NetWeight; }
		}

		ZString ICartageContainer.Seal
		{
			get { return JC_SealNum; }
		}

		IReadOnlyCollection<ICartageLooseCargo> ICartageContainer.LooseCargo
		{
			get { return (ICartageLooseCargo[])PackLines.ToArray(typeof(ICartageLooseCargo)); }
		}

		#endregion

		#region IEDocsProvider Members

		EDocsProviderSupporter IEDocsProvider.GetEDocsProviderSupporter()
		{
			return new JobInvoicingEDocsProviderSupporter(this);
		}

		#endregion

		#region IDocumentSupportable Members

		public virtual DocumentSupporter DocumentSupporter
		{
			get
			{
				DocumentSupporter result;

				if (LinkedShipment != null)
				{
					result = new ContainerDocumentSupporterShipment(this);
				}
				else
				{
					result = new ContainerDocumentSupporterConsol(this);
				}

				return result;
			}
		}

		#endregion

		#region IBillDetails

		ZPropertyInfo IBillDetails.AMSBillNumberInfo
		{
			get { return AMSNumberInfo; }
		}

		ZPropertyInfo IBillDetails.BillNumberInfo
		{
			get { return AMSNumberInfo; }
		}

		ZPropertyInfo IBillDetails.BKGBillNumberInfo
		{
			get { return null; }
		}

		ZPropertyInfo[] IBillDetails.GetNumberOfPackesInfos(ForwardingShipment shipment)
		{
			var result = new List<ZPropertyInfo>();
			var shipments = shipment.GetShipmentsForBill();
			foreach (PackLine packLine in PackLines)
			{
				if (shipments.Any(x => x == packLine.Shipment))
				{
					result.Add(packLine.JL_PackageCountInfo);
				}
			}
			return result.ToArray();
		}

		ZPropertyInfo[] IBillDetails.GetTypeOfPackesInfos(ForwardingShipment shipment)
		{
			var result = new List<ZPropertyInfo>();
			var shipments = shipment.GetShipmentsForBill();
			foreach (PackLine packLine in PackLines)
			{
				if (shipments.Any(x => x == packLine.Shipment))
				{
					result.Add(packLine.JL_F3_NKPackTypeInfo);
				}
			}
			return result.ToArray();
		}

		IEnumerable<OrgHeader> IBillDetails.SCACIssuers => Enumerable.Empty<OrgHeader>();

		ZDateTime IBillDetails.BillUssueDate => ZDateTime.Empty;
		#endregion

		#region Customs Reference Numbers Support

		[MaxLength(CusEntryNumber.Schema.CE_EntryNumMaxLength)]
		public virtual ZString AMSNumber
		{
			get { return GetReferenceNumber(CustomsReferenceNumberType.CustomsAdditionalReferenceNumbersCodes.AMS); }
			set
			{
				if (AMSNumber != value)
				{
					CheckMaximumLength(AMSNumberInfo, value);
					SetReferenceNumber(CustomsReferenceNumberType.CustomsAdditionalReferenceNumbersCodes.AMS, value);
				}
			}
		}

		public virtual ZPropertyInfo AMSNumberInfo
		{
			get { return GetZPropertyInfo(nameof(AMSNumber)); }
		}

		[MaxLength(11)]
		public virtual ZString ITReferenceNumber
		{
			get { return GetReferenceNumber(UnitedStatesAdditionalReferenceNumberTypes.Codes.IT); }
			set
			{
				if (ITReferenceNumber != value)
				{
					CheckMaximumLength(ITReferenceNumberInfo, value);
					SetReferenceNumber(UnitedStatesAdditionalReferenceNumberTypes.Codes.IT, value);
				}
			}
		}

		public virtual ZPropertyInfo ITReferenceNumberInfo
		{
			get { return GetZPropertyInfo(nameof(ITReferenceNumber)); }
		}

		ZString GetReferenceNumber(ZString referenceType)
		{
			var refNum = AdditionalReferenceNumbers.GetFirstReferenceNumberByType(referenceType);
			return refNum != null ? refNum.CE_EntryNum : ZString.Empty;
		}

		void SetReferenceNumber(ZString referenceType, ZString referenceNumber)
		{
			var refNum = AdditionalReferenceNumbers.GetFirstReferenceNumberByType(referenceType);
			if (referenceNumber.IsEmpty)
			{
				if (refNum != null)
				{
					AdditionalReferenceNumbers.RemoveAndDelete(refNum);
				}
			}
			else
			{
				if (refNum == null)
				{
					refNum = AdditionalReferenceNumbers.AddNew();
					refNum.CE_EntryType = referenceType;
				}
				refNum.CE_EntryNum = referenceNumber;
			}
		}

		public bool HasNewMBOL
		{
			get { return !AMSNumber.IsEmpty; }
		}

		public bool HasITNumber
		{
			get { return !ITReferenceNumber.IsEmpty; }
		}

		#endregion

		#region IPackLineSynchroniseProvider

		IPackLineSynchronise IPackLineSynchroniseProvider.PackLineSynchronise
		{
			get { return PackLineSynchroniser; }
		}
		public IPackLineSynchronise PackLineSynchroniser;

		#endregion

		#region ICheckFitsInConsol

		ZString ICargoDimensions.DimensionsUnits => Constants.Length.Feet;

		ZDecimal ICargoDimensions.Length => JC_TotalLength;

		ZDecimal ICargoDimensions.Width => JC_TotalWidth;

		ZDecimal ICargoDimensions.Height => JC_TotalHeight;

		#endregion

		protected override void OnEntryNumChangedCore(CusEntryNumber additionalReferenceNumber)
		{
			base.OnEntryNumChangedCore(additionalReferenceNumber);

			switch (additionalReferenceNumber.CE_EntryType)
			{
				case CustomsReferenceNumberType.CustomsAdditionalReferenceNumbersCodes.AMS:
					AMSNumberInfo.RefreshBinding();
					break;
				case UnitedStatesAdditionalReferenceNumberTypes.Codes.IT:
					ITReferenceNumberInfo.RefreshBinding();
					break;
				default:
					break;
			}
		}

		public ProcessHandlingInfo ProcessHandlingInfo
		{
			get
			{
				return new ForwardingContainerProcessHandlingInfo(this);
			}
		}

		#region Numbers

		protected override void OnNumbersLoaded()
		{
			base.OnNumbersLoaded();

			AdditionalReferenceNumbers.CountChanged += AdditionalReferenceNumbers_CountChanged;
		}

		void AdditionalReferenceNumbers_CountChanged(object sender, CollectionCountChangedEventArgs args)
		{
			if (args?.BizObject is CusEntryNumber cusEntryNumber
				&& args.ItemAdded
				&& !cusEntryNumber.IsDeleted
				&& IsAdditionalReferenceNumberForSystemOnly(cusEntryNumber))
			{
				AddCannotDeleteNumberHandler(cusEntryNumber, ResString.GetMultilingualString("79956FF9-9F91-4E9D-8189-C5713CD1A7BF", "The {0} is system generated and cannot be deleted.", cusEntryNumber.CE_EntryType));
			}
		}

		protected override bool AdditionalReferenceNumberCannotBeDeleted(CusEntryNumber cusEntryNumber)
		{
			return base.AdditionalReferenceNumberCannotBeDeleted(cusEntryNumber)
				|| IsAdditionalReferenceNumberForSystemOnly(cusEntryNumber);
		}

		bool IsAdditionalReferenceNumberForSystemOnly(CusEntryNumber cusEntryNumber)
		{
			var entryTypesForSystemOnly = new ZString[]
			{
				ContainerNonCustomsAdditionalReferenceCodesCodeList.Codes.CarrierMessageReference
			};

			return entryTypesForSystemOnly.Contains(cusEntryNumber.CE_EntryType) && cusEntryNumber.CE_EntryIsSystemGenerated;
		}

		#endregion

		#region Logging

		public override IDictionary<string, string> GetParametersForEvent(Event evnt)
		{
			IDictionary<string, string> parameters = new Dictionary<string, string>();

			if (evnt == Events.SubscriptionRequested)
			{
				parameters[EventConstants.EventReferenceParameters.Codes.Type] = Constants.EventReferenceParameterTypes.ContainerTracking;
			}
			else
			{
				parameters = base.GetParametersForEvent(evnt);
			}

			return parameters;
		}

		#endregion

		#region ITrackableContainer

		ZString ITrackableContainer.ContainerNumber
		{
			get
			{
				return this.JC_ContainerNum;
			}
		}

		bool ITrackableContainer.ContainerNumberHasChanges
		{
			get
			{
				return (!this.IsInDatabase && !this.JC_ContainerNum.IsEmpty) || this.JC_ContainerNumInfo.HasChanges;
			}
		}

		#endregion

		#region Update Full Container Date

		protected override void ProcessLogCore(IStmALog log)
		{
			base.ProcessLogCore(log);

			if (log.SL_SE_NKEvent == Events.PickedUpCode || log.SL_SE_NKEvent == Events.DeliveredCode)
			{
				UpdateFullContainerDate(log);
			}
			else if (new[]
				{
					Events.MessageSentCode,
					Events.InterchangeReceiptAcknowledgedCode,
					Events.InterchangeRejectedCode,
					Events.MessageAcceptedCode,
					Events.MessageWithdrawCancelRequestCode,
					Events.MessageRejectedCode,
					Events.StatusUpdatedCode
				}.Contains(log.SL_SE_NKEvent.ToString()))
			{
				UpdateVGMStatus(log);
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
		void UpdateVGMStatus(IStmALog log)
		{
			if (log.Parameters.ContainsKey(EventConstants.EventReferenceParameters.Codes.MessageType)
					&& log.Parameters[EventConstants.EventReferenceParameters.Codes.MessageType] == Constants.EventReferenceMessageTypes.VerifiedGrossContainerWeight)
			{
				switch (log.SL_SE_NKEvent)
				{
					case Events.MessageSentCode:
						JC_GrossWeightVerificationStatus = Constants.ContainerGrossWeightVerificationStatuses.Codes.Sent;
						break;

					case Events.InterchangeReceiptAcknowledgedCode:
						JC_GrossWeightVerificationStatus = MostRecentVGMLogByPostedTime(Events.MessageWithdrawCancelRequest) != null
							|| MostRecentVGMLogByPostedTime(Events.MessageWithdrawCancelAccepted) != null
							? Constants.ContainerGrossWeightVerificationStatuses.Codes.WithdrawAcknowledged
							: Constants.ContainerGrossWeightVerificationStatuses.Codes.Acknowledged;
						break;

					case Events.InterchangeRejectedCode:
					case Events.MessageRejectedCode:
						JC_GrossWeightVerificationStatus = MostRecentVGMLogByPostedTime(Events.MessageWithdrawCancelRequest) != null
							? Constants.ContainerGrossWeightVerificationStatuses.Codes.WithdrawRejected
							: Constants.ContainerGrossWeightVerificationStatuses.Codes.Rejected;
						break;

					case Events.MessageAcceptedCode:
						JC_GrossWeightVerificationStatus = MostRecentVGMLogByPostedTime(Events.MessageWithdrawCancelRequest) != null
							|| Logs.MostRecentLogByPostedTime(Events.MessageWithdrawCancelAccepted) != null
							? Constants.ContainerGrossWeightVerificationStatuses.Codes.WithdrawAcknowledged
							: Constants.ContainerGrossWeightVerificationStatuses.Codes.Accepted;
						break;

					case Events.MessageWithdrawCancelRequestCode:
						JC_GrossWeightVerificationStatus = Constants.ContainerGrossWeightVerificationStatuses.Codes.WithdrawSent;
						break;

					case Events.StatusUpdatedCode:
						if (log.Parameters.TryGetValue(EventConstants.EventReferenceParameters.Codes.Type, out string value)
							&& string.CompareOrdinal(value, Constants.EventReferenceMessageTypes.ResetToOriginal) == 0)
						{
							JC_GrossWeightVerificationStatus = Constants.ContainerGrossWeightVerificationStatuses.Codes.NotSent;
						}
						break;
				}
			}
		}

		void UpdateFullContainerDate(IStmALog log)
		{
			if (log.Parameters.ContainsKey(EventConstants.EventReferenceParameters.Codes.Type)
					&& log.Parameters[EventConstants.EventReferenceParameters.Codes.Type] == Constants.EventReferenceParameterTypes.FullContainer)
			{
				if (log.SL_SE_NKEvent == Events.PickedUpCode)
				{
					if (log.SL_IsEstimate)
					{
						JC_DepartureEstimatedPickup = log.SL_EventTime;
					}
					else
					{
						JC_DepartureCartageComplete = log.SL_EventTime;
					}
				}
				else if (log.SL_SE_NKEvent == Events.DeliveredCode)
				{
					if (log.SL_IsEstimate)
					{
						JC_ArrivalEstimatedDelivery = log.SL_EventTime;
					}
					else
					{
						JC_ArrivalCartageComplete = log.SL_EventTime;
					}
				}
			}
		}

		#endregion

		#region Diagnostics

		protected override StringBuilder BuildRowDeletedReport(string columnName, Exception ex, DataRowVersion versionToUse, DataRowVersion version, string message)
		{
			return base.BuildRowDeletedReport(columnName, ex, versionToUse, version, message)
				.Append((NoResString)"BO Deleted Stack Trace: ").AppendLine(deletedStack ?? (NoResString)"Delete stack never collected") // Error Reporter
				.AppendLine((NoResString)"Row Deleted Stack Trace: ").AppendLine(forwardingContainerInfoCollector.GetLastRowDeletedStackTrace(PK.ToGuid())); // Error Reporter
		}

		#endregion

		public override ZGuid JC_CLH_LoadListPlan
		{
			get => base.JC_CLH_LoadListPlan;
			set
			{
				if (base.JC_CLH_LoadListPlan != value)
				{
					base.JC_CLH_LoadListPlan = value;
					Consol?.MarkAsNeedingValidation();
				}
			}
		}

		public CFSContainerLoadList ContainerLoadPlan => JC_CLH_LoadListPlan.IsEmpty ? null : Factory.Load<CFSContainerLoadList>(JC_CLH_LoadListPlan);

		public override IContainerPenaltyCalculateHandler[] ContainerPenaltyCalculateHandlers
		{
			get
			{
				return new IContainerPenaltyCalculateHandler[] { new ContainerPenaltyCalculateHandlerForConsol(this), new ContainerPenaltyCalculateHandlerForShipment(this) };
			}
		}

		#region RelatedContainerLoadList

		[List("Lookups.RelatedContainerLoadListCollection")]
		public override ZGuid RelatedContainerLoadListPK => AdvOrmFeatureHelper.IsEnabled && JC_CLH_LoadListPlan.IsEmpty && !JC_JSB_SupplierBooking.IsEmpty && IsAttachedToContainerLoadList() ? ContainerLoadListLines[0].LoadListHeader.PK : ZGuid.Empty;

		public override ZBool RelatedContainerLoadListVisible => AdvOrmFeatureHelper.IsEnabled && !RelatedContainerLoadListPK.IsEmpty;

		public override ZBool RelatedSupplierBookingVisible => AdvOrmFeatureHelper.IsEnabled && RelatedContainerLoadListPK.IsEmpty;

		#endregion

		#region RelatedContainerLoadPlan

		public override bool JC_JSB_SupplierBooking_ReadOnly => !JC_CLH_LoadListPlan.IsEmpty;

		public override ZBool RelatedContainerLoadPlanVisible => AdvOrmFeatureHelper.IsEnabled;

		public override bool JC_CLH_LoadListPlan_ReadOnly => !JC_JSB_SupplierBooking.IsEmpty;

		#endregion

		#region TotalCO2e

		public ZString TotalCO2eForEmptyPickupForBinding => this.GetTotalCO2eForBinding(CO2eTypes.EmptyPickup);

		public ZPropertyInfo TotalCO2eForEmptyPickupForBindingInfo => GetZPropertyInfo(nameof(TotalCO2eForEmptyPickupForBinding));

		public ZString TotalCO2eForEmptyReturnForBinding => this.GetTotalCO2eForBinding(CO2eTypes.EmptyReturn);

		public ZPropertyInfo TotalCO2eForEmptyReturnForBindingInfo => GetZPropertyInfo(nameof(TotalCO2eForEmptyReturnForBinding));

		void RefreshCO2eEmptyPickupBinding()
		{
			if (this.JobCO2eExists(CO2eTypes.EmptyPickup))
			{
				if (!IsValidationSuspended)
				{
					Validation.ValidateTotalCO2eForEmptyPickupForBinding();
				}
				TotalCO2eForEmptyPickupForBindingInfo.RefreshBinding();
			}
		}

		void RefreshCO2eEmptyReturnBinding()
		{
			if (this.JobCO2eExists(CO2eTypes.EmptyReturn))
			{
				if (!IsValidationSuspended)
				{
					Validation.ValidateTotalCO2eForEmptyReturnForBinding();
				}
				TotalCO2eForEmptyReturnForBindingInfo.RefreshBinding();
			}
		}

		#endregion

		#region ICO2eParent

		[JobCO2eTypes(CO2eTypes.EmptyPickup, CO2eTypes.EmptyReturn)]
		public IJobCO2eCollection JobCO2eCollection
		{
			get
			{
				if (jobCO2eCollection == null)
				{
					jobCO2eCollection = new JobCO2eCollection(this);
					jobCO2eCollection.JobCO2e_StatusChanged += CO2eStatusChanged;
				}
				return jobCO2eCollection;
			}
		}
		IJobCO2eCollection jobCO2eCollection;

		ZGuid ICO2eParent.JobCO2eParentID => PK;

		ZString ICO2eParent.JobCO2eParentTableCode => TablePrefix;

		void ICO2eParent.RefreshCO2e()
		{
			CalculateTotalCO2e();
			RefreshCO2eEmptyPickupBinding();
			RefreshCO2eEmptyReturnBinding();
		}

		void CalculateTotalCO2e()
		{
			if (this.JobCO2eExists(CO2eTypes.EmptyPickup))
			{
				this.SetTotalCO2e(TotalCO2e(this, CO2eTypes.EmptyPickup), CO2eTypes.EmptyPickup);
			}

			if (this.JobCO2eExists(CO2eTypes.EmptyReturn))
			{
				this.SetTotalCO2e(TotalCO2e(this, CO2eTypes.EmptyReturn), CO2eTypes.EmptyReturn);
			}

			ZDecimal TotalCO2e(ICO2eEmptyContainerProvider provider, ZString type)
			{
				return provider.RequireTEU
					? provider.NumberOfTEU * this.GetCO2ePerTEUInKg(type)
					: Weight.ConvertSafe(provider.TotalWeight, provider.TotalWeightUnit, Weight.Tonnes) * provider.GetCO2ePerTonneInKg(type);
			}
		}

		void CO2eStatusChanged(object sender, EventArgs e)
		{
			if (sender is JobCO2e jobCO2)
			{
				if (jobCO2.JCO_Type == CO2eTypes.EmptyPickup)
				{
					RefreshCO2eEmptyPickupBinding();
				}
				if (jobCO2.JCO_Type == CO2eTypes.EmptyReturn)
				{
					RefreshCO2eEmptyReturnBinding();
				}
				if (sender is JobCO2e jobco2e)
				{
					CO2eStatusChangedEvent?.Invoke(this, jobco2e);
				}
			}
		}

		#endregion

		#region ICO2eEmptyContainerProvider

		bool ICO2eProvider.RequireTEU => ((ICO2eEmptyContainerProvider)this).NumberOfTEU > 0;

		void ICO2eProvider.RecordLog(CO2eEventType type, string extra, decimal previousCO2eValue) { }

		bool ICO2eTEUProvider.IncludeTEU => ((ICO2eProvider)this).RequireTEU;

		ZDecimal ICO2eTEUProvider.NumberOfTEU => JC_Calc_TEUCount;

		ZDecimal ICO2eTEUProvider.TonnesPerTEU => 0;

		ZDecimal ICO2eTEUProvider.ContainerEmptyWeightPerTEU => JC_Calc_TEUCount == 0 ? 0 : (JC_TareWeight + JC_DunnageWeight) / JC_Calc_TEUCount;

		ZString ICO2eTEUProvider.ContainerEmptyWeightPerTEUUnit => ContainerWeightUnit;

		ZString ICO2eEmptyContainerProvider.ConsolNumber => Consol.JK_UniqueConsignRef;

		ZString ICO2eEmptyContainerProvider.ContainerNumber => JC_ContainerNum;

		ZString ICO2eEmptyContainerProvider.ContainerJobID => JC_ContainerJobID;

		ZDecimal ICO2eEmptyContainerProvider.TotalWeight => JC_Calc_TareWeight;

		ZString ICO2eEmptyContainerProvider.TotalWeightUnit => ContainerWeightUnit;

		EmptyContainerAddressResolver ICO2eEmptyContainerProvider.EmptyPickupAddress =>
			(ICommonShipment shipment) => GetEmptyContainerAddressForCO2e(shipment as CommonShipment, true);

		EmptyContainerAddressResolver ICO2eEmptyContainerProvider.EmptyReturnAddress =>
			(ICommonShipment shipment) => GetEmptyContainerAddressForCO2e(shipment as CommonShipment, false);

		(OrgAddress, OrgAddress, string) GetEmptyContainerAddressForCO2e(CommonShipment shipment, bool isPickup)
		{
			var consol = Consol;
			if (shipment is null || consol is null)
			{
				return (null, null, null);
			}

			OrgAddress from, to;
			var isFCLDRT = consol.JK_ConsolMode == Constants.ContainerModes.FCL && consol.JK_AgentType == Constants.AgentType.Direct;
			if (isPickup)
			{
				from = DepartureContainerYardAddress ?? consol.ContainerYardEmptyPickupAddress;
				to = consol.PackDepotAddress;
				if (isFCLDRT)
				{
					to = shipment.ExportReceivingDepot ?? shipment.ConsignorPickupAddress.Address ?? to;
				}
			}
			else
			{
				from = consol.UnpackDepotAddress;
				to = ArrivalContainerYardAddress ?? consol.ContainerYardEmptyReturnAddress;
				if (isFCLDRT)
				{
					from = shipment.ImportReleaseDepot ?? shipment.ConsigneeDeliveryAddress.Address ?? from;
				}
			}
			var transportMode = isPickup ? EmptyPickupByTransportMode : EmptyReturnToTransportMode;
			return (from, to, string.IsNullOrEmpty(transportMode) ? TransportModes.Road : transportMode);
		}

		#endregion

		#region Transport Mode Bindings

		[List("PreCarriageOnCarriageTransportMode_List")]
		[MaxLength(3)]
		public override ZString EmptyPickupByTransportMode
		{
			get
			{
				return base.EmptyPickupByTransportMode;
			}
			set
			{
				var previousValue = base.EmptyPickupByTransportMode;
				if (previousValue != value)
				{
					base.EmptyPickupByTransportMode = value;
					this.UpdateCO2eStatusToNotCurrent(new CO2eStatusChangedReason(EmptyPickupByTransportModeInfo, previousValue), IsCopying, CO2eTypes.EmptyPickup);
				}
			}
		}

		[List("PreCarriageOnCarriageTransportMode_List")]
		[MaxLength(3)]
		public override ZString EmptyReturnToTransportMode
		{
			get
			{
				return base.EmptyReturnToTransportMode;
			}
			set
			{
				var previousValue = base.EmptyReturnToTransportMode;
				if (previousValue != value)
				{
					base.EmptyReturnToTransportMode = value;
					this.UpdateCO2eStatusToNotCurrent(new CO2eStatusChangedReason(EmptyReturnToTransportModeInfo, previousValue), IsCopying, CO2eTypes.EmptyReturn);
				}
			}
		}

		public event EventHandler<JobCO2e> CO2eStatusChangedEvent;

		#endregion
	}
}
