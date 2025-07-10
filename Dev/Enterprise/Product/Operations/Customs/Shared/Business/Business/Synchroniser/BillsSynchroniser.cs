using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Forwarding.Business;

namespace Enterprise.Customs.Business
{
	public class BillsSynchroniser : BaseSynchroniser
	{
		public BillsSynchroniser(BaseJobDeclaration declaration, Func<IBillDetails, ZString> getHouseBillOfSpecificShipment)
		{
			this.declaration = declaration;
			this.getHouseBillOfSpecificShipment = getHouseBillOfSpecificShipment;
		}

		protected readonly BaseJobDeclaration declaration;
		protected ZString GetHouseBillOfSpecificShipment(IBillDetails billDetails)
		{
			return getHouseBillOfSpecificShipment(billDetails).ToUpper();
		}
		readonly Func<IBillDetails, ZString> getHouseBillOfSpecificShipment;

		protected virtual void SynchronisePrimaryBills()
		{
			if (!SyncChangesDetected)
			{
				var primaryMB = declaration.PrimaryMasterBill;
				var primaryHB = declaration.PrimaryHouseBill;
				if (DetectEnabled)
				{
					SyncChangesDetected = !declaration.JE_MasterBill.EqualsIgnoringCase(primaryMB != null ? primaryMB.CU_BillNum : ZString.Empty) ||
						!declaration.JE_HouseBill.EqualsIgnoringCase(primaryHB != null ? primaryHB.CU_BillNum : ZString.Empty);
				}
				else
				{
					declaration.JE_MasterBill = primaryMB != null ? primaryMB.CU_BillNum : ZString.Empty;
					declaration.JE_HouseBill = primaryHB != null ? primaryHB.CU_BillNum : ZString.Empty;
				}
			}
		}

		protected virtual void SynchroniseBill(BillDetailsWrapper sourceBill, Bill destination)
		{
			if (!SyncChangesDetected)
			{
				var sourceParentBillPK = sourceBill.GetParentBill?.Invoke()?.PK ?? ZGuid.Empty;
				var sourceBillIssueDate = sourceBill.Bill.BillUssueDate;

				if (DetectEnabled)
				{
					if (destination.CU_GUIPresentationRecord != sourceBill.IsPrimary ||
						destination.CU_CU_ParentBill != sourceParentBillPK ||
						destination.CU_IssueDate != sourceBillIssueDate)
					{
						SyncChangesDetected = true;
						return;
					}
				}
				else
				{
					destination.CU_GUIPresentationRecord = sourceBill.IsPrimary;
					destination.CU_CU_ParentBill = sourceParentBillPK;
					destination.CU_IssueDate = sourceBillIssueDate;
				}
			}
		}

		IEnumerable<BillDetailsWrapper> GetBillsToSynchronise()
		{
			foreach (BillDetailsWrapper wrapper in GetMasterBillsToSynchronise())
			{
				yield return wrapper;
			}

			var relevantConsol = declaration.RelevantConsol;
			if (relevantConsol == null || !relevantConsol.IsDirect || declaration.IsHouseBillMandatory)
			{
				foreach (BillDetailsWrapper wrapper in GetHouseBillsToSynchronise())
				{
					yield return wrapper;
				}
			}
		}

		protected virtual IEnumerable<BillDetailsWrapper> GetMasterBillsToSynchronise()
		{
			var consolMasterBillWrapper = CreateConsolMasterBillWrapper();
			if (consolMasterBillWrapper != null)
			{
				yield return consolMasterBillWrapper;
			}
		}

		protected BillDetailsWrapper CreateConsolMasterBillWrapper()
		{
			BillDetailsWrapper result = null;
			var relevantConsol = declaration.RelevantConsol;
			if (relevantConsol != null)
			{
				var billDetails = declaration.CreateConsolBillDetails(relevantConsol);
				var billNumber = GetHouseBillOfSpecificShipment(billDetails);
				if (!billNumber.IsEmpty)
				{
					result = new BillDetailsWrapper(billDetails, BillTypeList.Codes.MasterBill, true);
				}
			}
			return result;
		}

		protected virtual IEnumerable<BillDetailsWrapper> GetHouseBillsToSynchronise()
		{
			var shipmentsToSynch = declaration.GetShipmentsToSync().Where(s => !s.JS_HouseBill.IsEmpty);

			var primary = shipmentsToSynch.FirstOrDefault();
			if (primary != null)
			{
				yield return GetHouseBillToSynchronise(primary, true);
			}

			foreach (var secondary in shipmentsToSynch.Skip(1))
			{
				yield return GetHouseBillToSynchronise(secondary, false);
			}
		}

		protected BillDetailsWrapper GetHouseBillToSynchronise(ForwardingShipment shipmentToSynch, bool isPrimary)
		{
			return new BillDetailsWrapper(shipmentToSynch, BillTypeList.Codes.HouseBill, isPrimary) { GetParentBill = () => declaration.PrimaryMasterBill };
		}

		protected override void HookEvents()
		{
			((BusinessObjectCollection)declaration.Bills).OnAddedFromDataRefresh -= Bills_OnAddedFromDataRefresh;
			((BusinessObjectCollection)declaration.Bills).OnAddedFromDataRefresh += Bills_OnAddedFromDataRefresh;
		}

		protected override void UnHookEvents()
		{
			if (declaration != null)
			{
				((BusinessObjectCollection)declaration.Bills).OnAddedFromDataRefresh -= Bills_OnAddedFromDataRefresh;
			}

			foreach (var synch in SynchronisersForBillPacks)
			{
				synch.SetEnabled(false, false);
			}
			SynchronisersForBillPacks.Clear();
		}

		void Bills_OnAddedFromDataRefresh(object sender, EventArgs e)
		{
			Synchronise(IsEnabled);
		}

		protected override void OnDetectEnabledChanged()
		{
			if (DetectEnabled)
			{
				declaration.Bills.Load();
			}
		}

		protected override void ForceSynchronise()
		{
			if (!SyncChangesDetected)
			{
				using (declaration.GetBillGUIPresentationFlagSuspender())
				{
					var billsToDelete = declaration.Bills.OfType<Bill>().ToList();

					var mapToSource = new List<(Bill Bill, BillDetailsWrapper SourceBill, ZString AdjustedBillNumber)>();

					foreach (BillDetailsWrapper sourceBill in GetBillsToSynchronise())
					{
						var adjustedBillNumber = GetHouseBillOfSpecificShipment(sourceBill.Bill);
						var bill = sourceBill.IsPrimary
									? billsToDelete.Find(b => b.CU_GUIPresentationRecord && b.CU_BillType == sourceBill.BillType)
									: billsToDelete.Find(b => b.CU_BillNum.EqualsIgnoringCase(adjustedBillNumber) && b.CU_BillType == sourceBill.BillType);
						if (bill == null)
						{
							if (DetectEnabled)
							{
								SyncChangesDetected = true;
								return;
							}
							bill = declaration.Bills.AddNew();
							bill.CU_BillType = sourceBill.BillType;
							PrepareAndStartFieldSynchronisersForPacks(sourceBill, bill);
						}
						else
						{
							billsToDelete.Remove(bill);
						}

						mapToSource.Add((bill, sourceBill, adjustedBillNumber));
					}

					if (billsToDelete.Count > 0)
					{
						if (DetectEnabled)
						{
							SyncChangesDetected = true;
							return;
						}
						billsToDelete.ForEach(bill => bill.Delete());
					}

					foreach (var (bill, sourceBill, adjustedBillNumber) in mapToSource)
					{
						bill.CU_BillNum = adjustedBillNumber;

						SynchroniseBill(sourceBill, bill);
						if (SyncChangesDetected)
						{
							return;
						}
					}

					SynchronisePrimaryBills();
					if (SyncChangesDetected)
					{
						return;
					}
				}
			}
		}

		void PrepareAndStartFieldSynchronisersForPacks(BillDetailsWrapper sourceBill, Bill bill)
		{
			var packsNumberSynch = new FieldSynchroniser(bill.CU_NoOfPacksInfo, delegate
			{ return (ZDecimal)sourceBill.GetNumberOfPacks(declaration.Shipment); }, delegate
			{ return sourceBill.GetNumberOfPacksSourceInfos(declaration.Shipment); });
			SynchronisersForBillPacks.Add(packsNumberSynch);
			packsNumberSynch.Synchronise();

			var synchForPackType = new FieldSynchroniser(bill.CU_PackTypeInfo, delegate
			{ return ConvertPack(sourceBill.GetTypeOfPack(declaration.Shipment), declaration.RegistryCompanyPK); }, delegate
			{ return sourceBill.GetTypeOfPacksSourceInfos(declaration.Shipment); });
			SynchronisersForBillPacks.Add(synchForPackType);
			synchForPackType.Synchronise();
		}

		protected virtual ZString ConvertPack(ZString freightPack, ZGuid registryCompanyPK)
		{
			return freightPack;
		}

		List<FieldSynchroniser> synchronisersForBillPacks;
		List<FieldSynchroniser> SynchronisersForBillPacks
		{
			get { return synchronisersForBillPacks ?? (synchronisersForBillPacks = new List<FieldSynchroniser>()); }
		}
	}
}
