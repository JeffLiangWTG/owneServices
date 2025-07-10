using CargoWise.EntityFramework;

namespace Enterprise.Customs.TR.NCTS.Business
{
	public class SPTSBillValidation : Customs.Business.CusInBondBillValidation
	{
		public SPTSBillValidation(SPTSBill parent)
			: base(parent)
		{
		}

		protected new SPTSBill Parent
		{
			get { return (SPTSBill)base.Parent; }
		}

		protected override void CheckB0_MasterBillNumber()
		{
			base.CheckB0_MasterBillNumber();
			MandatoryValidation.MessageErrorIfNotEntered(Parent.B0_MasterBillNumberInfo);
		}

		protected override void CheckB0_ReferenceQualifier()
		{
			base.CheckB0_ReferenceQualifier();
			MandatoryValidation.MessageErrorIfNotEntered(Parent.B0_ReferenceQualifierInfo);

			ListValidation.MessageErrorIfInvalidCode(Parent.B0_ReferenceQualifierInfo);
		}

		protected override void CheckB0_ReferenceID()
		{
			base.CheckB0_ReferenceID();
			if (Parent.B0_ReferenceQualifier != SPTSDeclarationTypeList.Codes.K)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.B0_ReferenceIDInfo);
			}
		}
	}
}
