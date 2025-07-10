using System;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.Business
{
	public class BillGUIPresentationFlagUpdater
	{
		public BillGUIPresentationFlagUpdater(BaseJobDeclaration declaration)
		{
			this.declaration = declaration;
		}

		readonly BaseJobDeclaration declaration;

		bool IsCopying
		{
			get { return ((IBusinessObjectInternals)declaration).IsCopying; }
		}

		internal bool IsSynchronising
		{
			get { return suspenderIndex > 0; }
		}

		public void SynchroniseToBills(ZString newValue, ZString billType, Func<Bill, bool> additionalBillDetailMatching = null)
		{
			if (!IsCopying && !IsSynchronising)
			{
				Bill oldPrimaryBill = declaration.Bills.GetPrimaryBill(billType);
				Bill newPrimaryBill = oldPrimaryBill;

				using (SuspendSynch())
				{
					if (!newValue.IsEmpty)
					{
						newPrimaryBill = MakeThisBillTheGUIPresentationRecord(newValue, billType, oldPrimaryBill, additionalBillDetailMatching);

						if (newPrimaryBill == null)
						{
							newPrimaryBill = declaration.Bills.CreatePrimaryBill(billType);
						}
					}

					if (billType == BillTypeList.Codes.MasterBill)
					{
						if (oldPrimaryBill != null)
						{
							UpdateTheChildBillsAccordingToParentBillWithoutSuspendingSync(oldPrimaryBill);
						}
						if (newPrimaryBill != null && newPrimaryBill != oldPrimaryBill)
						{
							UpdateTheChildBillsAccordingToParentBillWithoutSuspendingSync(newPrimaryBill);
						}
					}

					if (newPrimaryBill != null)
					{
						newPrimaryBill.CU_BillNum = newValue;

						Bill primaryMasterBill = declaration.PrimaryMasterBill;
						Bill primaryHouseBill = declaration.PrimaryHouseBill;

						if (primaryMasterBill != null && primaryHouseBill != null)
						{
							primaryHouseBill.CU_CU_ParentBill = primaryMasterBill.PK;

							if (declaration.JE_HouseBill != primaryHouseBill.CU_BillNum)
							{
								declaration.JE_HouseBill = primaryHouseBill.CU_BillNum;
							}
						}
					}
				}
			}
		}

		public void SynchroniseFromBillNum(Bill billWithChangedBillNum)
		{
			if (billWithChangedBillNum.CU_GUIPresentationRecord && !IsCopying && !IsSynchronising)
			{
				using (SuspendSynch())
				{
					string columnNameInDec = billWithChangedBillNum.IsMasterBill ? JobDeclarationSchema.Constants.JE_MasterBill : JobDeclarationSchema.Constants.JE_HouseBill;

					declaration[columnNameInDec] = billWithChangedBillNum.CU_BillNum;
				}
			}
		}

		public void UpdateWhenABillIsRemoved(Bill billToBeRemoved)
		{
			if (!IsCopying && !IsSynchronising)
			{
				using (SuspendSynch())
				{
					if (billToBeRemoved.CU_GUIPresentationRecord)
					{
						Bill replacementBill = null;
						var parentBillPK = billToBeRemoved.CU_CU_ParentBill;
						var billType = billToBeRemoved.CU_BillType;
						foreach (Bill bill in declaration.Bills)
						{
							if (bill != billToBeRemoved &&
								bill.CU_BillType == billType &&
								bill.CU_CU_ParentBill == parentBillPK)
							{
								replacementBill = bill;
								break;
							}
						}

						if (replacementBill == null)
						{
							var billWithSameType = declaration.Bills.OfType<Bill>().Where(x => x != billToBeRemoved && x.CU_CU_ParentBill.IsEmpty && x.CU_BillType == billType && x.PK != parentBillPK).Take(2).ToArray();
							if (billWithSameType.Length == 1)
							{
								replacementBill = billWithSameType[0];
								replacementBill.CU_CU_ParentBill = parentBillPK;
							}
						}

						var oldChildBillNum = billToBeRemoved.ChildBills.GUIPresentationBill?.CU_BillNum ?? ZString.Empty;
						var newChildBillNum = oldChildBillNum;

						if (replacementBill != null)
						{
							//do this for billToBeRemoved only when replacementBill is there
							billToBeRemoved.CU_GUIPresentationRecord = false;
							UpdateTheChildBillsAccordingToParentBillWithoutSuspendingSync(billToBeRemoved);

							replacementBill.CU_GUIPresentationRecord = true;
							UpdateTheChildBillsAccordingToParentBillWithoutSuspendingSync(replacementBill);
							newChildBillNum = replacementBill.ChildBills.GUIPresentationBill?.CU_BillNum ?? ZString.Empty;
						}

						var columnNameInDec = billToBeRemoved.IsMasterBill ? JobDeclarationSchema.Constants.JE_MasterBill : JobDeclarationSchema.Constants.JE_HouseBill;
						declaration[columnNameInDec] = replacementBill?.CU_BillNum ?? ZString.Empty;

						if (!oldChildBillNum.EqualsIgnoringCase(newChildBillNum))
						{
							declaration.JE_HouseBill = newChildBillNum;
						}
					}
				}
			}
		}

		public void UpdateWhenCU_CU_ParentBillChanged(Bill parentBill, Bill billBeingChanged)
		{
			if (!IsCopying && !IsSynchronising && billBeingChanged.CU_CU_ParentBill == parentBill.PK)
			{
				using (SuspendSynch())
				{
					ZBool oldGUIPresentationRecord = billBeingChanged.CU_GUIPresentationRecord;

					UpdateTheChildBillsAccordingToParentBillWithoutSuspendingSync(parentBill);

					//when a billBeingChanged is not added to the collection yet 
					if (parentBill.CU_GUIPresentationRecord && !parentBill.ChildBills.HasAGUIPresentationChild)
					{
						if (parentBill.IsMasterBill && billBeingChanged.IsHouseBill)
						{
							MakeThisBillTheGUIPresentationRecord(billBeingChanged, declaration.PrimaryHouseBill);
						}
						else
						{
							billBeingChanged.CU_GUIPresentationRecord = true;
						}
					}

					if (billBeingChanged.CU_GUIPresentationRecord && billBeingChanged.IsHouseBill && oldGUIPresentationRecord != billBeingChanged.CU_GUIPresentationRecord)
					{
						declaration.JE_HouseBill = billBeingChanged.CU_BillNum;
					}
				}
			}
		}

		public void UpdateWhenCU_BillTypeChanged(Bill billChanged)
		{
			if (!IsCopying && !IsSynchronising)
			{
				using (SuspendSynch())
				{
					billChanged.CU_GUIPresentationRecord = billChanged.IsThisTheOnlyMasterBill ||
						billChanged.IsThisTheOnlyHouseBillLinkedToPrimaryMasterBillOfDeclaration;

					if (billChanged.IsThisTheOnlyMasterBill && declaration.PrimaryHouseBill is Bill primaryHouseBill)
					{
						primaryHouseBill.CU_CU_ParentBill = billChanged.PK;
					}

					var newPrimaryMasterBill = declaration.PrimaryMasterBill?.CU_BillNum ?? ZString.Empty;
					if (!declaration.JE_MasterBill.EqualsIgnoringCase(newPrimaryMasterBill))
					{
						declaration.JE_MasterBill = newPrimaryMasterBill;
					}

					var newPrimaryHouseBill = declaration.PrimaryHouseBill?.CU_BillNum ?? ZString.Empty;
					if (!declaration.JE_HouseBill.EqualsIgnoringCase(newPrimaryHouseBill))
					{
						declaration.JE_HouseBill = newPrimaryHouseBill;
					}
				}
			}
		}

		Bill MakeThisBillTheGUIPresentationRecord(ZString billNum, ZString billType, Bill oldPrimaryBill, Func<Bill, bool> additionalBillDetailMatching)
		{
			var bill = GetBillMatchingNumberAndType(billNum, billType, additionalBillDetailMatching);
			return MakeThisBillTheGUIPresentationRecord(bill, oldPrimaryBill);
		}

		Bill GetBillMatchingNumberAndType(ZString billNum, ZString billType, Func<Bill, bool> additionalBillDetailMatching)
		{
			var shouldMatchAdditionalBillDetail = additionalBillDetailMatching != null;
			Bill firstBillMatch = null;
			foreach (var bill in declaration.Bills.Cast<Bill>().Where(b => b.CU_BillType.EqualsIgnoringCase(billType) && b.CU_BillNum.EqualsIgnoringCase(billNum)))
			{
				if (shouldMatchAdditionalBillDetail && additionalBillDetailMatching(bill))
				{
					return bill;
				}
				if (firstBillMatch == null)
				{
					firstBillMatch = bill;
				}
			}
			return firstBillMatch;
		}

		Bill MakeThisBillTheGUIPresentationRecord(Bill newBill, Bill oldPrimaryBill)
		{
			if (newBill == null)
			{
				newBill = oldPrimaryBill;
			}

			if (newBill != null && newBill != oldPrimaryBill && !newBill.CU_GUIPresentationRecord)
			{
				if (oldPrimaryBill != null)
				{
					oldPrimaryBill.CU_GUIPresentationRecord = false;
				}
				newBill.CU_GUIPresentationRecord = true;
			}

			return newBill;
		}

		void UpdateTheChildBillsAccordingToParentBillWithoutSuspendingSync(Bill parentBill)
		{
			if (parentBill != null && parentBill.IsMasterBill)
			{
				Bill newChildPrimaryBill = null;
				if (parentBill.CU_GUIPresentationRecord)
				{
					newChildPrimaryBill = parentBill.ChildBills.GUIPresentationBill;
					if (newChildPrimaryBill == null)
					{
						newChildPrimaryBill = parentBill.ChildBills.Count > 0 ? parentBill.ChildBills[0] : null;
					}
				}

				foreach (Bill bill in parentBill.ChildBills.ToArray())
				{
					if (bill == newChildPrimaryBill)
					{
						bill.CU_GUIPresentationRecord = true;
					}
					else
					{
						bill.CU_GUIPresentationRecord = false;
					}
				}
			}
		}

		byte suspenderIndex;

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1305:SpecifyIFormatProvider")]
		internal void CheckHouseBillAndPrimaryBill()
		{
			if (!IsSynchronising)
			{
				if (!declaration.JE_HouseBill.EqualsIgnoringCase(declaration.PrimaryHouseBill?.CU_BillNum ?? ZString.Empty))
				{
					ErrorReporter.ReportOnce("HouseBill and primary house bill numbers are different.", $"JE_DeclarationRef:{GetJobReferenceNo()}, JE_MessageType:{declaration.JE_MessageType}, JE_TransportMode:{declaration.JE_TransportMode}\r\nJE_MasterBill:{declaration.JE_MasterBill}, JE_HouseBill:{declaration.JE_HouseBill}\r\n{GetAllCusDecHouseBillReference()}");
				}
			}
		}

		ZString GetJobReferenceNo()
		{
			var result = declaration.JE_DeclarationReference;

			if (result.IsEmpty && declaration.Shipment is ForwardingShipment shipment)
			{
				result = shipment.JS_UniqueConsignRef;
			}

			return result;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1305:SpecifyIFormatProvider")]
		ZString GetAllCusDecHouseBillReference()
		{
			var stringBuilder = new ZStringBuilder();

			foreach (Bill bill in declaration.Bills)
			{
				stringBuilder.AppendLine($"CU_BillType:{bill.CU_BillType}, CU_BillNum:{bill.CU_BillNum}, CU_GUIPresentationRecord:{bill.CU_GUIPresentationRecord}, CU_AddInfo:{bill.CU_AddInfo}");
			}

			return stringBuilder.ToString();
		}

		public IDisposable SuspendSynch()
		{
			return new SynchroniserSuspender(this);
		}

		class SynchroniserSuspender : IDisposable
		{
			public SynchroniserSuspender(BillGUIPresentationFlagUpdater updater)
			{
				this.updater = updater;
				updater.suspenderIndex++;
			}
			readonly BillGUIPresentationFlagUpdater updater;

			void IDisposable.Dispose()
			{
				updater.suspenderIndex--;
			}
		}
	}
}
