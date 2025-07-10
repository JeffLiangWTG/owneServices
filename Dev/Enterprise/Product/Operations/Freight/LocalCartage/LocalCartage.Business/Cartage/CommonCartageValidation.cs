using System.Collections.Generic;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Environment;
using Enterprise.Freight.Common.Business;

namespace Enterprise.Freight.LocalCartage.Business
{
	public class CommonCartageValidation : JobCartageValidation
	{
		public CommonCartageValidation(CommonCartage parent)
			: base(parent)
		{
		}

		public override void ValidateAll()
		{
			base.ValidateAll();

			ValidatePortOfLoading();
			ValidatePortOfDischarge();
			ValidateE_DEP();
			ValidateE_ARV();
			ValidateVessel();
			ValidateVoyageFlight();
			ValidateFCLAvailabilityDate();
			ValidateLCLAvailabilityDate();
			ValidateFCLStorageDate();
			ValidateLCLStorageDate();
			ValidateFCLReceivalCommences();
			ValidateLCLReceivalCommences();
			ValidateFCLCutOff();
			ValidateLCLCutOff();
		}

		CommonCartage Cartage
		{
			get { return (CommonCartage)Parent; }
		}

		protected override void CheckJJ_E3_NKJobType()
		{
			base.CheckJJ_E3_NKJobType();
			ListValidation.ErrorIfInvalidCode(Cartage.JJ_E3_NKJobTypeInfo, ResString.GetMultilingualString("1f476ae8-8ec2-4740-8ce2-c6808cce08d8", "Port Transport Job Type"));
		}

		protected override void CheckJJ_RS_NKServiceLevel()
		{
			base.CheckJJ_RS_NKServiceLevel();
			ListValidation.ErrorIfInvalidCode(Cartage.JJ_RS_NKServiceLevelInfo, ResString.GetMultilingualString("7b3ba7b1-07f2-423b-86be-c0fc9818526d", "Service Level"));
		}

		protected override void CheckJJ_F3_NKPackType()
		{
			base.CheckJJ_F3_NKPackType();
			MandatoryValidation.CheckEntered(Cartage.JJ_F3_NKPackTypeInfo, Res.GetString("86fddfc3-9e1f-4e89-ae50-bf6c6a54a9f4", "Package Type"));
			ListValidation.WarnIfInvalidCode(Cartage.JJ_F3_NKPackTypeInfo);
		}

		protected override void CheckJJ_Weight()
		{
			base.CheckJJ_Weight();

			decimal totalWeight = 0;
			string totalWeightUnit = string.Empty;

			if (!Constants.Weight.ContainsCode(Cartage.JJ_WeightUQ))
			{
				return;
			}
			else if (Cartage.IsLoose)
			{
				totalWeight = Cartage.TotalLooseBookedWeight;
				totalWeightUnit = Cartage.TotalLooseBookedWeightUnit;
			}
			else if (Cartage.IsContainerised)
			{
				totalWeight = Cartage.TotalContainerWeight;
				totalWeightUnit = Cartage.TotalContainerWeightUnit;
			}
			else
			{
				return;
			}

			if (ZArchitecture.Core.Utilities.Round(Constants.Weight.Convert(totalWeight, totalWeightUnit, Cartage.JJ_WeightUQ, applyDefaultRounding: false), 3) != ZArchitecture.Core.Utilities.Round(Cartage.JJ_Weight, 3))
			{
				Cartage.JJ_WeightInfo.AddWarning(Res.GetString("c99855c9-05ed-4275-913e-64e1868434d7", "Entered weight for Port Transport Job does not equal the total weight of packages."));
			}
		}

		protected override void CheckJJ_Volume()
		{
			base.CheckJJ_Volume();

			decimal totalVolume = 0;
			string totalVolumeUnit = string.Empty;

			if (!Constants.Volume.ContainsCode(Cartage.JJ_VolumeUQ))
			{
				return;
			}
			else if (Cartage.IsLoose)
			{
				totalVolume = Cartage.TotalLooseBookedVolume;
				totalVolumeUnit = Cartage.TotalLooseBookedVolumeUnit;
			}
			else if (Cartage.IsContainerised)
			{
				totalVolume = Cartage.TotalContainerVolume;
				totalVolumeUnit = Cartage.TotalContainerVolumeUnit;
			}
			else
			{
				return;
			}

			if (ZArchitecture.Core.Utilities.Round(Constants.Volume.Convert(totalVolume, totalVolumeUnit, Cartage.JJ_VolumeUQ, applyDefaultRounding: false), 3) != ZArchitecture.Core.Utilities.Round(Cartage.JJ_Volume, 3))
			{
				Cartage.JJ_VolumeInfo.AddWarning(Res.GetString("baefbf47-952c-45fd-93a7-a4b85e2ef1e7", "Entered volume for Port Transport Job does not equal the total volume of packages."));
			}
		}

		protected override void CheckJJ_VolumeUQ()
		{
			base.CheckJJ_VolumeUQ();
			MandatoryValidation.CheckEntered(Cartage.JJ_VolumeUQInfo, Res.GetString("55440e74-24b9-4744-9143-92a0b9785712", "Volume Unit"));
			ListValidation.ErrorIfInvalidCode(Cartage.JJ_VolumeUQInfo, ResString.GetMultilingualString("b3f49e8f-ebd9-4506-a31d-de736b475c7f", "Volume Unit"));
		}

		protected override void CheckJJ_WeightUQ()
		{
			base.CheckJJ_WeightUQ();
			MandatoryValidation.CheckEntered(Cartage.JJ_WeightUQInfo, Res.GetString("4a379a74-ac8d-45a8-ad86-62f04bbd4ee7", "Weight Unit"));
			ListValidation.ErrorIfInvalidCode(Cartage.JJ_WeightUQInfo, ResString.GetMultilingualString("da32391b-4f7e-4705-ae74-cb00b1484914", "Weight Unit"));
		}

		protected override void CheckJJ_A_JCL()
		{
			base.CheckJJ_A_JCL();
			if (Cartage.JJ_A_JCL > ZDateTime.Now)
			{
				Cartage.JJ_A_JCLInfo.AddError(Res.GetString("674c0560-cb46-4b79-ac7c-107068b2996d", "Job completion date cannot be in the future."));
			}
		}

		protected override void CheckJJ_OuterPacks()
		{
			base.CheckJJ_OuterPacks();

			if ((Cartage.IsLoose && Cartage.TotalLooseBookedPackages != Cartage.JJ_OuterPacks)
				|| ((Cartage.IsContainerised && Cartage.TotalContainerPacks != Cartage.JJ_OuterPacks)))
			{
				Cartage.JJ_OuterPacksInfo.AddWarning(Res.GetString("05e22b9a-4433-43fb-ae22-7ffd759f0750", "Entered number of packages for Port Transport Job does not equal the total number of packages."));
			}
		}

		protected override void CheckJJ_DropMode()
		{
			base.CheckJJ_DropMode();
			ListValidation.ErrorIfInvalidCode(Cartage.JJ_DropModeInfo, Cartage.Lookups.DropModes);
		}

		protected override void CheckJJ_ContainerMode()
		{
			base.CheckJJ_ContainerMode();
			ListValidation.ErrorIfInvalidCode(Cartage.JJ_ContainerModeInfo, Cartage.BindToLists.CartageContainerModes);
		}

		protected override void CheckJJ_EstimatedPickup()
		{
			base.CheckJJ_EstimatedPickup();

			if (Cartage.JJ_EstimatedPickup > Cartage.JJ_EstimatedDelivery)
			{
				Cartage.JJ_EstimatedPickupInfo.AddError(Res.GetString("279e7ab5-6bb2-43d9-9cfd-acc9fbb15b3b", "{0} must be the same or earlier than {1}.", Cartage.JJ_EstimatedPickupInfo.HumanReadableName, Cartage.JJ_EstimatedDeliveryInfo.HumanReadableName));
			}
		}

		protected override void CheckJJ_EstimatedDelivery()
		{
			base.CheckJJ_EstimatedDelivery();

			if (Cartage.JJ_EstimatedDelivery < Cartage.JJ_EstimatedPickup)
			{
				Cartage.JJ_EstimatedDeliveryInfo.AddError(Res.GetString("5a2cf09a-f76c-4398-8962-a6df4e141b5d", "{0} must be the same or later than {1}.", Cartage.JJ_EstimatedDeliveryInfo.HumanReadableName, Cartage.JJ_EstimatedPickupInfo.HumanReadableName));
			}
		}

		protected override void CheckJJ_GB()
		{
			base.CheckJJ_GB();

			MandatoryValidation.CheckEntered(Cartage.JJ_GBInfo);
		}

		public void ValidatePortOfLoading()
		{
			ValidateCalculatedProperty(Cartage.PortOfLoadingInfo);
		}

		protected virtual void CheckPortOfLoading()
		{
			SailingCheck(Cartage.PortOfLoadingInfo, Cartage.Lookups.RefUNLOCOs);
		}

		public void ValidatePortOfDischarge()
		{
			ValidateCalculatedProperty(Cartage.PortOfDischargeInfo);
		}

		protected virtual void CheckPortOfDischarge()
		{
			SailingCheck(Cartage.PortOfDischargeInfo, Cartage.Lookups.RefUNLOCOs);
		}

		public void ValidateE_DEP()
		{
			ValidateCalculatedProperty(Cartage.E_DEPInfo);
		}

		protected virtual void CheckE_DEP()
		{
			var info = Cartage.E_DEPInfo;
			SailingCheck(info);

			var sailing = Cartage.SailingStandalone;
			if (sailing != null)
			{
				CheckUserHasPermissionToEditSailingSchedule(info, sailing.Origin.JA_E_DEP);
			}

			if (!validatingDEP)
			{
				validatingDEP = true;

				if (Cartage.IsAir)
				{
					if (Cartage.E_DEP.IsValid && Cartage.E_ARV.IsValid && Cartage.E_DEP >= Cartage.E_ARV.AddDays(1))
					{
						Cartage.E_DEPInfo.AddError(Res.GetString("78cd4269-2992-43ab-bc50-1cc0c0ef6210", "ETD cannot be more than a day after ETA."));
					}
				}
				else
				{
					if (Cartage.E_DEP.IsValid && Cartage.E_ARV.IsValid && Cartage.E_DEP > Cartage.E_ARV)
					{
						Cartage.E_DEPInfo.AddError(Res.GetString("67f21f4f-782d-4b89-a27d-4b072e059ea3", "ETD cannot be after ETA."));
					}
				}

				if (Cartage.E_ARVInfo.HasNotifications())
				{
					ValidateE_ARV();
				}

				validatingDEP = false;
			}
		}

		bool validatingDEP;

		public void ValidateE_ARV()
		{
			ValidateCalculatedProperty(Cartage.E_ARVInfo);
		}

		protected virtual void CheckE_ARV()
		{
			var info = Cartage.E_ARVInfo;
			SailingCheck(info);

			var sailing = Cartage.SailingStandalone;
			if (sailing != null)
			{
				CheckUserHasPermissionToEditSailingSchedule(info, sailing.Destination.JB_E_ARV);
			}

			if (!validatingARV)
			{
				validatingARV = true;

				if (Cartage.IsAir)
				{
					if (Cartage.E_DEP.IsValid && Cartage.E_ARV.IsValid && Cartage.E_ARV <= Cartage.E_DEP.AddDays(-1))
					{
						Cartage.E_ARVInfo.AddError(Res.GetString("1b79674a-9736-4295-8f1e-2659e57e9cf9", "ETA cannot be more than a day before ETD."));
					}
				}
				else
				{
					if (Cartage.E_DEP.IsValid && Cartage.E_ARV.IsValid && Cartage.E_ARV < Cartage.E_DEP)
					{
						Cartage.E_ARVInfo.AddError(Res.GetString("d9dd2803-4252-4948-bf51-46e1ced613cf", "ETA cannot be before ETD."));
					}
				}
				if (Cartage.E_DEPInfo.HasNotifications())
				{
					ValidateE_DEP();
				}

				validatingARV = false;
			}
		}

		bool validatingARV;

		public void ValidateVessel()
		{
			ValidateCalculatedProperty(Cartage.VesselInfo);
		}

		protected virtual void CheckVessel()
		{
			if (!Cartage.IsRail)
			{
				SailingCheck(Cartage.VesselInfo, Cartage.Lookups.Vessels);
			}
		}

		public void ValidateVoyageFlight()
		{
			ValidateCalculatedProperty(Cartage.VoyageFlightInfo);
		}

		protected virtual void CheckVoyageFlight()
		{
			SailingCheck(Cartage.VoyageFlightInfo);
		}

		public void ValidateLCLAvailabilityDate()
		{
			ValidateCalculatedProperty(Cartage.LCLAvailabilityDateInfo);
		}

		protected virtual void CheckLCLAvailabilityDate()
		{
			var info = Cartage.LCLAvailabilityDateInfo;
			SailingCheck(info);

			var sailing = Cartage.SailingStandalone;
			if (sailing != null)
			{
				CheckUserHasPermissionToEditSailingSchedule(info, (ZDateTime)sailing.JX_DepotAvailabilityDateInfo.OriginalValue);
			}
		}

		public void ValidateFCLStorageDate()
		{
			ValidateCalculatedProperty(Cartage.FCLStorageDateInfo);
		}

		protected virtual void CheckFCLStorageDate()
		{
			var info = Cartage.FCLStorageDateInfo;
			SailingCheck(info);

			var sailing = Cartage.SailingStandalone;
			if (sailing != null)
			{
				CheckUserHasPermissionToEditSailingSchedule(info, (ZDateTime)sailing.Destination.JB_StorageDateInfo.OriginalValue);
			}
		}

		public void ValidateLCLStorageDate()
		{
			ValidateCalculatedProperty(Cartage.LCLStorageDateInfo);
		}

		protected virtual void CheckLCLStorageDate()
		{
			var info = Cartage.LCLStorageDateInfo;
			SailingCheck(info);

			var sailing = Cartage.SailingStandalone;
			if (sailing != null)
			{
				CheckUserHasPermissionToEditSailingSchedule(info, (ZDateTime)sailing.JX_DepotStorageDateInfo.OriginalValue);
			}
		}

		public void ValidateFCLAvailabilityDate()
		{
			ValidateCalculatedProperty(Cartage.FCLAvailabilityDateInfo);
		}

		protected virtual void CheckFCLAvailabilityDate()
		{
			var info = Cartage.FCLAvailabilityDateInfo;
			SailingCheck(info);

			var sailing = Cartage.SailingStandalone;
			if (sailing != null)
			{
				CheckUserHasPermissionToEditSailingSchedule(info, (ZDateTime)sailing.Destination.JB_AvailabilityDateInfo.OriginalValue);
			}
		}

		public void ValidateFCLReceivalCommences()
		{
			ValidateCalculatedProperty(Cartage.FCLReceivalCommencesInfo);
		}

		protected virtual void CheckFCLReceivalCommences()
		{
			var info = Cartage.FCLReceivalCommencesInfo;
			SailingCheck(info);

			var sailing = Cartage.SailingStandalone;
			if (sailing != null)
			{
				CheckUserHasPermissionToEditSailingSchedule(info, (ZDateTime)sailing.Origin.JA_ReceivalCommencesInfo.OriginalValue);
			}
		}

		public void ValidateLCLCutOff()
		{
			ValidateCalculatedProperty(Cartage.LCLCutOffInfo);
		}

		protected virtual void CheckLCLCutOff()
		{
			var info = Cartage.LCLCutOffInfo;
			SailingCheck(info);

			var sailing = Cartage.SailingStandalone;
			if (sailing != null)
			{
				CheckUserHasPermissionToEditSailingSchedule(info, (ZDateTime)sailing.JX_DepotCutOffInfo.OriginalValue);
			}
		}

		public void ValidateFCLCutOff()
		{
			ValidateCalculatedProperty(Cartage.FCLCutOffInfo);
		}

		protected virtual void CheckFCLCutOff()
		{
			var info = Cartage.FCLCutOffInfo;
			SailingCheck(info);

			var sailing = Cartage.SailingStandalone;
			if (sailing != null)
			{
				CheckUserHasPermissionToEditSailingSchedule(info, (ZDateTime)sailing.Origin.JA_CutOffInfo.OriginalValue);
			}
		}

		public void ValidateLCLReceivalCommences()
		{
			ValidateCalculatedProperty(Cartage.LCLReceivalCommencesInfo);
		}

		protected virtual void CheckLCLReceivalCommences()
		{
			var info = Cartage.LCLReceivalCommencesInfo;
			SailingCheck(info);

			var sailing = Cartage.SailingStandalone;
			if (sailing != null)
			{
				CheckUserHasPermissionToEditSailingSchedule(info, (ZDateTime)sailing.JX_DepotReceivalCommencesInfo.OriginalValue);
			}
		}

		internal ZString NonPersistantSailingFieldsErrorText
		{
			get
			{
				return Res.GetString("6e9345af-e55a-4444-9118-d3740ea16db3", "This field will not be saved without having") + " " +
				(Cartage.IsSea || Cartage.IsRail ? Res.GetString("679750dd-6529-4e03-a78f-a80e860030ea", "Vessel, Voyage, Port of Loading and Port of Discharge.") :
				Res.GetString("a6bc5c72-eb3a-4695-94cb-737dda1136df", "Flight, Port of Loading, Port of Discharge and") + " " + (Cartage.IsImportOrDestination ? Res.GetString("722a58fd-62e3-4876-b29f-e0e227c77d34", "ETA.") : Res.GetString("7c3e59e5-32e8-4e46-bba5-411442e03fa9", "ETD.")));
			}
		}

		void SailingCheck(ZPropertyInfo info, IBusinessObjectCollection lookups)
		{
			SailingCheck(info);

			if (!Cartage.HasParent) // until LT supports Road & Rail journies
			{
				ListValidation.ErrorIfInvalidCode(info, lookups);
			}
		}

		void SailingCheck(ZPropertyInfo info)
		{
			if (!Cartage.HasParent && AreAnySailingFieldsEntered)
			{
				if (AreAllMandatorySailingFieldsSpecified && MandatorySailingFields.Contains(info))
				{
					MandatoryValidation.CheckEntered(info);
				}
				else if (!info.Value.IsEmpty && !AreAllMandatorySailingFieldsSpecified)
				{
					info.AddError(NonPersistantSailingFieldsErrorText);
				}

				bool allScheduleDetailsEnteredButScheduleInError = !info.Value.IsEmpty && !info.HasErrors() && AreAllMandatorySailingFieldsSpecified && Cartage.SailingStandalone == null;
				if (allScheduleDetailsEnteredButScheduleInError)
				{
					info.AddError(Res.GetString("d3bad621-e533-4ece-9bcf-d5f90735fc59", "This field will not be saved because some of the schedule details are not valid."));
				}
			}

			if (info is ZPropertyInfoDateTime)
			{
				TypeValidation.CheckValidZDateTimeAndRange(info);
			}
		}

		void CheckUserHasPermissionToEditSailingSchedule(ZPropertyInfo cartageDateInfo, ZDateTime scheduleEditDate)
		{
			if ((ZDateTime)cartageDateInfo.Value != scheduleEditDate && !Env.Security.SailingScheduleEdit.IsAllowed)
			{
				cartageDateInfo.AddError(Res.GetString("8777ffd9-5136-44ce-91c7-63f2ec6240af",
					"The change you are making would affect an existing Sailing Schedule. You do not have rights to edit Sailing Schedules. If you require such rights please ask your supervisor to grant the right at Operate > Schedules > Sailing Schedule > Edit."));
			}
		}

		bool AreAllMandatorySailingFieldsSpecified
		{
			get
			{
				foreach (ZPropertyInfo info in MandatorySailingFields)
				{
					if (info.Value.IsEmpty)
					{
						return false;
					}
				}
				return true;
			}
		}

		List<ZPropertyInfo> MandatorySailingFields
		{
			get
			{
				List<ZPropertyInfo> list = new List<ZPropertyInfo>();
				list.Add(Cartage.PortOfLoadingInfo);
				list.Add(Cartage.PortOfDischargeInfo);
				list.Add(Cartage.VoyageFlightInfo);

				if (Cartage.IsSea || Cartage.IsRail)
				{
					list.Add(Cartage.VesselInfo);
				}
				else
				{
					ZPropertyInfo dateInfo = Cartage.IsImportOrDestination ? Cartage.E_ARVInfo : Cartage.E_DEPInfo;
					list.Add(dateInfo);
				}

				return list;
			}
		}

		bool AreAnySailingFieldsEntered
		{
			get
			{
				return !Cartage.E_DEP.IsEmpty ||
					!Cartage.PortOfLoading.IsEmpty ||
					!Cartage.E_ARV.IsEmpty ||
					!Cartage.PortOfDischarge.IsEmpty ||
					!Cartage.Vessel.IsEmpty ||
					!Cartage.VoyageFlight.IsEmpty ||
					!Cartage.FCLCutOff.IsEmpty ||
					!Cartage.LCLCutOff.IsEmpty ||
					!Cartage.FCLReceivalCommences.IsEmpty ||
					!Cartage.LCLReceivalCommences.IsEmpty ||
					!Cartage.FCLAvailabilityDate.IsEmpty ||
					!Cartage.LCLAvailabilityDate.IsEmpty ||
					!Cartage.FCLStorageDate.IsEmpty ||
					!Cartage.LCLStorageDate.IsEmpty;
			}
		}
	}
}
