using Enterprise.Customs.Business;
using Enterprise.Customs.Common;

namespace Enterprise.Customs.US.Business
{
	public static class ChargeValidationExtension
	{
		/// <summary>
		/// This validates and adds a message error if a charge with 'Adjusted' ticked does not have another charge it is adjusting
		/// </summary>
		public static void ValidateAdjustedChargeHasAnotherChargeToAdjust(this BaseJobComInvHeaderCharge charge)
		{
			if (charge.J7_AdjustedCharge)
			{
				JobComInvCharge[] anotherCharges = charge.GetSameChargeWithDifferentAdjustedFlag();

				if (anotherCharges.Length > 0)
				{
					bool hasDutiableCharge = false;
					foreach (BaseJobComInvHeaderCharge anotherCharge in anotherCharges)
					{
						hasDutiableCharge = anotherCharge.J7_IsDutiable;
						if (hasDutiableCharge)
						{
							break;
						}
					}

					if (!hasDutiableCharge)
					{
						charge.J7_AdjustedChargeInfo.AddMessageError(CorrespondingChargeNotDutiable);
					}
				}
				else
				{
					JobDeclaration declaration = charge.Parent != null ? (JobDeclaration)charge.Parent.JobDeclaration : null;

					if (declaration != null && !declaration.ApportionmentDirty)
					{
						charge.J7_AdjustedChargeInfo.AddMessageError(NoCorrespondingChargeExistForAdjustedCharge);
					}
				}
			}
		}

		public const string NoCorrespondingChargeExistForAdjustedCharge = "You have indicated this is an override of another charge, but there is no other charge having the same set of 'Included in Lines' and 'Included in Invoice' flags.";
		public const string CorrespondingChargeNotDutiable = "You have indicated this is an override of another charge. However another charge has not been indicated as 'Dutiable'.";
	}
}
