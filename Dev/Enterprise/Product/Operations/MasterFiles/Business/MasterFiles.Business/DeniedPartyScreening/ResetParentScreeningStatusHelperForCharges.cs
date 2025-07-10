using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Integration;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.MasterFiles.Business
{
	public class ResetParentScreeningStatusHelperForCharges : IResetParentScreeningStatusHelperForCharges
	{
		public void ResetJobParentScreeningStatus(bool hasChanges, IJobHeaderParent jobParent, IEnumerable<JobCharge> charges)
		{
			if (jobParent == null || charges == null || !charges.Any())
			{
				return;
			}

			if (hasChanges
				&& jobParent is Forwarding.IForwardingShipment shipment
				&& shipment is IShouldUpdateScreeningStatus parentProvider
				&& jobParent is IScreeningStatusProvider screeningProvider
				&& !parentProvider.ShouldUpdateScreeningStatus)
			{
				var changedCharges = charges.Where(charge => ChargeWithOrgDeleting(charge) || ChargeWithOrgUpdated(charge) || ChargeWithOrgAdded(charge)).ToArray();
				if (changedCharges.Length > 0)
				{
					if (screeningProvider.ScreeningStatus == ScreeningStatusesList.Codes.JobCleared)
					{
						var clearStatus = new[]
						{
							ScreeningStatusesList.Codes.Clear,
							ScreeningStatusesList.Codes.PermanentClear,
							ScreeningStatusesList.Codes.JobCleared
						};
						foreach (var changedCharge in changedCharges)
						{
							if (SetParentShouldUpdateScreeningStatus(changedCharge, parentProvider, clearStatus))
							{
								return;
							}
						}
					}
					else
					{
						parentProvider.ShouldUpdateScreeningStatus = true;
					}
				}
			}
		}

		bool ChargeWithOrgDeleting(JobCharge charge)
		{
			return charge.IsDeleting && (!charge.JR_OH_CostAccount.IsEmpty || !charge.JR_OH_SellAccount.IsEmpty);
		}

		bool ChargeWithOrgUpdated(JobCharge charge)
		{
			return !charge.IsDeleted &&
				   (charge.JR_OH_CostAccountInfo.HasChanges || charge.JR_OH_SellAccountInfo.HasChanges);
		}

		bool ChargeWithOrgAdded(JobCharge charge)
		{
			return !charge.IsDeleted &&
				   !charge.IsInDatabase &&
				   !(charge.JR_OH_CostAccount.IsEmpty && charge.JR_OH_SellAccount.IsEmpty);
		}

		bool SetParentShouldUpdateScreeningStatus(JobCharge charge, IShouldUpdateScreeningStatus screeningProvider,
			string[] exceptedStatus)
		{
			var result = false;
			if (charge != null && !charge.IsDeleted)
			{
				if (charge.CostAccount != null && !charge.CostAccount.IsDeleted)
				{
					result = TryChangeParentShouldUpdateScreeningStatus(charge.CostAccount, screeningProvider, exceptedStatus);
				}

				if (!result && charge.SellAccount != null && !charge.SellAccount.IsDeleted)
				{
					result = TryChangeParentShouldUpdateScreeningStatus(charge.SellAccount, screeningProvider, exceptedStatus);
				}
			}

			return result;
		}

		bool TryChangeParentShouldUpdateScreeningStatus(BusinessObject bizo, IShouldUpdateScreeningStatus parentProvider, string[] exceptedStatus)
		{
			var result = false;
			if (bizo is IScreeningStatusProvider screeningStatusProvider && exceptedStatus.All(o => o != screeningStatusProvider.ScreeningStatus))
			{
				parentProvider.ShouldUpdateScreeningStatus = true;
				result = true;
			}

			return result;
		}
	}
}
