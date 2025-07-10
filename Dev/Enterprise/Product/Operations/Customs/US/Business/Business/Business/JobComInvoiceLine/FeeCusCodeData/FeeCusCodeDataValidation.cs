using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.US.Business
{
	public class FeeCusCodeDataValidation : Customs.Business.CusCodeDataValidation
	{
		public FeeCusCodeDataValidation(FeeCusCodeData parent)
			: base(parent)
		{
		}

		protected new FeeCusCodeData Parent
		{
			get { return (FeeCusCodeData)base.Parent; }
		}

		#region CY_SelectedRateType

		public void ValidateCY_SelectedRateType()
		{
			ValidateCalculatedProperty(Parent.CY_SelectedRateTypeInfo);
		}

		protected void CheckCY_SelectedRateType()
		{
			JobComInvoiceLine invoiceLine = Parent.Parent;
			if (invoiceLine != null && !invoiceLine.IsExport)
			{
				if (!Parent.CY_SelectedRateType_ReadOnly)
				{
					ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.CY_SelectedRateTypeInfo, Parent.Lookups.CY_SelectedRateTypeList, "rate type");
				}
				else
				{
					MandatoryValidation.MessageErrorIfIsEntered(Parent.CY_SelectedRateTypeInfo);
				}
			}
		}

		#endregion

		#region CY_FeeAmount

		public void ValidateCY_FeeAmount()
		{
			ValidateCalculatedProperty(Parent.CY_FeeAmountInfo);
		}

		protected void CheckCY_FeeAmount()
		{
			var fee = Parent;
			if (fee.CY_FeeAmount == ZDecimal.Zero && !fee.CY_IsOverridden && fee.IsMandatory)
			{
				fee.CY_FeeAmountInfo.AddWarning(NoFeeFormulaAvailableMessage);
			}
		}

		internal const string NoFeeFormulaAvailableMessage = "For the selected tariff number, no fee calculation formula is available. Please override the fee amount if necessary.";

		#endregion

		public override void ValidateAll()
		{
			base.ValidateAll();
			ValidateCY_SelectedRateType();
			ValidateCY_FeeAmount();
		}
	}
}
