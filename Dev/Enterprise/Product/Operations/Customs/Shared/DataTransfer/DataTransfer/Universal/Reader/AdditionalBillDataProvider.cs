using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.Extensions;
using Enterprise.UniversalDataBuss.DataObjects.Core;

namespace Enterprise.Customs.DataTransfer.Universal
{
	public class AdditionalBillDataProvider<TBill> : IDisposable
		where TBill : Bill
	{
		public AdditionalBillDataProvider(BaseJobDeclaration declaration)
		{
			this.declaration = Argument.NotNull(declaration, nameof(declaration));
			existingBills = new List<TBill>(new TypedEnumerable<TBill>(declaration.Bills));
			existingBills.LoadChildrenForDeletionIncludingCusAddInfoAndCusCodeData();
			declaration.Bills.CountChanged += Bills_CountChanged;
		}

		readonly BaseJobDeclaration declaration;
		readonly List<TBill> existingBills;

		public ZGuid DeclarationPK => declaration.PK;
		public ZInt ClusterKey => declaration.JE_ClusterKey;

		public TBill GetAndRemoveExistingBill(ZString billNum, ZString billType, Func<TBill, bool> additionalMatch = null)
		{
			var result = existingBills.FirstOrDefault(bill => bill.CU_BillType.EqualsIgnoringCase(billType) && bill.CU_BillNum.EqualsIgnoringCase(billNum) && (additionalMatch == null || additionalMatch(bill)));
			if (result != null)
			{
				existingBills.Remove(result);
			}
			return result;
		}

		public IEnumerable<TBill> GetUnprocessedExistingBills() => existingBills;

		public TBill CreateNewBill()
		{
			var result = (TBill)declaration.Bills.AddNew();
			result.CU_CU_ParentBill = ZGuid.Empty;
			result.CU_BillType = ZString.Empty;
			result.CU_BillNum = ZString.Empty;
			result.CU_GUIPresentationRecord = ZBool.False;
			existingBills.Remove(result);
			return result;
		}

		public void Dispose() => declaration.Bills.CountChanged -= Bills_CountChanged;

		void Bills_CountChanged(object sender, CollectionCountChangedEventArgs e)
		{
			if (e.ItemAdded)
			{
				existingBills.Add((TBill)e.BizObject);
			}
		}

		internal void SyncPrimaryBill(Bill bill, ZString billNumber, Func<Bill, bool> additionalMatch)
		{
			if (bill.IsMasterBill)
			{
				if (declaration.PrimaryMasterBill == bill && !declaration.JE_MasterBill.EqualsIgnoringCase(billNumber))
				{
					using (declaration.TemporarySetAdditionalPrimaryMasterBillMatching(additionalMatch))
					{
						declaration.JE_MasterBill = billNumber;
					}
				}
			}
			else if (declaration.PrimaryHouseBill == bill && !declaration.JE_HouseBill.EqualsIgnoringCase(billNumber))
			{
				using (declaration.TemporarySetAdditionalPrimaryHouseBillMatching(additionalMatch))
				{
					declaration.JE_HouseBill = billNumber;
				}
			}
		}
	}
}

