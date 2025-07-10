using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.Business
{
	public abstract class SharedCusPermitLineTransactionValidation : CusPermitLineTransactionValidation
	{
		public SharedCusPermitLineTransactionValidation(SharedCusPermitLineTransaction parent) : base(parent)
		{
		}

		public new SharedCusPermitLineTransaction Parent => (SharedCusPermitLineTransaction)base.Parent;

		protected override void CheckCPL_TransactionType()
		{
			var parent = Parent;
			var targetInfo = parent.CPL_TransactionTypeInfo;
			MandatoryValidation.CheckEntered(targetInfo);
			ListValidation.ErrorIfInvalidCode(targetInfo);

			if (parent.CPL_TransactionType == GuaranteeTransactionTypeList.Codes.OBA)
			{
				var applicationCode = parent.PermitHeader?.CPH_ApplicationCode ?? ZString.Empty;
				if (applicationCode != Common.Shared.CusPermitHeaderApplicationCodeList.Codes.Guarantee)
				{
					targetInfo.AddError(Res.GetString("3AD1F1F0-9C5E-4312-922F-D29E4CB63A2D", "Transaction with OBA type can only be added for Guarantee."));
				}
			}
		}

		protected override void CheckCPL_TransactionCategory()
		{
			var targetInfo = Parent.CPL_TransactionCategoryInfo;
			MandatoryValidation.CheckEntered(targetInfo);
			ListValidation.ErrorIfInvalidCode(targetInfo);
		}

		protected override void CheckCPL_Reference()
		{
			MandatoryValidation.CheckEntered(Parent.CPL_ReferenceInfo);
		}

		internal static string BalanceCannotBeNegative
		{
			get { return Res.GetString("15381647-C951-4A86-8CFF-2A0A325B3CF4", "Transaction results in a negative balance."); }
		}

		protected override void CheckCPL_TranValue()
		{
			var valueBalance = Parent.PermitHeader?.ValueBalance ?? ZDecimal.Zero;
			if (Parent.CPL_TranValue < ZDecimal.Zero && valueBalance < ZDecimal.Zero)
			{
				Parent.CPL_TranValueInfo.AddError(BalanceCannotBeNegative);
			}
		}
	}
}
