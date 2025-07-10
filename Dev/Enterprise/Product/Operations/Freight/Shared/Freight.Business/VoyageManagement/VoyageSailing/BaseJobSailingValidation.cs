using Enterprise.Freight.CarbonEmissions.Business;
using Enterprise.Freight.Common.Business;

namespace Enterprise.Freight.Business
{
	public class BaseJobSailingValidation : JobSailingValidation
	{
		public BaseJobSailingValidation(JobSailing parent)
			: base(parent)
		{
		}

		public new JobSailing Parent
		{
			get { return (JobSailing)base.Parent; }
		}

		#region JX_DepotCutOff

		protected override void CheckJX_DepotCutOff()
		{
			base.CheckJX_DepotCutOff();

			if (Parent.Origin != null)
			{
				if (!Parent.JX_DepotCutOff.IsEmpty && !Parent.Origin.JA_CutOff.IsEmpty)
				{
					if (Parent.Origin.JA_CutOff < Parent.JX_DepotCutOff)
					{
						Parent.JX_DepotCutOffInfo.AddError(Res.GetString("4d8b4b41-4aec-4842-aa91-5d18d650815d", "CFS Cut off date cannot be after the CTO cut off date."));
					}
				}

				if (!Parent.JX_DepotCutOff.IsEmpty && !Parent.Origin.JA_E_DEP.IsEmpty)
				{
					if (Parent.JX_DepotCutOff.Date > Parent.Origin.JA_E_DEP.Date)
					{
						Parent.JX_DepotCutOffInfo.AddError(Res.GetString("58310e48-324e-426e-9faf-3a9f5ccbd7f4", "CFS Cut Off date cannot be after ETD."));
					}
				}
			}
		}

		#endregion

		#region JX_DepotReceivalCommences

		protected override void CheckJX_DepotReceivalCommences()
		{
			base.CheckJX_DepotReceivalCommences();

			if (Parent.JX_DepotReceivalCommences > Parent.JX_DepotCutOff)
			{
				Parent.JX_DepotReceivalCommencesInfo.AddError(Res.GetString("5072ca13-5dc1-43b1-be73-01c7d31956cf", "CFS Receival Start date must be before the CFS Cut Off date."));
			}

			if (Parent.Origin != null)
			{
				if (!Parent.JX_DepotReceivalCommences.IsEmpty && !Parent.Origin.JA_E_DEP.IsEmpty)
				{
					if (Parent.JX_DepotReceivalCommences > Parent.Origin.JA_E_DEP)
					{
						Parent.JX_DepotReceivalCommencesInfo.AddError(Res.GetString("1cb331a7-bd1a-4bf4-96cb-e0d88fb78f75", "CFS Receival Start date cannot be after ETD."));
					}
				}
			}
		}

		#endregion

		#region JX_DepotStorageDate

		protected override void CheckJX_DepotStorageDate()
		{
			base.CheckJX_DepotStorageDate();

			if (!Parent.JX_DepotAvailabilityDate.IsEmpty && !Parent.JX_DepotStorageDate.IsEmpty)
			{
				if (Parent.JX_DepotStorageDate < Parent.JX_DepotAvailabilityDate)
				{
					Parent.JX_DepotStorageDateInfo.AddError(Res.GetString("c6509e1e-2ce4-4efd-91ce-c08c3318ef01", "CFS Storage Date must be after CFS Availability Date"));
				}
			}
		}

		#endregion

		#region JX_OnlineScheduleStatus

		protected override void CheckJX_OnlineScheduleStatus()
		{
			base.CheckJX_OnlineScheduleStatus();

			foreach (var warning in OnlineFlightMatchingValidationHelper.GetOnlineFlightMatchStatusWarnings(Parent))
			{
				Parent.JX_OnlineScheduleStatusInfo.AddWarning(warning);
			}
		}

		#endregion

		#region CO2ePerTonneInKg

		public void ValidateCO2ePerTonneInKgForBinding()
		{
			ValidateCalculatedProperty(Parent.CO2ePerTonneInKgForBindingInfo);
		}

		protected void CheckCO2ePerTonneInKgForBinding()
		{
			Parent.CheckCO2eStatus(Parent.CO2ePerTonneInKgForBindingInfo);
		}

		#endregion
	}
}
