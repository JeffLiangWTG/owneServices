using CargoWise.EntityFramework;

namespace Enterprise.Customs.PE.Manifest.Business
{
	public class AsycudaBillValidationForMasterChild : ASYCUDA.Business.AsycudaBillValidationForMasterChild
	{
		public AsycudaBillValidationForMasterChild(ASYCUDA.Business.AsycudaBill parent) : base(parent)
		{
		}

		protected override void CheckABL_BillIssueDate()
		{
			base.CheckABL_BillIssueDate();
			MandatoryValidation.MessageErrorIfNotEntered(Parent.ABL_BillIssueDateInfo);
		}
	}
}
