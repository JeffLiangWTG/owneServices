using CargoWise.EntityFramework;
using Enterprise.Customs.Common;

namespace Enterprise.Customs.Business
{
	public class JobComInvChargeCloneHelper
	{
		public void CopyCharges(IChargeHolder bizObjToClone, IChargeHolder clonedResult, CloneType cloneType)
		{
			BusinessObjectCloneArgs chargeArgs = null;

			foreach (BaseJobComInvHeaderCharge charge in bizObjToClone.Charges)
			{
				if (chargeArgs == null)
				{
					chargeArgs = CustomsBusinessObjectCloneArgs.GetCloneArgs(cloneType, charge.GetType(), clonedResult.Factory);
				}

				BaseJobComInvHeaderCharge clonedCharge = (BaseJobComInvHeaderCharge)new BusinessObjectCloneStrategy(charge).Clone(chargeArgs);

				using (clonedCharge.GetValidationSuspender())
				using (clonedCharge.SuspendSettingHasChanges())
				{
					clonedCharge.Parent = (ICommonInvoice)clonedResult;

					clonedResult.Charges.Add(clonedCharge);
				}
			}
		}
	}
}
