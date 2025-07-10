using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;

namespace Enterprise.Customs.Business
{
	public class NPBOCusGuaranteeLineTransactionValidation : AutoNPBOCusGuaranteeLineTransactionValidation
	{
		public NPBOCusGuaranteeLineTransactionValidation(AutoNPBOCusGuaranteeLineTransaction parent) : base(parent)
		{
		}

		public new NPBOCusGuaranteeLineTransaction Parent => (NPBOCusGuaranteeLineTransaction)base.Parent;

		protected override void CheckCPL_TransactionType()
		{
			base.CheckCPL_TransactionType();
			var transactionType = Parent.CPL_TransactionType;
			var propertyInfo = Parent.CPL_TransactionTypeInfo;
			var openingLineTransactions = Parent.GuaranteeHeader.OpeningCusGuaranteeLineTransactions;

			ListValidation.ErrorIfInvalidCode(propertyInfo);

			if (transactionType != PermitTransactionTypeList.Codes.OBL && !openingLineTransactions.Any())
			{
				propertyInfo.AddError(MustAddOBLBeforeAnyOtherTransactionType);
			}

			if (transactionType == PermitTransactionTypeList.Codes.OBL && openingLineTransactions.Any(x => x.CPL_TransactionType == PermitTransactionTypeList.Codes.OBL))
			{
				propertyInfo.AddError(CusGuaranteeLineTransactionValidation.OnlyOneOBLAllowed);
			}

			if (transactionType == PermitTransactionTypeList.Codes.ADJ || transactionType == GuaranteeTransactionTypeList.Codes.OBA)
			{
				if (!Env.Security.GuaranteesManualTransactions.IsAllowed)
				{
					propertyInfo.AddError(DoNotHaveManualTransactionPermission);
				}
			}
		}

		protected override void CheckCPL_Reference()
		{
			base.CheckCPL_Reference();
			var transactionType = Parent.CPL_TransactionType;
			if (transactionType == PermitTransactionTypeList.Codes.ADJ || transactionType == PermitTransactionTypeList.Codes.OBL || transactionType == GuaranteeTransactionTypeList.Codes.OBA)
			{
				MandatoryValidation.CheckEntered(Parent.CPL_ReferenceInfo);
			}
		}

		protected override void CheckCPL_Comment()
		{
			base.CheckCPL_Comment();
			var transactionType = Parent.CPL_TransactionType;
			if (transactionType == PermitTransactionTypeList.Codes.ADJ || transactionType == GuaranteeTransactionTypeList.Codes.OBA)
			{
				MandatoryValidation.CheckEntered(Parent.CPL_CommentInfo);
			}
		}

		protected override void CheckCPL_TransactionDate()
		{
			base.CheckCPL_TransactionDate();
			if (!Parent.CPL_TransactionType.IsEmpty)
			{
				MandatoryValidation.CheckEntered(Parent.CPL_TransactionDateInfo);
			}
		}

		protected override void CheckCPL_TranValue()
		{
			base.CheckCPL_TranValue();
			if (!Parent.CPL_TransactionType.IsEmpty)
			{
				var guaranteeHeader = Parent.GuaranteeHeader;
				var tranValue = Parent.CPL_TranValue;
				var tranValueIsEmpty = tranValue.IsEmpty;
				var propertyInfo = Parent.CPL_TranValueInfo;
				var transactionType = Parent.CPL_TransactionType;
				if (transactionType == PermitTransactionTypeList.Codes.OBL)
				{
					if (tranValueIsEmpty || tranValue < ZDecimal.Zero)
					{
						propertyInfo.AddError(CusGuaranteeLineTransactionValidation.OpeningBalanceTransactionValueMustGreaterThanZero);
					}
				}
				else
				{
					if (tranValueIsEmpty)
					{
						propertyInfo.AddError(CusGuaranteeLineTransactionValidation.AdjustmentTransactionValueCannotBeZero);
					}
					else
					{
						var isADJTransaction = transactionType == PermitTransactionTypeList.Codes.ADJ;
						if (isADJTransaction || transactionType == GuaranteeTransactionTypeList.Codes.OBA)
						{
							var remainingBalance = guaranteeHeader.CPH_Calc_TotalBalanceIncludingPendingDecimal + tranValue;
							if (remainingBalance < ZDecimal.Zero)
							{
								propertyInfo.AddError(RemainingBalanceMustNotBeBelowZeroAfterTransactionIsAdded);
							}
							else if (isADJTransaction && remainingBalance > guaranteeHeader.CPH_Calc_OpeningBalance)
							{
								propertyInfo.AddError(RemainingBalanceMustNotBeBeyondGuaranteeAmountAfterADJTransactionIsAdded);
							}
						}
					}
				}
			}
		}

		internal static string DoNotHaveManualTransactionPermission
		{
			get { return Res.GetString("4738C654-BCF7-4245-87C4-AA910C401820", "You do not have permission to add manual adjustment transaction."); }
		}

		internal static string MustAddOBLBeforeAnyOtherTransactionType
		{
			get { return Res.GetString("CD6C4E48-0B40-4F06-9256-ABB476EE11C3", "Please add an opening balance before any adjustment."); }
		}

		internal static string RemainingBalanceMustNotBeBelowZeroAfterTransactionIsAdded
		{
			get { return Res.GetString("CC776F5B-DCB4-4CA9-B507-09C073DE360E", "Remaining Balance mustn't be below zero after adjustment transaction is added."); }
		}

		internal static string RemainingBalanceMustNotBeBeyondGuaranteeAmountAfterADJTransactionIsAdded
		{
			get { return Res.GetString("5A2E51C4-A545-4793-84E3-EA183E00F947", "Remaining Balance mustn't be beyond Guarantee Amount after ADJ adjustment transaction is added."); }
		}
	}
}
