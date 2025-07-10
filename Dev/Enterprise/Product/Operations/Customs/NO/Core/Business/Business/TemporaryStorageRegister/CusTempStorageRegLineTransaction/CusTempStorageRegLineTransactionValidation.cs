using System.Linq;
using CargoWise.EntityFramework;
using CusTempStorageRegLineTransactionTypeList = Enterprise.Customs.EFTA.TemporaryStorageRegister.Business.CusTempStorageRegLineTransactionTypeList;

namespace Enterprise.Customs.NO.Business;

public class CusTempStorageRegLineTransactionValidation(CusTempStorageRegLineTransaction parent) : EFTA.TemporaryStorageRegister.Business.CusTempStorageRegLineTransactionValidation(parent)
{
	CusTempStorageRegLineTransaction RegLineTransaction => Parent as CusTempStorageRegLineTransaction;

	protected override void CheckSRT_GrossWeight()
	{
		base.CheckSRT_GrossWeight();

		var transactionType = RegLineTransaction.SRT_TransactionType;
		var isWeightMandatory = transactionType == CusTempStorageRegLineTransactionTypeList.Codes.OpeningBalance
								|| (transactionType == CusTempStorageRegLineTransactionTypeList.Codes.Adjustment && RegLineTransaction.SRT_PackageQty == 0);

		if (isWeightMandatory)
		{
			MandatoryValidation.CheckNotZero(Parent.SRT_GrossWeightInfo);
		}
	}

	protected override void CheckSRT_PackageQty()
	{
		base.CheckSRT_PackageQty();

		var transactionType = RegLineTransaction.SRT_TransactionType;
		var isPackQtyMandatory = transactionType == CusTempStorageRegLineTransactionTypeList.Codes.OpeningBalance
									|| (transactionType == CusTempStorageRegLineTransactionTypeList.Codes.Adjustment && RegLineTransaction.SRT_GrossWeight == 0);

		if (isPackQtyMandatory)
		{
			MandatoryValidation.CheckNotZero(Parent.SRT_PackageQtyInfo);
		}

		if (transactionType == CusTempStorageRegLineTransactionTypeList.Codes.OpeningBalance)
		{
			MandatoryValidation.CheckNotNegative(Parent.SRT_PackageQtyInfo);
		}
		else
		{
			var regLine = RegLineTransaction.RegLine;
			if (regLine is CusTempStorageRegLine cusTempStorageRegLine)
			{
				var sumPackageQty = cusTempStorageRegLine.CusTempStorageRegLineTransactions.Sum(t => t.SRT_PackageQty);

				if (sumPackageQty < 0)
				{
					Parent.SRT_PackageQtyInfo.AddError(Res.GetString("49967429-f7dc-4aed-bb10-fabaec8b8d54", "The sum of Package Qty. across all transaction lines should not be negative."));
				}
			}
		}
	}

	protected override void CheckSRT_Reference()
	{
		base.CheckSRT_Reference();

		if (!Parent.SRT_ReferenceType.IsEmpty)
		{
			MandatoryValidation.MessageErrorIfNotEntered(Parent.SRT_ReferenceInfo);
		}
	}

	protected override void CheckSRT_ReferenceType()
	{
		base.CheckSRT_ReferenceType();
		ListValidation.MessageErrorIfInvalidCode(Parent.SRT_ReferenceTypeInfo);
	}

	protected override void CheckSRT_BondAmount()
	{
		base.CheckSRT_BondAmount();

		MandatoryValidation.MessageErrorIfNotEntered(Parent.SRT_BondAmountInfo);
	}
}
