using CargoWise.EntityFramework;

namespace Enterprise.Customs.TW.Manifest.Business
{
	public class AsycudaBillValidationForMasterChild : ASYCUDA.Business.AsycudaBillValidation
	{
		public AsycudaBillValidationForMasterChild(AsycudaBill parent)
			: base(parent)
		{
		}

		protected override void CheckABL_BillNumber()
		{
			base.CheckABL_BillNumber();
			MandatoryValidation.MessageErrorIfNotEntered(Parent.ABL_BillNumberInfo);
		}

		protected override void CheckABL_GoodsLocation()
		{
			base.CheckABL_GoodsLocation();
			var targetInfo = Parent.ABL_GoodsLocationInfo;
			MandatoryValidation.MessageErrorIfNotEntered(targetInfo);
			ListValidation.MessageErrorIfInvalidCode(targetInfo);
		}

		protected override void CheckABL_E_ARV()
		{
			base.CheckABL_E_ARV();
			if (Parent.Header?.IsAir ?? false)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.ABL_E_ARVInfo);
			}
		}
	}
}
