using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Freight.Business
{
	public class SailingTemplateCopyCriteria : NonPersistentBusinessObject, IObsoleteValidation
	{
		#region Schema

		public static class Schema
		{
			public const string ReferencePort = "ReferencePort";
			public const string ReferencePortDate = "ReferencePortDate";
			public const string ReferencePortLabel = "ReferencePortLabel";
			public const string AdjustedDate = "AdjustedDate";
			public const string RetainVessel = "RetainVessel";
		}

		#endregion

		public SailingTemplateCopyCriteria(JobVoyage voyageToCopy)
			: base(voyageToCopy.Factory)
		{
			this.voyageToCopy = voyageToCopy;

			VoyageOrigin referenceOrigin = FindReferenceOrigin(voyageToCopy);
			if (referenceOrigin != null)
			{
				referencePort = referenceOrigin.JA_RL_NKPortOfLoading;
				referencePortDate = referenceOrigin.JA_E_DEP;
				referencePortLabel = Res.GetString("8740fd9f-9dcf-46b9-9a8d-bef6bf7e21f8", "Departs:");
			}
			else
			{
				VoyageDestination referenceDestination = FindReferenceDestination(voyageToCopy);
				if (referenceDestination != null)
				{
					referencePort = referenceDestination.JB_RL_NKPortOfDischarge;
					referencePortDate = referenceDestination.JB_E_ARV;
					referencePortLabel = Res.GetString("e399c6cb-a6e4-45ba-8d6f-1d58b17f8a91", "Arrives:");
				}
			}
		}

		#region GenerateCopy

		public JobVoyage GenerateCopy()
		{
			JobVoyage newVoyage = (JobVoyage)voyageToCopy.Clone();
			ProcessVoyage(newVoyage);
			return newVoyage;
		}

		#endregion

		#region Properties

		public ZString ReferencePort
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return referencePort; }
		}

		public ZPropertyInfo ReferencePortInfo
		{
			get { return GetZPropertyInfo(Schema.ReferencePort); }
		}

		public ZDateTime ReferencePortDate
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return referencePortDate; }
		}

		public ZPropertyInfo ReferencePortDateInfo
		{
			get { return GetZPropertyInfo(Schema.ReferencePortDate); }
		}

		public ZString ReferencePortLabel
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return referencePortLabel; }
		}

		public ZPropertyInfo ReferencePortLabelInfo
		{
			get { return GetZPropertyInfo(Schema.ReferencePortLabel); }
		}

		public ZDateTime AdjustedDate
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return adjustedDate; }
			set
			{
				SetNonPersistentPropertyValue(AdjustedDateInfo, ref adjustedDate, value);
				AdjustedDateInfo.RefreshBinding();

				if (!IsValidationSuspended)
				{
					ValidateAdjustedDate();
				}
			}
		}

		public ZPropertyInfo AdjustedDateInfo
		{
			get { return GetZPropertyInfo(Schema.AdjustedDate); }
		}

		public ZBool RetainVessel
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return retainVessel; }
			set { SetNonPersistentPropertyValue(RetainVesselInfo, ref retainVessel, value); }
		}

		public ZPropertyInfo RetainVesselInfo
		{
			get { return GetZPropertyInfo(Schema.RetainVessel); }
		}

		#endregion

		#region Validation

		public void ValidateAdjustedDate()
		{
			AdjustedDateInfo.ClearAllNotifications();
			MandatoryValidation.CheckEntered(AdjustedDateInfo);
			TypeValidation.CheckValidZDateTimeAndRange(AdjustedDateInfo);
		}

		protected override void RunPreSaveValidationCore()
		{
			base.RunPreSaveValidationCore();
			ValidateAdjustedDate();
		}

		#endregion

		#region Implementation

		int AddedDays
		{
			get
			{
				int result = 0;

				if (ReferencePortDate.IsValid && AdjustedDate.IsValid)
				{
					result = (AdjustedDate.Date - ReferencePortDate.Date).Days;
				}

				return result;
			}
		}

		void ProcessVoyage(JobVoyage voyage)
		{
			using (voyage.GetValidationSuspender())
			{
				voyage.JV_VoyageFlight = ZString.Empty;
				voyage.JV_FlightDate = ZDateTime.Empty;

				if (!RetainVessel)
				{
					voyage.JV_RV_NKVessel = ZString.Empty;
					voyage.JV_OH_Line = ZGuid.Empty;
					voyage.Countries.RemoveAndDeleteAll();
				}

				foreach (VoyageOrigin origin in voyage.Origins)
				{
					ProcessOrigin(origin);
				}

				foreach (VoyageDestination destination in voyage.Destinations)
				{
					ProcessDestination(destination);
				}

				foreach (JobSailing sailing in voyage.Sailings)
				{
					ProcessSailing(sailing);
				}
			}
		}

		void ProcessOrigin(VoyageOrigin origin)
		{
			using (origin.GetValidationSuspender())
			{
				int daysToAdd = AddedDays;

				AddDays(origin.JA_E_DEPInfo, daysToAdd);
				origin.JA_A_DEP = ZDateTime.Empty;

				AddDays(origin.JA_ReceivalCommencesInfo, daysToAdd);
				AddDays(origin.JA_CutOffInfo, daysToAdd);
				AddDays(origin.JA_DGReceivalCommencesInfo, daysToAdd);
				AddDays(origin.JA_DGCutOffInfo, daysToAdd);
				AddDays(origin.JA_DocumentaryCutoffInfo, daysToAdd);
				AddDays(origin.JA_VGMCutOffInfo, daysToAdd);

				if (!RetainVessel)
				{
					origin.SlotAllocations.RemoveAndDeleteAll();
				}
			}
		}

		void ProcessDestination(VoyageDestination destination)
		{
			using (destination.GetValidationSuspender())
			{
				int daysToAdd = AddedDays;

				AddDays(destination.JB_E_ARVInfo, daysToAdd);

				destination.JB_A_ARV = ZDateTime.Empty;

				AddDays(destination.JB_AvailabilityDateInfo, daysToAdd);
				AddDays(destination.JB_StorageDateInfo, daysToAdd);
			}
		}

		void ProcessSailing(JobSailing sailing)
		{
			using (sailing.GetValidationSuspender())
			{
				sailing.JX_ReservedMasterBill = ZString.Empty;
				foreach (ZPropertyInfo info in sailing.ZPropertyInfoHash)
				{
					if (info.HasSetter && info.PropertyType == typeof(ZDateTime))
					{
						info.Value = ZDateTime.Empty;
					}
				}

				if (!RetainVessel)
				{
					sailing.SlotAllocations.RemoveAndDeleteAll();
				}
			}
		}

		void AddDays(ZPropertyInfo info, int daysToAdd)
		{
			if (!info.Value.IsEmpty)
			{
				ZDate dateToBeAdded = ((ZDateTime)info.Value).Date;
				info.Value = (ZDateTime)dateToBeAdded.AddDays(daysToAdd);
			}
		}

		VoyageOrigin FindReferenceOrigin(JobVoyage voyage)
		{
			VoyageOrigin result = null;

			foreach (VoyageOrigin origin in voyage.Origins)
			{
				if (origin.JA_E_DEP.IsValid && (result == null || result.JA_E_DEP > origin.JA_E_DEP))
				{
					result = origin;
				}
			}

			return result;
		}

		VoyageDestination FindReferenceDestination(JobVoyage voyage)
		{
			VoyageDestination result = null;

			foreach (VoyageDestination destination in voyage.Destinations)
			{
				if (destination.JB_E_ARV.IsValid && (result == null || result.JB_E_ARV > destination.JB_E_ARV))
				{
					result = destination;
				}
			}

			return result;
		}

		readonly JobVoyage voyageToCopy;
		readonly ZString referencePort;
		readonly ZDateTime referencePortDate;
		readonly ZString referencePortLabel;
		ZDateTime adjustedDate;
		ZBool retainVessel;

		#endregion
	}
}
