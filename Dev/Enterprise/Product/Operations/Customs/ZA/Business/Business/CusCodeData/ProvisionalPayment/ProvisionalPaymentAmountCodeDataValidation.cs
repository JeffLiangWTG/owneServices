using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.ZA.Business
{
	public class ProvisionalPaymentAmountCodeDataValidation : CusCodeDataValidation
	{
		public ProvisionalPaymentAmountCodeDataValidation(ProvisionalPaymentAmountCodeData parent) : base(parent)
		{
		}

		public new ProvisionalPaymentAmountCodeData Parent => (ProvisionalPaymentAmountCodeData)base.Parent;

		#region Override

		public override void ValidateAll()
		{
			base.ValidateAll();
			ValidateCY_Value();
		}

		protected override void CheckCY_Code()
		{
			base.CheckCY_Code();

			var code = Parent.CY_Code;
			var targetInfo = Parent.CY_CodeInfo;
			var parentEntryLine = Parent?.ParentEntryLine;
			if (!code.IsEmpty && parentEntryLine != null)
			{
				if (parentEntryLine.ProvisionalPayments?.OfType<ProvisionalPaymentAmountCodeData>()?.Any(x => x.CY_Code == code && x.PK != Parent.PK) ?? ZBool.False)
				{
					targetInfo.AddMessageError(Res.GetString("4745E76F-5DCF-4066-8AAD-FBA66EDAD2AA", "Provisional Payment Code:{0} has already been specified for this Entry Line.", code));
				}
				var parentLineNumber = parentEntryLine.CL_LineNumber;
				if (parentEntryLine?.Header?.ProvisionalPaymentPayInfos.Any(x => x.C9_RemAdvReceived && x.C9_TransactionType == code && x.C9_IncomingPayResponseNo == parentLineNumber.ToString()) ?? false)
				{
					targetInfo.AddWarning(Res.GetString("244E7C6D-5E86-4A21-A8D5-7C25D962325A", "Provisional Payment Case for type:{0} has already been closed for this Entry Line, the entered amount won't be used for Fee Calculation or message sending.", code));
				}
			}

			((CusEntryLineValidation)parentEntryLine?.Validation)?.ValidateDiamondLevyValueAndAmount();
		}

		#endregion

		#region CY_Value

		public void ValidateCY_Value()
		{
			ValidateCalculatedProperty(Parent.CY_ValueInfo);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "This method is implicitly referenced by ZAttribute")]
		void CheckCY_Value()
		{
			if (!Parent.CY_Code.IsEmpty)
			{
				MandatoryValidation.MessageErrorIfIsZero(Parent.CY_ValueInfo);
			}
		}

		#endregion
	}
}
