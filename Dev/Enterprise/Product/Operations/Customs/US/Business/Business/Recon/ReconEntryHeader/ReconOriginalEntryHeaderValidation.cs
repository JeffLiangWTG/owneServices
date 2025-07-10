using System;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.US.Business
{
	public class ReconOriginalEntryHeaderValidation : ZValidation
	{
		public ReconOriginalEntryHeaderValidation(ReconOriginalEntryHeader reconEntry)
			: base(reconEntry)
		{
			this.reconEntry = reconEntry;
		}
		readonly ReconOriginalEntryHeader reconEntry;

		public override void ValidateAll()
		{
			ValidateMPC();
		}

		public void ValidateMPC()
		{
			ValidateCalculatedProperty(reconEntry.MPCInfo);
		}
		internal const string EnterNumberGreaterThanZero = "Please enter an MPF as calculated and unadjusted for MPF calculation. This is required as this entry has only changed lines entered.";

		protected void CheckMPC()
		{
			if (reconEntry.US_R_ChangedLinesOnly &&
				reconEntry.OriginalCharges.GetAmount(Core.Constants.USCustoms.FeeCodes.MerchandiseProcessing) > 0 &&
				reconEntry.MPC <= 0)
			{
				reconEntry.MPCInfo.AddMessageError(EnterNumberGreaterThanZero);
			}
		}

		public override Type AutoValidationType
		{
			get { return typeof(ReconOriginalEntryHeaderValidation); }
		}
	}
}
