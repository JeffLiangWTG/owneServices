using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Freight.Common.Business;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Business
{
	public class BaseSailingManager
	{
		public static BaseSailingManager New(ISailingManaged parent)
		{
			if (parent == null)
			{
				throw new ArgumentNullException(nameof(parent));
			}

			BaseSailingManager result;

			switch (parent.TransportMode)
			{
				case Constants.TransportModes.Sea:
					result = new SeaSailingManager(parent);
					result.VoyageDirty = true;
					result.VesselDirty = true;
					result.LoadDirty = true;
					result.DischargeDirty = true;
					result.CarrierDirty = true;
					break;

				case Constants.TransportModes.Rail:
					result = new RailScheduleManager(parent);
					result.VoyageDirty = true;
					result.VesselDirty = true;
					result.LoadDirty = true;
					result.DischargeDirty = true;
					break;

				case Constants.TransportModes.Air:
				case Constants.TransportModes.Road:
					result = new ScheduleManager(parent);
					result.VoyageDirty = true;
					result.LoadDirty = true;
					result.DischargeDirty = true;
					break;

				default:
					result = new BaseSailingManager(parent);
					break;
			}

			return result;
		}

		protected BaseSailingManager(ISailingManaged parent)
		{
			this.parent = parent;
		}

		#region Properties

		public bool IsInitialising => running;
		public bool LoadDirty { get; set; }
		public bool DischargeDirty { get; set; }
		public bool VesselDirty { get; set; }
		public bool VoyageDirty { get; set; }
		public bool RegistrationDirty { get; set; }
		public bool DepartureDirty { get; set; }
		public bool ArrivalDirty { get; set; }
		public bool TransportModeDirty { get; set; }
		public bool IsCharterDirty { get; set; }
		public bool IsCargoOnlyDirty { get; set; }
		public bool CarrierDirty { get; set; }
		public bool IsAircraftTypeDirty { get; set; }
		public bool IsOnlineScheduleStatusDirty { get; set; }

		public bool Dirty
		{
			get
			{
				return LoadDirty
					|| DischargeDirty
					|| VesselDirty
					|| VoyageDirty
					|| RegistrationDirty
					|| DepartureDirty
					|| ArrivalDirty
					|| TransportModeDirty
					|| IsCharterDirty
					|| IsCargoOnlyDirty
					|| CarrierDirty
					|| IsAircraftTypeDirty
					|| IsOnlineScheduleStatusDirty;
			}
			set
			{
				LoadDirty = value;
				DischargeDirty = value;
				VesselDirty = value;
				VoyageDirty = value;
				RegistrationDirty = value;
				DepartureDirty = value;
				ArrivalDirty = value;
				TransportModeDirty = value;
				IsCharterDirty = value;
				IsCargoOnlyDirty = value;
				CarrierDirty = value;
				IsAircraftTypeDirty = value;
				IsOnlineScheduleStatusDirty = value;
			}
		}

		public void UnhookWithoutReleasingSailing()
		{
			if (sailing != null)
			{
				UnHookSailing(sailing);
				VoyDestination = null;
				VoyOrigin = null;
				Voyage = null;
			}
		}

		public JobSailing Sailing
		{
			[DebuggerStepThrough]
			get { return sailing != null && !sailing.IsDeleted ? sailing : null; }
			set
			{
				if (sailing != value)
				{
					if (sailing != null)
					{
						UnHookSailing(sailing);
						ReleaseSailing(sailing);
					}
					performedInitialRead = true;
					sailing = value;
					ReasonForSailingNotGenerated = ZString.Empty;
					if (sailing != null)
					{
						HookSailing(sailing);

						VoyDestination = sailing.Destination;
						VoyOrigin = sailing.Origin;
						Voyage = sailing.Voyage;
					}
					else
					{
						VoyDestination = null;
						VoyOrigin = null;
						Voyage = null;
					}
				}
			}
		}

		public void LogReasonForSailingNotGenerated(ISimpleLogger logger)
		{
			if (!ReasonForSailingNotGenerated.IsEmpty)
			{
				logger.Log(LogType.Warning, Res.GetString("1b58f1fa-e625-4f0b-81bb-cbc33cc5bd4e", "Sailing is not generated. {0}", ReasonForSailingNotGenerated));
			}
		}

		public ZString ReasonForSailingNotGenerated { get; set; }

		public JobVoyage Voyage
		{
			[DebuggerStepThrough]
			get { return voyage; }
			protected set
			{
				if (voyage != value)
				{
					JobVoyage oldVoyage = voyage;
					if (voyage != null)
					{
						UnHookVoyage(voyage);
						ReleaseVoyage(voyage);
					}
					voyage = value;

					if (voyage != null)
					{
						HookVoyage(voyage);
					}
				}
			}
		}

		public VoyageOrigin VoyOrigin
		{
			[DebuggerStepThrough]
			get { return voyOrigin; }
			private set
			{
				if (voyOrigin != value)
				{
					if (voyOrigin != null)
					{
						UnHookOrigin(voyOrigin);
						ReleaseOrigin(voyOrigin);
					}

					voyOrigin = value;

					if (voyOrigin != null)
					{
						HookOrigin(voyOrigin);
					}
				}
			}
		}

		public VoyageDestination VoyDestination
		{
			[DebuggerStepThrough]
			get { return voyDestination; }
			private set
			{
				if (voyDestination != value)
				{
					if (voyDestination != null)
					{
						UnHookDestination(voyDestination);
						ReleaseDestination(voyDestination);
					}

					voyDestination = value;

					if (voyDestination != null)
					{
						HookDestination(voyDestination);
					}
				}
			}
		}

		internal Dictionary<string, IZType> TransportPropertiesChangedByDataRefresh { get; } = new Dictionary<string, IZType>();

		public DisposableAction PreserveParentScheduleDates()
		{
			preserveParentScheduleDates = true;
			return new DisposableAction(() => preserveParentScheduleDates = false);
		}

		bool preserveParentScheduleDates;

		#endregion

		#region NotifyRead

		public void NotifyRead()
		{
			if (!performedInitialRead && parent.SailingPK != ZGuid.Empty && !running)
			{
				running = true;
				try
				{
					performedInitialRead = true;
					Sailing = parent.Factory.Load<JobSailing>(parent.SailingPK);
					if (sailing != null && sailing.Origin != null && sailing.Destination != null && sailing.Voyage != null)
					{
						Dirty = false;
						parent.SuspendValidation();
						try
						{
							SetAllScheduleFields();
						}
						finally
						{
							parent.ResumeValidation();
						}
					}
				}
				finally
				{
					running = false;
				}
			}

			if (!running && Dirty)
			{
				running = true;
				try
				{
					OnNotifyRead();
				}
				finally
				{
					running = false;
				}
			}
		}

		#endregion

		#region NotifySave

		public void NotifySave()
		{
			using (new DisposableAction(() => performedInitialRead = false, () => performedInitialRead = true))
			{
				NotifyRead();
			}

			EnsureSailingExists();

			if (Voyage != null && !Voyage.IsInDatabase && !Voyage.IsDeleted)
			{
				EnsureUniqueVoyage();
			}

			bool nonUniqueOriginFound = false;
			bool nonUniqueDestinationFound = false;

			if (Sailing != null)
			{
				if (VoyOrigin != null && !VoyOrigin.IsInDatabase && !VoyOrigin.IsDeleted)
				{
					nonUniqueOriginFound = EnsureUniqueVoyOrigin();
				}

				if (VoyDestination != null && !VoyDestination.IsInDatabase && !VoyDestination.IsDeleted)
				{
					nonUniqueDestinationFound = EnsureUniqueVoyDestination();
				}

				if (!Sailing.IsInDatabase && !Sailing.IsDeleted)
				{
					EnsureUniqueSailing();
				}
			}

			if ((nonUniqueOriginFound || nonUniqueDestinationFound) && Voyage != null)
			{
				EnsureVoyageDoesNotHaveDuplicatedSailings();
			}
		}

		void EnsureVoyageDoesNotHaveDuplicatedSailings()
		{
			var newSailings = Voyage.Sailings.Cast<JobSailing>().Where(s => !s.IsInDatabase).ToArray();
			foreach (JobSailing newSailing in newSailings)
			{
				ZDBOnlyQuery sailingFilter = new ZDBOnlyQuery(typeof(JobSailing));
				sailingFilter.AddToFilter(JobSailingSchema.JX_JA, newSailing.JX_JA);
				sailingFilter.AddToFilter(JobSailingSchema.JX_JB, newSailing.JX_JB);
				sailingFilter.AddToFilter(JobSailingSchema.PK, SQLComparisonOperator.NotEqual, newSailing.PK);

				JobSailing existingSailing = parent.Factory.LoadTop1<JobSailing>(sailingFilter);
				if (existingSailing != null)
				{
					newSailing.Delete();
				}
			}
		}

		#endregion

		#region FindExistingSailing

		public JobSailing FindExistingSailing(bool ignoreActiveFilter = false)
		{
			JobSailing result = null;
			Voyage = GetExistingVoyage(ignoreActiveFilter);

			if (Voyage != null)
			{
				VoyOrigin = GetExistingOrigin();
				VoyDestination = GetExistingDestination();

				if (VoyOrigin != null && VoyDestination != null)
				{
					ZQuery sailingFilter = new ZQuery();
					sailingFilter.AddToFilter(new ZQuery(JobSailingSchema.JX_JA, VoyOrigin.PK));
					sailingFilter.AddToFilter(JobSailingSchema.JX_JB, VoyDestination.PK);
					result = parent.Factory.LoadTop1<JobSailing>(sailingFilter);
				}
			}

			Sailing = result;
			return result;
		}

		#endregion

		public bool HasSufficientInformation
		{
			get { return HasSufficientInformationCore; }
		}

		#region Implementation

		protected virtual bool HasSufficientInformationCore
		{
			get { return false; }
		}

		protected ZQuery TransportModeFilter
		{
			get
			{
				ZQuery filter = new ZQuery();
				filter.AddToFilter(JobVoyageSchema.JV_AirSeaRoad, parent.TransportMode);
				return filter;
			}
		}

		protected virtual void UpdateISailingValues()
		{
			parent.SailingPK = Sailing.PK;
		}

		protected virtual JobVoyage GetExistingVoyage(bool ignoreActiveFilter = false)
		{
			return null;
		}

		protected virtual JobVoyage NewVoyage()
		{
			return null;
		}

		protected virtual bool ShouldUpdateETD
		{
			get { return false; }
		}

		protected virtual VoyageOrigin CreateNewVoyageOrigin()
		{
			VoyageOrigin result;
			using (Voyage.Origins.SuppressSailingGeneration())
			{
				result = Voyage.Origins.AddNew();
			}

			result.JA_RL_NKPortOfLoading = parent.Load;
			result.JA_E_DEP = parent.ETD;
			result.JA_A_DEP = parent.ATD;
			result.JA_AutoCreated = true;
			return result;
		}

		protected virtual bool ShouldUpdateETA
		{
			get { return false; }
		}

		protected virtual VoyageDestination CreateNewVoyageDestination()
		{
			VoyageDestination result;
			using (Voyage.Destinations.SuppressSailingGeneration())
			{
				result = Voyage.Destinations.AddNew();
			}

			result.JB_RL_NKPortOfDischarge = parent.Discharge;
			result.JB_E_ARV = parent.ETA;
			result.JB_A_ARV = parent.ATA;
			result.JB_AutoCreated = true;
			return result;
		}

		protected virtual bool ShouldCheckUniqueVoyage
		{
			get { return true; }
		}

		public void ResetSailing()
		{
			var oldVoyage = Voyage;
			var oldOrigin = VoyOrigin;
			var oldDestination = VoyDestination;
			var oldSailing = Sailing;

			Sailing = null;
			VoyOrigin = null;
			VoyDestination = null;
			Voyage = null;

			ReleaseOld(oldVoyage, oldOrigin, oldDestination, oldSailing);
		}

		void RecreateSailing()
		{
			var oldVoyage = Voyage;
			var oldOrigin = VoyOrigin;
			var oldDestination = VoyDestination;
			var oldSailing = Sailing;

			if (parent.AllowScheduleCreation)
			{
				Voyage = GetOrCreateVoyage();
				VoyOrigin = GetOrCreateOrigin();
				VoyDestination = GetOrCreateDestination();
			}
			else if ((Voyage = GetExistingVoyage()) != null)
			{
				VoyOrigin = GetExistingOrigin();
				VoyDestination = GetExistingDestination();
			}
			else
			{
				VoyOrigin = null;
				VoyDestination = null;
			}

			var sailingWithReasonForNotGenerated = GetSailingWithReasonForNotGenerated();
			Sailing = sailingWithReasonForNotGenerated.Sailing;
			ReasonForSailingNotGenerated = sailingWithReasonForNotGenerated.ReasonForNotGenerated;

			if (Sailing != null)
			{
				Sailing.JX_IsPublished = true;
			}

			ReleaseOld(oldVoyage, oldOrigin, oldDestination, oldSailing);

			if (Sailing == null)
			{
				parent.SailingPK = ZGuid.Empty;
			}
			else
			{
				UpdateISailingValues();
			}

			Dirty = false;
		}

		void OnNotifyRead()
		{
			if (HasSufficientInformation)
			{
				RecreateSailing();

				if (VoyOrigin == null
					|| VoyDestination == null
					|| VoyOrigin.JA_RL_NKPortOfLoadingInfo.HasErrors()
					|| VoyDestination.JB_RL_NKPortOfDischargeInfo.HasErrors()
					|| !VoyOrigin.JA_E_DEP.IsEmpty && !VoyOrigin.JA_E_DEP.IsValid
					|| !VoyDestination.JB_E_ARV.IsEmpty && !VoyDestination.JB_E_ARV.IsValid
					|| SailingScheduleDataVendor.IsOriginUpdateSuppressed(VoyOrigin.Factory) && SailingScheduleDataVendor.IsDestinationUpdateSuppressed(VoyDestination.Factory))
				{
					return;
				}

				if (VoyOrigin.Factory.IsInTransaction)
				{
					return;
				}

				var confirmationProvider = VoyOrigin.Factory.GetValue<IConfirmationProvider>();

				if (confirmationProvider == null)
				{
					return;
				}

				var sailingInfoIncludingRelatedPorts = SailingScheduleDataVendor.Instance.TryFindSailingIncludingRelatedPorts(VoyOrigin, VoyDestination);

				if (sailingInfoIncludingRelatedPorts == null)
				{
					return;
				}

				if (VoyOrigin.JA_RL_NKPortOfLoading == sailingInfoIncludingRelatedPorts.Load
					&& VoyOrigin.JA_E_DEP == sailingInfoIncludingRelatedPorts.ETD
					&& VoyDestination.JB_RL_NKPortOfDischarge == sailingInfoIncludingRelatedPorts.Discharge
					&& VoyDestination.JB_E_ARV == sailingInfoIncludingRelatedPorts.ETA)
				{
					return;
				}

				var message = Res.GetString("a06c6ea1-6001-4430-80ea-90651983a81a",
					@"Global Sailing Schedules module could not find {0}{4} to {1}{5} route.
Related port routing is found for {2}{6} to {3}{7}.
Would you like to update your routing details with the related ports?",
					VoyOrigin.JA_RL_NKPortOfLoading,
					VoyDestination.JB_RL_NKPortOfDischarge,
					sailingInfoIncludingRelatedPorts.Load,
					sailingInfoIncludingRelatedPorts.Discharge,
					VoyOrigin.JA_E_DEP.IsEmpty ? "" : (" " + VoyOrigin.JA_E_DEP),
					VoyDestination.JB_E_ARV.IsEmpty ? "" : (" " + VoyDestination.JB_E_ARV),
					sailingInfoIncludingRelatedPorts.ETD.IsEmpty ? "" : (" " + sailingInfoIncludingRelatedPorts.ETD),
					sailingInfoIncludingRelatedPorts.ETA.IsEmpty ? "" : (" " + sailingInfoIncludingRelatedPorts.ETA));
				var caption = Res.GetString("FCCAA66C-8654-4ECF-B9A5-4A048501BBBA", "Please confirm");
				var confirmationResult = confirmationProvider.GetConfirmation(message, caption, ConfirmationOption.YesNo);

				if (confirmationResult == ConfirmationResult.Yes)
				{
					if (parent.Load != sailingInfoIncludingRelatedPorts.Load)
					{
						parent.Load = sailingInfoIncludingRelatedPorts.Load;
					}

					if (parent.ETD != sailingInfoIncludingRelatedPorts.ETD)
					{
						parent.ETD = sailingInfoIncludingRelatedPorts.ETD;
						DepartureDirty = true;
					}

					if (parent.Discharge != sailingInfoIncludingRelatedPorts.Discharge)
					{
						parent.Discharge = sailingInfoIncludingRelatedPorts.Discharge;
					}

					if (parent.ETA != sailingInfoIncludingRelatedPorts.ETA)
					{
						parent.ETA = sailingInfoIncludingRelatedPorts.ETA;
						ArrivalDirty = true;
					}

					using (SailingScheduleDataVendor.SuppressVoyageOriginUpdate(VoyOrigin.Factory))
					using (SailingScheduleDataVendor.SuppressVoyageDestinationUpdate(VoyDestination.Factory))
					{
						RecreateSailing();
					}
				}
			}
			else
			{
				Sailing = null;
				parent.SailingPK = ZGuid.Empty;
				performedInitialRead = true;
				Dirty = false;
			}
		}

		internal string voyageEventStackTrace;

		void HookVoyage(JobVoyage voyage)
		{
			voyage.VoyageUpdatedByDataRefresh += new EventHandler(Voyage_VoyageUpdatedByDataRefresh);

			voyageEventStackTrace = string.Concat((NoResString)"Hook Voyage Stack Trace: ", System.Environment.NewLine, new StackTrace().ToString());
		}

		void UnHookVoyage(JobVoyage voyage)
		{
			voyage.VoyageUpdatedByDataRefresh -= new EventHandler(Voyage_VoyageUpdatedByDataRefresh);

			voyageEventStackTrace = string.Concat((NoResString)"UnHook Voyage Stack Trace: ", System.Environment.NewLine, new StackTrace().ToString(), System.Environment.NewLine, voyageEventStackTrace);
		}

		void Voyage_VoyageUpdatedByDataRefresh(object sender, EventArgs e)
		{
			if (Sailing != null)
			{
				SetVoyageFields(true);

				if (parent is Transport transport)
				{
					transport.UpdateVoyageRelatedOriginalValues();
				}
			}
		}

		void Saving_LogDateEvents(object sender, EventArgs e)
		{
			parent.LogDateEvents(sender);
		}

		void HookOrigin(VoyageOrigin origin)
		{
			origin.OriginUpdatedByDataRefresh += new EventHandler(Origin_OriginUpdatedByDataRefresh);
			origin.AdditionalValidation = parent.VoyOriginAdditionalValidation;

			origin.OriginSaving += new EventHandler(Saving_LogDateEvents);
		}

		void UnHookOrigin(VoyageOrigin origin)
		{
			origin.OriginUpdatedByDataRefresh -= new EventHandler(Origin_OriginUpdatedByDataRefresh);
			origin.AdditionalValidation = null;

			origin.OriginSaving -= new EventHandler(Saving_LogDateEvents);
		}

		void Origin_OriginUpdatedByDataRefresh(object sender, EventArgs e)
		{
			if (Sailing != null)
			{
				SetOriginFields(true);

				if (parent is Transport transport)
				{
					transport.UpdateOriginRelatedOriginalValues();
				}
			}
		}

		void HookDestination(VoyageDestination destination)
		{
			destination.DestinationUpdatedByDataRefresh += new EventHandler(Destination_DestinationUpdatedByDataRefresh);
			destination.AdditionalValidation = parent.VoyDestinationAdditionalValidation;

			destination.DestinationSaving += new EventHandler(Saving_LogDateEvents);
		}

		void UnHookDestination(VoyageDestination destination)
		{
			destination.DestinationUpdatedByDataRefresh -= new EventHandler(Destination_DestinationUpdatedByDataRefresh);
			destination.AdditionalValidation = null;

			destination.DestinationSaving -= new EventHandler(Saving_LogDateEvents);
		}

		void Destination_DestinationUpdatedByDataRefresh(object sender, EventArgs e)
		{
			if (Sailing != null)
			{
				SetDestinationFields(true);

				if (parent is Transport transport)
				{
					transport.UpdateDestinationRelatedOriginalValues();
				}
			}
		}

		void HookSailing(JobSailing sailing)
		{
			sailing.SailingUpdatedByDataRefresh += new EventHandler(Sailing_SailingUpdatedByDataRefresh);
			sailing.AdditionalValidation = parent.SailingAdditionalValidation;

			sailing.SailingSaving += new EventHandler(Saving_LogDateEvents);
		}

		void UnHookSailing(JobSailing sailing)
		{
			sailing.SailingUpdatedByDataRefresh -= new EventHandler(Sailing_SailingUpdatedByDataRefresh);
			sailing.AdditionalValidation = null;

			sailing.SailingSaving -= new EventHandler(Saving_LogDateEvents);
		}

		void Sailing_SailingUpdatedByDataRefresh(object sender, EventArgs e)
		{
			if (Sailing != null)
			{
				SetSailingFields();

				if (parent is Transport transport)
				{
					transport.UpdateSailingRelatedOriginalValues();
				}
			}
		}

		void SetAllScheduleFields()
		{
			SetVoyageFields();
			SetOriginFields();
			SetDestinationFields();
			SetSailingFields();
		}

		void SetVoyageFields(bool byDataRefresh = false)
		{
			if (Sailing != null && Sailing.Voyage != null)
			{
				parent.Voyage = Sailing.Voyage.JV_VoyageFlight;
				parent.Vessel = Sailing.Voyage.JV_RV_NKVessel;
				parent.RegistrationNo = Sailing.Voyage.JV_RegistrationNo;
				parent.IsCargoOnly = Sailing.Voyage.JV_IsCargoOnly;
				parent.IsCharter = Sailing.Voyage.JV_IsChartered;
				parent.AircraftType = Sailing.Voyage.JV_AircraftType;

				SetVoyageFieldsCore();

				if (byDataRefresh)
				{
					TransportPropertiesChangedByDataRefresh[JobConsolTransportSchema.JW_Vessel.Name] = Sailing.Voyage.JV_RV_NKVessel;
					TransportPropertiesChangedByDataRefresh[JobConsolTransportSchema.JW_VoyageFlight.Name] = Sailing.Voyage.JV_VoyageFlight;
					TransportPropertiesChangedByDataRefresh[JobConsolTransportSchema.JW_IsCharter.Name] = Sailing.Voyage.JV_IsChartered;
					TransportPropertiesChangedByDataRefresh[JobConsolTransportSchema.JW_IsCargoOnly.Name] = Sailing.Voyage.JV_IsCargoOnly;
					TransportPropertiesChangedByDataRefresh[JobConsolTransportSchema.JW_AircraftType.Name] = Sailing.Voyage.JV_AircraftType;
				}
			}
		}

		protected virtual void SetVoyageFieldsCore()
		{
		}

		void SetOriginFields(bool byDataRefresh = false)
		{
			VoyageOrigin origin = Sailing.Origin;

			parent.Load = origin.JA_RL_NKPortOfLoading;
			parent.ETD = origin.JA_E_DEP;
			parent.ATD = origin.JA_A_DEP;
			parent.STD = origin.JA_S_DEP;
			parent.SetDocsCutOff(origin.JA_DocumentaryCutoff);
			parent.SetFCLReceivalCommences(origin.JA_ReceivalCommences);
			parent.SetFCLCutOff(origin.JA_CutOff);
			parent.SetVGMCutOff(origin.JA_VGMCutOff);
			parent.SetOA_DepartureLocation(origin.JA_OA_DepartureCTOAddress);
			parent.SetLoadETA(origin.JA_E_ARV);
			parent.SetLoadATA(origin.JA_A_ARV);
			parent.SetEmptyCutOff(origin.JA_EmptyCutOff);
			parent.SetEmptyReceivalCommences(origin.JA_EmptyReceivalCommences);
			parent.SetDGCutOff(origin.JA_DGCutOff);
			parent.SetDGReceivalCommences(origin.JA_DGReceivalCommences);
			parent.SetReeferCutOff(origin.JA_ReeferCutOff);
			parent.SetReeferReceivalCommences(origin.JA_ReeferReceivalCommences);

			if (byDataRefresh)
			{
				TransportPropertiesChangedByDataRefresh[JobConsolTransportSchema.JW_RL_NKLoadPort.Name] = origin.JA_RL_NKPortOfLoading;
				TransportPropertiesChangedByDataRefresh[JobConsolTransportSchema.JW_OA_DepartureLocation.Name] = origin.JA_OA_DepartureCTOAddress;
				TransportPropertiesChangedByDataRefresh[JobConsolTransportSchema.JW_STD.Name] = origin.JA_S_DEP;
				TransportPropertiesChangedByDataRefresh[JobConsolTransportSchema.JW_ETD.Name] = origin.JA_E_DEP;
				TransportPropertiesChangedByDataRefresh[JobConsolTransportSchema.JW_ATD.Name] = origin.JA_A_DEP;
			}
		}

		void SetDestinationFields(bool byDataRefresh = false)
		{
			VoyageDestination destination = Sailing.Destination;

			parent.Discharge = destination.JB_RL_NKPortOfDischarge;
			parent.ETA = destination.JB_E_ARV;
			parent.ATA = destination.JB_A_ARV;
			parent.STA = destination.JB_S_ARV;
			parent.SetAvailabilityDate(destination.JB_AvailabilityDate);
			parent.SetStorageDate(destination.JB_StorageDate);
			parent.SetOA_ArrivalLocation(destination.JB_OA_ArrivalCTOAddress);

			if (byDataRefresh)
			{
				TransportPropertiesChangedByDataRefresh[JobConsolTransportSchema.JW_RL_NKDiscPort.Name] = destination.JB_RL_NKPortOfDischarge;
				TransportPropertiesChangedByDataRefresh[JobConsolTransportSchema.JW_OA_ArrivalLocation.Name] = destination.JB_OA_ArrivalCTOAddress;
				TransportPropertiesChangedByDataRefresh[JobConsolTransportSchema.JW_STA.Name] = destination.JB_S_ARV;
				TransportPropertiesChangedByDataRefresh[JobConsolTransportSchema.JW_ATA.Name] = destination.JB_A_ARV;
				TransportPropertiesChangedByDataRefresh[JobConsolTransportSchema.JW_ETA.Name] = destination.JB_E_ARV;
			}
		}

		void SetSailingFields()
		{
			parent.SetLCLReceivalCommences(Sailing.JX_DepotReceivalCommences);
			parent.SetLCLCutOff(Sailing.JX_DepotCutOff);
			parent.SetLCLAvailabilityDate(Sailing.JX_DepotAvailabilityDate);
			parent.SetLCLStorageDate(Sailing.JX_DepotStorageDate);
			parent.OnlineScheduleStatus = Sailing.JX_OnlineScheduleStatus;
			parent.SetServiceString(Sailing.JX_ServiceString);
			parent.SetArrivalPortRouteId(Sailing.JX_ArrivalPortRouteId);
			parent.SetDeparturePortRouteId(Sailing.JX_DeparturePortRouteId);
		}

		JobVoyage GetOrCreateVoyage()
		{
			JobVoyage result = GetExistingVoyage() ?? NewVoyage();

			return result;
		}

		VoyageOrigin GetOrCreateOrigin()
		{
			VoyageOrigin result = GetExistingOrigin() ?? CreateNewVoyageOrigin();

			if (preserveParentScheduleDates && !parent.STD.IsEmpty && result.JA_S_DEP != parent.STD)
			{
				result.JA_S_DEP = parent.STD;
			}

			return result;
		}

		VoyageOrigin GetExistingOrigin()
		{
			ZQuery voyageOriginFilter = new ZQuery(JobVoyOriginSchema.JA_JV, Voyage.PK);
			voyageOriginFilter.AddToFilter(new ZQuery(JobVoyOriginSchema.JA_RL_NKPortOfLoading, parent.Load));

			VoyageOrigin result = parent.Factory.LoadTop1<VoyageOrigin>(voyageOriginFilter);

			if (result != null && parent.AllowScheduleDatesChanging && ShouldUpdateETD)
			{
				if (result.JA_E_DEP != parent.ETD)
				{
					result.JA_E_DEP = parent.ETD;
				}

				if (!parent.STD.IsEmpty && result.JA_S_DEP != parent.STD)
				{
					result.JA_S_DEP = parent.STD;
				}
			}

			return result;
		}

		VoyageDestination GetOrCreateDestination()
		{
			VoyageDestination result = GetExistingDestination() ?? CreateNewVoyageDestination();

			if (preserveParentScheduleDates && !parent.STA.IsEmpty && result.JB_S_ARV != parent.STA)
			{
				result.JB_S_ARV = parent.STA;
			}

			return result;
		}

		VoyageDestination GetExistingDestination()
		{
			ZQuery voyageDestinationFilter = new ZQuery(JobVoyDestinationSchema.JB_JV, Voyage.PK);
			voyageDestinationFilter.AddToFilter(new ZQuery(JobVoyDestinationSchema.JB_RL_NKPortOfDischarge, parent.Discharge));

			VoyageDestination result = parent.Factory.LoadTop1<VoyageDestination>(voyageDestinationFilter);

			if (result != null && parent.AllowScheduleDatesChanging && ShouldUpdateETA)
			{
				if (result.JB_E_ARV != parent.ETA)
				{
					result.JB_E_ARV = parent.ETA;
				}

				if (!parent.STA.IsEmpty && result.JB_S_ARV != parent.STA)
				{
					result.JB_S_ARV = parent.STA;
				}
			}

			return result;
		}

		JobSailingWithReasonForNotGenerated GetSailingWithReasonForNotGenerated()
		{
			if (Voyage == null || VoyOrigin == null || VoyDestination == null)
			{
				return new JobSailingWithReasonForNotGenerated(null, ZString.Empty);
			}
			else
			{
				var sailingFilter = new ZQuery();

				sailingFilter.AddToFilter(new ZQuery(JobSailingSchema.JX_JA, VoyOrigin.PK));
				sailingFilter.AddToFilter(JobSailingSchema.JX_JB, VoyDestination.PK);
				var result = parent.Factory.LoadTop1<JobSailing>(sailingFilter);

				if (result != null)
				{
					return new JobSailingWithReasonForNotGenerated(result, ZString.Empty);
				}

				var reasonForNotGenerated = Voyage.GenerateSailings();
				result = parent.Factory.LoadTop1<JobSailing>(sailingFilter);
				// if the sailing cant be found even after calling GenerateSailings() then that means it's invalid and should not be created.
				return new JobSailingWithReasonForNotGenerated(result, reasonForNotGenerated);
			}
		}

		void EnsureSailingExists()
		{
			if ((Sailing == null || Sailing.IsDeleted) && !running)
			{
				running = true;
				try
				{
					OnNotifyRead();
				}
				finally
				{
					running = false;
				}
			}
		}

		void EnsureUniqueVoyage()
		{
			if (ShouldCheckUniqueVoyage)
			{
				var existingVoyage = GetBestMatchingExistingVoyage(GetExistingMatchingVoyageQuery());
				if (existingVoyage != null && existingVoyage.PK != Voyage.PK)
				{
					if (Voyage.Sailings.Contains(Sailing))
					{
						Voyage.Sailings.Remove(Sailing);
					}

					if (Voyage.Origins.Contains(VoyOrigin))
					{
						Voyage.Origins.Remove(VoyOrigin);
					}

					if (Voyage.Destinations.Contains(VoyDestination))
					{
						Voyage.Destinations.Remove(VoyDestination);
					}

					Voyage.Delete();
					Voyage = existingVoyage;
					VoyOrigin.JA_JV = Voyage.PK;
					VoyDestination.JB_JV = Voyage.PK;
				}
			}
		}

		protected virtual JobVoyage GetBestMatchingExistingVoyage(ZQuery voyageFilter)
		{
			return parent.Factory.Load<JobVoyage>(voyageFilter).FirstOrDefault();
		}

		protected virtual ZQuery GetExistingMatchingVoyageQuery()
		{
			var voyageFilter = new ZDBOnlyQuery(typeof(JobVoyage));
			voyageFilter.AddToFilter(JobVoyageSchema.JV_VoyageFlight, Voyage.JV_VoyageFlight);
			voyageFilter.AddToFilter(TransportModeFilter);
			voyageFilter.AddToFilter(JobVoyageSchema.JV_RV_NKVessel, Voyage.JV_RV_NKVessel);
			voyageFilter.AddToFilter(JobVoyageSchema.PK, SQLComparisonOperator.NotEqual, Voyage.PK);
			return voyageFilter;
		}

		#region EnsureUniqueVoyOrigin

		bool EnsureUniqueVoyOrigin()
		{
			ZDBOnlyQuery originFilter = new ZDBOnlyQuery(typeof(VoyageOrigin));
			originFilter.AddToFilter(JobVoyOriginSchema.JA_RL_NKPortOfLoading, VoyOrigin.JA_RL_NKPortOfLoading);
			originFilter.AddToFilter(JobVoyOriginSchema.JA_JV, VoyOrigin.JA_JV);
			originFilter.AddToFilter(JobVoyOriginSchema.PK, SQLComparisonOperator.NotEqual, VoyOrigin.PK);

			VoyageOrigin existingOrigin = parent.Factory.LoadTop1<VoyageOrigin>(originFilter);
			if (existingOrigin != null)
			{
				SubstituteVoyageOriginWithAlreadyExistingOne(existingOrigin);
			}

			RemoveUnusedVoyageOrigins();

			return existingOrigin != null;
		}

		void SubstituteVoyageOriginWithAlreadyExistingOne(VoyageOrigin existingOrigin)
		{
			VoyageOrigin originToBeReleased = VoyOrigin;
			VoyOrigin = existingOrigin;

			using (SailingScheduleDataVendor.SuppressVoyageOriginUpdate(VoyOrigin.Factory))
			{
				Sailing.JX_JA = VoyOrigin.PK;

				if (Voyage != null)
				{
					foreach (JobSailing sailing in Voyage.Sailings)
					{
						if (sailing.JX_JA == originToBeReleased.PK)
						{
							sailing.JX_JA = VoyOrigin.PK;
						}
					}
				}
			}

			if (!originToBeReleased.IsDeleted)
			{
				ReleaseOrigin(originToBeReleased);
			}
		}

		void RemoveUnusedVoyageOrigins()
		{
			if (Voyage != null)
			{
				foreach (VoyageOrigin autoCreatedOrigin in Voyage.Origins.Cast<VoyageOrigin>().Where(o => o.JA_AutoCreated).ToArray())
				{
					JobSailing relatedSailing = parent.Factory.LoadTop1<JobSailing>(new ZQuery(JobSailingSchema.JX_JA, autoCreatedOrigin.PK));
					if (relatedSailing == null)
					{
						Voyage.Origins.RemoveAndDelete(autoCreatedOrigin);
					}
				}
			}
		}

		#endregion

		#region EnsureUniqueVoyDestination

		bool EnsureUniqueVoyDestination()
		{
			ZDBOnlyQuery destinationFilter = new ZDBOnlyQuery(typeof(VoyageDestination));
			destinationFilter.AddToFilter(JobVoyDestinationSchema.JB_RL_NKPortOfDischarge, VoyDestination.JB_RL_NKPortOfDischarge);
			destinationFilter.AddToFilter(JobVoyDestinationSchema.JB_JV, VoyDestination.JB_JV);
			destinationFilter.AddToFilter(JobVoyDestinationSchema.PK, SQLComparisonOperator.NotEqual, VoyDestination.PK);

			VoyageDestination existingDestination = parent.Factory.LoadTop1<VoyageDestination>(destinationFilter);
			if (existingDestination != null)
			{
				SubstituteVoyageDestinationWithAlreadyExistingOne(existingDestination);
			}

			RemoveUnusedVoyageDestinations();

			return existingDestination != null;
		}

		void SubstituteVoyageDestinationWithAlreadyExistingOne(VoyageDestination existingDestination)
		{
			VoyageDestination destinationToBeReleased = VoyDestination;
			VoyDestination = existingDestination;

			using (SailingScheduleDataVendor.SuppressVoyageDestinationUpdate(VoyDestination.Factory))
			{
				Sailing.JX_JB = VoyDestination.PK;

				if (Voyage != null)
				{
					foreach (JobSailing sailing in Voyage.Sailings)
					{
						if (sailing.JX_JB == destinationToBeReleased.PK)
						{
							sailing.JX_JB = VoyDestination.PK;
						}
					}
				}
			}

			if (!destinationToBeReleased.IsDeleted)
			{
				ReleaseDestination(destinationToBeReleased);
			}
		}

		void RemoveUnusedVoyageDestinations()
		{
			if (Voyage != null)
			{
				foreach (VoyageDestination autoCreatedDestination in Voyage.Destinations.Cast<VoyageDestination>().Where(d => d.JB_AutoCreated).ToArray())
				{
					JobSailing relatedSailing = parent.Factory.LoadTop1<JobSailing>(new ZQuery(JobSailingSchema.JX_JB, autoCreatedDestination.PK));
					if (relatedSailing == null)
					{
						Voyage.Destinations.RemoveAndDelete(autoCreatedDestination);
					}
				}
			}
		}

		#endregion

		void EnsureUniqueSailing()
		{
			ZQuery sailingFilter = new ZQuery(JobSailingSchema.JX_JA, Sailing.JX_JA);
			sailingFilter.AddToFilter(new ZQuery(JobSailingSchema.JX_JB, Sailing.JX_JB));
			sailingFilter.AddToFilter(new ZQuery(JobSailingSchema.PK, SQLComparisonOperator.NotEqual, Sailing.PK));

			JobSailing existingSailing = parent.Factory.LoadTop1<JobSailing>(sailingFilter);
			if (existingSailing != null)
			{
				Sailing.Delete();
				Sailing = parent.Factory.Load<JobSailing>(existingSailing.PK);
				parent.SailingPK = Sailing.PK;
			}
		}

		void ReleaseSailing(JobSailing sailingToRelease)
		{
			if (!sailingToRelease.IsInDatabase)
			{
				sailingToRelease.Delete();
			}
			else
			{
				sailingToRelease.Reload();
			}
		}

		void ReleaseVoyage(JobVoyage voyageToRelease)
		{
			if (!voyageToRelease.IsInDatabase)
			{
				ZQuery originsUsingVoyage = new ZQuery(JobVoyOriginSchema.JA_JV, voyageToRelease.PK) { FetchOnlyFromLocalCache = true };
				VoyageOrigin[] origins = voyageToRelease.Factory.Load<VoyageOrigin>(originsUsingVoyage);

				ZQuery destinationUsingVoyage = new ZQuery(JobVoyDestinationSchema.JB_JV, voyageToRelease.PK) { FetchOnlyFromLocalCache = true };
				VoyageDestination[] destinations = voyageToRelease.Factory.Load<VoyageDestination>(destinationUsingVoyage);

				if (origins.Length == 0 && destinations.Length == 0)
				{
					voyageToRelease.Delete();
					voyageToRelease = null;

					AfterOldVoyageHasBeenReleased();
				}
			}
			else
			{
				foreach (var log in voyageToRelease.Logs.LogsNotInDB)
				{
					log.Delete();
				}

				voyageToRelease.Reload();
			}
		}

		protected virtual void AfterOldVoyageHasBeenReleased()
		{
		}

		void ReleaseOrigin(VoyageOrigin originToRelease)
		{
			if (!originToRelease.IsInDatabase)
			{
				ZQuery sailingsUsingOrigin = new ZQuery(JobSailingSchema.JX_JA, originToRelease.PK) { FetchOnlyFromLocalCache = true };
				JobSailing[] sailings = originToRelease.Factory.Load<JobSailing>(sailingsUsingOrigin);

				if (sailings.Length == 0 || sailings.All(c => !c.IsReferenced(true)))
				{
					originToRelease.Delete();
					originToRelease = null;

					foreach (var originSailing in sailings)
					{
						if (Sailing == null || originSailing.PK != Sailing.PK)
						{
							ReleaseSailing(originSailing);
						}
					}
				}
			}
			else
			{
				originToRelease.Reload();
			}
		}

		void ReleaseDestination(VoyageDestination destinationToRelease)
		{
			if (!destinationToRelease.IsInDatabase)
			{
				ZQuery sailingsUsingDestination = new ZQuery(JobSailingSchema.JX_JB, destinationToRelease.PK) { FetchOnlyFromLocalCache = true };
				JobSailing[] sailings = destinationToRelease.Factory.Load<JobSailing>(sailingsUsingDestination);

				if (sailings.Length == 0 || sailings.All(c => !c.IsReferenced(true)))
				{
					destinationToRelease.Delete();
					destinationToRelease = null;

					foreach (var destinationSailing in sailings)
					{
						if (Sailing == null || destinationSailing.PK != Sailing.PK)
						{
							ReleaseSailing(destinationSailing);
						}
					}
				}
			}
			else
			{
				destinationToRelease.Reload();
			}
		}

		void ReleaseOld(JobVoyage oldVoyage, VoyageOrigin oldOrigin, VoyageDestination oldDestination, JobSailing oldSailing)
		{
			if (oldSailing != null && !oldSailing.IsDeleted && oldSailing != Sailing)
			{
				ReleaseSailing(oldSailing);
			}

			if (oldOrigin != null && !oldOrigin.IsDeleted && oldOrigin != VoyOrigin)
			{
				ReleaseOrigin(oldOrigin);
			}

			if (oldDestination != null && !oldDestination.IsDeleted && oldDestination != VoyDestination)
			{
				ReleaseDestination(oldDestination);
			}

			if (oldVoyage != null && !oldVoyage.IsDeleted && oldVoyage != Voyage)
			{
				ReleaseVoyage(oldVoyage);
			}
		}

		bool performedInitialRead;
		bool running;
		JobSailing sailing;
		JobVoyage voyage;
		VoyageOrigin voyOrigin;
		VoyageDestination voyDestination;
		protected readonly ISailingManaged parent;

		#endregion
	}
}
