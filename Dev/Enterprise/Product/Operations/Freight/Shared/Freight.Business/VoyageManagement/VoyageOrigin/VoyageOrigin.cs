using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Customs.Common.US.AMS;
using Enterprise.Environment;
using Enterprise.Freight.CarbonEmissions.Business;
using Enterprise.Freight.Common.Business;
using Enterprise.Integration;
using Enterprise.Integration.Freight;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using Constants = Enterprise.Core.Constants;
using EventConstants = CargoWise.EventReference.Constants;
using Params = CargoWise.EventReference.Constants.EventReferenceParameters.Codes;

namespace Enterprise.Freight.Business
{
	[UserDefinedValues]
	[CodeProperty(AutoJobVoyOrigin.Schema.JA_RL_NKPortOfLoading), DescriptionProperty(VoyageOrigin.Schema.Description)]
	[System.Diagnostics.DebuggerDisplay("Load = {JA_RL_NKPortOfLoading}")]
	[ActionFieldFollow(false)]
	public class VoyageOrigin : AutoJobVoyOrigin,
		IVoyageOrigin,
		ISendersMessageReferenceProvider,
		ISlotAllocationParent,
		ISailingEndPoint,
		IScheduleChangeParent,
		IWorkflowTriggerFieldChangeSource,
		IStowPlanMessageAttachee,
		IScheduleChangeEmailSupporter,
		IEventDatePropertyChecker,
		ITrackableVoyagePort
	{
		#region Schema

		public new class Schema : AutoJobVoyOrigin.Schema
		{
			public const string Description = "Description";
			public const string JA_Calc_DepartureCTOAddressOrg = "JA_Calc_DepartureCTOAddressOrg";
			public const string JA_Calc_DepartureCTOAddressCode = "JA_Calc_DepartureCTOAddressCode";
			public const string JA_Calc_DepartureCTOPremiseID = "JA_Calc_DepartureCTOPremiseID";
			public const string StowPlanMessageStatus = "StowPlanMessageStatus";
		}

		#endregion

		public VoyageOrigin(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
			JA_OA_DepartureCTOAddress_ZAddress.DefaultAddressType = AddressType.DLV;
		}

		#region Business Object Overrides

		protected override ZString HumanReadableNameCore
		{
			get
			{
				if (Voyage == null)
				{
					return Res.GetString("9197791e-3b5d-4767-9f60-ce0838c4a824", "Origin = '{0}'", JA_RL_NKPortOfLoading);
				}
				else
				{
					return Res.GetString("3f131d04-53ab-4e30-ba6f-ee26b6ad4b9c", "{0}, Origin = '{1}'", Voyage.HumanReadableName, JA_RL_NKPortOfLoading);
				}
			}
		}

		protected override void OnUpdatedByDataRefresh()
		{
			base.OnUpdatedByDataRefresh();
			OnOriginUpdatedByDataRefresh();
		}

		internal event EventHandler OriginUpdatedByDataRefresh;

		void OnOriginUpdatedByDataRefresh()
		{
			if (OriginUpdatedByDataRefresh != null)
			{
				OriginUpdatedByDataRefresh(this, EventArgs.Empty);
			}
		}

		protected override bool SupportsCloneCore()
		{
			return true;
		}

		protected override BusinessObject CloneInternal(BusinessObjectCloneArgs args)
		{
			VoyageOrigin origin = (VoyageOrigin)base.CloneInternal(args);

			foreach (SlotAllocation allocation in SlotAllocations)
			{
				origin.SlotAllocations.Add(allocation.Clone());
			}

			return origin;
		}

		protected override IEnumerable<string> GetPropertiesToExcludeFromCloning()
		{
			List<string> result = new List<string>(base.GetPropertiesToExcludeFromCloning());

			result.Add(JobVoyOriginSchema.Constants.JA_JV);

			return result;
		}

		public override void Delete()
		{
			base.Delete();
			SlotAllocations.RemoveAndDeleteAll();
			Messages.RemoveAndDeleteAll();
		}

		public override void OnSaving()
		{
			base.OnSaving();
			((ISendersMessageReferenceProvider)this).PopulateSendersReferenceIfNeeded();
			new JobScheduleChangeLogger().LogOriginDateChanges(this);

			if (Voyage != null)
			{
				UpdateVoyageEvents();
				ScheduleFeedServiceManager.Update(this, Voyage, Factory);
				UpdateFlightSubscriptionEvent();
				UpdateRelatedBookedAgencyBookingEvent();
			}

			OnOriginSaving();
		}

		protected virtual void OnOriginSaving()
		{
			OriginSaving?.Invoke(this, EventArgs.Empty);
		}

		public event EventHandler OriginSaving;

		void CancelOldEvent(Event evnt, bool? isEstimate = null)
		{
			var parameters = new Dictionary<string, string>();
			parameters[Params.Facility] = EventConstants.Facilities.Code.Terminal;
			parameters[Params.Location] = (ZString)JA_RL_NKPortOfLoadingInfo.OriginalValue;

			var log = Voyage.Logs.MostRecentLogByEventTime(evnt, MatchingParameters(parameters));
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

		void UpdateVoyageEvents()
		{
			if (JA_RL_NKPortOfLoadingInfo.HasChanges)
			{
				CancelOldEvent(AutoEvents.Departure, false);
				CancelOldEvent(AutoEvents.Departure, true);
				CancelOldEvent(AutoEvents.CutOffDate);
				CancelOldEvent(AutoEvents.ReceiptCommenced);
			}

			if (!IsInDatabase && JA_A_DEP.IsValid || JA_A_DEPInfo.HasChanges || JA_RL_NKPortOfLoadingInfo.HasChanges)
			{
				Voyage.Logs.CreateRecreateOrUpdateEventLog(Events.Departure, EstimateActual.Actual, JA_A_DEP.ToOffset(), ZString.Empty, GetParametersForEvent(Events.Departure).ToArray());
			}

			if (!IsInDatabase && JA_E_DEP.IsValid || JA_E_DEPInfo.HasChanges || JA_RL_NKPortOfLoadingInfo.HasChanges)
			{
				using (SuspendUpdatingDEPPropertiesFromLogs())
				{
					Voyage.Logs.CreateRecreateOrUpdateEventLog(Events.Departure, EstimateActual.Estimate, JA_E_DEP.ToOffset(), ZString.Empty, GetParametersForEvent(Events.Departure).ToArray());
				}
			}

			if (!IsInDatabase && JA_CutOff.IsValid || JA_CutOffInfo.HasChanges || JA_RL_NKPortOfLoadingInfo.HasChanges)
			{
				Voyage.Logs.CreateRecreateOrUpdateEventLog(Events.CutOffDate, EstimateActual.Actual, JA_CutOff.ToOffset(), ZString.Empty, GetParametersForEvent(Events.CutOffDate).ToArray());
			}

			if (!IsInDatabase && JA_ReceivalCommences.IsValid || JA_ReceivalCommencesInfo.HasChanges || JA_RL_NKPortOfLoadingInfo.HasChanges)
			{
				Voyage.Logs.CreateRecreateOrUpdateEventLog(Events.ReceiptCommenced, EstimateActual.Actual, JA_ReceivalCommences.ToOffset(), ZString.Empty, GetParametersForEvent(Events.ReceiptCommenced).ToArray());
			}
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
			var updateAllRelatedAgencyBookings = JA_RL_NKPortOfLoadingInfo.HasChanges || JA_E_DEPInfo.HasChanges;
			if (updateAllRelatedAgencyBookings || JA_ReceivalCommencesInfo.HasChanges || JA_CutOffInfo.HasChanges || JA_DocumentaryCutoffInfo.HasChanges || JA_VGMCutOffInfo.HasChanges)
			{
				RelatedAgencyBookingsEventLogHelper.UpdateRelatedBookedAgencyBookingEvent(FetchSailings(), !updateAllRelatedAgencyBookings);
			}
		}

		static Func<StmALog, bool> MatchingParameters(IEnumerable<KeyValuePair<string, string>> parameters)
		{
			return (StmALog log) =>
			{
				var allMatch = true;

				foreach (var p in parameters)
				{
					string val;

					if (!log.Parameters.TryGetValue(p.Key, out val) || val != p.Value)
					{
						allMatch = false;
						break;
					}
				}

				return allMatch;
			};
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
				JA_SendersMessageReference = ZString.Empty;
			}
		}

		protected override EnterpriseBusinessObjectFetchStrategy GetFetchStrategyCore()
		{
			return new VoyageDependentBizObjFetchStrategy(this, JA_JV);
		}

		#endregion

		#region Related Business Objects

		#region Voyage

		IJobVoyage IVoyageOrigin.Voyage => Voyage;

		public JobVoyage Voyage
		{
			get { return Factory.Load<JobVoyage>(JA_JV); }
		}

		#endregion

		#region Country

		public RefCountry Country
		{
			get { return (PortOfLoading != null) ? PortOfLoading.Country : null; }
		}

		#endregion

		#region VoyageCountry

		public VoyageCountry VoyageCountry
		{
			get { return Voyage.Countries.GetCountry(JA_RL_NKPortOfLoading.Left(2), true); }
		}

		#endregion

		#region SlotAllocations

		[ChildEditable(true)]
		public SlotAllocationDependentCollection SlotAllocations
		{
			get
			{
				if (fSlotAllocations == null)
				{
					fSlotAllocations = new SlotAllocationDependentCollection(this);
					fSlotAllocations.Load();
					RegisterEditableChildObject(fSlotAllocations);
				}

				return fSlotAllocations;
			}
		}
		SlotAllocationDependentCollection fSlotAllocations;

		#endregion

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
			return Factory.Load<JobSailing>(new ZQuery(JobSailingSchema.JX_JA, PK));
		}

		#endregion

		#endregion

		#region Properties

		#region JA_AutoCreated

		public override ZBool JA_AutoCreated
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return base.JA_AutoCreated; }
			set
			{
				base.JA_AutoCreated = value;
				MarkVoyageAsNeedingValidiation();
			}
		}

		#endregion

		#region JA_JV

		[RelatedBusinessObject("Voyage")]
		public override ZGuid JA_JV
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return base.JA_JV; }
			set
			{
				JobVoyage oldVoyage = Voyage;

				base.JA_JV = value;
				MarkVoyageAsNeedingValidiation();

				if (oldVoyage != null)
				{
					oldVoyage.MarkAsNeedingValidation();
				}
			}
		}

		#endregion

		#region JA_RL_NKPortOfLoading

		[List("Lookups.PortOfLoadings")]
		public override ZString JA_RL_NKPortOfLoading
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return base.JA_RL_NKPortOfLoading; }
			set
			{
				if (value != JA_RL_NKPortOfLoading)
				{
					base.JA_RL_NKPortOfLoading = value;
					if (Voyage != null)
					{
						NotifySailingLoadPorts();
						SailingScheduleDataVendor.Instance.UpdateVoyageOrigin(this);
						RunOnlineFlightMatchingForAllSailings();
						Voyage.RequiresSailingGeneration = true;
					}

					if (!IsValidationSuspended)
					{
						Validation.ValidateJA_A_ARV();
						Validation.ValidateJA_A_DEP();
					}
					DefaultCTOFromCarrier();
					MarkVoyageAsNeedingValidiation();
					MarkDestinationsAsNeedingValidation();
				}
			}
		}

		#endregion

		#region JA_A_DEP

		[EventDateProperty(Events.DepartureCode, EstimateActual.Actual)]
		public override ZDateTime JA_A_DEP
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return new ZDateTime(base.JA_A_DEP, DateTimeKind.Unspecified); }
			set
			{
				ZDateTime oldATD = JA_A_DEP;
				if (oldATD != value)
				{
					base.JA_A_DEP = value;

					if (Voyage != null)
					{
						IScheduleUpdateSubscriber[] subscribers = ScheduleUpdateSubscriberFactory.GetSubscribers(Factory);
						subscribers.ATDChanged(new ScheduleUpdateServices(Voyage), this, oldATD);
					}
				}
			}
		}

		#endregion

		#region JA_A_DEP_UTC

		public ZDateTime JA_A_DEP_UTC
		{
			get { return JA_A_DEP.IsValid ? Env.Time.GetUtcFromUnlocoTime(JA_RL_NKPortOfLoading.ToString(), JA_A_DEP.ToDateTime()) : ZDateTime.Empty; }
		}

		#endregion

		#region JA_E_DEP

		[EventDateProperty(Events.DepartureCode, EstimateActual.Estimate)]
		public override ZDateTime JA_E_DEP
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return new ZDateTime(base.JA_E_DEP, DateTimeKind.Unspecified); }
			set
			{
				ZDateTime oldETD = base.JA_E_DEP;
				if (oldETD != value)
				{
					base.JA_E_DEP = value;

					if (Voyage != null)
					{
						IScheduleUpdateSubscriber[] subscribers = ScheduleUpdateSubscriberFactory.GetSubscribers(Factory);
						subscribers.ETDChanged(new ScheduleUpdateServices(Voyage), this, oldETD);
						RunOnlineFlightMatchingForAllSailings();

						Voyage.RequiresSailingGeneration = true;
					}

					if (JA_E_DEP.IsValid && !JA_S_DEP.IsValid)
					{
						JA_S_DEP = JA_E_DEP;
					}

					MarkVoyageAsNeedingValidiation();
					MarkDestinationsAsNeedingValidation();
				}
			}
		}

		#endregion

		#region JA_E_DEP_UTC

		public ZDateTime JA_E_DEP_UTC
		{
			get { return JA_E_DEP.IsValid ? Env.Time.GetUtcFromUnlocoTime(JA_RL_NKPortOfLoading.ToString(), JA_E_DEP.ToDateTime()) : ZDateTime.Empty; }
		}

		#endregion

		#region JA_S_DEP

		public override ZDateTime JA_S_DEP
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return new ZDateTime(base.JA_S_DEP, DateTimeKind.Unspecified); }
			set
			{
				if (base.JA_S_DEP != value)
				{
					base.JA_S_DEP = value;
					if (JA_S_DEP.IsValid && !JA_E_DEP.IsValid)
					{
						JA_E_DEP = JA_S_DEP;
					}
				}
			}
		}

		#endregion

		#region JA_S_DEP_UTC

		public ZDateTime JA_S_DEP_UTC
		{
			get { return JA_S_DEP.IsValid ? Env.Time.GetUtcFromUnlocoTime(JA_RL_NKPortOfLoading.ToString(), JA_S_DEP.ToDateTime()) : ZDateTime.Empty; }
		}

		#endregion

		#region JA_OA_DepartureCTOAddress

		public override ZGuid JA_OA_DepartureCTOAddress
		{
			get { return base.JA_OA_DepartureCTOAddress; }
			set
			{
				if (value != JA_OA_DepartureCTOAddress)
				{
					base.JA_OA_DepartureCTOAddress = value;
					if (JA_Berth.IsEmpty && JA_RL_NKPortOfLoading == "DEHAM" && DepartureCTOAddress?.Header != null)
					{
						var customRegNo = DepartureCTOAddress.Header.CustomsCodes.GetCustomsRegNo(GermanyOrgCusCodeInfo.OrgCusCodes.DakosyBerthCode, Constants.CountryCodes.Germany, JA_OA_DepartureCTOAddress);
						JA_Berth = customRegNo.SubstringSafe(0, JA_BerthInfo.MaxLength);
					}
					JA_Calc_DepartureCTOAddressOrgInfo.RefreshBinding();
					JA_Calc_DepartureCTOAddressCodeInfo.RefreshBinding();
				}
			}
		}

		#endregion

		#region JA_DGReceivalCommences

		public override ZDateTime JA_DGReceivalCommences
		{
			get { return new ZDateTime(base.JA_DGReceivalCommences, DateTimeKind.Unspecified); }
			set
			{
				base.JA_DGReceivalCommences = value;

				if (!IsValidationSuspended)
				{
					Validation.ValidateJA_DGCutOff();
				}
			}
		}

		#endregion

		#region JA_DGCutOff

		public override ZDateTime JA_DGCutOff
		{
			get { return new ZDateTime(base.JA_DGCutOff, DateTimeKind.Unspecified); }
			set
			{
				base.JA_DGCutOff = value;

				if (!IsValidationSuspended)
				{
					Validation.ValidateJA_DGReceivalCommences();
				}
			}
		}

		#endregion

		#region JA_A_ARV

		public override ZDateTime JA_A_ARV
		{
			get { return new ZDateTime(base.JA_A_ARV, DateTimeKind.Unspecified); }
			set { base.JA_A_ARV = value; }
		}

		#endregion

		#region JA_E_ARV

		public override ZDateTime JA_E_ARV
		{
			get { return new ZDateTime(base.JA_E_ARV, DateTimeKind.Unspecified); }
			set
			{
				base.JA_E_ARV = value;

				if (JA_E_ARV.IsValid && !JA_S_ARV.IsValid)
				{
					JA_S_ARV = value;
				}
			}
		}

		#endregion

		#region JA_S_ARV

		public override ZDateTime JA_S_ARV
		{
			get { return new ZDateTime(base.JA_S_ARV, DateTimeKind.Unspecified); }
			set
			{
				if (base.JA_S_ARV != value)
				{
					base.JA_S_ARV = value;
					if (JA_S_ARV.IsValid && !JA_E_ARV.IsValid)
					{
						JA_E_ARV = JA_S_ARV;
					}
				}
			}
		}

		#endregion

		#region JA_S_ARV_UTC

		public ZDateTime JA_S_ARV_UTC
		{
			get { return JA_S_ARV.IsValid ? Env.Time.GetUtcFromUnlocoTime(JA_RL_NKPortOfLoading.ToString(), JA_S_ARV.ToDateTime()) : ZDateTime.Empty; }
		}

		#endregion

		#region JA_CutOff

		[EventDateProperty(Events.CutOffDateCode, EstimateActual.Actual)]
		public override ZDateTime JA_CutOff
		{
			get { return new ZDateTime(base.JA_CutOff, DateTimeKind.Unspecified); }
			set { base.JA_CutOff = value; }
		}

		#endregion

		#region JA_ReceivalCommences

		[EventDateProperty(Events.ReceiptCommencedCode, EstimateActual.Actual)]
		public override ZDateTime JA_ReceivalCommences
		{
			get { return new ZDateTime(base.JA_ReceivalCommences, DateTimeKind.Unspecified); }
			set { base.JA_ReceivalCommences = value; }
		}

		#endregion

		#region JA_DocumentaryCutoff

		public override ZDateTime JA_DocumentaryCutoff
		{
			get { return new ZDateTime(base.JA_DocumentaryCutoff, DateTimeKind.Unspecified); }
			set { base.JA_DocumentaryCutoff = value; }
		}

		#endregion

		#region Description

		public ZString Description
		{
			get { return JA_E_DEP.ToString(); }
		}

		#endregion

		#region JA_Calc_DepartureCTOAddressOrg

		[List("Lookups.CTO_List")]
		public ZGuid JA_Calc_DepartureCTOAddressOrg
		{
			get { return JA_OA_DepartureCTOAddress_ZAddress.OrgPK; }
			set
			{
				JA_OA_DepartureCTOAddress_ZAddress.OrgPK = value;
				JA_Calc_DepartureCTOAddressOrgInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo JA_Calc_DepartureCTOAddressOrgInfo
		{
			get { return GetZPropertyInfo(Schema.JA_Calc_DepartureCTOAddressOrg); }
		}

		#endregion

		#region JA_Calc_DepartureCTOAddressCode

		[BusinessObjectTestExclude]
		[List("Lookups.JA_DepartureCTOAddress_List")]
		[MaxLength(OrgAddress.Schema.OA_CodeMaxLength)]
		public ZString JA_Calc_DepartureCTOAddressCode
		{
			get
			{
				var address = Factory.Load<OrgAddress>(JA_OA_DepartureCTOAddress);
				return (address != null) ? address.OA_Code : ZString.Empty;
			}
			set
			{
				ZQuery filter = new ZQuery(OrgAddressSchema.OA_OH, JA_Calc_DepartureCTOAddressOrg);
				filter.AddToFilter(OrgAddressSchema.OA_Code, value);
				var address = Factory.LoadTop1<OrgAddress>(filter);
				JA_OA_DepartureCTOAddress = (address != null) ? address.PK : ZGuid.Empty;
				JA_Calc_DepartureCTOAddressCodeInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo JA_Calc_DepartureCTOAddressCodeInfo
		{
			get { return GetZPropertyInfo(Schema.JA_Calc_DepartureCTOAddressCode); }
		}

		protected bool JA_Calc_DepartureCTOAddressCode_ReadOnly
		{
			get { return !JA_Calc_DepartureCTOAddressOrg.IsValid; }
		}

		#endregion

		#region JA_Calc_DepartureCTOPremiseID

		[BusinessObjectTestExclude]
		[MaxLength(8)]
		public ZString JA_Calc_DepartureCTOPremiseID
		{
			get
			{
				var address = Factory.Load<OrgAddress>(JA_OA_DepartureCTOAddress);
				return (address != null) ? address.LocalControlledPremisesID : ZString.Empty;
			}
		}

		public ZPropertyInfo JA_Calc_DepartureCTOPremiseIDInfo
		{
			get { return GetZPropertyInfo(Schema.JA_Calc_DepartureCTOPremiseID); }
		}

		#endregion

		#region JA_DepartReference

		public override ZString JA_DepartReference
		{
			get { return base.JA_DepartReference; }
			set
			{
				base.JA_DepartReference = value;
				FetchSailings().ForEach(sailing => sailing.JX_DeparturePortRouteId = value);
			}
		}

		#endregion

		#region JA_DepartReferenceFieldType

		public ZString JA_DepartReferenceFieldType
		{
			get
			{
				return JA_RL_NKPortOfLoading.StartsWith(Constants.CountryCodes.SouthAfrica, StringComparison.OrdinalIgnoreCase)
						&& GlbCompany.CurrentCompany.GC_RN_NKCountryCode == Constants.CountryCodes.SouthAfrica
						&& Voyage != null
						&& Voyage.IsSea
					? nameof(FieldType.TextCodeFindBox)
					: nameof(FieldType.Text);
			}
		}

		#endregion

		#endregion

		#region Validation

		protected override JobVoyOriginValidation GetNewValidation()
		{
			var result = GetNewValidationByVoyageType();
			if (AdditionalValidation != null)
			{
				result.Add(AdditionalValidation);
			}

			return result;
		}

		JobVoyOriginValidation GetNewValidationByVoyageType()
		{
			if (Voyage != null)
			{
				if (Voyage.IsSea)
				{
					return new VoyageOriginSeaValidation(this);
				}

				if (Voyage.IsAir)
				{
					return new VoyageOriginAirValidation(this);
				}

				if (Voyage.IsRail || Voyage.IsRoad)
				{
					return new VoyageOriginLandValidation(this);
				}
			}
			return new BaseJobVoyOriginValidation(this);
		}

		public JobVoyOriginValidation AdditionalValidation { get; set; }

		#endregion

		#region Lists

		protected BindToLists BindingLists
		{
			get { return BindToLists.GetCachedLists(Factory); }
		}

		#endregion

		#region Implementation

		void NotifySailingLoadPorts()
		{
			foreach (JobSailing sailing in Voyage.Sailings)
			{
				if (sailing.JX_JA == PK)
				{
					sailing.JX_JA_RL_NKPortOfLoadingInfo.RefreshBinding();
					sailing.UpdateCO2eStatusToNotCurrent(new CO2eStatusChangedReason(JA_RL_NKPortOfLoadingInfo));
				}
			}
		}

		void DefaultCTOFromCarrier()
		{
			CTOFromCarrierDefaulter.SetCTOFromCarrier(Voyage, JA_RL_NKPortOfLoading, OrgConstants.CarrierAgentDirections.Code.Departure, JA_OA_DepartureCTOAddressInfo, JA_Calc_DepartureCTOAddressOrgInfo);
		}

		void MarkVoyageAsNeedingValidiation()
		{
			JobVoyage voyage = this.Voyage;
			if (voyage != null)
			{
				voyage.MarkAsNeedingValidation();
			}
		}

		void MarkDestinationsAsNeedingValidation()
		{
			JobVoyage voyage = this.Voyage;
			if (voyage != null)
			{
				voyage.Destinations.MarkAsNeedingValidation();
			}
		}

		void RunOnlineFlightMatchingForAllSailings()
		{
			using (Factory.GetValue<IBusyIndicatorProvider>()?.NewBusyIndicator())
			{
				FetchSailings()?.ForEach(sailing => sailing.TryMatchAgainstOnlineFlights());
			}
		}

		#region Parameters

		Dictionary<string, string> GetParametersForEvent(Event eventType)
		{
			var parameters = new Dictionary<string, string>();

			if (eventType.Code == Events.DepartureCode)
			{
				parameters[Params.Facility] = EventConstants.Facilities.Code.Terminal;
				parameters[Params.Location] = JA_RL_NKPortOfLoading;
				parameters[Params.Mode] = Voyage != null ? Voyage.JV_AirSeaRoad : ZString.Empty;
				if (Voyage.IsAir)
				{
					parameters[Params.VoyageFlightNumber] = Voyage != null ? Voyage.JV_VoyageFlight : ZString.Empty;
					parameters[Params.FlightDate] = Voyage != null && Voyage.JV_FlightDate.IsValid
												? Voyage.JV_FlightDate.ToISO8601ShortDateString()
												: string.Empty;
				}
			}
			else if (eventType.Code == Events.CutOffDateCode
				|| eventType.Code == Events.ReceiptCommencedCode)
			{
				parameters[Params.Facility] = EventConstants.Facilities.Code.Terminal;
				parameters[Params.Location] = JA_RL_NKPortOfLoading;
			}

			return parameters;
		}

		#endregion

		#endregion

		#region ISendersMessageReferenceProvider Members

		void ISendersMessageReferenceProvider.PopulateSendersReferenceIfNeeded()
		{
			if (JA_SendersMessageReference.IsEmpty
				&& TryGenerateMessageReferenceNumber(out var generatedNumbers)
				&& generatedNumbers.Length > 0)
			{
				JA_SendersMessageReference = generatedNumbers[0];
			}
		}

		bool TryGenerateMessageReferenceNumber(out string[] generatedNumbers)
		{
			if (Factory is IDbConnected connected)
			{
				var fountain = Env.NumberFountains.VoyageOriginNumber(Schema.JA_SendersMessageReferenceMaxLength);
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
				return JA_SendersMessageReference;
			}
		}

		#endregion

		#region IAllocationParent Members

		ZString ISlotAllocationParent.Code
		{
			get { return JobVoyOriginSchema.Constants.Prefix; }
		}

		#endregion

		#region ISailingEndPoint Members

		ZString ISailingEndPoint.Port
		{
			get { return JA_RL_NKPortOfLoading; }
		}

		ZDateTime ISailingEndPoint.EstimatedDate
		{
			get { return JA_E_DEP; }
		}

		ZString ISailingEndPoint.Direction
		{
			get { return Constants.PortDirection.Load; }
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

		public new BaseJobVoyOriginLookups Lookups
		{
			get { return lookups ?? (lookups = (BaseJobVoyOriginLookups)GetNewLookups()); }
		}
		BaseJobVoyOriginLookups lookups;

		protected override JobVoyOriginLookups GetNewLookups()
		{
			return new BaseJobVoyOriginLookups(this);
		}

		#endregion

		#region IScheduleChangeParent Members

		SchemaGuidColumn IScheduleChangeParent.SailingRefColumn
		{
			get { return JobSailingSchema.JX_JA; }
		}

		ZString IScheduleChangeParent.OriginPort
		{
			get { return JA_RL_NKPortOfLoading; }
		}

		ZString IScheduleChangeParent.DestinationPort
		{
			get { return ""; }
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
			if (IsDEPProperty(property) && suspendUpdatingDEPPropertiesFromLogsCount > 0)
			{
				return false;
			}

			log.Parameters.TryGetValue(Params.Location, out var locationInEvent);

			return JA_RL_NKPortOfLoading == locationInEvent;
		}

		#endregion

		#region SuspendUpdatingDEPPropertiesFromLogs

		int suspendUpdatingDEPPropertiesFromLogsCount;

		IDisposable SuspendUpdatingDEPPropertiesFromLogs()
		{
			suspendUpdatingDEPPropertiesFromLogsCount++;
			return new DisposableAction(() => suspendUpdatingDEPPropertiesFromLogsCount--);
		}

		bool IsDEPProperty(ZPropertyInfo property) => property == JA_E_DEPInfo || property == JA_A_DEPInfo;

		#endregion

		#region ITrackableVoyagePort

		ZPropertyInfo ITrackableVoyagePort.Unloco => JA_RL_NKPortOfLoadingInfo;
		ZPropertyInfo ITrackableVoyagePort.EstimatedDate => JA_E_DEPInfo;

		#endregion
	}
}
