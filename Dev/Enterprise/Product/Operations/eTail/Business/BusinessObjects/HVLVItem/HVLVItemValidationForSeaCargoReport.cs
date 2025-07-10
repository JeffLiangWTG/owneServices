using CargoWise.EntityFramework;

namespace Enterprise.eTail.Business
{
	public class HVLVItemValidationForSeaCargoReport : HVLVItemValidation
	{
		public HVLVItemValidationForSeaCargoReport(AutoHVLVItem parent) : base(parent)
		{
		}

		protected override void CheckHVI_ContainerNumber()
		{
			MandatoryValidation.CheckEntered(Parent.HVI_ContainerNumberInfo);
		}
	}
}
