using CargoWise.EntityFramework;

namespace Enterprise.Customs.TR.ETrade.Business
{
	public class AsycudaBillValidationForMasterChild : ASYCUDA.Business.AsycudaBillValidationForMasterChild
	{
		public AsycudaBillValidationForMasterChild(AsycudaBill parent)
			: base(parent)
		{
		}

		protected override bool ShouldCheckHasAsycudaCountry => false;

		protected override void CheckABL_Procedure()
		{
			base.CheckABL_Procedure();
			ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.ABL_ProcedureInfo);
		}

		protected override void CheckABL_GoodsDescription()
		{
			base.CheckABL_GoodsDescription();
			MandatoryValidation.MessageErrorIfNotEntered(Parent.ABL_GoodsDescriptionInfo);
		}
	}
}
