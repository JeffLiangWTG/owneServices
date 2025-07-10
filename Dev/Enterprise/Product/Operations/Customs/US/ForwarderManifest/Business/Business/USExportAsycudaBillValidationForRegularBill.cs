using CargoWise.Types;
using Enterprise.Customs.ASYCUDA.Business;

namespace Enterprise.Customs.US.ForwarderManifest.Business
{
	public class USExportAsycudaBillValidationForRegularBill : AsycudaBillValidationForRegularBill
	{
		public USExportAsycudaBillValidationForRegularBill(USExportAsycudaBill parent) : base(parent)
		{
		}

		protected override ZBool NeedsToShowABL_ManifestUQNotMappedMessageError => false;

		public override void ValidateAll()
		{
			base.ValidateAll();
			ValidateAESITNNumbers();
			ValidateInBondNumbers();
		}

		public void ValidateAESITNNumbers()
		{
			ValidateCalculatedProperty(((USExportAsycudaBill)Parent).AESITNNumbersInfo);
		}

		public void ValidateInBondNumbers()
		{
			ValidateCalculatedProperty(((USExportAsycudaBill)Parent).InBondNumbersInfo);
		}

		protected void CheckAESITNNumbers()
		{
			if (Parent is USExportAsycudaBill parent)
			{
				USExportAsycudaBillValidationHelper.AddNotificationFromCusEntryNumber(parent.AESITNNumberCollection, parent.AESITNNumbersInfo);
				USExportAsycudaBillValidationHelper.CheckITNAndExemptionCodeAndInBondNumber(parent, parent.AESITNNumbersInfo);
			}
		}

		protected void CheckInBondNumbers()
		{
			if (Parent is USExportAsycudaBill parent)
			{
				USExportAsycudaBillValidationHelper.AddNotificationFromCusEntryNumber(parent.InBondNumberCollection, parent.InBondNumbersInfo);
				USExportAsycudaBillValidationHelper.CheckITNAndExemptionCodeAndInBondNumber(parent, parent.InBondNumbersInfo);
			}
		}

		protected override void CheckABL_UCRNumber()
		{
			base.CheckABL_UCRNumber();
			if (Parent is USExportAsycudaBill parent)
			{
				USExportAsycudaBillValidationHelper.CheckITNAndExemptionCodeAndInBondNumber(parent, parent.ABL_UCRNumberInfo);
			}
		}
	}
}
