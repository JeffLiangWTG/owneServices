using CargoWise.EntityFramework;

namespace Enterprise.Customs.Business
{
	public class CommonCusReferenceValidation : CusReferenceValidation
	{
		public CommonCusReferenceValidation(CommonCusReference parent)
			: base(parent)
		{
		}

		public override void ValidateAll()
		{
			base.ValidateAll();
			ValidateOwnerOrgPK();
		}

		public void ValidateOwnerOrgPK()
		{
			ValidateCalculatedProperty(Parent.OwnerOrgPKInfo);
		}

		protected virtual void CheckOwnerOrgPK()
		{
			TypeValidation.CheckValidGuid(Parent.OwnerOrgPKInfo);
		}

		protected override void CheckCFR_Code()
		{
			base.CheckCFR_Code();

			var targetInfo = Parent.CFR_CodeInfo;
			MandatoryValidation.CheckEntered(targetInfo);
			ListValidation.ErrorIfInvalidCode(targetInfo);
		}

		protected override void CheckCFR_Reference()
		{
			base.CheckCFR_Reference();

			CheckCFR_ReferenceIsNotEmpty();
		}

		protected virtual void CheckCFR_ReferenceIsNotEmpty()
		{
			MandatoryValidation.CheckEntered(Parent.CFR_ReferenceInfo);
		}

		protected new CommonCusReference Parent => (CommonCusReference)base.Parent;
	}
}
