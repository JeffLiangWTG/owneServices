using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.US.Business
{
	public class USCustomsNumberViewStmNumsWrapperValidation : CustomsNumberViewStmNumsWrapperValidation
	{
		public USCustomsNumberViewStmNumsWrapperValidation(USCustomsNumberViewStmNumsWrapper parent)
			: base(parent)
		{ }

		protected override void ValidateAllCore()
		{
			ValidateAppliesTo();
		}

		public void ValidateAppliesTo()
		{
			ValidateCalculatedProperty(Parent.AppliesToInfo);
		}

		protected void CheckAppliesTo()
		{
			if (Parent.IsCustomsEntry)
			{
				MandatoryValidation.CheckEntered(Parent.AppliesToInfo);
			}
		}

		protected new USCustomsNumberViewStmNumsWrapper Parent => (USCustomsNumberViewStmNumsWrapper)base.Parent;
	}
}
