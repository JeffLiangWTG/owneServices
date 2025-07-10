using CargoWise.Types;

namespace Enterprise.Customs.US.Business
{
	public class BIRDBillUpdateTool
	{
		public void UpdateBill(JobDeclaration declaration, ZString masterBillNumber, ZString houseBillNumber, ZString subHouseBillNumber, ZInt quantity, ZString unit, ZDate iTDate, ZString iTNumber, ZString issuerCodeOfMasterBillNumber, ZString issuerCodeOfHouseBillNumber)
		{
			Bill masterBill = null, houseBill = null, subhouseBill = null;

			if (!masterBillNumber.IsEmpty)
			{
				masterBill = GetOrCreateBill(declaration, Customs.Business.BillTypeList.Codes.MasterBill, masterBillNumber);
			}

			if (!houseBillNumber.IsEmpty)
			{
				houseBill = GetOrCreateBill(declaration, Customs.Business.BillTypeList.Codes.HouseBill, houseBillNumber);

				houseBill.CU_CU_ParentBill = masterBill != null ? masterBill.PK : ZGuid.Empty;
			}

			if (!subHouseBillNumber.IsEmpty)
			{
				subhouseBill = GetOrCreateBill(declaration, Customs.Business.BillTypeList.Codes.SubHouseBill, subHouseBillNumber);

				subhouseBill.CU_CU_ParentBill = houseBill != null ? houseBill.PK : ZGuid.Empty;
			}

			Bill lowestBill = subhouseBill ?? (houseBill ?? masterBill);

			if (lowestBill != null)
			{
				lowestBill.CU_NoOfPacks += new ZDecimal(quantity);
				lowestBill.CU_PackType = unit;

				var itNos = lowestBill.ITAndSplitDetails.FindByItNumber(iTNumber);
				if (itNos.Length == 0)
				{
					var number = lowestBill.ITAndSplitDetails.AddNew();
					number.US_ITNumber = iTNumber;
					number.US_NoOfPacks = quantity;
				}
			}

			if (!iTDate.IsEmpty)
			{
				declaration.US_ITDate = iTDate;
			}

			if (masterBill != null)
			{
				masterBill.US_UI_NKBillIssuerSCAC = issuerCodeOfMasterBillNumber;
			}

			if (houseBill != null)
			{
				houseBill.US_UI_NKBillIssuerSCAC = issuerCodeOfHouseBillNumber;
			}
		}

		Bill GetOrCreateBill(JobDeclaration declaration, ZString billType, ZString billNum)
		{
			Bill result = declaration.Bills.FindByBillNumberAndType(billNum, billType);

			if (result == null)
			{
				result = declaration.Bills.AddNew();

				result.CU_BillType = billType;
				result.CU_BillNum = billNum;
			}

			return result;
		}
	}
}
