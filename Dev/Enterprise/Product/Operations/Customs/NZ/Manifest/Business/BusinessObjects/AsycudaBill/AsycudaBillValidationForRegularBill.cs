using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.NZ.Manifest.Business
{
	public partial class AsycudaBillValidationForRegularBill : ASYCUDA.Business.AsycudaBillValidationForRegularBill
	{
		public AsycudaBillValidationForRegularBill(AsycudaBill parent)
			: base(parent)
		{
		}

		protected new AsycudaBill Parent => (AsycudaBill)base.Parent;
		protected AsycudaManifestHeader Header => Parent.Header;

		protected override ZBool NeedsToCheckABL_Consignee => false;
		protected override ZBool NeedsToCheckABL_GrossWeight => false;
		protected override ZBool NeedsToCheckABL_GrossWeightUQ => false;
		protected override ZBool NeedsToCheckABL_ManifestQty => false;
		protected override ZBool NeedsToCheckABL_ManifestUQ => false;

		protected override void CheckABL_OA_Consignee()
		{
		}

		protected override void CheckABL_ConsigneeName()
		{
		}

		protected override void CheckCustomsEntryNumber()
		{
			base.CheckCustomsEntryNumber();
			if (Parent.IsOCR && Parent.CustomsEntryNumber.IsEmpty)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.CustomsEntryNumberInfo);
			}
		}
	}
}
