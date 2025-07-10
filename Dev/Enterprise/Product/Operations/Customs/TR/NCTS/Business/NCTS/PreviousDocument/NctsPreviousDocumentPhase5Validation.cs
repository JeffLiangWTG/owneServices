using CargoWise.EntityFramework;

namespace Enterprise.Customs.TR.NCTS.Business
{
	public class NctsPreviousDocumentPhase5Validation : EU.NCTS.Business.NctsPreviousDocumentPhase5Validation, INctsPreviousDocumentValidation
	{
		public NctsPreviousDocumentPhase5Validation(NctsPreviousDocument parent) : base(parent)
		{
		}

		protected new NctsPreviousDocument Parent => (NctsPreviousDocument)base.Parent;

		protected override bool IsSubTypeMandatory => true;

		public override string SubTypeEmpty => Res.GetString("C6CDF7A8-F998-4B0C-9ADE-31F3ACB37F06", "You have not entered a Payment Type.");

		public override void ValidateAll()
		{
			base.ValidateAll();
			ValidateIncoterm();
		}

		public void ValidateIncoterm()
		{
			ValidateCalculatedProperty(Parent.IncotermInfo);
		}

		protected void CheckIncoterm()
		{
			ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.IncotermInfo);
		}

		protected override void CheckCSI_ReferenceNumber()
		{
			base.CheckCSI_ReferenceNumber();
			MandatoryValidation.MessageErrorIfNotEntered(Parent.CSI_ReferenceNumberInfo);
		}

		protected override void CheckCSI_UnitOfQuantity2()
		{
			base.CheckCSI_UnitOfQuantity2();
			if (Parent.CSI_Quantity2 > 0)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.CSI_UnitOfQuantity2Info);
			}
			ListValidation.MessageErrorIfInvalidCode(Parent.CSI_UnitOfQuantity2Info);
		}

		protected override void CheckCSI_UnitOfQuantity3()
		{
			base.CheckCSI_UnitOfQuantity3();
			if (Parent.CSI_Quantity3 > 0)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.CSI_UnitOfQuantity3Info);
			}
			ListValidation.MessageErrorIfInvalidCode(Parent.CSI_UnitOfQuantity3Info);
		}

		protected override void CheckCSI_Procedure()
		{
			base.CheckCSI_Procedure();
			ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.CSI_ProcedureInfo);
		}

		protected override void CheckCSI_RN_NKCountryCode()
		{
			base.CheckCSI_RN_NKCountryCode();
			ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.CSI_RN_NKCountryCodeInfo);
		}

		protected override void CheckCSI_RX_NKCurrency()
		{
			base.CheckCSI_RX_NKCurrency();
			if (Parent.CSI_Value > 0)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.CSI_RX_NKCurrencyInfo);
			}
			ListValidation.MessageErrorIfInvalidCode(Parent.CSI_RX_NKCurrencyInfo);
		}
	}
}
