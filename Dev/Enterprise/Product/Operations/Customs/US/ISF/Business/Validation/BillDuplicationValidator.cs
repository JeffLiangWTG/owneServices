using System.Collections.Generic;
using CargoWise.Common.Testing;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common.US.ISF;
using Enterprise.Customs.US.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.US.ISF.Business
{
	[SuppressStaticMethodsAreLocatedOnCorrectClassMessage]
	public static class BillDuplicationValidator
	{
		public static void ValidateBillIsNotDuplicated(CusISFBill billToCheck, ZPropertyInfo info)
		{
			CusISFHeader header = billToCheck == null ? null : billToCheck.Header;
			if (header != null && !billToCheck.BB_BillNum.IsEmpty && !billToCheck.BB_BillType.IsEmpty)
			{
				bool foundDuplicateInThisISF = false;
				foreach (CusISFBill otherBill in header.ReferenceDatas)
				{
					var matchBillType = (IsBillOfLading(billToCheck.BB_BillType) && IsBillOfLading(otherBill.BB_BillType)) || billToCheck.BB_BillType == otherBill.BB_BillType;

					if (otherBill != billToCheck && matchBillType && otherBill.BB_BillNum == billToCheck.BB_BillNum)
					{
						foundDuplicateInThisISF = true;
						break;
					}
				}

				if (foundDuplicateInThisISF)
				{
					info.AddMessageError(ValidationConstants.Bill.ReferenceDataAlreadyExists);
				}
				else
				{
					CheckBillIsNotDuplicatedInAnotherISF(billToCheck, info);
				}
			}
		}

		static bool IsBillOfLading(ZString billType)
		{
			return billType == BillTypeList.Codes.HouseBillOfLading || billType == BillTypeList.Codes.OceanBillOfLading || billType == BillTypeList.Codes.MasterBillOfLading;
		}

		static void CheckBillIsNotDuplicatedInAnotherISF(CusISFBill billToCheck, ZPropertyInfo info)
		{
			CusISFBill bill = null;
			switch (billToCheck.BB_BillType)
			{
				case BillTypeList.Codes.HouseBillOfLading:
				case BillTypeList.Codes.OceanBillOfLading:
				case BillTypeList.Codes.MasterBillOfLading:
					bill = GetBillMatchingThisISF(billToCheck);
					break;
				case BillTypeList.Codes.BondReferenceNumber:
					if (billToCheck.Header.BF_BondType == ImporterBondTypeList.Codes.SingleTransactionBond && !billToCheck.Header.BF_SuretyCode.IsEmpty)
					{
						bill = GetSuretyBondReferenceNumberMatchingISF(billToCheck);
					}
					break;
				default:
					// do nothing
					break;
			}
			if (bill != null)
			{
				info.AddMessageError(ValidationConstants.Bill.ReferenceDataAlreadyExistsOnAnotherISF(billToCheck.BB_BillTypeDescription, bill.BB_BillNum, bill.Header.HumanReadableName));
			}
		}

		static CusISFBill GetBillMatchingThisISF(CusISFBill billToCheck)
		{
			ZQuery query = new ZQuery(CusISFBillSchema.BB_BF, SQLComparisonOperator.NotEqual, billToCheck.BB_BF);
			query.AddToFilter(CusISFBillSchema.BB_BillNum, billToCheck.BB_BillNum);
			var billTypeToCheck = billToCheck.BB_BillType == BillTypeList.Codes.MasterBillOfLading ? new[] { BillTypeList.Codes.HouseBillOfLading, BillTypeList.Codes.OceanBillOfLading }
				: new[] { BillTypeList.Codes.HouseBillOfLading, BillTypeList.Codes.OceanBillOfLading, BillTypeList.Codes.MasterBillOfLading };
			query.AddToFilter(CusISFBillSchema.BB_BillType, billTypeToCheck);
			return billToCheck.Factory.LoadTop1<CusISFBill>(query);
		}

		static CusISFBill GetSuretyBondReferenceNumberMatchingISF(CusISFBill billToCheck)
		{
			List<ZString> suretyCodes = new List<ZString>();
			CusISFHeader header = billToCheck.Header;
			foreach (CusISFBill bill in header.ReferenceDatas)
			{
				if (bill.IsSuretyCode && !bill.BB_BillNum.IsEmpty && !suretyCodes.Contains(bill.BB_BillNum))
				{
					suretyCodes.Add(bill.BB_BillNum);
				}
			}

			CusISFBill result = null;
			if (suretyCodes.Count > 0)
			{
				ZDBOnlySubQuery subQuery = new ZDBOnlySubQuery(typeof(CusISFBill), CusISFBillSchema.BB_BF);
				subQuery.AddToFilter(CusISFBillSchema.BB_BF, SQLComparisonOperator.NotEqual, billToCheck.BB_BF);
				subQuery.AddToFilter(CusISFBillSchema.BB_BillNum, suretyCodes);
				subQuery.AddToFilter(CusISFBillSchema.BB_BillType, BillTypeList.Codes.SuretyCode);

				ZDBOnlySubQuery subQuery2 = new ZDBOnlySubQuery(typeof(CusISFHeader), CusISFHeaderSchema.PK);
				subQuery2.AddToFilter(CusISFHeaderSchema.BF_BondType, ImporterBondTypeList.Codes.SingleTransactionBond);

				ZDBOnlyQuery query = new ZDBOnlyQuery(typeof(CusISFBill));
				query.AddToFilter(CusISFBillSchema.BB_BF, SQLComparisonOperator.NotEqual, billToCheck.BB_BF);
				query.AddToFilter(CusISFBillSchema.BB_BillNum, billToCheck.BB_BillNum);
				query.AddToFilter(CusISFBillSchema.BB_BillType, billToCheck.BB_BillType);
				query.AddSubQuery(CusISFBillSchema.BB_BF, subQuery, JoinCondition.And);
				query.AddSubQuery(CusISFBillSchema.BB_BF, subQuery2, JoinCondition.And);

				result = billToCheck.Factory.LoadTop1<CusISFBill>(query);
			}
			return result;
		}
	}
}
