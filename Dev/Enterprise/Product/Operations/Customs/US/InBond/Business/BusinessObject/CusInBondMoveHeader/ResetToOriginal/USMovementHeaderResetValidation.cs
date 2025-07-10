using System.Linq;

namespace Enterprise.Customs.US.InBond.Business
{
	public class USMovementHeaderResetValidation : AutoUSMovementHeaderResetValidation
	{
		public USMovementHeaderResetValidation(AutoUSMovementHeaderReset parent)
			: base(parent)
		{
		}

		protected new USMovementHeaderReset Parent
		{
			get { return (USMovementHeaderReset)base.Parent; }
		}

		protected override void CheckRO_ResetReason()
		{
			base.CheckRO_ResetReason();

			if (Parent.RO_ResetToOriginal && Parent.RO_ResetReason.IsEmpty)
			{
				Parent.RO_ResetReasonInfo.AddError(enterResettingReason);
			}
		}

		protected override void CheckRO_ResetToOriginal()
		{
			base.CheckRO_ResetToOriginal();
			if (Parent.Coll.All(x => !((USMovementHeaderReset)x).RO_ResetToOriginal))
			{
				Parent.RO_ResetToOriginalInfo.AddError(selectMovementHeader);
			}
		}

		public const string selectMovementHeader = "Please select at least one movement header to reset.";
		public const string enterResettingReason = "Please enter a reason for resetting to original.";
	}
}
