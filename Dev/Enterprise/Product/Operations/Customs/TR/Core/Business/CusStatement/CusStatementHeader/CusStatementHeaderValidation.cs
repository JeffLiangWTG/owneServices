using CargoWise.EntityFramework;

namespace Enterprise.Customs.TR.Business
{
	public class CusStatementHeaderValidation : Customs.Business.CusStatementHeaderValidation
	{
		public CusStatementHeaderValidation(CusStatementHeader parent) : base(parent)
		{
		}

		protected new CusStatementHeader Parent => (CusStatementHeader)base.Parent;

		protected override void CheckB2_PaymentType()
		{
			base.CheckB2_PaymentType();
			ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.B2_PaymentTypeInfo);
		}

		protected override void CheckB2_PrintDate()
		{
			base.CheckB2_PrintDate();
			MandatoryValidation.MessageErrorIfNotEntered(Parent.B2_PrintDateInfo);
		}

		protected override void CheckB2_Status()
		{
			base.CheckB2_Status();
			ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.B2_StatusInfo);
		}

		protected override void CheckB2_PaymentStatus()
		{
			base.CheckB2_PaymentStatus();
			ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.B2_PaymentStatusInfo);
		}

		protected override void CheckB2_PaymentParty()
		{
			base.CheckB2_PaymentParty();
			ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.B2_PaymentPartyInfo);
		}
	}
}
