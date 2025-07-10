using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.Business
{
	public class CusGuaranteeLineTransactionValidation : SharedCusPermitLineTransactionValidation
	{
		public CusGuaranteeLineTransactionValidation(BaseCusGuaranteeLineTransaction parent)
			: base(parent)
		{
		}

		public new BaseCusGuaranteeLineTransaction Parent => (BaseCusGuaranteeLineTransaction)base.Parent;

		public static string OpeningBalanceTransactionValueMustGreaterThanZero
		{
			get { return Res.GetString("C8224D1D-E2D7-4B54-8202-9F4426D417C0", "Opening balance transaction value must be greater than zero."); }
		}

		public static string OnlyOneOBLAllowed
		{
			get { return Res.GetString("20BE875E-77F9-4DAA-986D-C87C16EA1D10", "There cannot be multiple opening balance transactions."); }
		}

		public static string AdjustmentTransactionValueCannotBeZero
		{
			get { return Res.GetString("3877904C-FFC1-4574-BE2C-8E8F336DE9AC", "Adjustment transaction value cannot be zero."); }
		}

		protected override void CheckCPL_Comment()
		{
			if (Parent.CPL_TransactionType == PermitTransactionTypeList.Codes.ADJ)
			{
				MandatoryValidation.CheckEntered(Parent.CPL_CommentInfo);
			}
		}

		protected override void CheckCPL_Reference()
		{
			if (Parent.CPL_TransactionType == PermitTransactionTypeList.Codes.ADJ)
			{
				MandatoryValidation.CheckEntered(Parent.CPL_ReferenceInfo);
			}
		}

		protected override void CheckCPL_TranValue()
		{
			if (Parent.CPL_TransactionType == PermitTransactionTypeList.Codes.OBL)
			{
				if (Parent.CPL_TranValue.IsEmpty || Parent.CPL_TranValue <= ZDecimal.Zero)
				{
					Parent.CPL_TranValueInfo.AddError(OpeningBalanceTransactionValueMustGreaterThanZero);
				}
			}
			else if (Parent.CPL_TransactionType == PermitTransactionTypeList.Codes.ADJ)
			{
				if (Parent.CPL_TranValue.IsEmpty || Parent.CPL_TranValue == ZDecimal.Zero)
				{
					Parent.CPL_TranValueInfo.AddError(AdjustmentTransactionValueCannotBeZero);
				}
			}
		}

		protected override void CheckCPL_TransactionType()
		{
			base.CheckCPL_TransactionType();

			var oBLTransactions = Parent.GuaranteeHeader.CusGuaranteeLineTransactions.Find(x => x.CPL_TransactionType == PermitTransactionTypeList.Codes.OBL);
			if (Parent.CPL_TransactionType == PermitTransactionTypeList.Codes.OBL && oBLTransactions.Count() >= 2)
			{
				Parent.CPL_TransactionTypeInfo.AddError(OnlyOneOBLAllowed);
			}
		}
	}
}
