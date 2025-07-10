using CargoWise.Types;

namespace Enterprise.Freight.LocalCartage.Business
{
	public class StandaloneCartageBehaviorStrategy : CommonCartageBehaviorStrategy
	{
		protected override void SetupAddress(CommonCartage cartage, MasterFiles.Business.JobDocAddress docAddress, ZString orgType)
		{
			docAddress.ReadOnly = cartage.JJ_IsCancelled;
		}

		public override void AfterCartageTypeChange(CommonCartage cartage)
		{
			base.AfterCartageTypeChange(cartage);

			var cartageType = cartage.CartageType;
			if (cartageType != null && cartageType.IsLoose && cartage.LooseBookedMoves.Count == 0)
			{
				cartage.LooseBookedMoves.AddNew();
			}
		}

		public override void AfterTransportModeChange(CommonCartage cartage)
		{
			ClearSailingReference(cartage);
		}

		public override void AfterSailingPKChange(CommonCartage cartage)
		{
			cartage.SailingManager.Sailing = cartage.SailingStandalone;
			cartage.SailingManager.Dirty = false;
			SetAllScheduleFields(cartage);
		}

		void SetAllScheduleFields(CommonCartage cartage)
		{
			if (!isDefaultingFromSchedule)
			{
				isDefaultingFromSchedule = true;
				try
				{
					SetVoyageFields(cartage);
					SetVoyOriginFields(cartage);
					SetVoyDestinationFields(cartage);

					cartage.RefreshBinding();
				}
				finally
				{
					isDefaultingFromSchedule = false;
				}
			}
		}
		bool isDefaultingFromSchedule;

		void SetVoyageFields(CommonCartage cartage)
		{
			if (cartage.SailingStandalone != null && cartage.SailingStandalone.Voyage != null)
			{
				cartage.fVessel = cartage.SailingStandalone.Voyage.JV_RV_NKVessel;
				cartage.fVoyage = cartage.SailingStandalone.Voyage.JV_VoyageFlight;

				if (!cartage.IsValidationSuspended)
				{
					cartage.Validation.ValidateVessel();
					cartage.Validation.ValidateVoyageFlight();
				}
			}
		}

		void SetVoyOriginFields(CommonCartage cartage)
		{
			if (cartage.SailingStandalone != null && cartage.SailingStandalone.Origin != null && !cartage.SailingStandalone.Origin.IsDeleted)
			{
				cartage.fPortOfLoading = cartage.SailingStandalone.Origin.JA_RL_NKPortOfLoading;
				cartage.fE_DEP = cartage.SailingStandalone.Origin.JA_E_DEP;

				if (!cartage.IsValidationSuspended)
				{
					cartage.Validation.ValidatePortOfLoading();
					cartage.Validation.ValidateE_DEP();
				}
			}
		}

		void SetVoyDestinationFields(CommonCartage cartage)
		{
			if (cartage.SailingStandalone != null && cartage.SailingStandalone.Destination != null)
			{
				cartage.fPortOfDischarge = cartage.SailingStandalone.Destination.JB_RL_NKPortOfDischarge;
				cartage.fE_ARV = cartage.SailingStandalone.Destination.JB_E_ARV;

				if (!cartage.IsValidationSuspended)
				{
					cartage.Validation.ValidatePortOfDischarge();
					cartage.Validation.ValidateE_ARV();
				}
			}
		}

		void ClearSailingReference(CommonCartage cartage)
		{
			cartage.fSailingManager = null;

			cartage.JJ_JX_Sailing = ZGuid.Empty;
			cartage.fVoyage = ZString.Empty;
			cartage.fVessel = ZString.Empty;
			cartage.fPortOfLoading = ZString.Empty;
			cartage.fPortOfDischarge = ZString.Empty;
			cartage.SailingManager.NotifyRead();
			cartage.fE_DEP = ZDateTime.Empty;
			cartage.fE_ARV = ZDateTime.Empty;
			cartage.RefreshBinding();
		}

		public override ZString GetPortOfLoading(CommonCartage cartage)
		{
			cartage.SailingManager.NotifyRead();
			return cartage.fPortOfLoading;
		}

		public override ZString GetPortOfDischarge(CommonCartage cartage)
		{
			cartage.SailingManager.NotifyRead();
			return cartage.fPortOfDischarge;
		}

		public override ZString GetVessel(CommonCartage cartage)
		{
			cartage.SailingManager.NotifyRead();
			return cartage.fVessel;
		}

		public override ZString GetVoyageFlight(CommonCartage cartage)
		{
			cartage.SailingManager.NotifyRead();
			return cartage.fVoyage;
		}

		public override ZDateTime GetE_DEP(CommonCartage cartage)
		{
			cartage.SailingManager.NotifyRead();
			return cartage.fE_DEP;
		}

		public override ZDateTime GetE_ARV(CommonCartage cartage)
		{
			cartage.SailingManager.NotifyRead();
			return cartage.fE_ARV;
		}

		public override ZDateTime GetA_DEP(CommonCartage cartage)
		{
			var result = ZDateTime.Empty;
			cartage.SailingManager.NotifyRead();

			if (cartage.SailingStandalone != null)
			{
				result = cartage.SailingStandalone.Origin.JA_A_DEP;
			}

			return result;
		}

		public override ZDateTime GetA_ARV(CommonCartage cartage)
		{
			var result = ZDateTime.Empty;
			cartage.SailingManager.NotifyRead();

			if (cartage.SailingStandalone != null)
			{
				result = cartage.SailingStandalone.Destination.JB_A_ARV;
			}

			return result;
		}

		public override void SetPortOfLoading(CommonCartage cartage, ZString value)
		{
			cartage.SailingManager.NotifyRead();
			cartage.fPortOfLoading = value;
			cartage.SailingManager.LoadDirty = true;
			cartage.SailingManager.NotifyRead();
			cartage.HasChanges = true;
			if (!cartage.IsValidationSuspended)
			{
				cartage.Validation.ValidatePortOfLoading();
			}
			cartage.PortOfLoadingInfo.RefreshBinding();
		}

		public override void SetPortOfDischarge(CommonCartage cartage, ZString value)
		{
			cartage.SailingManager.NotifyRead();
			cartage.fPortOfDischarge = value;
			cartage.SailingManager.DischargeDirty = true;
			cartage.SailingManager.NotifyRead();
			cartage.HasChanges = true;
			if (!cartage.IsValidationSuspended)
			{
				cartage.Validation.ValidatePortOfDischarge();
			}
			cartage.PortOfDischargeInfo.RefreshBinding();
		}

		public override void SetVessel(CommonCartage cartage, ZString value)
		{
			cartage.SailingManager.NotifyRead();
			cartage.fVessel = value;
			cartage.SailingManager.VesselDirty = true;
			cartage.SailingManager.NotifyRead();
			cartage.HasChanges = true;
			if (!cartage.IsValidationSuspended)
			{
				cartage.Validation.ValidateVessel();
			}
			cartage.VesselInfo.RefreshBinding();
		}

		public override void SetVoyageFlight(CommonCartage cartage, ZString value)
		{
			cartage.SailingManager.NotifyRead();
			cartage.fVoyage = value;
			cartage.SailingManager.VoyageDirty = true;
			cartage.SailingManager.NotifyRead();
			cartage.HasChanges = true;
			if (!cartage.IsValidationSuspended)
			{
				cartage.Validation.ValidateVoyageFlight();
			}
			cartage.VoyageFlightInfo.RefreshBinding();
		}

		public override void SetE_DEP(CommonCartage cartage, ZDateTime value)
		{
			cartage.SailingManager.NotifyRead();
			cartage.fE_DEP = value;
			cartage.SailingManager.DepartureDirty = true;
			cartage.SailingManager.NotifyRead();
			cartage.HasChanges = true;
			if (!cartage.IsValidationSuspended)
			{
				cartage.Validation.ValidateE_DEP();
			}
			cartage.E_DEPInfo.RefreshBinding();
		}

		public override void SetE_ARV(CommonCartage cartage, ZDateTime value)
		{
			cartage.SailingManager.NotifyRead();
			cartage.fE_ARV = value;
			cartage.SailingManager.ArrivalDirty = true;
			cartage.SailingManager.NotifyRead();
			cartage.HasChanges = true;
			if (!cartage.IsValidationSuspended)
			{
				cartage.Validation.ValidateE_ARV();
			}
			cartage.E_ARVInfo.RefreshBinding();
		}

		public override void SetFCLCutOff(CommonCartage cartage, ZDateTime value)
		{
			if (cartage.SailingStandalone != null)
			{
				cartage.SailingStandalone.Origin.JA_CutOff = value;
				cartage.HasChanges = true;
				if (!cartage.IsValidationSuspended)
				{
					cartage.Validation.ValidateFCLCutOff();
				}
			}
			cartage.FCLCutOffInfo.RefreshBinding();
		}

		public override void SetLCLCutOff(CommonCartage cartage, ZDateTime value)
		{
			if (cartage.SailingStandalone != null)
			{
				cartage.SailingStandalone.JX_DepotCutOff = value;
				cartage.HasChanges = true;
				if (!cartage.IsValidationSuspended)
				{
					cartage.Validation.ValidateLCLCutOff();
				}
			}
			cartage.LCLCutOffInfo.RefreshBinding();
		}

		public override void SetFCLReceivalCommences(CommonCartage cartage, ZDateTime value)
		{
			if (cartage.SailingStandalone != null)
			{
				cartage.SailingStandalone.Origin.JA_ReceivalCommences = value;
				cartage.HasChanges = true;
				if (!cartage.IsValidationSuspended)
				{
					cartage.Validation.ValidateFCLReceivalCommences();
				}
			}
			cartage.FCLReceivalCommencesInfo.RefreshBinding();
		}

		public override void SetLCLReceivalCommences(CommonCartage cartage, ZDateTime value)
		{
			if (cartage.SailingStandalone != null)
			{
				cartage.SailingStandalone.JX_DepotReceivalCommences = value;
				cartage.HasChanges = true;
				if (!cartage.IsValidationSuspended)
				{
					cartage.Validation.ValidateLCLReceivalCommences();
				}
			}
			cartage.LCLReceivalCommencesInfo.RefreshBinding();
		}

		public override void SetFCLAvailabilityDate(CommonCartage cartage, ZDateTime value)
		{
			if (cartage.SailingStandalone != null)
			{
				cartage.SailingStandalone.Destination.JB_AvailabilityDate = value;
				cartage.HasChanges = true;
				if (!cartage.IsValidationSuspended)
				{
					cartage.Validation.ValidateFCLAvailabilityDate();
				}
			}
			cartage.FCLAvailabilityDateInfo.RefreshBinding();
		}

		public override void SetLCLAvailabilityDate(CommonCartage cartage, ZDateTime value)
		{
			if (cartage.SailingStandalone != null)
			{
				cartage.SailingStandalone.JX_DepotAvailabilityDate = value;
				cartage.HasChanges = true;
				if (!cartage.IsValidationSuspended)
				{
					cartage.Validation.ValidateLCLAvailabilityDate();
				}
			}
			cartage.LCLAvailabilityDateInfo.RefreshBinding();
		}

		public override void SetFCLStorageDate(CommonCartage cartage, ZDateTime value)
		{
			if (cartage.SailingStandalone != null)
			{
				cartage.SailingStandalone.Destination.JB_StorageDate = value;
				cartage.HasChanges = true;
				if (!cartage.IsValidationSuspended)
				{
					cartage.Validation.ValidateFCLStorageDate();
				}
			}
			cartage.FCLStorageDateInfo.RefreshBinding();
		}

		public override void SetLCLStorageDate(CommonCartage cartage, ZDateTime value)
		{
			if (cartage.SailingStandalone != null)
			{
				cartage.SailingStandalone.JX_DepotStorageDate = value;
				cartage.HasChanges = true;
				if (!cartage.IsValidationSuspended)
				{
					cartage.Validation.ValidateLCLStorageDate();
				}
			}
			cartage.LCLStorageDateInfo.RefreshBinding();
		}

		public override ZDateTime GetFCLReceivalCommences(CommonCartage cartage)
		{
			return cartage.SailingStandalone != null ? cartage.SailingStandalone.JX_JA_CTOReceivalCommences : ZDateTime.Empty;
		}

		public override ZDateTime GetLCLReceivalCommences(CommonCartage cartage)
		{
			return cartage.SailingStandalone != null ? cartage.SailingStandalone.JX_DepotReceivalCommences : ZDateTime.Empty;
		}

		public override ZDateTime GetFCLCutOff(CommonCartage cartage)
		{
			return cartage.SailingStandalone != null ? cartage.SailingStandalone.JX_JA_CTOCutOff : ZDateTime.Empty;
		}

		public override ZDateTime GetLCLCutOff(CommonCartage cartage)
		{
			return cartage.SailingStandalone != null ? cartage.SailingStandalone.JX_DepotCutOff : ZDateTime.Empty;
		}

		public override ZDateTime GetFCLAvailabilityDate(CommonCartage cartage)
		{
			return cartage.SailingStandalone != null ? cartage.SailingStandalone.JX_JB_CTOAvailabilityDate : ZDateTime.Empty;
		}

		public override ZDateTime GetLCLAvailabilityDate(CommonCartage cartage)
		{
			return cartage.SailingStandalone != null ? cartage.SailingStandalone.JX_DepotAvailabilityDate : ZDateTime.Empty;
		}

		public override ZDateTime GetFCLStorageDate(CommonCartage cartage)
		{
			return cartage.SailingStandalone != null ? cartage.SailingStandalone.JX_JB_CTOStorageDate : ZDateTime.Empty;
		}

		public override ZDateTime GetLCLStorageDate(CommonCartage cartage)
		{
			return cartage.SailingStandalone != null ? cartage.SailingStandalone.JX_DepotStorageDate : ZDateTime.Empty;
		}
	}
}

