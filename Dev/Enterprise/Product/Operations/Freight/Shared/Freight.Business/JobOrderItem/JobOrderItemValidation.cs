using CargoWise.EntityFramework;

namespace Enterprise.Freight.Business
{
	public class JobOrderItemValidation : AutoJobOrderItemValidation
	{
		public JobOrderItemValidation(AutoJobOrderItem parent) : base(parent)
		{
		}

		protected override void CheckJT_OrderReference()
		{
			base.CheckJT_OrderReference();
			MandatoryValidation.CheckEntered(Parent.JT_OrderReferenceInfo);
			OrderNumberValidation.ErrorIfOrderNumberNotValid(Parent.JT_OrderReferenceInfo);
		}
	}
}
