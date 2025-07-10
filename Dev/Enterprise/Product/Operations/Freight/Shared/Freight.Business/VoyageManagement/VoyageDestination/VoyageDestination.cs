using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Customs.Common.US.AMS;
using Enterprise.Environment;
using Enterprise.Freight.AIS;
using Enterprise.Freight.CarbonEmissions.Business;
using Enterprise.Freight.Common.Business;
using Enterprise.Freight.Integration;
using Enterprise.Integration;
using Enterprise.Integration.Freight;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.MasterFiles.Integration;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using Constants = Enterprise.Core.Constants;
using EventConstants = CargoWise.EventReference.Constants;
using Params = CargoWise.EventReference.Constants.EventReferenceParameters.Codes;

namespace Enterprise.Freight.Business
{
	[UserDefinedValues]
	[CodeProperty(AutoJobVoyDestination.Schema.JB_RL_NKPortOfDischarge), DescriptionProperty(VoyageDestination.Schema.Description)]
	[System.Diagnostics.DebuggerDisplay("Discharge = {JB_RL_NKPortOfDischarge}")]
	[ActionFieldFollow(false)]
	public class VoyageDestination : AutoJobVoyDestination,
		IVoyageDestination,
		ISendersMessageReferenceProvider,
		ISailingEndPoint,
		IScheduleChangeParent,
		IWorkflowTriggerFieldChangeSource,
		IStowPlanMessageAttachee,
		IScheduleChangeEmailSupporter,
		IEventDatePropertyChecker,
		ITrackableVoyagePort,
		IVesselMovementsUrlSupporter,
		IPortMatchingSupport,
		IPortMatchingHandler
	{
		#region Schema

		public new class Schema : AutoJobVoyDestination.Schema
		{
			public const string Description = "Description";
			public const string JB_Calc_ArrivalCTOAddressOrg = "JB_Calc_ArrivalCTOAddressOrg";
			public const string JB_Calc_ArrivalCTOAddressCode = "JB_Calc_ArrivalCTOAddressCode";
			public const string JB_Calc_ArrivalCTOPremiseID = "JB_Calc_ArrivalCTOPremiseID";
			public const string StowPlanMessageStatus = VoyageOrigin.Schema.StowPlanMessageStatus;
		}

		#endregion

		public VoyageDestination(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
			JB_OA_ArrivalCTOAddress_ZAddress.DefaultAddressType = AddressType.PIC;
			DateTimeOffsetRelatedPortHelper.HookDateTimeOffsetProperties(this);
		}

		#region Updated By Data Refresh Event Handler

		internal event EventHandler DestinationUpdatedByDataRefresh;

		void OnDestinationUpdatedByDataRefresh()
		{
			if (DestinationUpdatedByDataRefresh != null)
			{
				DestinationUpdatedByDataRefresh(this, EventArgs.Empty);
			}
		}

		#endregion

		#region Validation

		public new BaseJobVoyDestinationValidation Validation
		{
			get { return (BaseJobVoyDestinationValidation)base.Validation; }
		}

		protected override JobVoyDestinationValidation GetNewValidation()
		{
			var result = new BaseJobVoyDestinationValidation(this);
			if (AdditionalValidation != null)
			{
				result.Add(AdditionalValidation);
			}

			return result;
		}

		public JobVoyDestinationValidation AdditionalValidation { get; set; }

		#endregion

		#region Business Object Overrides

		protected override ZString HumanReadableNameCore
		{
			get
			{
				if (Voyage == null)
				{
					return Res.GetString("b87fef99-5380-4ed8-831a-69f75df31955", "Destination = '{0}'", JB_RL_NKPortOfDischarge);
				}
				else
				{
					return Res.GetString("a8194a36-df8c-4b23-8511-51b157187044", "{0}, Destination = '{1}'", Voyage.HumanReadableName, JB_RL_NKPortOfDischarge);
				}
			}
		}

		protected override void OnUpdatedByDataRefresh()
		{
			base.OnUpdatedByDataRefresh();
			OnDestinationUpdatedByDataRefresh();
		}

		protected override IEnumerable<string> GetPropertiesToExcludeFromCloning()
		{
			var result = new List<string>(base.GetPropertiesToExcludeFromCloning())
			{
				JobVoyDestinationSchema.Constants.JB_JV
			};

			return result;
		}

		protected override bool SupportsCloneCore()
		{
			return true;
		}

		public override void OnSaving()
		{
			base.OnSaving();
			((ISendersMessageReferenceProvider)this).PopulateSendersReferenceIfNeeded();
			new JobScheduleChangeLogger().LogDestinationDateChanges(this);
			CalculateJC_EmptyReturnedBy_ForContainersIfRequired();

			if (Voyage != null)
			{
				CreateOrUpdateVoyageLogsOnSaving();
				ScheduleFeedServiceManager.Update(this, Voyage, Factory);
				UpdateFlightSubscriptionEvent();
				UpdateRelatedBookedAgencyBookingEvent();
			}

			shouldUpdateShippingContainers = JB_AvailabilityDateInfo.HasChanges;

			OnDestinationSaving();
		}

		bool shouldUpdateShippingContainers;

		protected virtual void OnDestinationSaving()
		{
			DestinationSaving?.Invoke(this, EventArgs.Empty);
		}

		public event EventHandler DestinationSaving;

		void CancelOldEvent(Event evnt, bool? isEstimate = null)
		{
			var log = Voyage.Logs.MostRecentLogByEventTime(evnt, FromPort((ZString)JB_RL_NKPortOfDischargeInfo.OriginalValue));
			if (log == null)
			{
				return;
			}

			if (isEstimate.HasValue && isEstimate.Value != log.SL_IsEstimate)
			{
				return;
			}

			log.Cancel();
		}

		void UpdateFlightSubscriptionEvent()
		{
			foreach (JobSailing sailing in FetchSailings())
			{
				FlightMonitoringSystemManager.UpdateFlightSubscriptionEvent(sailing);
			}
		}

		void UpdateRelatedBookedAgencyBookingEvent()
		{
			if (JB_RL_NKPortOfDischargeInfo.HasChanges || JB_E_ARVInfo.HasChanges)
			{
				RelatedAgencyBookingsEventLogHelper.UpdateRelatedBookedAgencyBookingEvent(FetchSailings());
			}
		}

		void CreateOrUpdateVoyageLogsOnSaving()
		{
			if (JB_RL_NKPortOfDischargeInfo.HasChanges)
			{
				CancelOldEvent(AutoEvents.Arrival, false);
				CancelOldEvent(AutoEvents.Arrival, true);
				CancelOldEvent(AutoEvents.CargoAvailable);
			}

			if (!IsInDatabase && JB_A_ARV.IsValid || JB_A_ARVInfo.HasChanges || JB_RL_NKPortOfDischargeInfo.HasChanges)
			{
				Voyage.Logs.CreateRecreateOrUpdateEventLog(Events.Arrival, EstimateActual.Actual, JB_A_ARV.ToOffset(), ZString.Empty, GetParametersForEvent(Events.Arrival));
			}

			if (!IsInDatabase && JB_E_ARV.IsValid || JB_E_ARVInfo.HasChanges || JB_RL_NKPortOfDischargeInfo.HasChanges)
			{
				using (SuspendUpdatingARVPropertiesFromLogs())
				{
					Voyage.Logs.CreateRecreateOrUpdateEventLog(Events.Arrival, EstimateActual.Estimate, JB_E_ARV.ToOffset(), ZString.Empty, GetParametersForEvent(Events.Arrival));
				}
			}

			if (!IsInDatabase && JB_AvailabilityDate.IsValid || JB_AvailabilityDateInfo.HasChanges || JB_RL_NKPortOfDischargeInfo.HasChanges)
			{
				CreateRecreateOrUpdateCAVEventLog();
			}

			if (!IsInDatabase && JB_StorageDate.IsValid || JB_StorageDateInfo.HasChanges || JB_RL_NKPortOfDischargeInfo.HasChanges)
			{
				Voyage.Logs.CreateRecreateOrUpdateEventLog(Events.StorageCommenced, EstimateActual.Actual, JB_StorageDate.ToOffset(), ZString.Empty, GetParametersForEvent(Events.StorageCommenced));
			}

			if (JB_AvailabilityDate.IsValid || JB_AvailabilityDateInfo.HasChanges)
			{
				var action = JB_AvailabilityDate.IsEmpty ? (NoResString)"deleted" : string.Format((NoResString)"changed to {0}", JB_AvailabilityDate.ToShortDateString());
				var reference = string.Format((NoResString)"Availability Date for Discharge Port {0} {1}.", JB_RL_NKPortOfDischarge, action);

				Voyage.Logs.CreateOrRecreateEventLog(Events.EditedARecord, EstimateActual.Actual, ZDateTimeOffset.Now, reference);
			}
		}

		void CreateRecreateOrUpdateCAVEventLog()
		{
			if ((Voyage?.ParentConsol?.JK_ConsolMode ?? null) == Constants.ContainerModes.Groupage)
			{
				var ctoParameters = new Dictionary<string, string>();
				ctoParameters[Params.Facility] = EventConstants.Facilities.Code.Terminal;
				ctoParameters[Params.Location] = JB_RL_NKPortOfDischarge;
				Voyage.Logs.CreateRecreateOrUpdateEventLog(Events.CargoAvailable, EstimateActual.Actual, JB_AvailabilityDate.ToOffset(), ZString.Empty, ctoParameters.ToArray());

				var cfsParameters = new Dictionary<string, string>();
				cfsParameters[Params.Facility] = EventConstants.Facilities.Code.Depot;
				cfsParameters[Params.Location] = JB_RL_NKPortOfDischarge;
				Voyage.Logs.CreateRecreateOrUpdateEventLog(Events.CargoAvailable, EstimateActual.Actual, JB_AvailabilityDate.ToOffset(), ZString.Empty, cfsParameters.ToArray());
			}
			else
			{
				Voyage.Logs.CreateRecreateOrUpdateEventLog(Events.CargoAvailable, EstimateActual.Actual, JB_AvailabilityDate.ToOffset(), ZString.Empty, GetParametersForEvent(Events.CargoAvailable));
			}
		}

		static Func<StmALog, bool> FromPort(string portUnloco)
		{
			return (StmALog log) => { return log.Parameters.GetValueOrDefault(Params.Location) == portUnloco; };
		}

		KeyValuePair<string, string>[] GetParametersForEvent(Event eventType)
		{
			var parameters = new Dictionary<string, string>();

			if (eventType == Events.Arrival)
			{
				parameters[Params.Facility] = CargoWise.EventReference.Constants.Facilities.Code.Terminal;
				parameters[Params.Location] = JB_RL_NKPortOfDischarge;
				parameters[Params.Mode] = Voyage != null ? Voyage.JV_AirSeaRoad : ZString.Empty;
				if (Voyage.IsAir)
				{
					parameters[Params.VoyageFlightNumber] = Voyage != null ? Voyage.JV_VoyageFlight : ZString.Empty;
					parameters[Params.FlightDate] = Voyage != null && Voyage.JV_FlightDate.IsValid
													? Voyage.JV_FlightDate.ToISO8601ShortDateString()
													: string.Empty;
				}
			}
			else if (eventType == Events.CargoAvailable
				|| eventType == Events.StorageCommenced)
			{
				parameters[Params.Facility] = CargoWise.EventReference.Constants.Facilities.Code.Terminal;
				parameters[Params.Location] = JB_RL_NKPortOfDischarge;
			}

			return parameters.ToArray();
		}

		protected override void OnFactorySavingBeforeTransactionCore()
		{
			base.OnFactorySavingBeforeTransactionCore();
			if (!IsDeleted && HasChanges)
			{
				JobVoyage loadedVoyageSoAutoAdminLogIsRaised = Voyage;
			}
		}

		public override void OnSaved(bool saveSucceeded)
		{
			base.OnSaved(saveSucceeded);
			if (!saveSucceeded && !IsInDatabase)
			{
				JB_SendersMessageReference = ZString.Empty;
			}

			if (shouldUpdateShippingContainers && saveSucceeded)
			{
				TryCalculateAndSaveJC_EmptyReturnedBy_ForAgencyBillOfLadingContainers();
			}

			shouldUpdateShippingContainers = false;
		}

		public override void Delete()
		{
			base.Delete();
			Messages.RemoveAndDeleteAll();
		}

		protected override EnterpriseBusinessObjectFetchStrategy GetFetchStrategyCore()
		{
			return new VoyageDependentBizObjFetchStrategy(this, JB_JV);
		}

		#endregion

		#region Related Business Objects

		IJobVoyage IVoyageDestination.Voyage => Voyage;

		public JobVoyage Voyage
		{
			get { return Factory.Load<JobVoyage>(JB_JV); }
		}

		public RefCountry Country
		{
			get { return (PortOfDischarge != null) ? PortOfDischarge.Country : null; }
		}

		#region Messages

		EDIMessageCollection messages;
		public EDIMessageCollection Messages
		{
			get
			{
				if (messages == null)
				{
					messages = new EDIMessageCollection(this, Factory);
					messages.Load();
					messages.IsManagedForDataRefresh = true;
				}
				return messages;
			}
		}

		#endregion

		#region FetchSailings

		public JobSailing[] FetchSailings()
		{
			return Factory.Load<JobSailing>(new ZQuery(JobSailingSchema.JX_JB, PK));
		}

		#endregion

		#endregion

		#region Properties

		#region JB_AutoCreated

		public override ZBool JB_AutoCreated
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return base.JB_AutoCreated; }
			set
			{
				base.JB_AutoCreated = value;
				MarkVoyageAsNeedingValidation();
			}
		}

		#endregion

		#region JB_JV

		[RelatedBusinessObject("Voyage")]
		public override ZGuid JB_JV
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return base.JB_JV; }
			set
			{
				JobVoyage oldVoyage = Voyage;

				base.JB_JV = value;
				MarkVoyageAsNeedingValidation();

				if (oldVoyage != null)
				{
					oldVoyage.MarkAsNeedingValidation();
				}
			}
		}

		#endregion

		#region JB_RL_NKPortOfDischarge

		[List("Lookups.PortOfDischarges")]
		public override ZString JB_RL_NKPortOfDischarge
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return base.JB_RL_NKPortOfDischarge; }
			set
			{
				if (value != JB_RL_NKPortOfDischarge)
				{
					base.JB_RL_NKPortOfDischarge = value;
					if (Voyage != null)
					{
						NotifySailingDischargePortChanged();
						SailingScheduleDataVendor.Instance.UpdateVoyageDestination(this);
						RunOnlineFlightMatchingForAllSailings();

						Voyage.RequiresSailingGeneration = true;
					}

					if (!IsValidationSuspended)
					{
						Validation.ValidateJB_A_ARV();
					}
					DefaultCTOFromCarrier();
					MarkVoyageAsNeedingValidation();
					MarkOriginsAsNeedingValidation();
				}
			}
		}

		void NotifySailingDischargePortChanged()
		{
			foreach (JobSailing sailing in Voyage.Sailings)
			{
				if (sailing.JX_JB == PK)
				{
					sailing.UpdateCO2eStatusToNotCurrent(new CO2eStatusChangedReason(JB_RL_NKPortOfDischargeInfo));
				}
			}
		}

		#endregion

		#region JB_E_ARV

		[EventDateProperty(Events.ArrivalCode, EstimateActual.Estimate)]
		public override ZDateTime JB_E_ARV
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return new ZDateTime(base.JB_E_ARV, DateTimeKind.Unspecified); }
			set
			{
				ZDateTime oldETA = base.JB_E_ARV;

				if (oldETA != value)
				{
					base.JB_E_ARV = value;

					if (Voyage != null)
					{
						IScheduleUpdateSubscriber[] subscribers = ScheduleUpdateSubscriberFactory.GetSubscribers(Factory);
						subscribers.ETAChanged(new ScheduleUpdateServices(Voyage), this, oldETA);
						RunOnlineFlightMatchingForAllSailings();

						Voyage.RequiresSailingGeneration = true;
					}

					if (JB_E_ARV.IsValid && !JB_S_ARV.IsValid)
					{
						JB_S_ARV = JB_E_ARV;
					}

					MarkVoyageAsNeedingValidation();
					MarkOriginsAsNeedingValidation();
				}
			}
		}

		#endregion

		#region JB_E_ARV_UTC

		public ZDateTime JB_E_ARV_UTC
		{
			get { return JB_E_ARV.IsValid ? Env.Time.GetUtcFromUnlocoTime(JB_RL_NKPortOfDischarge.ToString(), JB_E_ARV.ToDateTime()) : ZDateTime.Empty; }
		}

		#endregion

		#region JB_A_ARV

		[EventDateProperty(Events.ArrivalCode, EstimateActual.Actual)]
		public override ZDateTime JB_A_ARV
		{
			get { return new ZDateTime(base.JB_A_ARV, DateTimeKind.Unspecified); }
			set
			{
				if (base.JB_A_ARV != value)
				{
					base.JB_A_ARV = value;
				}
			}
		}

		#endregion

		#region JB_A_ARV_UTC

		public ZDateTime JB_A_ARV_UTC
		{
			get { return JB_A_ARV.IsValid ? Env.Time.GetUtcFromUnlocoTime(JB_RL_NKPortOfDischarge.ToString(), JB_A_ARV.ToDateTime()) : ZDateTime.Empty; }
		}

		#endregion

		#region JB_S_ARV

		public override ZDateTime JB_S_ARV
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return new ZDateTime(base.JB_S_ARV, DateTimeKind.Unspecified); }
			set
			{
				if (base.JB_S_ARV != value)
				{
					base.JB_S_ARV = value;

					if (JB_S_ARV.IsValid && !JB_E_ARV.IsValid)
					{
						JB_E_ARV = JB_S_ARV;
					}
				}
			}
		}

		#endregion

		#region JB_S_ARV_UTC

		public ZDateTime JB_S_ARV_UTC
		{
			get { return JB_S_ARV.IsValid ? Env.Time.GetUtcFromUnlocoTime(JB_RL_NKPortOfDischarge.ToString(), JB_S_ARV.ToDateTime()) : ZDateTime.Empty; }
		}

		#endregion

		#region JB_AvailabilityDate

		[EventDateProperty(Events.CargoAvailableCode, EstimateActual.Actual, false)]
		public override ZDateTime JB_AvailabilityDate
		{
			get { return new ZDateTime(base.JB_AvailabilityDate, DateTimeKind.Unspecified); }
			set { base.JB_AvailabilityDate = value; }
		}

		#endregion

		#region JB_StorageDate

		[EventDateProperty(Events.StorageCommencedCode, EstimateActual.Actual)]
		public override ZDateTime JB_StorageDate
		{
			get { return new ZDateTime(base.JB_StorageDate, DateTimeKind.Unspecified); }
			set { base.JB_StorageDate = value; }
		}

		#endregion

		#region JB_OA_ArrivalCTOAddress

		public override ZGuid JB_OA_ArrivalCTOAddress
		{
			get { return base.JB_OA_ArrivalCTOAddress; }
			set
			{
				if (value != JB_OA_ArrivalCTOAddress)
				{
					base.JB_OA_ArrivalCTOAddress = value;
					JB_Calc_ArrivalCTOAddressOrgInfo.RefreshBinding();
					JB_Calc_ArrivalCTOAddressCodeInfo.RefreshBinding();
				}
			}
		}

		#endregion

		[DateTimeOffsetRelatedPort(nameof(JB_RL_NKFirstDischargePort))]
		public override ZDateTimeOffset JB_FirstDischargePortETA
		{
			get { return base.JB_FirstDischargePortETA; }
			set { base.JB_FirstDischargePortETA = value; }
		}

		[DateTimeOffsetRelatedPort(nameof(JB_RL_NKLastForeignPort))]
		public override ZDateTimeOffset JB_LastForeignPortETD
		{
			get { return base.JB_LastForeignPortETD; }
			set { base.JB_LastForeignPortETD = value; }
		}

		#region Description

		public ZString Description
		{
			get { return JB_E_ARV.ToString(); }
		}

		#endregion

		#region JB_Calc_ArrivalCTOAddressOrg

		[List("Lookups.CTO_List")]
		public ZGuid JB_Calc_ArrivalCTOAddressOrg
		{
			get { return JB_OA_ArrivalCTOAddress_ZAddress.OrgPK; }
			set
			{
				JB_OA_ArrivalCTOAddress_ZAddress.OrgPK = value;
				JB_Calc_ArrivalCTOAddressOrgInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo JB_Calc_ArrivalCTOAddressOrgInfo
		{
			get { return GetZPropertyInfo(Schema.JB_Calc_ArrivalCTOAddressOrg); }
		}

		#endregion

		#region JB_Calc_ArrivalCTOAddressCode

		[BusinessObjectTestExclude]
		[List("Lookups.JB_ArrivalCTOAddress_List")]
		[MaxLength(OrgAddress.Schema.OA_CodeMaxLength)]
		public ZString JB_Calc_ArrivalCTOAddressCode
		{
			get
			{
				var address = Factory.Load<OrgAddress>(JB_OA_ArrivalCTOAddress);
				return (address != null) ? address.OA_Code : ZString.Empty;
			}
			set
			{
				ZQuery filter = new ZQuery(OrgAddressSchema.OA_OH, JB_Calc_ArrivalCTOAddressOrg);
				filter.AddToFilter(OrgAddressSchema.OA_Code, value);
				var address = Factory.LoadTop1<OrgAddress>(filter);
				JB_OA_ArrivalCTOAddress = (address != null) ? address.PK : ZGuid.Empty;
				JB_Calc_ArrivalCTOAddressCodeInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo JB_Calc_ArrivalCTOAddressCodeInfo
		{
			get { return GetZPropertyInfo(Schema.JB_Calc_ArrivalCTOAddressCode); }
		}

		protected bool JB_Calc_ArrivalCTOAddressCode_ReadOnly
		{
			get { return !JB_Calc_ArrivalCTOAddressOrg.IsValid; }
		}

		#endregion

		#region JB_Calc_ArrivalCTOPremiseID

		[BusinessObjectTestExclude]
		[MaxLength(8)]
		public ZString JB_Calc_ArrivalCTOPremiseID
		{
			get
			{
				var address = Factory.Load<OrgAddress>(JB_OA_ArrivalCTOAddress);
				return (address != null) ? address.LocalControlledPremisesID : ZString.Empty;
			}
		}

		public ZPropertyInfo JB_Calc_ArrivalCTOPremiseIDInfo
		{
			get { return GetZPropertyInfo(Schema.JB_Calc_ArrivalCTOPremiseID); }
		}

		#endregion

		#region JB_ArrivalReferenceFieldType

		public override ZString JB_ArrivalReference
		{
			get { return base.JB_ArrivalReference; }
			set
			{
				base.JB_ArrivalReference = value;
				FetchSailings().ForEach(sailing => sailing.JX_ArrivalPortRouteId = value);
			}
		}

		#endregion

		#region JB_ArrivalReferenceFieldType

		public ZString JB_ArrivalReferenceFieldType
		{
			get
			{
				return JB_RL_NKPortOfDischarge.StartsWith(Constants.CountryCodes.SouthAfrica, StringComparison.OrdinalIgnoreCase)
						&& GlbCompany.CurrentCompany.GC_RN_NKCountryCode == Constants.CountryCodes.SouthAfrica
						&& IsSea
					? nameof(FieldType.TextCodeFindBox)
					: nameof(FieldType.Text);
			}
		}

		#endregion

		#endregion

		#region Calculated Properties

		public ZBool IsSea
		{
			get
			{
				return (Voyage != null && Voyage.IsSea);
			}
		}

		#endregion

		#region Code Description Pair List

		protected BindToLists BindingLists
		{
			get { return BindToLists.GetCachedLists(Factory); }
		}

		#endregion

		#region Populating JC_EmptyReturnedBy from Detention Free Days

		void CalculateJC_EmptyReturnedBy_ForContainersIfRequired()
		{
			bool fclAvailableDateHasChanges = JB_AvailabilityDate.IsValid && (!IsInDatabase || JB_AvailabilityDateInfo.HasChanges);

			if (fclAvailableDateHasChanges && Voyage != null)
			{
				foreach (JobSailing sailing in FetchSailings())
				{
					foreach (CommonContainer container in sailing.FreightContainers)
					{
						if (container.JC_EmptyReturnedBy.IsEmpty)
						{
							container.CalculateJC_EmptyReturnedBy();
						}
					}
				}
			}
		}

		void TryCalculateAndSaveJC_EmptyReturnedBy_ForAgencyBillOfLadingContainers()
		{
			if (!IsDeleted && IsSea)
			{
				var newFactory = new BusinessObjectFactory();
				var sailings = newFactory.Load<JobSailing>(new ZQuery(JobSailingSchema.JX_JB, PK));

				foreach (var sailing in sailings)
				{
					DetentionDatesUpdater.UpdateContainerDetentionDateFromSailing(sailing);
				}

				ZExceptionReporting.ProcessWithSaveExceptionHandling(newFactory.Save, null);
			}
		}

		IDetentionDatesUpdater DetentionDatesUpdater
		{
			get { return detentionDatesUpdater ?? (detentionDatesUpdater = ObjectFactory.Get<IDetentionDatesUpdater>()); }
		}

		IDetentionDatesUpdater detentionDatesUpdater;

		#endregion

		#region Implementation

		void DefaultCTOFromCarrier()
		{
			CTOFromCarrierDefaulter.SetCTOFromCarrier(Voyage, JB_RL_NKPortOfDischarge, OrgConstants.CarrierAgentDirections.Code.Arrival, JB_OA_ArrivalCTOAddressInfo, JB_Calc_ArrivalCTOAddressOrgInfo);
		}

		void MarkVoyageAsNeedingValidation()
		{
			JobVoyage voyage = this.Voyage;
			if (voyage != null)
			{
				voyage.MarkAsNeedingValidation();
			}
		}

		void MarkOriginsAsNeedingValidation()
		{
			JobVoyage voyage = this.Voyage;
			if (voyage != null)
			{
				voyage.Origins.MarkAsNeedingValidation();
			}
		}

		void RunOnlineFlightMatchingForAllSailings()
		{
			using (Factory.GetValue<IBusyIndicatorProvider>()?.NewBusyIndicator())
			{
				FetchSailings()?.ForEach(sailing => sailing.TryMatchAgainstOnlineFlights());
			}
		}

		#endregion

		#region ISendersMessageReferenceProvider Members

		void ISendersMessageReferenceProvider.PopulateSendersReferenceIfNeeded()
		{
			if (JB_SendersMessageReference.IsEmpty
				&& TryGenerateMessageReferenceNumber(out var generatedNumbers)
				&& generatedNumbers.Length > 0)
			{
				JB_SendersMessageReference = generatedNumbers[0];
			}
		}

		bool TryGenerateMessageReferenceNumber(out string[] generatedNumbers)
		{
			if (Factory is IDbConnected connected)
			{
				var fountain = Env.NumberFountains.VoyageDestinationNumber(Schema.JB_SendersMessageReferenceMaxLength);
				generatedNumbers = fountain.GetNextsFormatted(connected.Connection, 1);
				return true;
			}

			generatedNumbers = Array.Empty<string>();
			return false;
		}

		ZString ISendersMessageReferenceProvider.SendersReference
		{
			get
			{
				return JB_SendersMessageReference;
			}
		}

		#endregion

		#region ISailingEndPoint Members

		ZString ISailingEndPoint.Port
		{
			get { return JB_RL_NKPortOfDischarge; }
		}

		ZDateTime ISailingEndPoint.EstimatedDate
		{
			get { return JB_E_ARV; }
		}

		ZString ISailingEndPoint.Direction
		{
			get { return Constants.PortDirection.Discharge; }
		}

		ZString ISailingEndPoint.Vessel
		{
			get { return Voyage == null ? ZString.Empty : Voyage.JV_RV_NKVessel; }
		}

		ZString ISailingEndPoint.Voyage
		{
			get { return Voyage == null ? ZString.Empty : Voyage.JV_VoyageFlight; }
		}

		#endregion

		#region GetNewLookups

		public new BaseJobVoyDestinationLookups Lookups
		{
			get { return lookups ?? (lookups = (BaseJobVoyDestinationLookups)GetNewLookups()); }
		}
		BaseJobVoyDestinationLookups lookups;

		protected override JobVoyDestinationLookups GetNewLookups()
		{
			return new BaseJobVoyDestinationLookups(this);
		}

		#endregion

		#region IScheduleChangeParent Members

		SchemaGuidColumn IScheduleChangeParent.SailingRefColumn
		{
			get { return JobSailingSchema.JX_JB; }
		}

		ZString IScheduleChangeParent.OriginPort
		{
			get { return ""; }
		}

		ZString IScheduleChangeParent.DestinationPort
		{
			get { return JB_RL_NKPortOfDischarge; }
		}

		#endregion

		#region IScheduleChangeEmailSupporter Members

		Dictionary<string, string> JobScheduleChangeDataProviderList
		{
			get { return jobScheduleChangeDataProviderList ?? (jobScheduleChangeDataProviderList = new Dictionary<string, string>()); }
		}
		Dictionary<string, string> jobScheduleChangeDataProviderList;

		void IScheduleChangeEmailSupporter.AddOrUpdateJobScheduleChange(string dateType, string dataProvider)
		{
			JobScheduleChangeDataProviderList[dateType] = dataProvider;
		}

		string IScheduleChangeEmailSupporter.GetDataProvider(string dateType)
		{
			var result = (JobScheduleChangeDataProviderList.ContainsKey(dateType))
				? JobScheduleChangeDataProviderList[dateType]
				: string.Empty;

			return result;
		}

		#endregion

		#region IWorkflowTriggerFieldChangeSource Members

		IReadOnlyList<IWorkflowProvider> IWorkflowTriggerFieldChangeSource.ParentWorkflowProviders
		{
			get
			{
				var relatedProvider = Voyage;

				return relatedProvider == null
					? Array.Empty<IWorkflowProvider>()
					: new IWorkflowProvider[] { Voyage };
			}
		}

		#endregion

		#region IStowPlanMessageAttachee Members

		[MaxLength(1)]
		public ZString StowPlanMessageStatus
		{
			get { return this.GetUserDefinedValue<ZString>(Schema.StowPlanMessageStatus); }
			set
			{
				var oldValue = this.GetUserDefinedValue<ZString>(Schema.StowPlanMessageStatus);
				this.SetUserDefinedValue(Schema.StowPlanMessageStatus, value);

				HasStowPlanMessageStatusChanged = oldValue != value;
				UpdateStowPlanMessageStatusChangedSubscription();
			}
		}

		void UpdateStowPlanMessageStatusChangedSubscription()
		{
			var property = this.GetUserDefinedProperty(Schema.StowPlanMessageStatus, null);
			property.GenCustomAddOnValueOnSaved += OnStowPlanMessageStatusChanged;
		}

		void OnStowPlanMessageStatusChanged(BusinessObjectFactory factory, bool savedSuccessfully)
		{
			if (savedSuccessfully && !IsDeleted && IsInDatabase && !STWReportingLicenseLogged && MessageStatusListSTW.IsAccepted(StowPlanMessageStatus) && HasStowPlanMessageStatusChanged)
			{
				this.LogSTWLicense();
				STWReportingLicenseLogged = true;
			}
		}

		bool STWReportingLicenseLogged;

		bool HasStowPlanMessageStatusChanged { get; set; }

		#endregion

		#region IEventDatePropertyChecker

		bool IEventDatePropertyChecker.CanUpdateProperty(IStmALog log, ZPropertyInfo property)
		{
			if (IsARVProperty(property) && suspendUpdatingARVPropertiesFromLogsCount > 0)
			{
				return false;
			}

			log.Parameters.TryGetValue(Params.Location, out var locationInEvent);

			return JB_RL_NKPortOfDischarge == locationInEvent;
		}

		#endregion

		#region SuspendUpdatingARVPropertiesFromLogs

		int suspendUpdatingARVPropertiesFromLogsCount;

		IDisposable SuspendUpdatingARVPropertiesFromLogs()
		{
			suspendUpdatingARVPropertiesFromLogsCount++;
			return new DisposableAction(() => suspendUpdatingARVPropertiesFromLogsCount--);
		}

		bool IsARVProperty(ZPropertyInfo property) => property == JB_E_ARVInfo || property == JB_A_ARVInfo;

		#endregion

		#region IVesselMovementsUrlSupporter

		(VesselMovementsUrlModel model, string errorMessage) IVesselMovementsUrlSupporter.GetVesselMovementsUrlModel()
		{
			if (Voyage == null)
			{
				return (null, Res.GetString("22e2a233-4e1d-4d05-b6b3-4d44964d4bd0", "Voyage can not be empty."));
			}

			if (Voyage.Origins.IsNullOrEmpty())
			{
				return (null, Res.GetString("74339783-2b06-4d33-8572-9c1d3139d88c", "Voyage origins can not be empty."));
			}

			var origin = Voyage.Origins.Cast<VoyageOrigin>().Where(o => o.JA_E_DEP < JB_E_ARV)?.MaxBySafe(o => o.JA_E_DEP);

			if (origin == null)
			{
				return (null, Res.GetString("4a4c8ba4-8426-4d3c-aba4-b5cd708d1085", "Usable origin could not be found."));
			}

			var model = new VesselMovementsUrlModel
			{
				LloydsNumber = Voyage.Vessel?.RV_LloydsNumber ?? ZString.Empty,
				DepartureTime = origin.JA_E_DEP,
				DeparturePortUnloco = origin.JA_RL_NKPortOfLoading,
				ArrivalTime = JB_E_ARV,
				ArrivalPortUnloco = JB_RL_NKPortOfDischarge,
				CarrierCode = Voyage.Carrier?.SCACCode ?? ZString.Empty,
				VoyageNumber = Voyage.JV_VoyageFlight,
			};

			return (model, null);
		}

		#endregion

		#region IPortMatchingSupport

		ILocationReference IPortMatchingSupport.PortOfDischarge => PortOfDischarge;

		void IPortMatchingSupport.SetFirstArrivalPort(ZString unloco, ZDateTimeOffset dateTime)
		{
			if (!JB_RL_NKFirstDischargePortInfo.ReadOnly && JB_RL_NKFirstDischargePort.IsEmpty
				&& !JB_FirstDischargePortETAInfo.ReadOnly && JB_FirstDischargePortETA.IsEmpty)
			{
				if (!unloco.IsEmpty)
				{
					JB_RL_NKFirstDischargePort = unloco;
				}
				if (dateTime.IsValid)
				{
					JB_FirstDischargePortETA = dateTime;
				}
			}
		}

		void IPortMatchingSupport.SetLastForeignPort(ZString unloco, ZDateTimeOffset dateTime)
		{
			if (!JB_RL_NKLastForeignPortInfo.ReadOnly && JB_RL_NKLastForeignPort.IsEmpty
				&& !JB_LastForeignPortETDInfo.ReadOnly && JB_LastForeignPortETD.IsEmpty)
			{
				if (!unloco.IsEmpty)
				{
					JB_RL_NKLastForeignPort = unloco;
				}
				if (dateTime.IsValid)
				{
					JB_LastForeignPortETD = dateTime;
				}
			}
		}

		void IPortMatchingSupport.ReportException(Exception exception) { }

		#endregion

		#region IPortMatchingHandler

		bool IPortMatchingHandler.IsUpdateRequired =>
			(!JB_RL_NKFirstDischargePortInfo.ReadOnly && JB_RL_NKFirstDischargePort.IsEmpty && !JB_FirstDischargePortETAInfo.ReadOnly && JB_FirstDischargePortETA.IsEmpty)
			|| (!JB_RL_NKLastForeignPortInfo.ReadOnly && JB_RL_NKLastForeignPort.IsEmpty && !JB_LastForeignPortETDInfo.ReadOnly && JB_LastForeignPortETD.IsEmpty);

		public void TryMatchFirstArrivalAndLastForeignPorts()
		{
			if (!((IPortMatchingHandler)this).IsUpdateRequired)
			{
				return;
			}

			var (vesselMovements, _) = ((IVesselMovementsUrlSupporter)this).GetVesselMovementsUrlModel();
			if (vesselMovements == null || !vesselMovements.IsValidForPortMatching())
			{
				return;
			}

			using (var portMatcher = ObjectFactory.Get<IPortMatcher>())
			{
				IPortMatchingSupport portMatchingSupporter = this;
				try
				{
					var matches = Task.Run(() => portMatcher.MatchAsync(vesselMovements, CancellationToken.None)).GetAwaiter().GetResult();
					portMatchingSupporter.SetLastForeignPort(matches?.LastForeignPort?.Unloco, matches?.LastForeignPort?.DepartureTime ?? ZDateTimeOffset.Empty);
					portMatchingSupporter.SetFirstArrivalPort(matches?.FirstArrivalPort?.Unloco, matches?.FirstArrivalPort?.ArrivalTime ?? ZDateTimeOffset.Empty);
				}
				catch (Exception ex) when (ex is AisWebApiException)
				{
					portMatchingSupporter.ReportException(ex);
					ExceptionReporter.Instance.ReportDeveloperException("PortMatchingFailed", ex.Message, ex);
				}
				catch (Exception ex) when (!ex.IsCriticalException())
				{
					portMatchingSupporter.ReportException(ex);
					ErrorReporter.ReportOnce("PortMatchingFailed", ex.Message, ex);
				}
			}
		}

		#endregion

		#region ITrackableVoyagePort

		ZPropertyInfo ITrackableVoyagePort.Unloco => JB_RL_NKPortOfDischargeInfo;
		ZPropertyInfo ITrackableVoyagePort.EstimatedDate => JB_E_ARVInfo;

		#endregion
	}
}
