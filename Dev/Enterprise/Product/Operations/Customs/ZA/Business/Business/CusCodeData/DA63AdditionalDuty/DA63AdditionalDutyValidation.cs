using System.Linq;

namespace Enterprise.Customs.ZA.Business
{
	public class DA63AdditionalDutyValidation
		: Customs.Business.CusCodeDataValidation
	{
		public DA63AdditionalDutyValidation(DA63AdditionalDuty parent)
			: base(parent)
		{
		}

		protected new DA63AdditionalDuty Parent => (DA63AdditionalDuty)base.Parent;

		public override void ValidateAll()
		{
			base.ValidateAll();
			ValidateCY_Value();
		}

		#region CY_Value

		public void ValidateCY_Value()
		{
			ValidateCalculatedProperty(Parent.CY_ValueInfo);
		}

		protected void CheckCY_Value()
		{
			var invoiceLine = Parent.InvoiceLine;
			if (invoiceLine != null && invoiceLine.IsDA63WithOriginalEntry)
			{
				var pps = invoiceLine.Factory.GetAllProvisionalPaymentTypes().ToArray();
				var types = invoiceLine.Factory.GetDA63PartList().GetAllCodes().Except(pps).ToArray();
				var code = Parent.CY_Code;
				var originValue = Parent.OriginValue;

				if (!originValue.IsEmpty
						&& !code.IsEmpty
						&& types.Contains(code.ToString())
						&& Parent.CY_Value > originValue)
				{
					Parent.CY_ValueInfo.AddMessageError(JobComInvoiceLineValidation.DA63ValueShouldntBeExceedingOriginalValue);
				}
			}
		}

		#endregion

		protected override void CheckCY_Code()
		{
			base.CheckCY_Code();
			var code = Parent.CY_Code;
			if (!code.IsEmpty)
			{
				var invoiceLine = Parent.InvoiceLine;
				if (invoiceLine != null)
				{
					var sameCodeElements = invoiceLine.DA63AdditionalDuties.GetElementsHaving(code);
					if (sameCodeElements.Length > 1)
					{
						Parent.CY_CodeInfo.AddError(Res.GetString("2E352258-0E6A-4DD7-96C1-26FCB8DFEF63", "Duty '{0}' already exists.", code));
					}
				}
			}
		}
	}
}
